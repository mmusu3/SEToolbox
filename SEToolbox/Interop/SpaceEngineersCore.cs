namespace SEToolbox.Interop
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using Sandbox;
    using Sandbox.Engine.Networking;
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.Entities.Planet;
    using Sandbox.Game.GameSystems;
    using SEToolbox.Models;
    using SEToolbox.Support;
    using SpaceEngineers.Game;
    using VRage;
    using VRage.Collections;
    using VRage.FileSystem;
    using VRage.Game;
    using VRage.GameServices;
    using VRage.Plugins;
    using VRage.Steam;
    using VRage.Utils;
    using VRageRender;

    /// <summary>
    /// core interop for loading up Space Engineers content.
    /// </summary>
    public class SpaceEngineersCore
    {
        public static SpaceEngineersResources Resources
        {
            get => _singleton._worldResource?.Resources ?? _singleton._stockDefinitions;
        }

        public static Dictionary<string, byte> MaterialIndex
        {
            get => Resources.MaterialIndex;
        }

        public static WorldResource WorldResource
        {
            get => _singleton._worldResource;
            set => _singleton._worldResource = value;
        }

        public static List<string> ManageDeleteVoxelList
        {
            get => _singleton._manageDeleteVoxelList;
        }

        public static string GetDataPathOrDefault(string key, string defaultValue)
        {
            return Resources.GetDataPathOrDefault(key, defaultValue);
        }

        /// <summary>
        /// Forces static ctor to load stock defintiions.
        /// </summary>
        public static void LoadDefinitions()
        {
            Log.Debug("Init MyTexts.");

            typeof(MyTexts).TypeInitializer.Invoke(null, null); // For tests

            _singleton = new SpaceEngineersCore();
        }

        static SpaceEngineersCore _singleton;

        WorldResource _worldResource;
        readonly SpaceEngineersResources _stockDefinitions;
        readonly List<string> _manageDeleteVoxelList;
        MyCommonProgramStartup _startup;
        IMyGameService _steamService;

        const uint AppId = 244850; // Game
        //const uint AppId = 298740; // Dedicated Server

        public SpaceEngineersCore()
        {
            Log.Debug("Loading Space Engineers core.");

            var contentPath = ToolboxUpdater.GetApplicationContentPath();
            var userDataPath = SpaceEngineersConsts.BaseLocalPath.DataPath;

            MyFileSystem.ExePath = Path.GetDirectoryName(Assembly.GetAssembly(typeof(FastResourceLock)).Location);

            MyLog.Default = MySandboxGame.Log;

            Log.Debug("SetupBasicGameInfo");
            SpaceEngineersGame.SetupBasicGameInfo();

            Log.Debug("CommonProgramStartup");
            _startup = new MyCommonProgramStartup([]);

            //var appDataPath = _startup.GetAppDataPath();
            //MyInitializer.InvokeBeforeRun(AppId, MyPerGameSettings.BasicGameInfo.ApplicationName + "SEToolbox", appDataPath);
            //MyInitializer.InitCheckSum();

            Log.Debug("MyFileSystem init.");
            MyFileSystem.Reset();
            MyFileSystem.Init(contentPath, userDataPath);

            Log.Debug("Creating Steam service.");

            // This will start the Steam Service, and Steam will think SE is running.
            // TODO: we don't want to be doing this all the while SEToolbox is running,
            // perhaps a once off during load to fetch of mods then disconnect/Dispose.
            _steamService = MySteamGameService.Create(Sandbox.Engine.Platform.Game.IsDedicated, AppId);
            MyServiceManager.Instance.AddService(_steamService);

            Log.Debug("Init VRage platform.");

            MyVRage.Init(new ToolboxPlatform());
            MyVRage.Platform.Init();

            Log.Debug("Creating Ugc service.");

            IMyUGCService ugc = MySteamUgcService.Create(AppId, _steamService);
            //MyServiceManager.Instance.AddService(ugc);
            MyGameService.WorkshopService.AddAggregate(ugc);

            Log.Debug("MyFileSystem init user specific.");

            MyFileSystem.InitUserSpecific(_steamService.UserId.ToString()); // This sets the save file/path to load games from.
            //MyFileSystem.InitUserSpecific(null);
            //SpaceEngineersWorkshop.MySteam.Dispose();

            Log.Debug("Load config.");

            MySandboxGame.Config = new MyConfig("SpaceEngineers.cfg"); // TODO: Is specific to SE, not configurable to ME.
            MySandboxGame.Config.Load();

            Log.Debug("SetupPerGameSettings");

            SpaceEngineersGame.SetupPerGameSettings();
            MyPerGameSettings.UpdateOrchestratorType = null;

            Log.Debug("InitMultithreading");

            InitMultithreading();

            Log.Debug("MyRenderProxy init.");

            // Needed for MyRenderProxy.Log access in MyFont.LogWriteLine() and likely other things.
            // TODO: Static patching
            MyRenderProxy.Initialize(new MyNullRender());

            InitSandboxGame();

            Log.Debug("LoadLocalization");

            // Creating MySandboxGame will reset the CurrentUICulture, so I have to reapply it.
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfoByIetfLanguageTag(GlobalSettings.Default.LanguageCode);
            SpaceEngineersApi.LoadLocalization();

            Log.Debug("Creating session.");

            // Create an empty instance of MySession for use by low level code.
            var session = (Sandbox.Game.World.MySession)GetUninitializedObject(typeof(Sandbox.Game.World.MySession));

            // Required as the above code doesn't populate it during ctor of MySession.
            ReflectionUtil.ConstructField(session, "CreativeTools");
            ReflectionUtil.ConstructField(session, "m_sessionComponents");
            ReflectionUtil.ConstructField(session, "m_sessionComponentsForUpdate");

            session.Settings = new MyObjectBuilder_SessionSettings { EnableVoxelDestruction = true };

            // Change for the Clone() method to use XML cloning instead of Protobuf because of issues with MyObjectBuilder_CubeGrid.Clone()
            // TODO: ENABLE_PROTOBUFFERS_CLONING is a static readonly field. Setting these
            // via reflection is not guaranteed to work and is blocked in newer runtimes.
            ReflectionUtil.SetFieldValue(typeof(VRage.ObjectBuilders.Private.MyObjectBuilderSerializerKeen), "ENABLE_PROTOBUFFERS_CLONING", false);

            // Assign the instance back to the static.
            Sandbox.Game.World.MySession.Static = session;

            var heightmapSystem = new MyHeightMapLoadingSystem();
            session.RegisterComponent(heightmapSystem, heightmapSystem.UpdateOrder, heightmapSystem.Priority);
            heightmapSystem.LoadData();

            var planets = new MyPlanets();
            session.RegisterComponent(planets, heightmapSystem.UpdateOrder, heightmapSystem.Priority);
            planets.LoadData();

            Log.Debug("Load definitions.");

            _stockDefinitions = new SpaceEngineersResources();
            _stockDefinitions.LoadDefinitions();
            _manageDeleteVoxelList = [];
        }

        static void InitMultithreading()
        {
            //MySandboxGame.InitMultithreading();
            ParallelTasks.Parallel.Scheduler = new ParallelTasks.PrioritizedScheduler(Math.Max(Environment.ProcessorCount / 2, 1), amd: true, setup: null);
        }

        static void InitSandboxGame()
        {
            Log.Debug("InitSandboxGame");

            // If this is causing an exception then there is a missing dependency.
            // gameTemp instance gets captured in MySandboxGame.Static
            //MySandboxGame gameTemp = new DerivedGame(["-skipintro"]);

            // Required for definitions to work properly
            MySandboxGame.Static = (MySandboxGame)GetUninitializedObject(typeof(MySandboxGame));

            var game = MySandboxGame.Static;

            var iq1 = Activator.CreateInstance(typeof(MyConcurrentQueue<>).MakeGenericType(typeof(MySandboxGame).GetNestedType("MyInvokeData", BindingFlags.NonPublic)), [32]);
            var iq2 = Activator.CreateInstance(typeof(MyConcurrentQueue<>).MakeGenericType(typeof(MySandboxGame).GetNestedType("MyInvokeData", BindingFlags.NonPublic)), [32]);

            typeof(MySandboxGame).GetField("m_invokeQueue", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(game, iq1);
            typeof(MySandboxGame).GetField("m_invokeQueueExecuting", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(game, iq2);

            RegisterAssemblies();

            Log.Debug("MyGlobalTypeMetadata init.");

            VRage.Game.ObjectBuilder.MyGlobalTypeMetadata.Static.Init();

            Log.Debug("Preallocate");

            Preallocate();
        }

        static object GetUninitializedObject(Type type)
        {
#if NET
            return RuntimeHelpers.GetUninitializedObject(type);
#else
            return System.Runtime.Serialization.FormatterServices.GetUninitializedObject(type);
#endif
        }

        static void RegisterAssemblies()
        {
            Log.Debug("RegisterAssemblies");

            MyPlugins.RegisterGameAssemblyFile(MyPerGameSettings.GameModAssembly);

            if (MyPerGameSettings.GameModBaseObjBuildersAssembly != null)
                MyPlugins.RegisterBaseGameObjectBuildersAssemblyFile(MyPerGameSettings.GameModBaseObjBuildersAssembly);

            MyPlugins.RegisterGameObjectBuildersAssemblyFile(MyPerGameSettings.GameModObjBuildersAssembly);
            MyPlugins.RegisterSandboxAssemblyFile(MyPerGameSettings.SandboxAssembly);
            MyPlugins.RegisterSandboxGameAssemblyFile(MyPerGameSettings.SandboxGameAssembly);
            MyPlugins.Load();
        }

        static void Preallocate()
        {
            Type[] types = [
                typeof(Sandbox.Game.Entities.MyEntities),
                typeof(VRage.ObjectBuilders.MyObjectBuilder_Base),
                //typeof(MyTransparentGeometry),
                //typeof(MyCubeGridDeformationTables),
                typeof(VRageMath.MyMath),
                //typeof(MySimpleObjectDraw)
            ];

            //PreloadTypesFrom(MyPlugins.GameAssembly);
            //PreloadTypesFrom(MyPlugins.SandboxAssembly);
            ForceStaticCtor(types);
            //PreloadTypesFrom(typeof(MySandboxGame).Assembly);

            //static void PreloadTypesFrom(Assembly assembly)
            //{
            //    ForceStaticCtor(from type in assembly.GetTypes()
            //                    where Attribute.IsDefined(type, typeof(PreloadRequiredAttribute))
            //                    select type);
            //}

            static void ForceStaticCtor(IEnumerable<Type> types)
            {
                foreach (Type type in types)
                    RuntimeHelpers.RunClassConstructor(type.TypeHandle);
            }
        }

        //class DerivedGame(string[] commandlineArgs)
        //    : MySandboxGame(commandlineArgs, default)
        //{
        //    protected override void InitializeRender(IntPtr windowHandle) { }
        //}
    }
}

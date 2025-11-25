using System;
using System.Collections.Generic;
using System.Management;
using System.Security.Cryptography;
using System.Text;
using ParallelTasks;
using ProtoBuf.Meta;
using VRage;
using VRage.Analytics;
using VRage.Audio;
using VRage.Http;
using VRage.Input;
using VRage.Library.Threading;
using VRage.Scripting;
using VRage.Serialization;
using VRage.Utils;
using VRageRender;

namespace SEToolbox.Interop
{
    public class ToolboxPlatform : IVRagePlatform
    {
        public bool SessionReady { get; set; }

        public IVRageWindows Windows => throw new NotImplementedException();
        public IVRageHttp Http => throw new NotImplementedException();

        public IVRageSystem System { get; } = new VRageSystemImpl();
        public IVRageRender Render { get; } = new VRageRenderImpl();

        public IAnsel Ansel => throw new NotImplementedException();
        public IAfterMath AfterMath => throw new NotImplementedException();
        public IVRageInput Input => throw new NotImplementedException();
        public IVRageInput2 Input2 => throw new NotImplementedException();
        public IMyAudio Audio => throw new NotImplementedException();
        public IMyImeProcessor ImeProcessor => throw new NotImplementedException();
        public IMyCrashReporting CrashReporting => throw new NotImplementedException();
        public IVRageScripting Scripting => throw new NotImplementedException();

        IProtoTypeModel typeModel;

        public void Init()
        {
            typeModel = new DynamicTypeModel();
        }

        public bool CreateInput2() => throw new NotImplementedException();
        public IVideoPlayer CreateVideoPlayer() => throw new NotImplementedException();
        public void Done() => throw new NotImplementedException();

        public IProtoTypeModel GetTypeModel()
        {
            return typeModel;
        }

        public IMyAnalytics InitAnalytics(string projectId, string version) => throw new NotImplementedException();
        public IMyAnalytics InitAnalytics(string projectId, string version, bool idInited) => throw new NotImplementedException();
        public void InitScripting(IVRageScripting scripting) => throw new NotImplementedException();
        public void Update() => throw new NotImplementedException();
    }

    class VRageSystemImpl : IVRageSystem
    {
        public float CPUCounter => throw new NotImplementedException();
        public float RAMCounter => throw new NotImplementedException();
        public float GCMemory => throw new NotImplementedException();
        public long RemainingMemoryForGame => throw new NotImplementedException();
        public long ProcessPrivateMemory => throw new NotImplementedException();

        public bool IsUsingGeforceNow => false;
        public bool IsUsingGeforceNowCloud => false;

        public bool IsScriptCompilationSupported => throw new NotImplementedException();
        public string Clipboard { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool IsAllocationProfilingReady => throw new NotImplementedException();
        public bool IsSingleInstance => throw new NotImplementedException();
        public bool IsRemoteDebuggingSupported => throw new NotImplementedException();
        public SimulationQuality SimulationQuality => throw new NotImplementedException();
        public bool IsDeprecatedOS => throw new NotImplementedException();
        public bool IsMemoryLimited => throw new NotImplementedException();
        public bool HasSwappedMouseButtons => throw new NotImplementedException();
        public string ThreeLetterISORegionName => throw new NotImplementedException();
        public string TwoLetterISORegionName => throw new NotImplementedException();
        public string RegionLatitude => throw new NotImplementedException();
        public string RegionLongitude => throw new NotImplementedException();
        public string TempPath => throw new NotImplementedException();
        public int? OptimalHavokThreadCount => throw new NotImplementedException();

        public PrioritizedScheduler.ExplicitWorkerSetup? ExplicitWorkerSetup => null;

        public bool AreEnterBackButtonsSwapped => false;

        public float? ForcedUiRatio => throw new NotImplementedException();

        public event Action<string> OnSystemProtocolActivated;

        public event Action OnResuming;
        public event Action LeaveSession;
        public event Action OnSuspending;

        (string Name, uint MaxClock, uint Cores) m_cpuInfo;

        public string GetRootPath() => throw new NotImplementedException();
        public string GetAppDataPath() => throw new NotImplementedException();
        public ulong GetGlobalAllocationsStamp() => throw new NotImplementedException();

        public string GetInfoCPU(out uint frequency, out uint physicalCores)
        {
            if (m_cpuInfo.Name == null)
            {
                try
                {
                    using (var managementObjectSearcher = new ManagementObjectSearcher("select Name, MaxClockSpeed, NumberOfCores from Win32_Processor"))
                    {
                        foreach (ManagementObject item in managementObjectSearcher.Get())
                        {
                            m_cpuInfo.Name = item["Name"].ToString();
                            m_cpuInfo.Cores = (uint)item["NumberOfCores"];
                            m_cpuInfo.MaxClock = (uint)item["MaxClockSpeed"];
                        }
                    }
                }
                catch (Exception ex)
                {
                    //m_log.WriteLine("Couldn't get cpu info: " + ex);
                    m_cpuInfo.Name = "UnknownCPU";
                    m_cpuInfo.Cores = 0u;
                    m_cpuInfo.MaxClock = 0u;
                }
            }

            frequency = m_cpuInfo.MaxClock;
            physicalCores = m_cpuInfo.Cores;

            return m_cpuInfo.Name;
        }

        public string GetOsName() => throw new NotImplementedException();
        public List<string> GetProcessesLockingFile(string path) => throw new NotImplementedException();
        public ulong GetThreadAllocationStamp() => throw new NotImplementedException();
        public ulong GetTotalPhysicalMemory() => throw new NotImplementedException();
        public void LogEnvironmentInformation() => throw new NotImplementedException();
        public void LogToExternalDebugger(string message) => throw new NotImplementedException();
        public bool OpenUrl(string url) => throw new NotImplementedException();
        public void ResetColdStartRegister() => throw new NotImplementedException();

        public void WriteLineToConsole(string msg)
        {
            Console.WriteLine(msg);
        }

        public void GetGCMemory(out float allocated, out float used) => throw new NotImplementedException();
        public void OnThreadpoolInitialized() => throw new NotImplementedException();
        public void LogRuntimeInfo(Action<string> log) => throw new NotImplementedException();
        public void OnSessionStarted(SessionType sessionType) => throw new NotImplementedException();
        public void OnSessionUnloaded() => throw new NotImplementedException();
        public int? GetExperimentalPCULimit(int safePCULimit) => throw new NotImplementedException();
        public void DebuggerBreak() => throw new NotImplementedException();

        public void CollectGC(int generation, GCCollectionMode mode)
        {
            generation = Math.Min(generation, GC.MaxGeneration);
            GC.Collect(generation, mode);
        }

        public void CollectGC(int generation, GCCollectionMode mode, bool blocking, bool compacting)
        {
            generation = Math.Min(generation, GC.MaxGeneration);
            GC.Collect(generation, mode, blocking, compacting);
        }

        public bool OpenUrl(string url, bool predetermined = true) => throw new NotImplementedException();

        public ISharedCriticalSection CreateSharedCriticalSection(bool spinLock)
        {
            if (spinLock)
                return new MyCriticalSection_SpinLock();
            else
                return new MyCriticalSection_Mutex();
        }

        public DateTime GetNetworkTimeUTC() => throw new NotImplementedException();

        public string GetPlatformSpecificCrashReport() => null;

        public string GetModsCachePath() => null;
    }

    class VRageRenderImpl : IVRageRender
    {
        public bool UseParallelRenderInit => throw new NotImplementedException();
        public bool IsRenderOutputDebugSupported => throw new NotImplementedException();
        public bool ForceClearGBuffer => throw new NotImplementedException();

        public event Action OnResuming;
        public event Action OnSuspending;

        public void ApplyRenderSettings(MyRenderDeviceSettings? settings) => throw new NotImplementedException();
        public object CreateRenderAnnotation(object deviceContext) => throw new NotImplementedException();
        public void CreateRenderDevice(ref MyRenderDeviceSettings? settings, out object deviceInstance, out object swapChain) => throw new NotImplementedException();
        public void DisposeRenderDevice() => throw new NotImplementedException();
        public void FlushIndirectArgsFromComputeShader(object deviceContext) => throw new NotImplementedException();

        public ulong GetMemoryBudgetForStreamedResources() => 0;
        public ulong GetMemoryBudgetForGeneratedTextures() => 0;
        public ulong GetMemoryBudgetForVoxelTextureArrays() => 0;

        public MyAdapterInfo[] GetRenderAdapterList() => throw new NotImplementedException();
        public MyRenderPresetEnum GetRenderQualityHint() => throw new NotImplementedException();
        public void ResumeRenderContext() => throw new NotImplementedException();
        public void SetMemoryUsedForImprovedGFX(long bytes) => throw new NotImplementedException();
        public void SuspendRenderContext() => throw new NotImplementedException();
        public void RequestSuspendWait() => throw new NotImplementedException();
        public void CustomUpdateForDeferredBuffer(object deviceContext, object buffer) => throw new NotImplementedException();
        public void SubmitEmptyCustomContext(object deviceContext) => throw new NotImplementedException();
        public void FastVSSetConstantBuffer(object deviceContext, int slot, object buffer) => throw new NotImplementedException();
        public void FastGSSetConstantBuffer(object deviceContext, int slot, object buffer) => throw new NotImplementedException();
        public void FastPSSetConstantBuffer(object deviceContext, int slot, object buffer) => throw new NotImplementedException();
        public void FastCSSetConstantBuffer(object deviceContext, int slot, object buffer) => throw new NotImplementedException();
        public void FastVSSetConstantBuffers1(object deviceContext, int slot, object buffer, int offset, int size, ref object constantBindingsCache) => throw new NotImplementedException();
        public void FastPSSetConstantBuffers1(object deviceContext, int slot, object buffer, int offset, int size, ref object constantBindingsCache) => throw new NotImplementedException();
        public void SetDepthTextureHint(VRageRender_DepthTextureHintType hint, object deviceContext = null, object texture = null) => throw new NotImplementedException();
        public bool IsExclusiveTextureLoadRequired() => throw new NotImplementedException();
    }

    // Internal class copied from VRage.Platform.Windows
    class DynamicTypeModel : IProtoTypeModel
    {
        public TypeModel Model => m_typeModel;

        private RuntimeTypeModel m_typeModel;

        public DynamicTypeModel()
        {
            CreateTypeModel();
        }

        private void CreateTypeModel()
        {
            m_typeModel = RuntimeTypeModel.Create(true);
            m_typeModel.AutoAddMissingTypes = true;
            m_typeModel.UseImplicitZeroDefaults = false;
        }

        private static ushort Get16BitHash(string s)
        {
            using (MD5 mD = MD5.Create())
                return BitConverter.ToUInt16(mD.ComputeHash(Encoding.UTF8.GetBytes(s)), 0);
        }

        public void RegisterTypes(IEnumerable<Type> types)
        {
            var registered = new HashSet<Type>();

            foreach (Type type in types)
                RegisterType(type);

            void RegisterType(Type protoType)
            {
                if (protoType.IsGenericType)
                    return;

                if (protoType.BaseType == typeof(object) || protoType.IsValueType)
                {
                    m_typeModel.Add(protoType, true);
                }
                else
                {
                    RegisterType(protoType.BaseType);

                    if (registered.Add(protoType))
                    {
                        int fieldNumber = Get16BitHash(protoType.Name) + 65535;
                        m_typeModel.Add(protoType, true);
                        m_typeModel[protoType.BaseType].AddSubType(fieldNumber, protoType);
                    }
                }
            }
        }

        public void FlushCaches()
        {
            CreateTypeModel();
        }
    }
}

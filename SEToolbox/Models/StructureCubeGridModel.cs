namespace SEToolbox.Models
{
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Definitions;
    using SEToolbox.Interop;
    using SEToolbox.Support;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Windows.Media.Media3D;
    using System.Windows.Threading;
    using VRage;
    using VRage.Game;
    using VRage.Game.ObjectBuilders.Components;
    using VRage.ObjectBuilders;
    using VRage.Utils;
    using VRageMath;
    using IDType = VRage.MyEntityIdentifier.ID_OBJECT_TYPE;
    using Res = SEToolbox.Properties.Resources;

    [Serializable]
    public class StructureCubeGridModel : StructureBaseModel
    {
        #region Fields

        // Fields are marked as NonSerialized, as they aren't required during the drag-drop operation.

        [NonSerialized]
        private Point3D _min;

        [NonSerialized]
        private Point3D _max;

        [NonSerialized]
        private System.Windows.Media.Media3D.Vector3D _scale;

        [NonSerialized]
        private Size3D _size;

        [NonSerialized]
        private int _pilots;

        [NonSerialized]
        private TimeSpan _timeToProduce;

        [NonSerialized]
        private int _pcuToProduce;

        [NonSerialized]
        private string _cockpitOrientation;

        [NonSerialized]
        private List<CubeAssetModel> _cubeAssets;

        [NonSerialized]
        private List<CubeAssetModel> _componentAssets;

        [NonSerialized]
        private List<OreAssetModel> _ingotAssets;

        [NonSerialized]
        private List<OreAssetModel> _oreAssets;

        [NonSerialized]
        private string _activeComponentFilter;

        [NonSerialized]
        private string _componentFilter;

        [NonSerialized]
        private ObservableCollection<CubeItemModel> _cubeList;

        [NonSerialized]
        private static readonly object Locker = new object();

        [NonSerialized]
        private bool _isSubsSystemNotReady;

        [NonSerialized]
        private bool _isConstructionNotReady;

        #endregion

        #region ctor

        public StructureCubeGridModel(MyObjectBuilder_EntityBase entityBase)
            : base(entityBase)
        {
            IsSubsSystemNotReady = true;
            IsConstructionNotReady = true;
        }

        #endregion

        #region Properties

        public MyObjectBuilder_CubeGrid CubeGrid
        {
            get { return EntityBase as MyObjectBuilder_CubeGrid; }
        }

        public MyCubeSize GridSize
        {
            get { return CubeGrid.GridSizeEnum; }

            set
            {
                if (value != CubeGrid.GridSizeEnum)
                {
                    CubeGrid.GridSizeEnum = value;
                    OnPropertyChanged(nameof(GridSize));
                }
            }
        }

        public bool IsStatic
        {
            get { return CubeGrid.IsStatic; }

            set
            {
                if (value != CubeGrid.IsStatic)
                {
                    CubeGrid.IsStatic = value;
                    OnPropertyChanged(nameof(IsStatic));
                }
            }
        }

        public bool Dampeners
        {
            get { return CubeGrid.DampenersEnabled; }

            set
            {
                if (value != CubeGrid.DampenersEnabled)
                {
                    CubeGrid.DampenersEnabled = value;
                    OnPropertyChanged(nameof(Dampeners));
                }
            }
        }

        public bool Destructible
        {
            get { return CubeGrid.DestructibleBlocks; }

            set
            {
                if (value != CubeGrid.DestructibleBlocks)
                {
                    CubeGrid.DestructibleBlocks = value;
                    OnPropertyChanged(nameof(Destructible));
                }
            }
        }


        public override string DisplayName
        {
            get { return base.DisplayName; }

            set
            {
                base.DisplayName = value;
                CubeGrid.DisplayName = value;
            }
        }

        public Point3D Min
        {
            get { return _min; }

            set
            {
                if (value != _min)
                {
                    _min = value;
                    OnPropertyChanged(nameof(Min));
                }
            }
        }

        public Point3D Max
        {
            get { return _max; }

            set
            {
                if (value != _max)
                {
                    _max = value;
                    OnPropertyChanged(nameof(Max));
                }
            }
        }

        public System.Windows.Media.Media3D.Vector3D Scale
        {
            get { return _scale; }

            set
            {
                if (value != _scale)
                {
                    _scale = value;
                    OnPropertyChanged(nameof(Scale));
                }
            }
        }

        public Size3D Size
        {
            get { return _size; }

            set
            {
                if (value != _size)
                {
                    _size = value;
                    OnPropertyChanged(nameof(Size));
                }
            }
        }

        public int Pilots
        {
            get { return _pilots; }

            set
            {
                if (value != _pilots)
                {
                    _pilots = value;
                    OnPropertyChanged(nameof(Pilots));
                }
            }
        }

        public bool IsPiloted
        {
            get { return Pilots > 0; }
        }

        public bool IsDamaged
        {
            get
            {
                // TODO: check the CubeBlocks/ cube.IntegrityPercent
                return true; //CubeGrid.Skeleton.Count > 0;
            }
        }

        public int DamageCount
        {
            get
            {
                // TODO: create a seperate property for the CubeBlocks/ cube.IntegrityPercent
                return CubeGrid.Skeleton?.Count ?? 0;
            }
        }

        public override double LinearVelocity
        {
            get { return CubeGrid.LinearVelocity.ToVector3().LinearVector(); }
        }

        /// This is not to be taken as an accurate representation.
        public double AngularVelocity
        {
            get { return CubeGrid.AngularVelocity.ToVector3().LinearVector(); }
        }

        public TimeSpan TimeToProduce
        {
            get { return _timeToProduce; }

            set
            {
                if (value != _timeToProduce)
                {
                    _timeToProduce = value;
                    OnPropertyChanged(nameof(TimeToProduce));
                }
            }
        }

        public int PCUToProduce
        {
            get { return _pcuToProduce; }

            set
            {
                if (value != _pcuToProduce)
                {
                    _pcuToProduce = value;
                    OnPropertyChanged(nameof(PCUToProduce));
                }
            }
        }

        public override int BlockCount
        {
            get { return CubeGrid.CubeBlocks.Count; }
        }

        public string CockpitOrientation
        {
            get { return _cockpitOrientation; }

            set
            {
                if (value != _cockpitOrientation)
                {
                    _cockpitOrientation = value;
                    OnPropertyChanged(nameof(CockpitOrientation));
                }
            }
        }

        /// <summary>
        /// This is detail of the breakdown of cubes in the ship.
        /// </summary>
        public List<CubeAssetModel> CubeAssets
        {
            get { return _cubeAssets; }

            set
            {
                if (value != _cubeAssets)
                {
                    _cubeAssets = value;
                    OnPropertyChanged(nameof(CubeAssets));
                }
            }
        }

        /// <summary>
        /// This is detail of the breakdown of components in the ship.
        /// </summary>
        public List<CubeAssetModel> ComponentAssets
        {
            get { return _componentAssets; }

            set
            {
                if (value != _componentAssets)
                {
                    _componentAssets = value;
                    OnPropertyChanged(nameof(ComponentAssets));
                }
            }
        }

        /// <summary>
        /// This is detail of the breakdown of ingots in the ship.
        /// </summary>
        public List<OreAssetModel> IngotAssets
        {
            get { return _ingotAssets; }

            set
            {
                if (value != _ingotAssets)
                {
                    _ingotAssets = value;
                    OnPropertyChanged(nameof(IngotAssets));
                }
            }
        }

        /// <summary>
        /// This is detail of the breakdown of ore in the ship.
        /// </summary>
        public List<OreAssetModel> OreAssets
        {
            get { return _oreAssets; }

            set
            {
                if (value != _oreAssets)
                {
                    _oreAssets = value;
                    OnPropertyChanged(nameof(OreAssets));
                }
            }
        }

        public string ActiveComponentFilter
        {
            get { return _activeComponentFilter; }

            set
            {
                if (value != _activeComponentFilter)
                {
                    _activeComponentFilter = value;
                    OnPropertyChanged(nameof(ActiveComponentFilter));
                }
            }
        }

        public string ComponentFilter
        {
            get { return _componentFilter; }

            set
            {
                if (value != _componentFilter)
                {
                    _componentFilter = value;
                    OnPropertyChanged(nameof(ComponentFilter));
                }
            }
        }

        public ObservableCollection<CubeItemModel> CubeList
        {
            get { return _cubeList; }

            set
            {
                if (value != _cubeList)
                {
                    _cubeList = value;
                    OnPropertyChanged(nameof(CubeList));
                }
            }
        }

        public bool IsSubsSystemNotReady
        {
            get { return _isSubsSystemNotReady; }

            set
            {
                if (value != _isSubsSystemNotReady)
                {
                    _isSubsSystemNotReady = value;
                    OnPropertyChanged(nameof(IsSubsSystemNotReady));
                }
            }
        }

        public bool IsConstructionNotReady
        {
            get { return _isConstructionNotReady; }

            set
            {
                if (value != _isConstructionNotReady)
                {
                    _isConstructionNotReady = value;
                    OnPropertyChanged(nameof(IsConstructionNotReady));
                }
            }
        }

        #endregion

        #region methods

        [OnSerializing]
        internal void OnSerializingMethod(StreamingContext context)
        {
            SerializedEntity = SpaceEngineersApi.Serialize<MyObjectBuilder_CubeGrid>(CubeGrid);
        }

        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            EntityBase = SpaceEngineersApi.Deserialize<MyObjectBuilder_CubeGrid>(SerializedEntity);
        }

        public override void UpdateGeneralFromEntityBase()
        {
            double scaleMultiplyer = CubeGrid.GridSizeEnum.ToLength();
            if (IsStatic && CubeGrid.GridSizeEnum == MyCubeSize.Large)
            {
                ClassType = ClassType.LargeStation;
            }
            else if (IsStatic && CubeGrid.GridSizeEnum == MyCubeSize.Small)
            {
                ClassType = ClassType.SmallStation;
            }
            else if (!IsStatic && CubeGrid.GridSizeEnum == MyCubeSize.Large)
            {
                ClassType = ClassType.LargeShip;
            }
            else if (!IsStatic && CubeGrid.GridSizeEnum == MyCubeSize.Small)
            {
                ClassType = ClassType.SmallShip;
            }

            var min = new Point3D(int.MaxValue, int.MaxValue, int.MaxValue);
            var max = new Point3D(int.MinValue, int.MinValue, int.MinValue);
            float totalMass = 0;

            foreach (var block in CubeGrid.CubeBlocks)
            {
                min.X = Math.Min(min.X, block.Min.X);
                min.Y = Math.Min(min.Y, block.Min.Y);
                min.Z = Math.Min(min.Z, block.Min.Z);

                var cubeDefinition = SpaceEngineersApi.GetCubeDefinition(block.TypeId, CubeGrid.GridSizeEnum, block.SubtypeName);

                // definition is null when the block no longer exists in the Cube definitions. Ie, Ladder, or a Mod that was removed.
                if (cubeDefinition == null || (cubeDefinition.Size.X == 1 && cubeDefinition.Size.Y == 1 && cubeDefinition.Size.Z == 1))
                {
                    max.X = Math.Max(max.X, block.Min.X);
                    max.Y = Math.Max(max.Y, block.Min.Y);
                    max.Z = Math.Max(max.Z, block.Min.Z);
                }
                else
                {
                    // resolve the cube size acording to the cube's orientation.
                    var orientSize = cubeDefinition.Size.Add(-1).Transform(block.BlockOrientation).Abs();
                    max.X = Math.Max(max.X, block.Min.X + orientSize.X);
                    max.Y = Math.Max(max.Y, block.Min.Y + orientSize.Y);
                    max.Z = Math.Max(max.Z, block.Min.Z + orientSize.Z);
                }

                var cubeBlockDefinition = SpaceEngineersApi.GetCubeDefinition(block.TypeId, CubeGrid.GridSizeEnum, block.SubtypeName);

                float cubeMass = 0;
                if (cubeBlockDefinition != null)
                {
                    foreach (var component in cubeBlockDefinition.Components)
                    {
                        float componentMass = component.Definition.Mass * component.Count;
                        cubeMass += componentMass;
                    }
                }

                totalMass += cubeMass;
            }

            var cockpitOrientation = Res.ClsCockpitOrientationNone;
            var cockpits = CubeGrid.CubeBlocks.Where(b => b is MyObjectBuilder_Cockpit).ToArray();
            if (cockpits.Length > 0)
            {
                var count = cockpits.Count(b => b.BlockOrientation.Forward == cockpits[0].BlockOrientation.Forward && b.BlockOrientation.Up == cockpits[0].BlockOrientation.Up);
                if (cockpits.Length == count)
                {
                    // All cockpits share the same orientation.
                    cockpitOrientation = $"{Res.ClsCockpitOrientationForward}={cockpits[0].BlockOrientation.Forward} ({GetAxisIndicator(cockpits[0].BlockOrientation.Forward)}), Up={cockpits[0].BlockOrientation.Up} ({GetAxisIndicator(cockpits[0].BlockOrientation.Up)})";
                }
                else
                {
                    // multiple cockpits are present, and do not share a common orientation.
                    cockpitOrientation = Res.ClsCockpitOrientationMixed;
                }
            }
            CockpitOrientation = cockpitOrientation;

            var scale = max - min;
            scale.X++;
            scale.Y++;
            scale.Z++;

            if (CubeGrid.CubeBlocks.Count == 0)
                scale = new System.Windows.Media.Media3D.Vector3D();

            Min = min;
            Max = max;
            Scale = scale;
            Size = new Size3D(scale.X * scaleMultiplyer, scale.Y * scaleMultiplyer, scale.Z * scaleMultiplyer);
            Mass = totalMass;

            var quaternion = CubeGrid.PositionAndOrientation.Value.ToQuaternionD();
            var p1 = (min.ToVector3D() * CubeGrid.GridSizeEnum.ToLength()).Transform(quaternion) + CubeGrid.PositionAndOrientation.Value.Position - (CubeGrid.GridSizeEnum.ToLength() / 2);
            var p2 = ((min.ToVector3D() + Scale.ToVector3D()) * CubeGrid.GridSizeEnum.ToLength()).Transform(quaternion) + CubeGrid.PositionAndOrientation.Value.Position - (CubeGrid.GridSizeEnum.ToLength() / 2);
            //var p1 = VRageMath.Vector3D.Transform(min.ToVector3D() * CubeGrid.GridSizeEnum.ToLength(), quaternion) + CubeGrid.PositionAndOrientation.Value.Position - (CubeGrid.GridSizeEnum.ToLength() / 2);
            //var p2 = VRageMath.Vector3D.Transform((min.ToVector3D() + Scale.ToVector3D()) * CubeGrid.GridSizeEnum.ToLength(), quaternion) + CubeGrid.PositionAndOrientation.Value.Position - (CubeGrid.GridSizeEnum.ToLength() / 2);
            WorldAABB = new BoundingBoxD(VRageMath.Vector3D.Min(p1, p2), VRageMath.Vector3D.Max(p1, p2));
            Center = WorldAABB.Center;

            DisplayName = CubeGrid.DisplayName;

            // Add Beacon or Antenna detail for the Description.
            var broadcasters = CubeGrid.CubeBlocks.Where(b => b.SubtypeName == SubtypeId.LargeBlockBeacon.ToString()
                || b.SubtypeName == SubtypeId.SmallBlockBeacon.ToString()
                || b.SubtypeName == SubtypeId.LargeBlockRadioAntenna.ToString()
                || b.SubtypeName == SubtypeId.SmallBlockRadioAntenna.ToString()).ToArray();
            var broadcastNames = string.Empty;
            if (broadcasters.Length > 0)
            {
                var beaconNames = broadcasters.Where(b => b is MyObjectBuilder_Beacon).Select(b => ((MyObjectBuilder_Beacon)b).CustomName ?? "Beacon").ToArray();
                var antennaNames = broadcasters.Where(b => b is MyObjectBuilder_RadioAntenna).Select(b => ((MyObjectBuilder_RadioAntenna)b).CustomName ?? "Antenna").ToArray();
                broadcastNames = string.Join("|", beaconNames.Concat(antennaNames).OrderBy(s => s));
            }

            if (CubeGrid.CubeBlocks.Count == 1)
            {
                if (CubeGrid.CubeBlocks[0] is MyObjectBuilder_Wheel)
                {
                    MyObjectBuilder_CubeGrid grid = ExplorerModel.Default.FindConnectedTopBlock<MyObjectBuilder_MotorSuspension>(CubeGrid.CubeBlocks[0].EntityId);

                    if (grid == null)
                        Description = Res.ClsCubeGridWheelDetached;
                    else
                        Description = string.Format(Res.ClsCubeGridWheelAttached, grid.DisplayName);
                    return;
                }
            }

            if (string.IsNullOrEmpty(broadcastNames))
                Description = string.Format("{0}×{1}×{2}", Scale.X, Scale.Y, Scale.Z);
            else
                Description = string.Format("{3} {0}×{1}×{2}", Scale.X, Scale.Y, Scale.Z, broadcastNames);


            // TODO:
            // Report:
            // Reflectors On
            // Mass:      9,999,999 Kg
            // Speed:          0.0 m/s
            // Power Usage:      0.05%
            // Reactors:     12,999 GW
            // Thrusts:            999
            // Gyros:              999
            // Fuel Time:        0 sec
        }

        public override void InitializeAsync()
        {
            var worker = new BackgroundWorker();

            worker.DoWork += delegate (object s, DoWorkEventArgs workArgs)
            {
                lock (Locker)
                {
                    // Because I've bound models to the view, this is going to get messy.
                    var contentPath = ToolboxUpdater.GetApplicationContentPath();

                    if (IsConstructionNotReady)
                    {
                        var ingotRequirements = new Dictionary<string, BlueprintRequirement>();
                        var oreRequirements = new Dictionary<string, BlueprintRequirement>();
                        var timeTaken = new TimeSpan();
                        var cubeAssetDict = new Dictionary<string, CubeAssetModel>();
                        var componentAssetDict = new Dictionary<string, CubeAssetModel>();
                        var cubeAssets = new List<CubeAssetModel>();
                        var componentAssets = new List<CubeAssetModel>();
                        var ingotAssets = new List<OreAssetModel>();
                        var oreAssets = new List<OreAssetModel>();
                        int pcuUsed = 0;

                        foreach (var block in CubeGrid.CubeBlocks)
                        {
                            var blockName = block.SubtypeName;
                            if (string.IsNullOrEmpty(blockName))
                            {
                                blockName = block.TypeId.ToString();
                            }

                            var cubeBlockDefinition = SpaceEngineersApi.GetCubeDefinition(block.TypeId, CubeGrid.GridSizeEnum, block.SubtypeName);

                            float cubeMass = 0;
                            TimeSpan blockTime = TimeSpan.Zero;
                            string blockTexture = null;
                            int pcu = 0;

                            if (cubeBlockDefinition != null)
                            {
                                foreach (var component in cubeBlockDefinition.Components)
                                {
                                    TimeSpan componentTime;
                                    SpaceEngineersApi.AccumulateCubeBlueprintRequirements(component.Definition.Id.SubtypeName, component.Definition.Id.TypeId, component.Count, ingotRequirements, out componentTime);
                                    timeTaken += componentTime;

                                    float componentMass = component.Definition.Mass * component.Count;
                                    float componentVolume = component.Definition.Volume * SpaceEngineersConsts.VolumeMultiplyer * component.Count;
                                    cubeMass += componentMass;

                                    var componentName = component.Definition.Id.SubtypeName;
                                    if (componentAssetDict.ContainsKey(componentName))
                                    {
                                        componentAssetDict[componentName].Count += component.Count;
                                        componentAssetDict[componentName].Mass += componentMass;
                                        componentAssetDict[componentName].Volume += componentVolume;
                                        componentAssetDict[componentName].Time += componentTime;
                                    }
                                    else
                                    {
                                        var componentTexture = SpaceEngineersCore.GetDataPathOrDefault(component.Definition.Icons.First(), Path.Combine(contentPath, component.Definition.Icons.First()));
                                        var m = new CubeAssetModel() { Name = component.Definition.DisplayNameText, Mass = componentMass, Volume = componentVolume, Count = component.Count, Time = componentTime, TextureFile = componentTexture };
                                        componentAssets.Add(m);
                                        componentAssetDict.Add(componentName, m);
                                    }
                                }

                                blockTime = TimeSpan.FromSeconds(cubeBlockDefinition.IntegrityPointsPerSec != 0 ? cubeBlockDefinition.MaxIntegrity / cubeBlockDefinition.IntegrityPointsPerSec : 0);
                                blockTexture = (cubeBlockDefinition.Icons == null || cubeBlockDefinition.Icons.First() == null) ? null : SpaceEngineersCore.GetDataPathOrDefault(cubeBlockDefinition.Icons.First(), Path.Combine(contentPath, cubeBlockDefinition.Icons.First()));
                                pcu = cubeBlockDefinition.PCU;
                                pcuUsed += cubeBlockDefinition.PCU;
                            }

                            timeTaken += blockTime;

                            if (cubeAssetDict.ContainsKey(blockName))
                            {
                                cubeAssetDict[blockName].Count++;
                                cubeAssetDict[blockName].Mass += cubeMass;
                                cubeAssetDict[blockName].Time += blockTime;
                                cubeAssetDict[blockName].PCU += pcu;
                            }
                            else
                            {
                                var m = new CubeAssetModel() { Name = cubeBlockDefinition == null ? blockName : cubeBlockDefinition.DisplayNameText, Mass = cubeMass, Count = 1, TextureFile = blockTexture, Time = blockTime, PCU = pcu };
                                cubeAssets.Add(m);
                                cubeAssetDict.Add(blockName, m);
                            }
                        }

                        foreach (var kvp in ingotRequirements)
                        {
                            TimeSpan ingotTime;
                            SpaceEngineersApi.AccumulateCubeBlueprintRequirements(kvp.Value.SubtypeId, kvp.Value.Id.TypeId, kvp.Value.Amount, oreRequirements, out ingotTime);
                            MyDefinitionBase mydb = MyDefinitionManager.Static.GetDefinition(kvp.Value.Id);
                            if (mydb.Id.TypeId.IsNull)
                                continue;
                            var cd = (MyPhysicalItemDefinition)mydb;
                            var componentTexture = SpaceEngineersCore.GetDataPathOrDefault(cd.Icons.First(), Path.Combine(contentPath, cd.Icons.First()));
                            var volume = (double)kvp.Value.Amount * cd.Volume * SpaceEngineersConsts.VolumeMultiplyer;
                            var ingotAsset = new OreAssetModel { Name = cd.DisplayNameText, Amount = kvp.Value.Amount, Mass = (double)kvp.Value.Amount * cd.Mass, Volume = volume, Time = ingotTime, TextureFile = componentTexture };
                            ingotAssets.Add(ingotAsset);
                            timeTaken += ingotTime;
                        }

                        foreach (var kvp in oreRequirements)
                        {
                            var cd = MyDefinitionManager.Static.GetDefinition(kvp.Value.Id) as MyPhysicalItemDefinition;
                            if (cd != null)
                            {
                                var componentTexture = SpaceEngineersCore.GetDataPathOrDefault(cd.Icons.First(), Path.Combine(contentPath, cd.Icons.First()));
                                var volume = (double)kvp.Value.Amount * cd.Volume * SpaceEngineersConsts.VolumeMultiplyer;
                                var oreAsset = new OreAssetModel() { Name = cd.DisplayNameText, Amount = kvp.Value.Amount, Mass = (double)kvp.Value.Amount * cd.Mass, Volume = volume, TextureFile = componentTexture };
                                oreAssets.Add(oreAsset);
                            }
                        }

                        _dispatcher.Invoke(DispatcherPriority.Input, (Action)delegate
                        {
                            CubeAssets = cubeAssets;
                            ComponentAssets = componentAssets;
                            IngotAssets = ingotAssets;
                            OreAssets = oreAssets;
                            TimeToProduce = timeTaken;
                            PCUToProduce = pcuUsed;
                        });

                        IsConstructionNotReady = false;
                    }

                    if (IsSubsSystemNotReady)
                    {
                        var cubeList = new List<CubeItemModel>();

                        foreach (var block in CubeGrid.CubeBlocks)
                        {
                            var cubeDefinition = SpaceEngineersApi.GetCubeDefinition(block.TypeId, CubeGrid.GridSizeEnum, block.SubtypeName);

                            _dispatcher.Invoke(DispatcherPriority.Input, (Action)delegate
                            {
                                cubeList.Add(new CubeItemModel(block, cubeDefinition)
                                {
                                    TextureFile = (cubeDefinition == null || cubeDefinition.Icons == null || cubeDefinition.Icons.First() == null) ? null : SpaceEngineersCore.GetDataPathOrDefault(cubeDefinition.Icons.First(), Path.Combine(contentPath, cubeDefinition.Icons.First()))
                                });
                            });
                        }

                        _dispatcher.Invoke(DispatcherPriority.Input, (Action)delegate
                        {
                            CubeList = new ObservableCollection<CubeItemModel>(cubeList);
                        });

                        IsSubsSystemNotReady = false;
                    }
                }
            };

            worker.RunWorkerAsync();
        }

        /// <summary>
        /// Find any Cockpits that have player character/s in them.
        /// </summary>
        /// <returns></returns>
        public List<MyObjectBuilder_Cockpit> GetActiveCockpits()
        {
            List<MyObjectBuilder_Cockpit> list = new List<MyObjectBuilder_Cockpit>();

            foreach (MyObjectBuilder_CubeBlock cube in CubeGrid.CubeBlocks.Where<MyObjectBuilder_CubeBlock>(e => e is MyObjectBuilder_Cockpit))
            {
                var hierarchyBase = cube.ComponentContainer?.Components?.FirstOrDefault(e => e.TypeId == "MyHierarchyComponentBase")?.Component as MyObjectBuilder_HierarchyComponentBase;
                if (hierarchyBase != null)
                {
                    if (hierarchyBase.Children.Any(e => e is MyObjectBuilder_Character))
                        list.Add((MyObjectBuilder_Cockpit)cube);
                }
            }

            return list;
        }

        public void RepairAllDamage()
        {
            if (CubeGrid.Skeleton == null)
                CubeGrid.Skeleton = new List<BoneInfo>();
            else
                CubeGrid.Skeleton.Clear();

            foreach (var cube in CubeGrid.CubeBlocks)
            {
                cube.IntegrityPercent = cube.BuildPercent;
                // No need to set bones for individual blocks like rounded armor, as this is taken from the definition within the game itself.
            }

            OnPropertyChanged(nameof(IsDamaged));
            OnPropertyChanged(nameof(DamageCount));
        }

        public void ResetLinearVelocity()
        {
            CubeGrid.LinearVelocity = new Vector3(0, 0, 0);
            OnPropertyChanged(nameof(LinearVelocity));
        }

        public void ResetRotationVelocity()
        {
            CubeGrid.AngularVelocity = new Vector3(0, 0, 0);
            OnPropertyChanged(nameof(AngularVelocity));
        }

        public void ResetVelocity()
        {
            CubeGrid.LinearVelocity = new Vector3(0, 0, 0);
            CubeGrid.AngularVelocity = new Vector3(0, 0, 0);
            OnPropertyChanged(nameof(LinearVelocity));
        }

        public void ReverseVelocity()
        {
            CubeGrid.LinearVelocity = new Vector3(CubeGrid.LinearVelocity.X * -1, CubeGrid.LinearVelocity.Y * -1, CubeGrid.LinearVelocity.Z * -1);
            CubeGrid.AngularVelocity = new Vector3(CubeGrid.AngularVelocity.X * -1, CubeGrid.AngularVelocity.Y * -1, CubeGrid.AngularVelocity.Z * -1);
            OnPropertyChanged(nameof(LinearVelocity));
        }

        public void MaxVelocityAtPlayer(VRageMath.Vector3D playerPosition)
        {
            var v = playerPosition - CubeGrid.PositionAndOrientation.Value.Position;
            v.Normalize();
            v = Vector3.Multiply(v, SpaceEngineersConsts.MaxShipVelocity);

            CubeGrid.LinearVelocity = (Vector3)v;
            CubeGrid.AngularVelocity = new VRageMath.Vector3(0, 0, 0);
            OnPropertyChanged(nameof(LinearVelocity));
        }

        public bool ConvertFromLightToHeavyArmor()
        {
            bool changes = false;
            foreach (var cube in CubeGrid.CubeBlocks)
                changes |= CubeItemModel.ConvertFromLightToHeavyArmor(cube);

            if (changes)
            {
                IsSubsSystemNotReady = true;
                IsConstructionNotReady = true;
                UpdateGeneralFromEntityBase();
                InitializeAsync();
            }
            return changes;
        }

        public bool ConvertFromHeavyToLightArmor()
        {
            bool changes = false;
            foreach (var cube in CubeGrid.CubeBlocks)
                changes |= CubeItemModel.ConvertFromHeavyToLightArmor(cube);

            if (changes)
            {
                IsSubsSystemNotReady = true;
                IsConstructionNotReady = true;
                UpdateGeneralFromEntityBase();
                InitializeAsync();
            }
            return changes;
        }

        public void ConvertToFramework(float value)
        {
            foreach (var cube in CubeGrid.CubeBlocks)
            {
                cube.IntegrityPercent = value;
                cube.BuildPercent = value;
            }

            UpdateGeneralFromEntityBase();
        }

        public void ConvertToStation()
        {
            ResetVelocity();
            CubeGrid.IsStatic = true;
            UpdateGeneralFromEntityBase();
        }

        public int SetInertiaTensor(bool state)
        {
            int count = 0;

            foreach (MyObjectBuilder_CubeBlock cube in CubeGrid.CubeBlocks)
            {
                MyObjectBuilder_MechanicalConnectionBlock mechanicaBlock = cube as MyObjectBuilder_MechanicalConnectionBlock;
                if (mechanicaBlock != null)
                {
                    if (mechanicaBlock.ShareInertiaTensor != state)
                    {
                        count++;
                        mechanicaBlock.ShareInertiaTensor = state;
                    }
                }
            }

            return count;
        }

        public void ReorientStation()
        {
            var pos = CubeGrid.PositionAndOrientation.Value;
            pos.Position = pos.Position.RoundOff(MyCubeSize.Large.ToLength());
            pos.Forward = new SerializableVector3(-1, 0, 0); // The Station orientation has to be fixed, otherwise it glitches when you copy the object in game.
            pos.Up = new SerializableVector3(0, 1, 0);
            CubeGrid.PositionAndOrientation = pos;
        }

        public void RotateStructure(VRageMath.Quaternion quaternion)
        {
            // Rotate the ship/station in specified direction.
            var o = CubeGrid.PositionAndOrientation.Value.ToQuaternion() * quaternion;
            o.Normalize();
            var p = new MyPositionAndOrientation(o.ToMatrix());

            CubeGrid.PositionAndOrientation = new MyPositionAndOrientation
            {
                Position = CubeGrid.PositionAndOrientation.Value.Position,
                Forward = p.Forward,
                Up = p.Up
            };

            UpdateGeneralFromEntityBase();
        }

        public void RotateCubes(VRageMath.Quaternion quaternion)
        {
            foreach (var cube in CubeGrid.CubeBlocks)
            {
                var definition = SpaceEngineersApi.GetCubeDefinition(cube.TypeId, CubeGrid.GridSizeEnum, cube.SubtypeName);

                if (definition.Size.X == 1 && definition.Size.Y == 1 && definition.Size.Z == 1)
                {
                    // rotate position around origin.
                    cube.Min = Vector3I.Transform(cube.Min.ToVector3I(), quaternion);
                }
                else
                {
                    // resolve size of component, and transform to original orientation.
                    var orientSize = definition.Size.Add(-1).Transform(cube.BlockOrientation).Abs();

                    var min = Vector3I.Transform(cube.Min.ToVector3I(), quaternion);
                    var blockMax = new Vector3I(cube.Min.X + orientSize.X, cube.Min.Y + orientSize.Y, cube.Min.Z + orientSize.Z);
                    var max = Vector3I.Transform(blockMax, quaternion);

                    cube.Min = new SerializableVector3I(Math.Min(min.X, max.X), Math.Min(min.Y, max.Y), Math.Min(min.Z, max.Z));
                }

                // rotate BlockOrientation.
                var q = quaternion * cube.BlockOrientation.ToQuaternion();
                q.Normalize();
                cube.BlockOrientation = new SerializableBlockOrientation(ref q);
            }

            // Rotate Groupings.
            foreach (var group in CubeGrid.BlockGroups)
            {
                for (var i = 0; i < group.Blocks.Count; i++)
                {
                    // The Group location is in the center of the cube.
                    // It doesn't have to be exact though, as it appears SE is just doing a location test of whatever object is at that location.
                    group.Blocks[i] = Vector3I.Transform(group.Blocks[i], quaternion);
                }
            }

            // TODO: Rotate Bones
            //if (CubeGrid.Skeleton != null)
            //{
            //    for (var i = 0; i < CubeGrid.Skeleton.Count; i++)
            //    {
            //        var bone = CubeGrid.Skeleton[i];
            //        bone.BonePosition = Vector3I.Transform(bone.BonePosition, quaternion);
            //        bone.BoneOffset = bone.BoneOffset.Transform(VRageMath.Quaternion.Inverse(quaternion));
            //    }
            //}

            // TODO: Rotate ConveyorLines
            foreach (var conveyorLine in CubeGrid.ConveyorLines)
            {
                //conveyorLine.StartPosition = Vector3I.Transform(conveyorLine.StartPosition, quaternion);
                //conveyorLine.StartDirection = 
                //conveyorLine.EndPosition = Vector3I.Transform(conveyorLine.EndPosition, quaternion);
                //conveyorLine.EndDirection = 
            }

            // Rotate the ship also to maintain the appearance that it has not changed.
            var o = CubeGrid.PositionAndOrientation.Value.ToQuaternion() * VRageMath.Quaternion.Inverse(quaternion);
            o.Normalize();
            var p = new MyPositionAndOrientation(o.ToMatrix());

            CubeGrid.PositionAndOrientation = new MyPositionAndOrientation
            {
                Position = CubeGrid.PositionAndOrientation.Value.Position,
                Forward = p.Forward,
                Up = p.Up
            };

            UpdateGeneralFromEntityBase();
        }

        public void ConvertToShip()
        {
            CubeGrid.IsStatic = false;
            UpdateGeneralFromEntityBase();
        }

        public bool ConvertToCornerArmor()
        {
            var count = 0;
            count += CubeGrid.CubeBlocks.Where(c => c.SubtypeName == SubtypeId.LargeRoundArmor_Corner.ToString()).Select(c => { c.SubtypeName = SubtypeId.LargeBlockArmorCorner.ToString(); return c; }).ToList().Count;
            count += CubeGrid.CubeBlocks.Where(c => c.SubtypeName == SubtypeId.LargeRoundArmor_Slope.ToString()).Select(c => { c.SubtypeName = SubtypeId.LargeBlockArmorSlope.ToString(); return c; }).ToList().Count;
            count += CubeGrid.CubeBlocks.Where(c => c.SubtypeName == SubtypeId.LargeRoundArmor_CornerInv.ToString()).Select(c => { c.SubtypeName = SubtypeId.LargeBlockArmorCornerInv.ToString(); return c; }).ToList().Count;
            return count > 0;
        }

        public bool ConvertToRoundArmor()
        {
            var count = 0;
            count += CubeGrid.CubeBlocks.Where(c => c.SubtypeName == SubtypeId.LargeBlockArmorCorner.ToString()).Select(c => { c.SubtypeName = SubtypeId.LargeRoundArmor_Corner.ToString(); return c; }).ToList().Count;
            count += CubeGrid.CubeBlocks.Where(c => c.SubtypeName == SubtypeId.LargeBlockArmorSlope.ToString()).Select(c => { c.SubtypeName = SubtypeId.LargeRoundArmor_Slope.ToString(); return c; }).ToList().Count;
            count += CubeGrid.CubeBlocks.Where(c => c.SubtypeName == SubtypeId.LargeBlockArmorCornerInv.ToString()).Select(c => { c.SubtypeName = SubtypeId.LargeRoundArmor_CornerInv.ToString(); return c; }).ToList().Count;
            return count > 0;
        }

        public bool SetPivotPntHere(ViewModels.CubeItemViewModel selectedCubeItem)
        {
            // Is the selectedCubeItem the pivot point?
            var definitionS = SpaceEngineersApi.GetCubeDefinition(selectedCubeItem.TypeId, CubeGrid.GridSizeEnum, selectedCubeItem.SubtypeId);
            SerializableBlockOrientation blockOrientation = selectedCubeItem.Cube.BlockOrientation;
            var cubePos = selectedCubeItem.Cube.Min.ToVector3I();
            if (definitionS.Size.X == 1 && definitionS.Size.Y == 1 && definitionS.Size.Z == 1) 
            { 
            }
            else
            {
                var orientCenter = definitionS.Center.Transform(blockOrientation).Abs();
                cubePos = selectedCubeItem.Cube.Min.ToVector3I() + orientCenter;
            }
            if(cubePos.X == 0 && cubePos.Y == 0 && cubePos.Z == 0)
            {
                return false;
            }

            //Update the position of each cube to be relative to the selected cube.
            var position = selectedCubeItem.Position;
            foreach (var cube in CubeGrid.CubeBlocks)
            {
                // Offset the cube's position by the passed position.
                cube.Min = new SerializableVector3I(cube.Min.X - cubePos.X, cube.Min.Y - cubePos.Y, cube.Min.Z - cubePos.Z);
            }

            //Offset the ship's position by the passed position.
            var pivot = CubeGrid.PositionAndOrientation.Value.Position;
            var scaledPosition = new VRageMath.Vector3D(cubePos.X, cubePos.Y, cubePos.Z);
            scaledPosition *= CubeGrid.GridSizeEnum == MyCubeSize.Large ? 2.5 : 1.0;
            CubeGrid.PositionAndOrientation = new MyPositionAndOrientation
            {
                Position = new VRageMath.Vector3D(pivot.X + scaledPosition.X, pivot.Y + scaledPosition.Y, pivot.Z + scaledPosition.Z),
                Forward = CubeGrid.PositionAndOrientation.Value.Forward,
                Up = CubeGrid.PositionAndOrientation.Value.Up
            };


            return true;
        }

        #region Mirror

        public bool MirrorModel(bool usePlane, bool oddMirror)
        {
            var xMirror = Mirror.None;
            var yMirror = Mirror.None;
            var zMirror = Mirror.None;
            var xAxis = 0;
            var yAxis = 0;
            var zAxis = 0;
            var count = 0;

            if (!usePlane)
            // Find mirror Axis.
            //if (!CubeGrid.XMirroxPlane.HasValue && !CubeGrid.YMirroxPlane.HasValue && !CubeGrid.ZMirroxPlane.HasValue)
            {
                // Find the largest contigious exterior surface to use as the mirror.
                var minX = CubeGrid.CubeBlocks.Min(c => c.Min.X);
                var maxX = CubeGrid.CubeBlocks.Max(c => c.Min.X);
                var minY = CubeGrid.CubeBlocks.Min(c => c.Min.Y);
                var maxY = CubeGrid.CubeBlocks.Max(c => c.Min.Y);
                var minZ = CubeGrid.CubeBlocks.Min(c => c.Min.Z);
                var maxZ = CubeGrid.CubeBlocks.Max(c => c.Min.Z);

                var countMinX = CubeGrid.CubeBlocks.Count(c => c.Min.X == minX);
                var countMinY = CubeGrid.CubeBlocks.Count(c => c.Min.Y == minY);
                var countMinZ = CubeGrid.CubeBlocks.Count(c => c.Min.Z == minZ);
                var countMaxX = CubeGrid.CubeBlocks.Count(c => c.Min.X == maxX);
                var countMaxY = CubeGrid.CubeBlocks.Count(c => c.Min.Y == maxY);
                var countMaxZ = CubeGrid.CubeBlocks.Count(c => c.Min.Z == maxZ);

                if (countMinX > countMinY && countMinX > countMinZ && countMinX > countMaxX && countMinX > countMaxY && countMinX > countMaxZ)
                {
                    xMirror = oddMirror ? Mirror.Odd : Mirror.EvenDown;
                    xAxis = minX;
                }
                else if (countMinY > countMinX && countMinY > countMinZ && countMinY > countMaxX && countMinY > countMaxY && countMinY > countMaxZ)
                {
                    yMirror = oddMirror ? Mirror.Odd : Mirror.EvenDown;
                    yAxis = minY;
                }
                else if (countMinZ > countMinX && countMinZ > countMinY && countMinZ > countMaxX && countMinZ > countMaxY && countMinZ > countMaxZ)
                {
                    zMirror = oddMirror ? Mirror.Odd : Mirror.EvenDown;
                    zAxis = minZ;
                }
                else if (countMaxX > countMinX && countMaxX > countMinY && countMaxX > countMinZ && countMaxX > countMaxY && countMaxX > countMaxZ)
                {
                    xMirror = oddMirror ? Mirror.Odd : Mirror.EvenUp;
                    xAxis = maxX;
                }
                else if (countMaxY > countMinX && countMaxY > countMinY && countMaxY > countMinZ && countMaxY > countMaxX && countMaxY > countMaxZ)
                {
                    yMirror = oddMirror ? Mirror.Odd : Mirror.EvenUp;
                    yAxis = maxY;
                }
                else if (countMaxZ > countMinX && countMaxZ > countMinY && countMaxZ > countMinZ && countMaxZ > countMaxX && countMaxZ > countMaxY)
                {
                    zMirror = oddMirror ? Mirror.Odd : Mirror.EvenUp;
                    zAxis = maxZ;
                }

                var cubes = MirrorCubes(this, false, xMirror, xAxis, yMirror, yAxis, zMirror, zAxis).ToArray();
                CubeGrid.CubeBlocks.AddRange(cubes);
                count += cubes.Length;
            }
            else
            {
                // Use the built in Mirror plane defined in game.
                if (CubeGrid.XMirroxPlane.HasValue)
                {
                    xMirror = CubeGrid.XMirroxOdd ? Mirror.EvenDown : Mirror.Odd; // Meaning is back to front? Or is it my reasoning?
                    xAxis = CubeGrid.XMirroxPlane.Value.X;
                    var cubes = MirrorCubes(this, true, xMirror, xAxis, Mirror.None, 0, Mirror.None, 0).ToArray();
                    CubeGrid.CubeBlocks.AddRange(cubes);
                    count += cubes.Length;

                    // TODO: mirror BlockGroups
                    // TODO: mirror ConveyorLines 
                }
                if (CubeGrid.YMirroxPlane.HasValue)
                {
                    yMirror = CubeGrid.YMirroxOdd ? Mirror.EvenDown : Mirror.Odd;
                    yAxis = CubeGrid.YMirroxPlane.Value.Y;
                    var cubes = MirrorCubes(this, true, Mirror.None, 0, yMirror, yAxis, Mirror.None, 0).ToArray();
                    CubeGrid.CubeBlocks.AddRange(cubes);
                    count += cubes.Length;

                    // TODO: mirror BlockGroups
                    // TODO: mirror ConveyorLines 
                }
                if (CubeGrid.ZMirroxPlane.HasValue)
                {
                    zMirror = CubeGrid.ZMirroxOdd ? Mirror.EvenUp : Mirror.Odd;
                    zAxis = CubeGrid.ZMirroxPlane.Value.Z;
                    var cubes = MirrorCubes(this, true, Mirror.None, 0, Mirror.None, 0, zMirror, zAxis).ToArray();
                    CubeGrid.CubeBlocks.AddRange(cubes);
                    count += cubes.Length;

                    // TODO: mirror BlockGroups
                    // TODO: mirror ConveyorLines 
                }
            }

            UpdateGeneralFromEntityBase();
            OnPropertyChanged(nameof(BlockCount));
            return count > 0;
        }

        private static IEnumerable<MyObjectBuilder_CubeBlock> MirrorCubes(StructureCubeGridModel viewModel, bool integrate, Mirror xMirror, int xAxis, Mirror yMirror, int yAxis, Mirror zMirror, int zAxis)
        {
            var blocks = new List<MyObjectBuilder_CubeBlock>();

            if (xMirror == Mirror.None && yMirror == Mirror.None && zMirror == Mirror.None)
                return blocks;

            foreach (var block in viewModel.CubeGrid.CubeBlocks)
            {
                var newBlock = block.Clone() as MyObjectBuilder_CubeBlock;
                newBlock.EntityId = block.EntityId == 0 ? 0 : SpaceEngineersApi.GenerateEntityId(IDType.ENTITY);

                if (block is MyObjectBuilder_MotorBase)
                {
                    ((MyObjectBuilder_MotorBase)newBlock).RotorEntityId = ((MyObjectBuilder_MotorBase)block).RotorEntityId == 0 ? 0 : SpaceEngineersApi.GenerateEntityId(IDType.ENTITY);
                }

                if (block is MyObjectBuilder_PistonBase)
                {
                    ((MyObjectBuilder_PistonBase)newBlock).TopBlockId = ((MyObjectBuilder_PistonBase)block).TopBlockId == 0 ? 0 : SpaceEngineersApi.GenerateEntityId(IDType.ENTITY);
                }

                MyCubeBlockDefinition definition = SpaceEngineersApi.GetCubeDefinition(block.TypeId, viewModel.GridSize, block.SubtypeName);
                MyCubeBlockDefinition mirrorDefinition;
                MirrorCubeOrientation(definition, block.BlockOrientation, xMirror, yMirror, zMirror, out mirrorDefinition, out newBlock.BlockOrientation);

                newBlock.SubtypeName = mirrorDefinition.Id.SubtypeName;

                if (definition.Size.X == 1 && definition.Size.Y == 1 && definition.Size.Z == 1)
                {
                    newBlock.Min = block.Min.Mirror(xMirror, xAxis, yMirror, yAxis, zMirror, zAxis);
                }
                else
                {
                    // resolve size of component, and transform to original orientation.
                    var orientSize = definition.Size.Add(-1).Transform(block.BlockOrientation).Abs();

                    var min = block.Min.Mirror(xMirror, xAxis, yMirror, yAxis, zMirror, zAxis);
                    var blockMax = new SerializableVector3I(block.Min.X + orientSize.X, block.Min.Y + orientSize.Y, block.Min.Z + orientSize.Z);
                    var max = blockMax.Mirror(xMirror, xAxis, yMirror, yAxis, zMirror, zAxis);

                    if (xMirror != Mirror.None)
                        newBlock.Min = new SerializableVector3I(max.X, min.Y, min.Z);
                    if (yMirror != Mirror.None)
                        newBlock.Min = new SerializableVector3I(min.X, max.Y, min.Z);
                    if (zMirror != Mirror.None)
                        newBlock.Min = new SerializableVector3I(min.X, min.Y, max.Z);
                }

                // Don't place a block if one already exists there in the mirror.
                if (integrate && viewModel.CubeGrid.CubeBlocks.Any(b => b.Min.X == newBlock.Min.X && b.Min.Y == newBlock.Min.Y && b.Min.Z == newBlock.Min.Z /*|| b.Max == newBlock.Min*/))  // TODO: check cubeblock size.
                    continue;

                blocks.Add(newBlock);
            }
            return blocks;
        }

        private static void MirrorCubeOrientation(MyCubeBlockDefinition definition, SerializableBlockOrientation orientation, Mirror xMirror, Mirror yMirror, Mirror zMirror, out MyCubeBlockDefinition mirrorDefinition, out SerializableBlockOrientation mirrorOrientation)
        {
            mirrorDefinition = string.IsNullOrEmpty(definition.MirroringBlock) ? definition : SpaceEngineersApi.GetCubeDefinition(definition.Id.TypeId, definition.CubeSize, definition.MirroringBlock);

            Matrix sourceMatrix = Matrix.CreateFromDir(Base6Directions.GetVector(orientation.Forward), Base6Directions.GetVector(orientation.Up));
            Matrix targetMatrix;

            Vector3 mirrorNormal = Vector3.Zero;
            if (xMirror != Mirror.None)
                mirrorNormal = Vector3.Right;
            else if (yMirror != Mirror.None)
                mirrorNormal = Vector3.Up;
            else if (zMirror != Mirror.None)
                mirrorNormal = Vector3.Forward;

            MySymmetryAxisEnum blockMirrorAxis = MySymmetryAxisEnum.None;
            if (MyUtils.IsZero(Math.Abs(Vector3.Dot(sourceMatrix.Right, mirrorNormal)) - 1.0f))
                blockMirrorAxis = MySymmetryAxisEnum.X;
            else if (MyUtils.IsZero(Math.Abs(Vector3.Dot(sourceMatrix.Up, mirrorNormal)) - 1.0f))
                blockMirrorAxis = MySymmetryAxisEnum.Y;
            else if (MyUtils.IsZero(Math.Abs(Vector3.Dot(sourceMatrix.Forward, mirrorNormal)) - 1.0f))
                blockMirrorAxis = MySymmetryAxisEnum.Z;

            MySymmetryAxisEnum blockMirrorOption = MySymmetryAxisEnum.None;
            switch (blockMirrorAxis)
            {
                case MySymmetryAxisEnum.X: blockMirrorOption = definition.SymmetryX; break;
                case MySymmetryAxisEnum.Y: blockMirrorOption = definition.SymmetryY; break;
                case MySymmetryAxisEnum.Z: blockMirrorOption = definition.SymmetryZ; break;
                default: System.Diagnostics.Debug.Assert(false); break;
            }

            switch (blockMirrorOption)
            {
                case MySymmetryAxisEnum.X:
                    targetMatrix = Matrix.CreateRotationX(MathHelper.Pi) * sourceMatrix;
                    break;
                case MySymmetryAxisEnum.Y:
                case MySymmetryAxisEnum.YThenOffsetX:
                    targetMatrix = Matrix.CreateRotationY(MathHelper.Pi) * sourceMatrix;
                    break;
                case MySymmetryAxisEnum.Z:
                case MySymmetryAxisEnum.ZThenOffsetX:
                    targetMatrix = Matrix.CreateRotationZ(MathHelper.Pi) * sourceMatrix;
                    break;
                case MySymmetryAxisEnum.HalfX:
                    targetMatrix = Matrix.CreateRotationX(-MathHelper.PiOver2) * sourceMatrix;
                    break;
                case MySymmetryAxisEnum.HalfY:
                    targetMatrix = Matrix.CreateRotationY(-MathHelper.PiOver2) * sourceMatrix;
                    break;
                case MySymmetryAxisEnum.HalfZ:
                    targetMatrix = Matrix.CreateRotationZ(-MathHelper.PiOver2) * sourceMatrix;
                    break;
                case MySymmetryAxisEnum.XHalfY:
                    targetMatrix = Matrix.CreateRotationX(MathHelper.Pi) * sourceMatrix;
                    targetMatrix = Matrix.CreateRotationY(MathHelper.PiOver2) * targetMatrix;
                    break;
                case MySymmetryAxisEnum.YHalfY:
                    targetMatrix = Matrix.CreateRotationY(MathHelper.Pi) * sourceMatrix;
                    targetMatrix = Matrix.CreateRotationY(MathHelper.PiOver2) * targetMatrix;
                    break;
                case MySymmetryAxisEnum.ZHalfY:
                    targetMatrix = Matrix.CreateRotationZ(MathHelper.Pi) * sourceMatrix;
                    targetMatrix = Matrix.CreateRotationY(MathHelper.PiOver2) * targetMatrix;
                    break;
                case MySymmetryAxisEnum.XHalfX:
                    targetMatrix = Matrix.CreateRotationX(MathHelper.Pi) * sourceMatrix;
                    targetMatrix = Matrix.CreateRotationX(-MathHelper.PiOver2) * targetMatrix;
                    break;
                case MySymmetryAxisEnum.YHalfX:
                    targetMatrix = Matrix.CreateRotationY(MathHelper.Pi) * sourceMatrix;
                    targetMatrix = Matrix.CreateRotationX(-MathHelper.PiOver2) * targetMatrix;
                    break;
                case MySymmetryAxisEnum.ZHalfX:
                    targetMatrix = Matrix.CreateRotationZ(MathHelper.Pi) * sourceMatrix;
                    targetMatrix = Matrix.CreateRotationX(-MathHelper.PiOver2) * targetMatrix;
                    break;
                case MySymmetryAxisEnum.XHalfZ:
                    targetMatrix = Matrix.CreateRotationX(MathHelper.Pi) * sourceMatrix;
                    targetMatrix = Matrix.CreateRotationZ(-MathHelper.PiOver2) * targetMatrix;
                    break;
                case MySymmetryAxisEnum.YHalfZ:
                    targetMatrix = Matrix.CreateRotationY(MathHelper.Pi) * sourceMatrix;
                    targetMatrix = Matrix.CreateRotationZ(-MathHelper.PiOver2) * targetMatrix;
                    break;
                case MySymmetryAxisEnum.ZHalfZ:
                    targetMatrix = Matrix.CreateRotationZ(MathHelper.Pi) * sourceMatrix;
                    targetMatrix = Matrix.CreateRotationZ(-MathHelper.PiOver2) * targetMatrix;
                    break;
                case MySymmetryAxisEnum.XMinusHalfZ:
                    targetMatrix = Matrix.CreateRotationX(MathHelper.Pi) * sourceMatrix;
                    targetMatrix = Matrix.CreateRotationZ(MathHelper.PiOver2) * targetMatrix;
                    break;
                case MySymmetryAxisEnum.YMinusHalfZ:
                    targetMatrix = Matrix.CreateRotationY(MathHelper.Pi) * sourceMatrix;
                    targetMatrix = Matrix.CreateRotationZ(MathHelper.PiOver2) * targetMatrix;
                    break;
                case MySymmetryAxisEnum.ZMinusHalfZ:
                    targetMatrix = Matrix.CreateRotationZ(MathHelper.Pi) * sourceMatrix;
                    targetMatrix = Matrix.CreateRotationZ(MathHelper.PiOver2) * targetMatrix;
                    break;
                case MySymmetryAxisEnum.XMinusHalfX:
                    targetMatrix = Matrix.CreateRotationX(MathHelper.Pi) * sourceMatrix;
                    targetMatrix = Matrix.CreateRotationX(MathHelper.PiOver2) * targetMatrix;
                    break;
                case MySymmetryAxisEnum.YMinusHalfX:
                    targetMatrix = Matrix.CreateRotationY(MathHelper.Pi) * sourceMatrix;
                    targetMatrix = Matrix.CreateRotationX(MathHelper.PiOver2) * targetMatrix;
                    break;
                case MySymmetryAxisEnum.ZMinusHalfX:
                    targetMatrix = Matrix.CreateRotationZ(MathHelper.Pi) * sourceMatrix;
                    targetMatrix = Matrix.CreateRotationX(MathHelper.PiOver2) * targetMatrix;
                    break;
                case MySymmetryAxisEnum.MinusHalfX:
                    targetMatrix = Matrix.CreateRotationX(MathHelper.PiOver2) * sourceMatrix;
                    break;
                case MySymmetryAxisEnum.MinusHalfY:
                    targetMatrix = Matrix.CreateRotationY(MathHelper.PiOver2) * sourceMatrix;
                    break;
                case MySymmetryAxisEnum.MinusHalfZ:
                    targetMatrix = Matrix.CreateRotationZ(MathHelper.PiOver2) * sourceMatrix;
                    break;
                default: // or MySymmetryAxisEnum.None
                    targetMatrix = sourceMatrix;
                    break;
            }

            // Note, the Base6Directions methods call GetDirection(), which rounds off the vector, which should prevent floating point errors creeping in.
            mirrorOrientation = new SerializableBlockOrientation(Base6Directions.GetForward(ref targetMatrix), Base6Directions.GetUp(ref targetMatrix));
        }

        #endregion

        private static string GetAxisIndicator(Base6Directions.Direction direction)
        {
            switch (Base6Directions.GetAxis(direction))
            {
                case Base6Directions.Axis.LeftRight: // X
                    return Base6Directions.GetVector(direction).X < 0 ? "-X" : "+X";
                case Base6Directions.Axis.UpDown: // Y
                    return Base6Directions.GetVector(direction).Y < 0 ? "-Y" : "+Y";
                case Base6Directions.Axis.ForwardBackward: // Z
                    return Base6Directions.GetVector(direction).Z < 0 ? "-Z" : "+Z";
            }

            return null;
        }

        #endregion
    }
}

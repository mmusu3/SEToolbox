namespace SEToolbox.Models
{
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Game.Entities;
    using SEToolbox.Interop;
    using System;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Xml.Serialization;
    using VRage;
    using VRage.Game;
    using VRage.Game.ObjectBuilders.Components;
    using VRage.ObjectBuilders;
    using VRageMath;

    [Serializable]
    public class StructureSafeZoneModel : StructureBaseModel
    {
        #region Fields
        [NonSerialized]
        private MyObjectBuilder_CubeBlock _creatingBlock;
        [NonSerialized]
        private MyObjectBuilder_CubeGrid _creatingGrid;

        #endregion

        #region ctor

        public StructureSafeZoneModel(MyObjectBuilder_EntityBase entityBase)
            : base(entityBase)
        {
        }

        #endregion

        #region Properties
        [XmlIgnore]
        public MyObjectBuilder_CubeBlock CreatingBlock => _creatingBlock;

        [XmlIgnore]
        public MyObjectBuilder_CubeGrid CreatingGrid => _creatingGrid;
        [XmlIgnore]
        public MyObjectBuilder_SafeZone SafeZone => EntityBase as MyObjectBuilder_SafeZone;

        [XmlIgnore]
        public bool IsVisible //Done
        {
            get 
            { 
                return SafeZone.IsVisible; 
            }
            set
            {
                if (value != SafeZone.IsVisible)
                {
                    SafeZone.IsVisible = value;
                    OnPropertyChanged(nameof(IsVisible));
                }
            }
        }

        [XmlIgnore]
        public bool IsEnabled //Done
        {
            get
            {
                return SafeZone.Enabled;
            }
            set
            {
                if (value != SafeZone.Enabled)
                {
                    SafeZone.Enabled = value;
                    OnPropertyChanged(nameof(IsEnabled));
                }
            }
        }
        
        [XmlIgnore]
        public float Radius //Done
        {
            get
            {
                return SafeZone.Radius;
            }

            set
            {
                float clamped = Math.Min(Math.Max(value, MySafeZone.MIN_RADIUS), MySafeZone.MAX_RADIUS);
                if (clamped != SafeZone.Radius)
                {
                    SafeZone.Radius = clamped;
                    OnPropertyChanged(nameof(Radius));
                }
            }
        }

        [XmlIgnore]
        public Vector3 Size
        {
            get => SafeZone.Size;
            set
            {
                // TODO unknown what the max values are, but I guess this is reasonable
                Vector3 clamped = new Vector3(
                    Math.Min(Math.Max(value.X, MySafeZone.MIN_RADIUS), MySafeZone.MAX_RADIUS),
                    Math.Min(Math.Max(value.Y, MySafeZone.MIN_RADIUS), MySafeZone.MAX_RADIUS),
                    Math.Min(Math.Max(value.Z, MySafeZone.MIN_RADIUS), MySafeZone.MAX_RADIUS)
                );
                if (SafeZone.Size != clamped)
                {
                    SafeZone.Size = clamped;
                    OnPropertyChanged(nameof(Size));
                }
            }
        }

        [XmlIgnore] 
        public SerializableVector3 ModelColor => SafeZone.ModelColor;

        [XmlIgnore]
        public string Texture
        {
            get => SafeZone.Texture;
            set
            {
                if (value != SafeZone.Texture)
                {
                    SafeZone.Texture = value;
                    OnPropertyChanged(nameof(Texture));
                }
            }
        }

        [XmlIgnore]
        public MySafeZoneShape ShapeEnum
        {
            get => SafeZone.Shape;
            set
            {
                if (value != SafeZone.Shape)
                {
                    SafeZone.Shape = value;
                    OnPropertyChanged(nameof(ShapeEnum));
                    OnPropertyChanged(nameof(Shape));
                }
            }
        }

        [XmlIgnore]
        public string Shape
        {
            get => ShapeEnum.ToString();
            set
            {
                if (Enum.TryParse(value, out MySafeZoneShape parsed))
                {
                    ShapeEnum = parsed;
                }
            }
        }

        [XmlIgnore]
        public MySafeZoneAccess AccessTypePlayersEnum
        {
            get => SafeZone.AccessTypePlayers;
            set
            {
                if (value != SafeZone.AccessTypePlayers)
                {
                    SafeZone.AccessTypePlayers = value;
                    OnPropertyChanged(nameof(AccessTypePlayersEnum));
                    OnPropertyChanged(nameof(AccessTypePlayers));
                }
            }
        }

        [XmlIgnore]
        public string AccessTypePlayers
        {
            get => AccessTypePlayersEnum.ToString();
            set
            {
                if (Enum.TryParse(value, out MySafeZoneAccess parsed))
                {
                    AccessTypePlayersEnum = parsed;
                }
            }
        }

        [XmlIgnore]
        public MySafeZoneAccess AccessTypeFactionsEnum
        {
            get => SafeZone.AccessTypeFactions;
            set
            {
                if (value != SafeZone.AccessTypeFactions)
                {
                    SafeZone.AccessTypeFactions = value;
                    OnPropertyChanged(nameof(AccessTypeFactionsEnum));
                    OnPropertyChanged(nameof(AccessTypeFactions));
                }
            }
        }

        [XmlIgnore]
        public string AccessTypeFactions
        {
            get => AccessTypeFactionsEnum.ToString();
            set
            {
                if (Enum.TryParse(value, out MySafeZoneAccess parsed))
                {
                    AccessTypeFactionsEnum = parsed;
                }
            }
        }

        [XmlIgnore]
        public MySafeZoneAccess AccessTypeGridsEnum
        {
            get => SafeZone.AccessTypeGrids;
            set
            {
                if (value != SafeZone.AccessTypeGrids)
                {
                    SafeZone.AccessTypeGrids = value;
                    OnPropertyChanged(nameof(AccessTypeGridsEnum));
                    OnPropertyChanged(nameof(AccessTypeGrids));
                }
            }
        }

        [XmlIgnore]
        public string AccessTypeGrids
        {
            get => AccessTypeGridsEnum.ToString();
            set
            {
                if (Enum.TryParse(value, out MySafeZoneAccess parsed))
                {
                    AccessTypeGridsEnum = parsed;
                }
            }
        }

        [XmlIgnore]
        public MySafeZoneAccess AccessTypeFloatingObjectsEnum
        {
            get => SafeZone.AccessTypeFloatingObjects;
            set
            {
                if (value != SafeZone.AccessTypeFloatingObjects)
                {
                    SafeZone.AccessTypeFloatingObjects = value;
                    OnPropertyChanged(nameof(AccessTypeFloatingObjectsEnum));
                    OnPropertyChanged(nameof(AccessTypeFloatingObjects));
                }
            }
        }

        [XmlIgnore]
        public string AccessTypeFloatingObjects
        {
            get => AccessTypeFloatingObjectsEnum.ToString();
            set
            {
                if (Enum.TryParse(value, out MySafeZoneAccess parsed))
                {
                    AccessTypeFloatingObjectsEnum = parsed;
                }
            }
        }
        
        public MySafeZoneAction AllowedActions
        {
            get => SafeZone.AllowedActions;
            set
            {
                if (value != SafeZone.AllowedActions)
                {
                    SafeZone.AllowedActions = value;
                    OnPropertyChanged(nameof(AllowedActions));
                }
            }
        }
        #endregion

        #region methods
        public void FindCreatingEntities()
        {
            var allGrids = ExplorerModel.Default.ActiveWorld.SectorData.SectorObjects
                .OfType<MyObjectBuilder_CubeGrid>();

            _creatingBlock = null;
            _creatingGrid = null;
            foreach (var grid in allGrids)
            {
                foreach (var block in grid.CubeBlocks)
                {
                    if (SafeZone.SafeZoneBlockId == block.EntityId)
                    {
                        _creatingBlock = block;
                        _creatingGrid = grid;
                        OnPropertyChanged(nameof(CreatingBlock));
                        OnPropertyChanged(nameof(CreatingGrid));
                        return;
                    }
                }
            }
            OnPropertyChanged(nameof(CreatingBlock));
            OnPropertyChanged(nameof(CreatingGrid));
            return;
        }
        [XmlIgnore]
        public long SafeZoneBlockId
        {
            get => SafeZone.SafeZoneBlockId;
            set
            {
                if (SafeZone.SafeZoneBlockId != value)
                {
                    SafeZone.SafeZoneBlockId = value;
                    OnPropertyChanged(nameof(SafeZoneBlockId));
                }
            }
        }

        public override void UpdateGeneralFromEntityBase()
        {
            ClassType = ClassType.SafeZone;
            Description = "Safe Zone";
            DisplayName = SafeZone.Name ?? "Safe Zone";
            Mass = 0;
        }

        [OnSerializing]
        private void OnSerializingMethod(StreamingContext context)
        {
            SerializedEntity = SpaceEngineersApi.Serialize<MyObjectBuilder_SafeZone>(SafeZone);
        }

        [OnDeserialized]
        private void OnDeserializedMethod(StreamingContext context)
        {
            EntityBase = SpaceEngineersApi.Deserialize<MyObjectBuilder_SafeZone>(SerializedEntity);
        }

        #endregion
    }
}

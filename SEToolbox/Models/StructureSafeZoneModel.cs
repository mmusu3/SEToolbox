namespace SEToolbox.Models
{
    using Sandbox.Common.ObjectBuilders;
    using SEToolbox.Interop;
    using System;
    using System.Runtime.Serialization;
    using System.Xml.Serialization;
    using VRage;
    using VRage.Game.ObjectBuilders.Components;
    using VRage.ObjectBuilders;
    using VRageMath;

    [Serializable]
    public class StructureSafeZoneModel : StructureBaseModel
    {
        #region Fields
       
        #endregion

        #region ctor

        public StructureSafeZoneModel(MyObjectBuilder_EntityBase entityBase)
            : base(entityBase)
        {
        }

        #endregion

        #region Properties

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
                if (value != SafeZone.Radius)
                {
                    SafeZone.Radius = value;
                    OnPropertyChanged(nameof(Radius));
                }
            }
        }

        [XmlIgnore]
        public Vector3 Size => SafeZone.Size; //TODO

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

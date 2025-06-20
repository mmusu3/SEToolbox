using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Sandbox.Common.ObjectBuilders;
using VRageMath;
using VRage.ObjectBuilders;
using SEToolbox.Interop;
using VRage;
using System.ComponentModel;

namespace SEToolbox.Models
{
    [Serializable]
    public class StructureSafeZoneModel : StructureBaseModel
    {
        public StructureSafeZoneModel(MyObjectBuilder_EntityBase entityBase)
            : base(entityBase)
        {
        }

        [XmlIgnore]
        public MyObjectBuilder_SafeZone SafeZone => EntityBase as MyObjectBuilder_SafeZone;

        [XmlIgnore]
        public bool IsVisible
        {
            get => SafeZone.IsVisible;
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
        public bool IsEnabled
        {
            get => SafeZone.Enabled;
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
        public float Radius
        {
            get => SafeZone.Radius;
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
        public Vector3 Size => SafeZone.Size;

        [XmlIgnore]
        public SerializableVector3 ModelColor => SafeZone.ModelColor;

        [XmlIgnore]
        public string Texture => SafeZone.Texture;

        #region Enum-Properties mit String-Wrapper

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

        #endregion

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
    }
}

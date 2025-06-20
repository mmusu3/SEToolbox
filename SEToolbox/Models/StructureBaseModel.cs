namespace SEToolbox.Models
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Threading;
    using System.Xml.Serialization;

    using Sandbox.Common.ObjectBuilders;
    using SEToolbox.Interfaces;
    using SEToolbox.Interop;
    using VRageMath;
    using VRage.ObjectBuilders;
    using VRage;
    using VRage.Game;
    using VRage.Game.ObjectBuilders;

    [Serializable]
    public class StructureBaseModel : BaseModel, IStructureBase
    {
        #region fields

        // Fields are marked as NonSerialized, as they aren't required during the drag-drop operation.

        [NonSerialized]
        private MyObjectBuilder_EntityBase _entityBase;

        [NonSerialized]
        private ClassType _classType;

        [NonSerialized]
        private string _name;

        [NonSerialized]
        private string _description;

        [NonSerialized]
        private Vector3D _center;

        [NonSerialized]
        private BoundingBoxD _worldAabb;

        [NonSerialized]
        private double _playerDistance;

        [NonSerialized]
        private double _mass;

        [NonSerialized]
        private int _blockCount;

        [NonSerialized]
        private double _linearVelocity;

        [NonSerialized]
        private bool _isBusy;

        [NonSerialized]
        internal bool _isValid;

        private string _serializedEntity;

        [NonSerialized]
        internal Dispatcher _dispatcher;

        #endregion

        #region ctor

        public StructureBaseModel()
        {
        }

        public StructureBaseModel(MyObjectBuilder_EntityBase entityBase)
        {
            _dispatcher = Dispatcher.CurrentDispatcher;
            EntityBase = entityBase;
        }

        #endregion

        #region Properties

        [XmlIgnore]
        public virtual MyObjectBuilder_EntityBase EntityBase
        {
            get { return _entityBase; }
            set
            {
                if (value != _entityBase)
                {
                    _entityBase = value;
                    UpdateGeneralFromEntityBase();
                    OnPropertyChanged(nameof(EntityBase));
                }
            }
        }

        [XmlIgnore]
        public long EntityId
        {
            get { return _entityBase.EntityId; }

            set
            {
                if (value != _entityBase.EntityId)
                {
                    _entityBase.EntityId = value;
                    OnPropertyChanged(nameof(EntityId));
                }
            }
        }

        [XmlIgnore]
        public MyPositionAndOrientation? PositionAndOrientation
        {
            get { return _entityBase.PositionAndOrientation; }

            set
            {
                if (!EqualityComparer<MyPositionAndOrientation?>.Default.Equals(value, _entityBase.PositionAndOrientation))
                //if (value != entityBase.PositionAndOrientation)
                {
                    _entityBase.PositionAndOrientation = value;
                    OnPropertyChanged(nameof(PositionAndOrientation));
                }
            }
        }

        [XmlIgnore]
        public double PositionX
        {
            get { return _entityBase.PositionAndOrientation.Value.Position.X; }

            set
            {
                if (value != _entityBase.PositionAndOrientation.Value.Position.X)
                {
                    var pos = _entityBase.PositionAndOrientation.Value;
                    pos.Position.X = value;
                    _entityBase.PositionAndOrientation = pos;
                    OnPropertyChanged(nameof(PositionX));
                }
            }
        }

        [XmlIgnore]
        public double PositionY
        {
            get { return _entityBase.PositionAndOrientation.Value.Position.Y; }

            set
            {
                if (value != _entityBase.PositionAndOrientation.Value.Position.Y)
                {
                    var pos = _entityBase.PositionAndOrientation.Value;
                    pos.Position.Y = value;
                    _entityBase.PositionAndOrientation = pos;
                    OnPropertyChanged(nameof(PositionY));
                }
            }
        }

        [XmlIgnore]
        public double PositionZ
        {
            get { return _entityBase.PositionAndOrientation.Value.Position.Z; }

            set
            {
                if (value != _entityBase.PositionAndOrientation.Value.Position.Z)
                {
                    var pos = _entityBase.PositionAndOrientation.Value;
                    pos.Position.Z = value;
                    _entityBase.PositionAndOrientation = pos;
                    OnPropertyChanged(nameof(PositionZ));
                }
            }
        }

        [XmlIgnore]
        public ClassType ClassType
        {
            get { return _classType; }

            set
            {
                if (value != _classType)
                {
                    _classType = value;
                    OnPropertyChanged(nameof(ClassType));
                }
            }
        }

        [XmlIgnore]
        public virtual string DisplayName
        {
            get { return _name; }

            set
            {
                if (value != _name)
                {
                    _name = value;
                    OnPropertyChanged(nameof(DisplayName));
                }
            }
        }

        [XmlIgnore]
        public string Description
        {
            get { return _description; }

            set
            {
                if (value != _description)
                {
                    _description = value;
                    OnPropertyChanged(nameof(Description));
                }
            }
        }

        [XmlIgnore]
        public double PlayerDistance
        {
            get { return _playerDistance; }

            set
            {
                if (value != _playerDistance)
                {
                    _playerDistance = value;
                    OnPropertyChanged(nameof(PlayerDistance));
                }
            }
        }

        [XmlIgnore]
        public double Mass
        {
            get { return _mass; }

            set
            {
                if (value != _mass)
                {
                    _mass = value;
                    OnPropertyChanged(nameof(Mass));
                }
            }
        }

        [XmlIgnore]
        public virtual int BlockCount
        {
            get { return _blockCount; }

            set
            {
                if (value != _blockCount)
                {
                    _blockCount = value;
                    OnPropertyChanged(nameof(BlockCount));
                }
            }
        }

        [XmlIgnore]
        public virtual double LinearVelocity
        {
            get { return _linearVelocity; }

            set
            {
                if (value != _linearVelocity)
                {
                    _linearVelocity = value;
                    OnPropertyChanged(nameof(LinearVelocity));
                }
            }
        }

        /// <summary>
        /// Center of the object in space.
        /// </summary>
        [XmlIgnore]
        public Vector3D Center
        {
            get { return _center; }

            set
            {
                if (value != _center)
                {
                    _center = value;
                    OnPropertyChanged(nameof(Center));
                }
            }
        }

        /// <summary>
        /// Bounding box.
        /// </summary>
        [XmlIgnore]
        public BoundingBoxD WorldAABB
        {
            get { return _worldAabb; }

            set
            {
                if (value != _worldAabb)
                {
                    _worldAabb = value;
                    OnPropertyChanged(nameof(WorldAABB));
                }
            }
        }

        public string SerializedEntity
        {
            get
            {
                return _serializedEntity;
            }

            set
            {
                if (value != _serializedEntity)
                {
                    _serializedEntity = value;
                    OnPropertyChanged(nameof(SerializedEntity));
                }
            }
        }

        [XmlIgnore]
        public bool IsBusy
        {
            get { return _isBusy; }

            set
            {
                if (value != _isBusy)
                {
                    _isBusy = value;
                    OnPropertyChanged(nameof(IsBusy));
                    if (_isBusy)
                    {
                        System.Windows.Forms.Application.DoEvents();
                    }
                }
            }
        }

        [XmlIgnore]
        public bool IsValid
        {
            get { return _isValid; }

            set
            {
                if (value != _isValid)
                {
                    _isValid = value;
                    OnPropertyChanged(nameof(IsValid));
                }
            }
        }

        #endregion

        #region methods

        public virtual void UpdateGeneralFromEntityBase()
        {
            ClassType = ClassType.Unknown;
        }

        public static IStructureBase Create(MyObjectBuilder_EntityBase entityBase, string savefilePath)
        {
            if (entityBase is MyObjectBuilder_Planet)
            {
                return new StructurePlanetModel(entityBase, savefilePath);
            }

            if (entityBase is MyObjectBuilder_VoxelMap)
            {
                return new StructureVoxelModel(entityBase, savefilePath);
            }

            if (entityBase is MyObjectBuilder_Character)
            {
                return new StructureCharacterModel(entityBase);
            }

            if (entityBase is MyObjectBuilder_CubeGrid)
            {
                return new StructureCubeGridModel(entityBase);
            }

            if (entityBase is MyObjectBuilder_FloatingObject)
            {
                return new StructureFloatingObjectModel(entityBase);
            }

            if (entityBase is MyObjectBuilder_Meteor)
            {
                return new StructureMeteorModel(entityBase);
            }

            if (entityBase is MyObjectBuilder_InventoryBagEntity)
            {
                return new StructureInventoryBagModel(entityBase);
            }
            if (entityBase is MyObjectBuilder_SafeZone)
            {
                return new StructureSafeZoneModel(entityBase);
            }
            return new StructureUnknownModel(entityBase);
            //throw new NotImplementedException(string.Format("A new object has not been catered for in the StructureBase, of type '{0}'.", entityBase.GetType()));
        }

        public virtual void InitializeAsync()
        {
            // to be overridden.
        }

        public virtual void CancelAsync()
        {
            // to be overridden.
        }

        public virtual void RecalcPosition(Vector3D playerPosition)
        {
            PlayerDistance = (playerPosition - PositionAndOrientation.Value.Position).Length();
        }

        #endregion
    }
}

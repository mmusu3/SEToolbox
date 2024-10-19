namespace SEToolbox.Models
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;

    using SEToolbox.Interop;
    using SEToolbox.Support;
    using VRage;

    public class GenerateFloatingObjectModel : BaseModel
    {
        public const int UniqueUnits = 1;

        #region Fields

        private MyPositionAndOrientation _characterPosition;
        private ObservableCollection<ComponentItemModel> _stockItemList;
        private ComponentItemModel _stockItem;

        private bool _isValidItemToImport;

        private double? _volume;
        private double? _mass;
        private int? _units;
        private decimal? _decimalUnits;
        private bool _isDecimal;
        private bool _isInt;
        private bool _isUnique;
        private int _multiplier;
        private float _maxFloatingObjects;

        #endregion

        #region ctor

        public GenerateFloatingObjectModel()
        {
            _stockItemList = new ObservableCollection<ComponentItemModel>();
            Multiplier = 1;
        }

        #endregion

        #region Properties

        public MyPositionAndOrientation CharacterPosition
        {
            get
            {
                return _characterPosition;
            }

            set
            {
                //if (value != characterPosition) // Unable to check for equivilence, without long statement. And, mostly uncessary.
                _characterPosition = value;
                OnPropertyChanged(nameof(CharacterPosition));
            }
        }

        public ObservableCollection<ComponentItemModel> StockItemList
        {
            get
            {
                return _stockItemList;
            }

            set
            {
                if (value != _stockItemList)
                {
                    _stockItemList = value;
                    OnPropertyChanged(nameof(StockItemList));
                }
            }
        }

        public ComponentItemModel StockItem
        {
            get
            {
                return _stockItem;
            }

            set
            {
                if (value != _stockItem)
                {
                    _stockItem = value;
                    OnPropertyChanged(nameof(StockItem));
                    SetMassVolume();
                }
            }
        }

        public bool IsValidItemToImport
        {
            get
            {
                return _isValidItemToImport;
            }

            set
            {
                if (value != _isValidItemToImport)
                {
                    _isValidItemToImport = value;
                    OnPropertyChanged(nameof(IsValidItemToImport));
                }
            }
        }

        public double? Volume
        {
            get
            {
                return _volume;
            }

            set
            {
                if (value != _volume)
                {
                    _volume = value;
                    OnPropertyChanged(nameof(Volume));
                }
            }
        }

        public double? Mass
        {
            get
            {
                return _mass;
            }

            set
            {
                if (value != _mass)
                {
                    _mass = value;
                    OnPropertyChanged(nameof(Mass));
                }
            }
        }

        public int? Units
        {
            get
            {
                return _units;
            }

            set
            {
                if (value != _units)
                {
                    _units = value;
                    OnPropertyChanged(nameof(Units));
                    SetMassVolume();
                }
            }
        }

        public decimal? DecimalUnits
        {
            get
            {
                return _decimalUnits;
            }

            set
            {
                if (value != _decimalUnits)
                {
                    _decimalUnits = value;
                    OnPropertyChanged(nameof(DecimalUnits));
                    SetMassVolume();
                }
            }
        }

        public bool IsDecimal
        {
            get
            {
                return _isDecimal;
            }

            set
            {
                if (value != _isDecimal)
                {
                    _isDecimal = value;
                    OnPropertyChanged(nameof(IsDecimal));
                }
            }
        }

        public bool IsInt
        {
            get
            {
                return _isInt;
            }

            set
            {
                if (value != _isInt)
                {
                    _isInt = value;
                    OnPropertyChanged(nameof(IsInt));
                }
            }
        }

        public bool IsUnique
        {
            get
            {
                return _isUnique;
            }

            set
            {
                if (value != _isUnique)
                {
                    _isUnique = value;
                    OnPropertyChanged(nameof(IsUnique));
                }
            }
        }

        /// <summary>
        /// Generates this many individual items.
        /// </summary>
        public int Multiplier
        {
            get
            {
                return _multiplier;
            }

            set
            {
                if (value != _multiplier)
                {
                    _multiplier = value;
                    OnPropertyChanged(nameof(Multiplier));
                }
            }
        }

        /// <summary>
        /// The maximum number of floating objects as defined in the World.
        /// </summary>
        public float MaxFloatingObjects
        {
            get
            {
                return _maxFloatingObjects;
            }

            set
            {
                if (value != _maxFloatingObjects)
                {
                    _maxFloatingObjects = value;
                    OnPropertyChanged(nameof(MaxFloatingObjects));
                }
            }
        }

        #endregion

        #region methods

        public void Load(MyPositionAndOrientation characterPosition, float maxFloatingObjects)
        {
            MaxFloatingObjects = maxFloatingObjects;
            CharacterPosition = characterPosition;
            StockItemList.Clear();
            var list = new List<ComponentItemModel>();
            var contentPath = ToolboxUpdater.GetApplicationContentPath();

            foreach (var componentDefinition in SpaceEngineersCore.Resources.ComponentDefinitions)
            {
                var bp = SpaceEngineersApi.GetBlueprint(componentDefinition.Id.TypeId, componentDefinition.Id.SubtypeName);
                list.Add(new ComponentItemModel
                {
                    Name = componentDefinition.DisplayNameText,
                    TypeId = componentDefinition.Id.TypeId,
                    SubtypeId = componentDefinition.Id.SubtypeName,
                    Mass = componentDefinition.Mass,
                    TextureFile = componentDefinition.Icons == null ? null : SpaceEngineersCore.GetDataPathOrDefault(componentDefinition.Icons.First(), Path.Combine(contentPath, componentDefinition.Icons.First())),
                    Volume = componentDefinition.Volume * SpaceEngineersConsts.VolumeMultiplyer,
                    Accessible = componentDefinition.Public,
                    Time = bp != null ? TimeSpan.FromSeconds(bp.BaseProductionTimeInSeconds) : (TimeSpan?)null,
                });
            }

            foreach (var physicalItemDefinition in SpaceEngineersCore.Resources.PhysicalItemDefinitions)
            {
                if (physicalItemDefinition.Id.TypeId == typeof(VRage.Game.MyObjectBuilder_TreeObject))
                    continue;

                if (physicalItemDefinition.Id.SubtypeName == "CubePlacerItem" || physicalItemDefinition.Id.SubtypeName == "WallPlacerItem")
                    continue;

                var bp = SpaceEngineersApi.GetBlueprint(physicalItemDefinition.Id.TypeId, physicalItemDefinition.Id.SubtypeName);
                list.Add(new ComponentItemModel
                {
                    Name = physicalItemDefinition.DisplayNameText,
                    TypeId = physicalItemDefinition.Id.TypeId,
                    SubtypeId = physicalItemDefinition.Id.SubtypeName,
                    Mass = physicalItemDefinition.Mass,
                    Volume = physicalItemDefinition.Volume * SpaceEngineersConsts.VolumeMultiplyer,
                    TextureFile = physicalItemDefinition.Icons == null ? null : SpaceEngineersCore.GetDataPathOrDefault(physicalItemDefinition.Icons.First(), Path.Combine(contentPath, physicalItemDefinition.Icons.First())),
                    Accessible = physicalItemDefinition.Public,
                    Time = bp != null ? TimeSpan.FromSeconds(bp.BaseProductionTimeInSeconds) : (TimeSpan?)null,
                });
            }

            foreach (var item in list.OrderBy(i => i.FriendlyName))
            {
                StockItemList.Add(item);
            }

            //list.Clear();

            //foreach (var cubeDefinition in SpaceEngineersAPI.CubeBlockDefinitions)
            //{
            //    list.Add(new ComponentItemModel
            //    {
            //        Name = cubeDefinition.DisplayName,
            //        TypeId = cubeDefinition.Id.TypeId,
            //        SubtypeId = cubeDefinition.Id.SubtypeName,
            //        CubeSize = cubeDefinition.CubeSize,
            //        TextureFile = cubeDefinition.Icon == null ? null : Path.Combine(contentPath, cubeDefinition.Icon),
            //        Accessible = !string.IsNullOrEmpty(cubeDefinition.Model),
            //    });
            //}

            //foreach (var item in list.OrderBy(i => i.FriendlyName))
            //{
            //    StockItemList.Add(item);
            //}
        }

        private void SetMassVolume()
        {
            if (StockItem == null)
            {
                Mass = null;
                Volume = null;
            }
            else
            {
                if (StockItem.TypeId == SpaceEngineersTypes.Ore ||
                    StockItem.TypeId == SpaceEngineersTypes.Ingot)
                {
                    IsDecimal = true;
                    IsUnique = IsInt = false;
                    if (DecimalUnits.HasValue)
                    {
                        Mass = (double)DecimalUnits * StockItem.Mass;
                        Volume = (double)DecimalUnits * StockItem.Volume;
                    }
                    else
                    {
                        Mass = null;
                        Volume = null;
                    }
                }
                else if (StockItem.TypeId == SpaceEngineersTypes.Component ||
                    StockItem.TypeId == SpaceEngineersTypes.AmmoMagazine)
                {
                    IsInt = true;
                    IsUnique = IsDecimal = false;
                    if (Units.HasValue)
                    {
                        Mass = Units.Value * StockItem.Mass;
                        Volume = Units.Value * StockItem.Volume;
                    }
                    else
                    {
                        Mass = null;
                        Volume = null;
                    }
                }
                else if (StockItem.TypeId == SpaceEngineersTypes.PhysicalGunObject)
                {
                    IsUnique = true;
                    IsInt = IsDecimal = false;
                    Mass = UniqueUnits * StockItem.Mass;
                    Volume = UniqueUnits * StockItem.Volume;
                }
                else if (StockItem.TypeId == SpaceEngineersTypes.OxygenContainerObject)
                {
                    IsUnique = true;
                    IsInt = IsDecimal = false;
                    Mass = UniqueUnits * StockItem.Mass;
                    Volume = UniqueUnits * StockItem.Volume;
                }
                else
                {
                    // Assume any new objects are whole objects that cannot be stacked (for safety).
                    IsUnique = true;
                    IsInt = IsDecimal = false;
                    Mass = UniqueUnits * StockItem.Mass;
                    Volume = UniqueUnits * StockItem.Volume;
                }
            }
        }

        #endregion
    }
}

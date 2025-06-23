namespace SEToolbox.ViewModels
{
    using ObjectBuilders.SafeZone;
    using Sandbox.Common.ObjectBuilders;
    using SEToolbox.Models;
    using SEToolbox.Services;
    using System;
    using System.Collections.Generic;
    using System.Windows.Input;
    using VRage.Game;
    using VRage.Game.ObjectBuilders.Components;

    public class StructureSafeZoneViewModel : StructureBaseViewModel<StructureSafeZoneModel>
    {

        #region fields
        private ICommand _searchCreatingEntitiesCommand;
        #endregion

        #region Constructor

        public StructureSafeZoneViewModel(BaseViewModel parentViewModel, StructureSafeZoneModel dataModel)
            : base(parentViewModel, dataModel)
        {
            DataModel.PropertyChanged += (sender, e) =>
            {
                OnPropertyChanged(e.PropertyName);
                if (e.PropertyName == nameof(DataModel.CreatingBlock) || e.PropertyName == nameof(DataModel.CreatingGrid))
                {
                    OnPropertyChanged(nameof(CreatingBlockInfo));
                    OnPropertyChanged(nameof(CreatingGridInfo));
                }
            };
        }

        #endregion

        #region Commands

        public ICommand SearchCreatingEntitiesCommand
        {
            get
            {
                if (_searchCreatingEntitiesCommand == null)
                {
                    _searchCreatingEntitiesCommand = new DelegateCommand(
                        SearchCreatingEntitiesExecuted,
                        SearchCreatingEntitiesCanExecute
                    );
                }
                return _searchCreatingEntitiesCommand;
            }
        }

        #endregion

        #region Properties
        protected new StructureSafeZoneModel DataModel => base.DataModel as StructureSafeZoneModel;

        // --- SafeZone Settings ---
        public bool IsVisible
        {
            get { return DataModel.IsVisible; }
            set { DataModel.IsVisible = value; MainViewModel.IsModified = true; }
        }

        public bool IsEnabled
        {
            get { return DataModel.IsEnabled; }
            set { DataModel.IsEnabled = value; MainViewModel.IsModified = true; }
        }

        public float Radius
        {
            get { return DataModel.Radius; }
            set { DataModel.Radius = value; MainViewModel.IsModified = true; }
        }

        public MySafeZoneShape Shape
        {
            get => DataModel.ShapeEnum;
            set
            {
                if (value != DataModel.ShapeEnum)
                {
                    DataModel.ShapeEnum = value;
                    OnPropertyChanged(nameof(Shape));
                    OnPropertyChanged(nameof(IsSphereShape));
                    MainViewModel.IsModified = true;
                }
            }
        }

        public string Texture
        {
            get => DataModel.Texture;
            set
            {
                if (value != DataModel.Texture)
                {
                    DataModel.Texture = value;
                    OnPropertyChanged(nameof(Texture));
                    MainViewModel.IsModified = true;
                }
            }
        }

        public bool IsSphereShape => Shape == MySafeZoneShape.Sphere;
        public MySafeZoneAccess AccessTypePlayers
        {
            get { return DataModel.AccessTypePlayersEnum; }
            set
            {
                if (value != DataModel.AccessTypePlayersEnum)
                {
                    DataModel.AccessTypePlayersEnum = value;
                    MainViewModel.IsModified = true;
                    OnPropertyChanged(nameof(AccessTypePlayers));
                }
            }
        }

        public MySafeZoneAccess AccessTypeFactions
        {
            get => DataModel.AccessTypeFactionsEnum;
            set
            {
                if (value != DataModel.AccessTypeFactionsEnum)
                {
                    DataModel.AccessTypeFactionsEnum = value;
                    MainViewModel.IsModified = true;
                    OnPropertyChanged(nameof(AccessTypeFactions));
                }
            }
        }

        public MySafeZoneAccess AccessTypeGrids
        {
            get => DataModel.AccessTypeGridsEnum;
            set
            {
                if (value != DataModel.AccessTypeGridsEnum)
                {
                    DataModel.AccessTypeGridsEnum = value;
                    MainViewModel.IsModified = true;
                    OnPropertyChanged(nameof(AccessTypeGrids));
                }
            }
        }

        public MySafeZoneAccess AccessTypeFloatingObjects
        {
            get => DataModel.AccessTypeFloatingObjectsEnum;
            set
            {
                if (value != DataModel.AccessTypeFloatingObjectsEnum)
                {
                    DataModel.AccessTypeFloatingObjectsEnum = value;
                    MainViewModel.IsModified = true;
                    OnPropertyChanged(nameof(AccessTypeFloatingObjects));
                }
            }
        }

        // --- Allowed Actions Flags ---
        public MySafeZoneAction AllowedActions
        {
            get => DataModel.AllowedActions;
            set
            {
                if (DataModel.AllowedActions != value)
                {
                    DataModel.AllowedActions = value;
                    OnPropertyChanged(nameof(AllowedActions));
                    RaiseAllowedActionsProperties();
                    MainViewModel.IsModified = true;
                }
            }
        }
        public bool IsDamageAllowed
        {
            get => AllowedActions.HasFlag(MySafeZoneAction.Damage);
            set => SetAllowedActionFlag(MySafeZoneAction.Damage, value, nameof(IsDamageAllowed));
        }

        public bool IsShootingAllowed
        {
            get => AllowedActions.HasFlag(MySafeZoneAction.Shooting);
            set => SetAllowedActionFlag(MySafeZoneAction.Shooting, value, nameof(IsShootingAllowed));
        }

        public bool IsDrillingAllowed
        {
            get => AllowedActions.HasFlag(MySafeZoneAction.Drilling);
            set => SetAllowedActionFlag(MySafeZoneAction.Drilling, value, nameof(IsDrillingAllowed));
        }

        public bool IsWeldingAllowed
        {
            get => AllowedActions.HasFlag(MySafeZoneAction.Welding);
            set => SetAllowedActionFlag(MySafeZoneAction.Welding, value, nameof(IsWeldingAllowed));
        }

        public bool IsGrindingAllowed
        {
            get => AllowedActions.HasFlag(MySafeZoneAction.Grinding);
            set => SetAllowedActionFlag(MySafeZoneAction.Grinding, value, nameof(IsGrindingAllowed));
        }

        public bool IsBuildingAllowed
        {
            get => AllowedActions.HasFlag(MySafeZoneAction.Building);
            set => SetAllowedActionFlag(MySafeZoneAction.Building, value, nameof(IsBuildingAllowed));
        }

        public bool IsBuildingProjectionsAllowed
        {
            get => AllowedActions.HasFlag(MySafeZoneAction.BuildingProjections);
            set => SetAllowedActionFlag(MySafeZoneAction.BuildingProjections, value, nameof(IsBuildingProjectionsAllowed));
        }

        public bool IsVoxelHandAllowed
        {
            get => AllowedActions.HasFlag(MySafeZoneAction.VoxelHand);
            set => SetAllowedActionFlag(MySafeZoneAction.VoxelHand, value, nameof(IsVoxelHandAllowed));
        }

        public bool IsLandingGearLockAllowed
        {
            get => AllowedActions.HasFlag(MySafeZoneAction.LandingGearLock);
            set => SetAllowedActionFlag(MySafeZoneAction.LandingGearLock, value, nameof(IsLandingGearLockAllowed));
        }

        public bool IsConvertToStationAllowed
        {
            get => AllowedActions.HasFlag(MySafeZoneAction.ConvertToStation);
            set => SetAllowedActionFlag(MySafeZoneAction.ConvertToStation, value, nameof(IsConvertToStationAllowed));
        }

        // --- Possibilities for Textures (UI-Bindings) ---
        public List<string> AvailableTextures { get; } = new() {  "SafeZone_Texture_Default",
            "SafeZone_Texture_Aura",
            "SafeZone_Texture_Organic",
            "SafeZone_Texture_Rain",
            "SafeZone_Texture_Dots",
            "SafeZone_Texture_Disco",
            "SafeZone_Texture_Noise",
            "SafeZone_Texture_Hexagon",
            "SafeZone_Texture_Lines",
            "SafeZone_Texture_Digital",
            "SafeZone_Texture_Gloura",
            "SafeZone_Texture_Clang",
            "SafeZone_Texture_Voronoi",
            "SafeZone_Texture_Restricted",
            "SafeZone_Texture_KeenSWH",
            "SafeZone_Texture_Disabled"
            }; // TODO: Sadly I have no idea how to get the actual textures from the game, so this is a hardcoded list. Works until something changes.

        public Array AvailableShapes => Enum.GetValues(typeof(MySafeZoneShape));
        public Array AccessTypes => Enum.GetValues(typeof(MySafeZoneAccess));

        #endregion

        #region Properties
        // --- creating Block/Grid ---
        public MyObjectBuilder_CubeBlock CreatingBlock => DataModel.CreatingBlock;
        public MyObjectBuilder_CubeGrid CreatingGrid => DataModel.CreatingGrid;

        public string CreatingBlockInfo
        {
            get
            {
                //TODO This is still very crude, but it works for now.
                MyObjectBuilder_SafeZoneBlock safeZoneBlock = CreatingBlock as MyObjectBuilder_SafeZoneBlock;
                if (safeZoneBlock == null)
                    return "(no Block found)";

                var name = safeZoneBlock.CustomName ?? safeZoneBlock.Name ?? "<no Name>";
                var type = CreatingBlock.GetType().Name;
                var subtype = CreatingBlock.SubtypeName ?? "";
                return $"{name} (Type: {type}{(string.IsNullOrWhiteSpace(subtype) ? "" : $", Subtype: {subtype}")}) [ID: {CreatingBlock.EntityId}]";
            }
        }
        public string CreatingGridInfo
        {
            get
            {
                //TODO This is still very crude, but it works for now.
                if (CreatingGrid == null)
                    return "(no Grid found)";

                var name = CreatingGrid.DisplayName ?? CreatingGrid.Name ?? "<no name>";
                var id = CreatingGrid.EntityId;
                var subtype = CreatingGrid.SubtypeName ?? "";
                return !string.IsNullOrWhiteSpace(subtype) ? $"{name} (Typ: {subtype}) [ID: {id}]" : $"{name} [ID: {id}]";
            }
        }
        // --- Size und Color ---
        public float SizeX
        {
            get => DataModel.Size.X;
            set
            {
                if (DataModel.Size.X != value)
                {
                    var s = DataModel.Size;
                    s.X = value;
                    DataModel.Size = s;
                    OnPropertyChanged(nameof(SizeX));
                    MainViewModel.IsModified = true;
                }
            }
        }

        public float SizeY
        {
            get => DataModel.Size.Y;
            set
            {
                if (DataModel.Size.Y != value)
                {
                    var s = DataModel.Size;
                    s.Y = value;
                    DataModel.Size = s;
                    OnPropertyChanged(nameof(SizeY));
                    MainViewModel.IsModified = true;
                }
            }
        }

        public float SizeZ
        {
            get => DataModel.Size.Z;
            set
            {
                if (DataModel.Size.Z != value)
                {
                    var s = DataModel.Size;
                    s.Z = value;
                    DataModel.Size = s;
                    OnPropertyChanged(nameof(SizeZ));
                    MainViewModel.IsModified = true;
                }
            }
        }

        public float ColorR => DataModel.ModelColor.X;
        public float ColorG => DataModel.ModelColor.Y;
        public float ColorB => DataModel.ModelColor.Z;
        #endregion


        #region Methods
        private void RaiseAllowedActionsProperties()
        {
            OnPropertyChanged(nameof(IsDamageAllowed));
            OnPropertyChanged(nameof(IsShootingAllowed));
            OnPropertyChanged(nameof(IsDrillingAllowed));
            OnPropertyChanged(nameof(IsWeldingAllowed));
            OnPropertyChanged(nameof(IsGrindingAllowed));
            OnPropertyChanged(nameof(IsBuildingAllowed));
            OnPropertyChanged(nameof(IsBuildingProjectionsAllowed));
            OnPropertyChanged(nameof(IsVoxelHandAllowed));
            OnPropertyChanged(nameof(IsLandingGearLockAllowed));
            OnPropertyChanged(nameof(IsConvertToStationAllowed));
        }
        private void SetAllowedActionFlag(MySafeZoneAction flag, bool value, string propertyName)
        {
            if (value)
                AllowedActions |= flag;
            else
                AllowedActions &= ~flag;

            OnPropertyChanged(propertyName);
            MainViewModel.IsModified = true;
        }

        private void SearchCreatingEntitiesExecuted()
        {
            DataModel.FindCreatingEntities();
            OnPropertyChanged(nameof(CreatingBlock));
            OnPropertyChanged(nameof(CreatingGrid));

        }
        private bool SearchCreatingEntitiesCanExecute() => true;
        #endregion
    }
}

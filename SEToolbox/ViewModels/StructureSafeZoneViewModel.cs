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
        #region ctor

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

        #region AllowedActions
        public MySafeZoneAction AllowedActions
        {
            get => DataModel.AllowedActions;
            set
            {
                if (DataModel.AllowedActions != value)
                {
                    DataModel.AllowedActions = value;
                    OnPropertyChanged(nameof(AllowedActions));
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
                    MainViewModel.IsModified = true;
                }
            }
        }
        public bool IsDamageAllowed
        {
            get => AllowedActions.HasFlag(MySafeZoneAction.Damage);
            set
            {
                if (value)
                    AllowedActions |= MySafeZoneAction.Damage;
                else
                    AllowedActions &= ~MySafeZoneAction.Damage;

                OnPropertyChanged(nameof(IsDamageAllowed));
                MainViewModel.IsModified = true;
            }
        }

        public bool IsShootingAllowed
        {
            get => AllowedActions.HasFlag(MySafeZoneAction.Shooting);
            set
            {
                if (value)
                    AllowedActions |= MySafeZoneAction.Shooting;
                else
                    AllowedActions &= ~MySafeZoneAction.Shooting;

                OnPropertyChanged(nameof(IsShootingAllowed));
                MainViewModel.IsModified = true;
            }
        }

        public bool IsDrillingAllowed
        {
            get => AllowedActions.HasFlag(MySafeZoneAction.Drilling);
            set
            {
                if (value)
                    AllowedActions |= MySafeZoneAction.Drilling;
                else
                    AllowedActions &= ~MySafeZoneAction.Drilling;

                OnPropertyChanged(nameof(IsDrillingAllowed));
                MainViewModel.IsModified = true;
            }
        }

        public bool IsWeldingAllowed
        {
            get => AllowedActions.HasFlag(MySafeZoneAction.Welding);
            set
            {
                if (value)
                    AllowedActions |= MySafeZoneAction.Welding;
                else
                    AllowedActions &= ~MySafeZoneAction.Welding;

                OnPropertyChanged(nameof(IsWeldingAllowed));
                MainViewModel.IsModified = true;
            }
        }

        public bool IsGrindingAllowed
        {
            get => AllowedActions.HasFlag(MySafeZoneAction.Grinding);
            set
            {
                if (value)
                    AllowedActions |= MySafeZoneAction.Grinding;
                else
                    AllowedActions &= ~MySafeZoneAction.Grinding;

                OnPropertyChanged(nameof(IsGrindingAllowed));
                MainViewModel.IsModified = true;
            }
        }

        public bool IsBuildingAllowed
        {
            get => AllowedActions.HasFlag(MySafeZoneAction.Building);
            set
            {
                if (value)
                    AllowedActions |= MySafeZoneAction.Building;
                else
                    AllowedActions &= ~MySafeZoneAction.Building;

                OnPropertyChanged(nameof(IsBuildingAllowed));
                MainViewModel.IsModified = true;
            }
        }

        public bool IsBuildingProjectionsAllowed
        {
            get => AllowedActions.HasFlag(MySafeZoneAction.BuildingProjections);
            set
            {
                if (value)
                    AllowedActions |= MySafeZoneAction.BuildingProjections;
                else
                    AllowedActions &= ~MySafeZoneAction.BuildingProjections;

                OnPropertyChanged(nameof(IsBuildingProjectionsAllowed));
                MainViewModel.IsModified = true;
            }
        }

        public bool IsVoxelHandAllowed
        {
            get => AllowedActions.HasFlag(MySafeZoneAction.VoxelHand);
            set
            {
                if (value)
                    AllowedActions |= MySafeZoneAction.VoxelHand;
                else
                    AllowedActions &= ~MySafeZoneAction.VoxelHand;

                OnPropertyChanged(nameof(IsVoxelHandAllowed));
                MainViewModel.IsModified = true;
            }
        }

        public bool IsLandingGearLockAllowed
        {
            get => AllowedActions.HasFlag(MySafeZoneAction.LandingGearLock);
            set
            {
                if (value)
                    AllowedActions |= MySafeZoneAction.LandingGearLock;
                else
                    AllowedActions &= ~MySafeZoneAction.LandingGearLock;

                OnPropertyChanged(nameof(IsLandingGearLockAllowed));
                MainViewModel.IsModified = true;
            }
        }

        public bool IsConvertToStationAllowed
        {
            get => AllowedActions.HasFlag(MySafeZoneAction.ConvertToStation);
            set
            {
                if (value)
                    AllowedActions |= MySafeZoneAction.ConvertToStation;
                else
                    AllowedActions &= ~MySafeZoneAction.ConvertToStation;

                OnPropertyChanged(nameof(IsConvertToStationAllowed));
                MainViewModel.IsModified = true;
            }
        }

        #endregion

        #region "fields"
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

        protected new StructureSafeZoneModel DataModel => base.DataModel as StructureSafeZoneModel;


        public bool IsVisible                       //Done
        {
            get { return DataModel.IsVisible; }

            set
            {
                DataModel.IsVisible = value;
                MainViewModel.IsModified = true;
            }
        }

        public bool IsEnabled                       //Done
        {
            get { return DataModel.IsEnabled; }

            set
            {
                DataModel.IsEnabled = value;
                MainViewModel.IsModified = true;
            }
        }

        public float Radius                         //Done
        {
            get { return DataModel.Radius; }
            set { 
                DataModel.Radius = value;
                MainViewModel.IsModified = true;
            }
        }
        
        public MySafeZoneShape Shape                //Done
        {
            get => DataModel.ShapeEnum;
            set
            {
                if (value != DataModel.ShapeEnum)
                {
                    DataModel.ShapeEnum = value;
                    OnPropertyChanged(nameof(Shape));
                    //IsSphereShape = value == MySafeZoneShape.Sphere;
                    OnPropertyChanged(nameof(IsSphereShape));
                    MainViewModel.IsModified = true;
                }
            }
        }

        public string Texture                       //Done (Find more textures)
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

        public MySafeZoneAccess AccessTypePlayers             //Done?
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

        public MySafeZoneAccess AccessTypeFactions            //Done?
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

        public MySafeZoneAccess AccessTypeGrids               //Done?
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

        public MySafeZoneAccess AccessTypeFloatingObjects     //Done?
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


        public MyObjectBuilder_CubeBlock CreatingBlock => DataModel.CreatingBlock;
        public MyObjectBuilder_CubeGrid CreatingGrid => DataModel.CreatingGrid;

        public string CreatingBlockInfo
        {
            get
            {
                MyObjectBuilder_SafeZoneBlock safeZoneBlock = CreatingBlock as MyObjectBuilder_SafeZoneBlock;
                if (safeZoneBlock == null)
                    return "(no Block found)";

                var name = safeZoneBlock.CustomName
                        ?? safeZoneBlock.Name
                        ?? "<no Name>";

                var type = CreatingBlock.GetType().Name;
                var subtype = CreatingBlock.SubtypeName ?? "";
                return $"{name} (Type: {type}{(string.IsNullOrWhiteSpace(subtype) ? "" : $", Subtype: {subtype}")}) [ID: {CreatingBlock.EntityId}]";
            }
        }
        public string CreatingGridInfo
        {
            get
            {
                if (CreatingGrid == null)
                    return "(no Grid found)";

                // Name versuchen, falls vorhanden (CustomName, Name, DisplayName)
                var name = CreatingGrid.DisplayName
                        ?? CreatingGrid.Name
                        ?? "<no name>";

                var id = CreatingGrid.EntityId;
                var subtype = CreatingGrid.SubtypeName ?? "";
                if (!string.IsNullOrWhiteSpace(subtype))
                    return $"{name} (Typ: {subtype}) [ID: {id}]";
                else
                    return $"{name} [ID: {id}]";
            }
        }
        private void SearchCreatingEntitiesExecuted()
        {
            DataModel.FindCreatingEntities(); //TODO: Pass the actual entity collection
            OnPropertyChanged(nameof(CreatingBlock));
            OnPropertyChanged(nameof(CreatingGrid));
            MainViewModel.IsModified = true; // Not needed here, as this is just a search operation
        }
        public bool SearchCreatingEntitiesCanExecute()
        {
            return true;
        }
        #endregion

        #region command properties
        private ICommand _searchCreatingEntitiesCommand;
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

        #region Size and Color Properties
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
    }
}

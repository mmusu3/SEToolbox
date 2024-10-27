namespace SEToolbox.ViewModels
{
    using SEToolbox.Interop;
    using SEToolbox.Interop.Asteroids;
    using SEToolbox.Models;
    using SEToolbox.Services;
    using SEToolbox.Support;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Text;
    using System.Windows;
    using System.Windows.Input;
    using VRage;
    using VRage.Game;
    using VRage.ObjectBuilders;
    using IDType = VRage.MyEntityIdentifier.ID_OBJECT_TYPE;

    public class StructureVoxelViewModel : StructureBaseViewModel<StructureVoxelModel>
    {
        #region ctor

        public StructureVoxelViewModel(BaseViewModel parentViewModel, StructureVoxelModel dataModel)
            : base(parentViewModel, dataModel)
        {
            DataModel.PropertyChanged += delegate (object sender, PropertyChangedEventArgs e)
            {
                // Will bubble property change events from the Model to the ViewModel.
                OnPropertyChanged(e.PropertyName);
            };
        }

        #endregion

        #region command properties

        public ICommand CopyDetailCommand
        {
            get { return new DelegateCommand(CopyDetailExecuted, CopyDetailCanExecute); }
        }

        public ICommand ReseedCommand
        {
            get { return new DelegateCommand(ReseedExecuted, ReseedCanExecute); }
        }

        public ICommand ReplaceSurfaceCommand
        {
            get { return new DelegateCommand<string>(ReplaceSurfaceExecuted, ReplaceSurfaceCanExecute); }
        }

        public ICommand ReplaceAllCommand
        {
            get { return new DelegateCommand<string>(ReplaceAllExecuted, ReplaceAllCanExecute); }
        }

        public ICommand ReplaceSelectedMenuCommand
        {
            get { return new DelegateCommand<string>(new Func<string, bool>(ReplaceSelectedMenuCanExecute)); }
        }

        public ICommand ReplaceSelectedCommand
        {
            get { return new DelegateCommand<string>(ReplaceSelectedExecuted, ReplaceSelectedCanExecute); }
        }

        public ICommand SliceQuarterCommand
        {
            get { return new DelegateCommand(SliceQuarterExecuted, SliceQuarterCanExecute); }
        }

        public ICommand SliceHalfCommand
        {
            get { return new DelegateCommand(SliceHalfExecuted, SliceHalfCanExecute); }
        }

        public ICommand ExtractStationIntersectLooseCommand
        {
            get { return new DelegateCommand(ExtractStationIntersectLooseExecuted, ExtractStationIntersectLooseCanExecute); }
        }

        public ICommand ExtractStationIntersectTightCommand
        {
            get { return new DelegateCommand(ExtractStationIntersectTightExecuted, ExtractStationIntersectTightCanExecute); }
        }

        public ICommand RotateAsteroidYawPositiveCommand
        {
            get { return new DelegateCommand(RotateAsteroidYawPositiveExecuted, RotateAsteroidYawPositiveCanExecute); }
        }

        public ICommand RotateAsteroidYawNegativeCommand
        {
            get { return new DelegateCommand(RotateAsteroidYawNegativeExecuted, RotateAsteroidYawNegativeCanExecute); }
        }

        public ICommand RotateAsteroidPitchPositiveCommand
        {
            get { return new DelegateCommand(RotateAsteroidPitchPositiveExecuted, RotateAsteroidPitchPositiveCanExecute); }
        }

        public ICommand RotateAsteroidPitchNegativeCommand
        {
            get { return new DelegateCommand(RotateAsteroidPitchNegativeExecuted, RotateAsteroidPitchNegativeCanExecute); }
        }

        public ICommand RotateAsteroidRollPositiveCommand
        {
            get { return new DelegateCommand(RotateAsteroidRollPositiveExecuted, RotateAsteroidRollPositiveCanExecute); }
        }

        public ICommand RotateAsteroidRollNegativeCommand
        {
            get { return new DelegateCommand(RotateAsteroidRollNegativeExecuted, RotateAsteroidRollNegativeCanExecute); }
        }

        #endregion

        #region Properties

        protected new StructureVoxelModel DataModel
        {
            get { return base.DataModel as StructureVoxelModel; }
        }

        public string Name
        {
            get { return DataModel.Name; }
            set { DataModel.Name = value; }
        }

        public BindableSize3DIModel Size
        {
            get { return new BindableSize3DIModel(DataModel.Size); }
            set { DataModel.Size = value.ToVector3I(); }
        }

        public BindableSize3DIModel ContentSize
        {
            get { return new BindableSize3DIModel(DataModel.ContentSize); }
        }

        public BindableVector3DModel Center
        {
            get { return new BindableVector3DModel(DataModel.Center); }
            set { DataModel.Center = value.ToVector3(); }
        }

        public long VoxCells
        {
            get { return DataModel.VoxCells; }
            set { DataModel.VoxCells = value; }
        }

        public double Volume
        {
            get { return DataModel.Volume; }
        }

        public List<VoxelMaterialAssetModel> MaterialAssets
        {
            get { return DataModel.MaterialAssets; }
            set { DataModel.MaterialAssets = value; }
        }

        public VoxelMaterialAssetModel SelectedMaterialAsset
        {
            get { return DataModel.SelectedMaterialAsset; }
            set { DataModel.SelectedMaterialAsset = value; }
        }

        public List<VoxelMaterialAssetModel> GameMaterialList
        {
            get { return DataModel.GameMaterialList; }
            set { DataModel.GameMaterialList = value; }
        }

        public List<VoxelMaterialAssetModel> EditMaterialList
        {
            get { return DataModel.EditMaterialList; }
            set { DataModel.EditMaterialList = value; }
        }

        #endregion

        #region methods

        public bool CopyDetailCanExecute()
        {
            return true;
        }

        public void CopyDetailExecuted()
        {
            var ore = new StringBuilder();

            if (MaterialAssets != null)
            {
                foreach (var mat in MaterialAssets)
                {
                    ore.AppendFormat("{0}\t{1:#,##0.00} m³\t{2:P2}\r\n", mat.MaterialName, mat.Volume, mat.Percent);
                }
            }

            var detail = string.Format(Properties.Resources.CtlVoxelDetail,
                Name,
                Size.Width, Size.Height, Size.Depth,
                ContentSize.Width, ContentSize.Height, ContentSize.Depth,
                Center.X, Center.Y, Center.Z,
                Volume,
                VoxCells,
                PlayerDistance,
                PositionAndOrientation.Value.Position.X, PositionAndOrientation.Value.Position.Y, PositionAndOrientation.Value.Position.Z,
                ore);

            try
            {
                Clipboard.Clear();
                Clipboard.SetText(detail);
            }
            catch
            {
                // Ignore exception which may be generated by a Remote desktop session where Clipboard access has not been granted.
            }
        }

        public bool ReseedCanExecute()
        {
            return DataModel.IsValid;
        }

        public void ReseedExecuted()
        {
            MainViewModel.IsBusy = true;

            var sourceFile = DataModel.SourceVoxelFilepath ?? DataModel.VoxelFilepath;

            var asteroid = new MyVoxelMap();
            asteroid.Load(sourceFile);

            long cellCount = asteroid.VoxCells;

            // TODO: regenerate the materials inside of the asteroid randomly.

            var tempfilename = TempfileUtil.NewFilename(MyVoxelMap.V2FileExtension);
            asteroid.Save(tempfilename);

            DataModel.UpdateNewSource(asteroid, tempfilename);

            MainViewModel.IsModified = true;
            MainViewModel.IsBusy = false;

            DataModel.MaterialAssets = null;
            DataModel.InitializeAsync();
        }

        public bool ReplaceSurfaceCanExecute(string materialName)
        {
            return DataModel.IsValid;
        }

        public void ReplaceSurfaceExecuted(string materialName)
        {
            MainViewModel.IsBusy = true;

            var sourceFile = DataModel.SourceVoxelFilepath ?? DataModel.VoxelFilepath;

            var asteroid = new MyVoxelMap();
            asteroid.Load(sourceFile);
            asteroid.ForceShellMaterial(materialName, 2);

            var tempfilename = TempfileUtil.NewFilename(MyVoxelMap.V2FileExtension);
            asteroid.Save(tempfilename);

            DataModel.UpdateNewSource(asteroid, tempfilename);

            MainViewModel.IsModified = true;
            MainViewModel.IsBusy = false;

            DataModel.MaterialAssets = null;
            DataModel.InitializeAsync();
        }

        public bool ReplaceAllCanExecute(string materialName)
        {
            return DataModel.IsValid;
        }

        public void ReplaceAllExecuted(string materialName)
        {
            MainViewModel.IsBusy = true;

            var sourceFile = DataModel.SourceVoxelFilepath ?? DataModel.VoxelFilepath;

            var asteroid = new MyVoxelMap();
            asteroid.Load(sourceFile);
            asteroid.ForceBaseMaterial(materialName, materialName);

            var tempfilename = TempfileUtil.NewFilename(MyVoxelMap.V2FileExtension);
            asteroid.Save(tempfilename);

            DataModel.UpdateNewSource(asteroid, tempfilename);

            MainViewModel.IsModified = true;
            MainViewModel.IsBusy = false;

            DataModel.MaterialAssets = null;
            DataModel.InitializeAsync();
        }

        public bool ReplaceSelectedMenuCanExecute(string materialName)
        {
            return DataModel.IsValid && SelectedMaterialAsset != null;
        }

        public bool ReplaceSelectedCanExecute(string materialName)
        {
            return DataModel.IsValid && SelectedMaterialAsset != null;
        }

        public void ReplaceSelectedExecuted(string materialName)
        {
            MainViewModel.IsBusy = true;

            var sourceFile = DataModel.SourceVoxelFilepath ?? DataModel.VoxelFilepath;

            var asteroid = new MyVoxelMap();
            asteroid.Load(sourceFile);

            if (string.IsNullOrEmpty(materialName))
            {
                asteroid.RemoveContent(SelectedMaterialAsset.MaterialName, null);
                DataModel.VoxCells = asteroid.VoxCells;
            }
            else
            {
                asteroid.ReplaceMaterial(SelectedMaterialAsset.MaterialName, materialName);
            }

            var tempfilename = TempfileUtil.NewFilename(MyVoxelMap.V2FileExtension);
            asteroid.Save(tempfilename);

            DataModel.UpdateNewSource(asteroid, tempfilename);

            MainViewModel.IsModified = true;
            MainViewModel.IsBusy = false;

            DataModel.UpdateGeneralFromEntityBase();
            DataModel.MaterialAssets = null;
            DataModel.InitializeAsync();
        }

        public bool SliceQuarterCanExecute()
        {
            return DataModel.IsValid;
        }

        public void SliceQuarterExecuted()
        {
            MainViewModel.IsBusy = true;

            var sourceFile = DataModel.SourceVoxelFilepath ?? DataModel.VoxelFilepath;

            var asteroid = new MyVoxelMap();
            asteroid.Load(sourceFile);
            asteroid.RefreshAssets();

            int height = asteroid.BoundingContent.Size.Y + 1;
            var contentCenter = asteroid.ContentCenter;
            var asteroidSize = asteroid.Size;
            var min = (VRageMath.Vector3I)VRageMath.Vector3D.Round(contentCenter, 0);

            // remove the Top half.
            asteroid.RemoveMaterial(min.X, asteroidSize.X, min.Y, asteroidSize.Y, 0, min.Z);

            var tempfilename = TempfileUtil.NewFilename(MyVoxelMap.V2FileExtension);
            asteroid.Save(tempfilename);

            var newFilename = MainViewModel.CreateUniqueVoxelStorageName(DataModel.Name);
            var posOrient = DataModel.PositionAndOrientation.HasValue ? DataModel.PositionAndOrientation.Value : new MyPositionAndOrientation();
            posOrient.Position.y += height;

            // genreate a new Asteroid entry.
            var newEntity = new MyObjectBuilder_VoxelMap {
                EntityId = SpaceEngineersApi.GenerateEntityId(IDType.ASTEROID),
                PersistentFlags = MyPersistentEntityFlags2.CastShadows | MyPersistentEntityFlags2.InScene,
                StorageName = Path.GetFileNameWithoutExtension(newFilename),
                PositionAndOrientation = new MyPositionAndOrientation {
                    Position = posOrient.Position,
                    Forward = posOrient.Forward,
                    Up = posOrient.Up
                }
            };

            var structure = MainViewModel.AddEntity(newEntity);
            ((StructureVoxelModel)structure).UpdateNewSource(asteroid, tempfilename); // Set the temporary file location of the Source Voxel, as it hasn't been written yet.

            MainViewModel.IsModified = true;
            MainViewModel.IsBusy = false;
        }

        public bool SliceHalfCanExecute()
        {
            return DataModel.IsValid;
        }

        public void SliceHalfExecuted()
        {
            MainViewModel.IsBusy = true;

            var sourceFile = DataModel.SourceVoxelFilepath ?? DataModel.VoxelFilepath;

            var asteroid = new MyVoxelMap();
            asteroid.Load(sourceFile);
            asteroid.RefreshAssets();

            int height = asteroid.BoundingContent.Size.Y + 1;

            // remove the Top half.
            asteroid.RemoveMaterial(null, null, (int)Math.Round(asteroid.ContentCenter.Y, 0), asteroid.Size.Y, null, null);

            var tempfilename = TempfileUtil.NewFilename(MyVoxelMap.V2FileExtension);
            asteroid.Save(tempfilename);

            var newFilename = MainViewModel.CreateUniqueVoxelStorageName(DataModel.Name);
            var posOrient = DataModel.PositionAndOrientation.HasValue ? DataModel.PositionAndOrientation.Value : new MyPositionAndOrientation();
            posOrient.Position.y += height;

            // genreate a new Asteroid entry.
            var newEntity = new MyObjectBuilder_VoxelMap {
                EntityId = SpaceEngineersApi.GenerateEntityId(IDType.ASTEROID),
                PersistentFlags = MyPersistentEntityFlags2.CastShadows | MyPersistentEntityFlags2.InScene,
                StorageName = Path.GetFileNameWithoutExtension(newFilename),
                PositionAndOrientation = new MyPositionAndOrientation {
                    Position = posOrient.Position,
                    Forward = posOrient.Forward,
                    Up = posOrient.Up
                }
            };

            var structure = MainViewModel.AddEntity(newEntity);
            ((StructureVoxelModel)structure).UpdateNewSource(asteroid, tempfilename); // Set the temporary file location of the Source Voxel, as it hasn't been written yet.

            MainViewModel.IsModified = true;
            MainViewModel.IsBusy = false;
        }

        public bool ExtractStationIntersectLooseCanExecute()
        {
            return DataModel.IsValid;
        }

        public void ExtractStationIntersectLooseExecuted()
        {
            MainViewModel.IsBusy = true;
            var modified = ExtractStationIntersect(false);
            if (modified)
            {
                DataModel.InitializeAsync();
                MainViewModel.IsModified = true;
            }
            MainViewModel.IsBusy = false;
        }

        public bool ExtractStationIntersectTightCanExecute()
        {
            return DataModel.IsValid;
        }

        public void ExtractStationIntersectTightExecuted()
        {
            MainViewModel.IsBusy = true;
            var modified = ExtractStationIntersect(true);
            if (modified)
            {
                DataModel.InitializeAsync();
                MainViewModel.IsModified = true;
            }
            MainViewModel.IsBusy = false;
        }

        public bool RotateAsteroidYawPositiveCanExecute()
        {
            return DataModel.IsValid;
        }

        public void RotateAsteroidPitchPositiveExecuted()
        {
            MainViewModel.IsBusy = true;
            // +90 around X
            MaterialAssets = null;
            DataModel.RotateAsteroid(VRageMath.Quaternion.CreateFromYawPitchRoll(0, VRageMath.MathHelper.PiOver2, 0));
            DataModel.InitializeAsync();
            MainViewModel.IsModified = true;
            MainViewModel.IsBusy = false;
        }

        public bool RotateAsteroidPitchNegativeCanExecute()
        {
            return DataModel.IsValid;
        }

        public void RotateAsteroidPitchNegativeExecuted()
        {
            MainViewModel.IsBusy = true;
            // -90 around X
            DataModel.RotateAsteroid(VRageMath.Quaternion.CreateFromYawPitchRoll(0, -VRageMath.MathHelper.PiOver2, 0));
            DataModel.InitializeAsync();
            MainViewModel.IsModified = true;
            MainViewModel.IsBusy = false;
        }

        public bool RotateAsteroidRollPositiveCanExecute()
        {
            return DataModel.IsValid;
        }

        public void RotateAsteroidYawPositiveExecuted()
        {
            MainViewModel.IsBusy = true;
            // +90 around Y
            DataModel.RotateAsteroid(VRageMath.Quaternion.CreateFromYawPitchRoll(VRageMath.MathHelper.PiOver2, 0, 0));
            DataModel.InitializeAsync();
            MainViewModel.IsModified = true;
            MainViewModel.IsBusy = false;
        }

        public bool RotateAsteroidYawNegativeCanExecute()
        {
            return DataModel.IsValid;
        }

        public void RotateAsteroidYawNegativeExecuted()
        {
            MainViewModel.IsBusy = true;
            // -90 around Y
            DataModel.RotateAsteroid(VRageMath.Quaternion.CreateFromYawPitchRoll(-VRageMath.MathHelper.PiOver2, 0, 0));
            DataModel.InitializeAsync();
            MainViewModel.IsModified = true;
            MainViewModel.IsBusy = false;
        }

        public bool RotateAsteroidPitchPositiveCanExecute()
        {
            return DataModel.IsValid;
        }

        public void RotateAsteroidRollPositiveExecuted()
        {
            MainViewModel.IsBusy = true;
            // +90 around Z
            DataModel.RotateAsteroid(VRageMath.Quaternion.CreateFromYawPitchRoll(0, 0, VRageMath.MathHelper.PiOver2));
            DataModel.InitializeAsync();
            MainViewModel.IsModified = true;
            MainViewModel.IsBusy = false;
        }

        public bool RotateAsteroidRollNegativeCanExecute()
        {
            return DataModel.IsValid;
        }

        public void RotateAsteroidRollNegativeExecuted()
        {
            MainViewModel.IsBusy = true;
            // -90 around Z
            DataModel.RotateAsteroid(VRageMath.Quaternion.CreateFromYawPitchRoll(0, 0, -VRageMath.MathHelper.PiOver2));
            DataModel.InitializeAsync();
            MainViewModel.IsModified = true;
            MainViewModel.IsBusy = false;
        }

        private bool ExtractStationIntersect(bool tightIntersection)
        {
            return DataModel.ExtractStationIntersect(MainViewModel, tightIntersection);
        }

        #endregion
    }
}

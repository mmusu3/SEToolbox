namespace SEToolbox.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics.Contracts;
    using System.IO;
    using System.Windows.Forms;
    using System.Windows.Input;
    using System.Windows.Media.Media3D;

    using SEToolbox.Interfaces;
    using SEToolbox.Interop;
    using SEToolbox.Interop.Asteroids;
    using SEToolbox.Models;
    using SEToolbox.Services;
    using SEToolbox.Support;
    using VRageMath;
    using Res = SEToolbox.Properties.Resources;
    using IDType = VRage.MyEntityIdentifier.ID_OBJECT_TYPE;
    using VRage.ObjectBuilders;
    using VRage;
    using VRage.Game;

    public class ImportVoxelViewModel : BaseViewModel
    {
        #region Fields

        private readonly IDialogService _dialogService;
        private readonly Func<IOpenFileDialog> _openFileDialogFactory;
        private readonly ImportVoxelModel _dataModel;

        private bool? _closeResult;
        private bool _isBusy;

        #endregion

        #region Constructors

        public ImportVoxelViewModel(BaseViewModel parentViewModel, ImportVoxelModel dataModel)
            : this(parentViewModel, dataModel, ServiceLocator.Resolve<IDialogService>(), ServiceLocator.Resolve<IOpenFileDialog>)
        {
        }

        public ImportVoxelViewModel(BaseViewModel parentViewModel, ImportVoxelModel dataModel, IDialogService dialogService, Func<IOpenFileDialog> openFileDialogFactory)
            : base(parentViewModel)
        {
            Contract.Requires(dialogService != null);
            Contract.Requires(openFileDialogFactory != null);

            _dialogService = dialogService;
            _openFileDialogFactory = openFileDialogFactory;
            _dataModel = dataModel;
            // Will bubble property change events from the Model to the ViewModel.
            _dataModel.PropertyChanged += (sender, e) => OnPropertyChanged(e.PropertyName);
        }

        #endregion

        #region Properties

        public ICommand BrowseVoxelCommand => new DelegateCommand(BrowseVoxelExecuted, BrowseVoxelCanExecute);

        public ICommand CreateCommand => new DelegateCommand(CreateExecuted, CreateCanExecute);

        public ICommand CancelCommand => new DelegateCommand(CancelExecuted, CancelCanExecute);

        /// <summary>
        /// Gets or sets the DialogResult of the View.  If True or False is passed, this initiates the Close().
        /// </summary>
        public bool? CloseResult
        {
            get { return _closeResult; }

            set
            {
                _closeResult = value;
                OnPropertyChanged(nameof(CloseResult));
            }
        }

        public string Filename
        {
            get { return _dataModel.Filename; }
            set { _dataModel.Filename = value; }
        }

        public string SourceFile
        {
            get { return _dataModel.SourceFile; }
            set
            {
                _dataModel.SourceFile = value;
                SourceFileChanged();
            }
        }

        public bool IsValidVoxelFile
        {
            get { return _dataModel.IsValidVoxelFile; }
            set { _dataModel.IsValidVoxelFile = value; }
        }

        public BindablePoint3DModel Position
        {
            get { return _dataModel.Position; }
            set { _dataModel.Position = value; }
        }

        public BindableVector3DModel Forward
        {
            get { return _dataModel.Forward; }
            set { _dataModel.Forward = value; }
        }

        public BindableVector3DModel Up
        {
            get { return _dataModel.Up; }
            set { _dataModel.Up = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the View is currently in the middle of an asynchonise operation.
        /// </summary>
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
                        Application.DoEvents();
                    }
                }
            }
        }

        public bool IsStockVoxel
        {
            get { return _dataModel.IsStockVoxel; }
            set { _dataModel.IsStockVoxel = value; }
        }

        public bool IsFileVoxel
        {
            get { return _dataModel.IsFileVoxel; }
            set { _dataModel.IsFileVoxel = value; }
        }

        public bool IsSphere
        {
            get { return _dataModel.IsSphere; }
            set { _dataModel.IsSphere = value; }
        }

        public GenerateVoxelDetailModel StockVoxel
        {
            get { return _dataModel.StockVoxel; }
            set { _dataModel.StockVoxel = value; }
        }

        public List<GenerateVoxelDetailModel> VoxelFileList
        {
            get { return _dataModel.VoxelFileList; }
        }

        public ObservableCollection<MaterialSelectionModel> MaterialsCollection
        {
            get { return _dataModel.MaterialsCollection; }
        }

        public MaterialSelectionModel StockMaterial
        {
            get { return _dataModel.StockMaterial; }
            set { _dataModel.StockMaterial = value; }
        }

        public int SphereRadius
        {
            get { return _dataModel.SphereRadius; }
            set { _dataModel.SphereRadius = value; }
        }

        public int SphereShellRadius
        {
            get { return _dataModel.SphereShellRadius; }
            set { _dataModel.SphereShellRadius = value; }
        }

        #endregion

        #region command methods

        public bool BrowseVoxelCanExecute()
        {
            return true;
        }

        public void BrowseVoxelExecuted()
        {
            BrowseVoxel();
        }

        public bool CreateCanExecute()
        {
            return (IsValidVoxelFile && IsFileVoxel) 
                || (IsStockVoxel && StockVoxel != null)
                || (IsSphere && SphereRadius > 0);
        }

        public void CreateExecuted()
        {
            CloseResult = true;
        }

        public bool CancelCanExecute()
        {
            return true;
        }

        public void CancelExecuted()
        {
            CloseResult = false;
        }

        #endregion

        #region helpers

        private void BrowseVoxel()
        {
            IsValidVoxelFile = false;
            var openFileDialog = _openFileDialogFactory();
            openFileDialog.Filter = AppConstants.VoxelAnyFilter;
            openFileDialog.Title = Res.DialogImportVoxelTitle;

            // Open the dialog
            var result = _dialogService.ShowOpenFileDialog(OwnerViewModel, openFileDialog);

            if (result == DialogResult.OK)
            {
                SourceFile = openFileDialog.FileName;
            }
        }

        private void SourceFileChanged()
        {
            ProcessSourceFile(SourceFile);
        }

        private void ProcessSourceFile(string filename)
        {
            IsBusy = true;

            if (File.Exists(filename))
            {
                IsValidVoxelFile = MyVoxelMap.IsVoxelMapFile(filename);
                IsFileVoxel = true;
            }
            else
            {
                IsValidVoxelFile = false;
                IsFileVoxel = false;
            }

            IsBusy = false;
        }

        public MyObjectBuilder_EntityBase BuildEntity()
        {
            var asteroidCenter = new VRageMath.Vector3D();
            var asteroidSize = new Vector3I();

            string originalFile = null;
            if (IsStockVoxel)
            {
                var stockfile = StockVoxel.SourceFilename;

                if (StockMaterial == null || StockMaterial.Value == null)
                {
                    SourceFile = stockfile;
                    originalFile = SourceFile;

                    using var asteroid = new MyVoxelMap();
                    asteroid.Load(stockfile);
                    asteroidCenter = asteroid.BoundingContent.Center;
                    asteroidSize = asteroid.BoundingContent.Size + 1; // Content size
                }
                else
                {
                    using var asteroid = new MyVoxelMap();
                    asteroid.Load(stockfile);
                    asteroid.ForceBaseMaterial(SpaceEngineersCore.Resources.GetDefaultMaterialName(), StockMaterial.Value);
                    SourceFile = TempfileUtil.NewFilename(MyVoxelMap.V2FileExtension);
                    asteroid.Save(SourceFile);

                    originalFile = StockVoxel.SourceFilename;
                    asteroidCenter = asteroid.BoundingContent.Center;
                    asteroidSize = asteroid.BoundingContent.Size + 1; // Content size
                }
            }
            else if (IsFileVoxel)
            {
                originalFile = SourceFile;

                using var asteroid = new MyVoxelMap();
                asteroid.Load(SourceFile);
                asteroidCenter = asteroid.BoundingContent.Center;
                asteroidSize = asteroid.BoundingContent.Size + 1; // Content size

                if (StockMaterial != null && StockMaterial.Value != null)
                {
                    asteroid.ForceBaseMaterial(SpaceEngineersCore.Resources.GetDefaultMaterialName(), StockMaterial.Value);
                    SourceFile = TempfileUtil.NewFilename(MyVoxelMap.V2FileExtension);
                    asteroid.Save(SourceFile);
                }
            }
            else if (IsSphere)
            {
                byte materialIndex;
                if (StockMaterial?.MaterialIndex != null)
                    materialIndex = StockMaterial.MaterialIndex.Value;
                else
                    materialIndex = SpaceEngineersCore.Resources.GetDefaultMaterialIndex();

                string materialName = SpaceEngineersCore.Resources.GetMaterialName(materialIndex);

                originalFile = string.Format("sphere_{0}_{1}_{2}{3}", materialName.ToLowerInvariant(), SphereRadius, SphereShellRadius, MyVoxelMap.V2FileExtension);

                using var asteroid = MyVoxelBuilder.BuildAsteroidSphere(SphereRadius > 32, SphereRadius, materialIndex, materialIndex, SphereShellRadius != 0, SphereShellRadius);
                // TODO: progress bar.
                asteroidCenter = asteroid.BoundingContent.Center;
                asteroidSize = asteroid.BoundingContent.Size + 1; // Content size
                SourceFile = TempfileUtil.NewFilename(MyVoxelMap.V2FileExtension);
                asteroid.Save(SourceFile);
            }

            // automatically number all files, and check for duplicate filenames.
            Filename = MainViewModel.CreateUniqueVoxelStorageName(originalFile);

            // Figure out where the Character is facing, and plant the new constrcut right in front.
            // Calculate the hypotenuse, as it will be the safest distance to place in front.
            double distance = Math.Sqrt(Math.Pow(asteroidSize.X, 2) + Math.Pow(asteroidSize.Y, 2) + Math.Pow(asteroidSize.Z, 2)) / 2;

            var vector = new BindableVector3DModel(_dataModel.CharacterPosition.Forward).Vector3D;
            vector.Normalize();
            vector = System.Windows.Media.Media3D.Vector3D.Multiply(vector, distance);
            Position = new BindablePoint3DModel(Point3D.Add(new BindablePoint3DModel(_dataModel.CharacterPosition.Position).Point3D, vector));
            //Forward = new BindableVector3DModel(_dataModel.CharacterPosition.Forward);
            //Up = new BindableVector3DModel(_dataModel.CharacterPosition.Up);
            Forward = new BindableVector3DModel(Vector3.Forward);  // Asteroids currently don't have any orientation.
            Up = new BindableVector3DModel(Vector3.Up);

            var entity = new MyObjectBuilder_VoxelMap
            {
                EntityId = SpaceEngineersApi.GenerateEntityId(IDType.ASTEROID),
                PersistentFlags = MyPersistentEntityFlags2.CastShadows | MyPersistentEntityFlags2.InScene,
                StorageName = Path.GetFileNameWithoutExtension(Filename),
                PositionAndOrientation = new MyPositionAndOrientation
                {
                    Position = Position.ToVector3D() - asteroidCenter,
                    Forward = Forward.ToVector3(),
                    Up = Up.ToVector3()
                }
            };

            return entity;
        }

        #endregion
    }
}

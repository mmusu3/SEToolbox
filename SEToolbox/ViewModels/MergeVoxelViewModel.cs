namespace SEToolbox.ViewModels
{
    using SEToolbox.Interop;
    using SEToolbox.Interop.Asteroids;
    using SEToolbox.Models;
    using SEToolbox.Services;
    using SEToolbox.Support;
    using System.Collections.Generic;
    using System.IO;
    using System.Windows.Input;
    using VRage;
    using VRage.Game;
    using VRage.Library.Collections;
    using VRage.ObjectBuilders;
    using VRage.Voxels;
    using VRageMath;
    using IDType = VRage.MyEntityIdentifier.ID_OBJECT_TYPE;

    public class MergeVoxelViewModel : BaseViewModel
    {
        #region Fields

        private readonly MergeVoxelModel _dataModel;
        private bool? _closeResult;

        #endregion

        #region Constructors

        public MergeVoxelViewModel(BaseViewModel parentViewModel, MergeVoxelModel dataModel)
            : base(parentViewModel)
        {
            _dataModel = dataModel;

            // Will bubble property change events from the Model to the ViewModel.
            _dataModel.PropertyChanged += (sender, e) => OnPropertyChanged(e.PropertyName);

            MergeFileName = "merge";
        }

        #endregion

        #region command Properties

        public ICommand ApplyCommand
        {
            get { return new DelegateCommand(ApplyExecuted, ApplyCanExecute); }
        }

        public ICommand CancelCommand
        {
            get { return new DelegateCommand(CancelExecuted, CancelCanExecute); }
        }

        #endregion

        #region Properties

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

        /// <summary>
        /// Gets or sets a value indicating whether the View is currently in the middle of an asynchonise operation.
        /// </summary>
        public bool IsBusy
        {
            get { return _dataModel.IsBusy; }
            set { _dataModel.IsBusy = value; }
        }

        public bool IsValidMerge
        {
            get { return _dataModel.IsValidMerge; }
            set { _dataModel.IsValidMerge = value; }
        }

        public MyObjectBuilder_VoxelMap NewEntity { get; set; }

        public StructureVoxelModel SelectionLeft
        {
            get { return (StructureVoxelModel)_dataModel.SelectionLeft; }
            set { _dataModel.SelectionLeft = value; }
        }

        public StructureVoxelModel SelectionRight
        {
            get { return (StructureVoxelModel)_dataModel.SelectionRight; }
            set { _dataModel.SelectionRight = value; }
        }

        public string SourceFile
        {
            get { return _dataModel.SourceFile; }
            set { _dataModel.SourceFile = value; }
        }

        public VoxelMergeType VoxelMergeType
        {
            get { return _dataModel.VoxelMergeType; }
            set { _dataModel.VoxelMergeType = value; }
        }

        public string MergeFileName
        {
            get { return _dataModel.MergeFileName; }
            set { _dataModel.MergeFileName = value; }
        }

        public bool RemoveOriginalAsteroids
        {
            get { return _dataModel.RemoveOriginalAsteroids; }
            set { _dataModel.RemoveOriginalAsteroids = value; }
        }

        #endregion

        #region methods

        public bool ApplyCanExecute()
        {
            return IsValidMerge;
        }

        public void ApplyExecuted()
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

        public MyObjectBuilder_EntityBase BuildEntity()
        {
            // Realign both asteroids to a common grid, so voxels can be lined up.
            Vector3I roundedPosLeft = SelectionLeft.WorldAABB.Min.RoundToVector3I();
            Vector3D offsetPosLeft = SelectionLeft.WorldAABB.Min - (Vector3D)roundedPosLeft; // Use for everything.
            Vector3I roundedPosRight = (SelectionRight.WorldAABB.Min - offsetPosLeft).RoundToVector3I();
            Vector3D offsetPosRight = SelectionRight.WorldAABB.Min - (Vector3D)roundedPosRight; // Use for everything.

            // calculate smallest allowable size for contents of both.
            const int paddCells = 3;

            // Force a calculation of the ContentBounds, as multi select in the ListView doesn't necessarily make it happen, or make it happen fast enough.
            SelectionLeft.LoadDetailsSync();
            SelectionRight.LoadDetailsSync();

            var minLeft = SelectionLeft.WorldAABB.Min + SelectionLeft.InflatedContentBounds.Min - offsetPosLeft;
            var minRight = SelectionRight.WorldAABB.Min + SelectionRight.InflatedContentBounds.Min - offsetPosRight;
            var min = Vector3D.Zero;
            var posOffset = Vector3D.Zero;
            var asteroidSize = Vector3I.Zero;

            switch (VoxelMergeType)
            {
                case VoxelMergeType.UnionVolumeLeftToRight:
                case VoxelMergeType.UnionVolumeRightToLeft:
                    min = Vector3D.Min(minLeft, minRight) - paddCells;
                    var max = Vector3D.Max(SelectionLeft.WorldAABB.Min + SelectionLeft.InflatedContentBounds.Max - offsetPosLeft, SelectionRight.WorldAABB.Min + SelectionRight.InflatedContentBounds.Max - offsetPosRight) + paddCells;
                    posOffset = new Vector3D(minLeft.X < minRight.X ? offsetPosLeft.X : offsetPosRight.X, minLeft.Y < minRight.Y ? offsetPosLeft.Y : offsetPosRight.Y, minLeft.Z < minRight.Z ? offsetPosLeft.Z : offsetPosRight.Z);
                    var size = (max - min).RoundToVector3I();
                    asteroidSize = MyVoxelBuilder.CalcRequiredSize(size);
                    break;
                case VoxelMergeType.UnionMaterialLeftToRight:
                    min = SelectionRight.WorldAABB.Min - offsetPosRight;
                    posOffset = new Vector3D(minLeft.X < minRight.X ? offsetPosLeft.X : offsetPosRight.X, minLeft.Y < minRight.Y ? offsetPosLeft.Y : offsetPosRight.Y, minLeft.Z < minRight.Z ? offsetPosLeft.Z : offsetPosRight.Z);
                    asteroidSize = SelectionRight.Size;
                    break;
                case VoxelMergeType.UnionMaterialRightToLeft:
                    min = SelectionLeft.WorldAABB.Min - offsetPosLeft;
                    posOffset = new Vector3D(minLeft.X < minRight.X ? offsetPosLeft.X : offsetPosRight.X, minLeft.Y < minRight.Y ? offsetPosLeft.Y : offsetPosRight.Y, minLeft.Z < minRight.Z ? offsetPosLeft.Z : offsetPosRight.Z);
                    asteroidSize = SelectionLeft.Size;
                    break;
                case VoxelMergeType.SubtractVolumeLeftFromRight:
                    min = SelectionRight.WorldAABB.Min - offsetPosRight;
                    posOffset = new Vector3D(minLeft.X < minRight.X ? offsetPosLeft.X : offsetPosRight.X, minLeft.Y < minRight.Y ? offsetPosLeft.Y : offsetPosRight.Y, minLeft.Z < minRight.Z ? offsetPosLeft.Z : offsetPosRight.Z);
                    asteroidSize = SelectionRight.Size;
                    break;
                case VoxelMergeType.SubtractVolumeRightFromLeft:
                    min = SelectionLeft.WorldAABB.Min - offsetPosLeft;
                    posOffset = new Vector3D(minLeft.X < minRight.X ? offsetPosLeft.X : offsetPosRight.X, minLeft.Y < minRight.Y ? offsetPosLeft.Y : offsetPosRight.Y, minLeft.Z < minRight.Z ? offsetPosLeft.Z : offsetPosRight.Z);
                    asteroidSize = SelectionLeft.Size;
                    break;
            }

            // Prepare new asteroid.
            var newAsteroid = new MyVoxelMap();
            newAsteroid.Create(asteroidSize, SpaceEngineersCore.Resources.GetDefaultMaterialIndex());
            if (string.IsNullOrEmpty(MergeFileName))
                MergeFileName = "merge";
            var filename = MainViewModel.CreateUniqueVoxelStorageName(MergeFileName);

            // merge.
            switch (VoxelMergeType)
            {
                case VoxelMergeType.UnionVolumeLeftToRight:
                    MergeAsteroidVolumeInto(ref newAsteroid, min, SelectionRight, SelectionLeft, minRight, minLeft);
                    break;
                case VoxelMergeType.UnionVolumeRightToLeft:
                    MergeAsteroidVolumeInto(ref newAsteroid, min, SelectionLeft, SelectionRight, minLeft, minRight);
                    break;
                case VoxelMergeType.UnionMaterialLeftToRight:
                    MergeAsteroidMaterialFrom(ref newAsteroid, min, SelectionRight, SelectionLeft, minRight, minLeft);
                    break;
                case VoxelMergeType.UnionMaterialRightToLeft:
                    MergeAsteroidMaterialFrom(ref newAsteroid, min, SelectionLeft, SelectionRight, minLeft, minRight);
                    break;
                case VoxelMergeType.SubtractVolumeLeftFromRight:
                    SubtractAsteroidVolumeFrom(ref newAsteroid, min, SelectionRight, SelectionLeft, minRight, minLeft);
                    break;
                case VoxelMergeType.SubtractVolumeRightFromLeft:
                    SubtractAsteroidVolumeFrom(ref newAsteroid, min, SelectionLeft, SelectionRight, minLeft, minRight);
                    break;
            }

            // Generate Entity
            var tempfilename = TempfileUtil.NewFilename(MyVoxelMap.V2FileExtension);
            newAsteroid.Save(tempfilename);
            SourceFile = tempfilename;

            var position = min + posOffset;
            var entity = new MyObjectBuilder_VoxelMap(position, filename)
            {
                EntityId = SpaceEngineersApi.GenerateEntityId(IDType.ASTEROID),
                PersistentFlags = MyPersistentEntityFlags2.CastShadows | MyPersistentEntityFlags2.InScene,
                StorageName = Path.GetFileNameWithoutExtension(filename),
                PositionAndOrientation = new MyPositionAndOrientation
                {
                    Position = position,
                    Forward = Vector3.Forward,
                    Up = Vector3.Up
                }
            };

            return entity;
        }

        #region MergeAsteroidVolumeInto

        private void MergeAsteroidVolumeInto(ref MyVoxelMap newAsteroid, Vector3D min, StructureVoxelModel modelPrimary,
            StructureVoxelModel modelSecondary, Vector3D minPrimary, Vector3D minSecondary)
        {
            var filenameSecondary = modelSecondary.SourceVoxelFilepath ?? modelSecondary.VoxelFilepath;
            var filenamePrimary = modelPrimary.SourceVoxelFilepath ?? modelPrimary.VoxelFilepath;

            Vector3I block;
            Vector3I newBlock;
            Vector3I cacheSize;

            using var asteroid = new MyVoxelMap();
            asteroid.Load(filenameSecondary);

            BoundingBoxI content = modelSecondary.InflatedContentBounds;

            for (block.Z = content.Min.Z; block.Z <= content.Max.Z; block.Z += 64)
            {
                for (block.Y = content.Min.Y; block.Y <= content.Max.Y; block.Y += 64)
                {
                    for (block.X = content.Min.X; block.X <= content.Max.X; block.X += 64)
                    {
                        var cache = new MyStorageData();

                        cacheSize = new Vector3I(MathHelper.Min(content.Max.X, block.X + 63) - block.X + 1,
                            MathHelper.Min(content.Max.Y, block.Y + 63) - block.Y + 1,
                            MathHelper.Min(content.Max.Z, block.Z + 63) - block.Z + 1);

                        cache.Resize(cacheSize);

                        asteroid.Storage.ReadRange(cache, MyStorageDataTypeFlags.ContentAndMaterial, 0, block, block + cacheSize - 1);

                        newBlock = ((minSecondary - min) + (Vector3D)(block - content.Min)).RoundToVector3I();

                        newAsteroid.Storage.WriteRange(cache, MyStorageDataTypeFlags.ContentAndMaterial, newBlock, newBlock + cacheSize - 1);
                    }
                }
            }

            asteroid.Load(filenamePrimary);
            content = modelPrimary.InflatedContentBounds;

            for (block.Z = content.Min.Z; block.Z <= content.Max.Z; block.Z += 64)
            {
                for (block.Y = content.Min.Y; block.Y <= content.Max.Y; block.Y += 64)
                {
                    for (block.X = content.Min.X; block.X <= content.Max.X; block.X += 64)
                    {
                        var cache = new MyStorageData();

                        cacheSize = new Vector3I(MathHelper.Min(content.Max.X, block.X + 63) - block.X + 1,
                            MathHelper.Min(content.Max.Y, block.Y + 63) - block.Y + 1,
                            MathHelper.Min(content.Max.Z, block.Z + 63) - block.Z + 1);

                        cache.Resize(cacheSize);
                        asteroid.Storage.ReadRange(cache, MyStorageDataTypeFlags.ContentAndMaterial, 0, block, block + cacheSize - 1);

                        newBlock = ((minPrimary - min) + (Vector3D)(block - content.Min)).RoundToVector3I();

                        var newCache = new MyStorageData();
                        newCache.Resize(cacheSize);

                        newAsteroid.Storage.ReadRange(newCache, MyStorageDataTypeFlags.ContentAndMaterial, 0, newBlock, newBlock + cacheSize - 1);

                        Vector3I p;

                        for (p.Z = 0; p.Z < cacheSize.Z; p.Z++)
                        {
                            for (p.Y = 0; p.Y < cacheSize.Y; p.Y++)
                            {
                                for (p.X = 0; p.X < cacheSize.X; p.X++)
                                {
                                    byte volume = cache.Content(ref p);
                                    byte material = cache.Material(ref p);

                                    if (volume > 0)
                                    {
                                        byte existingVolume = newCache.Content(ref p);

                                        if (volume > existingVolume)
                                            newCache.Content(ref p, volume);

                                        // Overwrites secondary material with primary.
                                        newCache.Material(ref p, material);
                                    }
                                    else
                                    {
                                        // try to find cover material.
                                        Vector3I[] points = CreateTestPoints(p, cacheSize - 1);

                                        for (int i = 0 ; i < points.Length;i++)
                                        {
                                            byte testVolume = cache.Content(ref points[i]);

                                            if (testVolume > 0)
                                            {
                                                material = cache.Material(ref points[i]);
                                                newCache.Material(ref p, material);
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        newAsteroid.Storage.WriteRange(newCache, MyStorageDataTypeFlags.ContentAndMaterial, newBlock, newBlock + cacheSize - 1);
                    }
                }
            }
        }

        #endregion

        #region SubtractAsteroidVolumeFrom

        private void SubtractAsteroidVolumeFrom(ref MyVoxelMap newAsteroid, Vector3D min, StructureVoxelModel modelPrimary,
            StructureVoxelModel modelSecondary, Vector3D minPrimary, Vector3D minSecondary)
        {
            var filenameSecondary = modelSecondary.SourceVoxelFilepath ?? modelSecondary.VoxelFilepath;
            var filenamePrimary = modelPrimary.SourceVoxelFilepath ?? modelPrimary.VoxelFilepath;

            Vector3I block;
            Vector3I newBlock;
            Vector3I cacheSize;

            using var asteroid = new MyVoxelMap();
            asteroid.Load(filenamePrimary);

            BoundingBoxI content = modelPrimary.InflatedContentBounds;

            for (block.Z = content.Min.Z; block.Z <= content.Max.Z; block.Z += 64)
            {
                for (block.Y = content.Min.Y; block.Y <= content.Max.Y; block.Y += 64)
                {
                    for (block.X = content.Min.X; block.X <= content.Max.X; block.X += 64)
                    {
                        var cache = new MyStorageData();

                        cacheSize = new Vector3I(MathHelper.Min(content.Max.X, block.X + 63) - block.X + 1,
                            MathHelper.Min(content.Max.Y, block.Y + 63) - block.Y + 1,
                            MathHelper.Min(content.Max.Z, block.Z + 63) - block.Z + 1);

                        cache.Resize(cacheSize);

                        asteroid.Storage.ReadRange(cache, MyStorageDataTypeFlags.ContentAndMaterial, 0, block, block + cacheSize - 1);

                        newBlock = ((minPrimary - min) + (Vector3D)(block - content.Min)).RoundToVector3I();

                        newAsteroid.Storage.WriteRange(cache, MyStorageDataTypeFlags.ContentAndMaterial, newBlock, newBlock + cacheSize - 1);
                    }
                }
            }

            asteroid.Load(filenameSecondary);

            content = modelSecondary.InflatedContentBounds;

            for (block.Z = content.Min.Z; block.Z <= content.Max.Z; block.Z += 64)
            {
                for (block.Y = content.Min.Y; block.Y <= content.Max.Y; block.Y += 64)
                {
                    for (block.X = content.Min.X; block.X <= content.Max.X; block.X += 64)
                    {
                        var cache = new MyStorageData();

                        cacheSize = new Vector3I(MathHelper.Min(content.Max.X, block.X + 63) - block.X + 1,
                            MathHelper.Min(content.Max.Y, block.Y + 63) - block.Y + 1,
                            MathHelper.Min(content.Max.Z, block.Z + 63) - block.Z + 1);

                        cache.Resize(cacheSize);

                        asteroid.Storage.ReadRange(cache, MyStorageDataTypeFlags.Content, 0, block, block + cacheSize - 1);

                        newBlock = ((minSecondary - min) + (Vector3D)(block - content.Min)).RoundToVector3I();

                        var newCache = new MyStorageData();
                        newCache.Resize(cacheSize);

                        newAsteroid.Storage.ReadRange(newCache, MyStorageDataTypeFlags.ContentAndMaterial, 0, newBlock, newBlock + cacheSize - 1);

                        Vector3I p;

                        for (p.Z = 0; p.Z < cacheSize.Z; p.Z++)
                        {
                            for (p.Y = 0; p.Y < cacheSize.Y; p.Y++)
                            {
                                for (p.X = 0; p.X < cacheSize.X; p.X++)
                                {
                                    byte volume = cache.Content(ref p);

                                    if (volume > 0)
                                    {
                                        byte existingVolume = newCache.Content(ref p);

                                        if (existingVolume - volume < 0)
                                            volume = 0;
                                        else
                                            volume = (byte)(existingVolume - volume);

                                        newCache.Content(ref p, volume);
                                    }
                                }
                            }
                        }

                        newAsteroid.Storage.WriteRange(newCache, MyStorageDataTypeFlags.ContentAndMaterial, newBlock, newBlock + cacheSize - 1);
                    }
                }
            }
        }

        #endregion

        #region MergeAsteroidMaterialFrom

        private void MergeAsteroidMaterialFrom(ref MyVoxelMap newAsteroid, Vector3 min, StructureVoxelModel modelPrimary,
            StructureVoxelModel modelSecondary, Vector3 minPrimary, Vector3 minSecondary)
        {
            var filenameSecondary = modelSecondary.SourceVoxelFilepath ?? modelSecondary.VoxelFilepath;
            var filenamePrimary = modelPrimary.SourceVoxelFilepath ?? modelPrimary.VoxelFilepath;

            Vector3I block;
            Vector3I newBlock;
            Vector3I cacheSize;

            using var asteroid = new MyVoxelMap();
            asteroid.Load(filenamePrimary);

            BoundingBoxI content = modelPrimary.InflatedContentBounds;

            for (block.Z = content.Min.Z; block.Z <= content.Max.Z; block.Z += 64)
            {
                for (block.Y = content.Min.Y; block.Y <= content.Max.Y; block.Y += 64)
                {
                    for (block.X = content.Min.X; block.X <= content.Max.X; block.X += 64)
                    {
                        var cache = new MyStorageData();

                        cacheSize = new Vector3I(MathHelper.Min(content.Max.X, block.X + 63) - block.X + 1,
                            MathHelper.Min(content.Max.Y, block.Y + 63) - block.Y + 1,
                            MathHelper.Min(content.Max.Z, block.Z + 63) - block.Z + 1);

                        cache.Resize(cacheSize);

                        asteroid.Storage.ReadRange(cache, MyStorageDataTypeFlags.ContentAndMaterial, 0, block, block + cacheSize - 1);

                        newBlock = ((minPrimary - min) + (Vector3D)(block - content.Min)).RoundToVector3I();

                        newAsteroid.Storage.WriteRange(cache, MyStorageDataTypeFlags.ContentAndMaterial, newBlock, newBlock + cacheSize - 1);
                    }
                }
            }

            asteroid.Load(filenameSecondary);

            content = modelSecondary.InflatedContentBounds;

            for (block.Z = content.Min.Z; block.Z <= content.Max.Z; block.Z += 64)
            {
                for (block.Y = content.Min.Y; block.Y <= content.Max.Y; block.Y += 64)
                {
                    for (block.X = content.Min.X; block.X <= content.Max.X; block.X += 64)
                    {
                        var cache = new MyStorageData();

                        cacheSize = new Vector3I(MathHelper.Min(content.Max.X, block.X + 63) - block.X + 1,
                            MathHelper.Min(content.Max.Y, block.Y + 63) - block.Y + 1,
                            MathHelper.Min(content.Max.Z, block.Z + 63) - block.Z + 1);

                        cache.Resize(cacheSize);

                        asteroid.Storage.ReadRange(cache, MyStorageDataTypeFlags.ContentAndMaterial, 0, block, block + cacheSize - 1);

                        newBlock = ((minSecondary - min) + (Vector3D)(block - content.Min)).RoundToVector3I();

                        var newCache = new MyStorageData();
                        newCache.Resize(cacheSize);

                        newAsteroid.Storage.ReadRange(newCache, MyStorageDataTypeFlags.ContentAndMaterial, 0, newBlock, newBlock + cacheSize - 1);

                        Vector3I p;

                        for (p.Z = 0; p.Z < cacheSize.Z; p.Z++)
                        {
                            for (p.Y = 0; p.Y < cacheSize.Y; p.Y++)
                            {
                                for (p.X = 0; p.X < cacheSize.X; p.X++)
                                {
                                    byte volume = cache.Content(ref p);
                                    byte material = cache.Material(ref p);

                                    if (volume > 0)
                                    {
                                        newCache.Material(ref p, material);
                                    }
                                }
                            }
                        }

                        newAsteroid.Storage.WriteRange(newCache, MyStorageDataTypeFlags.ContentAndMaterial, newBlock, newBlock + cacheSize - 1);
                    }
                }
            }
        }

        #endregion

        /// <summary>
        /// Creates a list of points around the specified point (within a 3x3x3 grid), but only when they are within the bounds.
        /// </summary>
        private Vector3I[] CreateTestPoints(Vector3I point, Vector3I max)
        {
            List<Vector3I> points = new CacheList<Vector3I>();

            if (point.X > 0) points.Add(new Vector3I(point.X - 1, point.Y, point.Z));
            if (point.Y > 0) points.Add(new Vector3I(point.X, point.Y - 1, point.Z));
            if (point.Z > 0) points.Add(new Vector3I(point.X, point.Y, point.Z - 1));
            if (point.X < max.X) points.Add(new Vector3I(point.X + 1, point.Y, point.Z));
            if (point.Y < max.Y) points.Add(new Vector3I(point.X, point.Y + 1, point.Z));
            if (point.Z < max.Z) points.Add(new Vector3I(point.X, point.Y, point.Z + 1));

            if (point.X > 0 && point.Y > 0) points.Add(new Vector3I(point.X - 1, point.Y - 1, point.Z));
            if (point.Y > 0 && point.Z > 0) points.Add(new Vector3I(point.X, point.Y - 1, point.Z - 1));
            if (point.X > 0 && point.Z > 0) points.Add(new Vector3I(point.X - 1, point.Y, point.Z - 1));
            if (point.X < max.X && point.Y < max.Y) points.Add(new Vector3I(point.X + 1, point.Y + 1, point.Z));
            if (point.Y < max.Y && point.Z < max.Z) points.Add(new Vector3I(point.X, point.Y + 1, point.Z + 1));
            if (point.X < max.X && point.Z < max.Z) points.Add(new Vector3I(point.X + 1, point.Y, point.Z + 1));

            if (point.X > 0 && point.Y < max.Y) points.Add(new Vector3I(point.X - 1, point.Y + 1, point.Z));
            if (point.X < max.X && point.Y > 0) points.Add(new Vector3I(point.X + 1, point.Y - 1, point.Z));
            if (point.Y > 0 && point.Z < max.Z) points.Add(new Vector3I(point.X, point.Y - 1, point.Z + 1));
            if (point.Y < max.Y && point.Z > 0) points.Add(new Vector3I(point.X, point.Y + 1, point.Z - 1));
            if (point.X > 0 && point.Z < max.Z) points.Add(new Vector3I(point.X - 1, point.Y, point.Z + 1));
            if (point.X < max.X && point.Z > 0) points.Add(new Vector3I(point.X + 1, point.Y, point.Z - 1));

            if (point.X > 0 && point.Y > 0 && point.Z > 0) points.Add(new Vector3I(point.X - 1, point.Y - 1, point.Z - 1));
            if (point.X > 0 && point.Y > 0 && point.Z < max.Z) points.Add(new Vector3I(point.X - 1, point.Y - 1, point.Z + 1));
            if (point.X > 0 && point.Y < max.Y && point.Z > 0) points.Add(new Vector3I(point.X - 1, point.Y + 1, point.Z - 1));
            if (point.X > 0 && point.Y < max.Y && point.Z < max.Z) points.Add(new Vector3I(point.X - 1, point.Y + 1, point.Z + 1));
            if (point.X < max.X && point.Y > 0 && point.Z > 0) points.Add(new Vector3I(point.X + 1, point.Y - 1, point.Z - 1));
            if (point.X < max.X && point.Y > 0 && point.Z < max.Z) points.Add(new Vector3I(point.X + 1, point.Y - 1, point.Z + 1));
            if (point.X < max.X && point.Y < max.Y && point.Z > 0) points.Add(new Vector3I(point.X + 1, point.Y + 1, point.Z - 1));
            if (point.X < max.X && point.Y < max.Y && point.Z < max.Z) points.Add(new Vector3I(point.X + 1, point.Y + 1, point.Z + 1));

            return points.ToArray();
        }
    }
}

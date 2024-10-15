namespace SEToolbox.Interop.Asteroids
{
    using Sandbox.Engine.Voxels;
    using Sandbox.Engine.Voxels.Planet;
    using SEToolbox.Interop;
    using SEToolbox.Support;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.IO.Compression;
    using VRage.FileSystem;
    using VRage.Game.Voxels;
    using VRage.Library.Compression;
    using VRage.ObjectBuilders;
    using VRage.Voxels;
    using VRageMath;
    using Res = SEToolbox.Properties.Resources;

    public class MyVoxelMap : Sandbox.Game.Entities.MyVoxelBase, IDisposable
    {
        public const string V1FileExtension = ".vox";
        public const string V2FileExtension = ".vx2";
        internal const string TagCell = "Cell";

        #region fields

        private BoundingBoxI _boundingContent;

        private Dictionary<byte, long> _assetCount;

        #endregion

        #region properties

        public new Vector3I Size { get; private set; }

        public BoundingBoxI BoundingContent => _boundingContent;

        /// <summary>
        /// The BoundingContent + 1 around all sides.
        /// This allows operations for copying the voxel correctly.
        /// The volume itself, plus 1 extra layer surrounding the volume which affects the visual appearance at lower LODs.
        /// </summary>
        public BoundingBoxI InflatedBoundingContent
        {
            get
            {
                BoundingBoxI content = _boundingContent;

                content.Inflate(1);
                if (content.Min.X < 0) content.Min.X = 0;
                if (content.Min.Y < 0) content.Min.Y = 0;
                if (content.Min.Z < 0) content.Min.Z = 0;

                if (content.Max.X >= Size.X) content.Max.X = Size.X - 1;
                if (content.Max.Y >= Size.Y) content.Max.Y = Size.Y - 1;
                if (content.Max.Z >= Size.Z) content.Max.Z = Size.Z - 1;

                return content;
            }
        }

        public Vector3D ContentCenter => _boundingContent.ToBoundingBoxD().Center;

        //public byte VoxelMaterial { get; private set; }

        public bool IsValid { get; private set; }

        public long VoxCells { get; private set; }

        #endregion

        #region Init

        public override void Init(MyObjectBuilder_EntityBase builder, IMyStorage storage)
        {
            m_storage = (MyStorageBase)storage;
        }

        public override Sandbox.Game.Entities.MyVoxelBase RootVoxel => this;

        public override IMyStorage Storage
        {
            get { return m_storage; }
            set { m_storage = value; }
        }

        public void Create(Vector3I size, byte materialIndex)
        {
            m_storage?.Close();

            var octreeStorage = new MyOctreeStorage(null, size);
            octreeStorage.Geometry.Init(octreeStorage);
            m_storage = octreeStorage;
            OverwriteAllMaterials(materialIndex);

            IsValid = true;
            Size = octreeStorage.Size;
            _boundingContent = new BoundingBoxI();
            VoxCells = 0;
        }

        private void OverwriteAllMaterials(byte materialIndex)
        {
            // For some reason the cacheSize will NOT work at the same size of the storage when less than 64.
            // This can be seen by trying to read the material, update and write back, then read again to verify.
            // Trying to adjust the size in BlockFillMaterial will only lead to memory corruption.
            // Normally I wouldn't recommend usig an oversized cache, but in this case it should not be an issue as we are changing the material for the entire voxel space.
            var cacheSize = Vector3I.Min(new Vector3I(64), m_storage.Size * 2);

            Vector3I block;
            // read the asteroid in chunks to avoid the Arithmetic overflow issue.
            for (block.Z = 0; block.Z < m_storage.Size.Z; block.Z += 64)
                for (block.Y = 0; block.Y < m_storage.Size.Y; block.Y += 64)
                    for (block.X = 0; block.X < m_storage.Size.X; block.X += 64)
                    {
                        var cache = new MyStorageData();
                        cache.Resize(cacheSize);
                        // LOD1 is not detailed enough for content information on asteroids.
                        Vector3I maxRange = block + cacheSize - 1;
                        m_storage.ReadRange(cache, MyStorageDataTypeFlags.Material, 0, block, maxRange);
                        cache.BlockFillMaterial(Vector3I.Zero, cacheSize - 1, materialIndex);
                        m_storage.WriteRange(cache, MyStorageDataTypeFlags.Material, block, maxRange);
                    }
        }

        #endregion

        public static Dictionary<string, long> GetMaterialAssetDetails(string filename)
        {
            var list = new Dictionary<string, long>();
            var map = new MyVoxelMap();
            map.Load(filename);

            if (!map.IsValid)
                return list;

            list = map.RefreshAssets();
            map.Dispose();

            return list;
        }

        #region IsVoxelMapFile

        /// <summary>
        /// check for Magic Number: 1f 8b
        /// </summary>
        public static bool IsVoxelMapFile(string filename)
        {
            var extension = Path.GetExtension(filename);
            if (extension != null && extension.Equals(V1FileExtension, StringComparison.InvariantCultureIgnoreCase))
            {
                using (FileStream stream = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    try
                    {
                        var msgLength1 = stream.ReadByte();
                        var msgLength2 = stream.ReadByte();
                        var msgLength3 = stream.ReadByte();
                        var msgLength4 = stream.ReadByte();
                        var b1 = stream.ReadByte();
                        var b2 = stream.ReadByte();
                        return (b1 == 0x1f && b2 == 0x8b);
                    }
                    catch
                    {
                        return false;
                    }
                }
            }
            if (extension != null && extension.Equals(V2FileExtension, StringComparison.InvariantCultureIgnoreCase))
            {
                try
                {
                    return ZipTools.IsGzipedFile(filename);
                }
                catch
                {
                    return false;
                }
            }

            return false;
        }

        #endregion

        #region Load

        public void Load(string filename)
        {
            try
            {
                m_storage = MyStorageBase.LoadFromFile(filename);
                IsValid = true;
                Size = m_storage.Size;

                GC.Collect();
            }
            catch (FileNotFoundException)
            {
                // this exception may hide a dll dependancy from the game that is required, so it needs to be rethrown.
                throw;
            }
            catch (Exception ex)
            {
                Size = Vector3I.Zero;
                _boundingContent = new BoundingBoxI();
                VoxCells = 0;
                IsValid = false;
                DiagnosticsLogging.LogWarning(string.Format(Res.ExceptionState_CorruptAsteroidFile, filename), ex);
            }
        }

        /// implemented from Sandbox.Engine.Voxels.MyStorageBase.UpdateFileFormat(string originalVoxFile), but we need control of the destination file.
        public static void UpdateFileFormat(string originalVoxFile, string newVoxFile)
        {
            using (MyCompressionFileLoad myCompressionFileLoad = new MyCompressionFileLoad(originalVoxFile))
            {
                using (Stream stream = MyFileSystem.OpenWrite(newVoxFile, FileMode.Create))
                {
                    using (GZipStream gZipStream = new GZipStream(stream, CompressionMode.Compress))
                    {
                        using (BufferedStream bufferedStream = new BufferedStream(gZipStream))
                        {
                            bufferedStream.WriteNoAlloc(TagCell, null);
                            bufferedStream.Write7BitEncodedInt(myCompressionFileLoad.GetInt32());
                            byte[] array = new byte[16384];
                            for (int bytes = myCompressionFileLoad.GetBytes(array.Length, array); bytes != 0; bytes = myCompressionFileLoad.GetBytes(array.Length, array))
                            {
                                bufferedStream.Write(array, 0, bytes);
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region LoadVoxelSize

        /// <summary>
        /// Loads the header details only for voxel files, without having to decompress the entire file.
        /// </summary>
        /// <param name="filename"></param>
        public static Vector3I LoadVoxelSize(string filename)
        {
            try
            {
                if (Path.GetExtension(filename).Equals(V2FileExtension, StringComparison.InvariantCultureIgnoreCase))
                {
                    var map = new MyVoxelMap();
                    map.Load(filename);
                    map.Dispose();
                    return map.Size;
                }

                // Leaving the .vox file to the old code, as we only need to interrogate it for the voxel size, not load it into memory.

                // only 29 bytes are required for the header, but I'll leave it for 32 for a bit of extra leeway.
                var buffer = Uncompress(filename, 32);

                using (var reader = new BinaryReader(new MemoryStream(buffer)))
                {
                    reader.ReadInt32();// fileVersion
                    var sizeX = reader.ReadInt32();
                    var sizeY = reader.ReadInt32();
                    var sizeZ = reader.ReadInt32();

                    return new Vector3I(sizeX, sizeY, sizeZ);
                }
            }
            catch
            {
                return Vector3I.Zero;
            }
        }

        #endregion

        #region Save

        /// <summary>
        /// Saves the asteroid to the specified filename.
        /// </summary>
        /// <param name="filename">the file extension indicates the version of file been saved.</param>
        public new void Save(string filename)
        {
            Debug.Write("Saving binary.");

            byte[] array;
            m_storage.Save(out array);

            File.WriteAllBytes(filename, array);

            Debug.Write("Done.");
        }

        #endregion

        #region Un/Compress

        /// <summary>
        /// Used to compress the old .vox format voxel files.
        /// </summary>
        public static void CompressV1(string sourceFilename, string destinationFilename)
        {
            // Low memory, fast compress.
            using (var originalByteStream = new FileStream(sourceFilename, FileMode.Open))
            {
                if (File.Exists(destinationFilename))
                    File.Delete(destinationFilename);

                using (var compressedByteStream = new FileStream(destinationFilename, FileMode.CreateNew))
                {
                    compressedByteStream.Write(BitConverter.GetBytes(originalByteStream.Length), 0, 4);

                    // GZipStream requires using. Do not optimize the stream.
                    using (var compressionStream = new GZipStream(compressedByteStream, CompressionMode.Compress, true))
                    {
                        originalByteStream.CopyTo(compressionStream);
                    }

                    Debug.WriteLine("Compressed from {0:#,###0} bytes to {1:#,###0} bytes.", originalByteStream.Length, compressedByteStream.Length);
                }
            }
        }

        /// <summary>
        /// Used to decompress the old .vox format voxel files.
        /// </summary>
        /// <param name="sourceFilename"></param>
        /// <param name="destinationFilename"></param>
        public static void UncompressV1(string sourceFilename, string destinationFilename)
        {
            // Low memory, fast extract.
            using (var compressedByteStream = new FileStream(sourceFilename, FileMode.Open, FileAccess.Read))
            {
                var reader = new BinaryReader(compressedByteStream);
                // message Length.
                reader.ReadInt32();

                if (File.Exists(destinationFilename))
                    File.Delete(destinationFilename);

                using (var outStream = new FileStream(destinationFilename, FileMode.CreateNew))
                {
                    // GZipStream requires using. Do not optimize the stream.
                    using (var zip = new GZipStream(compressedByteStream, CompressionMode.Decompress))
                    {
                        zip.CopyTo(outStream);
                        Debug.WriteLine("Decompressed from {0:#,###0} bytes to {1:#,###0} bytes.", compressedByteStream.Length, outStream.Length);
                    }
                }
            }
        }

        /// <summary>
        /// Used for loading older format voxel file streams, like .vox and first version of .vx2
        /// This is kept for legacy purposes, nothing more.
        /// </summary>
        public static byte[] Uncompress(string sourceFilename, int numberBytes)
        {
            using (var compressedByteStream = new FileStream(sourceFilename, FileMode.Open, FileAccess.Read))
            {
                var reader = new BinaryReader(compressedByteStream);
                // message Length.
                reader.ReadInt32();

                // GZipStream requires using. Do not optimize the stream.
                using (var zip = new GZipStream(compressedByteStream, CompressionMode.Decompress))
                {
                    var arr = new byte[numberBytes];
                    zip.Read(arr, 0, numberBytes);
                    return arr;
                }
            }
        }

        #endregion

        #region methods

        #region SetVoxelContentRegion

        public void SetVoxelContentRegion(byte content, int? xMin, int? xMax, int? yMin, int? yMax, int? zMin, int? zMax)
        {
            Vector3I block;
            var cacheSize = Vector3I.Min(new Vector3I(64), m_storage.Size);

            // read the asteroid in chunks of 64 to avoid the Arithmetic overflow issue.
            for (block.Z = 0; block.Z < m_storage.Size.Z; block.Z += 64)
                for (block.Y = 0; block.Y < m_storage.Size.Y; block.Y += 64)
                    for (block.X = 0; block.X < m_storage.Size.X; block.X += 64)
                    {
                        var cache = new MyStorageData();
                        cache.Resize(cacheSize);
                        Vector3I maxRange = block + cacheSize - 1;
                        m_storage.ReadRange(cache, MyStorageDataTypeFlags.Content, 0, block, maxRange);

                        bool changed = false;
                        Vector3I p;
                        for (p.Z = 0; p.Z < cacheSize.Z; ++p.Z)
                            for (p.Y = 0; p.Y < cacheSize.Y; ++p.Y)
                                for (p.X = 0; p.X < cacheSize.X; ++p.X)
                                {
                                    var coords = p + block;

                                    if (xMin.HasValue && xMax.HasValue && yMin.HasValue && yMax.HasValue && zMin.HasValue && zMax.HasValue)
                                    {
                                        if (xMin.Value <= coords.X && coords.X <= xMax.Value && yMin.Value <= coords.Y && coords.Y <= yMax.Value && zMin.Value <= coords.Z && coords.Z <= zMax.Value)
                                        {
                                            cache.Content(ref p, content);
                                            changed = true;
                                        }
                                    }
                                    else if (xMin.HasValue && xMax.HasValue && yMin.HasValue && yMax.HasValue)
                                    {
                                        if (xMin.Value <= coords.X && coords.X <= xMax.Value && yMin.Value <= coords.Y && coords.Y <= yMax.Value)
                                        {
                                            cache.Content(ref p, content);
                                            changed = true;
                                        }
                                    }
                                    else if (xMin.HasValue && xMax.HasValue && zMin.HasValue && zMax.HasValue)
                                    {
                                        if (xMin.Value <= coords.X && coords.X <= xMax.Value && zMin.Value <= coords.Z && coords.Z <= zMax.Value)
                                        {
                                            cache.Content(ref p, content);
                                            changed = true;
                                        }
                                    }
                                    else if (yMin.HasValue && yMax.HasValue && zMin.HasValue && zMax.HasValue)
                                    {
                                        if (yMin.Value <= coords.Y && coords.Y <= yMax.Value && zMin.Value <= coords.Z && coords.Z <= zMax.Value)
                                        {
                                            cache.Content(ref p, content);
                                            changed = true;
                                        }
                                    }
                                    else if (xMin.HasValue && xMax.HasValue)
                                    {
                                        if (xMin.Value <= coords.X && coords.X <= xMax.Value)
                                        {
                                            cache.Content(ref p, content);
                                            changed = true;
                                        }
                                    }
                                    else if (yMin.HasValue && yMax.HasValue)
                                    {
                                        if (yMin.Value <= coords.Y && coords.Y <= yMax.Value)
                                        {
                                            cache.Content(ref p, content);
                                            changed = true;
                                        }
                                    }
                                    else if (zMin.HasValue && zMax.HasValue)
                                    {
                                        if (zMin.Value <= coords.Z && coords.Z <= zMax.Value)
                                        {
                                            cache.Content(ref p, content);
                                            changed = true;
                                        }
                                    }
                                }

                        if (changed)
                            m_storage.WriteRange(cache, MyStorageDataTypeFlags.Content, block, maxRange);
                    }
        }

        #endregion

        #region SeedMaterialSphere

        /// <summary>
        /// Set a material for a random voxel cell and possibly nearest ones to it.
        /// </summary>
        /// <param name="materialIndex">material name</param>
        /// <param name="radius">radius in voxels, defaults to zero, meaning only a random grid.</param>
        public void SeedMaterialSphere(byte materialIndex, byte radius = 0)
        {
            var fullCells = new List<Vector3I>();
            Vector3I block;
            //var cacheSize = new Vector3I(64 >> 3); 
            var cacheSize = new Vector3I(1);

            // Using 8 to replicate the size of DataCell.
            // TODO: determine if there is an issue because the cache is undersized. The cache in OverwriteAllMaterials had to be doubled in size to avoid issue when < 64 but it is at LOD0.
            for (block.Z = 0; block.Z < m_storage.Size.Z >> 3; block.Z += 1)
                for (block.Y = 0; block.Y < m_storage.Size.Y >> 3; block.Y += 1)
                    for (block.X = 0; block.X < m_storage.Size.X >> 3; block.X += 1)
                    {
                        var cache = new MyStorageData();
                        cache.Resize(cacheSize);
                        m_storage.ReadRange(cache, MyStorageDataTypeFlags.Content, 3, block, block + cacheSize - 1);

                        Vector3I p = Vector3I.Zero;

                        // Unless volume is read, the call to ComputeContentConstitution() causes the fullCells list to not clear properly.
                        byte volume = cache.Content(ref p);

                        //if (volume > 0)
                        //{
                        //    if (cache.ComputeContentConstitution() == MyVoxelContentConstitution.Empty)
                        //    {
                        //    }
                        //}
                        if (cache.ComputeContentConstitution() != MyVoxelContentConstitution.Empty)
                            // Collect the non-empty cell coordinates
                            fullCells.Add(block << 3);
                    }


            cacheSize = new Vector3I(8);

            // Choose random cell and switch material there
            fullCells.Shuffle();
            int cellCount = fullCells.Count;

            for (int i = 0; i < cellCount; i++)
            {
                block = fullCells[i];
                if (i == 0)
                {
                    var cache = new MyStorageData();
                    cache.Resize(cacheSize);
                    m_storage.ReadRange(cache, MyStorageDataTypeFlags.ContentAndMaterial, 0, block, block + cacheSize - 1);

                    //if (cache.ComputeContentConstitution() == MyVoxelContentConstitution.Empty)
                    //{
                    //    // TODO: need to prevent empty selections better.
                    //    fullCells.RemoveAt(i);
                    //    i--;
                    //    continue;
                    //}

                    cache.BlockFillMaterial(Vector3I.Zero, cache.Size3D, materialIndex);
                    m_storage.WriteRange(cache, MyStorageDataTypeFlags.Material, block, block + cacheSize - 1);
                    continue;
                }
                // Optionally seek adjanced cells and set their material too.
                if (radius == 0)
                    return;
                Vector3I vlen = fullCells[0] - block;
                if (vlen.RectangularLength() <= radius)
                {
                    var cache = new MyStorageData();
                    cache.Resize(cacheSize);
                    m_storage.ReadRange(cache, MyStorageDataTypeFlags.ContentAndMaterial, 0, block, block + cacheSize - 1);

                    //if (cache.ComputeContentConstitution() == MyVoxelContentConstitution.Empty)
                    //{
                    //    // TODO: need to prevent empty selections better.
                    //    fullCells.RemoveAt(i);
                    //    i--;
                    //    continue;
                    //}

                    cache.BlockFillMaterial(Vector3I.Zero, cache.Size3D, materialIndex);
                    m_storage.WriteRange(cache, MyStorageDataTypeFlags.Material, block, block + cacheSize - 1);
                }
            }

            // Might need to clear the list, as the Structs sit in memory otherwise and kill the List.
            // Could be caused by calling ComputeContentConstitution() on the cache without doing anything else on it.
            //fullCells.Clear();
            //fullCells = null;
        }

        ///// <summary>
        ///// Set a material for a random voxel cell and possibly nearest ones to it.
        ///// </summary>
        ///// <param name="materialName">material name</param>
        ///// <param name="radius">radius in voxels, defaults to zero, meaning only a random grid.</param>
        //[Obsolete]
        //public void SeedMaterialSphere(string materialName, byte radius = 0)
        //{
        //    var fullCells = new List<Vector3I>();
        //    Vector3I cellCoord;
        //    // Collect the non-empty cell coordinates
        //    for (cellCoord.X = 0; cellCoord.X < _dataCellsCount.X; cellCoord.X++)
        //        for (cellCoord.Y = 0; cellCoord.Y < _dataCellsCount.Y; cellCoord.Y++)
        //            for (cellCoord.Z = 0; cellCoord.Z < _dataCellsCount.Z; cellCoord.Z++)
        //                if (!CheckCellType(ref _voxelContentCells[cellCoord.X][cellCoord.Y][cellCoord.Z], MyVoxelCellType.EMPTY))
        //                    fullCells.Add(cellCoord);

        //    // Choose random cell and switch material there
        //    fullCells.Shuffle();
        //    int cellCount = fullCells.Count;
        //    Vector3I cell, vlen;
        //    for (int i = 0; i < cellCount; i++)
        //    {
        //        cell = fullCells[i];
        //        if (i == 0)
        //        {
        //            SetVoxelMaterialRegion(materialName, ref cell);
        //            continue;
        //        }
        //        // Optionally seek adjanced cells and set their material too.
        //        if (radius == 0)
        //            return;
        //        vlen = fullCells[0] - cell;
        //        if (vlen.RectangularLength() <= radius)
        //        {
        //            SetVoxelMaterialRegion(materialName, ref cell);
        //        }
        //    }
        //}

        #endregion

        #region ForceBaseMaterial

        /// <summary>
        /// This will replace all the materials inside the asteroid with specified material.
        /// </summary>
        /// <param name="defaultMaterial"></param>
        /// <param name="materialName"></param>
        public void ForceBaseMaterial(string defaultMaterial, string materialName)
        {
            var materialIndex = SpaceEngineersCore.Resources.GetMaterialIndex(materialName);
            Vector3I block;
            var cacheSize = Vector3I.Min(new Vector3I(64), m_storage.Size);

            // read the asteroid in chunks of 64 to avoid the Arithmetic overflow issue.
            for (block.Z = 0; block.Z < m_storage.Size.Z; block.Z += 64)
                for (block.Y = 0; block.Y < m_storage.Size.Y; block.Y += 64)
                    for (block.X = 0; block.X < m_storage.Size.X; block.X += 64)
                    {
                        var cache = new MyStorageData();
                        cache.Resize(cacheSize);
                        Vector3I maxRange = block + cacheSize - 1;
                        m_storage.ReadRange(cache, MyStorageDataTypeFlags.Material, 0, block, maxRange);

                        Vector3I p;
                        for (p.Z = 0; p.Z < cacheSize.Z; ++p.Z)
                            for (p.Y = 0; p.Y < cacheSize.Y; ++p.Y)
                                for (p.X = 0; p.X < cacheSize.X; ++p.X)
                                {
                                    cache.Material(ref p, materialIndex);
                                }

                        m_storage.WriteRange(cache, MyStorageDataTypeFlags.Material, block, maxRange);
                    }
        }

        #endregion

        #region ForceVoxelFaceMaterial

        /// <summary>
        /// Changes all the min and max face materials to a default to overcome the the hiding rare ore inside of nonrare ore.
        /// </summary>
        [Obsolete("This is no longer required, as the voxel's no longer take their 'surface' texture from the outer most cell.")]
        public void ForceVoxelFaceMaterial(byte materialIndex)
        {
            Vector3I block;
            var cacheSize = Vector3I.Min(new Vector3I(64), m_storage.Size);

            // read the asteroid in chunks of 64 to avoid the Arithmetic overflow issue.
            for (block.Z = 0; block.Z < m_storage.Size.Z; block.Z += 64)
                for (block.Y = 0; block.Y < m_storage.Size.Y; block.Y += 64)
                    for (block.X = 0; block.X < m_storage.Size.X; block.X += 64)
                    {

                        if (block.X == 0 || block.Y == 0 || block.Z == 0 ||
                            block.X + cacheSize.X == m_storage.Size.X - 1 || block.Z + cacheSize.Y == m_storage.Size.Y - 1 ||
                            block.Z + cacheSize.Z == m_storage.Size.Z - 1)
                        {
                            var cache = new MyStorageData();
                            cache.Resize(cacheSize);
                            // LOD1 is not detailed enough for content information on asteroids.
                            Vector3I maxRange = block + cacheSize - 1;
                            m_storage.ReadRange(cache, MyStorageDataTypeFlags.Material, 0, block, maxRange);

                            bool changed = false;
                            Vector3I p;
                            for (p.Z = 0; p.Z < cacheSize.Z; ++p.Z)
                                for (p.Y = 0; p.Y < cacheSize.Y; ++p.Y)
                                    for (p.X = 0; p.X < cacheSize.X; ++p.X)
                                    {
                                        var min = p + block;
                                        if (min.X == 0 || min.Y == 0 || min.Z == 0 ||
                                            min.X == m_storage.Size.X - 1 || min.Y == m_storage.Size.Y - 1 || min.Z == m_storage.Size.Z - 1)
                                        {
                                            if (cache.Material(ref p) != materialIndex)
                                            {
                                                cache.Material(ref p, materialIndex);
                                                changed = true;
                                            }
                                        }
                                    }

                            if (changed)
                                m_storage.WriteRange(cache, MyStorageDataTypeFlags.Material, block, maxRange);
                        }
                    }
        }

        #endregion

        #region ForceShellFaceMaterials

        /// <summary>
        /// Force the material of the outermost mixed voxcells to the given material
        /// </summary>
        /// <param name="materialName"></param>
        /// <param name="targtThickness"></param>
        public void ForceShellMaterial(string materialName, byte targtThickness = 0)
        {
            byte curThickness;
            var materialIndex = SpaceEngineersCore.Resources.GetMaterialIndex(materialName);

            // read the asteroid in chunks of 8 to simulate datacells.
            var cacheSize = new Vector3I(m_storage.Size.X >> 3);
            var cache = new MyStorageData();
            cache.Resize(cacheSize);
            m_storage.ReadRange(cache, MyStorageDataTypeFlags.ContentAndMaterial, 3, Vector3I.Zero, cacheSize - 1);

            var writebufferSize = new Vector3I(8);
            var writebuffer = new MyStorageData();
            writebuffer.Resize(writebufferSize);

            Vector3I dataCell;
            for (dataCell.X = 0; dataCell.X < cacheSize.X; dataCell.X++)
            {
                for (dataCell.Y = 0; dataCell.Y < cacheSize.Y; dataCell.Y++)
                {
                    for (curThickness = 0, dataCell.Z = 0; dataCell.Z < cacheSize.Z - 1; dataCell.Z++)
                    {
                        Vector3I nextCell = dataCell + new Vector3I(0, 0, 1);
                        if (cache.Content(ref dataCell) != 0 && cache.Content(ref nextCell) != 0)
                        {
                            // read the dataCell location in the storage, and update the material at a byte level.
                            Vector3I bufferPosition = new Vector3I(dataCell.X << 3, dataCell.Y << 3, dataCell.Z << 3);
                            var maxRange = bufferPosition + writebufferSize - 1;
                            writebuffer.ClearMaterials(0);
                            m_storage.ReadRange(writebuffer, MyStorageDataTypeFlags.Material, 0, bufferPosition, maxRange);
                            writebuffer.BlockFillMaterial(Vector3I.Zero, writebufferSize - 1, materialIndex);
                            m_storage.WriteRange(writebuffer, MyStorageDataTypeFlags.Material, bufferPosition, maxRange);

                            if ((targtThickness > 0 && ++curThickness >= targtThickness) || cache.Content(ref nextCell) == 255)
                                break;
                        }
                    }
                    for (curThickness = 0, dataCell.Z = cacheSize.Z - 1; dataCell.Z > 0; dataCell.Z--)
                    {
                        Vector3I nextCell = dataCell + new Vector3I(0, 0, -1);
                        if (cache.Content(ref dataCell) != 0 && cache.Content(ref nextCell) != 0)
                        {
                            Vector3I bufferPosition = new Vector3I(dataCell.X << 3, dataCell.Y << 3, dataCell.Z << 3);
                            var maxRange = bufferPosition + writebufferSize - 1;
                            writebuffer.ClearMaterials(0);
                            m_storage.ReadRange(writebuffer, MyStorageDataTypeFlags.Material, 0, bufferPosition, maxRange);
                            writebuffer.BlockFillMaterial(Vector3I.Zero, writebufferSize - 1, materialIndex);
                            m_storage.WriteRange(writebuffer, MyStorageDataTypeFlags.Material, bufferPosition, maxRange);

                            if ((targtThickness > 0 && ++curThickness >= targtThickness) || cache.Content(ref nextCell) == 255)
                                break;
                        }
                    }
                }
            }

            for (dataCell.X = 0; dataCell.X < cacheSize.X; dataCell.X++)
            {
                for (dataCell.Z = 0; dataCell.Z < cacheSize.Z; dataCell.Z++)
                {
                    for (curThickness = 0, dataCell.Y = 0; dataCell.Y < cacheSize.Y - 1; dataCell.Y++)
                    {
                        Vector3I nextCell = dataCell + new Vector3I(0, 1, 0);
                        if (cache.Content(ref dataCell) != 0 && cache.Content(ref nextCell) != 0)
                        {
                            Vector3I bufferPosition = new Vector3I(dataCell.X << 3, dataCell.Y << 3, dataCell.Z << 3);
                            var maxRange = bufferPosition + writebufferSize - 1;
                            writebuffer.ClearMaterials(0);
                            m_storage.ReadRange(writebuffer, MyStorageDataTypeFlags.Material, 0, bufferPosition, maxRange);
                            writebuffer.BlockFillMaterial(Vector3I.Zero, writebufferSize - 1, materialIndex);
                            m_storage.WriteRange(writebuffer, MyStorageDataTypeFlags.Material, bufferPosition, maxRange);
                            if ((targtThickness > 0 && ++curThickness >= targtThickness) || cache.Content(ref nextCell) == 255)
                                break;
                        }
                    }
                    for (curThickness = 0, dataCell.Y = cacheSize.Y - 1; dataCell.Y > 0; dataCell.Y--)
                    {
                        Vector3I nextCell = dataCell + new Vector3I(0, -1, 0);
                        if (cache.Content(ref dataCell) != 0 && cache.Content(ref nextCell) != 0)
                        {
                            Vector3I bufferPosition = new Vector3I(dataCell.X << 3, dataCell.Y << 3, dataCell.Z << 3);
                            var maxRange = bufferPosition + writebufferSize - 1;
                            writebuffer.ClearMaterials(0);
                            m_storage.ReadRange(writebuffer, MyStorageDataTypeFlags.Material, 0, bufferPosition, maxRange);
                            writebuffer.BlockFillMaterial(Vector3I.Zero, writebufferSize - 1, materialIndex);
                            m_storage.WriteRange(writebuffer, MyStorageDataTypeFlags.Material, bufferPosition, maxRange);
                            if ((targtThickness > 0 && ++curThickness >= targtThickness) || cache.Content(ref nextCell) == 255)
                                break;
                        }
                    }
                }
            }

            for (dataCell.Z = 0; dataCell.Z < cacheSize.Z; dataCell.Z++)
            {
                for (dataCell.Y = 0; dataCell.Y < cacheSize.Y; dataCell.Y++)
                {
                    for (curThickness = 0, dataCell.X = 0; dataCell.X < cacheSize.X - 1; dataCell.X++)
                    {
                        Vector3I nextCell = dataCell + new Vector3I(1, 0, 0);
                        if (cache.Content(ref dataCell) != 0 && cache.Content(ref nextCell) != 0)
                        {
                            Vector3I bufferPosition = new Vector3I(dataCell.X << 3, dataCell.Y << 3, dataCell.Z << 3);
                            var maxRange = bufferPosition + writebufferSize - 1;
                            writebuffer.ClearMaterials(0);
                            m_storage.ReadRange(writebuffer, MyStorageDataTypeFlags.Material, 0, bufferPosition, maxRange);
                            writebuffer.BlockFillMaterial(Vector3I.Zero, writebufferSize - 1, materialIndex);
                            m_storage.WriteRange(writebuffer, MyStorageDataTypeFlags.Material, bufferPosition, maxRange);
                            if ((targtThickness > 0 && ++curThickness >= targtThickness) || cache.Content(ref nextCell) == 255)
                                break;
                        }
                    }
                    for (curThickness = 0, dataCell.X = cacheSize.X - 1; dataCell.X > 0; dataCell.X--)
                    {
                        Vector3I nextCell = dataCell + new Vector3I(-1, 0, 0);
                        if (cache.Content(ref dataCell) != 0 && cache.Content(ref nextCell) != 0)
                        {
                            Vector3I bufferPosition = new Vector3I(dataCell.X << 3, dataCell.Y << 3, dataCell.Z << 3);
                            var maxRange = bufferPosition + writebufferSize - 1;
                            writebuffer.ClearMaterials(0);
                            m_storage.ReadRange(writebuffer, MyStorageDataTypeFlags.Material, 0, bufferPosition, maxRange);
                            writebuffer.BlockFillMaterial(Vector3I.Zero, writebufferSize - 1, materialIndex);
                            m_storage.WriteRange(writebuffer, MyStorageDataTypeFlags.Material, bufferPosition, maxRange);
                            if ((targtThickness > 0 && ++curThickness >= targtThickness) || cache.Content(ref nextCell) == 255)
                                break;
                        }
                    }
                }
            }
        }

        #endregion

        #region RemoveContent

        public void RemoveContent(string materialName, string replaceFillMaterial)
        {
            var materialIndex = SpaceEngineersCore.Resources.GetMaterialIndex(materialName);
            var replaceMaterialIndex = materialIndex;
            if (!string.IsNullOrEmpty(replaceFillMaterial))
                replaceMaterialIndex = SpaceEngineersCore.Resources.GetMaterialIndex(replaceFillMaterial);
            Vector3I block;
            var cacheSize = Vector3I.Min(new Vector3I(64), m_storage.Size);

            // read the asteroid in chunks of 64 to avoid the Arithmetic overflow issue.
            for (block.Z = 0; block.Z < m_storage.Size.Z; block.Z += 64)
                for (block.Y = 0; block.Y < m_storage.Size.Y; block.Y += 64)
                    for (block.X = 0; block.X < m_storage.Size.X; block.X += 64)
                    {
                        var cache = new MyStorageData();
                        cache.Resize(cacheSize);
                        // LOD1 is not detailed enough for content information on asteroids.
                        Vector3I maxRange = block + cacheSize - 1;
                        m_storage.ReadRange(cache, MyStorageDataTypeFlags.ContentAndMaterial, 0, block, maxRange);

                        bool changed = false;
                        Vector3I p;
                        for (p.Z = 0; p.Z < cacheSize.Z; ++p.Z)
                            for (p.Y = 0; p.Y < cacheSize.Y; ++p.Y)
                                for (p.X = 0; p.X < cacheSize.X; ++p.X)
                                {
                                    if (cache.Material(ref p) == materialIndex)
                                    {
                                        cache.Content(ref p, 0);
                                        if (replaceMaterialIndex != materialIndex)
                                            cache.Material(ref p, replaceMaterialIndex);
                                        changed = true;
                                    }
                                }

                        if (changed)
                            m_storage.WriteRange(cache, MyStorageDataTypeFlags.ContentAndMaterial, block, maxRange);
                    }
        }

        public void RemoveMaterial(int? xMin, int? xMax, int? yMin, int? yMax, int? zMin, int? zMax)
        {
            SetVoxelContentRegion(0x00, xMin, xMax, yMin, yMax, zMin, zMax);
        }

        #endregion

        #region ReplaceMaterial

        public void ReplaceMaterial(string materialName, string replaceFillMaterial)
        {
            var materialIndex = SpaceEngineersCore.Resources.GetMaterialIndex(materialName);
            var replaceMaterialIndex = SpaceEngineersCore.Resources.GetMaterialIndex(replaceFillMaterial);

            Vector3I block;
            var cacheSize = Vector3I.Min(new Vector3I(64), m_storage.Size);

            // read the asteroid in chunks of 64 to avoid the Arithmetic overflow issue.
            for (block.Z = 0; block.Z < m_storage.Size.Z; block.Z += 64)
                for (block.Y = 0; block.Y < m_storage.Size.Y; block.Y += 64)
                    for (block.X = 0; block.X < m_storage.Size.X; block.X += 64)
                    {
                        var cache = new MyStorageData();
                        cache.Resize(cacheSize);
                        // LOD1 is not detailed enough for content information on asteroids.
                        Vector3I maxRange = block + cacheSize - 1;
                        m_storage.ReadRange(cache, MyStorageDataTypeFlags.Material, 0, block, maxRange);

                        bool changed = false;
                        Vector3I p;
                        for (p.Z = 0; p.Z < cacheSize.Z; ++p.Z)
                            for (p.Y = 0; p.Y < cacheSize.Y; ++p.Y)
                                for (p.X = 0; p.X < cacheSize.X; ++p.X)
                                {
                                    if (cache.Material(ref p) == materialIndex)
                                    {
                                        cache.Material(ref p, replaceMaterialIndex);
                                        changed = true;
                                    }
                                }

                        if (changed)
                            m_storage.WriteRange(cache, MyStorageDataTypeFlags.Material, block, maxRange);
                    }
        }

        #endregion

        #region SumVoxelCells

        private void CalcVoxelCells()
        {
            long sum = 0;

            if (!IsValid)
            {
                _assetCount = new Dictionary<byte, long>();
                _boundingContent = new BoundingBoxI();
                VoxCells = sum;
                return;
            }

            if (m_storage.DataProvider is MyPlanetStorageProvider)
            {
                _assetCount = new Dictionary<byte, long>();
                _boundingContent = new BoundingBoxI(Vector3I.Zero, m_storage.Size);
                VoxCells = sum;
                return;
            }

            Vector3I min = Vector3I.MaxValue;
            Vector3I max = Vector3I.MinValue;
            Vector3I block;
            var cacheSize = Vector3I.Min(new Vector3I(64), m_storage.Size);
            Dictionary<byte, long> assetCount = new Dictionary<byte, long>();

            var cache = new MyStorageData();
            cache.Resize(cacheSize);

            // read the asteroid in chunks of 64 to avoid the Arithmetic overflow issue.
            for (block.Z = 0; block.Z < m_storage.Size.Z; block.Z += 64)
            {
                for (block.Y = 0; block.Y < m_storage.Size.Y; block.Y += 64)
                {
                    for (block.X = 0; block.X < m_storage.Size.X; block.X += 64)
                    {
                        // LOD1 is not detailed enough for content information on asteroids.
                        Vector3I maxRange = block + cacheSize - 1;
                        m_storage.ReadRange(cache, MyStorageDataTypeFlags.ContentAndMaterial, 0, block, maxRange);

                        Vector3I p;

                        for (p.Z = 0; p.Z < cacheSize.Z; ++p.Z)
                        {
                            for (p.Y = 0; p.Y < cacheSize.Y; ++p.Y)
                            {
                                for (p.X = 0; p.X < cacheSize.X; ++p.X)
                                {
                                    byte content = cache.Content(ref p);

                                    if (content > 0)
                                    {
                                        min = Vector3I.Min(min, p + block);
                                        max = Vector3I.Max(max, p + block + 1);

                                        byte material = cache.Material(ref p);

                                        if (assetCount.TryGetValue(material, out long c))
                                            assetCount[material] = c + content;
                                        else
                                            assetCount.Add(material, content);

                                        sum += content;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            _assetCount = assetCount;

            if (min == Vector3I.MaxValue && max == Vector3I.MinValue)
                _boundingContent = new BoundingBoxI();
            else
                _boundingContent = new BoundingBoxI(min, max - 1);

            VoxCells = sum;
        }

        #endregion

        #region CalculateMaterialCellAssets

        public IList<byte> CalcVoxelMaterialList()
        {
            if (!IsValid)
                return null;

            Vector3I block;
            var cacheSize = Vector3I.Min(new Vector3I(64), m_storage.Size);
            var voxelMaterialList = new List<byte>();

            // read the asteroid in chunks of 64 to avoid the Arithmetic overflow issue.
            for (block.Z = 0; block.Z < m_storage.Size.Z; block.Z += 64)
                for (block.Y = 0; block.Y < m_storage.Size.Y; block.Y += 64)
                    for (block.X = 0; block.X < m_storage.Size.X; block.X += 64)
                    {
                        var cache = new MyStorageData();
                        cache.Resize(cacheSize);
                        // LOD1 is not detailed enough for content information on asteroids.
                        Vector3I maxRange = block + cacheSize - 1;
                        m_storage.ReadRange(cache, MyStorageDataTypeFlags.ContentAndMaterial, 0, block, maxRange);

                        Vector3I p;
                        for (p.Z = 0; p.Z < cacheSize.Z; ++p.Z)
                            for (p.Y = 0; p.Y < cacheSize.Y; ++p.Y)
                                for (p.X = 0; p.X < cacheSize.X; ++p.X)
                                {
                                    var content = cache.Content(ref p);
                                    if (content > 0)
                                    {
                                        voxelMaterialList.Add(cache.Material(ref p));
                                    }
                                }
                    }

            return voxelMaterialList;
        }

        public void SetVoxelMaterialList(IList<byte> materials)
        {
            if (!IsValid)
                return;

            Vector3I block;
            var cacheSize = Vector3I.Min(new Vector3I(64), m_storage.Size);
            int index = 0;

            // read the asteroid in chunks of 64 to avoid the Arithmetic overflow issue.
            for (block.Z = 0; block.Z < m_storage.Size.Z; block.Z += 64)
                for (block.Y = 0; block.Y < m_storage.Size.Y; block.Y += 64)
                    for (block.X = 0; block.X < m_storage.Size.X; block.X += 64)
                    {
                        var cache = new MyStorageData();
                        cache.Resize(cacheSize);
                        // LOD1 is not detailed enough for content information on asteroids.
                        Vector3I maxRange = block + cacheSize - 1;
                        m_storage.ReadRange(cache, MyStorageDataTypeFlags.ContentAndMaterial, 0, block, maxRange);

                        Vector3I p;
                        for (p.Z = 0; p.Z < cacheSize.Z; ++p.Z)
                            for (p.Y = 0; p.Y < cacheSize.Y; ++p.Y)
                                for (p.X = 0; p.X < cacheSize.X; ++p.X)
                                {
                                    var content = cache.Content(ref p);
                                    if (content > 0)
                                    {
                                        cache.Material(ref p, materials[index]);
                                        index++;
                                    }
                                }

                        m_storage.WriteRange(cache, MyStorageDataTypeFlags.Material, block, maxRange);
                    }
        }

        #endregion

        #endregion

        #region CountAssets

        public Dictionary<string, long> RefreshAssets()
        {
            CalcVoxelCells();
            return CountAssets();
        }

        private Dictionary<string, long> CountAssets()
        {
            var materialDefinitions = SpaceEngineersCore.Resources.VoxelMaterialDefinitions;
            var assetNameCount = new Dictionary<string, long>();
            var defaultMaterial = SpaceEngineersCore.Resources.GetMaterialIndex(SpaceEngineersCore.Resources.GetDefaultMaterialName());

            foreach (var kvp in _assetCount)
            {
                string name;

                if (kvp.Key >= materialDefinitions.Count)
                    name = materialDefinitions[defaultMaterial].Id.SubtypeName;
                else
                    name = materialDefinitions[kvp.Key].Id.SubtypeName;

                if (assetNameCount.ContainsKey(name))
                {
                    assetNameCount[name] += kvp.Value;
                }
                else
                {
                    assetNameCount.Add(name, kvp.Value);
                }
            }

            return assetNameCount;
        }

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting
        /// unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~MyVoxelMap()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                m_storage?.Close();
            }
        }

        #endregion

        // mapped from: Sandbox.Game.Entities.MyVoxelBase because it's private.
        // can't be bothered using Reflection.
        public void UpdateVoxelShape(OperationType type, MyShape shape, byte material)
        {
            switch (type)
            {
            case OperationType.Fill:
                // Warning: FillInShape calls MySandboxGame.Invoke()
                MyVoxelGenerator.FillInShape(this, shape, material);
                break;
            case OperationType.Paint:
                MyVoxelGenerator.PaintInShape(this, shape, material);
                break;
            case OperationType.Cut:
                // MySession.Settings.EnableVoxelDestruction has to be enabled for Shapes to be deleted.
                MyVoxelGenerator.CutOutShape(this, shape);
                break;
            }
        }
    }
}
namespace ToolboxTest
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Windows.Media.Media3D;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SEToolbox.Interop;
    using SEToolbox.Interop.Asteroids;
    using SEToolbox.Support;
    using VRageMath;

    [TestClass]
    public class VoxelVolumeTests
    {
        [TestInitialize]
        public void InitTest()
        {
            SpaceEngineersCore.LoadDefinitions();
        }

        [TestMethod, TestCategory("UnitTest")]
        public void VoxelConvertToVolmeticOdd()
        {
            var materials = SpaceEngineersCore.Resources.VoxelMaterialDefinitions;

            var goldMaterial = materials.FirstOrDefault(m => m.Id.SubtypeName.Contains("Gold"));
            Assert.IsNotNull(goldMaterial, "Gold material should exist.");

            var modelFile = @".\TestAssets\Sphere_Gold.3ds";
            var scale = new Size3D(5, 5, 5);
            var rotateTransform = Matrix3D.Identity;
            var traceType = SEToolbox.Interop.Asteroids.TraceType.Odd;
            var traceCount = SEToolbox.Interop.Asteroids.TraceCount.Trace5;
            var traceDirection = SEToolbox.Interop.Asteroids.TraceDirection.XYZ;

            var asteroidFile = @".\TestOutput\test_sphere_odd.vx2";

            var model = MeshHelper.Load(modelFile, ignoreErrors: true);
            var info = new MyVoxelRayTracer.Model(model, scale, rotateTransform, goldMaterial.Index);

            var voxelMap = MyVoxelRayTracer.GenerateVoxelMapFromModel(info, rotateTransform, traceType, traceCount, traceDirection,
                ResetProgress, IncrementProgress, CompleteProgress, default);
            voxelMap.Save(asteroidFile);

            Dictionary<string, long> assetNameCount = voxelMap.RefreshAssets();

            Assert.IsTrue(File.Exists(asteroidFile), "Generated file must exist");

            var voxelFileLength = new FileInfo(asteroidFile).Length;
            Assert.AreEqual(13641, voxelFileLength, "File size must match.");
            Assert.AreEqual(new Vector3I(32, 32, 32), voxelMap.Size, "Voxel Bounding size must match.");
            Assert.AreEqual(new Vector3I(27, 27, 27), voxelMap.BoundingContent.Size + 1, "Voxel Content size must match.");
            Assert.AreEqual(new VRageMath.Vector3D(16, 16, 16), voxelMap.ContentCenter, "Voxel Content Center must match.");

            Assert.AreEqual(2035523, voxelMap.VoxCells, "Voxel cells must match.");

            Assert.AreEqual(1, assetNameCount.Count, "Asset count should be equal.");
            Assert.IsTrue(assetNameCount.ContainsKey(goldMaterial.Id.SubtypeName), $"{goldMaterial.Id.SubtypeName} asset should exist.");
            Assert.AreEqual(2035523, assetNameCount[goldMaterial.Id.SubtypeName], $"{goldMaterial.Id.SubtypeName} count should be equal.");
        }

        [TestMethod, TestCategory("UnitTest")]
        public void VoxelConvertToVolmeticEven()
        {
            var materials = SpaceEngineersCore.Resources.VoxelMaterialDefinitions;

            // Use anything except for stone for testing, as Stone is a default material, and it shouldn't show up in the test.
            var goldMaterial = materials.FirstOrDefault(m => m.Id.SubtypeName.Contains("Gold"));
            Assert.IsNotNull(goldMaterial, "Gold material should exist.");

            var modelFile = @".\TestAssets\Sphere_Gold.3ds";
            var scale = new Size3D(5, 5, 5);
            var rotateTransform = Matrix3D.Identity;
            var traceType = SEToolbox.Interop.Asteroids.TraceType.Even;
            var traceCount = SEToolbox.Interop.Asteroids.TraceCount.Trace5;
            var traceDirection = SEToolbox.Interop.Asteroids.TraceDirection.XYZ;
            var asteroidFile = @".\TestOutput\test_sphere_even.vx2";

            var model = MeshHelper.Load(modelFile, ignoreErrors: true);
            var info = new MyVoxelRayTracer.Model(model, scale, rotateTransform, goldMaterial.Index);

            var voxelMap = MyVoxelRayTracer.GenerateVoxelMapFromModel(info, rotateTransform, traceType, traceCount, traceDirection,
                ResetProgress, IncrementProgress, CompleteProgress, default);
            voxelMap.Save(asteroidFile);

            Dictionary<string, long> assetNameCount = voxelMap.RefreshAssets();

            Assert.IsTrue(File.Exists(asteroidFile), "Generated file must exist");

            var voxelFileLength = new FileInfo(asteroidFile).Length;
            Assert.AreEqual(13691, voxelFileLength, "File size must match.");
            Assert.AreEqual(new Vector3I(32, 32, 32), voxelMap.Size, "Voxel Bounding size must match.");
            Assert.AreEqual(new Vector3I(26, 26, 26), voxelMap.BoundingContent.Size + 1, "Voxel Content size must match.");
            Assert.AreEqual(new VRageMath.Vector3D(15.5, 15.5, 15.5), voxelMap.ContentCenter, "Voxel Content Center must match.");

            Assert.AreEqual(2047046, voxelMap.VoxCells, "Voxel cells must match.");

            Assert.AreEqual(1, assetNameCount.Count, "Asset count should be equal.");
            Assert.IsTrue(assetNameCount.ContainsKey(goldMaterial.Id.SubtypeName), $"{goldMaterial.Id.SubtypeName} asset should exist.");
            Assert.AreEqual(2047046, assetNameCount[goldMaterial.Id.SubtypeName], $"{goldMaterial.Id.SubtypeName} count should be equal.");
        }

        [TestMethod, TestCategory("UnitTest")]
        public void VoxelConvertToVolmeticCancel()
        {
            var materials = SpaceEngineersCore.Resources.VoxelMaterialDefinitions;

            var stoneMaterial = materials.FirstOrDefault(m => m.Id.SubtypeName.Contains("Stone"));
            Assert.IsNotNull(stoneMaterial, "Stone material should exist.");

            var modelFile = @".\TestAssets\Sphere_Gold.3ds";
            var scale = new Size3D(50, 50, 50);
            var rotateTransform = Matrix3D.Identity;
            var traceType = SEToolbox.Interop.Asteroids.TraceType.Odd;
            var traceCount = SEToolbox.Interop.Asteroids.TraceCount.Trace5;
            var traceDirection = SEToolbox.Interop.Asteroids.TraceDirection.XYZ;

            var model = MeshHelper.Load(modelFile, ignoreErrors: true);
            var cts = new CancellationTokenSource();

            // cancel the convertion after 2 seconds.
            var timer = new System.Timers.Timer(2000);
            timer.Elapsed += delegate
            {
                Debug.WriteLine("Cancelling!!!");
                cts.Cancel();
                timer.Stop();
            };
            timer.Start();

            var info = new MyVoxelRayTracer.Model(model, scale, rotateTransform, stoneMaterial.Index);
            var voxelMap = MyVoxelRayTracer.GenerateVoxelMapFromModel(info, rotateTransform, traceType, traceCount, traceDirection,
                ResetProgress, IncrementProgress, CompleteProgress, cts.Token);

            Assert.IsNull(voxelMap, "Asteroid must not exist.");
        }

        // This was set up to test various things, including memory usage in x86. It's no longer required, but still a good test base.
        [Ignore]
        [TestMethod]
        public void VoxelConvertToVolmeticMisc()
        {
            var materials = SpaceEngineersCore.Resources.VoxelMaterialDefinitions;

            var stoneMaterial = materials.FirstOrDefault(m => m.Id.SubtypeName.Contains("Stone"));
            Assert.IsNotNull(stoneMaterial, "Stone material should exist.");

            var goldMaterial = materials.FirstOrDefault(m => m.Id.SubtypeName.Contains("Gold"));
            Assert.IsNotNull(goldMaterial, "Gold material should exist.");

            var silverMaterial = materials.FirstOrDefault(m => m.Id.SubtypeName.Contains("Silver"));
            Assert.IsNotNull(silverMaterial, "Silver material should exist.");

            // Basic test...
            var modelFile = @".\TestAssets\Sphere_Gold.3ds";
            var scale = new Size3D(5, 5, 5);
            var rotateTransform = Matrix3D.Identity;
            var traceType = SEToolbox.Interop.Asteroids.TraceType.Odd;
            var traceCount = SEToolbox.Interop.Asteroids.TraceCount.Trace5;
            var traceDirection = SEToolbox.Interop.Asteroids.TraceDirection.XYZ;

            // Basic model test...
            //var modelFile = @".\TestAssets\TwoSpheres.3ds";
            //var scale = new Size3D(5, 5, 5);

            // Scale test...
            //var modelFile = @".\TestAssets\Sphere_Gold.3ds";
            //var scale = new Size3D(20, 20, 20);
            //var rotateTransform = Matrix3D.Identity;

            // Max Scale test...  will cause an OutOfMemory exception at this scale because MSTest runs in x86.
            //var modelFile = @".\TestAssets\Sphere_Gold.3ds";
            //var scale = new Size3D(120, 120, 120);
            //var rotateTransform = Matrix3D.Identity;

            // Memory test (probably won't load in Space Engineers) ...  will cause an OutOfMemory exception at this scale because MSTest runs in x86.
            //var modelFile = @".\TestAssets\Sphere_Gold.3ds";
            //var scale = new Size3D(200, 200, 200);

            // Complexity test...
            //var modelFile = @".\TestAssets\buddha-fixed-bottom.stl";
            //var scale = new Size3D(0.78, 0.78, 0.78);
            //var rotateTransform = MeshHelper.TransformVector(new Vector3D(0, 0, 0), 180, 0, 0).Value;

            var modelMaterials = new string[] { stoneMaterial.Id.SubtypeName, goldMaterial.Id.SubtypeName, stoneMaterial.Id.SubtypeName, stoneMaterial.Id.SubtypeName, stoneMaterial.Id.SubtypeName, stoneMaterial.Id.SubtypeName, stoneMaterial.Id.SubtypeName, stoneMaterial.Id.SubtypeName, stoneMaterial.Id.SubtypeName, stoneMaterial.Id.SubtypeName, stoneMaterial.Id.SubtypeName, stoneMaterial.Id.SubtypeName, stoneMaterial.Id.SubtypeName, stoneMaterial.Id.SubtypeName, stoneMaterial.Id.SubtypeName, stoneMaterial.Id.SubtypeName, stoneMaterial.Id.SubtypeName, stoneMaterial.Id.SubtypeName, stoneMaterial.Id.SubtypeName, stoneMaterial.Id.SubtypeName, stoneMaterial.Id.SubtypeName, stoneMaterial.Id.SubtypeName, stoneMaterial.Id.SubtypeName, stoneMaterial.Id.SubtypeName, stoneMaterial.Id.SubtypeName };
            var fillerMaterial = silverMaterial.Id.SubtypeName;
            var asteroidFile = @".\TestOutput\test_sphere.vx2";

            var model = MeshHelper.Load(modelFile, ignoreErrors: true);
            var info = new MyVoxelRayTracer.Model(model, scale, rotateTransform, stoneMaterial.Index);

            var voxelMap = MyVoxelRayTracer.GenerateVoxelMapFromModel(info, rotateTransform, traceType, traceCount, traceDirection,
                ResetProgress, IncrementProgress, CompleteProgress, default);
            voxelMap.Save(asteroidFile);

            Assert.IsTrue(File.Exists(asteroidFile), "Generated file must exist");

            var voxelFileLength = new FileInfo(asteroidFile).Length;

            Assert.IsTrue(voxelFileLength > 0, "File must not be empty.");

            Assert.IsTrue(voxelMap.Size.X > 0, "Voxel Size must be greater than zero.");
            Assert.IsTrue(voxelMap.Size.Y > 0, "Voxel Size must be greater than zero.");
            Assert.IsTrue(voxelMap.Size.Z > 0, "Voxel Size must be greater than zero.");

            Assert.IsTrue(voxelMap.BoundingContent.Size.X > 0, "Voxel ContentSize must be greater than zero.");
            Assert.IsTrue(voxelMap.BoundingContent.Size.Y > 0, "Voxel ContentSize must be greater than zero.");
            Assert.IsTrue(voxelMap.BoundingContent.Size.Z > 0, "Voxel ContentSize must be greater than zero.");

            Assert.IsTrue(voxelMap.VoxCells > 0, "voxCells must be greater than zero.");
        }

        #region helpers

        private static double _counter;
        private static double _maximumProgress;
        private static int _percent;
        private static System.Diagnostics.Stopwatch _timer;

        public static void ResetProgress(double initial, double maximumProgress)
        {
            _percent = 0;
            _counter = initial;
            _maximumProgress = maximumProgress;
            _timer = new System.Diagnostics.Stopwatch();
            _timer.Start();

            Debug.WriteLine("{0}%  {1:#,##0}/{2:#,##0}  {3}/Estimating", _percent, _counter, _maximumProgress, _timer.Elapsed);
        }

        public static void IncrementProgress()
        {
            _counter++;

            int p = (int)(_counter / _maximumProgress * 100);

            if (_percent < p)
            {
                var elapsed = _timer.Elapsed;
                var estimate = new TimeSpan(p == 0 ? 0 : (long)((double)elapsed.Ticks / ((double)p / 100f)));
                _percent = p;

                Debug.WriteLine("{0}%  {1:#,##0}/{2:#,##0}  {3}/{4}", _percent, _counter, _maximumProgress, elapsed, estimate);
            }
        }

        public static void CompleteProgress()
        {
            Debug.WriteLine("Complete Step finished.");
        }

        #endregion
    }
}

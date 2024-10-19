namespace SEToolbox.Interop
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Definitions;
    using SEToolbox.Models;
    using SEToolbox.Support;
    using VRage;
    using VRage.Game;
    using VRage.Game.ObjectBuilders.Components;
    using VRage.Game.ObjectBuilders.ComponentSystem;
    using VRage.Voxels;
    using VRage.ObjectBuilders;
    using VRageMath;

    /// <summary>
    /// Contains Extension methods specifically for Keen classes and structures.
    /// </summary>
    public static class SpaceEngineersExtensions
    {
        internal static SerializableVector3I Mirror(this SerializableVector3I vector, Mirror xMirror, int xAxis, Mirror yMirror, int yAxis, Mirror zMirror, int zAxis)
        {
            var newVector = new Vector3I(vector.X, vector.Y, vector.Z);
            switch (xMirror)
            {
                case Support.Mirror.Odd: newVector.X = xAxis - (vector.X - xAxis); break;
                case Support.Mirror.EvenUp: newVector.X = xAxis - (vector.X - xAxis) + 1; break;
                case Support.Mirror.EvenDown: newVector.X = xAxis - (vector.X - xAxis) - 1; break;
            }
            switch (yMirror)
            {
                case Support.Mirror.Odd: newVector.Y = yAxis - (vector.Y - yAxis); break;
                case Support.Mirror.EvenUp: newVector.Y = yAxis - (vector.Y - yAxis) + 1; break;
                case Support.Mirror.EvenDown: newVector.Y = yAxis - (vector.Y - yAxis) - 1; break;
            }
            switch (zMirror)
            {
                case Support.Mirror.Odd: newVector.Z = zAxis - (vector.Z - zAxis); break;
                case Support.Mirror.EvenUp: newVector.Z = zAxis - (vector.Z - zAxis) + 1; break;
                case Support.Mirror.EvenDown: newVector.Z = zAxis - (vector.Z - zAxis) - 1; break;
            }
            return newVector;
        }

        public static double LinearVector(this Vector3 vector)
        {
            return Math.Sqrt(Math.Pow(vector.X, 2) + Math.Pow(vector.Y, 2) + Math.Pow(vector.Z, 2));
        }

        public static Vector3I ToVector3I(this SerializableVector3I vector)
        {
            return new Vector3I(vector.X, vector.Y, vector.Z);
        }

        public static Vector3I RoundToVector3I(this Vector3 vector)
        {
            return new Vector3I((int)Math.Round(vector.X, 0, MidpointRounding.ToEven), (int)Math.Round(vector.Y, 0, MidpointRounding.ToEven), (int)Math.Round(vector.Z, 0, MidpointRounding.ToEven));
        }

        public static Vector3I RoundToVector3I(this Vector3D vector)
        {
            return new Vector3I((int)Math.Round(vector.X, 0, MidpointRounding.ToEven), (int)Math.Round(vector.Y, 0, MidpointRounding.ToEven), (int)Math.Round(vector.Z, 0, MidpointRounding.ToEven));
        }

        public static Vector3 ToVector3(this SerializableVector3I vector)
        {
            return new Vector3(vector.X, vector.Y, vector.Z);
        }

        public static Vector3D ToVector3D(this SerializableVector3I vector)
        {
            return new Vector3D(vector.X, vector.Y, vector.Z);
        }

        public static Vector3 ToVector3(this SerializableVector3 vector)
        {
            return new Vector3(vector.X, vector.Y, vector.Z);
        }

        public static Vector3I SizeInt(this BoundingBox box)
        {
            var size = box.Size;
            return new Vector3I((int)size.X, (int)size.Y, (int)size.Z);
        }

        public static Vector3I SizeInt(this BoundingBoxD box)
        {
            var size = box.Size;
            return new Vector3I((int)size.X, (int)size.Y, (int)size.Z);
        }

        public static System.Windows.Media.Media3D.Vector3D ToVector3D(this SerializableVector3 vector)
        {
            return new System.Windows.Media.Media3D.Vector3D(vector.X, vector.Y, vector.Z);
        }

        public static System.Windows.Media.Media3D.Vector3D ToVector3D(this Vector3 vector)
        {
            return new System.Windows.Media.Media3D.Vector3D(vector.X, vector.Y, vector.Z);
        }

        public static System.Windows.Media.Media3D.Point3D ToPoint3D(this Vector3D vector)
        {
            return new System.Windows.Media.Media3D.Point3D(vector.X, vector.Y, vector.Z);
        }

        public static System.Windows.Media.Media3D.Point3D ToPoint3D(this SerializableVector3 point)
        {
            return new System.Windows.Media.Media3D.Point3D(point.X, point.Y, point.Z);
        }

        public static System.Windows.Media.Media3D.Point3D ToPoint3D(this SerializableVector3D point)
        {
            return new System.Windows.Media.Media3D.Point3D(point.X, point.Y, point.Z);
        }

        public static System.Windows.Point ToPoint(this Vector2 vector)
        {
            return new System.Windows.Point(vector.X, vector.Y);
        }

        public static Vector3 ToVector3(this System.Windows.Media.Media3D.Point3D point)
        {
            return new Vector3((float)point.X, (float)point.Y, (float)point.Z);
        }

        public static Vector3D ToVector3D(this System.Windows.Media.Media3D.Point3D point)
        {
            return new Vector3D(point.X, point.Y, point.Z);
        }

        public static Vector3 ToVector3(this System.Windows.Media.Media3D.Size3D size3D)
        {
            return new Vector3((float)size3D.X, (float)size3D.Y, (float)size3D.Z);
        }

        public static Vector3D ToVector3D(this System.Windows.Media.Media3D.Size3D size3D)
        {
            return new Vector3D(size3D.X, size3D.Y, size3D.Z);
        }

        public static Vector3 ToVector3(this System.Windows.Media.Media3D.Vector3D size3D)
        {
            return new Vector3((float)size3D.X, (float)size3D.Y, (float)size3D.Z);
        }

        public static Vector3D ToVector3D(this System.Windows.Media.Media3D.Vector3D size3D)
        {
            return new Vector3D(size3D.X, size3D.Y, size3D.Z);
        }

        public static Quaternion ToQuaternion(this SerializableBlockOrientation blockOrientation)
        {
            var matrix = Matrix.CreateFromDir(Base6Directions.GetVector(blockOrientation.Forward), Base6Directions.GetVector(blockOrientation.Up));
            return Quaternion.CreateFromRotationMatrix(matrix);
        }

        public static Quaternion ToQuaternion(this MyPositionAndOrientation positionOrientation)
        {
            return Quaternion.CreateFromForwardUp(positionOrientation.Forward, positionOrientation.Up);
        }

        public static QuaternionD ToQuaternionD(this MyPositionAndOrientation positionOrientation)
        {
            return QuaternionD.CreateFromForwardUp(new Vector3D(positionOrientation.Forward), new Vector3D(positionOrientation.Up));
        }

        public static Matrix ToMatrix(this MyPositionAndOrientation positionOrientation)
        {
            return Matrix.CreateFromQuaternion(Quaternion.CreateFromForwardUp(positionOrientation.Forward, positionOrientation.Up));
        }

        public static Matrix ToMatrix(this Quaternion quaternion)
        {
            return Matrix.CreateFromQuaternion(quaternion);
        }

        public static Vector3 Transform(this Vector3 vector, SerializableBlockOrientation orientation)
        {
            var matrix = Matrix.CreateFromDir(Base6Directions.GetVector(orientation.Forward), Base6Directions.GetVector(orientation.Up));
            return Vector3.Transform(vector, matrix);
        }

        public static Vector3D Transform(this Vector3D vector, SerializableBlockOrientation orientation)
        {
            var matrix = MatrixD.CreateFromDir(Base6Directions.GetVector(orientation.Forward), Base6Directions.GetVector(orientation.Up));
            return Vector3D.Transform(vector, matrix);
        }

        public static Vector3I Transform(this SerializableVector3I size, SerializableBlockOrientation orientation)
        {
            var matrix = Matrix.CreateFromDir(Base6Directions.GetVector(orientation.Forward), Base6Directions.GetVector(orientation.Up));
            var rotation = Quaternion.CreateFromRotationMatrix(matrix);
            return Vector3I.Transform(size.ToVector3I(), rotation);
        }

        public static Vector3I Transform(this Vector3I size, SerializableBlockOrientation orientation)
        {
            var matrix = Matrix.CreateFromDir(Base6Directions.GetVector(orientation.Forward), Base6Directions.GetVector(orientation.Up));
            var rotation = Quaternion.CreateFromRotationMatrix(matrix);
            return Vector3I.Transform(size, rotation);
        }

        public static SerializableVector3I Add(this SerializableVector3I size, int value)
        {
            return new SerializableVector3I(size.X + value, size.Y + value, size.Z + value);
        }

        public static Vector3I Add(this Vector3I size, int value)
        {
            return new Vector3I(size.X + value, size.Y + value, size.Z + value);
        }

        public static Vector3I Abs(this Vector3I size)
        {
            return new Vector3I(Math.Abs(size.X), Math.Abs(size.Y), Math.Abs(size.Z));
        }

        public static Vector3D ToVector3D(this Vector3I vector)
        {
            return new Vector3D(vector.X, vector.Y, vector.Z);
        }

        public static BoundingBoxD ToBoundingBoxD(this BoundingBoxI box)
        {
            return new BoundingBoxD(box.Min, box.Max);
        }

        public static SerializableVector3 RoundOff(this SerializableVector3 vector, float roundTo)
        {
            return new SerializableVector3((float)Math.Round(vector.X / roundTo, 0, MidpointRounding.ToEven) * roundTo, (float)Math.Round(vector.Y / roundTo, 0, MidpointRounding.ToEven) * roundTo, (float)Math.Round(vector.Z / roundTo, 0, MidpointRounding.ToEven) * roundTo);
        }

        public static SerializableVector3D RoundOff(this SerializableVector3D vector, float roundTo)
        {
            return new SerializableVector3D(Math.Round(vector.X / roundTo, 0, MidpointRounding.ToEven) * roundTo, Math.Round(vector.Y / roundTo, 0, MidpointRounding.ToEven) * roundTo, Math.Round(vector.Z / roundTo, 0, MidpointRounding.ToEven) * roundTo);
        }

        public static MatrixD ToMatrixD(this QuaternionD value)
        {
            double num = value.X * value.X;
            double num2 = value.Y * value.Y;
            double num3 = value.Z * value.Z;
            double num4 = value.X * value.Y;
            double num5 = value.Z * value.W;
            double num6 = value.Z * value.X;
            double num7 = value.Y * value.W;
            double num8 = value.Y * value.Z;
            double num9 = value.X * value.W;
            MatrixD result = new MatrixD(
                (1.0d - 2.0d * (num2 + num3)),
                (2.0d * (num4 + num5)),
                (2.0d * (num6 - num7)),
                0d,
                (2.0d * (num4 - num5)),
                (1.0d - 2.0d * (num3 + num)),
                (2.0d * (num8 + num9)),
                0d,
                (2.0d * (num6 + num7)),
                (2.0d * (num8 - num9)),
                (1.0d - 2.0d * (num2 + num)),
                0d,
                0d,
                0d,
                0d,
                1d);
            return result;
        }

        public static SerializableVector3 RoundToAxis(this SerializableVector3 vector)
        {
            if (Math.Abs(vector.X) > Math.Abs(vector.Y) && Math.Abs(vector.X) > Math.Abs(vector.Z))
                return new SerializableVector3(Math.Sign(vector.X), 0, 0);

            if (Math.Abs(vector.Y) > Math.Abs(vector.X) && Math.Abs(vector.Y) > Math.Abs(vector.Z))
                return new SerializableVector3(0, Math.Sign(vector.Y), 0);

            if (Math.Abs(vector.Z) > Math.Abs(vector.X) && Math.Abs(vector.Z) > Math.Abs(vector.Y))
                return new SerializableVector3(0, 0, Math.Sign(vector.Z));

            return new SerializableVector3();
        }

        private static decimal Clamp(decimal value, decimal min, decimal max)
        {
            value = (value > max) ? max : value;
            value = (value < min) ? min : value;
            return value;
        }

        /// <summary>
        /// Converts from Keen's HSV stored format to RGB matching the in game color picker palatte.
        /// </summary>
        /// <param name="hsv">the HSV stored value in the range of Hue=X=0.0 to +1.0, Saturation=Y=-1.0 to +1.0, Value=Z=-1.0 to +1.0</param>
        /// <param name="red">converted red value</param>
        /// <param name="green">converted green value</param>
        /// <param name="blue">converted blue value</param>
        /// <remarks>sourced from wikipedia.</remarks>
        private static void FromHsvMaskToPaletteColor(SerializableVector3 hsv, out int red, out int green, out int blue)
        {
            // I've used decimal because of floating point aberation during calculations.
            // This needs to maintain the color accuracy as much as possible.
            // I'm still not happy with this, as the game color palette picker is not exactly representative of the in game colors, 
            // and looking through the calculations, the picker is actually ignoring part of the saturation and value.
            decimal hue = (decimal)hsv.X * 360;
            decimal saturation = Clamp((decimal)hsv.Y + (decimal)MyColorPickerConstants.SATURATION_DELTA, 0, 1);
            decimal value = Clamp((decimal)hsv.Z + (decimal)MyColorPickerConstants.VALUE_DELTA - (decimal)MyColorPickerConstants.VALUE_COLORIZE_DELTA, 0, 1);

            decimal chroma = value * saturation;
            decimal hue1 = hue / 60;
            decimal x = chroma * (1 - Math.Abs(hue1 % 2 - 1));
            decimal r1 = 0;
            decimal g1 = 0;
            decimal b1 = 0;

            if (hue1 < 0)
            {
                // nothing. Need to ignore values less than zero.
            }
            else if (hue1 <= 1)
            {
                r1 = chroma;
                g1 = x;
            }
            else if (hue1 <= 2)
            {
                r1 = x;
                g1 = chroma;
            }
            else if (hue1 <= 3)
            {
                g1 = chroma;
                b1 = x;
            }
            else if (hue1 <= 4)
            {
                g1 = x;
                b1 = chroma;
            }
            else if (hue1 <= 5)
            {
                r1 = x;
                b1 = chroma;
            }
            else if (hue1 <= 6)
            {
                r1 = chroma;
                b1 = x;
            }

            decimal m = value - chroma;

            // Need to round off (not up or truncate down) values to correct for aberration.
            red = (int)Math.Round((r1 + m) * 255);
            green = (int)Math.Round((g1 + m) * 255);
            blue = (int)Math.Round((b1 + m) * 255);
        }

        public static System.Drawing.Color FromHsvMaskToPaletteColor(this SerializableVector3 hsv)
        {
            int r, g, b;
            FromHsvMaskToPaletteColor(hsv, out r, out g, out b);
            return System.Drawing.Color.FromArgb(r, g, b);
        }

        public static System.Windows.Media.Color FromHsvMaskToPaletteMediaColor(this SerializableVector3 hsv)
        {
            int r, g, b;
            FromHsvMaskToPaletteColor(hsv, out r, out g, out b);
            return System.Windows.Media.Color.FromArgb(255, (byte)r, (byte)g, (byte)b);
        }

        /// <summary>
        /// Converts from RGB matching the in game color picker palatte, to Keen's HSV stored format.
        /// </summary>
        /// <param name="r">the System RGB color</param>
        /// <param name="g">the System RGB color</param>
        /// <param name="b">the System RGB color</param>
        /// <returns>the HSV stored value.</returns>
        /// <remarks>sourced from wikipedia</remarks>
        private static SerializableVector3 FromPaletteColorToHsvMask(decimal r, decimal g, decimal b)
        {
            decimal max = Math.Max(r, Math.Max(g, b));
            decimal min = Math.Min(r, Math.Min(g, b));
            decimal chroma = max - min;

            decimal hue1 = 0;

            if (chroma == 0)
                hue1 = 0;
            else if (max == r)
                hue1 = ((g - b) / chroma) % 6;
            else if (max == g)
                hue1 = ((b - r) / chroma) + 2;
            else if (max == b)
                hue1 = ((r - g) / chroma) + 4;

            decimal hue = 60 * hue1;
            decimal value = max;

            decimal saturation = 0;

            if (value != 0)
                saturation = chroma / value;

            return new SerializableVector3((float)hue / 360, (float)saturation - MyColorPickerConstants.SATURATION_DELTA, (float)value - MyColorPickerConstants.VALUE_DELTA + MyColorPickerConstants.VALUE_COLORIZE_DELTA);
        }

        public static SerializableVector3 FromPaletteColorToHsvMask(this System.Drawing.Color color)
        {
            return FromPaletteColorToHsvMask((decimal)color.R / 255, (decimal)color.G / 255, (decimal)color.B / 255);
        }

        public static SerializableVector3 FromPaletteColorToHsvMask(this System.Windows.Media.Color color)
        {
            return FromPaletteColorToHsvMask((decimal)color.R / 255, (decimal)color.G / 255, (decimal)color.B / 255);
        }

        /// <summary>
        /// Returns block size.
        /// </summary>
        /// <remarks>see: http://spaceengineerswiki.com/index.php?title=FAQs
        /// Why are the blocks 0.5 and 2.5 meter blocks?
        /// </remarks>
        /// <param name="cubeSize"></param>
        /// <returns></returns>
        public static float ToLength(this MyCubeSize cubeSize)
        {
            return MyDefinitionManager.Static.GetCubeSize(cubeSize);
        }

        public static MyFixedPoint ToFixedPoint(this decimal value)
        {
            return MyFixedPoint.DeserializeString(value.ToString(CultureInfo.InvariantCulture));
        }

        public static MyFixedPoint ToFixedPoint(this double value)
        {
            return MyFixedPoint.DeserializeString(value.ToString(CultureInfo.InvariantCulture));
        }

        public static MyFixedPoint ToFixedPoint(this float value)
        {
            return MyFixedPoint.DeserializeString(value.ToString(CultureInfo.InvariantCulture));
        }

        public static MyFixedPoint ToFixedPoint(this int value)
        {
            return MyFixedPoint.DeserializeString(value.ToString(CultureInfo.InvariantCulture));
        }

        public static Vector3D? IntersectsRayAt(this BoundingBoxD boundingBox, Vector3D position, Vector3D rayTo)
        {
            var triangles = new int[][] {
                new [] {2,1,0},
                new [] {3,2,0},
                new [] {4,5,6},
                new [] {4,6,7},
                new [] {0,1,5},
                new [] {0,5,4},
                new [] {7,6,2},
                new [] {7,2,3},
                new [] {0,4,7},
                new [] {0,7,3},
                new [] {5,1,2},
                new [] {5,2,6}};

            foreach (var triangle in triangles)
            {
                System.Windows.Media.Media3D.Point3D intersection;
                int norm;

                var c0 = boundingBox.GetCorner(triangle[0]);
                var c1 = boundingBox.GetCorner(triangle[1]);
                var c2 = boundingBox.GetCorner(triangle[2]);

                if (MeshHelper.RayIntersectTriangleRound(c0.ToPoint3D(), c1.ToPoint3D(), c2.ToPoint3D(), position.ToPoint3D(), rayTo.ToPoint3D(), out intersection, out norm))
                {
                    return intersection.ToVector3D();
                }
            }

            return null;
        }

        public static SerializableVector3UByte Transform(this SerializableVector3UByte value, Quaternion rotation)
        {
            var vector = Vector3I.Transform(new Vector3I(value.X - 127, value.Y - 127, value.Z - 127), rotation);
            return new SerializableVector3UByte((byte)(vector.X + 127), (byte)(vector.Y + 127), (byte)(vector.Z + 127));
        }

        public static Vector3D Transform(this Vector3D value, QuaternionD rotation)
        {
            double num = (rotation.X + rotation.X);
            double num2 = (rotation.Y + rotation.Y);
            double num3 = (rotation.Z + rotation.Z);
            double num4 = rotation.W * num;
            double num5 = rotation.W * num2;
            double num6 = rotation.W * num3;
            double num7 = rotation.X * num;
            double num8 = rotation.X * num2;
            double num9 = rotation.X * num3;
            double num10 = rotation.Y * num2;
            double num11 = rotation.Y * num3;
            double num12 = rotation.Z * num3;
            double x = value.X * (1.0 - num10 - num12) + value.Y * (num8 - num6) + value.Z * (num9 + num5);
            double y = value.X * (num8 + num6) + value.Y * (1.0 - num7 - num12) + value.Z * (num11 - num4);
            double z = value.X * (num9 - num5) + value.Y * (num11 + num4) + value.Z * (1.0 - num7 - num10);
            Vector3D result = new Vector3D(x, y, z);
            return result;
        }

        public static int Read7BitEncodedInt(this BinaryReader reader)
        {
            int num = 0;
            int num2 = 0;
            while (num2 != 35)
            {
                byte b = reader.ReadByte();
                num |= (int)(b & 127) << num2;
                num2 += 7;
                if ((b & 128) == 0)
                {
                    return num;
                }
            }
            return -1;
        }

        //public static ObservableCollection<InventoryEditorModel> GetInventory(this MyObjectBuilder_EntityBase objectBuilderBase)
        //{
        //    var inventoryEditors = new ObservableCollection<InventoryEditorModel>();

        //    if (objectBuilderBase.ComponentContainer != null)
        //    {
        //        var inventoryBase = objectBuilderBase.ComponentContainer.Components.FirstOrDefault(e => e.TypeId == "MyInventoryBase");

        //        if (inventoryBase != null)
        //        {
        //            var singleInventory = inventoryBase.Component as MyObjectBuilder_Inventory;
        //            if (singleInventory != null)
        //            {
        //                var iem = ParseInventory(singleInventory);
        //                if (iem != null)
        //                    inventoryEditors.Add(iem);
        //            }

        //            var aggregate = inventoryBase.Component as MyObjectBuilder_InventoryAggregate;
        //            if (aggregate != null)
        //                foreach (var field in aggregate.Inventories)
        //                {
        //                    var iem = ParseInventory(field as MyObjectBuilder_Inventory);
        //                    if (iem != null)
        //                        inventoryEditors.Add(iem);
        //                }
        //        }
        //    }
        //    return inventoryEditors;
        //}

        public static List<MyObjectBuilder_Character> GetHierarchyCharacters(this MyObjectBuilder_CubeBlock cube)
        {
            var list = new List<MyObjectBuilder_Character>();

            if (cube is not MyObjectBuilder_Cockpit cockpit)
                return list;

            var hierarchyBase = cockpit.ComponentContainer?.Components
                .FirstOrDefault(e => e.TypeId == "MyHierarchyComponentBase")?.Component as MyObjectBuilder_HierarchyComponentBase;

            if (hierarchyBase != null)
            {
                list.AddRange(hierarchyBase.Children.Where(e => e is MyObjectBuilder_Character).Cast<MyObjectBuilder_Character>());
            }

            return list;
        }

        /// <summary>
        /// Removes all sign of a pilot/characrter from a cockpit cube.
        /// </summary>
        /// <param name="cockpit">The specific cube.</param>
        /// <param name="character">Specific character to remove, if required, otherwise ANY chararcter will be removed.</param>
        /// <returns>Returns true if a character was removed.</returns>
        public static bool RemoveHierarchyCharacter(this MyObjectBuilder_Cockpit cockpit, MyObjectBuilder_Character character = null)
        {
            bool retValue = false;

            MyObjectBuilder_ComponentContainer.ComponentData hierarchyComponentBase = cockpit.ComponentContainer?.Components?.FirstOrDefault(e => e.TypeId == "MyHierarchyComponentBase");
            var hierarchyBase = hierarchyComponentBase?.Component as MyObjectBuilder_HierarchyComponentBase;
            if (hierarchyBase != null && hierarchyBase.Children.Count > 0)
            {
                for (int i = 0; i < hierarchyBase.Children.Count; i++)
                {
                    if (character != null && hierarchyBase.Children[i] == character)
                    {
                        retValue = true;
                        hierarchyBase.Children.RemoveAt(i);
                        i--;
                        break;
                    }

                    if (character == null && hierarchyBase.Children[i] is MyObjectBuilder_Character)
                    {
                        retValue = true;
                        hierarchyBase.Children.RemoveAt(i);
                        i--;
                    }
                }

                if (hierarchyBase.Children.Count == 0)
                {
                    cockpit.ComponentContainer.Components.Remove(hierarchyComponentBase);
                }
            }

            if (retValue)
            {
                cockpit.ClearPilotAndAutopilot();
                cockpit.PilotRelativeWorld = null; // This should also clear Pilot.
                cockpit.Pilot = null;
            }

            return retValue;
        }

        /// <summary>
        /// Remove all pilots, co-pilots and any other character entities from consoles, cockpits and passenger seats.
        /// </summary>
        public static void RemoveHierarchyCharacter(this MyObjectBuilder_CubeGrid cubeGrid)
        {
            cubeGrid.CubeBlocks.Where(c => c.TypeId == SpaceEngineersTypes.Cockpit).Select(c =>
            {
                ((MyObjectBuilder_Cockpit)c).RemoveHierarchyCharacter();
                return c;
            }).ToArray();
        }

        public static ObservableCollection<InventoryEditorModel> GetInventory(this MyObjectBuilder_ComponentContainer componentContainer, MyCubeBlockDefinition definition = null)
        {
            var inventoryEditors = new ObservableCollection<InventoryEditorModel>();

            if (componentContainer != null)
            {
                var inventoryBase = componentContainer.Components.FirstOrDefault(e => e.TypeId == "MyInventoryBase");

                if (inventoryBase != null)
                {
                    var singleInventory = inventoryBase.Component as MyObjectBuilder_Inventory;
                    if (singleInventory != null)
                    {
                        var iem = ParseInventory(singleInventory, definition);
                        if (iem != null)
                            inventoryEditors.Add(iem);
                    }

                    var aggregate = inventoryBase.Component as MyObjectBuilder_InventoryAggregate;
                    if (aggregate != null)
                        foreach (var field in aggregate.Inventories)
                        {
                            var iem = ParseInventory(field as MyObjectBuilder_Inventory, definition);
                            if (iem != null)
                                inventoryEditors.Add(iem);
                        }
                }
            }
            return inventoryEditors;
        }

        private static InventoryEditorModel ParseInventory(MyObjectBuilder_Inventory inventory, MyCubeBlockDefinition definition = null)
        {
            if (inventory == null)
                return null;
            float volumeMultiplier = 1f; // Unsure if there should be a default of 1 if there isn't a InventorySize defined.

            if (definition == null)
                volumeMultiplier = 0.4f;
            else
            {
                var definitionType = definition.GetType();
                var invSizeField = definitionType.GetField("InventorySize");
                var inventoryMaxVolumeField = definitionType.GetField("InventoryMaxVolume");
                if (invSizeField != null)
                {
                    var invSize = (Vector3)invSizeField.GetValue(definition);
                    volumeMultiplier = invSize.X * invSize.Y * invSize.Z;
                }
                if (inventoryMaxVolumeField != null)
                {
                    var maxSize = (float)inventoryMaxVolumeField.GetValue(definition);
                    volumeMultiplier = MathHelper.Min(volumeMultiplier, maxSize);
                }
            }

            var settings = SpaceEngineersCore.WorldResource.Checkpoint.Settings;
            return new InventoryEditorModel(inventory, volumeMultiplier * 1000 * settings.InventorySizeMultiplier, null) { Name = inventory.InventoryFlags.ToString(), IsValid = true };
        }

        public static List<MyGasProperties> GetGasDefinitions(this MyDefinitionManager definitionManager)
        {
            return definitionManager.GetAllDefinitions().Where(e => e.Id.TypeId == typeof(VRage.Game.ObjectBuilders.Definitions.MyObjectBuilder_GasProperties)).Cast<MyGasProperties>().ToList();
        }

        public static MyDefinitionBase GetDefinition(this MyDefinitionManager definitionManager, MyObjectBuilderType typeId, string subTypeId)
        {
            return definitionManager.GetAllDefinitions().FirstOrDefault(e => e.Id.TypeId == typeId && e.Id.SubtypeName == subTypeId);
        }

        public static string GetVoxelDisplayTexture(this MyVoxelMaterialDefinition voxelMaterialDefinition)
        {
            string texture = null;

            texture = voxelMaterialDefinition.RenderParams.TextureSets[0].ColorMetalXZnY;

            if (texture == null)
                texture = voxelMaterialDefinition.RenderParams.TextureSets[0].NormalGlossXZnY;

            if (texture == null)
                // The VoxelHandPreview texture is oddly shaped, and not suitable for SEToolbox.
                // It is a texture of last resort.
                texture = voxelMaterialDefinition.VoxelHandPreview;

            return texture;
        }

        public static void GetMaterialContent(this VRage.Game.Voxels.IMyStorage self, ref Vector3I voxelCoords, out byte material, out byte content)
        {
            MyStorageData myStorageData = new MyStorageData(MyStorageDataTypeFlags.ContentAndMaterial);
            myStorageData.Resize(Vector3I.One);
            myStorageData.ClearMaterials(0);
            self.ReadRange(myStorageData, MyStorageDataTypeFlags.ContentAndMaterial, 0, voxelCoords, voxelCoords);

            material = myStorageData.Material(0);
            content = myStorageData.Content(0);
        }

        public static int Max(int a, int b, int c, int d)
        {
            int abMax = a > b ? a : b;
            int cdMax = c > d ? c : d;
            return abMax > cdMax ? abMax : cdMax;
        }
    }
}

namespace SEToolbox.Interop.Models
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    using BulletXNA.BulletCollision;
    using VRage.Import;
    using VRageMath;
    using VRageMath.PackedVector;
    using VRageRender.Animations;
    using VRageRender.Import;

    public static class MyModel
    {
        #region LoadModelData

        public static Dictionary<string, object> LoadModelData(string filename)
        {
            var model = new MyModelImporter();
            model.ImportData(filename);
            return model.GetTagData();
        }

        /// <summary>
        /// Load Model Data
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static Dictionary<string, object> LoadCustomModelData(string filename)
        {
            var data = new Dictionary<string, object>();

            using (var stream = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                var reader = new BinaryReader(stream);
                try
                {
                    LoadTagData(reader, data);
                }
                catch
                {
                    // Ignore errors
                }
            }

            return data;
        }

        #endregion

        #region SaveModelData

        public static void SaveModelData(string filename, Dictionary<string, object> data)
        {
            var methods = typeof(MyModel).GetMethods(BindingFlags.Static | BindingFlags.NonPublic);

            using (var fileStream = new FileStream(filename, FileMode.Create))
            {
                var writer = new BinaryWriter(fileStream);
                foreach (var kvp in data)
                {
                    var method = methods.FirstOrDefault(m => m.Name.Equals("ExportData") && m.GetParameters().Length > 2 && m.GetParameters()[2].ParameterType == kvp.Value.GetType());

                    if (method != null)
                    {
                        method.Invoke(null, new[] { writer, kvp.Key, kvp.Value });
                    }
                    else
                    {
                        method = methods.FirstOrDefault(m => m.Name.Equals("ExportData") && m.GetParameters().Length > 2 && m.GetParameters()[2].ParameterType == kvp.Value.GetType().MakeByRefType());
                        if (method != null)
                        {
                            method.Invoke(null, new[] { writer, kvp.Key, kvp.Value });
                        }
                    }
                }
            }
        }

        #endregion

        #region Write helpers

        private static void WriteBone(this BinaryWriter writer, ref MyModelBone bone)
        {
            writer.Write(bone.Name);
            writer.Write(bone.Parent);
            WriteMatrix(writer, ref bone.Transform);
        }

        /// <summary>
        /// WriteTag
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="tagName"></param>
        private static void WriteTag(this BinaryWriter writer, string tagName)
        {
            writer.Write(tagName);
        }

        /// <summary>
        /// WriteVector3
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="vector"></param>
        private static void WriteVector3(this BinaryWriter writer, ref Vector3 vector)
        {
            writer.Write(vector.X);
            writer.Write(vector.Y);
            writer.Write(vector.Z);
        }

        /// <summary>
        /// WriteVector4
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="vector"></param>
        private static void WriteVector4(this BinaryWriter writer, ref Vector4 vector)
        {
            writer.Write(vector.X);
            writer.Write(vector.Y);
            writer.Write(vector.Z);
            writer.Write(vector.W);
        }

        /// <summary>
        /// WriteVector3I
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="vector"></param>
        private static void WriteVector3I(this BinaryWriter writer, ref Vector3I vector)
        {
            writer.Write(vector.X);
            writer.Write(vector.Y);
            writer.Write(vector.Z);
        }

        /// <summary>
        /// WriteVector4I
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="vector"></param>
        private static void WriteVector4I(this BinaryWriter writer, ref Vector4I vector)
        {
            writer.Write(vector.X);
            writer.Write(vector.Y);
            writer.Write(vector.Z);
            writer.Write(vector.W);
        }

        /// <summary>
        /// WriteVector2
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="vector"></param>
        private static void WriteVector2(this BinaryWriter writer, ref Vector2 vector)
        {
            writer.Write(vector.X);
            writer.Write(vector.Y);
        }

        /// <summary>
        /// WriteMatrix
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="matrix"></param>
        private static void WriteMatrix(this BinaryWriter writer, ref Matrix matrix)
        {
            writer.Write(matrix.M11);
            writer.Write(matrix.M12);
            writer.Write(matrix.M13);
            writer.Write(matrix.M14);

            writer.Write(matrix.M21);
            writer.Write(matrix.M22);
            writer.Write(matrix.M23);
            writer.Write(matrix.M24);

            writer.Write(matrix.M31);
            writer.Write(matrix.M32);
            writer.Write(matrix.M33);
            writer.Write(matrix.M34);

            writer.Write(matrix.M41);
            writer.Write(matrix.M42);
            writer.Write(matrix.M43);
            writer.Write(matrix.M44);
        }

        /// <summary>
        /// Write HalfVector4
        /// </summary>
        private static void WriteHalfVector4(this BinaryWriter writer, ref HalfVector4 value)
        {
            writer.Write(value.PackedValue);
        }

        /// <summary>
        /// Write HalfVector2
        /// </summary>
        private static void WriteHalfVector2(this BinaryWriter writer, ref HalfVector2 value)
        {
            writer.Write(value.PackedValue);
        }

        /// <summary>
        /// Write Byte4
        /// </summary>
        private static void WriteByte4(this BinaryWriter writer, ref Byte4 val)
        {
            writer.Write(val.PackedValue);
        }

        #endregion

        #region Export Data packers

        private static bool ExportDataPackedAsHV4(this BinaryWriter writer, string tagName, Vector3[] vectorArray)
        {
            WriteTag(writer, tagName);

            if (vectorArray == null)
            {
                writer.Write(0);
                return true;
            }

            writer.Write(vectorArray.Length);
            foreach (var vectorVal in vectorArray)
            {
                var v = vectorVal;
                var vector = VF_Packer.PackPosition(ref v);
                WriteHalfVector4(writer, ref vector);
            }

            return true;
        }

        private static bool ExportDataPackedAsHV2(this BinaryWriter writer, string tagName, Vector2[] vectorArray)
        {
            WriteTag(writer, tagName);

            if (vectorArray == null)
            {
                writer.Write(0);
                return true;
            }

            writer.Write(vectorArray.Length);
            foreach (var vectorVal in vectorArray)
            {
                var vector = new HalfVector2(vectorVal);
                WriteHalfVector2(writer, ref vector);
            }

            return true;
        }

        private static bool ExportDataPackedAsB4(this BinaryWriter writer, string tagName, Vector3[] vectorArray)
        {
            WriteTag(writer, tagName);

            if (vectorArray == null)
            {
                writer.Write(0);
                return true;
            }

            writer.Write(vectorArray.Length);
            foreach (var vectorVal in vectorArray)
            {
                var v = vectorVal;
                var vector = new Byte4();
                vector.PackedValue = VF_Packer.PackNormal(ref v);
                WriteByte4(writer, ref vector);
            }

            return true;
        }

        private static bool ExportData(this BinaryWriter writer, string tagName, HalfVector4[] vectorArray)
        {
            WriteTag(writer, tagName);

            if (vectorArray == null)
            {
                writer.Write(0);
                return true;
            }

            writer.Write(vectorArray.Length);
            foreach (var vectorVal in vectorArray)
            {
                writer.Write(vectorVal.PackedValue);
            }

            return true;
        }

        private static bool ExportData(this BinaryWriter writer, string tagName, HalfVector2[] vectorArray)
        {
            WriteTag(writer, tagName);

            if (vectorArray == null)
            {
                writer.Write(0);
                return true;
            }

            writer.Write(vectorArray.Length);
            foreach (var vectorVal in vectorArray)
            {
                writer.Write(vectorVal.PackedValue);
            }

            return true;
        }

        private static bool ExportData(this BinaryWriter writer, string tagName, Byte4[] vectorArray)
        {
            WriteTag(writer, tagName);

            if (vectorArray == null)
            {
                writer.Write(0);
                return true;
            }

            writer.Write(vectorArray.Length);
            foreach (var vectorVal in vectorArray)
            {
                writer.Write(vectorVal.PackedValue);
            }

            return true;
        }

        /// <summary>
        /// ExportData
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="tagName"></param>
        /// <param name="vectorArray"></param>
        /// <returns></returns>
        private static bool ExportData(this BinaryWriter writer, string tagName, Vector3[] vectorArray)
        {
            if (vectorArray == null)
                return true;

            WriteTag(writer, tagName);
            writer.Write(vectorArray.Length);
            foreach (var vectorVal in vectorArray)
            {
                var vector = vectorVal;
                WriteVector3(writer, ref vector);
            }

            return true;
        }

        /// <summary>
        /// ExportData
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="tagName"></param>
        /// <param name="vectorArray"></param>
        /// <returns></returns>
        private static bool ExportData(this BinaryWriter writer, string tagName, Vector4[] vectorArray)
        {
            if (vectorArray == null)
                return true;

            WriteTag(writer, tagName);
            writer.Write(vectorArray.Length);
            foreach (var vectorVal in vectorArray)
            {
                var vector = vectorVal;
                WriteVector4(writer, ref vector);
            }

            return true;
        }

        /// <summary>
        /// ExportData
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="tagName"></param>
        /// <param name="vectorArray"></param>
        /// <returns></returns>
        private static bool ExportData(this BinaryWriter writer, string tagName, Vector3I[] vectorArray)
        {
            if (vectorArray == null)
                return true;

            WriteTag(writer, tagName);
            writer.Write(vectorArray.Length);
            foreach (var vectorVal in vectorArray)
            {
                var vector = vectorVal;
                WriteVector3I(writer, ref vector);
            }

            return true;
        }

        /// <summary>
        /// ExportData
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="tagName"></param>
        /// <param name="vectorArray"></param>
        /// <returns></returns>
        private static bool ExportData(this BinaryWriter writer, string tagName, Vector4I[] vectorArray)
        {
            if (vectorArray == null)
                return true;

            WriteTag(writer, tagName);
            writer.Write(vectorArray.Length);
            foreach (var vectorVal in vectorArray)
            {
                var vector = vectorVal;
                WriteVector4I(writer, ref vector);
            }

            return true;
        }

        /// <summary>
        /// ExportData
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="tagName"></param>
        /// <param name="matrixArray"></param>
        /// <returns></returns>
        private static bool ExportData(this BinaryWriter writer, string tagName, Matrix[] matrixArray)
        {
            if (matrixArray == null)
                return true;

            WriteTag(writer, tagName);
            writer.Write(matrixArray.Length);
            foreach (var matVal in matrixArray)
            {
                var mat = matVal;
                WriteMatrix(writer, ref mat);
            }

            return true;
        }

        /// <summary>
        /// ExportData
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="tagName"></param>
        /// <param name="vectorArray"></param>
        /// <returns></returns>
        private static bool ExportData(this BinaryWriter writer, string tagName, Vector2[] vectorArray)
        {
            WriteTag(writer, tagName);

            if (vectorArray == null)
            {
                writer.Write(0);
                return true;
            }

            writer.Write(vectorArray.Length);
            foreach (var vectorVal in vectorArray)
            {
                var vector = vectorVal;
                WriteVector2(writer, ref vector);
            }

            return true;
        }

        /// <summary>
        /// ExportData
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="tagName"></param>
        /// <param name="stringArrayay"></param>
        /// <returns></returns>
        private static bool ExportData(this BinaryWriter writer, string tagName, string[] stringArrayay)
        {
            WriteTag(writer, tagName);

            if (stringArrayay == null)
            {
                writer.Write(0);
                return true;
            }

            writer.Write(stringArrayay.Length);
            foreach (var sVal in stringArrayay)
                writer.Write(sVal);

            return true;
        }


        /// <summary>
        /// ExportData
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="tagName"></param>
        /// <param name="intArray"></param>
        /// <returns></returns>
        private static bool ExportData(this BinaryWriter writer, string tagName, int[] intArray)
        {
            WriteTag(writer, tagName);

            if (intArray == null)
            {
                writer.Write(0);
                return true;
            }

            writer.Write(intArray.Length);
            foreach (var iVal in intArray)
                writer.Write(iVal);

            return true;
        }

        private static bool ExportData(this BinaryWriter writer, string tagName, byte[] byteArray)
        {
            WriteTag(writer, tagName);

            if (byteArray == null)
            {
                writer.Write(0);
                return true;
            }

            writer.Write(byteArray.Length);
            writer.Write(byteArray);
            return true;
        }

        private static bool ExportData(this BinaryWriter writer, string tagName, MyModelInfo modelInfo)
        {
            WriteTag(writer, tagName);

            writer.Write(modelInfo.TrianglesCount);
            writer.Write(modelInfo.VerticesCount);
            WriteVector3(writer, ref modelInfo.BoundingBoxSize);
            return true;
        }


        /// <summary>
        /// ExportData
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="tagName"></param>
        /// <param name="boundingBox"></param>
        /// <returns></returns>
        private static bool ExportData(this BinaryWriter writer, string tagName, ref BoundingBox boundingBox)
        {
            WriteTag(writer, tagName);
            WriteVector3(writer, ref boundingBox.Min);
            WriteVector3(writer, ref boundingBox.Max);
            return true;
        }

        /// <summary>
        /// ExportData
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="tagName"></param>
        /// <param name="boundingSphere"></param>
        /// <returns></returns>
        private static bool ExportData(this BinaryWriter writer, string tagName, ref BoundingSphere boundingSphere)
        {
            WriteTag(writer, tagName);
            WriteVector3(writer, ref boundingSphere.Center);
            writer.Write(boundingSphere.Radius);
            return true;
        }

        /// <summary>
        /// ExportData
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="tagName"></param>
        /// <param name="bvh"></param>
        /// <returns></returns>
        private static bool ExportData(this BinaryWriter writer, string tagName, ref GImpactQuantizedBvh bvh)
        {
            WriteTag(writer, tagName);

            var buffer = bvh.Save();

            writer.Write(buffer.Length);
            writer.Write(bvh.Save());

            return true;
        }

        private static bool ExportData(this BinaryWriter writer, string tagName, ref ModelAnimations animations)
        {
            WriteTag(writer, tagName);

            writer.Write(animations.Clips.Count);

            foreach (var clip in animations.Clips)
            {
                writer.Write(clip.Name);
                writer.Write(clip.Duration);
                writer.Write(clip.Bones.Count);

                foreach (var bone in clip.Bones)
                {
                    writer.Write(bone.Name);
                    writer.Write(bone.Keyframes.Length);

                    for (int i = 0; i < bone.Keyframes.Length; i++)
                    {
                        ref var keyframe = ref bone.Keyframes[i];
                        var rotation = keyframe.Rotation.ToVector4();

                        writer.Write(keyframe.Time);
                        writer.WriteVector4(ref rotation);
                        writer.WriteVector3(ref keyframe.Translation);
                    }
                }
            }

            writer.Write(animations.Skeleton.Count);

            foreach (var skeleton in animations.Skeleton)
                writer.Write(skeleton);

            return true;
        }

        private static bool ExportData(this BinaryWriter writer, string tagName, MyModelBone[] boneArray)
        {
            WriteTag(writer, tagName);
            writer.Write(boneArray.Length);

            foreach (var boneVal in boneArray)
            {
                var bone = boneVal;
                WriteBone(writer, ref bone);
            }

            return true;
        }

        private static bool ExportData(this BinaryWriter writer, string tagName, MyLODDescriptor[] lodArray)
        {
            WriteTag(writer, tagName);
            writer.Write(lodArray.Length);

            foreach (var lodVal in lodArray)
            {
                lodVal.Write(writer);
            }

            return true;
        }

        /// <summary>
        /// ExportData
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="tagName"></param>
        /// <param name="dict"></param>
        /// <returns></returns>
        private static bool ExportData(this BinaryWriter writer, string tagName, Dictionary<string, Matrix> dict)
        {
            WriteTag(writer, tagName);
            writer.Write(dict.Count);
            foreach (var pair in dict)
            {
                writer.Write(pair.Key);
                var mat = pair.Value;
                WriteMatrix(writer, ref mat);
            }
            return true;
        }

        /// <summary>
        /// ExportData
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="tagName"></param>
        /// <param name="dict"></param>
        /// <returns></returns>
        private static bool ExportData(this BinaryWriter writer, string tagName, Dictionary<int, MyMeshPartInfo> dict)
        {
            WriteTag(writer, tagName);
            writer.Write(dict.Count);
            foreach (var pair in dict)
            {
                var meshInfo = pair.Value;
                meshInfo.Export(writer);
            }

            return true;
        }

        private static bool ExportData(this BinaryWriter writer, string tagName, List<MyMeshPartInfo> list)
        {
            WriteTag(writer, tagName);
            writer.Write(list.Count);
            foreach (var meshInfo in list)
            {
                meshInfo.Export(writer);
            }

            return true;
        }

        /// <summary>
        /// ExportData
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="tagName"></param>
        /// <param name="dict"></param>
        /// <returns></returns>
        private static bool ExportData(this BinaryWriter writer, string tagName, Dictionary<string, MyModelDummy> dict)
        {
            WriteTag(writer, tagName);
            writer.Write(dict.Count);
            foreach (var pair in dict)
            {
                writer.Write(pair.Key);
                var mat = pair.Value.Matrix;
                WriteMatrix(writer, ref mat);

                writer.Write(pair.Value.CustomData.Count);
                foreach (var customDataPair in pair.Value.CustomData)
                {
                    writer.Write(customDataPair.Key);
                    writer.Write(customDataPair.Value.ToString());
                }
            }
            return true;
        }

        /// <summary>
        /// ExportFloat
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="tagName"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private static bool ExportData(this BinaryWriter writer, string tagName, float value)
        {
            WriteTag(writer, tagName);
            writer.Write(value);
            return true;
        }

        /// <summary>
        /// ExportFloat
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="tagName"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private static bool ExportData(this BinaryWriter writer, string tagName, bool value)
        {
            WriteTag(writer, tagName);
            writer.Write(value);
            return true;
        }

        #endregion

        #region read helpers

        /// <summary>
        /// Read HalfVector4
        /// </summary>
        private static HalfVector4 ReadHalfVector4(BinaryReader reader)
        {
            return new HalfVector4 { PackedValue = reader.ReadUInt64() };
        }

        /// <summary>
        /// Read HalfVector2
        /// </summary>
        private static HalfVector2 ReadHalfVector2(BinaryReader reader)
        {
            return new HalfVector2 { PackedValue = reader.ReadUInt32() };
        }

        /// <summary>
        /// Read Byte4
        /// </summary>
        private static Byte4 ReadByte4(BinaryReader reader)
        {
            return new Byte4 { PackedValue = reader.ReadUInt32() };
        }

        /// <summary>
        /// ReadVector3
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        private static Vector3 ReadVector3(BinaryReader reader)
        {
            Vector3 vector;
            vector.X = reader.ReadSingle();
            vector.Y = reader.ReadSingle();
            vector.Z = reader.ReadSingle();
            return vector;
        }

        /// <summary>
        /// ReadVector3I
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        private static Vector3I ReadVector3I(BinaryReader reader)
        {
            Vector3I vector;
            vector.X = reader.ReadInt32();
            vector.Y = reader.ReadInt32();
            vector.Z = reader.ReadInt32();
            return vector;
        }

        /// <summary>
        /// ReadVector4
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        private static Vector4 ReadVector4(BinaryReader reader)
        {
            Vector4 vector;
            vector.X = reader.ReadSingle();
            vector.Y = reader.ReadSingle();
            vector.Z = reader.ReadSingle();
            vector.W = reader.ReadSingle();
            return vector;
        }

        /// <summary>
        /// ReadVector4I
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        private static Vector4I ReadVector4I(BinaryReader reader)
        {
            Vector4I vector;
            vector.X = reader.ReadInt32();
            vector.Y = reader.ReadInt32();
            vector.Z = reader.ReadInt32();
            vector.W = reader.ReadInt32();
            return vector;
        }

        /// <summary>
        /// ReadVector2
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        private static Vector2 ReadVector2(BinaryReader reader)
        {
            Vector2 vector;
            vector.X = reader.ReadSingle();
            vector.Y = reader.ReadSingle();
            return vector;
        }

        /// <summary>
        /// Read array of Vector3
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        private static Vector3[] ReadArrayOfVector3(BinaryReader reader)
        {
            var nCount = reader.ReadInt32();
            var vectorArray = new Vector3[nCount];
            for (var i = 0; i < nCount; ++i)
            {
                vectorArray[i] = ReadVector3(reader);
            }

            return vectorArray;
        }

        /// <summary>
        /// Read array of Vector3I
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        private static Vector3I[] ReadArrayOfVector3I(BinaryReader reader)
        {
            var nCount = reader.ReadInt32();
            var vectorArray = new Vector3I[nCount];
            for (var i = 0; i < nCount; ++i)
            {
                vectorArray[i] = ReadVector3I(reader);
            }

            return vectorArray;
        }

        /// <summary>
        /// Read array of Vector4
        /// </summary>
        private static Vector4[] ReadArrayOfVector4(BinaryReader reader)
        {
            var nCount = reader.ReadInt32();
            var vectorArray = new Vector4[nCount];

            for (var i = 0; i < nCount; ++i)
            {
                vectorArray[i] = ReadVector4(reader);
            }

            return vectorArray;
        }

        /// <summary>
        /// Read array of HalfVector4
        /// </summary>
        private static HalfVector4[] ReadArrayOfHalfVector4(BinaryReader reader)
        {
            var nCount = reader.ReadInt32();
            var vectorArray = new HalfVector4[nCount];

            for (var i = 0; i < nCount; ++i)
            {
                vectorArray[i] = ReadHalfVector4(reader);
            }

            return vectorArray;
        }

        /// <summary>
        /// Read array of Byte4
        /// </summary>
        private static Byte4[] ReadArrayOfByte4(BinaryReader reader)
        {
            var nCount = reader.ReadInt32();
            var vectorArray = new Byte4[nCount];

            for (var i = 0; i < nCount; ++i)
            {
                vectorArray[i] = ReadByte4(reader);
            }

            return vectorArray;
        }

        /// <summary>
        /// Read array of HalfVector2
        /// </summary>
        private static HalfVector2[] ReadArrayOfHalfVector2(BinaryReader reader)
        {
            var nCount = reader.ReadInt32();
            var vectorArray = new HalfVector2[nCount];

            for (var i = 0; i < nCount; ++i)
            {
                vectorArray[i] = ReadHalfVector2(reader);
            }

            return vectorArray;
        }


        /// <summary>
        /// Read array of Vector2
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        private static Vector2[] ReadArrayOfVector2(BinaryReader reader)
        {
            var nCount = reader.ReadInt32();
            var vectorArray = new Vector2[nCount];
            for (var i = 0; i < nCount; ++i)
            {
                vectorArray[i] = ReadVector2(reader);
            }

            return vectorArray;
        }

        /// <summary>
        /// Read array of String
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        private static string[] ReadArrayOfString(BinaryReader reader)
        {
            var nCount = reader.ReadInt32();
            var stringArray = new string[nCount];
            for (var i = 0; i < nCount; ++i)
            {
                stringArray[i] = reader.ReadString();
            }

            return stringArray;
        }

        /// <summary>
        /// ReadBoundingBox
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        private static BoundingBox ReadBoundingBox(BinaryReader reader)
        {
            BoundingBox boundingBox;
            boundingBox.Min = ReadVector3(reader);
            boundingBox.Max = ReadVector3(reader);
            return boundingBox;

        }

        /// <summary>
        /// ReadBoundingSphere
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        private static BoundingSphere ReadBoundingSphere(BinaryReader reader)
        {
            BoundingSphere boundingSphere;
            boundingSphere.Center = ReadVector3(reader);
            boundingSphere.Radius = reader.ReadSingle();
            return boundingSphere;
        }

        /// <summary>
        /// ReadMatrix
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        private static Matrix ReadMatrix(BinaryReader reader)
        {
            Matrix matrix;

            matrix.M11 = reader.ReadSingle();
            matrix.M12 = reader.ReadSingle();
            matrix.M13 = reader.ReadSingle();
            matrix.M14 = reader.ReadSingle();

            matrix.M21 = reader.ReadSingle();
            matrix.M22 = reader.ReadSingle();
            matrix.M23 = reader.ReadSingle();
            matrix.M24 = reader.ReadSingle();

            matrix.M31 = reader.ReadSingle();
            matrix.M32 = reader.ReadSingle();
            matrix.M33 = reader.ReadSingle();
            matrix.M34 = reader.ReadSingle();

            matrix.M41 = reader.ReadSingle();
            matrix.M42 = reader.ReadSingle();
            matrix.M43 = reader.ReadSingle();
            matrix.M44 = reader.ReadSingle();

            return matrix;
        }

        /// <summary>
        /// ReadMeshParts
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        private static List<MyMeshPartInfo> ReadMeshParts(BinaryReader reader)
        {
            var list = new List<MyMeshPartInfo>();
            var nCount = reader.ReadInt32();
            for (var i = 0; i < nCount; ++i)
            {
                var meshPart = new MyMeshPartInfo();
                meshPart.Import(reader, 0); // TODO: test version detail
                list.Add(meshPart);
            }

            return list;
        }

        /// <summary>
        /// ReadDummies
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        private static Dictionary<string, MyModelDummy> ReadDummies(BinaryReader reader)
        {
            var dummies = new Dictionary<string, MyModelDummy>();
            var nCount = reader.ReadInt32();

            for (var i = 0; i < nCount; ++i)
            {
                var str = reader.ReadString();
                var mat = ReadMatrix(reader);

                var customData = new Dictionary<string, object>();
                var customDataCount = reader.ReadInt32();

                for (var j = 0; j < customDataCount; ++j)
                {
                    var name = reader.ReadString();
                    var value = reader.ReadString();
                    customData.Add(name, value);
                }

                var dummy = new MyModelDummy { Matrix = mat, CustomData = customData };
                dummies.Add(str, dummy);
            }

            return dummies;
        }

        /// <summary>
        /// ReadArrayOfInt
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        private static int[] ReadArrayOfInt(BinaryReader reader)
        {
            var nCount = reader.ReadInt32();
            var intArr = new int[nCount];
            for (var i = 0; i < nCount; ++i)
            {
                intArr[i] = reader.ReadInt32();
            }

            return intArr;
        }

        private static byte[] ReadArrayOfBytes(BinaryReader reader)
        {
            var nCount = reader.ReadInt32();
            var data = reader.ReadBytes(nCount);
            return data;
        }

        private static ModelAnimations ReadModelAnimations(BinaryReader reader)
        {
            var modelAnimations = new ModelAnimations { Clips = new List<MyAnimationClip>() };
            var animationCount = reader.ReadInt32();

            for (var i = 0; i < animationCount; i++)
            {
                var clipName = reader.ReadString();
                var duration = reader.ReadDouble();
                int boneCount = reader.ReadInt32();

                var animationClip = new MyAnimationClip() {
                    Name = clipName,
                    Duration = duration
                };

                for (var j = 0; j < boneCount; j++)
                {
                    var boneName = reader.ReadString();
                    int keyFrameCount = reader.ReadInt32();

                    var bone = new MyAnimationClip.Bone(keyFrameCount) {
                        Name = boneName
                    };

                    for (var k = 0; k < keyFrameCount; k++)
                    {
                        var time = reader.ReadDouble();
                        var vector = ReadVector4(reader);
                        var rotation = new Quaternion(vector.X, vector.Y, vector.Z, vector.W);
                        var translation = ReadVector3(reader);

                        bone.Keyframes[k] = new MyAnimationClip.Keyframe() {
                            Time = time,
                            Rotation = rotation,
                            Translation = translation
                        };
                    }

                    animationClip.Bones.Add(bone);
                }

                modelAnimations.Clips.Add(animationClip);
            }

            modelAnimations.Skeleton = ReadArrayOfInt(reader).ToList();
            return modelAnimations;
        }

        private static MyModelBone[] ReadMyModelBoneArray(BinaryReader reader)
        {
            var nCount = reader.ReadInt32();
            var myModelBoneArray = new MyModelBone[nCount];

            for (var i = 0; i < nCount; i++)
            {
                var name = reader.ReadString();
                var parent = reader.ReadInt32();
                var matrix = ReadMatrix(reader);
                myModelBoneArray[i] = new MyModelBone { Name = name, Parent = parent, Transform = matrix };
            }

            return myModelBoneArray;
        }

        private static MyLODDescriptor[] ReadMyLodDescriptorArray(BinaryReader reader)
        {
            var nCount = reader.ReadInt32();
            var myLodDescriptorArray = new MyLODDescriptor[nCount];

            for (var i = 0; i < nCount; i++)
            {
                var distance = reader.ReadSingle();
                var model = reader.ReadString();
                var renderQuality = reader.ReadString();
                myLodDescriptorArray[i] = new MyLODDescriptor { Distance = distance, Model = model, RenderQuality = renderQuality };
            }

            return myLodDescriptorArray;
        }

        #endregion

        #region Import Data readers

        /// <summary>
        /// LoadTagData
        /// </summary>
        /// <returns></returns>
        private static void LoadTagData(BinaryReader reader, Dictionary<string, object> data)
        {
            while (reader.BaseStream.Position != reader.BaseStream.Length)
            {
                var tagName = reader.ReadString();

                switch (tagName)
                {
                    case MyImporterConstants.TAG_DEBUG:
                        data.Add(tagName, ReadArrayOfString(reader));
                        break;

                    case MyImporterConstants.TAG_DUMMIES:
                        data.Add(tagName, ReadDummies(reader));
                        break;

                    case MyImporterConstants.TAG_VERTICES:
                        data.Add(tagName, ReadArrayOfHalfVector4(reader));
                        break;

                    case MyImporterConstants.TAG_NORMALS:
                        data.Add(tagName, ReadArrayOfByte4(reader));
                        break;

                    case MyImporterConstants.TAG_TEXCOORDS0:
                        data.Add(tagName, ReadArrayOfHalfVector2(reader));
                        break;

                    case MyImporterConstants.TAG_BINORMALS:
                        data.Add(tagName, ReadArrayOfByte4(reader));
                        break;

                    case MyImporterConstants.TAG_TANGENTS:
                        data.Add(tagName, ReadArrayOfByte4(reader));
                        break;

                    case MyImporterConstants.TAG_TEXCOORDS1:
                        data.Add(tagName, ReadArrayOfHalfVector2(reader));
                        break;

                    //case MyImporterConstants.TAG_RESCALE_TO_LENGTH_IN_METERS:
                    //    data.Add(tagName, reader.ReadBoolean());
                    //    break;

                    //case MyImporterConstants.TAG_LENGTH_IN_METERS:
                    //    data.Add(tagName, reader.ReadSingle());
                    //    break;

                    case MyImporterConstants.TAG_RESCALE_FACTOR:
                        data.Add(tagName, reader.ReadSingle());
                        break;

                    //case MyImporterConstants.TAG_CENTERED:
                    //    data.Add(tagName, reader.ReadBoolean());
                    //    break;

                    case MyImporterConstants.TAG_USE_CHANNEL_TEXTURES:
                        data.Add(tagName, reader.ReadBoolean());
                        break;

                    //case MyImporterConstants.TAG_SPECULAR_SHININESS:
                    //    data.Add(tagName, reader.ReadSingle());
                    //    break;

                    //case MyImporterConstants.TAG_SPECULAR_POWER:
                    //    data.Add(tagName, reader.ReadSingle());
                    //    break;

                    case MyImporterConstants.TAG_BOUNDING_BOX:
                        data.Add(tagName, ReadBoundingBox(reader));
                        break;

                    case MyImporterConstants.TAG_BOUNDING_SPHERE:
                        data.Add(tagName, ReadBoundingSphere(reader));
                        break;

                    case MyImporterConstants.TAG_SWAP_WINDING_ORDER:
                        data.Add(tagName, reader.ReadBoolean());
                        break;

                    case MyImporterConstants.TAG_MESH_PARTS:
                        data.Add(tagName, ReadMeshParts(reader));
                        break;

                    case MyImporterConstants.TAG_MODEL_BVH:
                        var bvh = new GImpactQuantizedBvh();
                        bvh.Load(ReadArrayOfBytes(reader));
                        data.Add(tagName, bvh);
                        break;

                    case MyImporterConstants.TAG_MODEL_INFO:
                        var tri = reader.ReadInt32();
                        var vert = reader.ReadInt32();
                        var bb = ReadVector3(reader);
                        data.Add(tagName, new MyModelInfo(tri, vert, bb));
                        break;

                    case MyImporterConstants.TAG_BLENDINDICES:
                        data.Add(tagName, ReadArrayOfVector4(reader));
                        break;

                    case MyImporterConstants.TAG_BLENDWEIGHTS:
                        data.Add(tagName, ReadArrayOfVector4(reader));
                        break;

                    case MyImporterConstants.TAG_ANIMATIONS:
                        data.Add(tagName, ReadModelAnimations(reader));
                        break;

                    case MyImporterConstants.TAG_BONES:
                        data.Add(tagName, ReadMyModelBoneArray(reader));
                        break;

                    case MyImporterConstants.TAG_BONE_MAPPING:
                        data.Add(tagName, ReadArrayOfVector3I(reader));
                        break;

                    case MyImporterConstants.TAG_HAVOK_COLLISION_GEOMETRY:
                        data.Add(tagName, ReadArrayOfBytes(reader));
                        break;

                    case MyImporterConstants.TAG_PATTERN_SCALE:
                        data.Add(tagName, reader.ReadSingle());
                        break;

                    case MyImporterConstants.TAG_LODS:
                        data.Add(tagName, ReadMyLodDescriptorArray(reader));
                        break;

                    default:
                        throw new NotImplementedException(string.Format("tag '{0}' has not been implmented ", tagName));
                }
            }
        }

        #endregion
    }
}

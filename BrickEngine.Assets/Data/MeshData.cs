using BinSerialize;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace BrickEngine.Assets.Data
{
    [Flags]
    public enum VertexFeatures : byte
    {
        Empty = 0,
        Positions = 1,
        Normals = 2,
        Tangents = 4,
        Colors = 8,
        TexCoord0 = 16,
        TexCoord1 = 32,
        TexCoord2 = 64,
        TexCoord3 = 128,
    }

    public readonly struct VertexData : IBinarySerializable<VertexData>
    {
        //Either Length is either Array.Empty or VertexCount
        public readonly VertexFeatures VertexFeatures;
        public readonly int VertexCount;
        public readonly Vector3[] Positions;
        public readonly Vector3[] Normals;
        public readonly Vector4[] Tangents;
        public readonly Vector4[] Colors;
        public readonly Vector2[] TexCoord0;
        public readonly Vector2[] TexCoord1;
        public readonly Vector2[] TexCoord2;
        public readonly Vector2[] TexCoord3;

        public VertexData(int vertexCount, Vector3[] positions, Vector3[]? normals = null, Vector4[]? tangents = null, Vector4[]? colors = null,
            Vector2[]? texCoord0 = null, Vector2[]? texCoord1 = null, Vector2[]? texCoord2 = null, Vector2[]? texCoord3 = null)
        {
            if (vertexCount != positions.Length)
            {
                throw new ArgumentException($"VertexCount {vertexCount} must match positions.Length {positions.Length}");
            }
            VertexCount = vertexCount;
            Positions = positions;
            Normals = normals ?? Array.Empty<Vector3>();
            Tangents = tangents ?? Array.Empty<Vector4>();
            Colors = colors ?? Array.Empty<Vector4>();
            TexCoord0 = texCoord0 ?? Array.Empty<Vector2>();
            TexCoord1 = texCoord1 ?? Array.Empty<Vector2>();
            TexCoord2 = texCoord2 ?? Array.Empty<Vector2>();
            TexCoord3 = texCoord3 ?? Array.Empty<Vector2>();
            VertexFeatures = VertexFeatures.Empty;
            VertexFeatures = GetFeatures(this);
        }


        public static VertexData Deserialize(ReadOnlySpan<byte> span)
        {
            Vector3[] Positions = null!;
            Vector3[]? Normals = null;
            Vector4[]? Tangents = null;
            Vector4[]? Colors = null;
            Vector2[]? TexCoord0 = null;
            Vector2[]? TexCoord1 = null;
            Vector2[]? TexCoord2 = null;
            Vector2[]? TexCoord3 = null;
            VertexFeatures features = (VertexFeatures)BinarySerializer.ReadByte(ref span);
            if (features.HasFlag(VertexFeatures.Positions)) Positions = BinarySerializer.ReadVec3Array(ref span);
            if (features.HasFlag(VertexFeatures.Normals)) Normals = BinarySerializer.ReadVec3Array(ref span);
            if (features.HasFlag(VertexFeatures.Tangents)) Tangents = BinarySerializer.ReadVec4Array(ref span);
            if (features.HasFlag(VertexFeatures.Colors)) Colors = BinarySerializer.ReadVec4Array(ref span);
            if (features.HasFlag(VertexFeatures.TexCoord0)) TexCoord0 = BinarySerializer.ReadVec2Array(ref span);
            if (features.HasFlag(VertexFeatures.TexCoord1)) TexCoord1 = BinarySerializer.ReadVec2Array(ref span);
            if (features.HasFlag(VertexFeatures.TexCoord2)) TexCoord2 = BinarySerializer.ReadVec2Array(ref span);
            if (features.HasFlag(VertexFeatures.TexCoord3)) TexCoord3 = BinarySerializer.ReadVec2Array(ref span);
            return new VertexData(Positions.Length, Positions, Normals, Tangents, Colors, TexCoord0, TexCoord1, TexCoord2, TexCoord3);
        }

        public static void Serialize(ByteBufferWriter writer, VertexData data)
        {
            var features = GetFeatures(data);
            var arrayCount = BitOperations.PopCount((uint)features);
            var span = writer.GetSpan(1 + arrayCount * data.VertexCount * sizeof(float) * 4);
            BinarySerializer.WriteByte(ref span, (byte)features);
            if (features.HasFlag(VertexFeatures.Positions)) BinarySerializer.WriteVec3Array(ref span, data.Positions);
            if (features.HasFlag(VertexFeatures.Normals)) BinarySerializer.WriteVec3Array(ref span, data.Normals);
            if (features.HasFlag(VertexFeatures.Tangents)) BinarySerializer.WriteVec4Array(ref span, data.Tangents);
            if (features.HasFlag(VertexFeatures.Colors)) BinarySerializer.WriteVec4Array(ref span, data.Colors);
            if (features.HasFlag(VertexFeatures.TexCoord0)) BinarySerializer.WriteVec2Array(ref span, data.TexCoord0);
            if (features.HasFlag(VertexFeatures.TexCoord1)) BinarySerializer.WriteVec2Array(ref span, data.TexCoord1);
            if (features.HasFlag(VertexFeatures.TexCoord2)) BinarySerializer.WriteVec2Array(ref span, data.TexCoord2);
            if (features.HasFlag(VertexFeatures.TexCoord3)) BinarySerializer.WriteVec2Array(ref span, data.TexCoord3);
            writer.ReturnSpanAndAdvance(ref span);
        }

        private static VertexFeatures GetFeatures(VertexData data)
        {
            VertexFeatures features = VertexFeatures.Empty;
            features |= UsesArray(data.Positions) ? VertexFeatures.Positions : VertexFeatures.Empty;
            features |= UsesArray(data.Normals) ? VertexFeatures.Normals : VertexFeatures.Empty;
            features |= UsesArray(data.Tangents) ? VertexFeatures.Tangents : VertexFeatures.Empty;
            features |= UsesArray(data.Colors) ? VertexFeatures.Colors : VertexFeatures.Empty;
            features |= UsesArray(data.TexCoord0) ? VertexFeatures.TexCoord0 : VertexFeatures.Empty;
            features |= UsesArray(data.TexCoord1) ? VertexFeatures.TexCoord1 : VertexFeatures.Empty;
            features |= UsesArray(data.TexCoord2) ? VertexFeatures.TexCoord2 : VertexFeatures.Empty;
            features |= UsesArray(data.TexCoord3) ? VertexFeatures.TexCoord3 : VertexFeatures.Empty;
            return features;
        }

        private static bool UsesArray(Array? array)
        {
            return array != null && array.Length > 0;
        }
    }

    public readonly struct IndexData : IBinarySerializable<IndexData>
    {
        public readonly int[] Indices;

        public IndexData(int[] indices)
        {
            Indices = indices;
        }

        public static IndexData Deserialize(ReadOnlySpan<byte> blob)
        {
            return new IndexData(BinarySerializer.ReadIntArray(ref blob));
        }

        public static void Serialize(ByteBufferWriter writer, IndexData data)
        {
            var span = writer.GetSpan(data.Indices.Length * 4);
            BinarySerializer.WriteIntArray(ref span, data.Indices);
            writer.ReturnSpanAndAdvance(ref span);
        }
    }

    public class MeshData : IBinarySerializable<MeshData>
    {
        public readonly VertexData VertexData;
        public readonly IndexData IndexData;

        public MeshData(VertexData vertexData, IndexData indexData)
        {
            VertexData = vertexData;
            IndexData = indexData;
        }

        public static MeshData Deserialize(ReadOnlySpan<byte> span)
        {
            var vertexData = VertexData.Deserialize(span);
            var indexData = IndexData.Deserialize(span);
            return new MeshData(vertexData, indexData);
        }

        public static void Serialize(ByteBufferWriter writer, MeshData data)
        {
            VertexData.Serialize(writer, data.VertexData);
            IndexData.Serialize(writer, data.IndexData);
        }
    }
}

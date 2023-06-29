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

    [MemoryPackable]
    public readonly partial struct VertexData
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

        [MemoryPackConstructor]
        public VertexData(VertexFeatures vertexFeatures, int vertexCount, Vector3[] positions, Vector3[] normals, Vector4[] tangents, Vector4[] colors, Vector2[] texCoord0, Vector2[] texCoord1, Vector2[] texCoord2, Vector2[] texCoord3)
        {
            VertexFeatures = vertexFeatures;
            VertexCount = vertexCount;
            Positions = positions;
            Normals = normals;
            Tangents = tangents;
            Colors = colors;
            TexCoord0 = texCoord0;
            TexCoord1 = texCoord1;
            TexCoord2 = texCoord2;
            TexCoord3 = texCoord3;
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
    [MemoryPackable]
    public readonly partial struct IndexData
    {
        public readonly uint[] Indices;

        public IndexData(uint[] indices)
        {
            Indices = indices;
        }

        public static IndexData Deserialize(ref ReadOnlySpan<byte> blob)
        {
            return new IndexData(BinarySerializer.ReadUIntArray(ref blob));
        }

        public static void Serialize(ByteBufferWriter writer, IndexData data)
        {
            var span = writer.GetSpan(data.Indices.Length * 4);
            BinarySerializer.WriteUIntArray(ref span, data.Indices);
            writer.ReturnSpanAndAdvance(ref span);
        }
    }

    [MemoryPackable(GenerateType.CircularReference)]
    public sealed partial class MeshData
    {
        [MemoryPackOrder(0)]
        public VertexData VertexData;
        [MemoryPackOrder(1)]
        public IndexData IndexData;
        [MemoryPackConstructor]
        public MeshData()
        {
                
        }

        public MeshData(VertexData vertexData, IndexData indexData)
        {
            VertexData = vertexData;
            IndexData = indexData;
        }
    }
}

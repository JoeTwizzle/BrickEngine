using BinSerialize;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace BrickEngine.Assets.Data
{
    public sealed class ModelData : IBinarySerializable<ModelData>
    {
        public readonly MeshData[] Meshes;
        public readonly MaterialData[] Materials;

        public ModelData(MeshData[] meshes, MaterialData[] materials)
        {
            Meshes = meshes;
            Materials = materials;
        }

        public static ModelData Deserialize(ref ReadOnlySpan<byte> blob)
        {
            var meshCount = BinarySerializer.ReadInt(ref blob);
            var meshes = new MeshData[meshCount];
            for (int i = 0; i < meshes.Length; i++)
            {
                meshes[i] = MeshData.Deserialize(ref blob);
            }
            var materialCount = BinarySerializer.ReadInt(ref blob);
            var materials = new MaterialData[materialCount];
            for (int i = 0; i < materials.Length; i++)
            {
                materials[i] = MaterialData.Deserialize(ref blob);
            }
            return new ModelData(meshes, materials);
        }

        public static void Serialize(ByteBufferWriter writer, ModelData data)
        {
            var span = writer.GetSpan(sizeof(int));
            BinarySerializer.WriteInt(ref span, data.Meshes.Length);
            writer.ReturnSpanAndAdvance(ref span);
            for (int i = 0; i < data.Meshes.Length; i++)
            {
                MeshData.Serialize(writer, data.Meshes[i]);
            }
            span = writer.GetSpan(sizeof(int));
            BinarySerializer.WriteInt(ref span, data.Materials.Length);
            writer.ReturnSpanAndAdvance(ref span);
            for (int i = 0; i < data.Materials.Length; i++)
            {
                MaterialData.Serialize(writer, data.Materials[i]);
            }
            throw new NotImplementedException();
        }
    }
}

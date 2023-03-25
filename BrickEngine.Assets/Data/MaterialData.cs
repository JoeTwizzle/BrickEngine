using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BinSerialize;

namespace BrickEngine.Assets.Data
{
    public class MaterialData : IBinarySerializable<MaterialData>
    {
        public readonly Guid ColorTexture;
        public readonly Guid NormalTexture;
        public readonly Guid MetalRoughnessTexture;
        public readonly Guid EmissiveTexture;

        public MaterialData(Asset colorTexture, Asset normalTexture, Asset metalRoughnessTexture, Asset emissiveTexture)
        {
            ThrowHelper.ThrowIfNotAssetOfType(colorTexture, typeof(TextureData));
            ThrowHelper.ThrowIfNotAssetOfType(normalTexture, typeof(TextureData));
            ThrowHelper.ThrowIfNotAssetOfType(metalRoughnessTexture, typeof(TextureData));
            ThrowHelper.ThrowIfNotAssetOfType(emissiveTexture, typeof(TextureData));
            ColorTexture = colorTexture.Header.Guid;
            NormalTexture = normalTexture.Header.Guid;
            MetalRoughnessTexture = metalRoughnessTexture.Header.Guid;
            EmissiveTexture = emissiveTexture.Header.Guid;
        }

        private MaterialData(Guid colorTexture, Guid normalTexture, Guid metalRoughnessTexture, Guid emissiveTexture)
        {
            ColorTexture = colorTexture;
            NormalTexture = normalTexture;
            MetalRoughnessTexture = metalRoughnessTexture;
            EmissiveTexture = emissiveTexture;
        }

        public static MaterialData Deserialize(ReadOnlySpan<byte> blob)
        {
            Guid color = BinarySerializer.ReadGuid(ref blob);
            Guid normal = BinarySerializer.ReadGuid(ref blob);
            Guid metalRough = BinarySerializer.ReadGuid(ref blob);
            Guid emissive = BinarySerializer.ReadGuid(ref blob);
            return new MaterialData(color, normal, metalRough, emissive);
        }

        public static void Serialize(ByteBufferWriter writer, MaterialData data)
        {
            var span = writer.GetSpan(4 * 16);//size of 4 guids
            BinarySerializer.WriteGuid(ref span, data.ColorTexture);
            BinarySerializer.WriteGuid(ref span, data.NormalTexture);
            BinarySerializer.WriteGuid(ref span, data.MetalRoughnessTexture);
            BinarySerializer.WriteGuid(ref span, data.EmissiveTexture);
        }
    }
}

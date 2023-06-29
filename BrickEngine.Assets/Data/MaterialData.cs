using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BinSerialize;

namespace BrickEngine.Assets.Data
{
    public class MaterialData : IBinarySerializable<MaterialData>
    {
        public readonly TextureData ColorTexture;
        public readonly TextureData NormalTexture;
        public readonly TextureData MetalRoughnessTexture;
        public readonly TextureData OcclusionTexture;
        public readonly TextureData EmissiveTexture;

        public MaterialData(TextureData colorTexture, TextureData normalTexture, TextureData metalRoughnessTexture, TextureData occlusionTexture, TextureData emissiveTexture)
        {
            ColorTexture = colorTexture;
            NormalTexture = normalTexture;
            MetalRoughnessTexture = metalRoughnessTexture;
            OcclusionTexture = occlusionTexture;
            EmissiveTexture = emissiveTexture;
        }

        public static MaterialData Deserialize(ref ReadOnlySpan<byte> blob)
        {
            return new MaterialData(TextureData.Deserialize(ref blob), TextureData.Deserialize(ref blob), TextureData.Deserialize(ref blob), TextureData.Deserialize(ref blob), TextureData.Deserialize(ref blob));
        }

        public static void Serialize(ByteBufferWriter writer, MaterialData data)
        {
            TextureData.Serialize(writer, data.ColorTexture);
            TextureData.Serialize(writer, data.NormalTexture);
            TextureData.Serialize(writer, data.MetalRoughnessTexture);
            TextureData.Serialize(writer, data.OcclusionTexture);
            TextureData.Serialize(writer, data.EmissiveTexture);
        }
    }
}

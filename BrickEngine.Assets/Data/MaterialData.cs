using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BinSerialize;

namespace BrickEngine.Assets.Data
{
    [MemoryPackable(GenerateType.CircularReference)]
    public sealed partial class MaterialData
    {
        [MemoryPackOrder(0)]
        public TextureData ColorTexture;
        [MemoryPackOrder(1)]
        public TextureData NormalTexture;
        [MemoryPackOrder(2)]
        public TextureData MetalRoughnessTexture;
        [MemoryPackOrder(3)]
        public TextureData OcclusionTexture;
        [MemoryPackOrder(4)]
        public TextureData EmissiveTexture;
        [MemoryPackConstructor]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public MaterialData()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {

        }
        public MaterialData(TextureData colorTexture, TextureData normalTexture, TextureData metalRoughnessTexture, TextureData occlusionTexture, TextureData emissiveTexture)
        {
            ColorTexture = colorTexture;
            NormalTexture = normalTexture;
            MetalRoughnessTexture = metalRoughnessTexture;
            OcclusionTexture = occlusionTexture;
            EmissiveTexture = emissiveTexture;
        }
    }
}

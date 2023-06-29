using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BinSerialize;
using System.Threading.Tasks;

namespace BrickEngine.Assets.Data
{
    [MemoryPackable]
    public readonly partial struct MipLevel
    {
        public readonly int Width;
        public readonly int Height;
        public readonly byte[] Data;

        public MipLevel(int width, int height, byte[] data)
        {
            Width = width;
            Height = height;
            Data = data;
        }
    }
    [MemoryPackable]
    public sealed partial class TextureData
    {
        public readonly uint VdFormat;
        public readonly MipLevel[] Miplevels;

        public TextureData(uint vdFormat, MipLevel[] miplevels)
        {
            VdFormat = vdFormat;
            Miplevels = miplevels;
        }
    }
}

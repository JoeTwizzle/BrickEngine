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
    public readonly struct MipLevel
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
    public class TextureData : ISerializable<TextureData>
    {
        public readonly uint VdFormat;
        public readonly MipLevel[] Miplevels;

        public TextureData(uint vdFormat, MipLevel[] miplevels)
        {
            VdFormat = vdFormat;
            Miplevels = miplevels;
        }

        public static TextureData Deserialize(ReadOnlySpan<byte> blob)
        {
            var VdFormat = BinarySerializer.ReadPackedUInt(ref blob);
            var MipCount = BinarySerializer.ReadByte(ref blob);
            var Miplevels = new MipLevel[MipCount];
            for (int i = 0; i < MipCount; i++)
            {
                ref var level = ref Miplevels[i];
                var Width = BinarySerializer.ReadPackedInt(ref blob);
                var Height = BinarySerializer.ReadPackedInt(ref blob);
                int dataLength = (int)BinarySerializer.ReadUInt(ref blob);
                var Data = BinarySerializer.ReadBlock(ref blob, dataLength);
                level = new MipLevel(Width, Height, Data);
            }
            return new TextureData(VdFormat, Miplevels);
        }

        public static void Serialize(ByteBufferWriter writer, TextureData data)
        {
            Span<byte> headerSpan = writer.GetSpan(5);
            BinarySerializer.WritePackedUInt(ref headerSpan, data.VdFormat);
            BinarySerializer.WriteByte(ref headerSpan, (byte)data.Miplevels.Length);
            writer.ReturnSpanAndAdvance(ref headerSpan);
            for (int i = 0; i < data.Miplevels.Length; i++)
            {
                ref var level = ref data.Miplevels[i];
                int maxbufferLength = level.Data.Length + sizeof(int) * 4;
                Span<byte> mipSpan = writer.GetSpan(maxbufferLength);
                BinarySerializer.WritePackedInt(ref mipSpan, level.Width);
                BinarySerializer.WritePackedInt(ref mipSpan, level.Height);
                BinarySerializer.WriteUInt(ref mipSpan, (uint)level.Data.Length);
                BinarySerializer.WriteBlock(ref mipSpan, level.Data);
                writer.ReturnSpanAndAdvance(ref mipSpan);
            }
        }
    }
}

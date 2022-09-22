using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BinSerialize;

namespace BrickEngine.Assets.Data
{
    public class FileData : ISerializable<FileData>
    {
        public readonly byte[] Data;

        public FileData(byte[] data)
        {
            Data = data;
        }

        public static FileData Deserialize(ReadOnlySpan<byte> span)
        {
            int length = BinarySerializer.ReadPackedInt(ref span);
            var data = BinarySerializer.ReadBlock(ref span, length);
            return new FileData(data);
        }

        public static void Serialize(ByteBufferWriter writer, FileData data)
        {
            var span = writer.GetSpan(5 + data.Data.Length);
            BinarySerializer.WritePackedInt(ref span, data.Data.Length);
            BinarySerializer.WriteBlock(ref span, data.Data);
        }
    }
}

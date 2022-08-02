using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrickEngine.Assets
{
    public static class GuidExtensions
    {
        public static void ToBytes(this Guid guid, out ulong GuidPart1, out ulong GuidPart2)
        {
            Span<byte> buffer = stackalloc byte[16];
            Debug.Assert(guid.TryWriteBytes(buffer));
            GuidPart1 = BinaryPrimitives.ReadUInt64LittleEndian(buffer);
            GuidPart2 = BinaryPrimitives.ReadUInt64LittleEndian(buffer.Slice(8, 8));
        }

        public static Guid FromBytes(ulong GuidPart1, ulong GuidPart2)
        {
            Span<byte> buffer = stackalloc byte[16];
            if (BitConverter.IsLittleEndian)
            {
                Debug.Assert(BitConverter.TryWriteBytes(buffer, GuidPart1));
                Debug.Assert(BitConverter.TryWriteBytes(buffer.Slice(8, 8), GuidPart2));
            }
            else
            {
                Debug.Assert(BitConverter.TryWriteBytes(buffer, BinaryPrimitives.ReverseEndianness(GuidPart1)));
                Debug.Assert(BitConverter.TryWriteBytes(buffer.Slice(8, 8), BinaryPrimitives.ReverseEndianness(GuidPart2)));
            }
            return new Guid(buffer);
        }
    }
}

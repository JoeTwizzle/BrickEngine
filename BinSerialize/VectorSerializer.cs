using System.Numerics;
using System.Runtime.InteropServices;

namespace BinSerialize
{
    public static unsafe partial class BinarySerializer
    {
        //Write
        public static int WriteVec2(ref Span<byte> span, ref Vector2 vec)
        {
            MemoryMarshal.Write(span, ref vec);

            // 'Advance' the span.
            const int size = sizeof(float) * 2;
            span = span.Slice(size);
            return size;
        }

        public static int WriteVec3(ref Span<byte> span, ref Vector3 vec)
        {
            MemoryMarshal.Write(span, ref vec);

            // 'Advance' the span.
            const int size = sizeof(float) * 3;
            span = span.Slice(size);
            return size;
        }

        public static int WriteVec4(ref Span<byte> span, ref Vector4 vec)
        {
            MemoryMarshal.Write(span, ref vec);

            // 'Advance' the span.
            const int size = sizeof(float) * 4;
            span = span.Slice(size);
            return size;
        }

        //Read
        public static Vector2 ReadVec2(ref ReadOnlySpan<byte> span)
        {
            var result = MemoryMarshal.Read<Vector2>(span);

            // 'Advance' the span.
            span = span.Slice(sizeof(float) * 2);
            return result;
        }

        public static Vector3 ReadVec3(ref ReadOnlySpan<byte> span)
        {
            var result = MemoryMarshal.Read<Vector3>(span);

            // 'Advance' the span.
            span = span.Slice(sizeof(float) * 3);
            return result;
        }

        public static Vector4 ReadVec4(ref ReadOnlySpan<byte> span)
        {
            var result = MemoryMarshal.Read<Vector4>(span);

            // 'Advance' the span.
            span = span.Slice(sizeof(float) * 4);
            return result;
        }
    }
}

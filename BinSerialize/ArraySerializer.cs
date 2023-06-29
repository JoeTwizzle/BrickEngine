using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BinSerialize
{
    public static unsafe partial class BinarySerializer
    {
        //Write
        public static int WriteArray<T>(ref Span<byte> span, Span<T> data) where T : unmanaged
        {
            WritePackedInt(ref span, data.Length);
            for (int i = 0; i < data.Length; i++)
            {
                ref var vector = ref data[i];
                WriteStruct(ref span, ref vector);
            }
            return sizeof(int) + Unsafe.SizeOf<T>() * data.Length;
        }

        
        public static int WriteVec2Array(ref Span<byte> span, Span<Vector2> vectors)
        {
            WritePackedInt(ref span, vectors.Length);
            for (int i = 0; i < vectors.Length; i++)
            {
                ref var vector = ref vectors[i];
                WriteVec2(ref span, ref vector);
            }
            return sizeof(int) + sizeof(float) * 2 * vectors.Length;
        }

        public static int WriteVec3Array(ref Span<byte> span, Span<Vector3> vectors)
        {
            WritePackedInt(ref span, vectors.Length);
            for (int i = 0; i < vectors.Length; i++)
            {
                ref var vector = ref vectors[i];
                WriteVec3(ref span, ref vector);
            }
            return sizeof(int) + sizeof(float) * 3 * vectors.Length;
        }

        public static int WriteVec4Array(ref Span<byte> span, Span<Vector4> vectors)
        {
            WritePackedInt(ref span, vectors.Length);
            for (int i = 0; i < vectors.Length; i++)
            {
                ref var vector = ref vectors[i];
                WriteVec4(ref span, ref vector);
            }
            return sizeof(int) + sizeof(float) * 4 * vectors.Length;
        }

        public static int WriteIntArray(ref Span<byte> span, Span<int> integers)
        {
            int headerSize = WritePackedInt(ref span, integers.Length);
            for (int i = 0; i < integers.Length; i++)
            {
                WriteInt(ref span, integers[i]);
            }
            return headerSize + sizeof(int) * 4 * integers.Length;
        }

        public static int WriteUIntArray(ref Span<byte> span, Span<uint> integers)
        {
            int headerSize = WritePackedInt(ref span, integers.Length);
            for (int i = 0; i < integers.Length; i++)
            {
                WriteUInt(ref span, integers[i]);
            }
            return headerSize + sizeof(int) * integers.Length;
        }

        //Read
        public static Vector2[] ReadVec2Array(ref ReadOnlySpan<byte> span)
        {
            int length = ReadPackedInt(ref span);
            Vector2[] vectors = new Vector2[length];
            for (int i = 0; i < vectors.Length; i++)
            {
                vectors[i] = ReadVec2(ref span);
            }
            return vectors;
        }

        public static Vector3[] ReadVec3Array(ref ReadOnlySpan<byte> span)
        {
            int length = ReadPackedInt(ref span);
            Vector3[] vectors = new Vector3[length];
            for (int i = 0; i < vectors.Length; i++)
            {
                vectors[i] = ReadVec3(ref span);
            }
            return vectors;
        }

        public static Vector4[] ReadVec4Array(ref ReadOnlySpan<byte> span)
        {
            int length = ReadPackedInt(ref span);
            Vector4[] vectors = new Vector4[length];
            for (int i = 0; i < vectors.Length; i++)
            {
                vectors[i] = ReadVec4(ref span);
            }
            return vectors;
        }

        public static int[] ReadIntArray(ref ReadOnlySpan<byte> span)
        {
            int length = ReadPackedInt(ref span);
            int[] integers = new int[length];
            for (int i = 0; i < integers.Length; i++)
            {
                integers[i] = ReadInt(ref span);
            }
            return integers;
        }

        public static uint[] ReadUIntArray(ref ReadOnlySpan<byte> span)
        {
            int length = ReadPackedInt(ref span);
            uint[] integers = new uint[length];
            for (int i = 0; i < integers.Length; i++)
            {
                integers[i] = ReadUInt(ref span);
            }
            return integers;
        }
    }
}

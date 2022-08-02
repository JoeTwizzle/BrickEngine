using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BrickEngine.Core.Utilities
{
    public static class Utils
    {
        //public static Span<T> StackallocOrCreateArray<T>(int length, int maxStackallocLength) where T : unmanaged
        //{
        //    return ((Unsafe.SizeOf<T>() * length) < maxStackallocLength) ? stackalloc T[length] : new T[length];
        //}
        public static IEnumerable<Type> GetDerivedTypes(this Type type)
        {
            return Assembly.GetAssembly(type)!.GetTypes().Where(myType => !myType.IsInterface && !myType.IsAbstract && myType.IsSubclassOf(type));
        }
        public static unsafe T SpanToStructure<T>(ReadOnlySpan<byte> bytes) where T : unmanaged
        {
            return MemoryMarshal.Read<T>(bytes);
        }
        public static byte[] ReadFully(this Stream input)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                input.CopyTo(ms);
                return ms.ToArray();
            }
        }
    }
}

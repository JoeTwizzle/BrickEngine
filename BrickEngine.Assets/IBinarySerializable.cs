using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrickEngine.Assets
{
    public interface IBinarySerializable<T>
    {
        static abstract T Deserialize(ReadOnlySpan<byte> blob);
        static abstract void Serialize(ByteBufferWriter writer, T data);
    }
}

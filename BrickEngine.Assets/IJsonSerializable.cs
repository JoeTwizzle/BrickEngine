using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BrickEngine.Assets
{
    public interface IJsonSerializable<T> where T : IJsonSerializable<T>
    {
        static abstract T Deserialize(Utf8JsonReader reader);
        static abstract void Serialize(Utf8JsonWriter writer, T item);
    }
}

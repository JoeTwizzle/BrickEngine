using BrickEngine.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrickEngine.Core.Serialization
{
    public static class Serializer
    {
        public static void Serialize<T>(T[] data) where T : struct
        {
            var type = typeof(T);
        }

        static void SerialeLayoutInternal(object data, Type type)
        {
            var formattable = type.GetInterface(nameof(IFormattable));
            bool isTextFormattable = !type.IsAbstract && formattable != null;
            var cache = ReflectionCache.GetCacheForType(type);
            foreach (var info in cache)
            {
                var fieldName = info.Name;
                var fieldType = info.FieldType;

            }
        }
    }
}

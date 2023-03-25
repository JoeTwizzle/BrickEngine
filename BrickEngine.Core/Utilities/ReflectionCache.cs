using BrickEngine.Core.Serialization;
using System.Reflection;

namespace BrickEngine.Core.Utilities
{
    public static class ReflectionCache
    {
        static readonly List<FieldInfo> filteredFields = new List<FieldInfo>();
        static readonly Dictionary<Type, FieldInfo[]> fieldCache = new Dictionary<Type, FieldInfo[]>();
        public static void Purge()
        {
            fieldCache.Clear();
        }

        public static FieldInfo[] GetCacheForType(Type t)
        {
            if (!fieldCache.TryGetValue(t, out var fieldsCached))
            {
                filteredFields.Clear();
                var fields = t.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                for (int i = 0; i < fields.Length; i++)
                {
                    if (fields[i].IsPublic && !Attribute.IsDefined(fields[i], typeof(IgnoreFieldAttribute)))
                    {
                        filteredFields.Add(fields[i]);
                    }
                    else if (!fields[i].IsPublic && Attribute.IsDefined(fields[i], typeof(SerializeFieldAttribute)))
                    {
                        filteredFields.Add(fields[i]);
                    }
                }
                fieldsCached = filteredFields.ToArray();
                fieldCache.Add(t, fieldsCached);
            }
            return fieldsCached;
        }
    }
}

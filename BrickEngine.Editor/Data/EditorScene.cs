using BrickEngine.Core.Utilities;
using System.Numerics;
using System.Reflection;
using System.Text.Json;

namespace BrickEngine.Editor.Data
{
    //public class ComponentConverterFactory : JsonConverterFactory
    //{
    //    public override bool CanConvert(Type typeToConvert)
    //    {
    //        if (!typeToConvert.IsValueType)
    //        {
    //            return false;
    //        }

    //        if (!typeToConvert.IsAssignableTo(typeof(IComponent)))
    //        {
    //            return false;
    //        }

    //        return true;
    //    }

    //    public override JsonConverter CreateConverter(
    //        Type type,
    //        JsonSerializerOptions options)
    //    {
    //        ReflectionCache.GetCacheForType(type);
    //        Type keyType = type.GetGenericArguments()[0];
    //        Type valueType = type.GetGenericArguments()[1];

    //        JsonConverter converter = (JsonConverter)Activator.CreateInstance(
    //            typeof(DictionaryEnumConverterInner<,>).MakeGenericType(
    //                new Type[] { keyType, valueType }),
    //            BindingFlags.Instance | BindingFlags.Public,
    //            binder: null,
    //            args: new object[] { options },
    //            culture: null)!;

    //        return converter;
    //    }
    //}

    //public class ComponentConverter<T> : JsonConverter<T> where T : struct, IComponent
    //{
    //    readonly FieldInfo[] fields;
    //    public ComponentConverter(FieldInfo[] fields)
    //    {
    //        this.fields = fields;
    //    }
    //    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    //    {
    //        object boxed = default(T);
    //        while (reader.Read())
    //        {
    //            int i = 0;
    //            if (reader.TokenType == JsonTokenType.Number)
    //            {
    //                fields[i].SetValue(boxed, reader.);
    //            }
    //        }
    //        return type;
    //    }

    //    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    //    {
    //        writer.WriteStartObject();
    //        for (int i = 0; i < fields.Length; i++)
    //        {
    //            var field = fields[i];
    //            if (!JsonHelper.TryWritePrimitive(field.Name, writer, field.FieldType, field.GetValue(value)!))
    //            {
    //                if (field.FieldType == typeof(string))
    //                {
    //                    writer.WriteString(field.Name, (string?)field.GetValue(value));
    //                }
    //                else
    //                {
    //                    writer.WritePropertyName(field.Name);
    //                    writer.WriteRawValue(JsonSerializer.Serialize(field.GetValue(value), field.FieldType), true);
    //                }
    //            }
    //        }
    //        writer.WriteEndObject();
    //        value.GetType();
    //    }
    //}

    public static class JsonTextSerializer
    {
        public static bool IsJsonType(Type type)
        {
            if (type == typeof(string))
            {
                return true;
            }
            else if (type == typeof(byte) || type == typeof(sbyte))
            {
                return true;
            }
            else if (type == typeof(ushort) || type == typeof(short))
            {
                return true;
            }
            else if (type == typeof(uint) || type == typeof(int))
            {
                return true;
            }
            else if (type == typeof(ulong) || type == typeof(long))
            {
                return true;
            }
            else if (type == typeof(float) || type == typeof(double))
            {
                return true;
            }
            else if (type.IsEnum)
            {
                return true;
            }
            return false;
        }

        static bool TryWriteValue(object? obj, Type objType, Utf8JsonWriter writer)
        {
            if (obj == null)
            {
                writer.WriteRawValue("null", true);
                return true;
            }
            else if (IsJsonType(objType))
            {
                if (objType == typeof(string))
                {
                    writer.WriteRawValue($"\"{obj}\"");
                }
                else if (objType.IsEnum) //serialize enums as text
                {
                    writer.WriteRawValue($"\"{obj}\"");
                }
                else
                {
                    writer.WriteRawValue(obj?.ToString() ?? "null");
                }
                return true;
            }
            return false;
        }

        public static void Serialize(object? obj, Type objType, Utf8JsonWriter writer)
        {
            if (TryWriteValue(obj, objType, writer))
            {
                return;
            }
            var cache = ReflectionCache.GetCacheForType(objType);
            writer.WriteStartObject();
            foreach (var field in cache)
            {
                var type = field.FieldType;
                object? value = field.GetValue(obj);
                writer.WritePropertyName(field.Name);
                if (value == null)
                {
                    writer.WriteRawValue("null", true);
                    continue;
                }
                if (IsJsonType(type))
                {
                    if (type == typeof(string))
                    {
                        writer.WriteRawValue($"\"{value}\"");
                    }
                    else if (type.IsEnum) //serialize enums as text
                    {
                        writer.WriteRawValue($"\"{value}\"");
                    }
                    else
                    {
                        writer.WriteRawValue(value.ToString() ?? "null");
                    }
                }
                else if (type.IsArray)
                {
                    var elementType = field.FieldType.GetElementType()!;
                    bool isValueType = elementType.IsValueType;
                    writer.WriteBoolean("IsValueType", isValueType);
                    writer.WriteStartArray();
                    var arr = (Array)value;
                    foreach (object? item in arr)
                    {
                        if (isValueType)
                        {
                            Serialize(item, elementType, writer);
                        }
                        else
                        {
                            Type itemType = item?.GetType() ?? elementType!;
                            writer.WriteString("Type", itemType.FullName);
                            Serialize(item, itemType, writer);
                        }
                    }
                    writer.WriteEndArray();
                }
                else
                {
                    Serialize(value, type, writer);
                }
            }
            writer.WriteEndObject();
        }

        static object? ReadValue(Type type, ref Utf8JsonReader reader)
        {
            if (type == typeof(string))
            {
                return reader.GetString();
            }
            else if (type == typeof(byte))
            {
                return reader.GetByte();
            }
            else if (type == typeof(sbyte))
            {
                return reader.GetSByte();
            }
            else if (type == typeof(ushort))
            {
                return reader.GetUInt16();
            }
            else if (type == typeof(short))
            {
                return reader.GetInt16();
            }
            else if (type == typeof(uint))
            {
                return reader.GetUInt32();
            }
            else if (type == typeof(int))
            {
                return reader.GetInt32();
            }
            else if (type == typeof(ulong))
            {
                return reader.GetUInt64();
            }
            else if (type == typeof(long))
            {
                return reader.GetInt64();
            }
            else if (type == typeof(float))
            {
                return reader.GetSingle();
            }
            else if (type == typeof(double))
            {
                return reader.GetDouble();
            }
            else if (type.IsEnum)
            {
                if (Enum.TryParse(type, reader.GetString()!, true, out var result))
                {
                    return result;
                }
                else
                {
                    return Enum.ToObject(type, 0);
                }
            }
            return null;
        }

        public static object? Deserialize(Type type, ref Utf8JsonReader reader)
        {
            bool first = true;
            object? o = null;
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    return o;
                }
                if (IsJsonType(type))
                {
                    if (first)
                    {
                        return ReadValue(type, ref reader);
                    }
                }
                else
                {
                    FieldInfo[] fields = ReflectionCache.GetCacheForType(type);
                    for (int i = 0; i < fields.Length; i++)
                    {
                        var field = fields[i];
                        if (field.FieldType.IsArray)
                        {
                            if (reader.TokenType == JsonTokenType.StartArray)
                            {

                            }
                        }
                    }
                }

                first = false;
            }
            return null;
        }
    }


    //public class EcsWorldConverter : JsonConverter<EcsWorld>
    //{
    //    public override EcsWorld? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    //    {
    //        int depth = 0;
    //        while (reader.Read())
    //        {
    //            if (reader.TokenType == JsonTokenType.StartObject)
    //            {
    //                depth++;
    //            }
    //            if (reader.TokenType == JsonTokenType.EndObject)
    //            {
    //                depth--;
    //                if (depth != 0)
    //                {
    //                    return new EcsWorld();
    //                }
    //            }

    //            if (reader.TokenType != JsonTokenType.PropertyName)
    //            {
    //                throw new JsonException();
    //            }
    //            string? propertyName = reader.GetString();


    //        }
    //        return null;
    //    }

    //    public override void Write(Utf8JsonWriter writer, EcsWorld value, JsonSerializerOptions options)
    //    {
    //        int[] entities = null!;
    //        int entityCount = value.GetAllEntities(ref entities!);
    //        writer.WriteStartObject("World");
    //        writer.WriteStartArray("Entities");
    //        for (int i = 0; i < entityCount; i++)
    //        {
    //            writer.WriteNumberValue(entities[i]);
    //        }
    //        writer.WriteEndArray();
    //        IEcsPool[] pools = null!;
    //        int poolCount = value.GetAllPools(ref pools!);
    //        writer.WriteStartArray("Pools");
    //        for (int i = 0; i < poolCount; i++)
    //        {
    //            writer.WriteStartObject();
    //            writer.WriteStringValue(pools[i].GetComponentType().AssemblyQualifiedName!);
    //            writer.WriteStartArray("Components");
    //            FieldInfo[] fields = ReflectionCache.GetCacheForType(pools[i].GetComponentType());
    //            writer.WriteStartObject();
    //            writer.WriteStartArray("Fields");
    //            for (int j = 0; j < fields.Length; j++)
    //            {
    //                writer.WriteString(fields[j].Name, fields[j].FieldType.AssemblyQualifiedName);
    //            }
    //            writer.WriteEndArray();
    //            writer.WriteEndObject();
    //            for (int j = 0; j < entityCount; j++)
    //            {
    //                WriteComponent(writer, fields, options, pools[i].GetRaw(entities[j]));
    //            }
    //            writer.WriteEndArray();
    //            writer.WriteEndObject();
    //        }
    //        writer.WriteEndArray();
    //        writer.WriteEndObject();
    //    }

    //    static void WriteComponent(Utf8JsonWriter writer, FieldInfo[] fields, JsonSerializerOptions options, object value)
    //    {
    //        for (int i = 0; i < fields.Length; i++)
    //        {
    //            var field = fields[i];
    //            if (!JsonHelper.TryWritePrimitive(field.Name, writer, field.FieldType, field.GetValue(value)!))
    //            {
    //                if (field.FieldType == typeof(string))
    //                {
    //                    writer.WriteString(field.Name, (string?)field.GetValue(value));
    //                }
    //                else
    //                {
    //                    writer.WritePropertyName(field.Name);
    //                    writer.WriteRawValue(JsonSerializer.Serialize(field.GetValue(value), field.FieldType, options), true);
    //                }
    //            }
    //        }
    //    }
    //}
}

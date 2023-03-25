using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BrickEngine.Editor.Utils
{
    public static class JsonHelper
    {
        public static bool TryReadPrimitive(ref Utf8JsonReader reader, out object value)
        {
            if (reader.TryGetByte(out byte b))
            {
                value = b;
                return true;
            }
            if (reader.TryGetSByte(out sbyte sb))
            {
                value = sb;
                return true;
            }
            if (reader.TryGetUInt16(out ushort us))
            {
                value = us;
                return true;
            }
            if (reader.TryGetInt16(out short s))
            {
                value = s;
                return true;
            }
            if (reader.TryGetUInt32(out uint ui))
            {
                value = ui;
                return true;
            }
            if (reader.TryGetInt32(out int i))
            {
                value = i;
                return true;
            }
            if (reader.TryGetUInt64(out ulong ul))
            {
                value = ul;
                return true;
            }
            if (reader.TryGetInt64(out long l))
            {
                value = l;
                return true;
            }
            if (reader.TryGetSingle(out float f))
            {
                value = f;
                return true;
            }
            if (reader.TryGetDouble(out double d))
            {
                value = d;
                return true;
            }
            if (reader.TryGetDecimal(out decimal dc))
            {
                value = dc;
                return true;
            }
            value = null;
            return false;
        }
        public static bool TryWritePrimitive(string name, Utf8JsonWriter writer, Type type, object value)
        {
            var typeCode = Type.GetTypeCode(type);
            switch (typeCode)
            {
                case TypeCode.Byte:
                    writer.WriteNumber(name, (byte)value);
                    break;
                case TypeCode.SByte:
                    writer.WriteNumber(name, (sbyte)value);
                    break;
                case TypeCode.UInt16:
                    writer.WriteNumber(name, (ushort)value);
                    break;
                case TypeCode.UInt32:
                    writer.WriteNumber(name, (uint)value);
                    break;
                case TypeCode.UInt64:
                    writer.WriteNumber(name, (ulong)value);
                    break;
                case TypeCode.Int16:
                    writer.WriteNumber(name, (short)value);
                    break;
                case TypeCode.Int32:
                    writer.WriteNumber(name, (int)value);
                    break;
                case TypeCode.Int64:
                    writer.WriteNumber(name, (long)value);
                    break;
                case TypeCode.Decimal:
                    writer.WriteNumber(name, (decimal)value);
                    break;
                case TypeCode.Double:
                    writer.WriteNumber(name, (double)value);
                    break;
                case TypeCode.Single:
                    writer.WriteNumber(name, (float)value);
                    break;
                case TypeCode.Boolean:
                    writer.WriteBoolean(name, (bool)value);
                    break;
                default:
                    return false;
            }
            return true;
        }
        public static bool TryWriteNumberValue(Utf8JsonWriter writer, Type type, object value)
        {
            var typeCode = Type.GetTypeCode(type);
            switch (typeCode)
            {
                case TypeCode.Byte:
                    writer.WriteNumberValue((byte)value);
                    break;
                case TypeCode.SByte:
                    writer.WriteNumberValue((sbyte)value);
                    break;
                case TypeCode.UInt16:
                    writer.WriteNumberValue((ushort)value);
                    break;
                case TypeCode.UInt32:
                    writer.WriteNumberValue((uint)value);
                    break;
                case TypeCode.UInt64:
                    writer.WriteNumberValue((ulong)value);
                    break;
                case TypeCode.Int16:
                    writer.WriteNumberValue((short)value);
                    break;
                case TypeCode.Int32:
                    writer.WriteNumberValue((int)value);
                    break;
                case TypeCode.Int64:
                    writer.WriteNumberValue((long)value);
                    break;
                case TypeCode.Decimal:
                    writer.WriteNumberValue((decimal)value);
                    break;
                case TypeCode.Double:
                    writer.WriteNumberValue((double)value);
                    break;
                case TypeCode.Single:
                    writer.WriteNumberValue((float)value);
                    break;
                case TypeCode.Boolean:
                    writer.WriteBooleanValue((bool)value);
                    break;
                default:
                    return false;
            }
            return true;
        }
    }
}
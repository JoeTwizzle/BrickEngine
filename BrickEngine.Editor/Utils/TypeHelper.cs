using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrickEngine.Editor.Utils
{
    public static class TypeHelper
    {      
        public static bool IsNumericType(this Type t)
        {
            return IsNumericType(t, out _);
        }
        public static bool IsNumericType(this Type t, out TypeCode typeCode)
        {
            typeCode = Type.GetTypeCode(t);
            switch (typeCode)
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return true;
                default:
                    return false;
            }
        }
    }
}

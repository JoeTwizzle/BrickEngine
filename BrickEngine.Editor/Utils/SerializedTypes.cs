using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrickEngine.Editor.Utils
{
    public static class SerializedTypes
    {
        public static IReadOnlySet<Type> CustomSerializedTypes => customSerializedTypes;
        readonly static HashSet<Type> customSerializedTypes = new();
        public static void AddType(Type type)
        {
            customSerializedTypes.Add(type);
        }
    }
}

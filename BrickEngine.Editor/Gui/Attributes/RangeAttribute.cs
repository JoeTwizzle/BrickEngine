using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrickEngine.Editor.Gui.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class RangeAttribute : Attribute
    {
        public readonly float Min, Max;

        public RangeAttribute(float min, float max)
        {
            Min = min;
            Max = max;
        }
    }
}

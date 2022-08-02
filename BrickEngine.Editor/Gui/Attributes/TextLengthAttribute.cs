using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrickEngine.Editor.Gui.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class TextLengthAttribute : Attribute
    {
        public readonly uint Length;

        public TextLengthAttribute(uint length)
        {
            Length = length;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrickEngine.Editor.Gui.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class ColorPickerAttribute : Attribute
    {
        public readonly ImGuiColorEditFlags Flags;

        public ColorPickerAttribute(ImGuiColorEditFlags flags = ImGuiColorEditFlags.Float)
        {
            Flags = flags;
        }
    }
}

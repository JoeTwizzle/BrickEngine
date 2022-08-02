using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrickEngine.Editor.Gui.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class DragAttribute : Attribute
    {
        public readonly float Speed;

        public DragAttribute(float speed = 0.1f)
        {
            Speed = speed;
        }
    }
}

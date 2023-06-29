using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrickEngine.Example.RayTracing
{
    public struct Split
    {
        public int Axis;
        public float Cost;
        public int RightBin;

        public static Split Create()
        {
            return new Split { Axis = 0, Cost = float.MaxValue, RightBin = 0 };
        }

        public static Split Min(Split a, Split b)
        {
            if (a.Cost == b.Cost)
            {
                return a;
            }
            return a.Cost < b.Cost ? a : b;
        }
    }
}

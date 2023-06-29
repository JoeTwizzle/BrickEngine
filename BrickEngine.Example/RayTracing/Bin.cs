using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrickEngine.Example.RayTracing
{
    public struct Bin
    {
        public BoundingBox BoundingBox;
        public int PrimCount;

        public static Bin Create()
        {
            return new Bin { BoundingBox = BoundingBox.Empty, PrimCount = 0 };
        }

        public void Extend(in Bin other)
        {
            BoundingBox.Extend(other.BoundingBox);
            PrimCount += other.PrimCount;
        }

        public float Cost => BoundingBox.HalfArea * PrimCount;
    }
}

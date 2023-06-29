using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BrickEngine.Example.RayTracing
{
    public struct Ray
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        float copysign(float mag, float sgn)
        {
            return (float)((float)MathF.Abs(mag) * (float)(sgn >= 0 ? 1f : -1f));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        float safe_inverse(float x)
        {
            return MathF.Abs(x) <= float.Epsilon
                ? copysign(999999999f, x)//Really large value
                : 1.0f / x;
        }
        public Vector3 org, dir;
        public float tmin, tmax;
        public Vector3 InverseDir()
        {
            return new Vector3(safe_inverse(dir.X), safe_inverse(dir.Y), safe_inverse(dir.Z));
        }
    };
}

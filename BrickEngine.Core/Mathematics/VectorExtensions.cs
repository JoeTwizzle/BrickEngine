using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BrickEngine.Core.Mathematics
{
    public static class VectorExtensions
    {
        #region Normalize
        public static Vector4 Normalized(this Vector4 self)
        {
            return Vector4.Normalize(self);
        }    
        public static Vector3 Normalized(this Vector3 self)
        {
            return Vector3.Normalize(self);
        }     
        public static Vector2 Normalized(this Vector2 self)
        {
            return Vector2.Normalize(self);
        }    
        #endregion

        #region Swizzle
        /// <summary>
        /// Gets or sets a Vector2 with the v.X and v.Y components of this instance.
        /// </summary>

        public static Vector2 Xy(this Vector4 v)
        {
            return Unsafe.As<Vector4, Vector2>(ref v);
        }

        /// <summary>
        /// Gets or sets a Vector2 with the v.X and v.Z components of this instance.
        /// </summary>

        public static Vector2 Xz(this Vector4 v)
        {
            return new Vector2(v.X, v.Z);
        }

        /// <summary>
        /// Gets or sets a Vector2 with the v.X and v.W components of this instance.
        /// </summary>

        public static Vector2 Xw(this Vector4 v)
        {
            return new Vector2(v.X, v.W);
        }

        /// <summary>
        /// Gets or sets a Vector2 with the v.Y and v.X components of this instance.
        /// </summary>

        public static Vector2 Yx(this Vector4 v)
        {
            return new Vector2(v.Y, v.X);
        }

        /// <summary>
        /// Gets or sets a Vector2 with the v.Y and v.Z components of this instance.
        /// </summary>

        public static Vector2 Yz(this Vector4 v)
        {
            return new Vector2(v.Y, v.Z);
        }

        /// <summary>
        /// Gets or sets a Vector2 with the v.Y and v.W components of this instance.
        /// </summary>

        public static Vector2 Yw(this Vector4 v)
        {
            return new Vector2(v.Y, v.W);
        }

        /// <summary>
        /// Gets or sets a Vector2 with the v.Z and v.X components of this instance.
        /// </summary>

        public static Vector2 Zx(this Vector4 v)
        {
            return new Vector2(v.Z, v.X);
        }

        /// <summary>
        /// Gets or sets a Vector2 with the v.Z and v.Y components of this instance.
        /// </summary>

        public static Vector2 Zy(this Vector4 v)
        {
            return new Vector2(v.Z, v.Y);
        }

        /// <summary>
        /// Gets or sets a Vector2 with the v.Z and v.W components of this instance.
        /// </summary>

        public static Vector2 Zw(this Vector4 v)
        {
            return new Vector2(v.Z, v.W);
        }

        /// <summary>
        /// Gets or sets a Vector2 with the v.W and v.X components of this instance.
        /// </summary>

        public static Vector2 Wx(this Vector4 v)
        {
            return new Vector2(v.W, v.X);
        }

        /// <summary>
        /// Gets or sets a Vector2 with the v.W and v.Y components of this instance.
        /// </summary>

        public static Vector2 Wy(this Vector4 v)
        {
            return new Vector2(v.W, v.Y);
        }

        /// <summary>
        /// Gets or sets a Vector2 with the v.W and v.Z components of this instance.
        /// </summary>

        public static Vector2 Wz(this Vector4 v)
        {
            return new Vector2(v.W, v.Z);
        }

        /// <summary>
        /// Gets or sets a Vector3 with the v.X, v.Y, and v.Z components of this instance.
        /// </summary>

        public static Vector3 Xyz(this Vector4 v)
        {
            return Unsafe.As<Vector4, Vector3>(ref v);
        }

        /// <summary>
        /// Gets or sets a Vector3 with the v.X, v.Y, and v.Z components of this instance.
        /// </summary>

        public static Vector3 Xyw(this Vector4 v)
        {
            return new Vector3(v.X, v.Y, v.W);
        }

        /// <summary>
        /// Gets or sets a Vector3 with the v.X, v.Z, and v.Y components of this instance.
        /// </summary>

        public static Vector3 Xzy(this Vector4 v)
        {
            return new Vector3(v.X, v.Z, v.Y);
        }

        /// <summary>
        /// Gets or sets a Vector3 with the v.X, v.Z, and v.W components of this instance.
        /// </summary>

        public static Vector3 Xzw(this Vector4 v)
        {
            return new Vector3(v.X, v.Z, v.W);
        }

        /// <summary>
        /// Gets or sets a Vector3 with the v.X, v.W, and v.Y components of this instance.
        /// </summary>

        public static Vector3 Xwy(this Vector4 v)
        {
            return new Vector3(v.X, v.W, v.Y);
        }

        /// <summary>
        /// Gets or sets a Vector3 with the v.X, v.W, and v.Z components of this instance.
        /// </summary>

        public static Vector3 Xwz(this Vector4 v)
        {
            return new Vector3(v.X, v.W, v.Z);
        }

        /// <summary>
        /// Gets or sets a Vector3 with the v.Y, v.X, and v.Z components of this instance.
        /// </summary>

        public static Vector3 Yxz(this Vector4 v)
        {
            return new Vector3(v.Y, v.X, v.Z);
        }

        /// <summary>
        /// Gets or sets a Vector3 with the v.Y, v.X, and v.W components of this instance.
        /// </summary>

        public static Vector3 Yxw(this Vector4 v)
        {
            return new Vector3(v.Y, v.X, v.W);
        }

        /// <summary>
        /// Gets or sets a Vector3 with the v.Y, v.Z, and v.X components of this instance.
        /// </summary>

        public static Vector3 Yzx(this Vector4 v)
        {
            return new Vector3(v.Y, v.Z, v.X);
        }

        /// <summary>
        /// Gets or sets a Vector3 with the v.Y, v.Z, and v.W components of this instance.
        /// </summary>

        public static Vector3 Yzw(this Vector4 v)
        {
            return new Vector3(v.Y, v.Z, v.W);
        }

        /// <summary>
        /// Gets or sets a Vector3 with the v.Y, v.W, and v.X components of this instance.
        /// </summary>

        public static Vector3 Ywx(this Vector4 v)
        {
            return new Vector3(v.Y, v.W, v.X);
        }

        /// <summary>
        /// Gets or sets a Vector3 with the v.Y, v.W, and v.Z components of this instance.
        /// </summary>

        public static Vector3 Ywz(this Vector4 v)
        {
            return new Vector3(v.Y, v.W, v.Z);
        }

        /// <summary>
        /// Gets or sets a Vector3 with the v.Z, v.X, and v.Y components of this instance.
        /// </summary>

        public static Vector3 Zxy(this Vector4 v)
        {
            return new Vector3(v.Z, v.X, v.Y);
        }

        /// <summary>
        /// Gets or sets a Vector3 with the v.Z, v.X, and v.W components of this instance.
        /// </summary>

        public static Vector3 Zxw(this Vector4 v)
        {
            return new Vector3(v.Z, v.X, v.W);
        }

        /// <summary>
        /// Gets or sets a Vector3 with the v.Z, v.Y, and v.X components of this instance.
        /// </summary>

        public static Vector3 Zyx(this Vector4 v)
        {
            return new Vector3(v.Z, v.Y, v.X);
        }

        /// <summary>
        /// Gets or sets a Vector3 with the v.Z, v.Y, and v.W components of this instance.
        /// </summary>

        public static Vector3 Zyw(this Vector4 v)
        {
            return new Vector3(v.Z, v.Y, v.W);
        }

        /// <summary>
        /// Gets or sets a Vector3 with the v.Z, v.W, and v.X components of this instance.
        /// </summary>

        public static Vector3 Zwx(this Vector4 v)
        {
            return new Vector3(v.Z, v.W, v.X);
        }

        /// <summary>
        /// Gets or sets a Vector3 with the v.Z, v.W, and v.Y components of this instance.
        /// </summary>

        public static Vector3 Zwy(this Vector4 v)
        {
            return new Vector3(v.Z, v.W, v.Y);
        }

        /// <summary>
        /// Gets or sets a Vector3 with the v.W, v.X, and v.Y components of this instance.
        /// </summary>

        public static Vector3 Wxy(this Vector4 v)
        {
            return new Vector3(v.W, v.X, v.Y);
        }

        /// <summary>
        /// Gets or sets a Vector3 with the v.W, v.X, and v.Z components of this instance.
        /// </summary>

        public static Vector3 Wxz(this Vector4 v)
        {
            return new Vector3(v.W, v.X, v.Z);
        }

        /// <summary>
        /// Gets or sets a Vector3 with the v.W, v.Y, and v.X components of this instance.
        /// </summary>

        public static Vector3 Wyx(this Vector4 v)
        {
            return new Vector3(v.W, v.Y, v.X);
        }

        /// <summary>
        /// Gets or sets a Vector3 with the v.W, v.Y, and v.Z components of this instance.
        /// </summary>

        public static Vector3 Wyz(this Vector4 v)
        {
            return new Vector3(v.W, v.Y, v.Z);
        }

        /// <summary>
        /// Gets or sets a Vector3 with the v.W, v.Z, and v.X components of this instance.
        /// </summary>

        public static Vector3 Wzx(this Vector4 v)
        {
            return new Vector3(v.W, v.Z, v.X);
        }

        /// <summary>
        /// Gets or sets a Vector3 with the v.W, v.Z, and v.Y components of this instance.
        /// </summary>

        public static Vector3 Wzy(this Vector4 v)
        {
            return new Vector3(v.W, v.Z, v.Y);
        }

        /// <summary>
        /// Gets or sets a Vector4 with the v.X, v.Y, v.W, and v.Z components of this instance.
        /// </summary>

        public static Vector4 Xywz(this Vector4 v)
        {
            return new Vector4(v.X, v.Y, v.W, v.Z);
        }

        /// <summary>
        /// Gets or sets a Vector4 with the v.X, v.Z, v.Y, and v.W components of this instance.
        /// </summary>

        public static Vector4 Xzyw(this Vector4 v)
        {
            return new Vector4(v.X, v.Z, v.Y, v.W);
        }

        /// <summary>
        /// Gets or sets a Vector4 with the v.X, v.Z, v.W, and v.Y components of this instance.
        /// </summary>

        public static Vector4 Xzwy(this Vector4 v)
        {
            return new Vector4(v.X, v.Z, v.W, v.Y);
        }

        /// <summary>
        /// Gets or sets a Vector4 with the v.X, v.W, v.Y, and v.Z components of this instance.
        /// </summary>

        public static Vector4 Xwyz(this Vector4 v)
        {
            return new Vector4(v.X, v.W, v.Y, v.Z);
        }

        /// <summary>
        /// Gets or sets a Vector4 with the v.X, v.W, v.Z, and v.Y components of this instance.
        /// </summary>

        public static Vector4 Xwzy(this Vector4 v)
        {
            return new Vector4(v.X, v.W, v.Z, v.Y);
        }

        /// <summary>
        /// Gets or sets a Vector4 with the v.Y, v.X, v.Z, and v.W components of this instance.
        /// </summary>

        public static Vector4 Yxzw(this Vector4 v)
        {
            return new Vector4(v.Y, v.X, v.Z, v.W);
        }

        /// <summary>
        /// Gets or sets a Vector4 with the v.Y, v.X, v.W, and v.Z components of this instance.
        /// </summary>

        public static Vector4 Yxwz(this Vector4 v)
        {
            return new Vector4(v.Y, v.X, v.W, v.Z);
        }

        /// <summary>
        /// Gets or sets a Vector4 with the v.Y, v.Y, v.Z, and v.W components of this instance.
        /// </summary>

        public static Vector4 Yyzw(this Vector4 v)
        {
            return new Vector4(v.Y, v.Y, v.Z, v.W);
        }

        /// <summary>
        /// Gets or sets a Vector4 with the v.Y, v.Y, v.W, and v.Z components of this instance.
        /// </summary>

        public static Vector4 Yywz(this Vector4 v)
        {
            return new Vector4(v.Y, v.Y, v.W, v.Z);
        }

        /// <summary>
        /// Gets or sets a Vector4 with the v.Y, v.Z, v.X, and v.W components of this instance.
        /// </summary>

        public static Vector4 Yzxw(this Vector4 v)
        {
            return new Vector4(v.Y, v.Z, v.X, v.W);
        }

        /// <summary>
        /// Gets or sets a Vector4 with the v.Y, v.Z, v.W, and v.X components of this instance.
        /// </summary>

        public static Vector4 Yzwx(this Vector4 v)
        {
            return new Vector4(v.Y, v.Z, v.W, v.X);
        }

        /// <summary>
        /// Gets or sets a Vector4 with the v.Y, v.W, v.X, and v.Z components of this instance.
        /// </summary>

        public static Vector4 Ywxz(this Vector4 v)
        {
            return new Vector4(v.Y, v.W, v.X, v.Z);
        }

        /// <summary>
        /// Gets or sets a Vector4 with the v.Y, v.W, v.Z, and v.X components of this instance.
        /// </summary>

        public static Vector4 Ywzx(this Vector4 v)
        {
            return new Vector4(v.Y, v.W, v.Z, v.X);
        }

        /// <summary>
        /// Gets or sets a Vector4 with the v.Z, v.X, v.Y, and v.Z components of this instance.
        /// </summary>

        public static Vector4 Zxyw(this Vector4 v)
        {
            return new Vector4(v.Z, v.X, v.Y, v.W);
        }

        /// <summary>
        /// Gets or sets a Vector4 with the v.Z, v.X, v.W, and v.Y components of this instance.
        /// </summary>

        public static Vector4 Zxwy(this Vector4 v)
        {
            return new Vector4(v.Z, v.X, v.W, v.Y);
        }

        /// <summary>
        /// Gets or sets a Vector4 with the v.Z, v.Y, v.X, and v.W components of this instance.
        /// </summary>

        public static Vector4 Zyxw(this Vector4 v)
        {
            return new Vector4(v.Z, v.Y, v.X, v.W);
        }

        /// <summary>
        /// Gets or sets a Vector4 with the v.Z, v.Y, v.W, and v.X components of this instance.
        /// </summary>

        public static Vector4 Zywx(this Vector4 v)
        {
            return new Vector4(v.Z, v.Y, v.W, v.X);
        }

        /// <summary>
        /// Gets or sets a Vector4 with the v.Z, v.W, v.X, and v.Y components of this instance.
        /// </summary>

        public static Vector4 Zwxy(this Vector4 v)
        {
            return new Vector4(v.Z, v.W, v.X, v.Y);
        }

        /// <summary>
        /// Gets or sets a Vector4 with the v.Z, v.W, v.Y, and v.X components of this instance.
        /// </summary>

        public static Vector4 Zwyx(this Vector4 v)
        {
            return new Vector4(v.Z, v.W, v.Y, v.X);
        }

        /// <summary>
        /// Gets or sets a Vector4 with the v.Z, v.W, v.Z, and v.Y components of this instance.
        /// </summary>

        public static Vector4 Zwzy(this Vector4 v)
        {
            return new Vector4(v.Z, v.W, v.Z, v.Y);
        }

        /// <summary>
        /// Gets or sets a Vector4 with the v.W, v.X, v.Y, and v.Z components of this instance.
        /// </summary>

        public static Vector4 Wxyz(this Vector4 v)
        {
            return new Vector4(v.W, v.X, v.Y, v.Z);
        }

        /// <summary>
        /// Gets or sets a Vector4 with the v.W, v.X, v.Z, and v.Y components of this instance.
        /// </summary>

        public static Vector4 Wxzy(this Vector4 v)
        {
            return new Vector4(v.W, v.X, v.Z, v.Y);
        }

        /// <summary>
        /// Gets or sets a Vector4 with the v.W, v.Y, v.X, and v.Z components of this instance.
        /// </summary>

        public static Vector4 Wyxz(this Vector4 v)
        {
            return new Vector4(v.W, v.Y, v.X, v.Z);
        }

        /// <summary>
        /// Gets or sets a Vector4 with the v.W, v.Y, v.Z, and v.X components of this instance.
        /// </summary>

        public static Vector4 Wyzx(this Vector4 v)
        {
            return new Vector4(v.W, v.Y, v.Z, v.X);
        }

        /// <summary>
        /// Gets or sets a Vector4 with the v.W, v.Z, v.X, and v.Y components of this instance.
        /// </summary>

        public static Vector4 Wzxy(this Vector4 v)
        {
            return new Vector4(v.W, v.Z, v.X, v.Y);
        }

        /// <summary>
        /// Gets or sets a Vector4 with the v.W, v.Z, v.Y, and v.X components of this instance.
        /// </summary>

        public static Vector4 Wzyx(this Vector4 v)
        {
            return new Vector4(v.W, v.Z, v.Y, v.X);
        }

        /// <summary>
        /// Gets or sets a Vector4 with the v.W, v.Z, v.Y, and v.W components of this instance.
        /// </summary>

        public static Vector4 Wzyw(this Vector4 v)
        {
            return new Vector4(v.W, v.Z, v.Y, v.W);
        }


        /// <summary>
        /// Gets or sets a Vector2 with the v.X and v.Y components of this instance.
        /// </summary>

        public static Vector2 Xy(this Vector3 v)
        {
            return Unsafe.As<Vector3, Vector2>(ref v);
        }

        /// <summary>
        /// Gets or sets a Vector2 with the v.X and v.Z components of this instance.
        /// </summary>

        public static Vector2 Xz(this Vector3 v)
        {
            return new Vector2(v.X, v.Z);
        }

        /// <summary>
        /// Gets or sets a Vector2 with the v.Y and v.X components of this instance.
        /// </summary>

        public static Vector2 Yx(this Vector3 v)
        {
            return new Vector2(v.Y, v.X);
        }

        /// <summary>
        /// Gets or sets a Vector2 with the v.Y and v.X components of this instance.
        /// </summary>

        public static Vector2 Yx(this Vector2 v)
        {
            return new Vector2(v.Y, v.X);
        }

        /// <summary>
        /// Gets or sets a Vector2 with the v.Y and v.Z components of this instance.
        /// </summary>

        public static Vector2 Yz(this Vector3 v)
        {
            return new Vector2(v.Y, v.Z);
        }

        /// <summary>
        /// Gets or sets a Vector2 with the v.Z and v.X components of this instance.
        /// </summary>

        public static Vector2 Zx(this Vector3 v)
        {
            return new Vector2(v.Z, v.X);
        }

        /// <summary>
        /// Gets or sets a Vector2 with the v.Z and v.Y components of this instance.
        /// </summary>

        public static Vector2 Zy(this Vector3 v)
        {
            return new Vector2(v.Z, v.Y);
        }

        /// <summary>
        /// Gets or sets a Vector3 with the v.X, v.Z, and v.Y components of this instance.
        /// </summary>

        public static Vector3 Xzy(this Vector3 v)
        {
            return new Vector3(v.X, v.Z, v.Y);
        }

        /// <summary>
        /// Gets or sets a Vector3 with the v.Y, v.X, and v.Z components of this instance.
        /// </summary>

        public static Vector3 Yxz(this Vector3 v)
        {
            return new Vector3(v.Y, v.X, v.Z);
        }

        /// <summary>
        /// Gets or sets a Vector3 with the v.Y, v.Z, and v.X components of this instance.
        /// </summary>

        public static Vector3 Yzx(this Vector3 v)
        {
            return new Vector3(v.Y, v.Z, v.X);
        }

        /// <summary>
        /// Gets or sets a Vector3 with the v.Z, v.X, and v.Y components of this instance.
        /// </summary>

        public static Vector3 Zxy(this Vector3 v)
        {
            return new Vector3(v.Z, v.X, v.Y);
        }

        /// <summary>
        /// Gets or sets a Vector3 with the v.Z, v.Y, and v.X components of this instance.
        /// </summary>

        public static Vector3 Zyx(this Vector3 v)
        {
            return new Vector3(v.Z, v.Y, v.X);
        }
        #endregion
    }
}

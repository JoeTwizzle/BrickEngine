using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrickEngine.Core.Utilities
{
    public static class MatrixExtensions
    {

        // Most of these are from OpenTK

        /// <summary>
        /// Returns the translation component of this instance.
        /// </summary>
        /// <returns>The translation.</returns>
        public static Vector3 ExtractTranslation(this Matrix4x4 matrix)
        {
            return new Vector3(matrix.M41, matrix.M42, matrix.M43);
        }

        /// <summary>
        /// Returns the scale component of this instance.
        /// </summary>
        /// <returns>The scale.</returns>
        public static Vector3 ExtractScale(this Matrix4x4 matrix)
        {
            var row0 = new Vector3(matrix.M11, matrix.M12, matrix.M13);
            var row1 = new Vector3(matrix.M21, matrix.M22, matrix.M23);
            var row2 = new Vector3(matrix.M31, matrix.M32, matrix.M33);
            return new Vector3(row0.Length(), row1.Length(), row2.Length());
        }

        /// <summary>
        /// Returns the rotation component of this instance. Quite slow.
        /// </summary>
        /// <param name="rowNormalize">
        /// Whether the method should row-normalize (i.e. remove scale from) the Matrix. Pass false if
        /// you know it's already normalized.
        /// </param>
        /// <returns>The rotation.</returns>
        public static Quaternion ExtractRotation(this Matrix4x4 matrix, bool rowNormalize = true)
        {
            var row0 = new Vector3(matrix.M11, matrix.M12, matrix.M13);
            var row1 = new Vector3(matrix.M21, matrix.M22, matrix.M23);
            var row2 = new Vector3(matrix.M31, matrix.M32, matrix.M33);

            if (rowNormalize)
            {
                row0 = Vector3.Normalize(row0);
                row1 = Vector3.Normalize(row1);
                row2 = Vector3.Normalize(row2);
            }

            // code below adapted from Blender
            var q = default(Quaternion);
            var trace = 0.25 * (row0.X + row1.Y + row2.Z + 1.0);

            if (trace > 0)
            {
                var sq = Math.Sqrt(trace);

                q.W = (float)sq;
                sq = 1.0 / (4.0 * sq);
                q.X = (float)((row1.Z - row2.Y) * sq);
                q.Y = (float)((row2.X - row0.Z) * sq);
                q.Z = (float)((row0.Y - row1.X) * sq);
            }
            else if (row0.X > row1.Y && row0.X > row2.Z)
            {
                var sq = 2.0 * Math.Sqrt(1.0 + row0.X - row1.Y - row2.Z);

                q.X = (float)(0.25 * sq);
                sq = 1.0 / sq;
                q.W = (float)((row2.Y - row1.Z) * sq);
                q.Y = (float)((row1.X + row0.Y) * sq);
                q.Z = (float)((row2.X + row0.Z) * sq);
            }
            else if (row1.Y > row2.Z)
            {
                var sq = 2.0 * Math.Sqrt(1.0 + row1.Y - row0.X - row2.Z);

                q.Y = (float)(0.25 * sq);
                sq = 1.0 / sq;
                q.W = (float)((row2.X - row0.Z) * sq);
                q.X = (float)((row1.X + row0.Y) * sq);
                q.Z = (float)((row2.Y + row1.Z) * sq);
            }
            else
            {
                var sq = 2.0 * Math.Sqrt(1.0 + row2.Z - row0.X - row1.Y);

                q.Z = (float)(0.25 * sq);
                sq = 1.0 / sq;
                q.W = (float)((row1.X - row0.Y) * sq);
                q.X = (float)((row2.X + row0.Z) * sq);
                q.Y = (float)((row2.Y + row1.Z) * sq);
            }
            return Quaternion.Normalize(q);
        }
    }
}

using BrickEngine.Core.Utilities;
using System.Numerics;

namespace BrickEngine.Example
{
    sealed class Transform
    {
        #region Fields

        #region Local
        public bool matrixDirty = true;
        private Vector3 localPosition = new Vector3();
        private Quaternion localRotation = Quaternion.Identity;
        private Vector3 localScale = new Vector3(1, 1, 1);
        private Matrix4x4 worldMatrix;
        //private Transform? parent;
        //private readonly List<Transform> children = new List<Transform>();

        #endregion

        #endregion

        #region Properties

        //public Transform? Parent
        //{
        //    get => parent;
        //    set
        //    {
        //        if (value != null && value.GameObject.Scene != GameObject.Scene)
        //        {
        //            throw new Exception("Parent does not exist in the same scene.");
        //        }
        //        if (value != null && (value == this || IsChildOfRecursive(this, value)))
        //        {
        //            return;
        //        }
        //        if (parent != null)
        //        {
        //            parent.children.Remove(this);
        //        }
        //        parent = value;
        //        parent?.children.Add(this);
        //        if (GameObject?.Scene != null)
        //        {
        //            GameObject.Scene.InvokeGraphChange();
        //        }
        //    }
        //}

        //public static bool IsChildOf(Transform scr, Transform potentialChild)
        //{
        //    bool isChild = false;
        //    for (int i = 0; i < scr.children.Count; i++)
        //    {
        //        isChild = scr.children[i] == potentialChild;

        //        if (isChild)
        //        {
        //            break;
        //        }
        //    }
        //    return isChild;
        //}

        //public static bool IsChildOfRecursive(Transform scr, Transform potentialChild)
        //{
        //    bool isChild = false;
        //    for (int i = 0; i < scr.children.Count; i++)
        //    {
        //        isChild = scr.children[i] == potentialChild;
        //        //Depth first search
        //        if (!isChild)
        //        {
        //            isChild = IsChildOfRecursive(scr.children[i], potentialChild);
        //        }
        //        if (isChild)
        //        {
        //            break;
        //        }
        //    }
        //    return isChild;
        //}

        //public IReadOnlyList<Transform> Children
        //{
        //    get { return children; }
        //}

        #region Local
        public Vector3 LocalPosition
        {
            get => localPosition; set
            {
                matrixDirty = true;
                localPosition = value;
            }
        }
        public Quaternion LocalRotation
        {
            get => localRotation; set
            {
                matrixDirty = true;
                localRotation = value;
            }
        }
        public Vector3 LocalScale
        {
            get => localScale; set
            {
                matrixDirty = true;
                localScale = value;
            }
        }
        #endregion

        #region World

        public Vector3 Position
        {
            get
            {

                return LocalPosition;

            }

            set
            {
                matrixDirty = true;

                LocalPosition = value;

            }
        }

        public Quaternion Rotation
        {
            get
            {

                return LocalRotation;

            }
            set
            {
                matrixDirty = true;

                LocalRotation = value;

            }
        }

        public Vector3 Scale
        {
            get
            {

                return LocalScale;

            }

            set
            {
                matrixDirty = true;

                LocalScale = value;

            }
        }
        #endregion

        #endregion


        #region Matrix
        public Matrix4x4 WorldMatrix
        {
            get
            {
                if (matrixDirty)
                {
                    worldMatrix = Matrix4x4.CreateScale(localScale)
                * Matrix4x4.CreateFromQuaternion(localRotation)
                * Matrix4x4.CreateTranslation(localPosition);
                    matrixDirty = false;
                }
                return worldMatrix;

            }

            set
            {
                Scale = value.ExtractScale();
                Rotation = value.ExtractRotation();
                Position = value.ExtractTranslation();
                worldMatrix = value;
            }
        }

        #endregion


        #region Unit Axes

        #region Local
        public Vector3 LocalForward
        {
            get
            {
                return Vector3.Transform(Vector3.UnitZ, LocalRotation);
            }
        }
        public Vector3 LocalUp
        {
            get
            {
                return Vector3.Transform(Vector3.UnitY, LocalRotation);
            }
        }
        public Vector3 LocalRight
        {
            get
            {
                return Vector3.Transform(Vector3.UnitX, LocalRotation);
            }
        }


        #endregion

        #region World
        public Vector3 Forward
        {
            get
            {
                return Vector3.Transform(Vector3.UnitZ, Rotation);
            }
        }
        public Vector3 Up
        {
            get
            {
                return Vector3.Transform(Vector3.UnitY, Rotation);
            }
        }

        public Vector3 Right
        {
            get
            {
                return Vector3.Transform(Vector3.UnitX, Rotation);
            }
        }

        #endregion

        #endregion

        public static Transform Create()
        {
            return new Transform();
        }

        public static Transform Create(System.Numerics.Matrix4x4 matrix)
        {
            var t = new Transform();
            t.SetLocalTransform(matrix);
            return t;
        }

        Transform()
        {
            matrixDirty = true;
        }

        public void SetTransform(Matrix4x4 matrix)
        {
            matrixDirty = true;
            var mat = new Matrix4x4(
             matrix.M11, matrix.M12, matrix.M13, matrix.M14,
             matrix.M21, matrix.M22, matrix.M23, matrix.M24,
             matrix.M31, matrix.M32, matrix.M33, matrix.M34,
             matrix.M41, matrix.M42, matrix.M43, matrix.M44);
            Position = mat.ExtractTranslation();
            Rotation = mat.ExtractRotation();
            Scale = mat.ExtractScale();
        }

        public void SetLocalTransform(Matrix4x4 matrix)
        {
            matrixDirty = true;
            var mat = new Matrix4x4(
             matrix.M11, matrix.M12, matrix.M13, matrix.M14,
             matrix.M21, matrix.M22, matrix.M23, matrix.M24,
             matrix.M31, matrix.M32, matrix.M33, matrix.M34,
             matrix.M41, matrix.M42, matrix.M43, matrix.M44);
            localPosition = mat.ExtractTranslation();
            localRotation = mat.ExtractRotation();
            localScale = mat.ExtractScale();
        }

        internal Transform CopyTemporary()
        {
            var t = new Transform();
            t.WorldMatrix = WorldMatrix;
            t.Position = Position;
            t.Rotation = Rotation;
            t.Scale = Scale;
            return t;
        }
    }
}

using BrickEngine.Core;
using BrickEngine.Core.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Veldrid;
namespace BrickEngine.Example
{
    sealed class Camera
    {
        public bool KeepAspect { get; set; }
        public bool IsPerspective { get; set; } = true;
        public int Priority { get; set; }
        public Matrix4x4 ViewMatrix { get { return Matrix4x4.CreateLookAt(Transform.LocalPosition, Transform.LocalPosition + Transform.LocalForward, Transform.LocalUp); } }
        public Matrix4x4 PerspectiveMatrix { get { return ComputePerspective(); } }

        public Matrix4x4 OrthographicMatrix { get { return Matrix4x4.CreateOrthographicOffCenter(-64, 64, -64, 64, far, near); } }
        public Matrix4x4 ProjectionMatrix { get { return IsPerspective ? (PerspectiveMatrix * ViewMatrix) : (ViewMatrix * OrthographicMatrix); } }

        const float ToRadians = (MathF.PI / 180f);
        const float ToDegrees = (180f / MathF.PI);

        private int msaa = 2;
        public int MSAA
        {
            get => msaa; set
            {
                msaa = Math.Max(value, 1);
            }
        }

        private float fov = 90;
        public float AspectRatio { get; set; } = 1;
        private float near;
        private float far;

        Matrix4x4 ComputePerspective()
        {
            return Matrix4x4.CreatePerspectiveFieldOfView(ToRadians * FOV, AspectRatio, Near, Far);
            ////float f = 1.0f / MathF.Tan(MathHelper.DegreesToRadians(FOV) / 2.0f);
            //try
            //{
            //    float f = 1 / MathF.Tan(MathHelper.DegreesToRadians(FOV) / 2f);
            //    var result = new Matrix4(f / (viewport.Size.X / (float)viewport.Size.Y), 0, 0, 0,
            //        0, f, 0, 0,
            //        0, 0, 0, -1,
            //        0, 0, near, 0);
            //    //var result = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(FOV), RenderTexture.Width / (float)RenderTexture.Height, Near, Far);
            //    //result.Row2.X = 0;
            //    //result.Row2.Y = 0;
            //    //result.Row2.Z = (Near / (Far - Near));
            //    //result.Row3.Z = (Far * Near) / (Far - Near);
            //    return result;
            //}
            //catch
            //{
            //    return Matrix4.Identity;
            //}

        }

        Transform transform;
        public Transform Transform
        {
            get
            {
                return transform;
            }
        }

        public float Near
        {
            get => near; set
            {
                if (value <= 0)
                {
                    return;
                }
                near = value;
            }
        }
        public float Far
        {
            get => far; set
            {
                if (value <= near)
                {
                    return;
                }
                far = value;
            }
        }
        public float FOV
        {
            get => fov; set
            {
                fov = float.Clamp(value, 0.1f, 179.9f);
            }
        }

        public Camera()
        {
            near = 0.06f;
            far = 10000;
            FOV = 90f;
            transform = Transform.Create();
        }

        float angleX, angleY;

        public void Update(Input Input, float dt)
        {
            float speedRot = 4f;
            float speed = 8f;
            if (Input.GetKey(Key.LeftControl))
            {
                speed = 16f;
            }
            var fwd = Transform.LocalForward;
            fwd = new Vector3(fwd.X, 0, fwd.Z).Normalized();
            if (Input.GetKey(Key.W))
            {
                Transform.LocalPosition -= fwd * speed * dt;
            }
            if (Input.GetKey(Key.S))
            {
                Transform.LocalPosition += fwd * speed * dt;
            }
            if (Input.GetKey(Key.A))
            {
                Transform.LocalPosition -= Transform.LocalRight * speed * dt;
            }
            if (Input.GetKey(Key.D))
            {
                Transform.LocalPosition += Transform.LocalRight * speed * dt;
            }
            if (Input.GetKey(Key.Space))
            {
                Transform.LocalPosition += new Vector3(0, dt, 0) * speed;
            }
            if (Input.GetKey(Key.LeftShift))
            {
                Transform.LocalPosition -= new Vector3(0, dt, 0) * speed;
            }

            if (Input.GetKey(Key.Left))
            {
                angleX += dt * speedRot;
            }
            if (Input.GetKey(Key.Right))
            {
                angleX -= dt * speedRot;
            }
            if (Input.GetKey(Key.Down))
            {
                angleY -= dt * speedRot;
            }
            if (Input.GetKey(Key.Up))
            {
                angleY += dt * speedRot;
            }
            if (Input.GetKey(Key.Return))
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
            Transform.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, angleX) * Quaternion.CreateFromAxisAngle(Vector3.UnitX, angleY);
        }
    }
}

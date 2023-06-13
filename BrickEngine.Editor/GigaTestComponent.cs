using BrickEngine.Core.Mathematics;
using BrickEngine.Editor.Gui.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace BrickEngine.Editor
{
    struct GigaTestComponent : IComponent<GigaTestComponent>
    {
        public string Name;
        [Drag]
        public float speed;
        public Vector2 Position2d;
        public Vector3 Position;
        public Vector4 Position4d;
        public Vector2i SpriteSize;
        [NoDrag]
        public Vector3i BoundingBoxExtents;
        public Vector4i BoundingBox2d;
        [Range(0f, 1f)]
        public Vector2 Magic1;
        [Range(0f, 1f)]
        public Vector3 Magic2;
        [Range(0f, 1f)]
        public Vector4 Magic3;
        [Range(0f, 1f)]
        public Vector2i Magic4;
        [Range(0f, 1f)]
        public Vector3i Magic5;
        [Range(0f, 1f)]
        public Vector4i Magic6;
        public int ammo;
        [ColorPicker]
        public Vector4 color;
        public double money;
        [TextLength(10)]
        public string UsernameMax10;

        public void Init()
        {
            Position = new Vector3(0, 1, 0);
            Position4d = new Vector4(0, 1, 0, 0);
            Name = "John Madden";
            ammo = 100;
            color = new Vector4(102 / 255f, 89 / 255f, 227 / 255f, 1);
            money = 35.50;
            Position2d = new Vector2(32, 32);
            BoundingBoxExtents = new Vector3i(10, 10, 10);
            BoundingBox2d = new Vector4i(-10, -10, 10, 10);
        }
    }
}

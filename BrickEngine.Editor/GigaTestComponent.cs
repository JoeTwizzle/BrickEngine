using BrickEngine.Core.Mathematics;
using BrickEngine.Editor.Gui.Attributes;
using EcsLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace BrickEngine.Editor
{
    struct GigaTestComponent : IComponent, IEcsInit<GigaTestComponent>
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

        public static void OnInit(ref GigaTestComponent c)
        {
            c.Position = new Vector3(0, 1, 0);
            c.Position4d = new Vector4(0, 1, 0, 0);
            c.Name = "John Madden";
            c.ammo = 100;
            c.color = new Vector4(102 / 255f, 89 / 255f, 227 / 255f, 1);
            c.money = 35.50;
            c.Position2d = new Vector2(32, 32);
            c.BoundingBoxExtents = new Vector3i(10, 10, 10);
            c.BoundingBox2d = new Vector4i(-10, -10, 10, 10);
        }
    }
}

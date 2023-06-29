global using MemoryPack;
using System;
using System.Collections.Generic;
using System.Linq;
using BinSerialize;
using System.Threading.Tasks;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Reflection.Metadata;
using static BrickEngine.Assets.Data.VisualScene;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Xml.Linq;

namespace BrickEngine.Assets.Data
{
    [MemoryPackable(GenerateType.CircularReference)]
    public sealed partial class SceneNode
    {
        [MemoryPackOrder(0)]
        public string Name;
        [MemoryPackOrder(1)]
        public int? ParentIndex;
        [MemoryPackOrder(2)]
        public int[] Children;
        [MemoryPackOrder(3)]
        public Matrix4x4 LocalToWorld;
        [MemoryPackOrder(4)]
        public ModelData? Model;
        [MemoryPackConstructor]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public SceneNode()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {

        }
        public SceneNode(string name, int? parentIndex, int[] children, Matrix4x4 localToWorld, ModelData? model)
        {
            Name = name;
            ParentIndex = parentIndex;
            Children = children;
            LocalToWorld = localToWorld;
            Model = model;
        }

        public Matrix4x4 GetWorldTransfrom(SceneNode[] Nodes)
        {
            if (ParentIndex.HasValue)
            {
                return Nodes[ParentIndex.Value].GetWorldTransfrom(Nodes) * LocalToWorld;
            }
            return LocalToWorld;
        }
    }

    [MemoryPackable(GenerateType.CircularReference)]
    public sealed partial class VisualScene
    {
        [MemoryPackOrder(0)]
        public SceneNode[] Nodes;
        [MemoryPackConstructor]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public VisualScene()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
                
        }
        public VisualScene(SceneNode[] nodes)
        {
            Nodes = nodes;
        }
    }
}

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
    public class VisualScene : IBinarySerializable<VisualScene>
    {
        [MemoryPackable]
        public readonly partial struct SceneNode
        {
            public readonly string Name;
            public readonly int? ParentIndex;
            public readonly int[] Children;
            public readonly Matrix4x4 LocalToWorld;
            public readonly ModelData? Model;

            public SceneNode(string name, int? parentIndex, int[] children, Matrix4x4 localToWorld, ModelData? model)
            {
                Name = name;
                ParentIndex = parentIndex;
                Children = children;
                LocalToWorld = localToWorld;
                Model = model;
            }

            public readonly Matrix4x4 GetWorldTransfrom(SceneNode[] Nodes)
            {
                if (ParentIndex.HasValue)
                {
                    return Nodes[ParentIndex.Value].GetWorldTransfrom(Nodes) * LocalToWorld;
                }
                return LocalToWorld;
            }

            public static SceneNode Deserialize(ref ReadOnlySpan<byte> blob)
            {
                string name = BinarySerializer.ReadString(ref blob);
                var hasParent = BinarySerializer.ReadBool(ref blob);
                int? parent = null;
                if (hasParent)
                {
                    parent = BinarySerializer.ReadInt(ref blob);
                }
                Matrix4x4 mat = default;
                var destSpan = MemoryMarshal.CreateSpan(ref mat.M11, 4 * 4);
                BinarySerializer.Read(destSpan, 4 * 4, ref blob);

                ModelData? model = null;
                var hasModel = BinarySerializer.ReadBool(ref blob);
                if (hasModel)
                {
                    model = ModelData.Deserialize(ref blob);
                }
                int childCount = BinarySerializer.ReadPackedInt(ref blob);
                int[] children = new int[childCount];
                for (int j = 0; j < children.Length; j++)
                {
                    children[j] = BinarySerializer.ReadPackedInt(ref blob);
                }
                return new SceneNode(name, parent, children, mat, model);
            }

            public static void Serialize(ByteBufferWriter writer, SceneNode node)
            {
                int estimatedLength = sizeof(int);
                //Name
                estimatedLength += BinarySerializer.GetSizeForString(node.Name);
                //ParentIndex
                estimatedLength += sizeof(int) + sizeof(bool);
                //Matrix
                estimatedLength += sizeof(float) * 4 * 4;
                //ChildCount
                estimatedLength += sizeof(int);
                //ChildIndices
                estimatedLength += sizeof(int) * node.Children.Length;
                var span = writer.GetSpan(estimatedLength);

                BinarySerializer.WriteString(ref span, node.Name);
                BinarySerializer.WriteBool(ref span, node.ParentIndex.HasValue);
                if (node.ParentIndex.HasValue)
                {
                    BinarySerializer.WriteInt(ref span, node.ParentIndex.Value);
                }
                BinarySerializer.WriteStruct(ref span, node.LocalToWorld);
                var hasModel = node.Model != null;
                BinarySerializer.WriteBool(ref span, hasModel);
                if (hasModel)
                {
                    writer.ReturnSpanAndAdvance(ref span);
                    ModelData.Serialize(writer, node.Model!);
                    span = writer.GetSpan(sizeof(int) + sizeof(int) * node.Children.Length);
                }
                BinarySerializer.WritePackedInt(ref span, node.Children.Length);
                for (int j = 0; j < node.Children.Length; j++)
                {
                    BinarySerializer.WritePackedInt(ref span, node.Children[j]);
                }
                writer.ReturnSpanAndAdvance(ref span);
            }
        }

        public readonly SceneNode[] Nodes;

        public VisualScene(SceneNode[] nodes)
        {
            Nodes = nodes;
        }

        public static VisualScene Deserialize(ref ReadOnlySpan<byte> blob)
        {
            SceneNode[] nodes = new SceneNode[BinarySerializer.ReadPackedInt(ref blob)];
            for (int i = 0; i < nodes.Length; i++)
            {
                nodes[i] = SceneNode.Deserialize(ref blob);
            }
            return new VisualScene(nodes);
        }

        public static void Serialize(ByteBufferWriter writer, VisualScene data)
        {
            var span = writer.GetSpan(sizeof(int));
            BinarySerializer.WritePackedInt(ref span, data.Nodes.Length);
            writer.ReturnSpanAndAdvance(ref span);
            for (int i = 0; i < data.Nodes.Length; i++)
            {
                SceneNode.Serialize(writer, data.Nodes[i]);
            }
        }
    }
}

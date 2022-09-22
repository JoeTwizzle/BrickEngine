using System;
using System.Collections.Generic;
using System.Linq;
using BinSerialize;
using System.Threading.Tasks;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Reflection.Metadata;

namespace BrickEngine.Assets.Data
{
    public class NodeGraph : ISerializable<NodeGraph>
    {
        public struct Node
        {
            public readonly string Name;
            public readonly int ParentIndex;
            public readonly int[] Children;
            public readonly Matrix4x4 LocalToWorld;
            public readonly Matrix4x4 GetWorldTransfrom(Node[] Nodes)
            {
                return Nodes[ParentIndex].GetWorldTransfrom(Nodes) * LocalToWorld;
            }

            public Node(string name, int parentIndex, int[] children, Matrix4x4 localToWorld)
            {
                Name = name;
                ParentIndex = parentIndex;
                Children = children;
                LocalToWorld = localToWorld;
            }
        }

        public readonly Node[] Nodes;

        public NodeGraph(Node[] nodes)
        {
            Nodes = nodes;
        }

        public static NodeGraph Deserialize(ReadOnlySpan<byte> blob)
        {
            Node[] nodes = new Node[BinarySerializer.ReadPackedInt(ref blob)];
            for (int i = 0; i < nodes.Length; i++)
            {
                string name = BinarySerializer.ReadString(ref blob);
                int parent = BinarySerializer.ReadPackedInt(ref blob);
                Matrix4x4 mat = default;
                var destSpan = MemoryMarshal.CreateSpan(ref mat.M11, 4 * 4);
                BinarySerializer.Read(destSpan, 4 * 4, ref blob);
                int childCount = BinarySerializer.ReadPackedInt(ref blob);
                int[] children = new int[childCount];
                for (int j = 0; j < children.Length; j++)
                {
                    children[j] = BinarySerializer.ReadPackedInt(ref blob);
                }
                nodes[i] = new Node(name, parent, children, mat);
            }
            return new NodeGraph(nodes);
        }

        public static void Serialize(ByteBufferWriter writer, NodeGraph data)
        {
            int estimatedLength = sizeof(int);
            for (int i = 0; i < data.Nodes.Length; i++)
            {
                ref var node = ref data.Nodes[i];
                //Name
                estimatedLength += BinarySerializer.GetSizeForString(node.Name);
                //ParentIndex
                estimatedLength += sizeof(int);
                //Matrix
                estimatedLength += sizeof(float) * 4 * 4;
                //ChildCount
                estimatedLength += sizeof(int);
                //ChildIndices
                estimatedLength += sizeof(int) * node.Children.Length;
            }


            var span = writer.GetSpan(estimatedLength);
            BinarySerializer.WritePackedInt(ref span, data.Nodes.Length);
            for (int i = 0; i < data.Nodes.Length; i++)
            {
                ref var node = ref data.Nodes[i];
                BinarySerializer.WriteString(ref span, node.Name);
                BinarySerializer.WritePackedInt(ref span, node.ParentIndex);
                Matrix4x4 mat = node.LocalToWorld;
                var srcSpan = MemoryMarshal.CreateReadOnlySpan(ref mat.M11, 4 * 4);
                BinarySerializer.Write(ref span, srcSpan);
                BinarySerializer.WritePackedInt(ref span, node.Children.Length);
                for (int j = 0; j < node.Children.Length; j++)
                {
                    BinarySerializer.WritePackedInt(ref span, node.Children[j]);
                }
            }
            writer.ReturnSpanAndAdvance(ref span);
        }
    }
}

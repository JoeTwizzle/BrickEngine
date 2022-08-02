using System;
using System.Collections.Generic;
using System.Linq;
using BinSerialize;
using System.Threading.Tasks;

namespace BrickEngine.Assets.Data
{
    public class NodeGraph : ISerializable<NodeGraph>
    {
        public struct Node
        {
            public readonly string Name;
            public readonly int ParentIndex;
            public readonly int[] Children;

            public Node(string name, int parentIndex, int[] children)
            {
                Name = name;
                ParentIndex = parentIndex;
                Children = children;
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
                int childCount= BinarySerializer.ReadPackedInt(ref blob);
                int[] children = new int[childCount];
                for (int j = 0; j < children.Length; j++)
                {
                    children[j] = BinarySerializer.ReadPackedInt(ref blob);
                }
                nodes[i] = new Node(name, parent, children);
            }
            return new NodeGraph(nodes);
        }

        public static void Serialize(ByteBufferWriter writer,NodeGraph data)
        {
            int estimatedLength = sizeof(int);
            for (int i = 0; i < data.Nodes.Length; i++)
            {
                ref var node = ref data.Nodes[i];
                //Name
                estimatedLength += BinarySerializer.GetSizeForString(node.Name);
                //ParentIndex
                estimatedLength += sizeof(int);
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

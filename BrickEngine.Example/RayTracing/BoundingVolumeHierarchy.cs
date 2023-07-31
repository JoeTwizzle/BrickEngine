using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrickEngine.Example.RayTracing
{
    public sealed class BoundingVolumeHierarchy
    {
        public Node[] Nodes;
        public int[] PrimitiveIndices;

        public int MaxDepth;

        public void Refresh()
        {
            MaxDepth = GetDepth();
        }

        public int GetDepth(uint node_index = 0)
        {
            var node = Nodes[node_index];
            return node.IsLeaf ? 1 : 1 + Math.Max(GetDepth(node.FirstIndex), GetDepth(node.FirstIndex + 1));
        }
    }
}

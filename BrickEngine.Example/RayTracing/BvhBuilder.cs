using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static SharpGLTF.Scenes.LightBuilder;

namespace BrickEngine.Example.RayTracing
{
    public struct BuildConfig
    {
        public static BuildConfig Default => new(2, 8, 1.0f);
        /// <summary>
        /// Minimum number of primitves before stopping splitting
        /// </summary>
        public int MinPrimitives;
        /// <summary>
        /// 
        /// </summary>
        public int MaxPrimitives;
        public float TraversalCost; // ditto

        public BuildConfig(int minPrims, int maxPrims, float traversalCost)
        {
            MinPrimitives = minPrims;
            MaxPrimitives = maxPrims;
            TraversalCost = traversalCost;
        }
    }
    public static class BVHBuilder
    {
        const int BinCount = 16;

        public static BuildConfig BuildConfig = new(2, 8, 0.0f);

        internal sealed class ComparisonComparer : Comparer<int>
        {
            public Vector3[] centers;
            public int axis;

            public override int Compare(int x, int y)
            {
                float a;
                float b;
                switch (axis)
                {
                    case 0:
                        a = centers[x].X;
                        b = centers[y].X;
                        return a.CompareTo(b);
                    case 1:
                        a = centers[x].Y;
                        b = centers[y].Y;
                        return a.CompareTo(b);
                    case 2:
                        a = centers[x].Z;
                        b = centers[y].Z;
                        return a.CompareTo(b);
                }
                return -1;
            }
        }

        static readonly ComparisonComparer MedianSplitComparer = new();

        public static BoundingVolumeHierarchy Build(BoundingBox[] bboxes, Vector3[] centers, int PrimCount)
        {
            BoundingVolumeHierarchy bvh = new()
            {
                PrimitiveIndices = new int[PrimCount],
                Nodes = new Node[2 * PrimCount - 1]
            };

            //Fill indices in ascending order
            for (int i = 0; i < bvh.PrimitiveIndices.Length; i++)
            {
                bvh.PrimitiveIndices[i] = i;
            }

            bvh.Nodes[0].PrimitiveCount = PrimCount;
            bvh.Nodes[0].FirstIndex = 0;

            int node_count = 1;
            BuildRecursive(bvh, 0, ref node_count, bboxes, centers);
            Array.Resize(ref bvh.Nodes, node_count);
            bvh.Refresh();
            return bvh;
        }

        /// <summary>
        /// Recursively iterates each node and subdivides it based on the SAH
        /// </summary>
        /// <param name="bvh">The bvh to operate on</param>
        /// <param name="nodeIndex">The index of the node to subdivide</param>
        /// <param name="nodeCount">The number of nodes present</param>
        /// <param name="bboxes">The bounding boxes of the triangles</param>
        /// <param name="centers">The centers of the triangles</param>
        static void BuildRecursive(BoundingVolumeHierarchy bvh, int nodeIndex, ref int nodeCount, BoundingBox[] bboxes, Vector3[] centers)
        {
            //The node being split
            ref Node node = ref bvh.Nodes[nodeIndex];
            Debug.Assert(node.IsLeaf);

            node.BoundingBox = BoundingBox.Empty;
            for (int i = 0; i < node.PrimitiveCount; ++i)
            {
                node.BoundingBox.Extend(bboxes[bvh.PrimitiveIndices[node.FirstIndex + i]]);
            }
            if (node.PrimitiveCount <= BuildConfig.MinPrimitives)
            {
                return;
            }

            //Try splitting along X,Y,Z Plane and select whichever is best
            Split minSplit = Split.Create();
            for (int axis = 0; axis < 3; ++axis)
            {
                minSplit = Split.Min(minSplit, FindBestSplit(bvh, axis, ref node, bboxes, centers));
            }

            float leafCost = node.BoundingBox.HalfArea * (node.PrimitiveCount - BuildConfig.TraversalCost);
            int firstRight; // Index of the first primitive in the right child
            if ((minSplit.RightBin == 0) || minSplit.Cost >= leafCost)
            {
                if (node.PrimitiveCount > BuildConfig.MaxPrimitives)
                {
                    // Fall back solution: The node has too many primitives, we use the median split
                    int axis = node.BoundingBox.LargestAxis;
                    MedianSplitComparer.axis = axis;
                    MedianSplitComparer.centers = centers;
                    Array.Sort(bvh.PrimitiveIndices, node.FirstIndex, node.PrimitiveCount, MedianSplitComparer);

                    firstRight = node.FirstIndex + node.PrimitiveCount / 2;
                }
                else
                {
                    // Terminate with a leaf
                    return;
                }
            }
            else
            {
                firstRight = Partition(bvh.PrimitiveIndices, node.FirstIndex, node.FirstIndex + node.PrimitiveCount, ref node.BoundingBox, ref minSplit, centers);
            }
            var firstChild = nodeCount;
            ref var left = ref bvh.Nodes[firstChild];
            ref var right = ref bvh.Nodes[firstChild + 1];
            nodeCount += 2;

            left.PrimitiveCount = firstRight - node.FirstIndex;
            right.PrimitiveCount = node.PrimitiveCount - left.PrimitiveCount;
            left.FirstIndex = node.FirstIndex;
            right.FirstIndex = firstRight;

            node.FirstIndex = firstChild;
            node.PrimitiveCount = 0;

            BuildRecursive(bvh, firstChild, ref nodeCount, bboxes, centers);
            BuildRecursive(bvh, firstChild + 1, ref nodeCount, bboxes, centers);
        }

        #region std::Partition
        static int Partition(int[] array, int first, int last, ref BoundingBox bbox, ref Split min_split, Vector3[] center)
        {
            int ufirst = first;
            int ulast = last;
            //int first = FindIfNot(array, start, end, ref bbox, ref min_split, center);
            while (true)
            {
                while (true)
                {
                    if (ufirst == ulast)
                    {
                        first = ufirst;
                        return first;
                    }

                    if (!Predicate(array[ufirst], ref bbox, ref min_split, center))
                    {
                        break;
                    }

                    ++ufirst;
                }

                do
                {
                    --ulast;
                    if (ufirst == ulast)
                    {
                        first = ufirst;
                        return first;
                    }
                } while (!Predicate(array[ulast], ref bbox, ref min_split, center));


                int temp = array[ufirst];
                array[ufirst] = array[ulast];
                array[ulast] = temp;

                ++ufirst;
            }
        }

        static bool Predicate(int i, ref BoundingBox bbox, ref Split minSplit, Vector3[] centers)
        {
            return BinIndex(minSplit.Axis, bbox, centers[i]) < minSplit.RightBin;
        }
        #endregion

        static int BinIndex(int axis, in BoundingBox bbox, in Vector3 center)
        {
            int index;
            switch (axis)
            {
                case 0:
                    index = (int)((center.X - bbox.Min.X) * (BinCount / (bbox.Max.X - bbox.Min.X)));
                    return Math.Min(BinCount - 1, Math.Max(0, index));
                case 1:
                    index = (int)((center.Y - bbox.Min.Y) * (BinCount / (bbox.Max.Y - bbox.Min.Y)));
                    return Math.Min(BinCount - 1, Math.Max(0, index));
                case 2:
                    index = (int)((center.Z - bbox.Min.Z) * (BinCount / (bbox.Max.Z - bbox.Min.Z)));
                    return Math.Min(BinCount - 1, Math.Max(0, index));
            }
            return -1;
        }

        /// <summary>
        /// Splits a node into two subnodes
        /// </summary>
        /// <param name="bvh"></param>
        /// <param name="axis"></param>
        /// <param name="node"></param>
        /// <param name="bboxes"></param>
        /// <param name="centers"></param>
        /// <returns>A split along the given axis</returns>
        static Split FindBestSplit(BoundingVolumeHierarchy bvh, int axis, ref Node node, BoundingBox[] bboxes, Vector3[] centers)
        {
            Span<Bin> bins = stackalloc Bin[BinCount];
            for (int i = 0; i < BinCount; i++)
            {
                bins[i] = Bin.Create();
            }
            for (int i = 0; i < node.PrimitiveCount; ++i)
            {
                var primIndex = bvh.PrimitiveIndices[node.FirstIndex + i];
                ref var bin = ref bins[BinIndex(axis, node.BoundingBox, centers[primIndex])];
                bin.BoundingBox.Extend(bboxes[primIndex]);
                bin.PrimCount++;
            }
            Span<float> rightCost = stackalloc float[BinCount];
            rightCost[0] = float.MinValue;
            Bin leftAccum = Bin.Create();
            Bin rightAccum = Bin.Create();
            for (int i = BinCount - 1; i > 0; --i)
            {
                rightAccum.Extend(bins[i]);
                // Due to the definition of an empty bounding box, the cost of an empty bin is -NaN
                rightCost[i] = rightAccum.Cost;
            }
            Split split = Split.Create();
            split.Axis = axis;
            for (int i = 0; i < BinCount - 1; ++i)
            {
                leftAccum.Extend(bins[i]);
                float cost = leftAccum.Cost + rightCost[i + 1];
                // This test is defined such that NaNs are automatically ignored.
                // Thus, only valid combinations with non-empty bins are considered.
                if (cost < split.Cost)
                {
                    split.Cost = cost;
                    split.RightBin = i + 1;
                }
            }
            return split;
        }
    }
}

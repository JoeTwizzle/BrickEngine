using BrickEngine.Assets.Data;
using BrickEngine.Example.RayTracing;
using static System.Numerics.Vector3;
using System.Numerics;
using System.Buffers;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace BrickEngine.Example
{
    sealed class RaytracedScene
    {
        struct RayHit
        {
            public int PrimIndex;
            public Vector3 Position;
            public Vector3 Normal;
            public Vector2 BarycentricCoords;
            public float Dist;
        }
        [StructLayout(LayoutKind.Sequential, Pack = 1, Size = sizeof(float) * 4 * 3)]
        public struct RayVertex
        {
            public Vector3 Pos;
            public Vector3 Normal;
            public Vector2 UV;
            public Vector4 Tangent;

            public RayVertex(Vector3 pos, Vector3 normal, Vector4 tangent, Vector2 uV)
            {
                Pos = pos;
                Normal = normal;
                Tangent = tangent;
                UV = uV;
            }
        }
        [StructLayout(LayoutKind.Sequential, Pack = 1, Size = sizeof(float) * 4 * 3)]
        public struct RayVertexGpu
        {
            public Vector4 PosNx;
            public Vector4 NormalYZUV;
            public Vector4 Tangent;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct ModelInfo //Per Model
        {
            public uint BvhNodesOffset;
            public uint BvhPrimIndexOffset;
            public uint ModelToMeshesMapIndex;
            public uint Padding;
        }
        //TODO: map model to meshes and add vertex & index offsets

        //Bvh of objects
        public readonly BoundingVolumeHierarchy TopLevelBvh;
        //bvh per model
        public readonly BoundingVolumeHierarchy[] SceneModels;



        //All Vertices of all models
        public readonly RayVertex[] Vertices;
        //All Indices of all models
        public readonly uint[] Indices;

        public readonly Node[] BlNodes;
        public readonly uint[] BlPrimIndices;

        //ModelIndex to meshCount, (int indexOffset, int indexCount, int vertexOffset)*
        public readonly uint[] ModelToMeshesArray;

        public readonly ModelInfo[] ModelInfos;

        public readonly uint[] ObjectIdToModelId;

        public readonly Matrix4x4[] WorldTransforms;

        public RaytracedScene(BoundingVolumeHierarchy topLevelBvh, BoundingVolumeHierarchy[] sceneModels, RayVertex[] vertices, uint[] indices, Node[] blNodes, uint[] blPrimIndices, uint[] modelToMeshesArray, ModelInfo[] modelInfos, uint[] objectIdToModelId, Matrix4x4[] worldTransforms)
        {
            TopLevelBvh = topLevelBvh;
            SceneModels = sceneModels;
            Vertices = vertices;
            Indices = indices;
            BlNodes = blNodes;
            BlPrimIndices = blPrimIndices;
            ModelToMeshesArray = modelToMeshesArray;
            ModelInfos = modelInfos;
            ObjectIdToModelId = objectIdToModelId;
            WorldTransforms = worldTransforms;
        }

        public static RaytracedScene LoadScene(VisualSceneNew scene)
        {
            //Per Model
            var blBvhs = GenarateBlBvhs(scene);
            int max = 0;
            for (int i = 0; i < blBvhs.Length; i++)
            {
                max = Math.Max(max, blBvhs[i].GetDepth());
            }
            Console.WriteLine("max blBvh: " + max);
            GenerateTlBvh(scene, out var tlBvh, out var objectIdToModelId);
            Console.WriteLine("max tlBvh: " + tlBvh.GetDepth());

            //blBvhs.Length == scene.Models.Length
            uint[] nodeOffsets = new uint[blBvhs.Length];
            uint[] primIndexOffsets = new uint[blBvhs.Length];
            uint nodeOffset = 0;
            uint primIndexOffset = 0;
            for (int i = 0; i < blBvhs.Length; i++)
            {
                nodeOffsets[i] = nodeOffset;
                nodeOffset += (uint)blBvhs[i].Nodes.Length;
                primIndexOffsets[i] = primIndexOffset;
                primIndexOffset += (uint)blBvhs[i].PrimitiveIndices.Length;
            }
            Node[] blNodes = new Node[nodeOffset];
            uint[] blIndices = new uint[primIndexOffset];
            int nodeCount = 0;
            int primCount = 0;
            for (int i = 0; i < blBvhs.Length; i++)
            {
                blBvhs[i].Nodes.CopyTo(blNodes, nodeCount);
                nodeCount += blBvhs[i].Nodes.Length;
                blBvhs[i].PrimitiveIndices.CopyTo(blIndices, primCount);
                primCount += blBvhs[i].PrimitiveIndices.Length;
            }
            //calc size
            int vertexCount = 0;
            int indexCount = 0;
            for (int i = 0; i < scene.Meshes.Length; i++)
            {
                checked
                {
                    MeshData mesh = scene.Meshes[i];
                    vertexCount += mesh.VertexData.VertexCount;
                    indexCount += mesh.IndexData.Indices.Length;
                }
            }
            RayVertex[] vertices = new RayVertex[vertexCount];
            uint[] indices = new uint[indexCount];
            uint[] indexOffsets = new uint[scene.Meshes.Length];
            uint[] vertexOffsets = new uint[scene.Meshes.Length];
            uint indexOffset = 0;
            uint vertexOffset = 0;
            for (int i = 0; i < scene.Meshes.Length; i++)
            {
                checked
                {
                    MeshData mesh = scene.Meshes[i];
                    mesh.IndexData.Indices.CopyTo(indices, indexOffset);
                    indexOffsets[i] = indexOffset;
                    indexOffset += (uint)mesh.IndexData.Indices.Length;
                    for (int j = 0; j < mesh.VertexData.VertexCount; j++)
                    {
                        vertices[vertexOffset + j] = new RayVertex(mesh.VertexData.Positions[j], mesh.VertexData.Normals[j], mesh.VertexData.Tangents[j], mesh.VertexData.TexCoord0[j]);
                    }
                    vertexOffsets[i] = vertexOffset;
                    vertexOffset += (uint)mesh.VertexData.VertexCount;
                }
            }

            int modelToMeshArrayLength = 0;
            for (int i = 0; i < scene.Models.Length; i++)
            {
                modelToMeshArrayLength += scene.Models[i].Meshes.Length * 3 + 1;
            }

            uint[] modelToMeshArray = new uint[modelToMeshArrayLength];
            uint[] modelToMeshArrayOffsets = new uint[scene.Models.Length];
            uint modelToMeshArrayOffset = 0;
            for (int i = 0; i < scene.Models.Length; i++)
            {
                checked
                {
                    modelToMeshArrayOffsets[i] = modelToMeshArrayOffset;

                    modelToMeshArray[modelToMeshArrayOffset] = (uint)scene.Models[i].Meshes.Length;
                    modelToMeshArrayOffset++;
                    for (uint meshId = 0; meshId < (uint)scene.Models[i].Meshes.Length; meshId++)
                    {
                        var mesh = scene.Models[i].Meshes[meshId];
                        modelToMeshArray[modelToMeshArrayOffset + meshId * 3 + 0] = indexOffsets[mesh];                                  //indexOffset
                        modelToMeshArray[modelToMeshArrayOffset + meshId * 3 + 1] = (uint)scene.Meshes[mesh].IndexData.Indices.Length;   //indexCount
                        modelToMeshArray[modelToMeshArrayOffset + meshId * 3 + 2] = vertexOffsets[mesh];                                 //vertexOffset
                    }
                    modelToMeshArrayOffset += (uint)scene.Models[i].Meshes.Length * 3;
                }
            }

            ModelInfo[] modelInfos = new ModelInfo[scene.Models.Length];
            for (int i = 0; i < scene.Models.Length; i++)
            {
                modelInfos[i].BvhNodesOffset = nodeOffsets[i];
                modelInfos[i].BvhPrimIndexOffset = primIndexOffsets[i];
                modelInfos[i].ModelToMeshesMapIndex = modelToMeshArrayOffsets[i];
            }

            Matrix4x4[] worldTransforms = new Matrix4x4[objectIdToModelId.Length];
            int index = 0;
            for (int i = 0; i < scene.Nodes.Length; i++)
            {
                var node = scene.Nodes[i];
                var model = node.Model;
                if (model != null)
                {
                    worldTransforms[index++] = node.GetWorldTransfrom(scene.Nodes);
                }
            }

            RaytracedScene raytracedScene = new(tlBvh, blBvhs, vertices, indices, blNodes, blIndices, modelToMeshArray, modelInfos, objectIdToModelId, worldTransforms);
            return raytracedScene;
        }

        private static BoundingVolumeHierarchy[] GenarateBlBvhs(VisualSceneNew scene)
        {
            BoundingVolumeHierarchy[] blBvhs = new BoundingVolumeHierarchy[scene.Models.Length];
            for (int i = 0; i < scene.Models.Length; i++)
            {
                var model = scene.Models[i];
                int triangleCount = 0;
                for (int j = 0; j < model.Meshes.Length; j++)
                {
                    MeshData mesh = scene.Meshes[model.Meshes[j]];
                    triangleCount += mesh.IndexData.Indices.Length / 3;
                }
                var bboxes = ArrayPool<BoundingBox>.Shared.Rent(triangleCount);
                var centers = ArrayPool<Vector3>.Shared.Rent(triangleCount);
                int offset = 0;
                for (int j = 0; j < model.Meshes.Length; j++)
                {
                    MeshData mesh = scene.Meshes[model.Meshes[j]];
                    for (int k = 0; k < mesh.IndexData.Indices.Length; k += 3)
                    {
                        var v0 = mesh.VertexData.Positions[mesh.IndexData.Indices[k]];
                        var v1 = mesh.VertexData.Positions[mesh.IndexData.Indices[k + 1]];
                        var v2 = mesh.VertexData.Positions[mesh.IndexData.Indices[k + 2]];

                        centers[offset + k / 3] = (v0 + v1 + v2) / 3f;
                        bboxes[offset + k / 3] = new BoundingBox(Min(v2, Min(v0, v1)), Max(v2, Max(v0, v1)));
                    }
                    offset += mesh.IndexData.Indices.Length / 3;
                }
                blBvhs[i] = BVHBuilder.BuildBl(bboxes.AsSpan(0, triangleCount), centers.AsSpan(0, triangleCount), triangleCount);

                ArrayPool<BoundingBox>.Shared.Return(bboxes);
                ArrayPool<Vector3>.Shared.Return(centers);
            }

            return blBvhs;
        }

        private static void GenerateTlBvh(VisualSceneNew scene, out BoundingVolumeHierarchy tlBvh, out uint[] objectIndexToModelIndex)
        {
            var tlModelMap = ArrayPool<uint>.Shared.Rent(scene.Nodes.Length);
            var tlBBoxes = ArrayPool<BoundingBox>.Shared.Rent(scene.Nodes.Length);
            var tlCenters = ArrayPool<Vector3>.Shared.Rent(scene.Nodes.Length);
            int objectCount = 0;
            for (int i = 0; i < scene.Nodes.Length; i++)
            {
                var node = scene.Nodes[i];
                var model = node.Model;
                if (model != null)
                {
                    var mdl = scene.Models[model.Value];

                    Vector3 center = Vector3.Zero;
                    Vector3 min = new(float.MaxValue);
                    Vector3 max = new(float.MinValue);
                    
                    for (int j = 0; j < mdl.Meshes.Length; j++)
                    {
                        MeshData mesh = scene.Meshes[mdl.Meshes[j]];
                        for (int k = 0; k < mesh.IndexData.Indices.Length; k += 3)
                        {
                            var v0 = mesh.VertexData.Positions[mesh.IndexData.Indices[k]];
                            var v1 = mesh.VertexData.Positions[mesh.IndexData.Indices[k + 1]];
                            var v2 = mesh.VertexData.Positions[mesh.IndexData.Indices[k + 2]];

                            center += (v0 + v1 + v2) / 3f;
                            min = Min(min, Min(v2, Min(v0, v1)));
                            max = Max(max, Max(v2, Max(v0, v1)));
                        }
                    }
                    //var worldMatrix = node.GetWorldTransfrom(scene.Nodes);
                    //min = Transform(min, worldMatrix);
                    //max = Transform(max, worldMatrix);
                    //center = Transform(center, worldMatrix);
                    tlBBoxes[objectCount] = new BoundingBox(min, max);
                    tlCenters[objectCount] = center;
                    //ModelID at Object[i]
                    tlModelMap[objectCount] = (uint)model.Value;
                    objectCount++;
                }
            }
            tlBvh = BVHBuilder.BuildTl(tlBBoxes.AsSpan(0, objectCount), tlCenters.AsSpan(0, objectCount), objectCount);
            objectIndexToModelIndex = tlModelMap.AsSpan(0, objectCount).ToArray();
            ArrayPool<BoundingBox>.Shared.Return(tlBBoxes);
            ArrayPool<Vector3>.Shared.Return(tlCenters);
            ArrayPool<uint>.Shared.Return(tlModelMap);
        }
    }

    sealed class SceneMesh
    {
        public readonly BoundingVolumeHierarchy Bvh;
        public MaterialData MaterialData;

        public SceneMesh(BoundingVolumeHierarchy bvh, MaterialData materialData)
        {
            Bvh = bvh;
            MaterialData = materialData;
        }
    }
}

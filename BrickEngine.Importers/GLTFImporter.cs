//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Runtime.CompilerServices;
//using BrickEngine.Core.Events;
//using System.Threading.Tasks;
//using SharpGLTF;
//using SharpGLTF.Schema2;
//using BrickEngine.Core.Graphics;
//using Microsoft.Toolkit.HighPerformance;
//using BrickEngine.Core.Components;
//using System.Diagnostics;
//using System.Buffers;
//using System.Numerics;

//namespace BrickEngine.Importers
//{
//    public static class GLTFImporter
//    {
//        public struct GLTFImportSettings
//        {
//            public bool Flatten;
//            public bool CompressTextures;
//        }
        
//        public static LevelAsset Import(string path, GLTFImportSettings settings)
//        {
//            ModelRoot modelRoot = ModelRoot.Load(path, SharpGLTF.Validation.ValidationMode.TryFix);
//            var meshGroups = LoadMeshes(modelRoot);
//            textureAssets = LoadTextures(modelRoot);
//            materialAssets = LoadMaterials(modelRoot, textureAssets);
//            modelAssets = LoadNodes(modelRoot, meshGroups, materialAssets);
//            var modelRefs = new AssetRefrence[modelAssets.Length];
//            for (int i = 0; i < modelAssets.Length; i++)
//            {
//                modelRefs[i] = new AssetRefrence();
//                modelRefs[i].SetAsset(modelAssets[i]);
//            }
//            LevelAsset levelAsset = new LevelAsset(modelRefs);
//            return levelAsset;
//        }

//        static ModelAsset[] LoadNodes(ModelRoot root, StaticMesh[][] meshGroups, PBRMaterial[] materials)
//        {
//            var materialRefs = new AssetRefrence[materials.Length];
//            for (int i = 0; i < materials.Length; i++)
//            {
//                materialRefs[i] = new AssetRefrence();
//                materialRefs[i].SetAsset(materials[i]);
//            }
//            ModelAsset[] models = new ModelAsset[meshGroups.Length];
//            foreach (var node in root.LogicalNodes)
//            {
//                if (node.Mesh != null)
//                {
//                    models[node.Mesh.LogicalIndex] = new ModelAsset(Transform.FromMatrix(node.WorldMatrix), meshGroups[node.Mesh.LogicalIndex], materialRefs);
//                }
//            }
//            return models;
//        }

//        static TextureAsset[] LoadTextures(ModelRoot root)
//        {
//            TextureAsset[] textures = new TextureAsset[root.LogicalTextures.Count];
//            int i = 0;
//            foreach (var tex in root.LogicalTextures)
//            {
//                textures[i++] = TextureImporter.Import(tex.PrimaryImage.Content.Content.AsStream(), TextureImportSettings.Default);
//            }
//            return textures;
//        }

//        static PBRMaterial[] LoadMaterials(ModelRoot root, TextureAsset[] textures)
//        {
//            PBRMaterial[] materials = new PBRMaterial[root.LogicalMaterials.Count];
//            int currentMaterial = 0;
//            foreach (var mat in root.LogicalMaterials)
//            {
//                PBRMaterial material = new PBRMaterial();
//                foreach (var channel in mat.Channels)
//                {
//                    switch (channel.Key)
//                    {
//                        case "BaseColor":
//                            if (channel.Texture != null)
//                            {
//                                material.ColorTexture.SetAsset(textures[channel.Texture.LogicalIndex]);
//                            }
//                            else
//                            {
//                                material.ColorTexture.SetAsset(null);
//                            }
//                            break;
//                        case "Normal":
//                            if (channel.Texture != null)
//                            {
//                                material.NormalTexture.SetAsset(textures[channel.Texture.LogicalIndex]);
//                            }
//                            else
//                            {
//                                material.NormalTexture.SetAsset(null);
//                            }
//                            break;
//                        case "MetallicRoughness":
//                            if (channel.Texture != null)
//                            {
//                                material.MetalRoughnessTexture.SetAsset(textures[channel.Texture.LogicalIndex]);
//                            }
//                            else
//                            {
//                                material.MetalRoughnessTexture.SetAsset(null);
//                            }
//                            break;
//                        case "Occlusion":
//                            break;
//                        case "Emissive":
//                            if (channel.Texture != null)
//                            {
//                                material.EmissiveTexture.SetAsset(textures[channel.Texture.LogicalIndex]);
//                            }
//                            else
//                            {
//                                material.EmissiveTexture.SetAsset(null);
//                            }
//                            break;
//                        default:
//                            break;
//                    }
//                }
//                materials[currentMaterial] = material;
//                currentMaterial++;
//            }
//            return materials;
//        }

//        static StaticMesh[][] LoadMeshes(ModelRoot root)
//        {
//            int currentModel = 0;
//            StaticMesh[][] meshes = new StaticMesh[root.LogicalMeshes.Count][];
//            foreach (var mesh in root.LogicalMeshes)
//            {
//                StaticMesh[] meshPrimitives = new StaticMesh[mesh.Primitives.Count];
//                int currentPrim = 0;
//                foreach (var primitive in mesh.Primitives)
//                {
//                    IList<Vector3> positions;
//                    IList<Vector3>? normals = null;
//                    IList<Vector4>? tangents = null;
//                    IList<Vector2>? texCoord0 = null;
//                    primitive.VertexAccessors.TryGetValue("POSITION", out var pos);
//                    positions = pos!.AsVector3Array();
//                    if (primitive.VertexAccessors.TryGetValue("NORMAL", out var norm))
//                    {
//                        normals = norm.AsVector3Array();
//                    }
//                    if (primitive.VertexAccessors.TryGetValue("TANGENT", out var tan))
//                    {
//                        tangents = tan.AsVector4Array();
//                    }
//                    if (primitive.VertexAccessors.TryGetValue("TEXCOORD_0", out var tex0))
//                    {
//                        texCoord0 = tex0.AsVector2Array();
//                    }
//                    if (primitive.VertexAccessors.TryGetValue("TEXCOORD_1", out var tex1))
//                    {
//                        texCoord0 = tex1.AsVector2Array();
//                    }
//                    if (primitive.VertexAccessors.TryGetValue("TEXCOORD_2", out var tex2))
//                    {
//                        texCoord0 = tex2.AsVector2Array();
//                    }
//                    if (primitive.VertexAccessors.TryGetValue("TEXCOORD_3", out var tex3))
//                    {
//                        texCoord0 = tex3.AsVector2Array();
//                    }

//                    StaticVertex[] vertices = ArrayPool<StaticVertex>.Shared.Rent(positions.Count);
//                    for (int i = 0; i < positions.Count; i++)
//                    {
//                        Vector3 normal = normals != null ? normals[i] : Vector3.Zero;
//                        Vector4 tang = tangents != null ? tangents[i] : Vector4.Zero;
//                        Vector2 uv0 = texCoord0 != null ? texCoord0[i] : Vector2.Zero;
//                        vertices[i] = new StaticVertex(positions[i], normal, tang, new Vector4(uv0, 1, 1), new Vector2());
//                    }
//                    meshPrimitives[currentPrim++] = new StaticMesh(vertices, primitive.GetIndices().ToArray());
//                    ArrayPool<StaticVertex>.Shared.Return(vertices);
//                }
//                //Store Model
//                meshes[currentModel] = meshPrimitives;
//                currentModel++;
//            }
//            return meshes;
//        }
//    }
//}

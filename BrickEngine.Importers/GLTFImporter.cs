using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using SharpGLTF;
using SharpGLTF.Schema2;
using BrickEngine.Core.Graphics;
using Microsoft.Toolkit.HighPerformance;
using BrickEngine.Core;
using System.Diagnostics;
using System.Buffers;
using System.Numerics;
using BrickEngine.Core.Graphics.Data;
using BrickEngine.Assets.Data;
using System.Runtime.InteropServices;
using System.Threading.Channels;

namespace BrickEngine.Importers
{

    public static class GLTFImporter
    {

        public static VisualScene Import(string path, bool compressTextures = true)
        {
            ModelRoot modelRoot = ModelRoot.Load(path, SharpGLTF.Validation.ValidationMode.TryFix);
            var meshGroups = LoadMeshes(modelRoot);
            var materialAssets = LoadMaterials(modelRoot, compressTextures);
            var nodes = LoadNodes(modelRoot, meshGroups, materialAssets);
            var levelAsset = new VisualScene(nodes);
            return levelAsset;
        }

        static SceneNode[] LoadNodes(ModelRoot root, MeshData[][] meshGroups, MaterialData[] materials)
        {
            SceneNode[] nodes = new SceneNode[root.LogicalNodes.Count];
            int i = 0;
            foreach (var node in root.LogicalNodes)
            {
                ModelData? model = null;
                if (node.Mesh != null)
                {
                    model = new ModelData(meshGroups[node.Mesh.LogicalIndex], materials);
                }
                var children = node.VisualChildren.ToArray();
                int[] childIdx = new int[children.Length];
                for (int j = 0; j < children.Length; j++)
                {
                    childIdx[j] = children[j].LogicalIndex;
                }
                nodes[i] = new SceneNode(node.Name ?? $"Node {i + 1}", node.VisualParent?.LogicalIndex, childIdx, node.WorldMatrix, model);
                i++;
            }
            return nodes;
        }

        //static TextureData[] LoadTextures(ModelRoot root, bool compress)
        //{
        //    TextureData[] textures = new TextureData[root.LogicalTextures.Count];
        //    int i = 0;
        //    foreach (var tex in root.LogicalTextures)
        //    {
        //        textures[i++] = TextureImporter.Import(tex.PrimaryImage.Content.Content.AsStream(), settings);
        //    }
        //    return textures;
        //}
        static readonly TextureData DefaultColorData = new((int)Veldrid.PixelFormat.R8_G8_B8_A8_UNorm_SRgb, new MipLevel[1] { new MipLevel(1, 1, new byte[] { 255, 255, 255, 255 }) });
        static readonly TextureData DefaultNormalData = new((int)Veldrid.PixelFormat.R8_G8_B8_A8_UNorm, new MipLevel[1] { new MipLevel(1, 1, new byte[] { 127, 127, 255, 255 }) });
        static readonly TextureData DefaultMRData = new((int)Veldrid.PixelFormat.R8_G8_UNorm, new MipLevel[1] { new MipLevel(1, 1, new byte[] { 0, 0 }) });
        static readonly TextureData DefaultEmissiveData = new((int)Veldrid.PixelFormat.R8_G8_B8_A8_UNorm_SRgb, new MipLevel[1] { new MipLevel(1, 1, new byte[] { 0, 0, 0, 0 }) });
        static readonly TextureData DefaultOcclusionData = new((int)Veldrid.PixelFormat.R8_UNorm, new MipLevel[1] { new MipLevel(1, 1, new byte[] { 0 }) });
        enum TextureChannel
        {
            Color = 1,
            Normal = 2,
            MetalRoughness = 4,
            Occlusion = 8,
            Emissive = 16
        }

        struct TextureChannelsData
        {
            public TextureData Color;
            public TextureData Normal;
            public TextureData MetalRoughness;
            public TextureData Emissive;
            public TextureData Occlusion;
        }

        static MaterialData[] LoadMaterials(ModelRoot root, bool compressTextures)
        {
            MaterialData[] materials = new MaterialData[root.LogicalMaterials.Count];
            int currentMaterial = 0;
            Dictionary<int, TextureChannel> neededTextures = new();
            Dictionary<int, TextureChannelsData> textures = new();
            foreach (var mat in root.LogicalMaterials)
            {
                foreach (var channel in mat.Channels)
                {
                    switch (channel.Key)
                    {
                        case "BaseColor":
                            if (channel.Texture != null)
                            {
                                ref var settings = ref CollectionsMarshal.GetValueRefOrAddDefault(neededTextures, channel.Texture.LogicalIndex, out var exists);
                                settings |= TextureChannel.Color;
                            }
                            break;
                        case "Normal":
                            if (channel.Texture != null)
                            {
                                ref var settings = ref CollectionsMarshal.GetValueRefOrAddDefault(neededTextures, channel.Texture.LogicalIndex, out var exists);
                                settings |= TextureChannel.Normal;
                            }
                            break;
                        case "MetallicRoughness":
                            if (channel.Texture != null)
                            {
                                ref var settings = ref CollectionsMarshal.GetValueRefOrAddDefault(neededTextures, channel.Texture.LogicalIndex, out var exists);
                                settings |= TextureChannel.MetalRoughness;
                            }
                            break;
                        case "Occlusion":
                            if (channel.Texture != null)
                            {
                                ref var settings = ref CollectionsMarshal.GetValueRefOrAddDefault(neededTextures, channel.Texture.LogicalIndex, out var exists);
                                settings |= TextureChannel.Occlusion;
                            }
                            break;
                        case "Emissive":
                            if (channel.Texture != null)
                            {
                                ref var settings = ref CollectionsMarshal.GetValueRefOrAddDefault(neededTextures, channel.Texture.LogicalIndex, out var exists);
                                settings |= TextureChannel.Emissive;
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
            foreach (var texture in neededTextures)
            {
                ref var textureAsset = ref CollectionsMarshal.GetValueRefOrAddDefault(textures, texture.Key, out var _);
                var compression = compressTextures ? TextureCompressionMode.Medium : TextureCompressionMode.None;
                var tex = root.LogicalTextures[texture.Key];
                using var s = tex.PrimaryImage.Content.Content.AsStream();
                if (texture.Value.HasFlag(TextureChannel.Color))
                {
                    textureAsset.Color = TextureImporter.Import(s, new TextureImportSettings(TextureType.RGBA, compression, true, true));
                }
                if (texture.Value.HasFlag(TextureChannel.Normal))
                {
                    textureAsset.Normal = TextureImporter.Import(s, new TextureImportSettings(TextureType.RG, compression, true, false));
                }
                if (texture.Value.HasFlag(TextureChannel.MetalRoughness))
                {
                    textureAsset.MetalRoughness = TextureImporter.Import(s, new TextureImportSettings(TextureType.RG, compression, true, false));
                }
                if (texture.Value.HasFlag(TextureChannel.Occlusion))
                {
                    textureAsset.Occlusion = TextureImporter.Import(s, new TextureImportSettings(TextureType.R, compression, true, false));
                }
                if (texture.Value.HasFlag(TextureChannel.Emissive))
                {
                    textureAsset.Emissive = TextureImporter.Import(s, new TextureImportSettings(TextureType.RGBA, compression, true, true));
                }
            }
            foreach (var mat in root.LogicalMaterials)
            {
                TextureData? color = null;
                TextureData? normal = null;
                TextureData? metalRoughness = null;
                TextureData? occlusion = null;
                TextureData? emissive = null;
                foreach (var channel in mat.Channels)
                {
                    switch (channel.Key)
                    {
                        case "BaseColor":
                            if (channel.Texture != null)
                            {
                                color = textures[channel.Texture.LogicalIndex].Color;
                            }
                            break;
                        case "Normal":
                            if (channel.Texture != null)
                            {
                                normal = textures[channel.Texture.LogicalIndex].Normal;
                            }
                            break;
                        case "MetallicRoughness":
                            if (channel.Texture != null)
                            {
                                metalRoughness = textures[channel.Texture.LogicalIndex].MetalRoughness;
                            }
                            break;
                        case "Occlusion":
                            if (channel.Texture != null)
                            {
                                occlusion = textures[channel.Texture.LogicalIndex].Occlusion;
                            }
                            break;
                        case "Emissive":
                            if (channel.Texture != null)
                            {
                                emissive = textures[channel.Texture.LogicalIndex].Emissive;
                            }
                            break;
                        default:
                            break;
                    }
                }
                color ??= DefaultColorData;
                normal ??= DefaultNormalData;
                metalRoughness ??= DefaultMRData;
                emissive ??= DefaultEmissiveData;
                occlusion ??= DefaultOcclusionData;
                materials[currentMaterial] = new MaterialData(color, normal, metalRoughness, occlusion, emissive);
                currentMaterial++;
            }
            return materials;
        }

        static MeshData[][] LoadMeshes(ModelRoot root)
        {
            int currentModel = 0;
            MeshData[][] meshes = new MeshData[root.LogicalMeshes.Count][];
            foreach (var mesh in root.LogicalMeshes)
            {
                MeshData[] meshPrimitives = new MeshData[mesh.Primitives.Count];
                int currentPrim = 0;
                foreach (var primitive in mesh.Primitives)
                {
                    IList<Vector3> positions;
                    IList<Vector3>? normals = null;
                    IList<Vector4>? tangents = null;
                    IList<Vector4>? colors = null;
                    IList<Vector2>? texCoord0 = null;
                    primitive.VertexAccessors.TryGetValue("POSITION", out var pos);
                    positions = pos!.AsVector3Array();
                    if (primitive.VertexAccessors.TryGetValue("COLOR", out var col))
                    {
                        colors = col.AsColorArray();
                    }
                    if (primitive.VertexAccessors.TryGetValue("NORMAL", out var norm))
                    {
                        normals = norm.AsVector3Array();
                    }
                    if (primitive.VertexAccessors.TryGetValue("TANGENT", out var tan))
                    {
                        tangents = tan.AsVector4Array();
                    }
                    if (primitive.VertexAccessors.TryGetValue("TEXCOORD_0", out var tex0))
                    {
                        texCoord0 = tex0.AsVector2Array();
                    }
                    //if (primitive.VertexAccessors.TryGetValue("TEXCOORD_1", out var tex1))
                    //{
                    //    texCoord0 = tex1.AsVector2Array();
                    //}
                    //if (primitive.VertexAccessors.TryGetValue("TEXCOORD_2", out var tex2))
                    //{
                    //    texCoord0 = tex2.AsVector2Array();
                    //}
                    //if (primitive.VertexAccessors.TryGetValue("TEXCOORD_3", out var tex3))
                    //{
                    //    texCoord0 = tex3.AsVector2Array();
                    //}

                    var vertexData = new VertexData(positions.Count, positions.ToArray(), normals?.ToArray(), tangents?.ToArray(), colors?.ToArray(), texCoord0?.ToArray());
                    meshPrimitives[currentPrim++] = new MeshData(vertexData, new IndexData(primitive.GetIndices().ToArray()));
                }
                //Store Model
                meshes[currentModel] = meshPrimitives;
                currentModel++;
            }
            return meshes;
        }
    }
}

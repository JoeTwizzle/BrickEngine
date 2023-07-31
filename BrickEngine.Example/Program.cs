using BCnEncoder.Shared;
using BrickEngine.Assets.Data;
using BrickEngine.Core;
using BrickEngine.Importers;
using MemoryPack;
using System.Numerics;
using System.Runtime.Serialization;

namespace BrickEngine.Example
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var debug = args.Contains("-d");
#if DEBUG
            debug = true;
#endif
            new MyGameWindow(debug).Run();
        }
    }


    class MyGameWindow : GameWindow, IFeatureLayer
    {
        public MyGameWindow(bool debug) : base(debug)
        {
        }

        public override IFeatureLayer[] RegisterLayers()
        {
            return new IFeatureLayer[] { this, new Editor.Editor() };
        }

        public bool IsEnabled { get; set; } = true;
        Renderer renderer;
        public void OnLoad(GameWindow gameWindow)
        {
            VisualSceneNew scene;
            Console.WriteLine("Loading Scene!");
            if (!File.Exists("Beta map.bin"))
            {
                using var sceneStream = ConvertToNewFormat(GLTFImporter.Import("SunTempleFull.glb", false)).Result!;
            }

            using var fs = new FileStream("Beta map.bin", FileMode.Open, FileAccess.Read);
            scene = MemoryPackSerializer.DeserializeAsync<VisualSceneNew>(fs).Result!;
            Console.WriteLine("Loaded Bin!");
            Console.WriteLine("Nodes: " + scene.Nodes.Length);
            Console.WriteLine("Done!");
            //Implementierung von stochastic-subsets-for-bvh-construction [?]
            //Erweiterung von Binned-SAH mit 2, 4, 8 Child nodes perf vergleich [?]
            //VisBuf renderer 
            //Shadow RT optimized BVH
            //Parallax Raymarching for voxel surfaces
            //ReSTIR GI
            //Investigate PTEX textures
            //Real time Voxelization
            //Stochastic OIT
            //Other OIT???
            //LTC lights
            renderer = new Renderer(RaytracedScene.LoadScene(scene), this);
            //using FileStream fs2 = await ConvertToNewFormat(scene);
            Window.Resized += Window_Resized;
        }

        private void Window_Resized()
        {
            renderer.Resize();
        }

        private static async Task<FileStream> ConvertToNewFormat(VisualScene scene)
        {
            Console.WriteLine("Converting!");
            SceneNodeNew[] newNodes = new SceneNodeNew[scene.Nodes.Length];
            ObjectIDGenerator objectIDGenerator = new();
            Dictionary<long, int> modelMap = new();
            List<ModelDataNew> modelDataNews = new List<ModelDataNew>();
            Dictionary<long, int> meshMap = new();
            List<MeshData> meshList = new();
            Dictionary<long, int> materialMap = new();
            List<MaterialDataNew> materialDataNews = new List<MaterialDataNew>();
            Dictionary<long, int> textureMap = new();
            List<TextureData> textureList = new List<TextureData>();
            for (int i = 0; i < newNodes.Length; i++)
            {
                var oldNode = scene.Nodes[i];
                if (oldNode.Model != null)
                {
                    var id = objectIDGenerator.GetId(oldNode.Model, out var firstTime);
                    if (firstTime)
                    {
                        int[] materials = GetMaterialIndices(objectIDGenerator, materialMap, materialDataNews, textureMap, textureList, oldNode);
                        int[] meshes = GetMeshIndices(objectIDGenerator, meshMap, meshList, oldNode);
                        modelMap.Add(id, modelDataNews.Count);
                        modelDataNews.Add(new ModelDataNew(meshes, materials));
                    }
                    newNodes[i] = new SceneNodeNew(oldNode.Name, oldNode.ParentIndex, oldNode.Children, oldNode.LocalToWorld, modelMap[id]);
                    continue;
                }


                newNodes[i] = new SceneNodeNew(oldNode.Name, oldNode.ParentIndex, oldNode.Children, oldNode.LocalToWorld);
            }
            var sceneV2 = new VisualSceneNew(newNodes, modelDataNews.ToArray(), materialDataNews.ToArray(), textureList.ToArray(), meshList.ToArray()); ;
            var fs2 = (new FileStream("Beta map.bin", FileMode.Create));
            await MemoryPackSerializer.SerializeAsync(fs2, sceneV2);
            Console.WriteLine("Saved!");
            Console.WriteLine("Done!");
            return fs2;
        }

        private static int[] GetMaterialIndices(ObjectIDGenerator objectIDGenerator, Dictionary<long, int> materialMap, List<MaterialDataNew> materialDataNews, Dictionary<long, int> textureMap, List<TextureData> textureList, SceneNode oldNode)
        {
            int[] materials = new int[oldNode.Model.Materials.Length];
            for (int j = 0; j < oldNode.Model.Materials.Length; j++)
            {
                var material = oldNode.Model.Materials[j];
                var materialId = objectIDGenerator.GetId(material, out var materialFirstTime);
                if (materialFirstTime)
                {
                    TextureData[] textures = new TextureData[5] { material.ColorTexture, material.NormalTexture, material.MetalRoughnessTexture, material.OcclusionTexture, material.EmissiveTexture };
                    int[] textureIndices = new int[textures.Length];
                    for (int k = 0; k < textures.Length; k++)
                    {
                        var texture = textures[k];
                        var textureId = objectIDGenerator.GetId(texture, out var textureFirstTime);
                        if (textureFirstTime)
                        {
                            textureIndices[k] = textureList.Count;
                            textureMap.Add(textureId, textureList.Count);
                            textureList.Add(texture);
                        }
                        else
                        {
                            textureIndices[k] = textureMap[textureId];
                        }
                    }
                    materials[j] = materialDataNews.Count;
                    materialMap.Add(materialId, materialDataNews.Count);
                    materialDataNews.Add(new MaterialDataNew(textureIndices[0], textureIndices[1], textureIndices[2], textureIndices[3], textureIndices[4]));
                }
                else
                {
                    materials[j] = materialMap[materialId];
                }
            }

            return materials;
        }

        private static int[] GetMeshIndices(ObjectIDGenerator objectIDGenerator, Dictionary<long, int> meshMap, List<MeshData> meshList, SceneNode oldNode)
        {
            int[] meshes = new int[oldNode.Model.Meshes.Length];
            for (int j = 0; j < oldNode.Model.Meshes.Length; j++)
            {
                var mesh = oldNode.Model.Meshes[j];
                var meshId = objectIDGenerator.GetId(mesh, out var meshFirstTime);
                if (meshFirstTime)
                {
                    meshes[j] = meshList.Count;
                    meshMap.Add(meshId, meshList.Count);
                    meshList.Add(mesh);
                }
                else
                {
                    meshes[j] = meshMap[meshId];
                }
            }

            return meshes;
        }

        public void OnUnload()
        {
            Window.Resized -= Window_Resized;
        }

        public void Update()
        {
            renderer.Update();
            renderer.Render();
        }
    }
}
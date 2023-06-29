using BrickEngine.Assets;
using BrickEngine.Assets.Data;
using BrickEngine.Core;
using BrickEngine.Editor;
using BrickEngine.Example.RayTracing;
using BrickEngine.Importers;
using MemoryPack;
using SharpGLTF.Schema2;

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

        public async void OnLoad(GameWindow gameWindow)
        {
            VisualScene scene;
            Console.WriteLine("Loading Scene!");
            bool preSaved = File.Exists("SunTempleAsset.bin");
            if (!File.Exists("SunTempleAsset.bin"))
            {
                scene = GLTFImporter.Import("SunTempleFull.glb");
                Console.WriteLine("Loaded!");
                using var fs = new FileStream("SunTempleAsset.bin", FileMode.Create);
                await MemoryPackSerializer.SerializeAsync(fs, scene);
            }
            else
            {
                var fs = new FileStream("SunTempleAsset.bin", FileMode.Open);
                if (fs.Length > 0)
                {
                    scene = (await MemoryPackSerializer.DeserializeAsync<VisualScene>(fs))!;
                }
                else
                {
                    fs.Close();
                    fs.Dispose();

                    scene = GLTFImporter.Import("SunTempleFull.glb");
                    Console.WriteLine("Loaded!");
                    using var file = new FileStream("SunTempleAsset.bin", FileMode.Create);
                    await MemoryPackSerializer.SerializeAsync(file, scene);
                }
            }
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


        }

        public void OnUnload()
        {

        }

        public void Update()
        {

        }
    }
}
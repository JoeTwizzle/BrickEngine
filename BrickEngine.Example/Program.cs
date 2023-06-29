using BrickEngine.Assets;
using BrickEngine.Assets.Data;
using BrickEngine.Core;
using BrickEngine.Editor;
using BrickEngine.Example.RayTracing;
using BrickEngine.Importers;

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

        public void OnLoad(GameWindow gameWindow)
        {
            Asset sceneAsset;
            Console.WriteLine("Loading Scene!");
            if (File.Exists("SunTempleAsset.bin"))
            {
                sceneAsset = Asset.Load(new FileStream("SunTempleAsset.bin", FileMode.Open));
            }
            else
            {

                var scene = GLTFImporter.Import("SunTempleFull.glb");
                using var writer = new ByteBufferWriter();
                VisualScene.Serialize(writer, scene);
                sceneAsset = Asset.Create(1, 0, true, writer.WrittenSpan.ToArray());
                Asset.Save(sceneAsset, new FileStream("SunTempleAsset.bin", FileMode.Create));
            }
            var span = (ReadOnlySpan<byte>)sceneAsset.GetDecompressedBlob().AsSpan();
            var visualScene = VisualScene.Deserialize(ref span);
            Console.WriteLine("Nodes: " + visualScene.Nodes.Length);
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
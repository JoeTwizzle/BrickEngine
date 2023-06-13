using BrickEngine.Core;
using BrickEngine.Editor;

namespace BrickEngine.Example
{
    internal class Program
    {
        static void Main(string[] args)
        {
            new MyGameWindow(true).Run();
        }
    }


    class MyGameWindow : GameWindow, IFeatureLayer
    {
        public MyGameWindow(bool debug) : base(debug)
        {
        }

        public bool IsEnabled { get; } = true;

        public void Display()
        {

        }

        public void OnLoad(GameWindow gameWindow)
        {

        }

        public void OnUnload()
        {

        }

        public override IFeatureLayer[] RegisterLayers()
        {
            return new IFeatureLayer[] { this, new Editor.Editor() };
        }

        public void Update()
        {

        }
    }
}
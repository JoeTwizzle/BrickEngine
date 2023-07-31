global using System.Numerics;
global using Veldrid;
global using Veldrid.Sdl2;
global using Veldrid.StartupUtilities;
using BrickEngine.Core.Graphics;
using BrickEngine.Core.Utilities;
using BrickEngine.Graphics;
using System.Diagnostics;

namespace BrickEngine.Core
{
    public abstract class GameWindow
    {
        public Sdl2Window Window { get; private set; }
        public Input Input { get; private set; }
        public float DeltaTime { get; private set; }
        public double DeltaTimeFull { get; private set; }
        public GraphicsContext GraphicsContext { get; private set; }
        public IFeatureLayer[] FeatureLayers { get; private set; }
        public bool Debug { get; private set; }

        public GameWindow(bool debug)
        {
            Debug = debug;
            WindowCreateInfo windowCI = new()
            {
                WindowInitialState = WindowState.Normal,
                WindowWidth = 1280,
                WindowHeight = 720,
                X = 100,
                Y = 100,
                WindowTitle = "Brick Engine",
            };
            Window = VeldridStartup.CreateWindow(windowCI);
            Input = new Input(Window);
            GraphicsContext = new(this);
            Window.Resized += Window_Resized;
            FeatureLayers = RegisterLayers();
       
        }

        private void Window_Resized()
        {
            static int Clamp(int val)
            {
                return Math.Max(val, 64);
            }
            if (Window.WindowState != WindowState.Minimized)
            {
                GraphicsContext.GraphicsDevice.WaitForIdle();
                GraphicsContext.GraphicsDevice.MainSwapchain!.Resize((uint)Clamp(Window.Width), (uint)Clamp(Window.Height));
                GraphicsContext.GraphicsDevice.WaitForIdle();
            }
        }

        public abstract IFeatureLayer[] RegisterLayers();

        public void Run()
        {
            DeltaTimeFull = double.Epsilon;
            DeltaTime = MathF.Max(DeltaTime, float.Epsilon);
            long prev = Stopwatch.GetTimestamp();
            for (int i = 0; i < FeatureLayers.Length; i++)
            {
                FeatureLayers[i].OnLoad(this);
            }
            Window_Resized();
            while (Window.Exists)
            {
                long current = Stopwatch.GetTimestamp();
                DeltaTimeFull = (current - prev) * 1e-7; //Ticks to seconds constant
                DeltaTime = (float)DeltaTimeFull;
                prev = current;
                var InputSnapshot = Window.PumpEvents();
                if (!Window.Exists)
                {
                    break;
                }
                Input.UpdateFrameInput(InputSnapshot, Window);

                for (int i = 0; i < FeatureLayers.Length; i++)
                {
                    if (FeatureLayers[i].IsEnabled)
                    {
                        FeatureLayers[i].Update();
                    }
                }
                for (int i = 0; i < FeatureLayers.Length; i++)
                {
                    if (FeatureLayers[i].IsEnabled)
                    {
                        FeatureLayers[i].Display();
                    }
                }

                GraphicsContext.GraphicsDevice.SwapBuffers();
                GraphicsContext.EndFrame();
            }
            for (int i = 0; i < FeatureLayers.Length; i++)
            {
                FeatureLayers[i].OnUnload();
            }
            GraphicsContext.Dispose();
        }
    }
}

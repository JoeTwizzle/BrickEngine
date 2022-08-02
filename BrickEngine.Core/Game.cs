global using Veldrid;
global using Veldrid.Sdl2;
global using Veldrid.StartupUtilities;
global using Veldrid.Utilities;
global using System.Numerics;
global using EcsLite;
global using EcsLite.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using BrickEngine.Gui;
using BrickEngine.Core.Graphics;
using ImGuiNET;
using System.Diagnostics.CodeAnalysis;
using BrickEngine.Core.Utilities;

namespace BrickEngine.Core
{
    public abstract class Game
    {
        public Sdl2Window Window { get; private set; }
        public GraphicsDevice GraphicsDevice { get; private set; }
        public Input Input { get; private set; }
        public ResourceFactory ResourceFactory { get; private set; }
        public float DeltaTime { get; private set; }
        public double DeltaTimeFull { get; private set; }
        public bool ResizedThisFrame { get; private set; }
        public bool IsFullscreen { get; private set; }
        public bool Initialized { get; private set; }
        public EcsSystems Systems { get; private set; }
        public EcsWorld DefaultWorld { get; private set; }
        public InputSnapshot InputSnapshot { get; private set; }
        public event Action OnResized;
        public event Action OnInit;
        public event Action<EcsWorld> OnCreateDefaultWorld;
        public event Action<EcsSystemsBuilder> OnCreateSystems;
        public event Action OnRegisterWorlds;
        public event Action OnPreUpdate;
        public event Action OnPostUpdate;
        public event Action OnDisposeGame;
        public IReadOnlyList<ViewportRegion> ViewportRegions => _viewportRegions;
        public IReadOnlyDictionary<EcsWorld, string> AllWorldsReverse => _allWorldsReverse;
        public IReadOnlyDictionary<string, EcsWorld> AllWorlds => _allWorlds;
        public IReadOnlyDictionary<string, EcsWorld> OtherWorlds => _otherWorlds;
        public ViewportRegion DefaultViewport => _viewportRegions[0];
        public static RenderDoc? RenderDoc => _renderDoc;
        protected abstract void Init();
        protected abstract void PreUpdate();
        protected abstract void PostUpdate();
        protected abstract void DisposeGame();
        /// <summary>
        /// This is where you Initialize your default world
        /// </summary>
        /// <param name="defaultWorld"></param>
        protected abstract void InitDefaultWorld(EcsWorld defaultWorld);

        /// <summary>
        /// This is where you Initialize your Systems
        /// </summary>
        /// <param name="builder"></param>
        /// <returns>Number of Threads used by the systems</returns>
        protected abstract int InitSystems(EcsSystemsBuilder builder);

        private readonly AutoDisposer autoDisposer;
        private readonly List<ViewportRegion> _viewportRegions;
        private readonly Dictionary<string, EcsWorld> _otherWorlds;
        private readonly Dictionary<string, EcsWorld> _allWorlds;
        private readonly Dictionary<EcsWorld, string> _allWorldsReverse;
        private static RenderDoc? _renderDoc;
        private int _windowWidth;
        private int _windowHeight;
        private Stopwatch _stopwatch;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public Game()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            _allWorldsReverse = new Dictionary<EcsWorld, string>();
            _allWorlds = new Dictionary<string, EcsWorld>();
            _otherWorlds = new Dictionary<string, EcsWorld>();
            WindowCreateInfo windowCI = new WindowCreateInfo()
            {
                WindowInitialState = WindowState.Normal,
                WindowWidth = 1280,
                WindowHeight = 720,
                X = 100,
                Y = 100,
                WindowTitle = "Brick Engine",
            };
            GraphicsDeviceOptions options = new GraphicsDeviceOptions
            {
                PreferStandardClipSpaceYDirection = true,
                PreferDepthRangeZeroToOne = true,
#if VALIDATION
                Debug = true
#else
                Debug = false
#endif
            };
#if VALIDATION
            //if (RenderDoc.Load(out _renderDoc))
            //{
            //    Console.WriteLine("Renderdoc loaded");
            //    _renderDoc.LaunchReplayUI();
            //}
            //else
            //{
            //    Console.WriteLine("Renderdoc failed to load");
            //}
#endif
            autoDisposer = new AutoDisposer();

            Window = VeldridStartup.CreateWindow(windowCI);
            GraphicsDevice = VeldridStartup.CreateVulkanGraphicsDevice(options, Window, false);
            ResourceFactory = GraphicsDevice.ResourceFactory;
            Input = new Input(Window);

            _viewportRegions = new List<ViewportRegion>();
            _viewportRegions.Add(new ViewportRegion(new Rectangle(0, 0, Window.Width, Window.Height)));
            Window.Resized += Window_Resized;
            Window_Resized();
        }

        private static int Clamp(int val)
        {
            return Math.Max(val, 64);
        }

        private void Window_Resized()
        {
            _viewportRegions[0].Dimensions.Width = Clamp(Window.Width);
            _viewportRegions[0].Dimensions.Height = Clamp(Window.Height);
            Window.Width = Clamp(Window.Width);
            Window.Height = Clamp(Window.Height);
            ResizedThisFrame = true;
        }

        public void AddWorld(string name, EcsWorld world)
        {
            _allWorldsReverse.Add(world, name);
            _allWorlds.Add(name, world);
            _otherWorlds.Add(name, world);
        }

        public void AddViewport(ViewportRegion viewportRegion)
        {
            _viewportRegions.Add(viewportRegion);
        }

        public void RemoveViewport(ViewportRegion viewportRegion)
        {
            int idx = _viewportRegions.IndexOf(viewportRegion);
            if (idx != 0)
            {
                _viewportRegions.RemoveAt(idx);
            }
            else
            {
                Debug.WriteLine("You tried to remove the default viewport FOOL!");
            }
        }

        public void EnterFullscreen()
        {
            if (Window.WindowState == WindowState.BorderlessFullScreen)
            {
                return;
            }
            IsFullscreen = true;
            _windowWidth = Window.Width;
            _windowHeight = Window.Height;
            Window.WindowState = WindowState.BorderlessFullScreen;
            ResizedThisFrame = true;
        }

        public void ExitFullscreen()
        {
            if (Window.WindowState != WindowState.BorderlessFullScreen)
            {
                return;
            }
            IsFullscreen = false;
            Window.WindowState = WindowState.Normal;
            Window.Width = _windowWidth;
            Window.Height = _windowHeight;
            ResizedThisFrame = true;
        }

        private void Setup()
        {
            DefaultWorld = new EcsWorld();
            _allWorldsReverse.Add(DefaultWorld, "Default");
            _allWorlds.Add("Default", DefaultWorld);
            InitDefaultWorld(DefaultWorld);
            OnCreateDefaultWorld?.Invoke(DefaultWorld);
            var builder = new EcsSystemsBuilder(DefaultWorld);
            OnRegisterWorlds?.Invoke();
            foreach (var world in _otherWorlds)
            {
                builder.AddWorld(world.Value, world.Key);
            }
            var threadCount = InitSystems(builder);
            OnCreateSystems?.Invoke(builder);
            Systems = builder.Finish(threadCount);
            Init();
            OnInit?.Invoke();
        }

        public void Run()
        {
            Setup();
            Initialized = true;
            _stopwatch = Stopwatch.StartNew();
            DeltaTimeFull = double.Epsilon;
            DeltaTime = MathF.Max(DeltaTime, float.Epsilon);
            while (Window.Exists)
            {
                double prev = _stopwatch.Elapsed.TotalSeconds;
                InputSnapshot = Window.PumpEvents();
                if (ResizedThisFrame)
                {
                    if (Window.WindowState != WindowState.Minimized)
                    {
                        GraphicsDevice.ResizeMainWindow((uint)Clamp(Window.Width), (uint)Clamp(Window.Height));
                        OnResized?.Invoke();
                    }
                    ResizedThisFrame = false;
                }
                PreUpdate();
                OnPreUpdate?.Invoke();
                Systems.Run(DeltaTimeFull);
                PostUpdate();
                OnPostUpdate?.Invoke();
                if (Window.WindowState != WindowState.Minimized)
                {
                    GraphicsDevice.SwapBuffers();
                }
                double current = _stopwatch.Elapsed.TotalSeconds;
                DeltaTimeFull = current - prev;
                DeltaTimeFull = Math.Max(DeltaTimeFull, double.Epsilon);
                DeltaTime = (float)DeltaTimeFull;
                DeltaTime = MathF.Max(DeltaTime, float.Epsilon);
            }
            GraphicsDevice.WaitForIdle();
            DisposeGame();
            OnDisposeGame?.Invoke();
        }

#region Graphics
        public void DisposeWhenUnused<T>(T disposable) where T : IDisposable, DeviceResource
        {
            Debug.WriteLine("Defering disposal of object: " + disposable.GetType() + " Name: " + disposable.Name);
            autoDisposer.DisposeWhenUnused(disposable);
        }

        public void SubmitCommands(CommandList commandList)
        {
            Fence? fence = autoDisposer.GetPooledFence();
            if (fence == null)
            {
                fence = ResourceFactory.CreateFence(false); //no fences available, create new one
            }
            GraphicsDevice.SubmitCommands(commandList, fence);
            autoDisposer.AddFence(fence);
        }
#endregion
    }
}

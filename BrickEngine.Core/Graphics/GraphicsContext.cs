using BrickEngine.Core.Graphics;
using BrickEngine.Core;
using BrickEngine.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace BrickEngine.Graphics
{
    public sealed class GraphicsContext : IDisposable
    {
        public GraphicsDevice GraphicsDevice { get; private set; }
        public ResourceFactory ResourceFactory { get; private set; }
        [MemberNotNullWhen(true, nameof(RenderDoc))]
        public bool RenderdocLoaded { get; private set; }
        public static RenderDoc? RenderDoc { get; private set; }
        public GraphicsCache GraphicsCache { get; private set; }

        private readonly AutoDisposer autoDisposer;

        public GraphicsContext(GameWindow game)
        {
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
            autoDisposer = new();
            GraphicsDevice = VeldridStartup.CreateVulkanGraphicsDevice(options, game.Window, false);
            ResourceFactory = GraphicsDevice.ResourceFactory;
            if (game.Debug)
            {
                RenderdocLoaded = RenderDoc.Load(out var renderDoc);
                RenderDoc = renderDoc;
            }
            GraphicsCache = new(ResourceFactory);
        }

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

        public void Dispose()
        {
            GraphicsCache.Dispose();
            GraphicsDevice.WaitForIdle();
            GraphicsDevice.Dispose();
        }
    }
}

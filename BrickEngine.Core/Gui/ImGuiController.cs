using System;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection;
using System.IO;
using Veldrid;
using System.Runtime.CompilerServices;
using ImGuiNET;
using Veldrid.Sdl2;
using System.Runtime.InteropServices;
using ImPlotNET;
using ImGuizmoNET;
using Veldrid.SPIRV;

namespace BrickEngine.Gui
{
    /// <summary>
    /// A modified version of Veldrid.ImGui's ImGuiRenderer.
    /// Manages input for ImGui and handles rendering ImGui's DrawLists with Veldrid.
    /// </summary>
    public class ImGuiController : IDisposable
    {
        private GraphicsDevice _gd;
        private readonly Sdl2Window _window;
        private bool _frameBegun;

        // Veldrid objects
        private DeviceBuffer _vertexBuffer;
        private DeviceBuffer _indexBuffer;
        private DeviceBuffer _projMatrixBuffer;
        private Texture _fontTexture;
        private TextureView _fontTextureView;
        private Shader _vertexShader;
        private Shader _fragmentShader;
        private ResourceLayout _layout;
        private ResourceLayout _textureLayout;
        private Pipeline _pipeline;
        private ResourceSet _mainResourceSet;
        private ResourceSet _fontTextureResourceSet;

        private IntPtr _fontAtlasID = (IntPtr)1;
        private bool _controlDown;
        private bool _shiftDown;
        private bool _altDown;
        private bool _winKeyDown;

        private int _windowWidth;
        private int _windowHeight;
        private Vector2 _scaleFactor = Vector2.One;

        // Image trackers
        private readonly Dictionary<TextureView, ResourceSetInfo> _setsByView
            = new Dictionary<TextureView, ResourceSetInfo>();
        private readonly Dictionary<Texture, TextureView> _autoViewsByTexture
            = new Dictionary<Texture, TextureView>();
        private readonly Dictionary<IntPtr, ResourceSetInfo> _viewsById = new Dictionary<IntPtr, ResourceSetInfo>();
        private readonly List<IDisposable> _ownedResources = new List<IDisposable>();
        private readonly VeldridImGuiWindow _mainViewportWindow;
        private readonly Platform_CreateWindow _createWindow;
        private readonly Platform_DestroyWindow _destroyWindow;
        private readonly Platform_GetWindowPos _getWindowPos;
        private readonly Platform_ShowWindow _showWindow;
        private readonly Platform_SetWindowPos _setWindowPos;
        private readonly Platform_SetWindowSize _setWindowSize;
        private readonly Platform_GetWindowSize _getWindowSize;
        private readonly Platform_SetWindowFocus _setWindowFocus;
        private readonly Platform_GetWindowFocus _getWindowFocus;
        private readonly Platform_GetWindowMinimized _getWindowMinimized;
        private readonly Platform_SetWindowTitle _setWindowTitle;
        private int _lastAssignedID = 100;

        /// <summary>
        /// Constructs a new ImGuiController.
        /// </summary>
        public unsafe ImGuiController(GraphicsDevice gd, Sdl2Window window, OutputDescription outputDescription, int width, int height)
        {
            _gd = gd;
            _window = window;
            _windowWidth = width;
            _windowHeight = height;

            IntPtr context = ImGui.CreateContext();
            ImGui.SetCurrentContext(context);
            IntPtr implotContext = ImPlot.CreateContext();
            ImPlot.SetCurrentContext(implotContext);
            ImPlot.SetImGuiContext(context);
            //ImGuizmo.SetImGuiContext(context);
            ImGuiIOPtr io = ImGui.GetIO();
            io.Fonts.AddFontFromFileTTF("Roboto-Bold.ttf", 18);
            io.ConfigFlags |= ImGuiConfigFlags.DockingEnable;
            io.ConfigFlags |= ImGuiConfigFlags.ViewportsEnable;
            ImGui.StyleColorsDark();
            var style = ImGui.GetStyle();
            style.WindowRounding = 0.0f;
            style.FrameBorderSize = 1;
            style.Colors[(int)ImGuiCol.WindowBg] = new Vector4(0.1f, 0.105f, 0.11f, 1.0f);

            style.Colors[(int)ImGuiCol.Header] = new Vector4(0.2f, 0.205f, 0.21f, 1.0f);
            style.Colors[(int)ImGuiCol.HeaderHovered] = new Vector4(0.3f, 0.305f, 0.31f, 1.0f);
            style.Colors[(int)ImGuiCol.HeaderActive] = new Vector4(0.15f, 0.1505f, 0.151f, 1.0f);

            style.Colors[(int)ImGuiCol.Button] = new Vector4(0.2f, 0.205f, 0.21f, 1.0f);
            style.Colors[(int)ImGuiCol.ButtonHovered] = new Vector4(0.3f, 0.305f, 0.31f, 1.0f);
            style.Colors[(int)ImGuiCol.ButtonActive] = new Vector4(0.15f, 0.1505f, 0.151f, 1.0f);

            style.Colors[(int)ImGuiCol.FrameBg] = new Vector4(0.2f, 0.205f, 0.21f, 1.0f);
            style.Colors[(int)ImGuiCol.FrameBgHovered] = new Vector4(0.3f, 0.305f, 0.31f, 1.0f);
            style.Colors[(int)ImGuiCol.FrameBgActive] = new Vector4(0.15f, 0.1505f, 0.151f, 1.0f);
            ImGuiPlatformIOPtr platformIO = ImGui.GetPlatformIO();

            ImGuiViewportPtr mainViewport = platformIO.Viewports[0];
            mainViewport.PlatformHandle = window.Handle;
            _mainViewportWindow = new VeldridImGuiWindow(this, gd, mainViewport, _window);

            _createWindow = CreateWindow;
            _destroyWindow = DestroyWindow;
            _getWindowPos = GetWindowPos;
            _showWindow = ShowWindow;
            _setWindowPos = SetWindowPos;
            _setWindowSize = SetWindowSize;
            _getWindowSize = GetWindowSize;
            _setWindowFocus = SetWindowFocus;
            _getWindowFocus = GetWindowFocus;
            _getWindowMinimized = GetWindowMinimized;
            _setWindowTitle = SetWindowTitle;

            platformIO.Platform_CreateWindow = Marshal.GetFunctionPointerForDelegate(_createWindow);
            platformIO.Platform_DestroyWindow = Marshal.GetFunctionPointerForDelegate(_destroyWindow);
            platformIO.Platform_ShowWindow = Marshal.GetFunctionPointerForDelegate(_showWindow);
            platformIO.Platform_SetWindowPos = Marshal.GetFunctionPointerForDelegate(_setWindowPos);
            platformIO.Platform_SetWindowSize = Marshal.GetFunctionPointerForDelegate(_setWindowSize);
            platformIO.Platform_SetWindowFocus = Marshal.GetFunctionPointerForDelegate(_setWindowFocus);
            platformIO.Platform_GetWindowFocus = Marshal.GetFunctionPointerForDelegate(_getWindowFocus);
            platformIO.Platform_GetWindowMinimized = Marshal.GetFunctionPointerForDelegate(_getWindowMinimized);
            platformIO.Platform_SetWindowTitle = Marshal.GetFunctionPointerForDelegate(_setWindowTitle);

            ImGuiNative.ImGuiPlatformIO_Set_Platform_GetWindowPos(platformIO.NativePtr, Marshal.GetFunctionPointerForDelegate(_getWindowPos));
            ImGuiNative.ImGuiPlatformIO_Set_Platform_GetWindowSize(platformIO.NativePtr, Marshal.GetFunctionPointerForDelegate(_getWindowSize));

            unsafe
            {
                io.NativePtr->BackendPlatformName = (byte*)new FixedAsciiString("Veldrid.SDL2 Backend").DataPtr;
            }
            io.BackendFlags |= ImGuiBackendFlags.HasMouseCursors;
            io.BackendFlags |= ImGuiBackendFlags.HasSetMousePos;
            io.BackendFlags |= ImGuiBackendFlags.PlatformHasViewports;
            io.BackendFlags |= ImGuiBackendFlags.RendererHasViewports;
            io.BackendFlags |= ImGuiBackendFlags.RendererHasVtxOffset;

            var fonts = ImGui.GetIO().Fonts;
            fonts.AddFontDefault();

            CreateDeviceResources(gd, outputDescription);
            //SetKeyMappings();
            if (File.Exists("imguiSettings.ini"))
            {
                ImGui.LoadIniSettingsFromDisk("imguiSettings.ini");
            }

            SetPerFrameImGuiData(1f / 60f);
            UpdateMonitors();

            ImGui.NewFrame();
            _frameBegun = true;
        }

        private void CreateWindow(ImGuiViewportPtr vp)
        {
            VeldridImGuiWindow window = new VeldridImGuiWindow(this, _gd, vp);
        }

        private void DestroyWindow(ImGuiViewportPtr vp)
        {
            if (vp.PlatformUserData != IntPtr.Zero)
            {
                VeldridImGuiWindow window = (VeldridImGuiWindow)GCHandle.FromIntPtr(vp.PlatformUserData).Target!;
                window.Dispose();

                vp.PlatformUserData = IntPtr.Zero;
            }
        }

        private void ShowWindow(ImGuiViewportPtr vp)
        {
            VeldridImGuiWindow window = (VeldridImGuiWindow)GCHandle.FromIntPtr(vp.PlatformUserData).Target!;
            Sdl2Native.SDL_ShowWindow(window.Window.SdlWindowHandle);
        }

        private unsafe void GetWindowPos(ImGuiViewportPtr vp, Vector2* outPos)
        {
            VeldridImGuiWindow window = (VeldridImGuiWindow)GCHandle.FromIntPtr(vp.PlatformUserData).Target!;
            *outPos = new Vector2(window.Window.Bounds.X, window.Window.Bounds.Y);
        }

        private void SetWindowPos(ImGuiViewportPtr vp, Vector2 pos)
        {
            VeldridImGuiWindow window = (VeldridImGuiWindow)GCHandle.FromIntPtr(vp.PlatformUserData).Target!;
            window.Window.X = (int)pos.X;
            window.Window.Y = (int)pos.Y;
        }

        private void SetWindowSize(ImGuiViewportPtr vp, Vector2 size)
        {
            VeldridImGuiWindow window = (VeldridImGuiWindow)GCHandle.FromIntPtr(vp.PlatformUserData).Target!;
            Sdl2Native.SDL_SetWindowSize(window.Window.SdlWindowHandle, (int)size.X, (int)size.Y);
        }

        private unsafe void GetWindowSize(ImGuiViewportPtr vp, Vector2* outSize)
        {
            VeldridImGuiWindow window = (VeldridImGuiWindow)GCHandle.FromIntPtr(vp.PlatformUserData).Target!;
            Rectangle bounds = window.Window.Bounds;
            *outSize = new Vector2(bounds.Width, bounds.Height);
        }

        private delegate void SDL_RaiseWindow_t(IntPtr sdl2Window);
        private static SDL_RaiseWindow_t? p_sdl_RaiseWindow;

        private unsafe delegate uint SDL_GetGlobalMouseState_t(int* x, int* y);
        private static SDL_GetGlobalMouseState_t? p_sdl_GetGlobalMouseState;

        private unsafe delegate int SDL_GetDisplayUsableBounds_t(int displayIndex, Rectangle* rect);
        private static SDL_GetDisplayUsableBounds_t? p_sdl_GetDisplayUsableBounds_t;

        private delegate int SDL_GetNumVideoDisplays_t();
        private static SDL_GetNumVideoDisplays_t? p_sdl_GetNumVideoDisplays;

        private void SetWindowFocus(ImGuiViewportPtr vp)
        {
            if (p_sdl_RaiseWindow == null)
            {
                p_sdl_RaiseWindow = Sdl2Native.LoadFunction<SDL_RaiseWindow_t>("SDL_RaiseWindow");
            }

            VeldridImGuiWindow window = (VeldridImGuiWindow)GCHandle.FromIntPtr(vp.PlatformUserData).Target!;
            p_sdl_RaiseWindow(window.Window.SdlWindowHandle);
        }

        private byte GetWindowFocus(ImGuiViewportPtr vp)
        {
            VeldridImGuiWindow window = (VeldridImGuiWindow)GCHandle.FromIntPtr(vp.PlatformUserData).Target!;
            SDL_WindowFlags flags = Sdl2Native.SDL_GetWindowFlags(window.Window.SdlWindowHandle);
            return (flags & SDL_WindowFlags.InputFocus) != 0 ? (byte)1 : (byte)0;
        }

        private byte GetWindowMinimized(ImGuiViewportPtr vp)
        {
            VeldridImGuiWindow window = (VeldridImGuiWindow)GCHandle.FromIntPtr(vp.PlatformUserData).Target!;
            SDL_WindowFlags flags = Sdl2Native.SDL_GetWindowFlags(window.Window.SdlWindowHandle);
            return (flags & SDL_WindowFlags.Minimized) != 0 ? (byte)1 : (byte)0;
        }

        private unsafe void SetWindowTitle(ImGuiViewportPtr vp, IntPtr title)
        {
            VeldridImGuiWindow window = (VeldridImGuiWindow)GCHandle.FromIntPtr(vp.PlatformUserData).Target!;
            byte* titlePtr = (byte*)title;
            int count = 0;
            while (titlePtr[count] != 0)
            {
                count += 1;
            }
            window.Window.Title = System.Text.Encoding.ASCII.GetString(titlePtr, count);
        }

        public void WindowResized(int width, int height)
        {
            _windowWidth = width;
            _windowHeight = height;
        }

        public void DestroyDeviceObjects()
        {
            Dispose();
        }

        public void CreateDeviceResources(GraphicsDevice gd, OutputDescription outputDescription)
        {
            _gd = gd;
            ResourceFactory factory = gd.ResourceFactory;
            _vertexBuffer = factory.CreateBuffer(new BufferDescription(10000, BufferUsage.VertexBuffer | BufferUsage.DynamicReadWrite));
            _vertexBuffer.Name = "ImGui.NET Vertex Buffer";
            _indexBuffer = factory.CreateBuffer(new BufferDescription(2000, BufferUsage.IndexBuffer | BufferUsage.DynamicReadWrite));
            _indexBuffer.Name = "ImGui.NET Index Buffer";
            RecreateFontDeviceTexture(gd);

            _projMatrixBuffer = factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer | BufferUsage.DynamicReadWrite));
            _projMatrixBuffer.Name = "ImGui.NET Projection Buffer";
            const string frag = @"#version 450

            layout(set = 1, binding = 0) uniform texture2D FontTexture;
            layout(set = 0, binding = 1) uniform sampler FontSampler;
            
            layout (location = 0) in vec4 color;
            layout (location = 1) in vec2 texCoord;
            layout (location = 0) out vec4 outputColor;
            
            void main()
            {
                outputColor = color * texture(sampler2D(FontTexture, FontSampler), texCoord);
            }";

            const string vert = @"#version 450

            layout (location = 0) in vec2 in_position;
            layout (location = 1) in vec2 in_texCoord;
            layout (location = 2) in vec4 in_color;

            layout (binding = 0) uniform ProjectionMatrixBuffer
            {
                mat4 projection_matrix;
            };

            layout (location = 0) out vec4 color;
            layout (location = 1) out vec2 texCoord;

            void main() 
            {
                gl_Position = projection_matrix * vec4(in_position, 0, 1);
                color = in_color;
                texCoord = in_texCoord;
            }
            ";
            byte[] vertexShaderBytes = SpirvCompilation.CompileGlslToSpirv(vert, "imgui-vertex", ShaderStages.Vertex, GlslCompileOptions.Default).SpirvBytes;
            byte[] fragmentShaderBytes = SpirvCompilation.CompileGlslToSpirv(frag, "imgui-frag", ShaderStages.Fragment, GlslCompileOptions.Default).SpirvBytes;
            _vertexShader = factory.CreateShader(new ShaderDescription(ShaderStages.Vertex, vertexShaderBytes, gd.BackendType == GraphicsBackend.Metal ? "VS" : "main"));
            _fragmentShader = factory.CreateShader(new ShaderDescription(ShaderStages.Fragment, fragmentShaderBytes, gd.BackendType == GraphicsBackend.Metal ? "FS" : "main"));

            VertexLayoutDescription[] vertexLayouts = new VertexLayoutDescription[]
            {
                new VertexLayoutDescription(
                    new VertexElementDescription("in_position", VertexElementSemantic.Position, VertexElementFormat.Float2),
                    new VertexElementDescription("in_texCoord", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
                    new VertexElementDescription("in_color", VertexElementSemantic.Color, VertexElementFormat.Byte4_Norm))
            };

            _layout = factory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("ProjectionMatrixBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex),
                new ResourceLayoutElementDescription("MainSampler", ResourceKind.Sampler, ShaderStages.Fragment)));
            _textureLayout = factory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("MainTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment)));

            GraphicsPipelineDescription pd = new GraphicsPipelineDescription(
                BlendStateDescription.SingleAlphaBlend,
                new DepthStencilStateDescription(false, false, ComparisonKind.Always),
                new RasterizerStateDescription(FaceCullMode.None, PolygonFillMode.Solid, FrontFace.Clockwise, false, true),
                PrimitiveTopology.TriangleList,
                new ShaderSetDescription(vertexLayouts, new[] { _vertexShader, _fragmentShader }),
                new ResourceLayout[] { _layout, _textureLayout },
                outputDescription,
                ResourceBindingModel.Default);
            _pipeline = factory.CreateGraphicsPipeline(pd);

            _mainResourceSet = factory.CreateResourceSet(new ResourceSetDescription(_layout,
                _projMatrixBuffer,
                gd.PointSampler));

            _fontTextureResourceSet = factory.CreateResourceSet(new ResourceSetDescription(_textureLayout, _fontTextureView));
        }

        /// <summary>
        /// Gets or creates a handle for a texture to be drawn with ImGui.
        /// Pass the returned handle to Image() or ImageButton().
        /// </summary>
        public IntPtr GetOrCreateImGuiBinding(ResourceFactory factory, TextureView textureView)
        {
            if (!_setsByView.TryGetValue(textureView, out ResourceSetInfo rsi))
            {
                ResourceSet resourceSet = factory.CreateResourceSet(new ResourceSetDescription(_textureLayout, textureView));
                rsi = new ResourceSetInfo(GetNextImGuiBindingID(), resourceSet);

                _setsByView.Add(textureView, rsi);
                _viewsById.Add(rsi.ImGuiBinding, rsi);
                _ownedResources.Add(resourceSet);
            }

            return rsi.ImGuiBinding;
        }

        private IntPtr GetNextImGuiBindingID()
        {
            int newID = _lastAssignedID++;
            return (IntPtr)newID;
        }

        /// <summary>
        /// Gets or creates a handle for a texture to be drawn with ImGui.
        /// Pass the returned handle to Image() or ImageButton().
        /// </summary>
        public IntPtr GetOrCreateImGuiBinding(ResourceFactory factory, Texture texture)
        {
            if (!_autoViewsByTexture.TryGetValue(texture, out TextureView? textureView))
            {
                textureView = factory.CreateTextureView(texture);
                _autoViewsByTexture.Add(texture, textureView);
                _ownedResources.Add(textureView);
            }

            return GetOrCreateImGuiBinding(factory, textureView);
        }

        /// <summary>
        /// Retrieves the shader texture binding for the given helper handle.
        /// </summary>
        public ResourceSet GetImageResourceSet(IntPtr imGuiBinding)
        {
            if (!_viewsById.TryGetValue(imGuiBinding, out ResourceSetInfo tvi))
            {
                throw new InvalidOperationException("No registered ImGui binding with id " + imGuiBinding.ToString());
            }

            return tvi.ResourceSet;
        }

        public void ClearCachedImageResources()
        {
            foreach (IDisposable resource in _ownedResources)
            {
                resource.Dispose();
            }

            _ownedResources.Clear();
            _setsByView.Clear();
            _viewsById.Clear();
            _autoViewsByTexture.Clear();
            _lastAssignedID = 100;
        }

        private byte[] LoadEmbeddedShaderCode(ResourceFactory factory, string name, ShaderStages stage)
        {
            switch (factory.BackendType)
            {
                case GraphicsBackend.Direct3D11:
                    {
                        string resourceName = name + ".hlsl.bytes";
                        return GetEmbeddedResourceBytes(resourceName);
                    }
                case GraphicsBackend.OpenGL:
                    {
                        string resourceName = name + ".glsl";
                        return GetEmbeddedResourceBytes(resourceName);
                    }
                case GraphicsBackend.Vulkan:
                    {
                        string resourceName = name + ".spv";
                        return GetEmbeddedResourceBytes(resourceName);
                    }
                case GraphicsBackend.Metal:
                    {
                        string resourceName = name + ".metallib";
                        return GetEmbeddedResourceBytes(resourceName);
                    }
                default:
                    throw new NotImplementedException();
            }
        }

        private byte[] GetEmbeddedResourceBytes(string resourceName)
        {
            Assembly assembly = typeof(ImGuiController).Assembly;
            using (Stream s = assembly.GetManifestResourceStream(resourceName)!)
            {
                byte[] ret = new byte[s.Length];
                s.Read(ret, 0, (int)s.Length);
                return ret;
            }
        }

        /// <summary>
        /// Recreates the device texture used to render text.
        /// </summary>
        public unsafe void RecreateFontDeviceTexture(GraphicsDevice gd)
        {
            ImGuiIOPtr io = ImGui.GetIO();
            // Build
            byte* pixels;
            int width, height, bytesPerPixel;
            io.Fonts.GetTexDataAsRGBA32(out pixels, out width, out height, out bytesPerPixel);
            // Store our identifier
            io.Fonts.SetTexID(_fontAtlasID);

            _fontTexture = gd.ResourceFactory.CreateTexture(TextureDescription.Texture2D(
                (uint)width,
                (uint)height,
                1,
                1,
                PixelFormat.R8_G8_B8_A8_UNorm,
                TextureUsage.Sampled));
            _fontTexture.Name = "ImGui.NET Font Texture";
            gd.UpdateTexture(
                _fontTexture,
                (IntPtr)pixels,
                (uint)(bytesPerPixel * width * height),
                0,
                0,
                0,
                (uint)width,
                (uint)height,
                1,
                0,
                0);
            _fontTextureView = gd.ResourceFactory.CreateTextureView(_fontTexture);

            io.Fonts.ClearTexData();
        }

        /// <summary>
        /// Renders the ImGui draw list data.
        /// This method requires a <see cref="GraphicsDevice"/> because it may create new DeviceBuffers if the size of vertex
        /// or index data has increased beyond the capacity of the existing buffers.
        /// A <see cref="CommandList"/> is needed to submit drawing and resource update commands.
        /// </summary>
        public void Render(GraphicsDevice gd, CommandList cl)
        {
            if (_frameBegun)
            {
                _frameBegun = false;
                ImGui.Render();
                RenderImDrawData(ImGui.GetDrawData(), gd, cl);

                // Update and Render additional Platform Windows
                if ((ImGui.GetIO().ConfigFlags & ImGuiConfigFlags.ViewportsEnable) != 0)
                {
                    ImGui.UpdatePlatformWindows();
                    ImGuiPlatformIOPtr platformIO = ImGui.GetPlatformIO();
                    for (int i = 1; i < platformIO.Viewports.Size; i++)
                    {
                        ImGuiViewportPtr vp = platformIO.Viewports[i];
                        VeldridImGuiWindow window = (VeldridImGuiWindow)GCHandle.FromIntPtr(vp.PlatformUserData).Target!;
                        cl.SetFramebuffer(window.Swapchain.Framebuffer);
                        RenderImDrawData(vp.DrawData, gd, cl);
                    }
                }
            }
        }

        public void SwapExtraWindows(GraphicsDevice gd)
        {
            ImGuiPlatformIOPtr platformIO = ImGui.GetPlatformIO();
            for (int i = 1; i < platformIO.Viewports.Size; i++)
            {
                ImGuiViewportPtr vp = platformIO.Viewports[i];
                VeldridImGuiWindow window = (VeldridImGuiWindow)GCHandle.FromIntPtr(vp.PlatformUserData).Target!;
                gd.SwapBuffers(window.Swapchain);
            }
        }

        /// <summary>
        /// Updates ImGui input and IO configuration state.
        /// </summary>
        public void Update(float deltaSeconds)
        {
            if (_frameBegun)
            {
                ImGui.Render();
                ImGui.UpdatePlatformWindows();
            }

            SetPerFrameImGuiData(deltaSeconds);
            ImGuiPlatformIOPtr platformIO = ImGui.GetPlatformIO();
            ImVector<ImGuiViewportPtr> viewports = platformIO.Viewports;
            for (int i = 1; i < viewports.Size; i++)
            {
                ImGuiViewportPtr v = viewports[i];
                VeldridImGuiWindow window = (VeldridImGuiWindow)GCHandle.FromIntPtr(v.PlatformUserData).Target!;
                window.Update();
            }


            UpdateMouse();
            UpdateMonitors();

            _frameBegun = true;
            ImGui.NewFrame();

            //ImGui.Text($"Main viewport Position: {ImGui.GetPlatformIO().Viewports[0].Pos}");
            //ImGui.Text($"Main viewport Size: {ImGui.GetPlatformIO().Viewports[0].Size}");
            //ImGui.Text($"MoouseHoveredViewport: {ImGui.GetIO().MouseHoveredViewport}");
        }

        private unsafe void UpdateMonitors()
        {
            if (p_sdl_GetNumVideoDisplays == null)
            {
                p_sdl_GetNumVideoDisplays = Sdl2Native.LoadFunction<SDL_GetNumVideoDisplays_t>("SDL_GetNumVideoDisplays");
            }
            if (p_sdl_GetDisplayUsableBounds_t == null)
            {
                p_sdl_GetDisplayUsableBounds_t = Sdl2Native.LoadFunction<SDL_GetDisplayUsableBounds_t>("SDL_GetDisplayUsableBounds");
            }

            ImGuiPlatformIOPtr platformIO = ImGui.GetPlatformIO();
            Marshal.FreeHGlobal(platformIO.NativePtr->Monitors.Data);
            int numMonitors = p_sdl_GetNumVideoDisplays();
            IntPtr data = Marshal.AllocHGlobal(Unsafe.SizeOf<ImGuiPlatformMonitor>() * numMonitors);
            platformIO.NativePtr->Monitors = new ImVector(numMonitors, numMonitors, data);
            for (int i = 0; i < numMonitors; i++)
            {
                Rectangle r;
                p_sdl_GetDisplayUsableBounds_t(i, &r);
                ImGuiPlatformMonitorPtr monitor = platformIO.Monitors[i];
                monitor.DpiScale = 1f;
                monitor.MainPos = new Vector2(r.X, r.Y);
                monitor.MainSize = new Vector2(r.Width, r.Height);
                monitor.WorkPos = new Vector2(r.X, r.Y);
                monitor.WorkSize = new Vector2(r.Width, r.Height);
            }
        }

        /// <summary>
        /// Sets per-frame data based on the associated window.
        /// This is called by Update(float).
        /// </summary>
        private void SetPerFrameImGuiData(float deltaSeconds)
        {
            ImGuiIOPtr io = ImGui.GetIO();
            io.DisplaySize = new Vector2(
                _windowWidth / _scaleFactor.X,
                _windowHeight / _scaleFactor.Y);
            io.DisplayFramebufferScale = _scaleFactor;
            io.DeltaTime = deltaSeconds; // DeltaTime is in seconds.

            ImGui.GetPlatformIO().Viewports[0].Pos = new Vector2(_window.X, _window.Y);
            ImGui.GetPlatformIO().Viewports[0].Size = new Vector2(_window.Width, _window.Height);
        }

        static ImGuiKey KeycodeToImGuiKey(VKey keycode)
        {
            switch (keycode)
            {
                case VKey.Tab: return ImGuiKey.Tab;
                case VKey.Left: return ImGuiKey.LeftArrow;
                case VKey.Right: return ImGuiKey.RightArrow;
                case VKey.Up: return ImGuiKey.UpArrow;
                case VKey.Down: return ImGuiKey.DownArrow;
                case VKey.PageUp: return ImGuiKey.PageUp;
                case VKey.PageDown: return ImGuiKey.PageDown;
                case VKey.Home: return ImGuiKey.Home;
                case VKey.End: return ImGuiKey.End;
                case VKey.Insert: return ImGuiKey.Insert;
                case VKey.Delete: return ImGuiKey.Delete;
                case VKey.Backspace: return ImGuiKey.Backspace;
                case VKey.Space: return ImGuiKey.Space;
                case VKey.Return: return ImGuiKey.Enter;
                case VKey.Escape: return ImGuiKey.Escape;
                case VKey.Quote: return ImGuiKey.Apostrophe;
                case VKey.Comma: return ImGuiKey.Comma;
                case VKey.Minus: return ImGuiKey.Minus;
                case VKey.Period: return ImGuiKey.Period;
                case VKey.Slash: return ImGuiKey.Slash;
                case VKey.Semicolon: return ImGuiKey.Semicolon;
                case VKey.Equals: return ImGuiKey.Equal;
                case VKey.LeftBracket: return ImGuiKey.LeftBracket;
                case VKey.Backslash: return ImGuiKey.Backslash;
                case VKey.RightBracket: return ImGuiKey.RightBracket;
                case VKey.Backquote: return ImGuiKey.GraveAccent;
                case VKey.CapsLock: return ImGuiKey.CapsLock;
                case VKey.ScrollLock: return ImGuiKey.ScrollLock;
                case VKey.NumLockClear: return ImGuiKey.NumLock;
                case VKey.PrintScreen: return ImGuiKey.PrintScreen;
                case VKey.Pause: return ImGuiKey.Pause;
                case VKey.Keypad0: return ImGuiKey.Keypad0;
                case VKey.Keypad1: return ImGuiKey.Keypad1;
                case VKey.Keypad2: return ImGuiKey.Keypad2;
                case VKey.Keypad3: return ImGuiKey.Keypad3;
                case VKey.Keypad4: return ImGuiKey.Keypad4;
                case VKey.Keypad5: return ImGuiKey.Keypad5;
                case VKey.Keypad6: return ImGuiKey.Keypad6;
                case VKey.Keypad7: return ImGuiKey.Keypad7;
                case VKey.Keypad8: return ImGuiKey.Keypad8;
                case VKey.Keypad9: return ImGuiKey.Keypad9;
                case VKey.KeypadPeriod: return ImGuiKey.KeypadDecimal;
                case VKey.KeypadDivide: return ImGuiKey.KeypadDivide;
                case VKey.KeypadMultiply: return ImGuiKey.KeypadMultiply;
                case VKey.KeypadMinus: return ImGuiKey.KeypadSubtract;
                case VKey.KeypadPlus: return ImGuiKey.KeypadAdd;
                case VKey.KeypadEnter: return ImGuiKey.KeypadEnter;
                case VKey.KeypadEquals: return ImGuiKey.KeypadEqual;
                case VKey.LeftControl: return ImGuiKey.LeftCtrl;
                case VKey.LeftShift: return ImGuiKey.LeftShift;
                case VKey.LeftAlt: return ImGuiKey.LeftAlt;
                case VKey.LeftGui: return ImGuiKey.LeftSuper;
                case VKey.RightControl: return ImGuiKey.RightCtrl;
                case VKey.RightShift: return ImGuiKey.RightShift;
                case VKey.RightAlt: return ImGuiKey.RightAlt;
                case VKey.RightGui: return ImGuiKey.RightSuper;
                case VKey.Application: return ImGuiKey.Menu;
                case VKey.Num0: return ImGuiKey._0;
                case VKey.Num1: return ImGuiKey._1;
                case VKey.Num2: return ImGuiKey._2;
                case VKey.Num3: return ImGuiKey._3;
                case VKey.Num4: return ImGuiKey._4;
                case VKey.Num5: return ImGuiKey._5;
                case VKey.Num6: return ImGuiKey._6;
                case VKey.Num7: return ImGuiKey._7;
                case VKey.Num8: return ImGuiKey._8;
                case VKey.Num9: return ImGuiKey._9;
                case VKey.a: return ImGuiKey.A;
                case VKey.b: return ImGuiKey.B;
                case VKey.c: return ImGuiKey.C;
                case VKey.d: return ImGuiKey.D;
                case VKey.e: return ImGuiKey.E;
                case VKey.f: return ImGuiKey.F;
                case VKey.g: return ImGuiKey.G;
                case VKey.h: return ImGuiKey.H;
                case VKey.i: return ImGuiKey.I;
                case VKey.j: return ImGuiKey.J;
                case VKey.k: return ImGuiKey.K;
                case VKey.l: return ImGuiKey.L;
                case VKey.m: return ImGuiKey.M;
                case VKey.n: return ImGuiKey.N;
                case VKey.o: return ImGuiKey.O;
                case VKey.p: return ImGuiKey.P;
                case VKey.q: return ImGuiKey.Q;
                case VKey.r: return ImGuiKey.R;
                case VKey.s: return ImGuiKey.S;
                case VKey.t: return ImGuiKey.T;
                case VKey.u: return ImGuiKey.U;
                case VKey.v: return ImGuiKey.V;
                case VKey.w: return ImGuiKey.W;
                case VKey.x: return ImGuiKey.X;
                case VKey.y: return ImGuiKey.Y;
                case VKey.z: return ImGuiKey.Z;
                case VKey.F1: return ImGuiKey.F1;
                case VKey.F2: return ImGuiKey.F2;
                case VKey.F3: return ImGuiKey.F3;
                case VKey.F4: return ImGuiKey.F4;
                case VKey.F5: return ImGuiKey.F5;
                case VKey.F6: return ImGuiKey.F6;
                case VKey.F7: return ImGuiKey.F7;
                case VKey.F8: return ImGuiKey.F8;
                case VKey.F9: return ImGuiKey.F9;
                case VKey.F10: return ImGuiKey.F10;
                case VKey.F11: return ImGuiKey.F11;
                case VKey.F12: return ImGuiKey.F12;
            }
            return ImGuiKey.None;
        }

        private void UpdateMouse()
        {
            ImGuiIOPtr io = ImGui.GetIO();
            // Determine if any of the mouse buttons were pressed during this snapshot period, even if they are no longer held.

            if (p_sdl_GetGlobalMouseState == null)
            {
                p_sdl_GetGlobalMouseState = Sdl2Native.LoadFunction<SDL_GetGlobalMouseState_t>("SDL_GetGlobalMouseState");
            }

            int x, y;
            unsafe
            {
                uint buttons = p_sdl_GetGlobalMouseState(&x, &y);

                io.MouseDown[0] = (buttons & 0b00001) != 0;//left
                io.MouseDown[1] = (buttons & 0b00100) != 0;//right
                io.MouseDown[2] = (buttons & 0b00010) != 0;//middle
                io.MouseDown[3] = (buttons & 0b01000) != 0;//back
                io.MouseDown[4] = (buttons & 0b10000) != 0;//forward
            }
            io.MousePos = new Vector2(x, y);
        }

        public void UpdateKeys(KeyEvent keyEvent)
        {
            ImGuiIOPtr io = ImGui.GetIO();
            io.AddKeyEvent(KeycodeToImGuiKey(keyEvent.Virtual), keyEvent.Down);
            if (keyEvent.Physical == Key.LeftControl || keyEvent.Physical == Key.RightControl)
            {
                _controlDown = keyEvent.Down;
            }
            if (keyEvent.Physical == Key.LeftShift || keyEvent.Physical == Key.RightShift)
            {
                _shiftDown = keyEvent.Down;
            }
            if (keyEvent.Physical == Key.LeftAlt || keyEvent.Physical == Key.RightAlt)
            {
                _altDown = keyEvent.Down;
            }
            if (keyEvent.Physical == Key.LeftGui || keyEvent.Physical == Key.RightGui)
            {
                _winKeyDown = keyEvent.Down;
            }
            io.KeyMods = _controlDown ? ImGuiKey.ImGuiMod_Ctrl : ImGuiKey.ImGuiMod_None;
            io.KeyCtrl = _controlDown;
            io.KeyMods |= _altDown ? ImGuiKey.ImGuiMod_Alt : ImGuiKey.ImGuiMod_None;
            io.KeyAlt = _altDown;
            io.KeyMods |= _shiftDown ? ImGuiKey.ImGuiMod_Shift : ImGuiKey.ImGuiMod_None;
            io.KeyShift = _shiftDown;
            io.KeyMods |= _winKeyDown ? ImGuiKey.ImGuiMod_Super : ImGuiKey.ImGuiMod_None;
            io.KeySuper = _winKeyDown;
        }

        public void UpdateMouseWheel(MouseWheelEvent wheelEvent)
        {
            ImGuiIOPtr io = ImGui.GetIO();
            io.AddMouseWheelEvent(wheelEvent.WheelDelta.X, wheelEvent.WheelDelta.Y);
        }

        public void UpdateText(TextInputEvent textInputEvent)
        {
            ImGuiIOPtr io = ImGui.GetIO();
            for (int i = 0; i < textInputEvent.Runes.Length; i++)
            {
                uint c = (uint)textInputEvent.Runes[i].Value;
                io.AddInputCharacter(c);
            }
        }

        private void RenderImDrawData(ImDrawDataPtr draw_data, GraphicsDevice gd, CommandList cl)
        {
            uint vertexOffsetInVertices = 0;
            uint indexOffsetInElements = 0;

            if (draw_data.CmdListsCount == 0)
            {
                return;
            }

            uint totalVBSize = (uint)(draw_data.TotalVtxCount * Unsafe.SizeOf<ImDrawVert>());
#pragma warning disable CS0618 // Type or member is obsolete
            if (totalVBSize > _vertexBuffer.SizeInBytes)
            {
                gd.DisposeWhenIdle(_vertexBuffer);
                _vertexBuffer = gd.ResourceFactory.CreateBuffer(new BufferDescription((uint)(totalVBSize * 1.5f), BufferUsage.VertexBuffer | BufferUsage.DynamicReadWrite));
            }

            uint totalIBSize = (uint)(draw_data.TotalIdxCount * sizeof(ushort));
            if (totalIBSize > _indexBuffer.SizeInBytes)
            {
                gd.DisposeWhenIdle(_indexBuffer);
                _indexBuffer = gd.ResourceFactory.CreateBuffer(new BufferDescription((uint)(totalIBSize * 1.5f), BufferUsage.IndexBuffer | BufferUsage.DynamicReadWrite));
            }
#pragma warning restore CS0618 // Type or member is obsolete

            Vector2 pos = draw_data.DisplayPos;
            for (int i = 0; i < draw_data.CmdListsCount; i++)
            {
                ImDrawListPtr cmd_list = draw_data.CmdListsRange[i];

                cl.UpdateBuffer(
                    _vertexBuffer,
                    vertexOffsetInVertices * (uint)Unsafe.SizeOf<ImDrawVert>(),
                    cmd_list.VtxBuffer.Data,
                    (uint)(cmd_list.VtxBuffer.Size * Unsafe.SizeOf<ImDrawVert>()));

                cl.UpdateBuffer(
                    _indexBuffer,
                    indexOffsetInElements * sizeof(ushort),
                    cmd_list.IdxBuffer.Data,
                    (uint)(cmd_list.IdxBuffer.Size * sizeof(ushort)));

                vertexOffsetInVertices += (uint)cmd_list.VtxBuffer.Size;
                indexOffsetInElements += (uint)cmd_list.IdxBuffer.Size;
            }

            // Setup orthographic projection matrix into our constant buffer
            ImGuiIOPtr io = ImGui.GetIO();
            Matrix4x4 mvp = Matrix4x4.CreateOrthographicOffCenter(
                pos.X,
                pos.X + draw_data.DisplaySize.X,
                pos.Y + draw_data.DisplaySize.Y,
                pos.Y,
                -1.0f,
                1.0f);

            cl.UpdateBuffer(_projMatrixBuffer, 0, ref mvp);

            cl.SetVertexBuffer(0, _vertexBuffer);
            cl.SetIndexBuffer(_indexBuffer, IndexFormat.UInt16);
            cl.SetPipeline(_pipeline);
            cl.SetGraphicsResourceSet(0, _mainResourceSet);

            draw_data.ScaleClipRects(io.DisplayFramebufferScale);

            // Render command lists
            int vtx_offset = 0;
            int idx_offset = 0;
            for (int n = 0; n < draw_data.CmdListsCount; n++)
            {
                ImDrawListPtr cmd_list = draw_data.CmdListsRange[n];
                for (int cmd_i = 0; cmd_i < cmd_list.CmdBuffer.Size; cmd_i++)
                {
                    ImDrawCmdPtr pcmd = cmd_list.CmdBuffer[cmd_i];
                    if (pcmd.UserCallback != IntPtr.Zero)
                    {
                        throw new NotImplementedException();
                    }
                    else
                    {
                        if (pcmd.TextureId != IntPtr.Zero)
                        {
                            if (pcmd.TextureId == _fontAtlasID)
                            {
                                cl.SetGraphicsResourceSet(1, _fontTextureResourceSet);
                            }
                            else
                            {
                                cl.SetGraphicsResourceSet(1, GetImageResourceSet(pcmd.TextureId));
                            }
                        }

                        cl.SetScissorRect(
                            0,
                            (uint)(pcmd.ClipRect.X - pos.X),
                            (uint)(pcmd.ClipRect.Y - pos.Y),
                            (uint)(pcmd.ClipRect.Z - pcmd.ClipRect.X),
                            (uint)(pcmd.ClipRect.W - pcmd.ClipRect.Y));

                        cl.DrawIndexed(pcmd.ElemCount, 1, pcmd.IdxOffset + (uint)idx_offset, (int)pcmd.VtxOffset + vtx_offset, 0);
                    }
                }
                vtx_offset += cmd_list.VtxBuffer.Size;
                idx_offset += cmd_list.IdxBuffer.Size;
            }
        }

        /// <summary>
        /// Frees all graphics resources used by the renderer.
        /// </summary>
        public void Dispose()
        {
            _vertexBuffer.Dispose();
            _indexBuffer.Dispose();
            _projMatrixBuffer.Dispose();
            _fontTexture.Dispose();
            _fontTextureView.Dispose();
            _vertexShader.Dispose();
            _fragmentShader.Dispose();
            _layout.Dispose();
            _textureLayout.Dispose();
            _pipeline.Dispose();
            _mainResourceSet.Dispose();

            foreach (IDisposable resource in _ownedResources)
            {
                resource.Dispose();
            }
        }

        private struct ResourceSetInfo
        {
            public readonly IntPtr ImGuiBinding;
            public readonly ResourceSet ResourceSet;

            public ResourceSetInfo(IntPtr imGuiBinding, ResourceSet resourceSet)
            {
                ImGuiBinding = imGuiBinding;
                ResourceSet = resourceSet;
            }
        }
    }
}

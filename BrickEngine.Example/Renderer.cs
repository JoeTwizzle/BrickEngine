using BrickEngine.Assets.Data;
using BrickEngine.Core;
using BrickEngine.Example.RayTracing;
using Veldrid;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Veldrid.SPIRV;
using System;
using System.Runtime.InteropServices;
using System.Net.Http.Headers;
using System.Drawing;
using static BrickEngine.Example.RaytracedScene;

namespace BrickEngine.Example
{
    sealed class Renderer
    {
        public RaytracedScene Scene;
        CommandList cl;
        DeviceBuffer cameraBuffer;
        ResourceLayout renderBufferLayout;
        ResourceLayout displayBufferLayout;
        ResourceLayout meshBufferLayout;
        ResourceSet cameraSet;
        ResourceSet tlBvhSet;
        ResourceSet meshSet;
        ResourceSet renderSet;
        ResourceSet displaySet;
        Pipeline displayPipline;
        Pipeline raytracePipeline;
        Texture mainTex;
        Texture depthTex;
        GameWindow window;

        public Camera Camera { get; }

        public Renderer(RaytracedScene scene, GameWindow window)
        {
            Scene = scene;
            Camera = new();
            this.window = window;
            ResourceFactory rf = window.GraphicsContext.ResourceFactory;
            var gd = window.GraphicsContext.GraphicsDevice;


            //Create Camera buffer
            Camera.AspectRatio = (window.Window.Width / (float)window.Window.Height);
            cameraBuffer = rf.CreateBuffer(new BufferDescription((uint)(Unsafe.SizeOf<CameraProperties>()), BufferUsage.UniformBuffer, 0));
            var p = Camera.ProjectionMatrix;
            Matrix4x4.Invert(p, out var pInv);
            gd.UpdateBuffer(cameraBuffer, 0, new CameraProperties(Camera.Transform.WorldMatrix, pInv));

            //Create resource layouts
            var cameraBufferLayout = rf.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("_CameraProperites", ResourceKind.UniformBuffer, ShaderStages.Compute))
                );
            cameraSet = rf.CreateResourceSet(new ResourceSetDescription(cameraBufferLayout, cameraBuffer));

            // BVH LAYOUT
            var bvhBufferLayout = rf.CreateResourceLayout(new ResourceLayoutDescription(
                    new ResourceLayoutElementDescription("BVHNodeBuffer", ResourceKind.StructuredBufferReadOnly, ShaderStages.Compute),
                    new ResourceLayoutElementDescription("BVHIndexBuffer", ResourceKind.StructuredBufferReadOnly, ShaderStages.Compute),
                    new ResourceLayoutElementDescription("BlBVHNodeBuffer", ResourceKind.StructuredBufferReadOnly, ShaderStages.Compute),
                    new ResourceLayoutElementDescription("BlBVHIndexBuffer", ResourceKind.StructuredBufferReadOnly, ShaderStages.Compute)
                ));
            //Create TlBvh buffers
            var nodeBuffer = rf.CreateBuffer(new BufferDescription((uint)(Unsafe.SizeOf<Node>() * Scene.TopLevelBvh.Nodes.Length), BufferUsage.StructuredBufferReadOnly, (uint)Unsafe.SizeOf<Node>()));
            var objectIndexBuffer = rf.CreateBuffer(new BufferDescription((uint)(sizeof(uint) * Scene.TopLevelBvh.PrimitiveIndices.Length), BufferUsage.StructuredBufferReadOnly, sizeof(uint)));
            gd.UpdateBuffer(nodeBuffer, 0, Scene.TopLevelBvh.Nodes);
            gd.UpdateBuffer(objectIndexBuffer, 0, Scene.TopLevelBvh.PrimitiveIndices);
            //Create BlBvh buffers
            var blNodeBuffer = rf.CreateBuffer(new BufferDescription((uint)(Unsafe.SizeOf<Node>() * Scene.BlNodes.Length), BufferUsage.StructuredBufferReadOnly, (uint)Unsafe.SizeOf<Node>()));
            var blPrimIndexBuffer = rf.CreateBuffer(new BufferDescription((uint)(sizeof(uint) * Scene.BlPrimIndices.Length), BufferUsage.StructuredBufferReadOnly, sizeof(uint)));
            gd.UpdateBuffer(blNodeBuffer, 0, Scene.BlNodes);
            gd.UpdateBuffer(blPrimIndexBuffer, 0, Scene.BlPrimIndices);
            tlBvhSet = rf.CreateResourceSet(new ResourceSetDescription(bvhBufferLayout, nodeBuffer, objectIndexBuffer, blNodeBuffer, blPrimIndexBuffer));


            renderBufferLayout = rf.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("screen", ResourceKind.TextureReadWrite, ShaderStages.Compute),
                new ResourceLayoutElementDescription("depth", ResourceKind.TextureReadWrite, ShaderStages.Compute))
                );

            meshBufferLayout = rf.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("ObjectIdToModelIdBuffer", ResourceKind.StructuredBufferReadOnly, ShaderStages.Compute),
                new ResourceLayoutElementDescription("TransformsBuffer", ResourceKind.StructuredBufferReadOnly, ShaderStages.Compute),
                new ResourceLayoutElementDescription("ModelIdToMeshesBuffer", ResourceKind.StructuredBufferReadOnly, ShaderStages.Compute),
                new ResourceLayoutElementDescription("ModelInfosBuffer", ResourceKind.StructuredBufferReadOnly, ShaderStages.Compute),
                new ResourceLayoutElementDescription("VerticesBuffer", ResourceKind.StructuredBufferReadOnly, ShaderStages.Compute),
                new ResourceLayoutElementDescription("IndicesBuffer", ResourceKind.StructuredBufferReadOnly, ShaderStages.Compute))
                );

            var objectIdToModelIdBuffer = rf.CreateBuffer(new BufferDescription((uint)(sizeof(uint) * Scene.ObjectIdToModelId.Length), BufferUsage.StructuredBufferReadOnly, sizeof(uint)));
            objectIdToModelIdBuffer.Name = "objectIdToModelIdBuffer";
            var transformsBuffer = rf.CreateBuffer(new BufferDescription((uint)(Unsafe.SizeOf<Matrix4x4>() * Scene.WorldTransforms.Length), BufferUsage.StructuredBufferReadOnly, (uint)Unsafe.SizeOf<Matrix4x4>()));
            transformsBuffer.Name = "transformsBuffer";
            var modelIdToMeshesBuffer = rf.CreateBuffer(new BufferDescription((uint)(sizeof(uint) * Scene.ModelToMeshesArray.Length), BufferUsage.StructuredBufferReadOnly, sizeof(uint)));
            modelIdToMeshesBuffer.Name = "modelIdToMeshesBuffer";
            var modelInfosBuffer = rf.CreateBuffer(new BufferDescription((uint)(Unsafe.SizeOf<ModelInfo>() * Scene.ModelInfos.Length), BufferUsage.StructuredBufferReadOnly, (uint)Unsafe.SizeOf<ModelInfo>()));
            modelInfosBuffer.Name = "modelInfosBuffer";
            var verticesBuffer = rf.CreateBuffer(new BufferDescription((uint)(Unsafe.SizeOf<RayVertexGpu>() * Scene.Vertices.Length), BufferUsage.StructuredBufferReadOnly, (uint)Unsafe.SizeOf<RayVertexGpu>()));
            verticesBuffer.Name = "verticesBuffer";
            var indicesBuffer = rf.CreateBuffer(new BufferDescription((uint)(sizeof(uint) * Scene.Indices.Length), BufferUsage.StructuredBufferReadOnly, sizeof(uint)));
            indicesBuffer.Name = "indicesBuffer";
            gd.UpdateBuffer(objectIdToModelIdBuffer, 0, Scene.ObjectIdToModelId);
            gd.UpdateBuffer(transformsBuffer, 0, Scene.WorldTransforms);
            gd.UpdateBuffer(modelIdToMeshesBuffer, 0, Scene.ModelToMeshesArray);
            gd.UpdateBuffer(modelInfosBuffer, 0, Scene.ModelInfos);
            gd.UpdateBuffer(verticesBuffer, 0, Scene.Vertices);
            gd.UpdateBuffer(indicesBuffer, 0, Scene.Indices);

            //create resource sets
            meshSet = rf.CreateResourceSet(new ResourceSetDescription(meshBufferLayout, objectIdToModelIdBuffer, transformsBuffer, modelIdToMeshesBuffer, modelInfosBuffer, verticesBuffer, indicesBuffer));


            //Create shader
            var shaderResult = SpirvCompilation.CompileGlslToSpirv(File.ReadAllText("Raytrace.glsl"), "Raytrace.glsl", ShaderStages.Compute, new GlslCompileOptions(true));
            var shader = rf.CreateShader(new ShaderDescription(ShaderStages.Compute, shaderResult.SpirvBytes, "main", true));

            //Create Pipeline
            raytracePipeline = rf.CreateComputePipeline(new ComputePipelineDescription(shader, new ResourceLayout[] { cameraBufferLayout, bvhBufferLayout, renderBufferLayout, meshBufferLayout }, 8, 8, 1));


            //-----------DISPLAY-------------
            const string fsTriVert = """
            #version 460
            layout(location = 0) out vec2 texCoord;

            void main()
            {
                float x = -1.0 + float((gl_VertexIndex & 1) << 2);
                float y = -1.0 + float((gl_VertexIndex & 2) << 1);
                texCoord.x = (x+1.0)*0.5;
                texCoord.y = (y+1.0)*0.5;
                gl_Position = vec4(x, y, 0, 1);
            }
            """;
            const string fsTriFrag = """
            #version 460
            layout(location = 0) in vec2 texCoord;
            layout(set = 0, binding = 0) uniform sampler _MainSampler;
            layout(set = 0, binding = 1) uniform texture2D _MainTexture;
            layout(location = 0) out vec4 color;

            void main()
            {
              color = vec4(texture(sampler2D(_MainTexture, _MainSampler), texCoord).rgb, 1.0);
            }
            """;

            var vertShaderResult = SpirvCompilation.CompileGlslToSpirv(fsTriVert, "fsTriVert.glsl", ShaderStages.Vertex, GlslCompileOptions.Default);
            var vertShader = rf.CreateShader(new ShaderDescription(ShaderStages.Vertex, vertShaderResult.SpirvBytes, "main"));
            var fragResult = SpirvCompilation.CompileGlslToSpirv(fsTriFrag, "fsTriFrag.glsl", ShaderStages.Fragment, GlslCompileOptions.Default);
            var fragShader = rf.CreateShader(new ShaderDescription(ShaderStages.Fragment, fragResult.SpirvBytes, "main"));

            //Create 
            displayBufferLayout = rf.CreateResourceLayout(new ResourceLayoutDescription(
                  new ResourceLayoutElementDescription("_MainSampler", ResourceKind.Sampler, ShaderStages.Fragment),
                  new ResourceLayoutElementDescription("_MainTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment))
                  );

            CreateTextures(window, rf);

            var fsTriShaderSet = new ShaderSetDescription(null, new Shader[] { vertShader, fragShader });
            displayPipline = rf.CreateGraphicsPipeline(new GraphicsPipelineDescription(BlendStateDescription.SingleOverrideBlend, DepthStencilStateDescription.Disabled, RasterizerStateDescription.CullNone,
                  PrimitiveTopology.TriangleList, fsTriShaderSet, displayBufferLayout, window.GraphicsContext.GraphicsDevice.MainSwapchain!.Framebuffer.OutputDescription));

            cl = rf.CreateCommandList();
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct CameraProperties
        {
            public Matrix4x4 Model;
            public Matrix4x4 CameraInverseProjection;

            public CameraProperties(Matrix4x4 cameraToWorld, Matrix4x4 cameraInverseProjection)
            {
                Model = cameraToWorld;
                CameraInverseProjection = cameraInverseProjection;
            }
        }

        private void CreateTextures(GameWindow window, ResourceFactory rf)
        {
            mainTex = rf.CreateTexture(new TextureDescription((uint)window.Window.Width, (uint)window.Window.Height, 1, 1, 1, PixelFormat.R16_G16_B16_A16_Float, TextureUsage.Sampled | TextureUsage.Storage, TextureType.Texture2D));
            depthTex = rf.CreateTexture(new TextureDescription((uint)window.Window.Width, (uint)window.Window.Height, 1, 1, 1, PixelFormat.R32_Float, TextureUsage.Sampled | TextureUsage.Storage, TextureType.Texture2D));
            renderSet = rf.CreateResourceSet(new ResourceSetDescription(renderBufferLayout, mainTex, depthTex));
            displaySet = rf.CreateResourceSet(new ResourceSetDescription(displayBufferLayout, window.GraphicsContext.GraphicsDevice.LinearSampler, mainTex));
        }

        public void Resize()
        {
            if (mainTex.Width != window.Window.Width || mainTex.Height != window.Window.Height)
            {
                Camera.AspectRatio = (window.Window.Width / (float)window.Window.Height);
                window.GraphicsContext.DisposeWhenUnused(renderSet);
                window.GraphicsContext.DisposeWhenUnused(displaySet);
                window.GraphicsContext.DisposeWhenUnused(mainTex);
                window.GraphicsContext.DisposeWhenUnused(depthTex);
                CreateTextures(window, window.GraphicsContext.ResourceFactory);
            }
        }

        public void Update()
        {
            //window.Input.GetKey(Key.K);
            //window.DeltaTime
            Camera.Update(window.Input, window.DeltaTime);

            var p = Camera.PerspectiveMatrix;
            Matrix4x4.Invert(p, out var pInv);

            cl.Begin();
            cl.UpdateBuffer(cameraBuffer, 0, new CameraProperties(Camera.Transform.WorldMatrix, pInv));
            cl.End();
            window.GraphicsContext.SubmitCommands(cl);
        }

        public void Render()
        {
            cl.Begin();
            cl.SetPipeline(raytracePipeline);
            cl.SetComputeResourceSet(0, cameraSet);
            cl.SetComputeResourceSet(1, tlBvhSet);
            cl.SetComputeResourceSet(2, renderSet);
            cl.SetComputeResourceSet(3, meshSet);
            uint worksizeX = BitOperations.RoundUpToPowerOf2((uint)window.Window.Width);
            uint worksizeY = BitOperations.RoundUpToPowerOf2((uint)window.Window.Height);
            cl.Dispatch(worksizeX / 8, worksizeY / 8, 1);
            cl.SetPipeline(displayPipline);
            cl.SetGraphicsResourceSet(0, displaySet);
            cl.SetFramebuffer(window.GraphicsContext.GraphicsDevice.SwapchainFramebuffer!);
            cl.SetFullViewport(0);
            cl.ClearColorTarget(0, RgbaFloat.Blue);
            cl.Draw(3);
            cl.End();
            window.GraphicsContext.SubmitCommands(cl);
        }
    }
}

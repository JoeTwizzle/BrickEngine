using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrickEngine.Core.Graphics
{
    public class GraphicsCache : IDisposable
    {
        readonly ResourceFactory factory;
        public GraphicsCache(ResourceFactory factory)
        {
            this.factory = factory;
        }

        readonly Dictionary<GraphicsPipelineDescription, Pipeline> pipelineCache = new();
        public Pipeline GetPipeline(GraphicsPipelineDescription description)
        {
            if (pipelineCache.TryGetValue(description, out var pipeline))
            {
                return pipeline;
            }
            pipeline = factory.CreateGraphicsPipeline(description);
            pipelineCache.Add(description, pipeline);
            return pipeline;
        }

        readonly Dictionary<ResourceLayoutDescription, ResourceLayout> resourceLayoutCache = new();
        public ResourceLayout GetResourceLayout(ResourceLayoutDescription description)
        {
            if (resourceLayoutCache.TryGetValue(description, out var resourceLayout))
            {
                return resourceLayout;
            }
            resourceLayout = factory.CreateResourceLayout(description);
            resourceLayoutCache.Add(description, resourceLayout);
            return resourceLayout;
        }

        readonly Dictionary<string, Pipeline> pipelineDescCache = new();
        public Pipeline GetPipeline(string name, GraphicsPipelineDescription description)
        {
            if (pipelineDescCache.TryGetValue(name, out var pipeline))
            {
                return pipeline;
            }
            pipeline = GetPipeline(description);
            pipelineDescCache.Add(name, pipeline);
            return pipeline;
        }

        readonly Dictionary<string, ResourceLayout> resourceLayoutDescCache = new();
        public ResourceLayout GetResourceLayout(string name, ResourceLayoutDescription description)
        {
            if (resourceLayoutDescCache.TryGetValue(name, out var resourceLayout))
            {
                return resourceLayout;
            }
            var layout = GetResourceLayout(description);
            resourceLayoutDescCache.Add(name, layout);
            return layout;
        }

        public bool TryGetPipeline(string name, [NotNullWhen(true)] out Pipeline? pipeline)
        {
            return pipelineDescCache.TryGetValue(name, out pipeline);
        }

        public bool TryGetResourceLayout(string name, [NotNullWhen(true)] out ResourceLayout? resourceLayout)
        {
            return resourceLayoutDescCache.TryGetValue(name, out resourceLayout);
        }

        public void Dispose()
        {
            pipelineDescCache.Clear();
            pipelineCache.Clear();
            resourceLayoutCache.Clear();
            resourceLayoutDescCache.Clear();
        }
    }
}

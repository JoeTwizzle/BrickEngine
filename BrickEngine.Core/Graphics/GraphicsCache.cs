using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrickEngine.Core.Graphics
{
    public class GraphicsCache
    {
        ResourceFactory factory;
        public GraphicsCache(ResourceFactory factory)
        {
            this.factory = factory;
        }

        Dictionary<GraphicsPipelineDescription, Pipeline> PipelineCache = new Dictionary<GraphicsPipelineDescription, Pipeline>();
        public Pipeline GetPipeline(GraphicsPipelineDescription description)
        {
            if (PipelineCache.TryGetValue(description, out var pipeline))
            {
                return pipeline;
            }
            pipeline = factory.CreateGraphicsPipeline(description);
            PipelineCache.Add(description, pipeline);
            return pipeline;
        }

        Dictionary<ResourceLayoutDescription, ResourceLayout> ResourceLayoutCache = new Dictionary<ResourceLayoutDescription, ResourceLayout>();
        public ResourceLayout GetResourceLayout(ResourceLayoutDescription description)
        {
            if (ResourceLayoutCache.TryGetValue(description, out var resourceLayout))
            {
                return resourceLayout;
            }
            resourceLayout = factory.CreateResourceLayout(description);
            ResourceLayoutCache.Add(description, resourceLayout);
            return resourceLayout;
        }

        Dictionary<string, Pipeline> PipelineDescCache = new Dictionary<string, Pipeline>();
        public Pipeline GetPipeline(string name, GraphicsPipelineDescription description)
        {
            if (PipelineDescCache.TryGetValue(name, out var pipeline))
            {
                return pipeline;
            }
            pipeline = GetPipeline(description);
            PipelineDescCache.Add(name, pipeline);
            return pipeline;
        }

        Dictionary<string, ResourceLayout> ResourceLayoutDescCache = new Dictionary<string, ResourceLayout>();
        public ResourceLayout GetResourceLayout(string name, ResourceLayoutDescription description)
        {
            if (ResourceLayoutDescCache.TryGetValue(name, out var resourceLayout))
            {
                return resourceLayout;
            }
            var layout = GetResourceLayout(description);
            ResourceLayoutDescCache.Add(name, layout);
            return layout;
        }

        public bool TryGetPipeline(string name, [NotNullWhen(true)]out Pipeline? pipeline)
        {
            return PipelineDescCache.TryGetValue(name, out pipeline);
        }

        public bool TryGetResourceLayout(string name, [NotNullWhen(true)] out ResourceLayout? resourceLayout)
        {
            return ResourceLayoutDescCache.TryGetValue(name, out resourceLayout);
        }
    }
}

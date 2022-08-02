using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrickEngine.Core.Graphics
{
    public class ViewportRegion
    {
        public Rectangle Dimensions;
        /// <summary>
        /// If null then assume default framebuffer
        /// </summary>
        public Framebuffer? FrameBuffer;

        public ViewportRegion(Rectangle dimensions, Framebuffer? frameBuffer = null)
        {
            Dimensions = dimensions;
            FrameBuffer = frameBuffer;
        }
    }
}

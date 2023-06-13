using BrickEngine.Core.Utilities;
using BrickEngine.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BrickEngine.Core.Graphics
{
    public class TypedBufferList<T> where T : unmanaged
    {
        public DeviceBuffer DeviceBuffer { get; private set; }
        private readonly GraphicsContext graphicsContext;
        public readonly uint StructuredBufferMinOffsetAlignment;
        public readonly uint UniformBufferMinOffsetAlignment;
        public readonly uint StructSize;
        public readonly uint PaddedStructSize;
        public readonly uint StructByteStride;
        public readonly bool Raw;
        public readonly bool Resizable;
        public readonly BufferUsage Usage;
        public uint Capacity { get; private set; }
        public uint Count { get; private set; }
        public TypedBufferList(GraphicsContext graphicsContext, BufferUsage usage, bool resizable = false, bool raw = false, uint initialSize = 16)
        {
            this.graphicsContext = graphicsContext;
            Count = 0;
            Resizable = resizable;
            Raw = raw;
            Usage = usage;
            StructuredBufferMinOffsetAlignment = graphicsContext.GraphicsDevice.StructuredBufferMinOffsetAlignment;
            UniformBufferMinOffsetAlignment = graphicsContext.GraphicsDevice.UniformBufferMinOffsetAlignment;
            StructSize = (uint)Unsafe.SizeOf<T>();
            if (usage.HasFlag(BufferUsage.UniformBuffer))
            {
                StructByteStride = PaddedStructSize = (uint)MathF.Round((StructSize / (float)UniformBufferMinOffsetAlignment), MidpointRounding.AwayFromZero) * UniformBufferMinOffsetAlignment;
            }
            else if (usage.HasFlag(BufferUsage.StructuredBufferReadWrite) || usage.HasFlag(BufferUsage.StructuredBufferReadOnly))
            {
                StructByteStride = PaddedStructSize = (uint)MathF.Round((StructSize / (float)StructuredBufferMinOffsetAlignment), MidpointRounding.AwayFromZero) * StructuredBufferMinOffsetAlignment;
            }
            else
            {
                PaddedStructSize = StructSize;
                StructByteStride = 0;
            }
            if (StructSize != PaddedStructSize)
            {
                Debug.WriteLine("WARNING!: StructSize != PaddedStructSize");
            }
            Capacity = initialSize;
            BufferDescription description = new BufferDescription(PaddedStructSize * Capacity, usage, StructByteStride, raw);
            DeviceBuffer = graphicsContext.ResourceFactory.CreateBuffer(description);
        }

        private void ResizeBuffer(CommandList cl, uint cap)
        {
            BufferDescription description = new BufferDescription(PaddedStructSize * cap, Usage, StructByteStride, Raw);
            var oldBuffer = DeviceBuffer;
            DeviceBuffer = graphicsContext.ResourceFactory.CreateBuffer(description);
            Capacity = cap;
            if (oldBuffer != null)
            {
                cl.CopyBuffer(oldBuffer, 0, DeviceBuffer, 0, Math.Min(DeviceBuffer.SizeInBytes, oldBuffer.SizeInBytes));
                graphicsContext.DisposeWhenUnused(oldBuffer);
            }
        }

        public void ResizeCapacity(CommandList cl, uint cap)
        {
            if (Capacity != cap)
            {
                ResizeBuffer(cl, cap);
            }
        }

        public void EnsureCapacity(CommandList cl, uint minCap)
        {
            if (Capacity < minCap)
            {
                uint newCap = Capacity + unchecked(Capacity >> 1);
                ResizeBuffer(cl, Math.Max(newCap, minCap));
            }
        }

        public void Add(CommandList cl, T element)
        {
            if (!Resizable)
            {
                if (Count >= Capacity)
                {
                    throw new InvalidOperationException("The buffer is full and not Resizable");
                }
            }
            else
            {
                EnsureCapacity(cl, Count + 1);
            }
            cl.UpdateBuffer(DeviceBuffer, Count * PaddedStructSize, element);
            Count++;
        }

        public void Add(CommandList cl, ReadOnlySpan<T> elements)
        {
            if (!Resizable)
            {
                if (Count + elements.Length >= Capacity)
                {
                    throw new InvalidOperationException("The buffer is full and not Resizable");
                }
            }
            else
            {
                EnsureCapacity(cl, Count + (uint)elements.Length);
            }
            cl.UpdateBuffer(DeviceBuffer, Count * PaddedStructSize, elements);
            Count += (uint)elements.Length;
        }

        public void Update(CommandList cl, uint index, T element)
        {
            if (Resizable)
            {
                if (index >= Count)
                {
                    throw new IndexOutOfRangeException();
                }
            }
            else
            {
                if (index >= Capacity)
                {
                    throw new IndexOutOfRangeException();
                }
            }
            
            cl.UpdateBuffer(DeviceBuffer, index * PaddedStructSize, element);
        }

        public void Update(CommandList cl, uint index, ReadOnlySpan<T> elements)
        {
            if (Resizable)
            {
                if (index >= Count || index + elements.Length >= Count)
                {
                    throw new IndexOutOfRangeException();
                }
            }
            else
            {
                if (index >= Capacity || index + elements.Length >= Capacity)
                {
                    throw new IndexOutOfRangeException();
                }
            }
            
            cl.UpdateBuffer(DeviceBuffer, index * PaddedStructSize, elements);
        }

        public void Clear() => Count = 0;

        public void Fill(CommandList cl, T template)
        {
            uint steps = (Capacity / 32) - 1;
            uint remainder = Capacity % 32;
            Span<T> a = stackalloc T[32];
            a.Fill(template);

            cl.UpdateBuffer(DeviceBuffer, 0, a.Slice(0, Math.Min(32, (int)Capacity)));
            Count = Capacity;
            //This can be made faster by doubling the length every iteration
            for (uint i = 0; i < steps; i++)
            {
                cl.CopyBuffer(DeviceBuffer, i * 32 * PaddedStructSize, DeviceBuffer, (i + 1) * 32 * PaddedStructSize, PaddedStructSize * 32);
            }
            if (remainder != 0)
            {
                cl.UpdateBuffer(DeviceBuffer, DeviceBuffer.SizeInBytes - (remainder * PaddedStructSize), a.Slice(0, (int)remainder));
            }
        }
    }
}

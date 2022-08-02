using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrickEngine.Assets
{
    public class ByteBufferWriter : IBufferWriter<byte>, IDisposable
    {
        private byte[] _rentedBuffer;
        private int _written;

        private const int MinimumBufferSize = 256;

        public ByteBufferWriter(int initialCapacity = MinimumBufferSize)
        {
            if (initialCapacity <= 0)
                throw new ArgumentException(nameof(initialCapacity));

            _rentedBuffer = ArrayPool<byte>.Shared.Rent(initialCapacity);
            _written = 0;
        }

        public Memory<byte> WrittenMemory
        {
            get
            {
                CheckIfDisposed();
                return _rentedBuffer.AsMemory(0, _written);
            }
        }

        public Span<byte> WrittenSpan
        {
            get
            {
                CheckIfDisposed();
                return _rentedBuffer.AsSpan(0, _written);
            }
        }

        public int WrittenCount
        {
            get
            {
                CheckIfDisposed();
                return _written;
            }
        }

        public void Clear()
        {
            CheckIfDisposed();
            _rentedBuffer.AsSpan(0, _written).Clear();
            _written = 0;
        }

        [Obsolete("Prefer ReturnSpanAndAdvance() to Advance()")]
        public void Advance(int count)
        {
            CheckIfDisposed();

            if (count < 0)
                throw new ArgumentException(nameof(count));

            if (_written > _rentedBuffer.Length - count)
                throw new InvalidOperationException("Cannot advance past the end of the buffer.");

            _written += count;
        }

        // Returns the rented buffer back to the pool
        public void Dispose()
        {
            if (_rentedBuffer == null)
            {
                return;
            }

            ArrayPool<byte>.Shared.Return(_rentedBuffer, clearArray: true);
            _rentedBuffer = null!;
            _written = 0;
            GC.SuppressFinalize(this);
        }

        private void CheckIfDisposed()
        {
            if (_rentedBuffer == null)
                throw new ObjectDisposedException(nameof(ByteBufferWriter));
        }

        public Memory<byte> GetMemory(int sizeHint = 0)
        {
            CheckIfDisposed();

            if (sizeHint < 0)
                throw new ArgumentException(nameof(sizeHint));

            CheckAndResizeBuffer(sizeHint);
            return _rentedBuffer.AsMemory(_written);
        }

        public Span<byte> GetSpan(int sizeHint = 0)
        {
            CheckIfDisposed();

            if (sizeHint < 0)
                throw new ArgumentException(nameof(sizeHint));

            CheckAndResizeBuffer(sizeHint);
            return _rentedBuffer.AsSpan(_written);
        }

        public int GetWrittenLength(ref Span<byte> bytes)
        {
            CheckIfDisposed();
            int length = bytes.Length - _rentedBuffer.Length;
            return length;
        }

        public int ReturnSpanAndAdvance(ref Span<byte> bytes)
        {
            CheckIfDisposed();
            int length = GetWrittenLength(ref bytes);
#pragma warning disable CS0618 // Type or member is obsolete
            Advance(length);
#pragma warning restore CS0618 // Type or member is obsolete
            bytes = Span<byte>.Empty;
            return length;
        }

        private void CheckAndResizeBuffer(int sizeHint)
        {
            Debug.Assert(sizeHint >= 0);

            if (sizeHint == 0)
            {
                sizeHint = MinimumBufferSize;
            }

            int availableSpace = _rentedBuffer.Length - _written;

            if (sizeHint > availableSpace)
            {
                int growBy = sizeHint > _rentedBuffer.Length ? sizeHint : _rentedBuffer.Length;

                int newSize = checked(_rentedBuffer.Length + growBy);

                byte[] oldBuffer = _rentedBuffer;

                _rentedBuffer = ArrayPool<byte>.Shared.Rent(newSize);

                Debug.Assert(oldBuffer.Length >= _written);
                Debug.Assert(_rentedBuffer.Length >= _written);

                oldBuffer.AsSpan(0, _written).CopyTo(_rentedBuffer);
                ArrayPool<byte>.Shared.Return(oldBuffer, clearArray: true);
            }

            Debug.Assert(_rentedBuffer.Length - _written > 0);
            Debug.Assert(_rentedBuffer.Length - _written >= sizeHint);
        }
    }
}

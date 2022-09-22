using System.Buffers;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using K4os.Compression.LZ4;
using K4os.Compression.LZ4.Streams;
using System;

namespace BrickEngine.Assets
{
    public sealed class Asset
    {
        public readonly AssetHeader Header;
        public readonly byte[] Blob;

        public Asset(AssetHeader header, byte[] blob)
        {
            Header = header;
            Blob = blob;
        }

        internal Asset(AssetReadHeader header, byte[] blob)
        {
            Blob = blob;
            Header = new AssetHeader(header);
        }

        public Asset(Asset assetData)
        {
            Header = assetData.Header;
            Blob = new byte[assetData.Blob.Length];
            assetData.Blob.CopyTo(Blob, 0);
        }

        public static Asset Create(int version, int assetType, bool compress, byte[] uncompressedBlob)
        {
            int size;
            byte[] data;
            if (compress)
            {
                byte[] compressedBuffer = ArrayPool<byte>.Shared.Rent(LZ4Codec.MaximumOutputSize(uncompressedBlob.Length));
                size = LZ4Codec.Encode(uncompressedBlob, compressedBuffer, LZ4Level.L12_MAX);
                data = new byte[size];
                Array.Copy(compressedBuffer, 0, data, 0, size);
                ArrayPool<byte>.Shared.Return(compressedBuffer);
            }
            else
            {
                data = uncompressedBlob;
                size = uncompressedBlob.Length;
            }

            return new Asset(new AssetHeader(version, assetType, compress, uncompressedBlob.Length, size), data);
        }

        private static readonly LZ4EncoderSettings settings = new LZ4EncoderSettings() { CompressionLevel = LZ4Level.L12_MAX };

        public static Asset Create(int version, int assetType, bool compress, Stream uncompressedBlob)
        {
            byte[] data;
            if (compress)
            {
                using var stream = new MemoryStream();
                using (var target = LZ4Stream.Encode(stream, settings, true))
                {
                    uncompressedBlob.CopyTo(target);
                }
                data = stream.ToArray();
            }
            else
            {
                using var stream = new MemoryStream();
                uncompressedBlob.CopyTo(stream);
                data = stream.ToArray();
            }

            return new Asset(new AssetHeader(version, assetType, compress, (int)uncompressedBlob.Length, data.Length), data);
        }

        public static byte[] Compress(Stream uncompressedBlob)
        {
            using var stream = new MemoryStream();
            using (var target = LZ4Stream.Encode(stream, settings, true))
            {
                uncompressedBlob.CopyTo(target);
            }
            byte[] data = stream.ToArray();
            return data;
        }

        public static Asset Load(Stream stream)
        {
            if (!stream.CanRead)
            {
                throw new IOException("Cannot read from this stream");
            }
            var header = new AssetReadHeader();
            unsafe
            {
                var smallBuffer = new Span<byte>(Unsafe.AsPointer(ref header), Unsafe.SizeOf<AssetReadHeader>());
                int read = stream.Read(smallBuffer);
                if (header.Magic != AssetHeader.Magic)
                {
                    throw new IOException("Asset header corrupt. File may not be asset file.");
                }
                if (read < smallBuffer.Length)
                {
                    throw new IOException("Could not read Asset header. Early end of stream.");
                }
            }
            byte[] blobData = new byte[header.rawHeader.BlobBinarySize];
            if (stream.Read(blobData) != blobData.Length)
            {
                throw new IOException("Could not read Asset blob. Early end of stream.");
            }
            return new Asset(header, blobData);
        }

        public static async Task<Asset> LoadAsync(Stream stream)
        {
            if (!stream.CanRead)
            {
                throw new IOException("Cannot read from this stream");
            }
            var header = new AssetReadHeader();
            void ReadHeader()
            {
                unsafe
                {
                    var smallBuffer = new Span<byte>(Unsafe.AsPointer(ref header), Unsafe.SizeOf<AssetReadHeader>());
                    int read = stream.Read(smallBuffer);
                    if (header.Magic != AssetHeader.Magic)
                    {
                        throw new IOException("Asset header corrupt. File may not be asset file.");
                    }
                    if (read < smallBuffer.Length)
                    {
                        throw new IOException("Could not read Asset header. Early end of stream.");
                    }
                }
            }
            ReadHeader();
            byte[] blobData = new byte[header.rawHeader.BlobBinarySize];
            if (await stream.ReadAsync(blobData) != blobData.Length)
            {
                throw new IOException("Could not read Asset blob. Early end of stream.");
            }
            return new Asset(header, blobData);
        }

        public static void Save(Asset assetData, Stream stream)
        {
            AssetDataExtensions.Save(assetData, stream);
        }

        public static byte[] GetDecompressedBlob(Asset assetData)
        {
            return AssetDataExtensions.GetDecompressedBlob(assetData);
        }

        public static bool GetDecompressedBlob(Asset assetData, Span<byte> buffer)
        {
            return AssetDataExtensions.GetDecompressedBlob(assetData, buffer);
        }

        public static Task SaveAsync(Asset assetData, Stream stream)
        {
            return AssetDataExtensions.SaveAsync(assetData, stream);
        }

        public static Task SaveAsync(Asset assetData, Stream stream, CancellationToken token)
        {
            return AssetDataExtensions.SaveAsync(assetData, stream, token);
        }
    }

    public static class AssetDataExtensions
    {
        public static byte[] GetDecompressedBlob(this Asset assetData)
        {
            if (assetData.Header.IsCompressed)
            {
                byte[] blob = new byte[assetData.Header.RawBlobSize];
                int neededSize = LZ4Codec.Decode(assetData.Blob, blob);
                if (neededSize < 0 || neededSize != assetData.Header.RawBlobSize)
                {
                    throw new InvalidOperationException("Could not decompress asset Header.RawBlobBinarySize is the wrong size.");
                }
                return blob;
            }
            return assetData.Blob;
        }

        public static bool GetDecompressedBlob(this Asset assetData, Span<byte> buffer)
        {
            if (assetData.Header.IsCompressed)
            {
                if (buffer.Length < assetData.Header.RawBlobSize)
                {
                    return false;
                }
                int neededSize = LZ4Codec.Decode(assetData.Blob, buffer);
                if (neededSize < 0 || neededSize != assetData.Header.RawBlobSize)
                {
                    throw new InvalidOperationException("Could not decompress asset Header.RawBlobBinarySize is the wrong size.");
                }
                return true;
            }
            if (buffer.Length < assetData.Header.RawBlobSize)
            {
                return false;
            }
            assetData.Blob.CopyTo(buffer);
            return true;
        }

        //TODO: do this
        //public static Stream GetDecompressedBlob(this Asset assetData, Stream buffer)
        //{
        //    LZ4Stream.Decode(new MemoryStream(assetData.Blob, false)).;
        //    if (buffer.Length < assetData.Header.RawBlobSize)
        //    {
        //        return false;
        //    }
        //    int neededSize = LZ4Codec.Decode(assetData.Blob, buffer);

        //}

        public static void Save(this Asset assetData, Stream stream)
        {
            if (!stream.CanWrite)
            {
                throw new IOException("Cannot write to this stream");
            }
            var readHeader = new AssetReadHeader(assetData.Header);
            unsafe
            {
                var smallBuffer = MemoryMarshal.Cast<AssetReadHeader, byte>(MemoryMarshal.CreateSpan(ref readHeader, 1));
                stream.Write(smallBuffer);
            }
            stream.Write(assetData.Blob);
        }

        public static Task SaveAsync(this Asset assetData, Stream stream)
        {
            return SaveAsync(assetData, stream, CancellationToken.None);
        }

        public static async Task SaveAsync(this Asset assetData, Stream stream, CancellationToken token)
        {
            if (!stream.CanWrite)
            {
                throw new IOException("Cannot write to this stream");
            }

            int length = Unsafe.SizeOf<AssetReadHeader>();
            byte[] headerBuffer = ArrayPool<byte>.Shared.Rent(length);
            void WriteData()
            {
                unsafe
                {
                    var readHeader = new AssetReadHeader(assetData.Header);
                    var smallBuffer = MemoryMarshal.Cast<AssetReadHeader, byte>(MemoryMarshal.CreateSpan(ref readHeader, 1));
                    smallBuffer.CopyTo(headerBuffer);
                }
            }
            WriteData();
            await stream.WriteAsync(headerBuffer.AsMemory(0, length), token);
            await stream.WriteAsync(assetData.Blob, token);
        }
    }
}
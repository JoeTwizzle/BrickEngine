using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Buffers.Binary;
using System.Threading.Tasks;
using System.Diagnostics;

namespace BrickEngine.Assets
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct AssetHeader
    {
        public const uint Magic = 0xDECE75CD;
        public fixed byte RawGuid[16];
        public readonly int RawBlobSize;
        public readonly int BlobBinarySize;
        public readonly int Version;
        public readonly int AssetType;
        public readonly bool IsCompressed;

        public Guid Guid
        {
            get
            {
                return new Guid(MemoryMarshal.CreateReadOnlySpan(ref RawGuid[0], 16));
            }
        }

        public AssetHeader(int version, int assetType, bool isCompressed, int rawBlobSize, int blobBinarySize)
        {
            Span<byte> guidSpan = stackalloc byte[16];
            Debug.Assert(Guid.NewGuid().TryWriteBytes(guidSpan));
            guidSpan.CopyTo(MemoryMarshal.CreateSpan(ref RawGuid[0], 16));
            Version = version;
            RawBlobSize = rawBlobSize;
            BlobBinarySize = blobBinarySize;
            AssetType = assetType;
            IsCompressed = isCompressed;
        }

        internal AssetHeader(AssetReadHeader header)
        {
            var src = MemoryMarshal.CreateReadOnlySpan(ref header.rawHeader.RawGuid[0], 16);
            var dest = MemoryMarshal.CreateSpan(ref RawGuid[0], 16);
            src.CopyTo(dest);
            Version = header.rawHeader.Version;
            RawBlobSize = header.rawHeader.RawBlobSize;
            BlobBinarySize = header.rawHeader.BlobBinarySize;
            AssetType = header.rawHeader.AssetType;
            IsCompressed = header.rawHeader.IsCompressed;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct AssetReadHeader
    {
        public uint Magic;
        public AssetHeader rawHeader;

        internal AssetReadHeader(AssetHeader header)
        {
            Magic = BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(AssetHeader.Magic) : AssetHeader.Magic;
            rawHeader = header;
        }
    }
}

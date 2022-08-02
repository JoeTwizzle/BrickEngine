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
    public readonly struct AssetHeader
    {
        public const uint Magic = 0xDECE75CD;
        public readonly ulong GuidPart1;
        public readonly ulong GuidPart2;
        public readonly int RawBlobSize;
        public readonly int BlobBinarySize;
        public readonly uint Version;
        public readonly int AssetType;
        public readonly bool IsCompressed;

        public Guid Guid
        {
            get
            {
                return GuidExtensions.FromBytes(GuidPart1, GuidPart2);
            }
        }

        public AssetHeader(uint version, int assetType, bool isCompressed, int rawBlobSize, int blobBinarySize)
        {
            Guid.NewGuid().ToBytes(out GuidPart1, out GuidPart2);
            Version = version;
            RawBlobSize = rawBlobSize;
            BlobBinarySize = blobBinarySize;
            AssetType = assetType;
            IsCompressed = isCompressed;
        }

        internal AssetHeader(in AssetReadHeader header)
        {
            GuidPart1 = header.rawHeader.GuidPart1;
            GuidPart2 = header.rawHeader.GuidPart2;
            Version = header.rawHeader.Version;
            RawBlobSize = header.rawHeader.RawBlobSize;
            BlobBinarySize = header.rawHeader.BlobBinarySize;
            AssetType = header.rawHeader.AssetType;
            IsCompressed = header.rawHeader.IsCompressed;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal readonly struct AssetReadHeader
    {
        public readonly uint Magic;
        public readonly AssetHeader rawHeader;

        internal AssetReadHeader(in AssetHeader header)
        {
            Magic = AssetHeader.Magic;
            rawHeader = header;
        }
    }
}

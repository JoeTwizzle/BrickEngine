using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BinSerialize;

namespace BrickEngine.Assets.Data
{
    [MemoryPackable]
    public sealed partial class FileData
    {
        public readonly byte[] Data;

        public FileData(byte[] data)
        {
            Data = data;
        }
    }
}

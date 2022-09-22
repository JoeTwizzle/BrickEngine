using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrickEngine.Assets
{
    public struct AssetVersion
    {
        public int Version;
        public AssetVersion(int version)
        {
            Version = version;
        }
        public AssetVersion(byte major, byte minor, byte revision, byte patch = 0)
        {
            Version = 0;
            Version |= major << (8 * 3);
            Version |= minor << (8 * 2);
            Version |= revision << (8 * 1);
            Version |= patch << (8 * 0);
        }
        public static AssetVersion Create(byte major, byte minor, byte revision, byte patch = 0)
        {
            return new AssetVersion(major, minor, revision, patch);
        }
        public static implicit operator int(AssetVersion v) => v.Version;
        public static implicit operator uint(AssetVersion v) => (uint)v.Version;
    }
}

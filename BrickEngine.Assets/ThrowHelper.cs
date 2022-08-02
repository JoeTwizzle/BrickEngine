using BrickEngine.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrickEngine.Assets
{
    internal static class ThrowHelper
    {
        internal static void ThrowIfNotAssetOfType(Asset asset, Type type)
        {
            if (asset.Header.AssetType != type.GetHashCode())
            {
                throw new ArgumentException($"Asset was not an asset containing {type.Name}");
            }
        }
    }
}

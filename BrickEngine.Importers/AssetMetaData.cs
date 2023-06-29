using BrickEngine.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BinSerialize;

namespace BrickEngine.Importers
{
    public class AssetMetaData : IBinarySerializable<AssetMetaData>
    {
        /// <summary>
        /// The Guid of this AssetSource
        /// </summary>
        public readonly Guid Guid;
        /// <summary>
        /// The Path to this AssetSource
        /// </summary>
        public readonly string Path;
        /// <summary>
        /// The Children of this AssetSource
        /// </summary>
        public readonly Guid[] Children;

        public AssetMetaData(Guid guid, string path, Guid[] children)
        {
            Guid = guid;
            Path = path;
            Children = children;
        }

        public static AssetMetaData Deserialize(ref ReadOnlySpan<byte> span)
        {
            Guid guid = BinarySerializer.ReadGuid(ref span);
            string path = BinarySerializer.ReadString(ref span);
            int length = BinarySerializer.ReadPackedInt(ref span);
            Guid[] children = new Guid[length];
            for (int i = 0; i < length; i++)
            {
                children[i] = BinarySerializer.ReadGuid(ref span);
            }
            return new AssetMetaData(guid, path, children);
        }

        public static void Serialize(ByteBufferWriter writer, AssetMetaData data)
        {
            var size = BinarySerializer.GetSizeForString(data.Path) + 16 * (data.Children.Length + 1);
            var span = writer.GetSpan(size);
            span = span.Slice(16);
            BinarySerializer.WriteGuid(ref span, data.Guid);
            BinarySerializer.WriteString(ref span, data.Path);
            BinarySerializer.WritePackedInt(ref span, data.Children.Length);
            for (int i = 0; i < data.Children.Length; i++)
            {
                BinarySerializer.WriteGuid(ref span, data.Children[i]);
            }
            writer.ReturnSpanAndAdvance(ref span);
        }
    }
}

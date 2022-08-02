using BrickEngine.Assets;
using BrickEngine.Assets.Data;
using K4os.Compression.LZ4.Streams;

namespace BrickEngine.Importers
{
    public static class FileImporter
    {
        public static Asset Import(string path)
        {
            using var s = File.OpenRead(path);
            var data = new FileData(Asset.Compress(s));
            using ByteBufferWriter writer = new ByteBufferWriter();
            FileData.Serialize(writer, data);
            return Asset.Create(AssetVersion.Create(1, 0, 0), typeof(FileData).GetHashCode(), true, writer.WrittenSpan.ToArray());
        }
    }
}

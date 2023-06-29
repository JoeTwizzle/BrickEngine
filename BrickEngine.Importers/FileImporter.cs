using BrickEngine.Assets;
using BrickEngine.Assets.Data;
using K4os.Compression.LZ4.Streams;

namespace BrickEngine.Importers
{
    public static class FileImporter
    {
        public static FileData Import(string path)
        {
            using var s = File.OpenRead(path);
            return new FileData(Asset.Compress(s));
        }
    }
}

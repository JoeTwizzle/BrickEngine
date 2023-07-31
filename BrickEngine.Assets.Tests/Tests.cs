using BrickEngine.Importers;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrickEngine.Assets.Tests
{

    static class Tests
    {
        [Test]
        public static void TestTexture()
        {
            var texture = TextureImporter.Import("Files/plains.png", new TextureImportSettings(TextureType.RG, TextureCompressionMode.High, true, true));
            TextureData data = TextureData.Deserialize(texture.GetDecompressedBlob());
            using ByteBufferWriter writer = new ByteBufferWriter();
            TextureData.Serialize(writer, data);
            var asset1 = Asset.Create(1, 1, true, writer.WrittenSpan.ToArray());
            var asset2 = Asset.Create(1, 1, false, writer.WrittenSpan.ToArray());
            using var fs1 = new FileStream("TextureCompCB.bin", FileMode.Create);
            using var fs2 = new FileStream("TextureCB.bin", FileMode.Create);
            asset1.Save(fs1);
            asset2.Save(fs2);
            var newData = TextureData.Deserialize(writer.WrittenSpan);
            Assert.AreEqual(newData.Miplevels, newData.Miplevels);
        }
    }
}

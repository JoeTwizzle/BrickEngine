using System;
using System.Collections.Generic;
using System.Linq;
using Veldrid;
using StbImageSharp;
using BCnEncoder.Shared;
using BCnEncoder.Encoder;
using BCnEncoder.Encoder.Options;
using Microsoft.Toolkit.HighPerformance;
using BrickEngine.Assets;
using BrickEngine.Assets.Data;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace BrickEngine.Importers
{
    public enum TextureType : uint
    {
        R,
        RG,
        RGB,
        RGBA
    }
    public struct TextureImportSettings
    {
        public static TextureImportSettings Default => new TextureImportSettings(TextureType.RGBA, true, true, true);
        public TextureType TextureType;
        public bool Compress;
        public bool GenerateMipmaps;
        public bool IsSrgb;

        public TextureImportSettings(TextureType textureType, bool compress, bool generateMipmaps, bool isSrgb)
        {
            IsSrgb = isSrgb;
            TextureType = textureType;
            Compress = compress;
            GenerateMipmaps = generateMipmaps;
        }
    }


    public class TextureImporter
    {
        public static Asset Import(string path)
        {
            using var fs = new FileStream(path, FileMode.Open);
            return Import(fs, TextureImportSettings.Default);
        }

        public static Asset Import(string path, TextureImportSettings settings)
        {
            using var fs = new FileStream(path, FileMode.Open);
            return Import(fs, settings);
        }

        public static Asset Import(Stream stream, TextureImportSettings settings)
        {
            var info = ImageInfo.FromStream(stream);
            if (!info.HasValue)
            {
                throw new IOException("Tried to import texture that was not a valid image file");
            }
            MipLevel[] mipLevels;
            bool isHdr = info.Value.BitsPerChannel > 8;
            uint vdPixelFormat = (uint)GetVdPixelFormat(isHdr, settings);
            if (settings.Compress)
            {
                var encoder = new BcEncoder();
                encoder.OutputOptions.MaxMipMapLevel = settings.GenerateMipmaps ? -1 : 1;
                encoder.OutputOptions.Quality = CompressionQuality.BestQuality;
                CompressionFormat compressFormat;
                if (isHdr)
                {
                    compressFormat = settings.TextureType switch
                    {
                        TextureType.RGB => CompressionFormat.Bc6U,
                        _ => throw new NotSupportedException($"The format {nameof(settings.TextureType)} is not supported for compressed Hdr textures."),
                    };
                    encoder.OutputOptions.Format = compressFormat;
                    var res = ImageResultFloat.FromStream(stream);

                    //Dangerous and unsafe, Array.Length is no longer correct
                    var aata = Unsafe.As<ColorRgbFloat[]>(res.Data);
                    byte[][] data = encoder.EncodeToRawBytesHdr(new ReadOnlyMemory2D<ColorRgbFloat>(aata, res.Height, res.Width));
                    mipLevels = new MipLevel[data.Length];
                    for (int i = 0; i < mipLevels.Length; i++)
                    {
                        encoder.CalculateMipMapSize(res.Width, res.Height, i, out var mipWidth, out var mipHeight);
                        mipLevels[i] = new MipLevel(mipWidth, mipHeight, data[i]);
                    }
                }
                else // not hdr
                {
                    compressFormat = settings.TextureType switch
                    {
                        TextureType.R => CompressionFormat.Bc4,
                        TextureType.RG => CompressionFormat.Bc5,
                        TextureType.RGB => CompressionFormat.Bc7,
                        TextureType.RGBA => CompressionFormat.Bc7,
                        _ => throw new NotSupportedException($"Unkown TextureType {settings.TextureType}."),
                    };
                    encoder.OutputOptions.Format = compressFormat;
                    ColorComponents comp = settings.TextureType switch
                    {
                        TextureType.R => ColorComponents.RedGreenBlue,//only rgb or rgba supported for compression input
                        TextureType.RG => ColorComponents.RedGreenBlue,//only rgb or rgba supported for compression input
                        TextureType.RGB => ColorComponents.RedGreenBlue,
                        TextureType.RGBA => ColorComponents.RedGreenBlueAlpha,
                        _ => throw new NotSupportedException($"Unkown TextureType {settings.TextureType}."),
                    };
                    BCnEncoder.Encoder.PixelFormat pixFmt = settings.TextureType switch
                    {
                        TextureType.R => BCnEncoder.Encoder.PixelFormat.Rgb24,//only rgb or rgba supported for compression input
                        TextureType.RG => BCnEncoder.Encoder.PixelFormat.Rgb24,//only rgb or rgba supported for compression input
                        TextureType.RGB => BCnEncoder.Encoder.PixelFormat.Rgb24,
                        TextureType.RGBA => BCnEncoder.Encoder.PixelFormat.Rgba32,
                        _ => throw new NotSupportedException($"Unkown TextureType {settings.TextureType}."),
                    };
                    var res = ImageResult.FromStream(stream, comp);
                    byte[][] data = encoder.EncodeToRawBytes(res.Data, res.Width, res.Height, pixFmt);
                    mipLevels = new MipLevel[data.Length];
                    for (int i = 0; i < mipLevels.Length; i++)
                    {
                        encoder.CalculateMipMapSize(res.Width, res.Height, i, out var mipWidth, out var mipHeight);
                        mipLevels[i] = new MipLevel(mipWidth, mipHeight, data[i]);
                    }
                }
            }
            else //not compressed
            {
                if (isHdr)
                {
                    ColorComponents comp = settings.TextureType switch
                    {
                        TextureType.R => ColorComponents.Grey,
                        TextureType.RG => ColorComponents.GreyAlpha,
                        TextureType.RGB => throw new NotSupportedException($"The format {nameof(settings.TextureType)} is not supported for uncompressed textures."),
                        TextureType.RGBA => ColorComponents.RedGreenBlueAlpha,
                        _ => throw new NotSupportedException($"Unkown TextureType {settings.TextureType}."),
                    };
                    var resf = ImageResultFloat.FromStream(stream, comp);
                    System.Half[] pixels = new System.Half[resf.Data.Length];
                    for (int i = 0; i < pixels.Length; i++)
                    {
                        pixels[i] = (System.Half)resf.Data[i];
                    }
                    byte componentCount = (byte)(((int)settings.TextureType) + 1);
                    mipLevels = MipMapper.GenerateMipChain(resf.Width, resf.Height, componentCount, pixels, settings.GenerateMipmaps ? -1 : 1);
                }
                else
                {
                    ColorComponents comp = settings.TextureType switch
                    {
                        TextureType.R => ColorComponents.Grey,
                        TextureType.RG => ColorComponents.GreyAlpha,
                        TextureType.RGB => throw new NotSupportedException($"The format {nameof(settings.TextureType)} is not supported for uncompressed textures."),
                        TextureType.RGBA => ColorComponents.RedGreenBlueAlpha,
                        _ => throw new NotSupportedException($"Unkown TextureType {settings.TextureType}."),
                    };
                    byte componentCount = (byte)(((int)settings.TextureType) + 1);
                    var res = ImageResult.FromStream(stream, comp);
                    mipLevels = MipMapper.GenerateMipChain(res.Width, res.Height, componentCount, res.Data, settings.GenerateMipmaps ? -1 : 1);
                }
            }
            using ByteBufferWriter writer = new ByteBufferWriter();
            TextureData.Serialize(writer, new TextureData(vdPixelFormat, mipLevels));
            return Asset.Create(AssetVersion.Create(1, 0, 0), typeof(TextureData).GetHashCode(), !settings.Compress, writer.WrittenSpan.ToArray());
        }


        static Veldrid.PixelFormat GetVdPixelFormat(bool isHdr, TextureImportSettings settings)
        {
            Veldrid.PixelFormat format = Veldrid.PixelFormat.R8_G8_B8_A8_UNorm;
            if (settings.Compress)
            {
                if (isHdr)
                {
                    format = settings.TextureType switch
                    {
                        TextureType.RGB => Veldrid.PixelFormat.BC6_UFloat,
                        _ => throw new NotSupportedException($"The format {nameof(settings.TextureType)} is not supported for compressed Hdr textures."),
                    };
                }
                else //not hdr
                {
                    switch (settings.TextureType)
                    {
                        case TextureType.R:
                            format = Veldrid.PixelFormat.BC4_UNorm;
                            break;
                        case TextureType.RG:
                            format = Veldrid.PixelFormat.BC5_UNorm;
                            break;
                        case TextureType.RGB:
                            if (settings.IsSrgb)
                            {
                                format = Veldrid.PixelFormat.BC7_UNorm_SRgb;
                            }
                            else
                            {
                                format = Veldrid.PixelFormat.BC7_UNorm;
                            }
                            break;
                        case TextureType.RGBA:
                            if (settings.IsSrgb)
                            {
                                format = Veldrid.PixelFormat.BC7_UNorm_SRgb;
                            }
                            else
                            {
                                format = Veldrid.PixelFormat.BC7_UNorm;
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
            else //Not compressed
            {
                if (isHdr)
                {
                    format = settings.TextureType switch
                    {
                        TextureType.R => Veldrid.PixelFormat.R16_Float,
                        TextureType.RG => Veldrid.PixelFormat.R16_G16_Float,
                        TextureType.RGB => throw new NotSupportedException($"The format {nameof(settings.TextureType)} is not supported for uncompressed textures."),
                        TextureType.RGBA => Veldrid.PixelFormat.R16_G16_B16_A16_Float,
                        _ => throw new NotSupportedException($"Unkown TextureType {settings.TextureType}."),
                    };
                }
                else
                {
                    switch (settings.TextureType)
                    {
                        case TextureType.R:
                            if (settings.IsSrgb)
                            {
                                throw new NotSupportedException($"The srgb format {nameof(settings.TextureType)} is not supported for uncompressed textures.");
                            }
                            else
                            {
                                format = Veldrid.PixelFormat.R8_UNorm;
                            }
                            break;
                        case TextureType.RG:
                            if (settings.IsSrgb)
                            {
                                throw new NotSupportedException($"The srgb format {nameof(settings.TextureType)} is not supported for uncompressed textures.");
                            }
                            else
                            {
                                format = Veldrid.PixelFormat.R8_G8_UNorm;
                            }

                            break;
                        case TextureType.RGB:
                            throw new NotSupportedException($"The format {nameof(settings.TextureType)} is not supported for uncompressed textures.");
                        case TextureType.RGBA:
                            if (settings.IsSrgb)
                            {
                                format = Veldrid.PixelFormat.R8_G8_B8_A8_UNorm_SRgb;
                            }
                            else
                            {
                                format = Veldrid.PixelFormat.R8_G8_B8_A8_UNorm;
                            }
                            break;
                        default:
                            throw new NotSupportedException($"Unkown TextureType {settings.TextureType}.");
                    }
                }
            }
            return format;
        }
    }

    static class MipMapper
    {
        public static int CalculateMipChainLength(int width, int height, int maxNumMipMaps)
        {
            if (maxNumMipMaps == 1)
            {
                return 1;
            }

            if (maxNumMipMaps <= 0)
            {
                maxNumMipMaps = int.MaxValue;
            }

            var output = 0;
            for (var mipLevel = 1; mipLevel <= maxNumMipMaps; mipLevel++)
            {
                var mipWidth = Math.Max(1, width >> mipLevel);
                var mipHeight = Math.Max(1, height >> mipLevel);

                if (mipLevel == maxNumMipMaps)
                {
                    return maxNumMipMaps;
                }

                if (mipWidth == 1 && mipHeight == 1)
                {
                    output = mipLevel + 1;
                    break;
                }
            }

            return output;
        }

        public static void CalculateMipLevelSize(int width, int height, int mipIdx, out int mipWidth, out int mipHeight)
        {
            mipWidth = Math.Max(1, width >> mipIdx);
            mipHeight = Math.Max(1, height >> mipIdx);
        }

        public static MipLevel[] GenerateMipChain(int width, int height, byte channelCount, System.Half[] pixels, int numMipMaps)
        {
            var mipChainLength = CalculateMipChainLength(width, height, numMipMaps);

            System.Half[][] layers = new System.Half[mipChainLength][];
            var result = new MipLevel[mipChainLength];
            layers[0] = pixels;
            byte[] pixelData = new byte[width * height * channelCount * 2];
            for (int i = 0; i < pixels.Length; i++)
            {
                BitConverter.TryWriteBytes(pixelData.AsSpan(i * 2), pixels[i]);
            }
            result[0] = new MipLevel(width, height, pixelData);
            // If only one mipmap was requested, return original image only
            if (numMipMaps == 1)
            {
                return result;
            }

            // If number of mipmaps is "marked as boundless", do as many mipmaps as it takes to reach a size of 1x1
            if (numMipMaps <= 0)
            {
                numMipMaps = int.MaxValue;
            }

            // Generate mipmaps
            for (var mipLevel = 1; mipLevel < numMipMaps; mipLevel++)
            {
                CalculateMipLevelSize(width, height, mipLevel, out var newWidth, out var newHeight);
                CalculateMipLevelSize(width, height, mipLevel - 1, out var w, out var h);
                var res = ResizeToHalfFloat(layers[mipLevel - 1], w, h, channelCount);
                byte[] mipData = new byte[newHeight * newWidth * channelCount * 2];
                for (int i = 0; i < res.Length; i++)
                {
                    BitConverter.TryWriteBytes(mipData.AsSpan(i * 2), res[i]);
                }
                layers[mipLevel] = res;
                result[mipLevel] = new MipLevel(newWidth, newHeight, mipData);
                // Stop generating if last generated mipmap was of size 1x1
                if (newWidth == 1 && newHeight == 1)
                {
                    numMipMaps = mipLevel + 1;
                    break;
                }
            }

            return result;
        }

        private static System.Half[] ResizeToHalfFloat(System.Half[] pixels, int oldWidth, int oldHeight, byte channelCount)
        {
            var newWidth = Math.Max(1, oldWidth >> 1);
            var newHeight = Math.Max(1, oldHeight >> 1);

            var result = new System.Half[newHeight * newWidth * channelCount];

            int ClampW(int x) => Math.Max(0, Math.Min(oldWidth - 1, x));
            int ClampH(int y) => Math.Max(0, Math.Min(oldHeight - 1, y));

            for (var y2 = 0; y2 < newHeight; y2++)
            {
                for (var x2 = 0; x2 < newWidth; x2++)
                {
                    for (int i = 0; i < channelCount; i++)
                    {
                        var ul = (float)pixels[ClampH(y2 * 2) * newWidth + ClampW(x2 * 2) + i];
                        var ur = (float)pixels[ClampH(y2 * 2) * newWidth + ClampW(x2 * 2 + 1) + i];
                        var ll = (float)pixels[ClampH(y2 * 2 + 1) * newWidth + ClampW(x2 * 2) + i];
                        var lr = (float)pixels[ClampH(y2 * 2 + 1) * newWidth + ClampW(x2 * 2 + 1) + i];
                        result[y2 * newWidth + x2 + i] = (System.Half)((ul + ur + ll + lr) / 4f);
                    }
                }
            }
            return result;
        }

        public static MipLevel[] GenerateMipChain(int width, int height, byte channelCount, byte[] pixels, int numMipMaps)
        {
            var mipChainLength = CalculateMipChainLength(width, height, numMipMaps);

            var result = new MipLevel[mipChainLength];
            result[0] = new MipLevel(width, height, pixels);

            // If only one mipmap was requested, return original image only
            if (numMipMaps == 1)
            {
                return result;
            }

            // If number of mipmaps is "marked as boundless", do as many mipmaps as it takes to reach a size of 1x1
            if (numMipMaps <= 0)
            {
                numMipMaps = int.MaxValue;
            }

            // Generate mipmaps
            for (var mipLevel = 1; mipLevel < numMipMaps; mipLevel++)
            {
                CalculateMipLevelSize(width, height, mipLevel, out var mipWidth, out var mipHeight);
                CalculateMipLevelSize(width, height, mipLevel - 1, out var w, out var h);
                result[mipLevel] = ResizeToHalf(result[mipLevel - 1], w, h, channelCount);

                // Stop generating if last generated mipmap was of size 1x1
                if (mipWidth == 1 && mipHeight == 1)
                {
                    numMipMaps = mipLevel + 1;
                    break;
                }
            }

            return result;
        }

        private static MipLevel ResizeToHalf(MipLevel pixels, int oldWidth, int oldHeight, byte channelCount)
        {
            var newWidth = Math.Max(1, oldWidth >> 1);
            var newHeight = Math.Max(1, oldHeight >> 1);

            var result = new byte[newHeight * newWidth * channelCount];

            int ClampW(int x) => Math.Max(0, Math.Min(oldWidth - 1, x));
            int ClampH(int y) => Math.Max(0, Math.Min(oldHeight - 1, y));

            for (var y2 = 0; y2 < newHeight; y2++)
            {
                for (var x2 = 0; x2 < newWidth; x2++)
                {
                    for (int i = 0; i < channelCount; i++)
                    {
                        var ul = (float)pixels.Data[ClampH(y2 * 2) * newWidth + ClampW(x2 * 2) + i];
                        var ur = (float)pixels.Data[ClampH(y2 * 2) * newWidth + ClampW(x2 * 2 + 1) + i];
                        var ll = (float)pixels.Data[ClampH(y2 * 2 + 1) * newWidth + ClampW(x2 * 2) + i];
                        var lr = (float)pixels.Data[ClampH(y2 * 2 + 1) * newWidth + ClampW(x2 * 2 + 1) + i];
                        result[y2 * newWidth + x2 + i] = (byte)((ul + ur + ll + lr) / 4f);
                    }
                }
            }
            return new MipLevel(newWidth, newHeight, result);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using InteropTypes.Numerics;

using SkiaSharp;

namespace InteropTypes.TensorBitmaps
{
    public static partial class SkiaSharpForTensorBitmapsExtensions
    {
        public static PixelFormat ToPixelFormat(this (SKColorType color, SKAlphaType alpha) skiaFormat)
        {
            switch (skiaFormat.color)
            {
                case SKColorType.Alpha8: return KnownPixelFormats.Alpha8;
                case SKColorType.Alpha16: return KnownPixelFormats.Alpha16;
                case SKColorType.AlphaF16: return KnownPixelFormats.AlphaF16;

                case SKColorType.Gray8: return KnownPixelFormats.Luminance8;

                case SKColorType.Rgb888x: return KnownPixelFormats.Rgbx8;
                case SKColorType.Rgba8888 when skiaFormat.alpha == SKAlphaType.Premul: return KnownPixelFormats.RgbPremul8;
                case SKColorType.Rgba8888: return KnownPixelFormats.Rgba8;

                case SKColorType.Rgba16161616: return KnownPixelFormats.Rgba16;

                case SKColorType.Bgra8888 when skiaFormat.alpha == SKAlphaType.Premul: return KnownPixelFormats.BgrPremul8;
                case SKColorType.Bgra8888: return KnownPixelFormats.Bgra8;                

                case SKColorType.RgbaF16 when skiaFormat.alpha == SKAlphaType.Premul: return KnownPixelFormats.RgbPremulF16;
                case SKColorType.RgbaF16: return KnownPixelFormats.RgbaF16;

                case SKColorType.RgbaF16Clamped when skiaFormat.alpha == SKAlphaType.Premul: return KnownPixelFormats.RgbPremulF16;
                case SKColorType.RgbaF16Clamped: return KnownPixelFormats.RgbaF16;

                case SKColorType.RgbaF32 when skiaFormat.alpha == SKAlphaType.Premul: return KnownPixelFormats.RgbPremulF32;
                case SKColorType.RgbaF32: return KnownPixelFormats.RgbaF32;

                case SKColorType.Rg88: return KnownPixelFormats.Rg8;
                case SKColorType.Rg1616: return KnownPixelFormats.Rg16;
                case SKColorType.RgF16: return KnownPixelFormats.RgF16;

                // add more pixel types here

                default: throw new NotImplementedException($"{skiaFormat.color} {skiaFormat.alpha}");
            }
        }
    }
}

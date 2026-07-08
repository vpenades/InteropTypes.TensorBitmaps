using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using TUnit;

namespace InteropTypes.TensorBitmaps
{
    internal class SkiaSharpConversionTests
    {
        [Test]
        public async Task TestPixelConversions()
        {
            if (OperatingSystem.IsLinux()) return; // skiasharp is failing on linux

            using var img = SkiaSharp.SKBitmap.Decode(ResourceInfo.From("shannon.jpg"));

            var tbmp = img
                .ToTensorBitmap<byte, Pixel888>(KnownPixelFormats.Rgb888)
                .AsReadOnlyTensorSpanBitmap()
                .GetCropped(new System.Drawing.Rectangle(200,100,280,280)); // crop Shannon's face.

            ConvertAndSave<byte, int>(tbmp, KnownPixelFormats.Rgba8888);

            ConvertAndSave<byte, int>(tbmp, KnownPixelFormats.Bgra8888);

            ConvertAndSave<byte, Pixel888>(tbmp, KnownPixelFormats.Bgr888);

            ConvertAndSave<float, Vector4>(tbmp, KnownPixelFormats.RgbaF32);

            ConvertAndSave<float, Vector3>(tbmp, KnownPixelFormats.RgbF32);

            ConvertAndSave<ushort, int>(tbmp, KnownPixelFormats.Rg1616);
        }

        private static void ConvertAndSave<TElement, TPixel>(ReadOnlyTensorSpanBitmap<byte, Pixel888> src, TensorPixelFormat fmt)
            where TElement: unmanaged, INumber<TElement>
            where TPixel: unmanaged
        {            
            var dst = TensorBitmap<TElement, TPixel>.Create(256, 256, fmt);

            // copies the pixels from src to dst, taking into account the pixel layout and each component range.
            src.CopyPixelsTo(dst);

            using var result = dst.ToSkiaSharp();            

            AttachmentInfo
                .From($"shannon.{typeof(TPixel).Name}.jpg")
                .WriteToStream(s=>result.Encode(s, SkiaSharp.SKEncodedImageFormat.Jpeg, 75));
        }
    }
}

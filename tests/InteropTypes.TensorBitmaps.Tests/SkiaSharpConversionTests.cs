using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using InteropTypes.Numerics;

using TUnit;

namespace InteropTypes.TensorBitmaps
{
    internal class SkiaSharpConversionTests
    {
        [Test]
        public async Task TestPixelConversions()
        {
            if (OperatingSystem.IsLinux()) return; // skiasharp is failing me on linux            

            var tbmp = ResourceInfo.From("shannon.jpg")
                .File
                .LoadTensorBitmapWithSkiaSharp<byte, Pixel888>(KnownPixelFormats.Rgb8)
                .AsReadOnlyTensorSpanBitmap()
                .GetCropped(new System.Drawing.Rectangle(200,100,280,280)); // crop Shannon's face.

            ConvertAndSave<byte, int>(tbmp, KnownPixelFormats.Rgba8);
            ConvertAndSave<byte, int>(tbmp, KnownPixelFormats.Bgra8);
            ConvertAndSave<byte, Pixel888>(tbmp, KnownPixelFormats.Bgr8);
            ConvertAndSave<float, Vector4>(tbmp, KnownPixelFormats.RgbaF32);
            ConvertAndSave<float, Vector3>(tbmp, KnownPixelFormats.RgbF32);
            ConvertAndSave<ushort, int>(tbmp, KnownPixelFormats.Rg16); // blue channel will be missing in converted image
        }

        private static void ConvertAndSave<TElement, TPixel>(ReadOnlyTensorSpanBitmap<byte, Pixel888> src, PixelFormat fmt)
            where TElement: unmanaged, INumber<TElement>
            where TPixel: unmanaged
        {            
            var dst = TensorBitmap<TElement, TPixel>.Create(256, 256, fmt);

            // copies the pixels from src to dst, taking into account the pixel layout and each component range.
            src.CopyPixelsTo(dst);                     

            AttachmentInfo
                .From($"shannon.{typeof(TPixel).Name}.jpg")
                .WriteToStream(s=> s.WriteTensorBitmapWithSkiaSharp(dst.AsReadOnlyTensorSpanBitmap(), SkiaSharp.SKEncodedImageFormat.Jpeg, 75));
        }
    }
}

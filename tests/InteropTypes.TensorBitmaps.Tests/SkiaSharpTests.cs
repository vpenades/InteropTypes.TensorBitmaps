using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using InteropTypes.Numerics;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

using TUnit;

namespace InteropTypes.TensorBitmaps
{
    internal class SkiaSharpTests
    {
        [Test]
        public async Task TestPixelConversions()
        {
            if (OperatingSystem.IsLinux()) return; // skiasharp is failing me on linux            

            using var img = SkiaSharpBitmapOperand<uint>.Load(ResourceInfo.From("shannon.jpg"));
            using var tbmp = img.GetCropped(new System.Drawing.Rectangle(200,100,280,280)); // crop Shannon's face.

            ConvertAndSave<byte, int>(tbmp, KnownPixelFormats.Rgba8);
            ConvertAndSave<byte, int>(tbmp, KnownPixelFormats.Bgra8);
            ConvertAndSave<byte, Pixel888>(tbmp, KnownPixelFormats.Bgr8);
            ConvertAndSave<float, Vector4>(tbmp, KnownPixelFormats.RgbaF32);
            ConvertAndSave<float, Vector3>(tbmp, KnownPixelFormats.RgbF32);
            ConvertAndSave<ushort, int>(tbmp, KnownPixelFormats.Rg16); // blue channel will be missing in converted image
        }

        private static void ConvertAndSave<TElement, TPixel>(SkiaSharpBitmapOperand<uint> src, PixelFormat fmt)
            where TElement: unmanaged, INumber<TElement>
            where TPixel: unmanaged
        {            
            var dst = TensorBitmap<TElement, TPixel>.Create(256, 256, fmt);

            // copies the pixels from src to dst, taking into account the pixel layout and each component range.
            dst.GetContext<uint>().Fill(BitmapOperations.Copy, src);

            using var skiabmp = SkiaSharpBitmapOperand<uint>.Create<TensorBitmap<TElement, TPixel>, TPixel>(dst);

            AttachmentInfo
                .From($"shannon.{typeof(TPixel).Name}.jpg")
                .WriteToStream(s=> skiabmp.Write(s));
        }

        [Test]
        public async Task TestAsBitmapOperand()
        {
            if (OperatingSystem.IsLinux()) return; // skiasharp is failing me on linux

            using var bmp = SkiaSharpBitmapOperand<uint>.Load(ResourceInfo.From("shannon.jpg"));

            using var stretched = bmp.CreateStretched(64, 48);

            AttachmentInfo.From("shannon.stretched.jpg").WriteToStream(s=> stretched.Write(s) );
        }

        [Test]
        [Arguments(48, 256)]
        [Arguments(256, 48)]
        public async Task BitmapPreserveAspectFitTests(int w, int h)
        {
            if (OperatingSystem.IsLinux()) return; // skiasharp is failing me on linux

            using var img = SkiaSharpBitmapOperand<uint>.Read(ResourceInfo.From("shannon.jpg").OpenRead);

            for (int oa = 0; oa <= 10; oa++)
            {
                var bmp = TensorBitmap<byte, Rgb24>.Create(w, h, KnownPixelFormats.Rgb8);

                bmp.GetContext<uint>().Fill(BitmapOperations.ScaleToFit(oa / 10f), img);

                using var img2 = bmp.Cast<Rgb24>().ToImageSharp();

                AttachmentInfo.From($"shannon.{oa}.jpg").WriteObjectEx( f=> img2.SaveAsJpeg(f.FullName));
            }
        }
    }
}

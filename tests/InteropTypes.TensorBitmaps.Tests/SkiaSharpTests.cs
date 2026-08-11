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
            using var tbmp = img.CreateCropped(new System.Drawing.Rectangle(200,100,280,280)); // crop Shannon's face.

            await ConvertAndSave<byte, int>(tbmp, KnownPixelFormats.Rgba8, 3005288302);
            await ConvertAndSave<byte, int>(tbmp, KnownPixelFormats.Bgra8, 1827876581);
            await ConvertAndSave<byte, Pixel888>(tbmp, KnownPixelFormats.Bgr8, 1915969248);
            await ConvertAndSave<float, Vector4>(tbmp, KnownPixelFormats.RgbaF32, 2862329585);
            await ConvertAndSave<float, Vector3>(tbmp, KnownPixelFormats.RgbF32, 3535056545);
            await ConvertAndSave<ushort, int>(tbmp, KnownPixelFormats.Rg16, 1100000087); // blue channel will be missing in converted image
        }

        private static async Task ConvertAndSave<TElement, TPixel>(SkiaSharpBitmapOperand<uint> src, PixelFormat fmt, uint crc32)
            where TElement: unmanaged, INumber<TElement>
            where TPixel: unmanaged
        {            
            var dst = TensorBitmap<TElement, TPixel>.Create(256, 256, fmt);

            // copies the pixels from src to dst, taking into account the pixel layout and each component range.
            dst.GetContext<uint>().Apply(FillOperations.Copy, src);

            await Assert.That(dst.CalculateCrc32()).IsEqualTo(crc32);

            using var skiabmp = SkiaSharpBitmapOperand<uint>.Create<ReadOnlyTensorSpanBitmap<TElement, TPixel>, TPixel>(dst.AsReadOnlyTensorSpanBitmap());            

            AttachmentInfo
                .From($"shannon.{typeof(TPixel).Name}.jpg")
                .WriteToStream(s=> skiabmp.Write(s));
        }

        [Test]
        public async Task TestAsBitmapOperand()
        {
            if (OperatingSystem.IsLinux()) return; // skiasharp is failing me on linux

            using var bmp = SkiaSharpBitmapOperand<uint>.Load(ResourceInfo.From("shannon.jpg"));

            using var stretched = bmp.CreateStretched(new System.Drawing.Size(64, 48));            

            await Assert.That(new[] { 2859208297u , 145639874u}).Contains(stretched.CalculateCrc32());

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

                bmp.GetContext<uint>().Apply(FillOperations.ScaleToFit(oa / 10f), img);

                using var img2 = bmp.Cast<Rgb24>().ToImageSharp();

                AttachmentInfo.From($"shannon.{oa}.jpg").WriteObjectEx( f=> img2.SaveAsJpeg(f.FullName));
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using InteropTypes.Numerics;
using InteropTypes.TensorBitmaps.Operators;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

using TUnit;

namespace InteropTypes.TensorBitmaps
{
    internal class PixelTransformTests
    {
        [Test]
        public async Task BitmapFitTest()
        {
            // load image and convert it to a tensor bitmap

            using var img = ImageSharpBitmapOperand<Rgb24>.Read(ResourceInfo.From("shannon.jpg").OpenRead);            

            // create another tensor bitmap and fill it with the image we've loaded

            var bmp = TensorBitmap<byte, Bgr24>.Create(48, 32, KnownPixelFormats.Rgb8);
            bmp.GetContext<Rgb24>().Fill(BitmapOperations.StretchToFit, img);

            // save back

            using var img2 = bmp.ToImageSharp();

            AttachmentInfo.From("shannon.stretched.jpg").WriteObject(img2.Save);
        }

        [Test]
        public async Task TestScaledIntersectionCrop()
        {
            var l = new System.Drawing.Size(100, 50);
            var r = new System.Drawing.Size(50, 100);

            var crop = ScaledIntersectionCrop.CreateFrom(l, r, 0.5f);
        }
        
        [Test]
        [Arguments(48,256)]
        [Arguments(256, 48)]
        public async Task BitmapPreserveAspectFitTests(int w, int h)
        {
            using var img_isharp = ImageSharpBitmapOperand<Rgb24>.Read(ResourceInfo.From("shannon.jpg").OpenRead);

            using var img_skya = OperatingSystem.IsLinux()
                ? null 
                : SkiaSharpBitmapOperand<uint>.Read(ResourceInfo.From("shannon.jpg").OpenRead);

            img_isharp.ToTensorBitmap(out TensorBitmap<byte, Rgb24> img_ref);

            for (int oa = 0; oa <= 10; oa ++)
            {
                // imagesharp
                var bmp = TensorBitmap<byte, Rgb24>.Create(w, h, KnownPixelFormats.Rgb8);
                bmp.GetContext<Rgb24>().Fill(BitmapOperations.ScaleToFit(oa / 10f), img_isharp);
                
                using var img2 = bmp.Cast<Rgb24>().ToImageSharp();
                AttachmentInfo.From($"shannon.{oa}.imagesharp.jpg").WriteObject(img2.Save);

                // ref
                bmp = TensorBitmap<byte, Rgb24>.Create(w, h, KnownPixelFormats.Rgb8);
                bmp.GetContext<Rgb24>().Fill(BitmapOperations.ScaleToFit(oa / 10f), img_ref);

                using var img3 = bmp.Cast<Rgb24>().ToImageSharp();
                AttachmentInfo.From($"shannon.{oa}.ref.jpg").WriteObject(img3.Save);

                if (img_skya == null) continue;

                // ref
                bmp = TensorBitmap<byte, Rgb24>.Create(w, h, KnownPixelFormats.Rgb8);
                bmp.GetContext<uint>().Fill(BitmapOperations.ScaleToFit(oa / 10f), img_skya);

                using var img4 = bmp.Cast<Rgb24>().ToImageSharp();
                AttachmentInfo.From($"shannon.{oa}.skia.jpg").WriteObject(img4.Save);
            }
        }

    }
}

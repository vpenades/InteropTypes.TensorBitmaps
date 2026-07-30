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

            using var img = ImageSharpBitmap<Rgb24>.Read(ResourceInfo.From("shannon.jpg").OpenRead);            

            // create another tensor bitmap and fill it with the image we've loaded

            var bmp = TensorBitmap<byte, Bgr24>.Create(48, 32, KnownPixelFormats.Rgb8);
            bmp.GetContext<Rgb24>().Fill(BitmapOperations.StretchToFit, img);

            // save back

            using var img2 = bmp.ToImageSharp();

            AttachmentInfo.From("shannon.stretched.jpg").WriteObject(img2.Save);
        }

        [Test]
        public async Task LanczosResizeTest()
        {
            // load image and convert it to a tensor bitmap

            using var img = ImageSharpBitmap<Rgb24>.Read(ResourceInfo.From("shannon.jpg").OpenRead);

            var tmp = new ManagedBitmap<Vector4>(img.Width, img.Height, KnownPixelFormats.RgbaF32);
            tmp.GetContext<Rgb24>().Fill(img);

            tmp = LanczosResizer.Resize(tmp, 120, 80);            

            AttachmentInfo.From("shannon.resized.jpg").WriteObjectEx(x=> tmp.Save(x));
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
            for (int oa = 0; oa <= 10; oa ++)
            {
                // imagesharp

                using var img_isharp = ImageSharpBitmap<Rgb24>.Read(ResourceInfo.From("shannon.jpg").OpenRead);

                var bmp = TensorBitmap<byte, Rgb24>.Create(w, h, KnownPixelFormats.Rgb8);
                bmp.GetContext<Rgb24>().Fill(BitmapOperations.ScaleToFit(oa / 10f), img_isharp);                
                AttachmentInfo.From($"shannon.{oa}.imagesharp.jpg").WriteObjectEx(bmp.Save);

                // ref

                img_isharp.ToTensorBitmap(out TensorBitmap<byte, Rgb24> img_ref);

                bmp = TensorBitmap<byte, Rgb24>.Create(w, h, KnownPixelFormats.Rgb8);
                bmp.GetContext<Rgb24>().Fill(BitmapOperations.ScaleToFit(oa / 10f), img_ref);                
                AttachmentInfo.From($"shannon.{oa}.ref.jpg").WriteObjectEx(bmp.Save);

                // magicScaler
                /*
                bmp = TensorBitmap<byte, Rgb24>.Create(w, h, KnownPixelFormats.Rgb8);
                bmp.GetContext<Rgb24>().Fill(MagicScalerUtils.ScaleToFit(oa / 10f), img_ref);
                AttachmentInfo.From($"shannon.{oa}.magicScaler.jpg").WriteObjectEx(bmp.Save);
                */

                if (OperatingSystem.IsLinux()) continue;

                // skia

                using var img_skia = SkiaSharpBitmapOperand<uint>.Read(ResourceInfo.From("shannon.jpg").OpenRead); ;

                bmp = TensorBitmap<byte, Rgb24>.Create(w, h, KnownPixelFormats.Rgb8);
                bmp.GetContext<uint>().Fill(BitmapOperations.ScaleToFit(oa / 10f), img_skia);                
                AttachmentInfo.From($"shannon.{oa}.skia.jpg").WriteObjectEx(bmp.Save);
            }
        }

    }
}

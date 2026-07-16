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
    internal class PixelTransformTests
    {
        [Test]
        public async Task BitmapFitTest()
        {
            // load image and convert it to a tensor bitmap

            using var img = Image.Load<Rgb24>(ResourceInfo.From("shannon.jpg"));
            var imgTB = img.ToTensorBitmap<byte, Rgb24>();

            // create another tensor bitmao and fill it with the image we've loaded

            var bmp = TensorBitmap<byte, Bgr24>.Create(48, 32, KnownPixelFormats.Rgb8);
            bmp.GetContext<Rgb24>().Fill(PixelsTransform.StretchToFit, imgTB);

            // save back

            using var img2 = bmp.ToImageSharp();

            AttachmentInfo.From("shannon.stretched.jpg").WriteObject(img2.Save);
        }

        [Test]
        [Arguments(48,256)]
        [Arguments(256, 48)]
        public async Task BitmapPreserveAspectFitTests(int w, int h)
        {
            using var img0 = Image.Load<Rgb24>(ResourceInfo.From("shannon.jpg"));
            var img1 = img0.ToTensorBitmap<byte,Rgb24>();

            for (int oa = 0; oa <= 10; oa ++)
            {
                var bmp = TensorBitmap<byte, Rgb24>.Create(w, h, KnownPixelFormats.Rgb8);

                bmp.GetContext<Rgb24>().Fill(PixelsTransform.ScaleToFit(oa / 10f), img1);                

                using var img2 = bmp.Cast<Rgb24>().ToImageSharp();

                AttachmentInfo.From($"shannon.{oa}.jpg").WriteObject(img2.Save);
            }
        }

    }
}

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
            var bmp = TensorBitmap<byte, Rgb24>.Create(48,32,KnownPixelFormats.Rgb8);

            using var img = Image.Load<Rgb24>(ResourceInfo.From("shannon.jpg"));
            var bmp2 = img.ToTensorBitmap<byte, Rgb24>();

            bmp.GetContext<Rgb24, Matrix3x2>(PixelsTransform.StretchToFit).ApplyFrom(bmp2);

            using var img2 = bmp.Cast<Rgb24>().ToImageSharp();

            AttachmentInfo.From("shannon.transformed.jpg").WriteObject(img2.Save);
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

                bmp.GetContext<Rgb24, Matrix3x2>(PixelsTransform.ScaleToFit(oa / 10f)).ApplyFrom(img1);                

                using var img2 = bmp.Cast<Rgb24>().ToImageSharp();

                AttachmentInfo.From($"shannon.{oa}.jpg").WriteObject(img2.Save);
            }
        }

    }
}

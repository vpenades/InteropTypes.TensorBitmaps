using System;
using System.Collections.Generic;
using System.Linq;
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

            img.ToTensorBitmap<byte, Rgb24>()
                .AsReadOnlyTensorSpanBitmap()
                .CopyPixelsTo(PixelsTransform.StretchToFit, bmp.AsTensorSpanBitmap());

            using var img2 = bmp.Cast<Rgb24>().ToImageSharp();

            AttachmentInfo.From("shannon.transformed.jpg").WriteObject(img2.Save);
        }

        [Test]
        [Arguments(48,256)]
        [Arguments(256, 48)]
        public async Task BitmapPreserveAspectFitTests(int w, int h)
        {
            using var img0 = Image.Load<Rgb24>(ResourceInfo.From("shannon.jpg"));

            for (int oa = 0; oa <= 10; oa ++)
            {
                var bmp = TensorBitmap<byte, Rgb24>.Create(w, h, KnownPixelFormats.Rgb8);                

                img0.ToTensorBitmap<byte, Rgb24>()
                    .AsReadOnlyTensorSpanBitmap()
                    .CopyPixelsTo(PixelsTransform.ScaleToFit(oa / 10f), bmp.AsTensorSpanBitmap());

                using var img1 = bmp.Cast<Rgb24>().ToImageSharp();

                AttachmentInfo.From($"shannon.{oa}.jpg").WriteObject(img1.Save);
            }
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics.Tensors;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

using InteropTypes.Numerics;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

using TUnit;

namespace InteropTypes.TensorBitmaps
{
    internal class TensorPlanesTests
    {
        [Test]
        public async Task TestPlaneBitmaps()
        {
            using var img = Image.Load<Bgr24>(ResourceInfo.From("shannon.jpg"));

            // convert ImageSharp to TensorBitmap

            var srcBmp = img.ToTensorBitmap<byte, Bgr24>()
                .AsReadOnlyTensorSpanBitmap()
                .GetCropped(new System.Drawing.Rectangle(200, 100, 280, 280));

            // create a custom RGB format with a std-mean range

            var biasedRed = new PixelComponent<float>("Red", -1, 1);
            var biasedGreen = new PixelComponent<float>("Green", -1.3f, 1.3f);
            var biasedBlue = new PixelComponent<float>("Blue", -0.8f, 0.8f);
            var biasedFormat = new PixelFormat(biasedRed, biasedBlue, biasedGreen);

            // create a CWH bitmap and extract planar bitmaps

            var planes = TensorSpanPlanes3<float>.Create(256,256, biasedFormat);

            // fill planes with shannon.jpg:

            planes.CopyPixelsFrom(srcBmp);

            // merge planes back to a regular bitmap

            var dstBmp = TensorBitmap<byte, Rgb24>.Create(planes.Width, planes.Height, KnownPixelFormats.Rgb8);
            planes.CopyPixelsTo(dstBmp);

            using var result = dstBmp.ToImageSharp();

            AttachmentInfo.From($"shannon.merged.jpg").WriteObject(result.Save);
        }
    }
}

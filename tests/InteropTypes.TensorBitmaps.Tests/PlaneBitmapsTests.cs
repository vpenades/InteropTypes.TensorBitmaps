using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics.Tensors;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

using TUnit;

namespace InteropTypes.TensorBitmaps
{
    internal class PlaneBitmapsTests
    {
        [Test]
        public async Task TestPlaneBitmaps()
        {
            using var img = Image.Load<Bgr24>(ResourceInfo.From("shannon.jpg"));
            var srcBmp = img.ToTensorBitmap<byte, Bgr24>().AsReadOnlyTensorSpanBitmap().GetCropped(new System.Drawing.Rectangle(200, 100, 280, 280));

            // create a CWH tensor and extract planar bitmaps

            var biasedRed = new TensorPixelComponent<float>("Red", -1, 1);
            var biasedGreen = new TensorPixelComponent<float>("Green", -1.3f, 1.3f);
            var biasedBlue = new TensorPixelComponent<float>("Blue", -0.8f, 0.8f);
            var biasedFormat = new TensorPixelFormat(biasedRed, biasedBlue, biasedGreen);

            var tensor = Tensor.Create(new float[256 * 256 * 3], [3, 256, 256]);
            TensorSpanBitmap<float, float>.CreatePlaneBitmaps(tensor, biasedFormat, out var planeR, out var planeG, out var planeB);

            // fill planar bitmaps:

            srcBmp.CopyPixelsTo(planeR);
            srcBmp.CopyPixelsTo(planeG);
            srcBmp.CopyPixelsTo(planeB);

            // merge back:

            var dstBmp = TensorBitmap<byte, Rgb24>.Create(planeR.Width, planeR.Height, TensorPixelFormat.Rgb24);
            planeR.CopyPixelsTo(dstBmp.AsTensorSpanBitmap(), false);
            planeG.CopyPixelsTo(dstBmp.AsTensorSpanBitmap(), false);
            planeB.CopyPixelsTo(dstBmp.AsTensorSpanBitmap(), false);


            using var result = dstBmp.ToImageSharp();

            AttachmentInfo.From($"shannon.merged.jpg").WriteObject(result.Save);
        }
    }
}

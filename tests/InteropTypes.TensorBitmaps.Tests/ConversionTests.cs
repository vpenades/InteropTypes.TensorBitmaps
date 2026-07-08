using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

using TUnit;

namespace InteropTypes.TensorBitmaps
{
    internal class ConversionTests
    {
        [Test]
        public async Task TestPixelConversions()
        {
            using var img = Image.Load<Rgb24>(ResourceInfo.From("shannon.jpg"));

            var tbmp = img.ToTensorBitmap<byte, Rgb24>().AsReadOnlyTensorSpanBitmap().GetCropped(new System.Drawing.Rectangle(200,100,280,280));

            ConvertAndSave<byte, Rgb24>(tbmp);

            ConvertAndSave<byte, Bgra32>(tbmp);

            ConvertAndSave<byte, Bgr24>(tbmp);

            ConvertAndSave<byte, Argb32>(tbmp);

            ConvertAndSave<float, RgbaVector>(tbmp);
        }

        private static void ConvertAndSave<TElement, TPixel>(ReadOnlyTensorSpanBitmap<byte, Rgb24> src)
            where TElement: unmanaged
            where TPixel: unmanaged, IPixel<TPixel>
        {
            var dst = TensorBitmap<TElement, TPixel>.Create(256, 256, _ImageSharpUtils.ToTensorPixelFormat(typeof(TPixel)));

            src.CopyPixelsTo(dst.AsTensorSpanBitmap());

            using var result = dst.ToImageSharp();

            AttachmentInfo.From($"shannon.{typeof(TPixel).Name}.jpg").WriteObject(result.Save);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

using TUnit;

namespace InteropTypes.TensorBitmaps
{
    internal class ImageSharpConversionTests
    {
        [Test]
        public async Task TestPixelConversions()
        {
            using var img = Image.Load<Rgb24>(ResourceInfo.From("shannon.jpg"));

            var tbmp = img
                .ToTensorBitmap<byte, Rgb24>()
                .AsReadOnlyTensorSpanBitmap()
                .GetCropped(new System.Drawing.Rectangle(200,100,280,280)); // crop Shannon's face.

            ConvertAndSave<byte, Rgb24>(tbmp);

            ConvertAndSave<byte, Bgra32>(tbmp);

            ConvertAndSave<byte, Bgr24>(tbmp);

            ConvertAndSave<byte, Argb32>(tbmp);

            ConvertAndSave<ushort, Rg32>(tbmp); // blue channel will be lost;

            ConvertAndSave<Half, HalfVector2>(tbmp); // blue channel will be lost;

            ConvertAndSave<float, RgbaVector>(tbmp);
        }

        private static void ConvertAndSave<TElement, TPixel>(ReadOnlyTensorSpanBitmap<byte, Rgb24> src)
            where TElement: unmanaged, INumber<TElement>
            where TPixel: unmanaged, IPixel<TPixel>
        {
            var fmt = _ImageSharpUtils.ToTensorPixelFormat(typeof(TPixel));
            var dst = TensorBitmap<TElement, TPixel>.Create(256, 256, fmt);

            // copies the pixels from src to dst, taking into account the pixel layout and each component range.
            src.CopyPixelsTo(dst);

            using var result = dst.ToImageSharp();

            AttachmentInfo
                .From($"shannon.{typeof(TPixel).Name}.jpg")
                .WriteObject(result.Save);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InteropTypes.TensorBitmaps
{
    internal class PixelFormatsTests
    {
        [Test]
        public async Task TestCreateHybridFormat()
        {
            // tensors bitmaps themselves may not support hybrid formats,
            // but nothing prevents TensorPixelFormat to support them.

            var hybrid = new TensorPixelFormat(KnownComponentFormats.RedByte, KnownComponentFormats.RedSingle);
            await Assert.That(hybrid).IsNotNull();
        }

        [Test]
        public async Task TestFormatEquality()
        {
            await Assert.That(KnownPixelFormats.Rgb888).IsNotEqualTo(KnownPixelFormats.Bgr888);

            var r = new TensorPixelComponent<byte>("Red", 0, 255);
            var g = new TensorPixelComponent<byte>("Green", 0, 255);
            var b = new TensorPixelComponent<byte>("Blue", 0, 255);
            var rgb24 = new TensorPixelFormat(r, g, b);
            await Assert.That(KnownPixelFormats.Rgb888).IsEqualTo(KnownPixelFormats.Rgb888);
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using InteropTypes.Numerics;

namespace InteropTypes.TensorBitmaps
{
    internal class PixelFormatsTests
    {
        [Test]
        public async Task TestCreateHybridFormat()
        {
            // tensors bitmaps themselves may not support hybrid formats,
            // but nothing prevents TensorPixelFormat to support them.

            var hybrid = new PixelFormat(KnownComponentFormats.RedByte, KnownComponentFormats.RedSingle);
            await Assert.That(hybrid).IsNotNull();
        }

        [Test]
        public async Task TestFormatEquality()
        {
            await Assert.That(KnownPixelFormats.Rgb8).IsNotEqualTo(KnownPixelFormats.Bgr8);

            var r = new PixelComponent<byte>("Red", 0, 255);
            var g = new PixelComponent<byte>("Green", 0, 255);
            var b = new PixelComponent<byte>("Blue", 0, 255);
            var rgb24 = new PixelFormat(r, g, b);
            await Assert.That(KnownPixelFormats.Rgb8).IsEqualTo(KnownPixelFormats.Rgb8);
        }

    }
}

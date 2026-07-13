using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
            await Assert.That(hybrid.TryGetCommonType(out _)).IsFalse();
        }

        [Test]
        public async Task TestFormatEquality()
        {
            await Assert.That(KnownPixelFormats.Rgb8).IsNotEqualTo(KnownPixelFormats.Bgr8);

            var x1 = new PixelComponent<byte>("x", 10, 20);
            var x2 = new PixelComponent<byte>("x", 10, 20);
            var x3 = new PixelComponent<byte>("x", 20, 30);
            var x4 = new PixelComponent<float>("x", 10, 20);

            await Assert.That(x1 != null).IsTrue();

            await Assert.That(x1.GetHashCode()).IsEqualTo(x2.GetHashCode());
            await Assert.That(x1).IsEqualTo(x2);
            await Assert.That(x1 == x2).IsTrue();

            await Assert.That(x1.GetHashCode()).IsNotEqualTo(x3.GetHashCode());
            await Assert.That(x1).IsNotEqualTo(x3);
            await Assert.That(x1 == x3).IsFalse();

            await Assert.That((PixelComponent)x1 == (PixelComponent)x2).IsTrue();
            await Assert.That((PixelComponent)x1).IsEqualTo(x2);
            await Assert.That((PixelComponent)x1).IsNotEqualTo(x4);

            var r = new PixelComponent<byte>("Red");
            var g = new PixelComponent<byte>("Green");
            var b = new PixelComponent<byte>("Blue");

            var rgb24 = new PixelFormat(r, g, b);

            await Assert.That(r == KnownComponentFormats.RedByte).IsTrue();
            await Assert.That(r).IsEqualTo(KnownComponentFormats.RedByte);                       
            
            await Assert.That(rgb24).IsEqualTo(KnownPixelFormats.Rgb8);

            await Assert.That(rgb24 == KnownPixelFormats.Rgb8).IsTrue();
        }

    }
}

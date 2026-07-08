using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InteropTypes.TensorBitmaps
{
    internal class TensorPixelFormatTests
    {
        public async Task TestFormatEquality()
        {
            await Assert.That(TensorPixelFormat.Rgb24).IsNotEqualTo(TensorPixelFormat.Bgr24);

            var r = new TensorPixelComponent<byte>("Red", 0, 255);
            var g = new TensorPixelComponent<byte>("Green", 0, 255);
            var b = new TensorPixelComponent<byte>("Blue", 0, 255);
            var rgb24 = new TensorPixelFormat(r, g, b);
            await Assert.That(TensorPixelFormat.Rgb24).IsEqualTo(TensorPixelFormat.Rgb24);
        }

    }
}

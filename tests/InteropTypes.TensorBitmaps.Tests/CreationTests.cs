using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace InteropTypes.TensorBitmaps
{
    internal class CreationTests
    {

        [Test]
        public async Task TestCreateBitmaps()
        {
            var bmp = TensorBitmap<float, Vector3>.Create(256, 267, TensorPixelFormat.Rgb96f);
            await Assert.That(bmp).IsNotNull();

            var slice = bmp.GetCropped(new System.Drawing.Rectangle(5, 5, 16, 16));
            await Assert.That(slice.Width).IsEqualTo(16);
            await Assert.That(slice.Height).IsEqualTo(16);

            for(int i=0; i < slice.Height; i++)
            {
                var row = slice.GetRowPixelsSpan(i);

                await Assert.That(row.Length).IsEqualTo(16);
            }

        }


    }
}

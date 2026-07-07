using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

using TUnit;

namespace InteropTypes.TensorBitmaps
{
    internal class CreationTests
    {

        [Test]
        public async Task TestCreateBitmaps()
        {
            var bmp = TensorBitmap<float, Vector3>.Create(256, 267, TensorPixelFormat.Rgb96f);
            await Assert.That(bmp).IsNotNull();

            var crop = bmp.GetCropped(new System.Drawing.Rectangle(5, 5, 16, 16));
            await Assert.That(crop.Width).IsEqualTo(16);
            await Assert.That(crop.Height).IsEqualTo(16);

            for(int i=0; i < crop.Height; i++)
            {
                var row = crop.GetRowPixelsSpan(i);

                await Assert.That(row.Length).IsEqualTo(16);
            }
        }

        [Test]
        public async Task LoadChangeSaveTest()
        {
            using var img = Image.Load<Rgb24>(ResourceInfo.From("shannon.jpg"));

            var tbmp = img.ToTensorBitmap<byte,Rgb24>().Cast<PixelRGB>();

            var crop = tbmp.GetCropped(new System.Drawing.Rectangle(5, 5, 16, 16));

            var fillColor = new PixelRGB(0, 255, 0);
            for (int i = 0; i < crop.Height; i++)
            {
                var row = crop.GetRowPixelsSpan(i); 
                row.Fill(fillColor);
            }

            using var img2 = tbmp.Cast<Rgb24>().ToImageSharp();

            AttachmentInfo.From("shannon.modified.jpg").WriteObject(img2.Save);
        }


        [StructLayout(LayoutKind.Sequential,Pack =1)]
        struct PixelRGB
        {
            public byte Red;
            public byte Green;
            public byte Blue;

            public PixelRGB(byte red, byte green, byte blue)
            {
                Red = red;
                Green = green;
                Blue = blue;
            }
        }
    }
}

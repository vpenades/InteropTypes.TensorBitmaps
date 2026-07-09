using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;

using InteropTypes.Numerics;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

using TUnit;

namespace InteropTypes.TensorBitmaps
{
    internal class TensorBitmapTests
    {

        [Test]
        public async Task TestCreateBitmaps()
        {
            var bmp = TensorBitmap<float, Vector3>.Create(256, 267, KnownPixelFormats.RgbF32);
            await Assert.That(bmp).IsNotNull();

            bmp.AsTensorSpanBitmap().FillPixels(new Vector3(0, 1, 0));

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
        public async Task LoadSaveRoundtripTest()
        {
            using var img = Image.Load<Rgb24>(ResourceInfo.From("shannon.jpg"));

            var tbmp = img.ToTensorBitmap<byte,Rgb24>().Cast<Pixel888>();

            var crop = tbmp.GetCropped(new System.Drawing.Rectangle(5, 5, 16, 16));

            var fillColor = new Pixel888(0, 255, 0);
            for (int i = 0; i < crop.Height; i++)
            {
                var row = crop.GetRowPixelsSpan(i); 
                row.Fill(fillColor);
            }

            using var img2 = tbmp.Cast<Rgb24>().ToImageSharp();

            AttachmentInfo.From("shannon.modified.jpg").WriteObject(img2.Save);
        }


        
    }
}

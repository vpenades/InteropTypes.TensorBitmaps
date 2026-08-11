using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

using InteropTypes.Numerics;
using InteropTypes.TensorBitmaps.Operators;
using InteropTypes.TensorBitmaps.Primitives;

using PhotoSauce.MagicScaler;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

using TUnit;

namespace InteropTypes.TensorBitmaps
{
    internal class PixelTransformTests
    {
        [Test]
        public async Task BitmapFitTest()
        {
            // load image and convert it to a tensor bitmap

            using var img = ImageSharpBitmap<Rgb24>.Read(ResourceInfo.From("shannon.jpg").OpenRead);                        

            // create another tensor bitmap and fill it with the image we've loaded

            var bmp = TensorBitmap<byte, Rgb24>.Create(48, 32, KnownPixelFormats.Rgb8);
            bmp.Apply(FillOperations.StretchToFitStep, img);

            // save back

            using var img2 = bmp.ToImageSharp();

            AttachmentInfo.From("shannon.stretched.jpg").WriteObject(img2.Save);
        }

        [Test]
        public async Task StretchOperationTest()
        {
            // load image and convert it to a tensor bitmap

            var s = new System.Drawing.Size(128, 96);

            var f = ResourceInfo.From("shannon.jpg");

            // magic scaler reference

            if (OperatingSystem.IsWindows())
            {
                var mso = new ProcessImageSettings { Width = s.Width, Height = s.Height, ResizeMode = CropScaleMode.Stretch };
                using var ms = MagicImageProcessor.BuildPipeline(f.File.FullName, mso);
                AttachmentInfo.From($"shannon.Reference.MagicScaler.jpg").WriteToStream(x => ms.WriteOutput(x));
            }

            // imagesharp reference

            using var img = ImageSharpBitmap<Rgb24>.Read(f.OpenRead);

            using var iref = img.CreateStretched(s);
            AttachmentInfo.From($"shannon.Reference.ImageSharp.jpg").WriteObjectEx(x => iref.Save(x));

            // operators

            var ops = new List<BitmapOperationFactory<Matrix3x2>>();
            ops.Add(FillOperations.StretchToFitStep);
            ops.Add(FillOperations.StretchToFitBicubic);
            ops.Add(FillOperations.StretchToFitLanczos);
            if (OperatingSystem.IsWindows())  ops.Add(MagicScalerUtils.StretchToFit);

            foreach (var op in ops)
            {
                var src = new ArrayBitmap<uint>(img.Width, img.Height, KnownPixelFormats.Rgba8);                
                src.CopyFrom(img);

                var dst = new ArrayBitmap<uint>(s.Width, s.Height, KnownPixelFormats.Rgba8);                
                dst.Apply(op, src);

                AttachmentInfo.From($"shannon.{op.GetType().Name}.jpg").WriteObjectEx(x => dst.Save(x));
            }
        }

        [Test]
        public async Task TestScaledIntersectionCrop()
        {
            var l = new System.Drawing.Size(100, 50);
            var r = new System.Drawing.Size(50, 100);

            var crop = ScaledIntersectionCrop.CreateFrom(l, r, 0.5f);
        }
        
        [Test]
        [Arguments(48,256)]
        [Arguments(256, 48)]
        public async Task BitmapPreserveAspectFitTests(int w, int h)
        {
            for (int oa = 0; oa <= 10; oa ++)
            {
                // imagesharp

                using var img_isharp = ImageSharpBitmap<Rgb24>.Read(ResourceInfo.From("shannon.jpg").OpenRead);

                var bmp = TensorBitmap<byte, Rgb24>.Create(w, h, KnownPixelFormats.Rgb8);
                bmp.Apply(FillOperations.ScaleToFit(oa / 10f), img_isharp);                
                AttachmentInfo.From($"shannon.{oa}.imagesharp.jpg").WriteObjectEx(x => bmp.Save(x));

                // ref

                img_isharp.ToTensorBitmap(out TensorBitmap<byte, Rgb24> img_ref);

                bmp = TensorBitmap<byte, Rgb24>.Create(w, h, KnownPixelFormats.Rgb8);
                bmp.Apply(FillOperations.ScaleToFit(oa / 10f), img_ref);
                AttachmentInfo.From($"shannon.{oa}.ref.jpg").WriteObjectEx(x => bmp.Save(x));

                // ref lanczos
                img_isharp.ToTensorBitmap(out img_ref);

                bmp = TensorBitmap<byte, Rgb24>.Create(w, h, KnownPixelFormats.Rgb8);
                bmp.Apply(FillOperations.ScaleToFit(oa / 10f, FillOperations.StretchToFitLanczos), img_ref);
                AttachmentInfo.From($"shannon.{oa}.ref_lanczos.jpg").WriteObjectEx(x => bmp.Save(x));

                if (OperatingSystem.IsLinux()) continue;

                // skia

                using var img_skia = SkiaSharpBitmapOperand<uint>.Read(ResourceInfo.From("shannon.jpg").OpenRead); ;

                bmp = TensorBitmap<byte, Rgb24>.Create(w, h, KnownPixelFormats.Rgb8);
                bmp.Apply(FillOperations.ScaleToFit(oa / 10f), img_skia);                
                AttachmentInfo.From($"shannon.{oa}.skia.jpg").WriteObjectEx(x => bmp.Save(x));
            }
        }

    }
}

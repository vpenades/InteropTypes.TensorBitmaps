using System.Numerics;
using System.Numerics.Tensors;
using System.Runtime.CompilerServices;

using SkiaSharp;

namespace InteropTypes.TensorBitmaps
{
    internal static class _SkiaSharpUtils
    {
        public static TensorBitmap<TElement, TPixel> ToResizedTensorBitmap<TElement, TPixel>(this SKBitmap srcImage, int newWidth, int newHeight, TensorPixelFormat dstFmt, SKSamplingOptions? options = null)
            where TElement : unmanaged, INumber<TElement>
            where TPixel : unmanaged
        {
            options ??= SKSamplingOptions.Default;

            var s = new SKSizeI(newWidth, newHeight);

            using (var resized = srcImage.Resize(s, options.Value))
            {
                return resized.ToTensorBitmap<TElement, TPixel>(dstFmt);
            }
        }

        public static TensorBitmap<TElement, TPixel> ToTensorBitmap<TElement, TPixel>(this SKBitmap srcImage, TensorPixelFormat dstFmt)
            where TElement : unmanaged , INumber<TElement>
            where TPixel : unmanaged
        {
            if (TryCastToTensorBitmap<byte>(srcImage, out var srcTensor1))
            {
                var dstBitmap = TensorBitmap<TElement, TPixel>.Create(srcImage.Width, srcImage.Height, dstFmt);
                srcTensor1.CopyPixelsTo(dstBitmap.AsTensorSpanBitmap());
                return dstBitmap;
            }

            if (TryCastToTensorBitmap<int>(srcImage, out var srcTensor4))
            {
                var dstBitmap = TensorBitmap<TElement, TPixel>.Create(srcImage.Width, srcImage.Height, dstFmt);
                srcTensor4.CopyPixelsTo(dstBitmap.AsTensorSpanBitmap());
                return dstBitmap;
            }

            throw new NotImplementedException();
        }

        public static bool TryCastToTensorBitmap<TPixel>(this SKBitmap srcImage, out TensorSpanBitmap<byte, TPixel> tensorBitmap)
            where TPixel : unmanaged
        {
            var srcTensor = DangerousGetPixelsAsTensorSpan(srcImage);

            if (Unsafe.SizeOf<TPixel>() == 1 && srcImage.BytesPerPixel == 1)
            {
                var srcFmt = _SkiaSharpToTensorPixelFormat(srcImage.ColorType, srcImage.AlphaType);
                tensorBitmap = new TensorSpanBitmap<byte, byte>(srcTensor, srcFmt).Cast<TPixel>();
                return true;
            }

            if (Unsafe.SizeOf<TPixel>() == 4 && srcImage.BytesPerPixel == 4)
            {
                var srcFmt = _SkiaSharpToTensorPixelFormat(srcImage.ColorType, srcImage.AlphaType);
                tensorBitmap = new TensorSpanBitmap<byte, int>(srcTensor, srcFmt).Cast<TPixel>();
                return true;
            }

            tensorBitmap = default;
            return false;
        }

        public static SKBitmap ToSkiaSharp<TElement, TPixel>(this TensorBitmap<TElement, TPixel> srcBitmap)
            where TElement : unmanaged, INumber<TElement>
            where TPixel : unmanaged
        {
            return ToSkiaSharp(srcBitmap.AsReadOnlyTensorSpanBitmap());
        }

        public static SKBitmap ToSkiaSharp<TElement, TPixel>(this TensorSpanBitmap<TElement, TPixel> srcBitmap)
            where TElement : unmanaged, INumber<TElement>
            where TPixel : unmanaged
        {
            return ToSkiaSharp(srcBitmap.AsReadOnlyTensorSpanBitmap());
        }

        public static SKBitmap ToSkiaSharp<TElement, TPixel>(this ReadOnlyTensorSpanBitmap<TElement, TPixel> srcBitmap)
            where TElement : unmanaged, INumber<TElement>
            where TPixel : unmanaged
        {
            var dstImage = new SKBitmap(srcBitmap.Width, srcBitmap.Height, true); // todo: check srcBitmap.Format for alpha

            if (TryCastToTensorBitmap<byte>(dstImage, out var dstTensor1))
            {
                srcBitmap.CopyPixelsTo(dstTensor1);
                return dstImage;
            }

            if (TryCastToTensorBitmap<int>(dstImage, out var dstTensor2))
            {
                srcBitmap.CopyPixelsTo(dstTensor2);
                return dstImage;
            }

            throw new NotImplementedException();
        }

        public static TensorSpan<byte> DangerousGetPixelsAsTensorSpan(this SKBitmap srcBitmap)
        {
            if (srcBitmap == null) throw new ArgumentNullException(nameof(srcBitmap));

            var srcBuffer = srcBitmap.GetPixelSpan();            

            var strides = new nint[3];
            strides[0] = srcBitmap.RowBytes;
            strides[1] = srcBitmap.BytesPerPixel; // pixel stride; here needs to be 4 
            strides[2] = strides[1] > 1 ? 1 : 0;

            return new TensorSpan<byte>(srcBuffer, [srcBitmap.Height, srcBitmap.Width, srcBitmap.BytesPerPixel], strides);
        }

        private static TensorPixelFormat _SkiaSharpToTensorPixelFormat(SKColorType ct, SKAlphaType at)
        {
            switch (ct)
            {
                case SKColorType.Alpha8: return KnownPixelFormats.Alpha8;
                case SKColorType.Gray8: return KnownPixelFormats.Luminance8;
                case SKColorType.Rgb888x: return KnownPixelFormats.Rgbx8888;

                case SKColorType.Rgba8888 when at == SKAlphaType.Unpremul: return KnownPixelFormats.Rgba8888;
                case SKColorType.Rgba8888 when at == SKAlphaType.Premul: return KnownPixelFormats.Rgbp8888;
                case SKColorType.Rgba8888: return KnownPixelFormats.Rgba8888;

                case SKColorType.Bgra8888 when at == SKAlphaType.Unpremul: return KnownPixelFormats.Bgra8888;
                case SKColorType.Bgra8888 when at == SKAlphaType.Premul: return KnownPixelFormats.Bgrp8888;
                case SKColorType.Bgra8888: return KnownPixelFormats.Bgra8888;

                case SKColorType.RgbaF32 when at == SKAlphaType.Unpremul: return KnownPixelFormats.RgbaF32;
                case SKColorType.RgbaF32 when at == SKAlphaType.Premul: return KnownPixelFormats.RgbpF32;
                case SKColorType.RgbaF32: return KnownPixelFormats.RgbaF32;

                case SKColorType.Rg88:
                    {
                        var r = new TensorPixelComponent<byte>("Red", 0, 255);
                        var g = new TensorPixelComponent<byte>("Green",0, 255);
                        return new TensorPixelFormat(r, g);
                    }

                // add more pixel types here

                default: throw new NotImplementedException($"{ct} {at}");
            }
        }
    }
}

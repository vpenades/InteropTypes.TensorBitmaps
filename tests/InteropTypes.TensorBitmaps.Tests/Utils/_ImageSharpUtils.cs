using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using SixLabors.ImageSharp.PixelFormats;

namespace InteropTypes.TensorBitmaps
{
    static class _ImageSharpUtils
    {
        public static SixLabors.ImageSharp.Image<TPixel> ToImageSharp<TElement, TPixel>(this TensorBitmap<TElement, TPixel> srcBitmap)
            where TElement : unmanaged, INumber<TElement>
            where TPixel : unmanaged, IPixel<TPixel>
        {
            var dstImage = new SixLabors.ImageSharp.Image<TPixel>(srcBitmap.Width, srcBitmap.Height);

            void _processPixels(SixLabors.ImageSharp.PixelAccessor<TPixel> pixelAccessor)
            {
                for (int y = 0; y < dstImage.Height; ++y)
                {
                    var srcRow = srcBitmap.GetRowPixelsSpan(y);
                    var dstRow = pixelAccessor.GetRowSpan(y);
                    srcRow.CopyTo(dstRow);
                }
            }

            dstImage.ProcessPixelRows(_processPixels);

            return dstImage;
        }

        public static SixLabors.ImageSharp.Image<TPixel> ToImageSharp<TElement, TPixel>(this ReadOnlyTensorSpanBitmap<TElement, TPixel> srcBitmap)
            where TElement : unmanaged, INumber<TElement>
            where TPixel : unmanaged, IPixel<TPixel>
        {
            var dstImage = new SixLabors.ImageSharp.Image<TPixel>(srcBitmap.Width, srcBitmap.Height);

            for (int y = 0; y < dstImage.Height; ++y)
            {
                var srcRow = srcBitmap.GetRowPixelsSpan(y);
                var dstRow = dstImage.Frames[0].PixelBuffer.DangerousGetRowSpan(y);
                srcRow.CopyTo(dstRow);
            }

            return dstImage;
        }

        public static TensorBitmap<TElement, TPixel> ToTensorBitmap<TElement, TPixel>(this SixLabors.ImageSharp.Image<TPixel> srcImage)
            where TElement : unmanaged, INumber<TElement>
            where TPixel : unmanaged, IPixel<TPixel>
        {
            var dstFmt = ToTensorPixelFormat(typeof(TPixel));
            var dstBitmap = TensorBitmap<TElement, TPixel>.Create(srcImage.Width, srcImage.Height, dstFmt);

            void _processPixels(SixLabors.ImageSharp.PixelAccessor<TPixel> pixelAccessor)
            {
                for (int y = 0; y < dstBitmap.Height; ++y)
                {
                    var dstRow = dstBitmap.GetRowPixelsSpan(y);
                    var srcRow = pixelAccessor.GetRowSpan(y);
                    srcRow.CopyTo(dstRow);
                }
            }

            srcImage.ProcessPixelRows(_processPixels);

            return dstBitmap;
        }

        public static TensorPixelFormat ToTensorPixelFormat(Type type)
        {
            if (type == typeof(A8)) return KnownPixelFormats.Alpha8;
            if (type == typeof(L8)) return KnownPixelFormats.Luminance8;

            if (type == typeof(Rgb24)) return KnownPixelFormats.Rgb888;
            if (type == typeof(Rgba32)) return KnownPixelFormats.Rgba8888;
            if (type == typeof(RgbaVector)) return KnownPixelFormats.RgbaF32;

            if (type == typeof(Bgr24)) return KnownPixelFormats.Bgr888;
            if (type == typeof(Bgra32)) return KnownPixelFormats.Bgra8888;

            if (type == typeof(Argb32)) return KnownPixelFormats.Argb8888;
            if (type == typeof(Abgr32)) return KnownPixelFormats.Abgr8888;

            if (type == typeof(Rg32)) return KnownPixelFormats.Rg1616;
            if (type == typeof(HalfVector2))
            {
                var r = new TensorPixelComponent<Half>("Red", -Half.One, Half.One);
                var g = new TensorPixelComponent<Half>("Green", -Half.One, Half.One);
                return new TensorPixelFormat(r, g);
            }

            // add more pixel types here

            throw new NotImplementedException(type.Name);
        }
    }
}

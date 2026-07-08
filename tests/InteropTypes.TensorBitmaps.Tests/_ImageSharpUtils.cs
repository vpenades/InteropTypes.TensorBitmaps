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
            if (type == typeof(L8)) return new TensorPixelFormat(TensorPixelComponent.LuminanceByte);

            if (type == typeof(Rgb24)) return TensorPixelFormat.Rgb24;
            if (type == typeof(Rgba32)) return TensorPixelFormat.Rgba32;
            if (type == typeof(RgbaVector)) return TensorPixelFormat.Rgba128f;

            if (type == typeof(Bgr24)) return TensorPixelFormat.Bgr24;
            if (type == typeof(Bgra32)) return TensorPixelFormat.Bgra32;

            if (type == typeof(Argb32)) return TensorPixelFormat.Argb32;
            if (type == typeof(Abgr32)) return TensorPixelFormat.Abgr32;


            if (type == typeof(Rg32)) return TensorPixelFormat.Rg32;
            if (type == typeof(HalfVector2))
            {
                var r = new TensorPixelComponent<Half>("Red", -Half.One, Half.One);
                var g = new TensorPixelComponent<Half>("Green", -Half.One, Half.One);
                return new TensorPixelFormat(r, g);
            }

            throw new NotImplementedException(type.Name);
        }
    }
}

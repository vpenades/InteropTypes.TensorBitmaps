using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace InteropTypes.TensorBitmaps
{
    public static partial class SkiaSharpForTensorBitmapsExtensions
    {
        public static TensorBitmap<TElement, TPixel> LoadTensorBitmapWithSkiaSharp<TElement, TPixel>(this System.IO.FileInfo finfo, Numerics.PixelFormat fmt)
            where TElement : unmanaged, INumber<TElement>
            where TPixel : unmanaged
        {
            return ReadTensorBitmapWithSkiaSharp<TElement, TPixel>(finfo.OpenRead, fmt);
        }

        public static TensorBitmap<TElement, TPixel> ReadTensorBitmapWithSkiaSharp<TElement, TPixel>(this Func<System.IO.Stream> stream, Numerics.PixelFormat fmt)
            where TElement : unmanaged, INumber<TElement>
            where TPixel : unmanaged
        {
            using(var s = stream.Invoke())
            {
                return ReadTensorBitmapWithSkiaSharp<TElement, TPixel>(s, fmt);
            }
        }

        public static void WriteTensorBitmapWithSkiaSharp<TElement, TPixel>(this Func<System.IO.Stream> stream, ReadOnlyTensorSpanBitmap<TElement, TPixel> bitmap, SkiaSharp.SKEncodedImageFormat fmt, int quality)
            where TElement : unmanaged, INumber<TElement>
            where TPixel : unmanaged
        {
            using (var s = stream.Invoke())
            {
                WriteTensorBitmapWithSkiaSharp(s, bitmap, fmt, quality);
            }
        }

        public static TensorBitmap<TElement, TPixel> ReadTensorBitmapWithSkiaSharp<TElement,TPixel>(this System.IO.Stream stream, Numerics.PixelFormat fmt)
            where TElement: unmanaged, INumber<TElement>
            where TPixel: unmanaged
        {            
            using var skbmp = SkiaSharp.SKBitmap.Decode(stream);
            return skbmp.ToTensorBitmap<TElement, TPixel>(fmt);            
        }

        public static void WriteTensorBitmapWithSkiaSharp<TElement, TPixel>(this System.IO.Stream stream, ReadOnlyTensorSpanBitmap<TElement, TPixel> bitmap, SkiaSharp.SKEncodedImageFormat fmt, int quality)
            where TElement : unmanaged, INumber<TElement>
            where TPixel : unmanaged
        {
            using var skbmp = bitmap.ToSkiaSharp();
            skbmp.Encode(stream, fmt, quality);
        }
    }
}

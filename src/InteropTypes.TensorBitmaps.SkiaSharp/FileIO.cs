using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using SkiaSharp;

namespace InteropTypes.TensorBitmaps
{
    public static partial class SkiaSharpForTensorBitmapsExtensions
    {
        public static bool TryGetSKEncodedImageFormat(System.IO.Stream stream, out SKEncodedImageFormat format)
        {
            format = SKEncodedImageFormat.Png;

            if (stream is not FileStream fs) return false;

            bool hasExt(string ext) => fs.Name.EndsWith(ext, StringComparison.OrdinalIgnoreCase);

            if (hasExt(".jpg") || hasExt(".jpeg")) { format = SKEncodedImageFormat.Jpeg; return true; }
            if (hasExt(".png")) { format = SKEncodedImageFormat.Png; return true; }
            if (hasExt(".gif")) { format = SKEncodedImageFormat.Gif; return true; }
            if (hasExt(".avif")) { format = SKEncodedImageFormat.Avif; return true; }
            if (hasExt(".webp")) { format = SKEncodedImageFormat.Webp; return true; }
            
            return false;
        }

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

using System;
using System.Collections.Generic;
using System.Numerics;
using System.Numerics.Tensors;
using System.Text;

using InteropTypes.TensorBitmaps;
using InteropTypes.TensorBitmaps.Primitives;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace InteropTypes
{
    internal static class _Extensions
    {
        public static uint CalculateCrc32<TPixel>(this IReadOnlyBitmap<TPixel> bitmap)
            where TPixel: unmanaged
        {
            var xbmp = new RefStructReadOnlyBitmap<TPixel>(bitmap);            

            return CalculateCrc32<RefStructReadOnlyBitmap<TPixel>, TPixel>(xbmp);
        }

        public static uint CalculateCrc32<TBitmap, TPixel>(this TBitmap bitmap)
            where TBitmap: IReadOnlyBitmap<TBitmap, TPixel>, allows ref struct
            where TPixel : unmanaged
        {
            var crc32 = new System.IO.Hashing.Crc32();

            for (int y = 0; y < bitmap.Height; ++y)
            {
                var bytes = bitmap.GetRowBytesSpan(y);

                crc32.Append(bytes);
            }

            return crc32.GetCurrentHashAsUInt32();
        }

        public static void Save<TPixel>(this IReadOnlyBitmap<TPixel> bitmap, System.IO.FileInfo finfo)            
            where TPixel : unmanaged
        {
            using var img = bitmap.ToImageSharp<TPixel,Rgba32>();            
            
            img.Save(finfo.FullName);
        }

        public static ArrayBitmap<TPixel> LoadBitmap<TPixel>(this System.IO.FileInfo finfo)
            where TPixel: unmanaged , IPixel<TPixel>
        {
            using var img = ImageSharpBitmap<TPixel>.Load(finfo);

            var dst = new ArrayBitmap<TPixel>(img.Width, img.Height, img.Format);
            dst.CopyFrom(img);
            return dst;
        }

        public static void Save<TElement, TPixel>(this TensorBitmap<TElement, TPixel> tensor, System.IO.FileInfo finfo)
            where TElement : unmanaged, INumber<TElement>
            where TPixel : unmanaged
        {
            tensor.AsReadOnlyTensorSpanBitmap().Save(finfo);
        }

        public static void Save<TElement, TPixel>(this TensorSpanBitmap<TElement, TPixel> tensor, System.IO.FileInfo finfo)
            where TElement : unmanaged, INumber<TElement>
            where TPixel : unmanaged
        {
            tensor.AsReadOnlyTensorSpanBitmap().Save(finfo);
        }

        public static void Save<TElement, TPixel>(this ReadOnlyTensorSpanBitmap<TElement, TPixel> tensor, System.IO.FileInfo finfo)
            where TElement : unmanaged, INumber<TElement>
            where TPixel: unmanaged
        {
            using var img = tensor.Cast<Rgb24>().ToImageSharp();
            img.Save(finfo.FullName);            
        }

    }
}

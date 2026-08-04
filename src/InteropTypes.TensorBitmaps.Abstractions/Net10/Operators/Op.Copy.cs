using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using InteropTypes.Numerics;

namespace InteropTypes.TensorBitmaps.Operators
{
    /// <summary>
    /// Operator that simply copies source over destination
    /// </summary>    
    readonly struct _DirectCopyOperator<TSrcPixel, TDstPixel>
            : IDrawOperation<TSrcPixel, TDstPixel, int>
            , IFillOperation<TSrcPixel, TDstPixel, int>
            where TSrcPixel : unmanaged
            where TDstPixel : unmanaged
    {
        public static _DirectCopyOperator<TSrcPixel, TDstPixel> Instance { get; } = new _DirectCopyOperator<TSrcPixel, TDstPixel>();

        public int Draw<TSrcBmp, TDstBmp>(TSrcBmp src, TDstBmp dst, IPixelConverter<TSrcPixel, TDstPixel> pixelConverter)
            where TSrcBmp : IReadOnlyBitmap<TSrcBmp, TSrcPixel>, allows ref struct
            where TDstBmp : IBitmap<TDstBmp, TDstPixel>, allows ref struct
        {
            var w = Math.Min(src.Width, dst.Width);
            var h = Math.Min(src.Height, dst.Height);

            for (int y = 0; y < h; ++y)
            {
                var srcRow = src.GetRowPixelsSpan(y);
                var dstRow = dst.GetRowPixelsSpan(y);
                pixelConverter.ConvertPixels(srcRow.Slice(0, w), dstRow);
            }

            return 0;
        }

        public int Fill<TSrcBmp, TDstBmp>(TSrcBmp src, TDstBmp dst, IPixelConverter<TSrcPixel, TDstPixel> pixelConverter)
            where TSrcBmp : IBitmapReader<TSrcBmp, TSrcPixel>, allows ref struct
            where TDstBmp : IBitmapWriter<TDstBmp, TDstPixel>, allows ref struct
        {
            var w = Math.Min(src.Width, dst.Width);
            var h = Math.Min(src.Height, dst.Height);

            Span<TSrcPixel> srcRow = stackalloc TSrcPixel[src.Width];
            Span<TDstPixel> dstRow = stackalloc TDstPixel[w];

            for (int y = 0; y < h; ++y)
            {
                src.ReadRowPixelsSpan(y, srcRow);
                pixelConverter.ConvertPixels(srcRow.Slice(0, w), dstRow);
                dst.WriteRowPixelsSpan(y, dstRow);
            }

            return 0;
        }

        public int FillEx<TSrcBmp, TDstBmp>(TSrcBmp src, TDstBmp dst, IPixelConverter<TSrcPixel, TDstPixel> pixelConverter)
            where TSrcBmp : IReadOnlyBitmap<TSrcBmp, TSrcPixel>, allows ref struct
            where TDstBmp : IBitmapWriter<TDstBmp, TDstPixel>, allows ref struct
        {
            var w = Math.Min(src.Width, dst.Width);
            var h = Math.Min(src.Height, dst.Height);

            Span<TDstPixel> dstRow = stackalloc TDstPixel[w];

            for (int y = 0; y < h; ++y)
            {
                var srcRow = src.GetRowPixelsSpan(y);
                pixelConverter.ConvertPixels(srcRow.Slice(0, w), dstRow);
                dst.WriteRowPixelsSpan(y, dstRow);
            }

            return 0;
        }
    }
}

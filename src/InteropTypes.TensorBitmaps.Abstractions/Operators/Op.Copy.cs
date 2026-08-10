using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using InteropTypes.Numerics;
using InteropTypes.TensorBitmaps.Primitives;

namespace InteropTypes.TensorBitmaps.Operators
{
    /// <summary>
    /// Operator that simply copies source over destination
    /// </summary>    
    readonly struct _DirectCopyOperator<TSrcPixel, TDstPixel>
            : IBitmapOperation<TSrcPixel, TDstPixel, int>            
            where TSrcPixel : unmanaged
            where TDstPixel : unmanaged
    {
        public static _DirectCopyOperator<TSrcPixel, TDstPixel> Instance { get; } = new _DirectCopyOperator<TSrcPixel, TDstPixel>();

        public int Apply<TSrcBmp, TDstBmp>(TSrcBmp src, TDstBmp dst, IPixelConverter<TSrcPixel, TDstPixel> pixelConverter)
            #if NET9_0_OR_GREATER
            where TSrcBmp : IReadOnlyBitmap<TSrcBmp, TSrcPixel>, allows ref struct
            where TDstBmp : IBitmap<TDstBmp, TDstPixel>, allows ref struct
            #else
            where TSrcBmp : IReadOnlyBitmap<TSrcPixel>
            where TDstBmp : IBitmap<TDstPixel>
            #endif
        {
            var w = Math.Min(src.Width, dst.Width);
            var h = Math.Min(src.Height, dst.Height);

            Span<TSrcPixel> srcRow = stackalloc TSrcPixel[src.Width];
            Span<TDstPixel> dstRow = stackalloc TDstPixel[w];

            for (int y = 0; y < h; ++y)
            {
                src.ReadRowPixelsSpan(y, 0, srcRow);
                pixelConverter.ConvertPixels(srcRow.Slice(0, w), dstRow);
                dst.WriteRowPixelsSpan(y, 0, dstRow);
            }

            return 0;
        }
    }
}

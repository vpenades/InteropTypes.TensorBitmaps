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
    readonly struct _DirectFillOperator<TSrcPixel, TDstPixel>
            : IFillOperation<TSrcPixel, TDstPixel, int>            
            where TSrcPixel : unmanaged
            where TDstPixel : unmanaged
    {
        public static _DirectCopyOperator<TSrcPixel, TDstPixel> Instance { get; } = new _DirectCopyOperator<TSrcPixel, TDstPixel>();        

        public int Execute<TSrcBmp, TDstBmp>(TSrcBmp src, TDstBmp dst, IPixelConverter<TSrcPixel, TDstPixel> pixelConverter)
            where TSrcBmp : IBitmapReader<TSrcBmp, TSrcPixel>, allows ref struct
            where TDstBmp : IBitmapWriter<TDstBmp, TDstPixel>, allows ref struct
        {
            var w = Math.Min(src.Width, dst.Width);
            var h = Math.Min(src.Height, dst.Height);

            Span<TSrcPixel> srcRow = stackalloc TSrcPixel[w];
            Span<TDstPixel> dstRow = stackalloc TDstPixel[w];

            for (int y = 0; y < h; ++y)
            {
                src.ReadRowPixelsSpan(y,srcRow);
                pixelConverter.ConvertPixels(srcRow, dstRow);
                dst.WriteRowPixelsSpan(y, dstRow);                
            }

            return 0;
        }

        public int Execute2<TSrcBmp, TDstBmp>(TSrcBmp src, TDstBmp dst, IPixelConverter<TSrcPixel, TDstPixel> pixelConverter)
            where TSrcBmp : IReadOnlyBitmap<TSrcBmp, TSrcPixel>, allows ref struct
            where TDstBmp : IBitmapWriter<TDstBmp, TDstPixel>, allows ref struct
        {
            var w = Math.Min(src.Width, dst.Width);
            var h = Math.Min(src.Height, dst.Height);

            Span<TDstPixel> dstRow = stackalloc TDstPixel[w];

            for (int y = 0; y < h; ++y)
            {
                var srcRow = src.GetRowPixelsSpan(y).Slice(0, w);
                pixelConverter.ConvertPixels(srcRow, dstRow);
                dst.WriteRowPixelsSpan(y, dstRow);
            }

            return 0;
        }
    }
}

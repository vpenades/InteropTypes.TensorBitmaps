using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Numerics.Tensors;
using System.Text;
using System.Threading.Tasks;

using InteropTypes.Numerics;

namespace InteropTypes.TensorBitmaps
{
    #if !NET9_0_OR_GREATER   

    /// <summary>
    /// Reimplementation of <see cref="Numerics.BitmapOperators.IBinaryOperation{TSrcPixel, TDstPixel, TResult}"/> for Net8.0
    /// </summary>    
    interface ITensorSpanBitmapBinaryOperation<TSrcPixel, TDstPixel, TResult>        
        where TSrcPixel : unmanaged
        where TDstPixel : unmanaged
    {
        TResult Execute<TSrcElement, TDstElement>(
            ReadOnlyTensorSpanBitmap<TSrcElement, TSrcPixel> src,
            TensorSpanBitmap<TDstElement, TDstPixel> dst,
            IPixelConverter<TSrcPixel, TDstPixel> pixelConverter)
            where TSrcElement : unmanaged, INumber<TSrcElement>
            where TDstElement : unmanaged, INumber<TDstElement>;
    }

    /// <summary>
    /// Operator that simply copies source over destination
    /// </summary>    
    readonly struct _DirectCopyOperator< TSrcPixel, TDstPixel>
            : ITensorSpanBitmapBinaryOperation< TSrcPixel,  TDstPixel, int>            
            where TSrcPixel : unmanaged            
            where TDstPixel : unmanaged
    {
        public int Execute<TSrcElement,TDstElement>(ReadOnlyTensorSpanBitmap<TSrcElement, TSrcPixel> src, TensorSpanBitmap<TDstElement, TDstPixel> dst, IPixelConverter<TSrcPixel, TDstPixel> pixelConverter)
            where TSrcElement : unmanaged, INumber<TSrcElement>
            where TDstElement : unmanaged, INumber<TDstElement>
        {
            var h = Math.Min(src.Height, dst.Height);

            for (int y = 0; y < h; ++y)
            {
                var srcRow = src.GetRowPixelsSpan(y);
                var dstRow = dst.GetRowPixelsSpan(y);
                pixelConverter.ConvertPixels(srcRow, dstRow);
            }

            return default;
        }
    }

    /// <summary>
    /// Operator that resizes and crops the source so it fits into destination while preserving aspect ration.
    /// </summary>    
    readonly struct _ScaleToFitOperator<TSrcPixel, TDstPixel>
        : ITensorSpanBitmapBinaryOperation< TSrcPixel,  TDstPixel, Matrix3x2>        
        where TSrcPixel : unmanaged        
        where TDstPixel : unmanaged
    {
        public _ScaleToFitOperator(float overflow)
        {
            Overflow = overflow;
        }

        public float Overflow { get; }

        public Matrix3x2 Execute<TSrcElement, TDstElement>(ReadOnlyTensorSpanBitmap<TSrcElement, TSrcPixel> src, TensorSpanBitmap<TDstElement, TDstPixel> dst, IPixelConverter<TSrcPixel, TDstPixel> pixelConverter)
            where TSrcElement : unmanaged, INumber<TSrcElement>
            where TDstElement : unmanaged, INumber<TDstElement>
        {
            // calculate aspect ratio:
            var srck = (float)src.Width / (float)src.Height;
            var dstk = (float)dst.Width / (float)dst.Height;
            var k = srck * (1 - Overflow) + dstk * Overflow;

            // shrink both src and dst to ensure they have k aspect ratio:
            
            src = src.GetCropped(_GetCenterCrop(src.Width, src.Height, k));
            dst = dst.GetCropped(_GetCenterCrop(dst.Width, dst.Height, k));

            srck = (float)src.Width / (float)src.Height;
            dstk = (float)dst.Width / (float)dst.Height;

            new _ScaleToFitOperator<TSrcPixel, TDstPixel>().Execute(src, dst, pixelConverter);

            return default;
        }
        
        private static System.Drawing.Rectangle _GetCenterCrop(int w, int h, float aspect)
        {
            var k = (float)w /(float)h;

            if (k > aspect) // clip horizontally
            {
                var ww = h * aspect;
                var r = new System.Drawing.RectangleF((w - ww) / 2, 0, ww, h);
                return System.Drawing.Rectangle.Truncate(r);
            }
            
            if (k < aspect) // clip vertically
            {
                var hh = w / aspect;
                var r = new System.Drawing.RectangleF(0, (h - hh) / 2, w, hh);
                return System.Drawing.Rectangle.Truncate(r);
            }

            return new System.Drawing.Rectangle(0, 0, w, h);
        }        
    }

    /// <summary>
    /// Operator that resizes and stretches the source to fit into destination.
    /// </summary>
    /// <typeparam name="TSrcPixel"></typeparam>
    /// <typeparam name="TDstPixel"></typeparam>
    readonly struct _StretchToFitOperator<TSrcPixel, TDstPixel>
        : ITensorSpanBitmapBinaryOperation<TSrcPixel, TDstPixel, Matrix3x2>        
        where TSrcPixel : unmanaged        
        where TDstPixel : unmanaged
    {
        public Matrix3x2 Execute<TSrcElement, TDstElement>(ReadOnlyTensorSpanBitmap<TSrcElement, TSrcPixel> src, TensorSpanBitmap<TDstElement, TDstPixel> dst, IPixelConverter<TSrcPixel, TDstPixel> pixelConverter)
            where TSrcElement : unmanaged, INumber<TSrcElement>
            where TDstElement : unmanaged, INumber<TDstElement>
        {
            Span<TSrcPixel> tmpRow = stackalloc TSrcPixel[dst.Width];

            for (int y = 0; y < dst.Height; ++y)
            {
                var srcRow = src.GetRowPixelsSpan(y * src.Height / dst.Height);

                for (int x = 0; x < tmpRow.Length; ++x)
                {
                    tmpRow[x] = srcRow[x * srcRow.Length / tmpRow.Length];
                }

                var dstRow = dst.GetRowPixelsSpan(y);
                pixelConverter.ConvertPixels(tmpRow, dstRow);
            }

            return default;
        }
    }

    #endif

}

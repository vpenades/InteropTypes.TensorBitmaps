using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace InteropTypes.Numerics.BitmapOperators
{
    /// <summary>
    /// interface for operator that perform a pixel transfer between a source and destination bitmap
    /// </summary>
    /// <typeparam name="TSrcPixel">The pixel type of source</typeparam>
    /// <typeparam name="TDstPixel">The pixel type od destination</typeparam>
    public interface IBinaryOperation<TSrcPixel, TDstPixel>
        where TSrcPixel : unmanaged        
        where TDstPixel : unmanaged
    {
        void Execute<TSrcBmp,TDstBmp>(TSrcBmp src, TDstBmp dst, IPixelConverter<TSrcPixel, TDstPixel> pixelConverter)
            where TSrcBmp : IReadOnlyBitmapOperand<TSrcBmp,TSrcPixel>
            #if NET9_0_OR_GREATER
            , allows ref struct
            #endif
            where TDstBmp : IBitmapOperand<TDstBmp, TDstPixel>
            #if NET9_0_OR_GREATER
            , allows ref struct
            #endif
            ;

        public static IBinaryOperation<TSrcPixel, TDstPixel> DirectCopy { get; } = new _DirectCopyOperator<TSrcPixel, TDstPixel>();
        public static IBinaryOperation<TSrcPixel, TDstPixel> StretchToFit { get; } = new _StretchToFitOperator<TSrcPixel, TDstPixel>();
        public static IBinaryOperation<TSrcPixel, TDstPixel> ScaleToFit(float overflowAmount) => new _ScaleToFitOperator<TSrcPixel,TDstPixel>(overflowAmount);
    }
    

    /// <summary>
    /// Operator that simply copies source over destination
    /// </summary>    
    readonly struct _DirectCopyOperator<TSrcPixel, TDstPixel>
            : IBinaryOperation<TSrcPixel, TDstPixel>
            where TSrcPixel : unmanaged            
            where TDstPixel : unmanaged
    {
        public void Execute<TSrcBmp, TDstBmp>(TSrcBmp src, TDstBmp dst, IPixelConverter<TSrcPixel, TDstPixel> pixelConverter)
            where TSrcBmp : IReadOnlyBitmapOperand<TSrcBmp,TSrcPixel>
            #if NET9_0_OR_GREATER
            , allows ref struct
            #endif
            where TDstBmp : IBitmapOperand<TDstBmp, TDstPixel>
            #if NET9_0_OR_GREATER
            , allows ref struct
            #endif
        {
            var h = Math.Min(src.Height, dst.Height);

            for (int y = 0; y < h; ++y)
            {
                var srcRow = src.GetRowPixelsSpan(y);
                var dstRow = dst.GetRowPixelsSpan(y);
                pixelConverter.ConvertPixels(srcRow, dstRow);
            }
        }
    }

    /// <summary>
    /// Operator that resizes and crops the source so it fits into destination while preserving aspect ration.
    /// </summary>    
    readonly struct _ScaleToFitOperator<TSrcPixel, TDstPixel>
        : IBinaryOperation<TSrcPixel, TDstPixel>        
        where TSrcPixel : unmanaged        
        where TDstPixel : unmanaged
    {
        public _ScaleToFitOperator(float overflowAmount)
        {
            this.OverflowAmount = overflowAmount;
        }

        /// <summary>
        /// Represents the allowed overflow amount
        /// </summary>
        /// <remarks>
        /// A value of 0 means no overflow allowed, the source bitmap will shrink to completely fit into destionation.<br/>
        /// A value of 1 menas full overflow is allowed, the source bitmap will shrink enough to completely fill the destination,
        /// allowing parts of the source bitmap to overflow the destination.
        /// </remarks>
        public float OverflowAmount { get; }

        public void Execute<TSrcBmp, TDstBmp>(TSrcBmp src, TDstBmp dst, IPixelConverter<TSrcPixel, TDstPixel> pixelConverter)
            where TSrcBmp : IReadOnlyBitmapOperand<TSrcBmp,TSrcPixel>
            #if NET9_0_OR_GREATER
            , allows ref struct
            #endif
            where TDstBmp : IBitmapOperand<TDstBmp, TDstPixel>
            #if NET9_0_OR_GREATER
            , allows ref struct
            #endif
        {
            // calculate aspect ratios:
            var srck = (float)src.Width / (float)src.Height;
            var dstk = (float)dst.Width / (float)dst.Height;

            // lerp aspect ratios using allowed overflow amount
            var k = srck * (1 - OverflowAmount) + dstk * OverflowAmount;

            // shrink both src and dst to ensure they have k aspect ratio:            
            src = src.GetCropped(_GetCenterCrop(src.Width, src.Height, k));
            dst = dst.GetCropped(_GetCenterCrop(dst.Width, dst.Height, k));

            #if DEBUG
            srck = (float)src.Width / (float)src.Height;
            dstk = (float)dst.Width / (float)dst.Height;
            System.Diagnostics.Debug.Assert(Math.Abs(1f - srck / dstk) < 0.1f, "At this point the aspect ratio of both crops must be close enough");
            #endif

            IBinaryOperation<TSrcPixel, TDstPixel>.StretchToFit.Execute(src, dst, pixelConverter);
        }
        
        private static System.Drawing.Rectangle _GetCenterCrop(int width, int height, float aspect)
        {
            var k = (float)width /(float)height;

            if (k > aspect) // clip horizontally
            {
                var ww = height * aspect;
                var r = new System.Drawing.RectangleF((width - ww) / 2, 0, ww, height);
                return System.Drawing.Rectangle.Truncate(r);
            }
            
            if (k < aspect) // clip vertically
            {
                var hh = width / aspect;
                var r = new System.Drawing.RectangleF(0, (height - hh) / 2, width, hh);
                return System.Drawing.Rectangle.Truncate(r);
            }

            return new System.Drawing.Rectangle(0, 0, width, height);
        }        
    }

    /// <summary>
    /// Operator that resizes and stretches the source to fit into destination.
    /// </summary>
    /// <typeparam name="TSrcPixel"></typeparam>
    /// <typeparam name="TDstPixel"></typeparam>
    readonly struct _StretchToFitOperator<TSrcPixel, TDstPixel>
        : IBinaryOperation<TSrcPixel, TDstPixel>        
        where TSrcPixel : unmanaged        
        where TDstPixel : unmanaged
    {
        public void Execute<TSrcBmp, TDstBmp>(TSrcBmp src, TDstBmp dst, IPixelConverter<TSrcPixel, TDstPixel> pixelConverter)
            where TSrcBmp : IReadOnlyBitmapOperand<TSrcBmp,TSrcPixel>
            #if NET9_0_OR_GREATER
            , allows ref struct
            #endif
            where TDstBmp : IBitmapOperand<TDstBmp, TDstPixel>
            #if NET9_0_OR_GREATER
            , allows ref struct
            #endif
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
        }
    }

}

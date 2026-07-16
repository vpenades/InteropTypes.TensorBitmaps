using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using InteropTypes.Numerics;
using InteropTypes.TensorBitmaps.Operands;

namespace InteropTypes.TensorBitmaps.Operators
{
    /// <summary>
    /// interface for operator that perform a pixel transfer between a source and destination bitmap
    /// </summary>
    /// <typeparam name="TSrcPixel">The pixel type of source</typeparam>
    /// <typeparam name="TDstPixel">The pixel type od destination</typeparam>
    public interface IBinaryOperation<TSrcPixel, TDstPixel, TResult>
        where TSrcPixel : unmanaged        
        where TDstPixel : unmanaged        
    {
        TResult Execute<TSrcBmp, TDstBmp>(TSrcBmp src, TDstBmp dst, bool initPixels = true)
            where TSrcBmp : IReadOnlyBitmapOperand<TSrcBmp, TSrcPixel>, allows ref struct
            where TDstBmp : IBitmapOperand<TDstBmp, TDstPixel>, allows ref struct
        {
            var pixelConverter = IPixelConverter<TSrcPixel, TDstPixel>.Create(src.Format, dst.Format, initPixels);
            return Execute(src, dst, pixelConverter);
        }

        TResult Execute<TSrcBmp,TDstBmp>(TSrcBmp src, TDstBmp dst, IPixelConverter<TSrcPixel, TDstPixel> pixelConverter)
            where TSrcBmp : IReadOnlyBitmapOperand<TSrcBmp,TSrcPixel>, allows ref struct
            where TDstBmp : IBitmapOperand<TDstBmp, TDstPixel>, allows ref struct;        

        public static IBinaryOperation<TSrcPixel, TDstPixel, Matrix3x2> GetScaleToFit(float overflowAmount)
        {
            return new _ScaleToFitOperator<TSrcPixel,TDstPixel>(overflowAmount, _StretchToFitOperator<TSrcPixel,TDstPixel>.Instance);
        }
    }    
    

    /// <summary>
    /// Operator that simply copies source over destination
    /// </summary>    
    readonly struct _DirectCopyOperator<TSrcPixel, TDstPixel>
            : IBinaryOperation<TSrcPixel, TDstPixel, int>
            where TSrcPixel : unmanaged            
            where TDstPixel : unmanaged
    {
        public static _DirectCopyOperator<TSrcPixel, TDstPixel> Instance { get; } = new _DirectCopyOperator<TSrcPixel, TDstPixel>();

        public int Execute<TSrcBmp, TDstBmp>(TSrcBmp src, TDstBmp dst, IPixelConverter<TSrcPixel, TDstPixel> pixelConverter)
            where TSrcBmp : IReadOnlyBitmapOperand<TSrcBmp,TSrcPixel>, allows ref struct
            where TDstBmp : IBitmapOperand<TDstBmp, TDstPixel>, allows ref struct
        {
            var h = Math.Min(src.Height, dst.Height);

            for (int y = 0; y < h; ++y)
            {
                var srcRow = src.GetRowPixelsSpan(y);
                var dstRow = dst.GetRowPixelsSpan(y);
                pixelConverter.ConvertPixels(srcRow, dstRow);
            }

            return 0;
        }
    }

    /// <summary>
    /// Operator that resizes and crops the source so it fits into destination while preserving aspect ration.
    /// </summary>    
    readonly struct _ScaleToFitOperator<TSrcPixel, TDstPixel>
        : IBinaryOperation<TSrcPixel, TDstPixel, Matrix3x2>        
        where TSrcPixel : unmanaged        
        where TDstPixel : unmanaged
    {
        

        public _ScaleToFitOperator(float overflowAmount, IBinaryOperation<TSrcPixel, TDstPixel, Matrix3x2> stretchOperator)
        {
            _OverflowAmount = overflowAmount;
            _StretchOperator = stretchOperator;
        }

        /// <summary>
        /// Represents the allowed overflow amount
        /// </summary>
        /// <remarks>
        /// A value of 0 means no overflow allowed, the source bitmap will shrink to completely fit into destionation.<br/>
        /// A value of 1 menas full overflow is allowed, the source bitmap will shrink enough to completely fill the destination,
        /// allowing parts of the source bitmap to overflow the destination.
        /// </remarks>
        private readonly float _OverflowAmount;

        private readonly IBinaryOperation<TSrcPixel, TDstPixel, Matrix3x2> _StretchOperator;

        public Matrix3x2 Execute<TSrcBmp, TDstBmp>(TSrcBmp src, TDstBmp dst, IPixelConverter<TSrcPixel, TDstPixel> pixelConverter)
            where TSrcBmp : IReadOnlyBitmapOperand<TSrcBmp,TSrcPixel>, allows ref struct
            where TDstBmp : IBitmapOperand<TDstBmp, TDstPixel>, allows ref struct
        {
            // calculate aspect ratios:
            var srck = (float)src.Width / (float)src.Height;
            var dstk = (float)dst.Width / (float)dst.Height;

            // lerp aspect ratios using allowed overflow amount
            var k = srck * (1 - _OverflowAmount) + dstk * _OverflowAmount;

            // shrink both src and dst to ensure they have k aspect ratio:            
            var srcr = _GetCenterCrop(src.Width, src.Height, k);
            src = src.GetCropped(srcr);
            var dstr = _GetCenterCrop(dst.Width, dst.Height, k);
            dst = dst.GetCropped(dstr);

            #if DEBUG
            srck = (float)src.Width / (float)src.Height;
            dstk = (float)dst.Width / (float)dst.Height;
            System.Diagnostics.Debug.Assert(Math.Abs(1f - srck / dstk) < 0.1f, "At this point the aspect ratio of both crops must be close enough");
            #endif

            var transform = _StretchOperator.Execute(src, dst, pixelConverter);

            transform = Matrix3x2.CreateTranslation(-dstr.X, -dstr.Y) * transform;

            transform *= Matrix3x2.CreateTranslation(srcr.X, srcr.Y);

            return transform;
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
        : IBinaryOperation<TSrcPixel, TDstPixel, Matrix3x2>        
        where TSrcPixel : unmanaged        
        where TDstPixel : unmanaged
    {
        public static _StretchToFitOperator<TSrcPixel, TDstPixel> Instance { get; } = new _StretchToFitOperator<TSrcPixel, TDstPixel>();

        public Matrix3x2 Execute<TSrcBmp, TDstBmp>(TSrcBmp src, TDstBmp dst, IPixelConverter<TSrcPixel, TDstPixel> pixelConverter)
            where TSrcBmp : IReadOnlyBitmapOperand<TSrcBmp,TSrcPixel>, allows ref struct
            where TDstBmp : IBitmapOperand<TDstBmp, TDstPixel>, allows ref struct
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

            return Matrix3x2.CreateScale(src.Width / (float)dst.Width, src.Height / (float)dst.Height);
        }
    }

}

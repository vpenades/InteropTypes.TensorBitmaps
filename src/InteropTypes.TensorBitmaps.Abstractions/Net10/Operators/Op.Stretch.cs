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
    /// Operator that resizes and crops the source so it fits into destination while preserving aspect ration.
    /// </summary>    
    readonly struct _ScaleToFitOperator<TSrcPixel, TDstPixel>
        : IBitmapOperation<TSrcPixel, TDstPixel, Matrix3x2>        
        where TSrcPixel : unmanaged        
        where TDstPixel : unmanaged
    {
        public _ScaleToFitOperator(float overflowAmount, IBitmapOperation<TSrcPixel, TDstPixel, Matrix3x2> stretchOperator)
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

        private readonly IBitmapOperation<TSrcPixel, TDstPixel, Matrix3x2> _StretchOperator;

        public Matrix3x2 Apply<TSrcBmp, TDstBmp>(TSrcBmp src, TDstBmp dst, IPixelConverter<TSrcPixel, TDstPixel> pixelConverter)
            where TSrcBmp : IReadOnlyBitmap<TSrcBmp,TSrcPixel>, allows ref struct
            where TDstBmp : IBitmap<TDstBmp, TDstPixel>, allows ref struct
        {
            var l = new System.Drawing.Size(src.Width, src.Height);
            var r = new System.Drawing.Size(dst.Width, dst.Height);
            var crops = ScaledIntersectionCrop.CreateFrom(l, r, _OverflowAmount);

            src = src.GetCropped(crops.SourceCrop);
            dst = dst.GetCropped(crops.TargetCrop);

            var xform = _StretchOperator.Apply(src, dst, pixelConverter);

            return crops.GetTransform(xform);
        }        
    }

    /// <summary>
    /// Operator that resizes and stretches the source to fit into destination.
    /// </summary>
    /// <typeparam name="TSrcPixel"></typeparam>
    /// <typeparam name="TDstPixel"></typeparam>
    readonly struct _StretchToFitOperator<TSrcPixel, TDstPixel>
        : IBitmapOperation<TSrcPixel, TDstPixel, Matrix3x2>        
        where TSrcPixel : unmanaged        
        where TDstPixel : unmanaged
    {
        public static _StretchToFitOperator<TSrcPixel, TDstPixel> Instance { get; } = new _StretchToFitOperator<TSrcPixel, TDstPixel>();

        

        public Matrix3x2 Apply<TSrcBmp, TDstBmp>(TSrcBmp src, TDstBmp dst, IPixelConverter<TSrcPixel, TDstPixel> pixelConverter)
            where TSrcBmp : IReadOnlyBitmap<TSrcBmp,TSrcPixel>, allows ref struct
            where TDstBmp : IBitmap<TDstBmp, TDstPixel>, allows ref struct
        {
            Span<TSrcPixel> srcRow = stackalloc TSrcPixel[src.Width];
            Span<TSrcPixel> tmpRow = stackalloc TSrcPixel[dst.Width];
            Span<TDstPixel> dstRow = stackalloc TDstPixel[dst.Width];

            int dsth = dst.Height;
            int dstw = dst.Width;

            for (int y = 0; y < dsth; ++y)
            {
                var yy = (2 * y + 1) * src.Height / (2 * dsth);

                src.ReadRowPixelsSpan(yy, 0, srcRow);

                for (int x = 0; x < dstw; ++x)
                {
                    var xx = (2 * x + 1) * srcRow.Length / (2 * dstw);

                    tmpRow[x] = srcRow[xx];
                }
                    
                pixelConverter.ConvertPixels(tmpRow, dstRow);

                dst.WriteRowPixelsSpan(y, 0, dstRow);
            }            

            return Matrix3x2.CreateScale(src.Width / (float)dst.Width, src.Height / (float)dst.Height);
        }
    }

}

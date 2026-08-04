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
            where TSrcPixel : unmanaged            
            where TDstPixel : unmanaged
    {
        public static _DirectCopyOperator<TSrcPixel, TDstPixel> Instance { get; } = new _DirectCopyOperator<TSrcPixel, TDstPixel>();

        public int Execute<TSrcBmp, TDstBmp>(TSrcBmp src, TDstBmp dst, IPixelConverter<TSrcPixel, TDstPixel> pixelConverter)
            where TSrcBmp : IReadOnlyBitmap<TSrcBmp,TSrcPixel>, allows ref struct
            where TDstBmp : IBitmap<TDstBmp, TDstPixel>, allows ref struct
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
        : IDrawOperation<TSrcPixel, TDstPixel, Matrix3x2>        
        where TSrcPixel : unmanaged        
        where TDstPixel : unmanaged
    {
        public _ScaleToFitOperator(float overflowAmount, IDrawOperation<TSrcPixel, TDstPixel, Matrix3x2> stretchOperator)
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

        private readonly IDrawOperation<TSrcPixel, TDstPixel, Matrix3x2> _StretchOperator;

        public Matrix3x2 Execute<TSrcBmp, TDstBmp>(TSrcBmp src, TDstBmp dst, IPixelConverter<TSrcPixel, TDstPixel> pixelConverter)
            where TSrcBmp : IReadOnlyBitmap<TSrcBmp,TSrcPixel>, allows ref struct
            where TDstBmp : IBitmap<TDstBmp, TDstPixel>, allows ref struct
        {
            var l = new System.Drawing.Size(src.Width, src.Height);
            var r = new System.Drawing.Size(dst.Width, dst.Height);
            var crops = ScaledIntersectionCrop.CreateFrom(l, r, _OverflowAmount);

            src = src.GetCropped(crops.SourceCrop);
            dst = dst.GetCropped(crops.TargetCrop);

            var xform = _StretchOperator.Execute(src, dst, pixelConverter);

            return crops.GetTransform(xform);
        }        
    }

    /// <summary>
    /// Operator that resizes and stretches the source to fit into destination.
    /// </summary>
    /// <typeparam name="TSrcPixel"></typeparam>
    /// <typeparam name="TDstPixel"></typeparam>
    readonly struct _StretchToFitOperator<TSrcPixel, TDstPixel>
        : IDrawOperation<TSrcPixel, TDstPixel, Matrix3x2>        
        where TSrcPixel : unmanaged        
        where TDstPixel : unmanaged
    {
        public static _StretchToFitOperator<TSrcPixel, TDstPixel> Instance { get; } = new _StretchToFitOperator<TSrcPixel, TDstPixel>();

        public Matrix3x2 Execute<TSrcBmp, TDstBmp>(TSrcBmp src, TDstBmp dst, IPixelConverter<TSrcPixel, TDstPixel> pixelConverter)
            where TSrcBmp : IReadOnlyBitmap<TSrcBmp,TSrcPixel>, allows ref struct
            where TDstBmp : IBitmap<TDstBmp, TDstPixel>, allows ref struct
        {
            if (src.TryCastTo<IReadOnlyBitmap<TSrcPixel>>(out var srcManaged) &&
                IClientReadOnlyBitmap<TSrcPixel>.TryCreateStretched(srcManaged, new System.Drawing.Size(dst.Width,dst.Height), out var stretchedBitmap))
            {
                System.Diagnostics.Debug.Assert(dst.Width == stretchedBitmap.Width);
                System.Diagnostics.Debug.Assert(dst.Height == stretchedBitmap.Height);

                for (int y = 0; y < dst.Height; ++y)
                {
                    var srcRow = stretchedBitmap.GetRowPixelsSpan(y);

                    var dstRow = dst.GetRowPixelsSpan(y);
                    pixelConverter.ConvertPixels(srcRow, dstRow);
                }
                stretchedBitmap.Dispose();
            }
            
            else
            {
                Span<TSrcPixel> tmpRow = stackalloc TSrcPixel[dst.Width];

                for (int y = 0; y < dst.Height; ++y)
                {
                    var yy = (2 * y + 1) * src.Height / (2 * dst.Height);

                    var srcRow = src.GetRowPixelsSpan(yy);

                    for (int x = 0; x < tmpRow.Length; ++x)
                    {
                        var xx = (2 * x + 1) * srcRow.Length / (2 * tmpRow.Length);

                        tmpRow[x] = srcRow[xx];
                    }

                    var dstRow = dst.GetRowPixelsSpan(y);
                    pixelConverter.ConvertPixels(tmpRow, dstRow);
                }
            }

            return Matrix3x2.CreateScale(src.Width / (float)dst.Width, src.Height / (float)dst.Height);
        }
    }

}

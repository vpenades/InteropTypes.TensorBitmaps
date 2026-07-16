using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace InteropTypes.TensorBitmaps
{
    public static class PixelsTransform
    {
        public static PixelsTransform<int> Copy { get; } = new _DirectCopy();

        public static PixelsTransform<Matrix3x2> StretchToFit { get; } = new _StretchToFit();

        public static PixelsTransform<Matrix3x2> ScaleToFit(float overflowAmount) { return new _ScaleToFit(overflowAmount); }

        sealed class _DirectCopy : PixelsTransform<int>
        {
            public override BITMAPOPERATORS.IBinaryOperation<TSrcPixel, TDstPixel, int> GetInstance<TSrcPixel, TDstPixel>()
            {
                return BITMAPOPERATORS._DirectCopyOperator<TSrcPixel, TDstPixel>.Instance;
            }
        }

        private sealed class _StretchToFit : PixelsTransform<Matrix3x2>
        {
            public override BITMAPOPERATORS.IBinaryOperation<TSrcPixel, TDstPixel, Matrix3x2> GetInstance<TSrcPixel, TDstPixel>()
            {
                return BITMAPOPERATORS._StretchToFitOperator<TSrcPixel, TDstPixel>.Instance;
            }
        }

        private sealed class _ScaleToFit : PixelsTransform<Matrix3x2>
        {
            public _ScaleToFit(float overflowAmount)
            {
                _overflowAmount = overflowAmount;
            }

            private readonly float _overflowAmount;

            public override BITMAPOPERATORS.IBinaryOperation<TSrcPixel, TDstPixel, Matrix3x2> GetInstance<TSrcPixel, TDstPixel>()
            {
                return BITMAPOPERATORS.IBinaryOperation<TSrcPixel, TDstPixel, Matrix3x2>.GetScaleToFit(_overflowAmount);
            }
        }        
    }

    /// <summary>
    /// represents a pixel transformation to be applyed when copying pixels from one bitmap to another.
    /// </summary>
    /// <remarks>
    /// Used by  using <see cref="ReadOnlyTensorSpanBitmap{TElement, TPixel}.CopyPixelsTo{TDstElement, TDstPixel}(PixelsTransform, TensorSpanBitmap{TDstElement, TDstPixel}, bool)"/>
    /// </remarks>
    public abstract class PixelsTransform<TResult>
    {
        public abstract BITMAPOPERATORS.IBinaryOperation<TSrcPixel, TDstPixel, TResult> GetInstance<TSrcPixel, TDstPixel>()
            where TSrcPixel : unmanaged
            where TDstPixel : unmanaged;
    }

    
}

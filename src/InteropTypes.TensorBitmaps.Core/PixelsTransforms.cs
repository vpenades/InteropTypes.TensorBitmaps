using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace InteropTypes.TensorBitmaps
{
    public abstract class PixelsTransform
    {
        public static PixelsTransform<int> Copy { get; } = new _DirectCopy();

        public static PixelsTransform<Matrix3x2> StretchToFit { get; } = new _StretchToFit();

        public static PixelsTransform<Matrix3x2> ScaleToFit(float overflowAmount) { return new _ScaleToFit(overflowAmount); }

        #if NET9_0_OR_GREATER        

        sealed class _DirectCopy : PixelsTransform<int>
        {
            internal override InteropTypes.Numerics.BitmapOperators.IBinaryOperation<TSrcPixel, TDstPixel, int> GetInstance<TSrcPixel, TDstPixel>()
            {
                return InteropTypes.Numerics.BitmapOperators.IBinaryOperation<TSrcPixel, TDstPixel, int>.DirectCopy;
            }
        }

        private sealed class _StretchToFit : PixelsTransform<Matrix3x2>
        {
            internal override InteropTypes.Numerics.BitmapOperators.IBinaryOperation<TSrcPixel, TDstPixel, Matrix3x2> GetInstance<TSrcPixel, TDstPixel>()
            {
                return InteropTypes.Numerics.BitmapOperators.IBinaryOperation<TSrcPixel, TDstPixel, Matrix3x2>.StretchToFit;
            }
        }

        private sealed class _ScaleToFit : PixelsTransform<Matrix3x2>
        {
            public _ScaleToFit(float overflowAmount)
            {
                _overflowAmount = overflowAmount;
            }

            private readonly float _overflowAmount;

            internal override InteropTypes.Numerics.BitmapOperators.IBinaryOperation<TSrcPixel, TDstPixel, Matrix3x2> GetInstance<TSrcPixel, TDstPixel>()
            {
                return InteropTypes.Numerics.BitmapOperators.IBinaryOperation<TSrcPixel, TDstPixel, Matrix3x2>.GetScaleToFit(_overflowAmount);
            }
        }

        #else        

        sealed class _DirectCopy : PixelsTransform<int>
        {
            internal override ITensorSpanBitmapBinaryOperation<TSrcPixel, TDstPixel, int> GetInstance<TSrcPixel, TDstPixel>()
            {
                return new _DirectCopyOperator<TSrcPixel, TDstPixel>();
            }
        }

        private sealed class _StretchToFit : PixelsTransform<Matrix3x2>
        {
            internal override ITensorSpanBitmapBinaryOperation<TSrcPixel, TDstPixel, Matrix3x2> GetInstance<TSrcPixel, TDstPixel>()
            {
                return new _StretchToFitOperator<TSrcPixel, TDstPixel>();
            }
        }        

        private sealed class _ScaleToFit : PixelsTransform<Matrix3x2>
        {
            private readonly float _overflowAmount;

            public _ScaleToFit(float overflowAmount)
            {
                _overflowAmount = overflowAmount;
            }

            internal override ITensorSpanBitmapBinaryOperation<TSrcPixel, TDstPixel, Matrix3x2> GetInstance<TSrcPixel, TDstPixel>()
            {
                return new _ScaleToFitOperator<TSrcPixel, TDstPixel>(_overflowAmount);
            }
        }

        #endif
    }


    /// <summary>
    /// represents a pixel transformation to be applyed when copying pixels from one bitmap to another.
    /// </summary>
    /// <remarks>
    /// Used by  using <see cref="ReadOnlyTensorSpanBitmap{TElement, TPixel}.CopyPixelsTo{TDstElement, TDstPixel}(PixelsTransform, TensorSpanBitmap{TDstElement, TDstPixel}, bool)"/>
    /// </remarks>
    public abstract class PixelsTransform<TResult>
    {

        #if NET9_0_OR_GREATER

        internal abstract InteropTypes.Numerics.BitmapOperators.IBinaryOperation<TSrcPixel, TDstPixel, TResult> GetInstance<TSrcPixel, TDstPixel>()
            where TSrcPixel : unmanaged
            where TDstPixel : unmanaged;

        #else

        internal abstract ITensorSpanBitmapBinaryOperation<TSrcPixel, TDstPixel, TResult> GetInstance<TSrcPixel, TDstPixel>()            
            where TSrcPixel : unmanaged            
            where TDstPixel : unmanaged;

        #endif
    }


}

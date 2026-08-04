using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace InteropTypes.TensorBitmaps
{
    public static class BitmapOperations
    {
        public static BitmapBinaryOperation<int> Copy { get; } = new _DirectCopy();

        public static BitmapBinaryOperation<Matrix3x2> StretchToFitStep { get; } = new _StretchToFitStep();

        public static BitmapBinaryOperation<Matrix3x2> StretchToFitBicubic { get; } = new _StretchToFitBicubic();

        public static BitmapBinaryOperation<Matrix3x2> StretchToFitLanczos { get; } = new _StretchToFitLanczos3();

        public static BitmapBinaryOperation<Matrix3x2> ScaleToFit(float overflowAmount, BitmapBinaryOperation<Matrix3x2> stretch = null) { return new _ScaleToFit(overflowAmount, stretch); }

        sealed class _DirectCopy : BitmapBinaryOperation<int>
        {
            public override BITMAPOPERATORS.IDrawOperation<TSrcPixel, TDstPixel, int> GetInstance<TSrcPixel, TDstPixel>()
            {
                return BITMAPOPERATORS._DirectCopyOperator<TSrcPixel, TDstPixel>.Instance;
            }
        }

        private sealed class _StretchToFitStep : BitmapBinaryOperation<Matrix3x2>
        {
            public override BITMAPOPERATORS.IDrawOperation<TSrcPixel, TDstPixel, Matrix3x2> GetInstance<TSrcPixel, TDstPixel>()
            {
                return BITMAPOPERATORS._StretchToFitOperator<TSrcPixel, TDstPixel>.Instance;
            }
        }

        private sealed class _StretchToFitBicubic : BitmapBinaryOperation<Matrix3x2>
        {
            public override BITMAPOPERATORS.IDrawOperation<TSrcPixel, TDstPixel, Matrix3x2> GetInstance<TSrcPixel, TDstPixel>()
            {
                return BITMAPOPERATORS._InterpolatedStretchToFitOperator<TSrcPixel, TDstPixel>.CreateBicubic();
            }
        }

        private sealed class _StretchToFitLanczos3 : BitmapBinaryOperation<Matrix3x2>
        {
            public override BITMAPOPERATORS.IDrawOperation<TSrcPixel, TDstPixel, Matrix3x2> GetInstance<TSrcPixel, TDstPixel>()
            {
                return BITMAPOPERATORS._InterpolatedStretchToFitOperator<TSrcPixel, TDstPixel>.CreateLanczos3();
            }
        }

        private sealed class _ScaleToFit : BitmapBinaryOperation<Matrix3x2>
        {
            public _ScaleToFit(float overflowAmount, BitmapBinaryOperation<Matrix3x2> stretch = null)
            {
                _overflowAmount = overflowAmount;
                _Stretch = stretch ?? StretchToFitStep;
            }

            private readonly float _overflowAmount;
            private readonly BitmapBinaryOperation<Matrix3x2> _Stretch;

            public override BITMAPOPERATORS.IDrawOperation<TSrcPixel, TDstPixel, Matrix3x2> GetInstance<TSrcPixel, TDstPixel>()
            {
                return BITMAPOPERATORS.IDrawOperation<TSrcPixel, TDstPixel, Matrix3x2>.GetScaleToFit(_overflowAmount, _Stretch.GetInstance<TSrcPixel,TDstPixel>());
            }
        }        
    }

    /// <summary>
    /// represents a pixel transformation to be applyed when copying pixels from one bitmap to another.
    /// </summary>
    /// <remarks>
    /// Used by  using <see cref="ReadOnlyTensorSpanBitmap{TElement, TPixel}.CopyPixelsTo{TDstElement, TDstPixel}(BitmapOperations, TensorSpanBitmap{TDstElement, TDstPixel}, bool)"/>
    /// </remarks>
    public abstract class BitmapBinaryOperation<TResult>
    {
        public abstract BITMAPOPERATORS.IDrawOperation<TSrcPixel, TDstPixel, TResult> GetInstance<TSrcPixel, TDstPixel>()
            where TSrcPixel : unmanaged
            where TDstPixel : unmanaged;
    }


    public abstract class BitmapFillOperation<TResult>
    {
        public abstract BITMAPOPERATORS.IFillOperation<TSrcPixel, TDstPixel, TResult> GetInstance<TSrcPixel, TDstPixel>()
            where TSrcPixel : unmanaged
            where TDstPixel : unmanaged;
    }


}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using InteropTypes.TensorBitmaps.Operators;

namespace InteropTypes.TensorBitmaps
{
    public static class BitmapOperations
    {
        public static BitmapDrawOperation<int> DrawCopy { get; } = new _DrawCopy();

        public static BitmapFillOperation<int> FillCopy { get; } = new _FillCopy();

        public static BitmapFillOperation<Matrix3x2> StretchToFitStep { get; } = new _StretchToFitStep();

        public static BitmapFillOperation<Matrix3x2> StretchToFitBicubic { get; } = new _StretchToFitBicubic();

        public static BitmapFillOperation<Matrix3x2> StretchToFitLanczos { get; } = new _StretchToFitLanczos3();

        public static BitmapFillOperation<Matrix3x2> ScaleToFit(float overflowAmount, BitmapFillOperation<Matrix3x2> stretch = null) { return new _ScaleToFit(overflowAmount, stretch); }

        sealed class _DrawCopy : BitmapDrawOperation<int>
        {
            public override BITMAPOPERATORS.IDrawOperation<TSrcPixel, TDstPixel, int> GetInstance<TSrcPixel, TDstPixel>()
            {
                return BITMAPOPERATORS._DirectCopyOperator<TSrcPixel, TDstPixel>.Instance;
            }
        }

        sealed class _FillCopy : BitmapFillOperation<int>
        {
            public override BITMAPOPERATORS.IFillOperation<TSrcPixel, TDstPixel, int> GetInstance<TSrcPixel, TDstPixel>()
            {
                return BITMAPOPERATORS._DirectCopyOperator<TSrcPixel, TDstPixel>.Instance;
            }
        }

        private sealed class _StretchToFitStep : BitmapFillOperation<Matrix3x2>
        {
            public override BITMAPOPERATORS.IFillOperation<TSrcPixel, TDstPixel, Matrix3x2> GetInstance<TSrcPixel, TDstPixel>()
            {
                return BITMAPOPERATORS._StretchToFitOperator<TSrcPixel, TDstPixel>.Instance;
            }
        }

        private sealed class _StretchToFitBicubic : BitmapFillOperation<Matrix3x2>
        {
            public override BITMAPOPERATORS.IFillOperation<TSrcPixel, TDstPixel, Matrix3x2> GetInstance<TSrcPixel, TDstPixel>()
            {
                return BITMAPOPERATORS._InterpolatedStretchToFitOperator<TSrcPixel, TDstPixel>.CreateBicubic();
            }
        }

        private sealed class _StretchToFitLanczos3 : BitmapFillOperation<Matrix3x2>
        {
            public override BITMAPOPERATORS.IFillOperation<TSrcPixel, TDstPixel, Matrix3x2> GetInstance<TSrcPixel, TDstPixel>()
            {
                return BITMAPOPERATORS._InterpolatedStretchToFitOperator<TSrcPixel, TDstPixel>.CreateLanczos3();
            }
        }

        private sealed class _ScaleToFit : BitmapFillOperation<Matrix3x2>
        {
            public _ScaleToFit(float overflowAmount, BitmapFillOperation<Matrix3x2> stretch = null)
            {
                _overflowAmount = overflowAmount;
                _Stretch = stretch ?? StretchToFitStep;
            }

            private readonly float _overflowAmount;
            private readonly BitmapFillOperation<Matrix3x2> _Stretch;

            public override BITMAPOPERATORS.IFillOperation<TSrcPixel, TDstPixel, Matrix3x2> GetInstance<TSrcPixel, TDstPixel>()
            {
                var stretcher = _Stretch.GetInstance<TSrcPixel, TDstPixel>();

                return new _ScaleToFitOperator<TSrcPixel, TDstPixel>(_overflowAmount, stretcher);
            }
        }        
    }

    /// <summary>
    /// represents a pixel transformation to be applyed when copying pixels from one bitmap to another.
    /// </summary>
    /// <remarks>
    /// Used by  using <see cref="ReadOnlyTensorSpanBitmap{TElement, TPixel}.CopyPixelsTo{TDstElement, TDstPixel}(BitmapOperations, TensorSpanBitmap{TDstElement, TDstPixel}, bool)"/>
    /// </remarks>
    public abstract class BitmapDrawOperation<TResult>
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

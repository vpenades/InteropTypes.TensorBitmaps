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
        public static PixelsTransformFrom<TSrcPixel,int> CopyFrom<TSrcPixel>()
            where TSrcPixel: unmanaged
        {
            return new PixelsTransformFrom<TSrcPixel, int>(Copy);
        }

        public static PixelsTransform<int> Copy { get; } = new _DirectCopy();

        public static PixelsTransform<Matrix3x2> StretchToFit { get; } = new _StretchToFit();

        public static PixelsTransform<Matrix3x2> ScaleToFit(float overflowAmount) { return new _ScaleToFit(overflowAmount); }

        
        public static PixelsTransformTo<TDstPixel,int> GetCopyTransform<TDstPixel>()
            where TDstPixel:unmanaged
        {
            return new PixelsTransformTo<TDstPixel, int>(Copy);
        }

        sealed class _DirectCopy : PixelsTransform<int>
        {
            public override BITMAPOPERATORS.IBinaryOperation<TSrcPixel, TDstPixel, int> GetInstance<TSrcPixel, TDstPixel>()
            {
                return BITMAPOPERATORS.IBinaryOperation<TSrcPixel, TDstPixel, int>.DirectCopy;
            }
        }

        private sealed class _StretchToFit : PixelsTransform<Matrix3x2>
        {
            public override BITMAPOPERATORS.IBinaryOperation<TSrcPixel, TDstPixel, Matrix3x2> GetInstance<TSrcPixel, TDstPixel>()
            {
                return BITMAPOPERATORS.IBinaryOperation<TSrcPixel, TDstPixel, Matrix3x2>.StretchToFit;
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

    public readonly struct PixelsTransformFrom<TSrcPixel, TResult>
        where TSrcPixel : unmanaged
    {
        public PixelsTransformFrom(PixelsTransform<TResult> transform)
        {
            _Transform = transform;
        }

        private readonly PixelsTransform<TResult> _Transform;

        public BITMAPOPERATORS.IBinaryOperation<TSrcPixel, TDstPixel, TResult> GetInstance<TDstPixel>()
            where TDstPixel : unmanaged
        {
            return _Transform.GetInstance<TSrcPixel, TDstPixel>();
        }
    }

    public readonly struct PixelsTransformTo<TDstPixel, TResult>
        where TDstPixel : unmanaged
    {
        public PixelsTransformTo(PixelsTransform<TResult> transform)
        {
            _Transform = transform;
        }

        private readonly PixelsTransform<TResult> _Transform;        

        public BITMAPOPERATORS.IBinaryOperation<TSrcPixel, TDstPixel, TResult> GetInstance<TSrcPixel>()
            where TSrcPixel : unmanaged
        {
            return _Transform.GetInstance<TSrcPixel, TDstPixel>();
        }
    }
}

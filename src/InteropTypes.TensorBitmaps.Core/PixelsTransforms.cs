using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace InteropTypes.TensorBitmaps
{

    /// <summary>
    /// represents a pixel transformation to be applyed when copying pixels from one bitmap to another.
    /// </summary>
    /// <remarks>
    /// Used by  using <see cref="ReadOnlyTensorSpanBitmap{TElement, TPixel}.CopyPixelsTo{TDstElement, TDstPixel}(PixelsTransform, TensorSpanBitmap{TDstElement, TDstPixel}, bool)"/>
    /// </remarks>
    public abstract class PixelsTransform
    {
        public static PixelsTransform Copy { get; } = new _DirectCopy();

        public static PixelsTransform StretchToFit { get; } = new _StretchToFit();

        public static PixelsTransform ScaleToFit(float overflowAmount)
        {
            return new _ScaleToFit(overflowAmount);
        }

        #if NET9_0_OR_GREATER

        internal abstract InteropTypes.Numerics.BitmapOperators.IBinaryOperation<TSrcPixel, TDstPixel> GetInstance<TSrcPixel, TDstPixel>()            
            where TSrcPixel : unmanaged            
            where TDstPixel : unmanaged;

        sealed class _DirectCopy : PixelsTransform
        {
            internal override InteropTypes.Numerics.BitmapOperators.IBinaryOperation<TSrcPixel, TDstPixel> GetInstance<TSrcPixel, TDstPixel>()
            {
                return InteropTypes.Numerics.BitmapOperators.IBinaryOperation<TSrcPixel, TDstPixel>.DirectCopy;
            }
        }

        private sealed class _StretchToFit : PixelsTransform
        {
            internal override InteropTypes.Numerics.BitmapOperators.IBinaryOperation<TSrcPixel, TDstPixel> GetInstance<TSrcPixel, TDstPixel>()
            {
                return InteropTypes.Numerics.BitmapOperators.IBinaryOperation<TSrcPixel, TDstPixel>.StretchToFit;
            }
        }        

        private sealed class _ScaleToFit : PixelsTransform
        {
            public _ScaleToFit(float overflowAmount)
            {
                _overflowAmount = overflowAmount;
            }

            private readonly float _overflowAmount;

            

            internal override InteropTypes.Numerics.BitmapOperators.IBinaryOperation<TSrcPixel, TDstPixel> GetInstance<TSrcPixel, TDstPixel>()
            {
                return InteropTypes.Numerics.BitmapOperators.IBinaryOperation<TSrcPixel, TDstPixel>.ScaleToFit(_overflowAmount);
            }
        }

        #else

        internal abstract ITensorSpanBitmapBinaryOperation<TSrcPixel, TDstPixel> GetInstance<TSrcPixel, TDstPixel>()            
            where TSrcPixel : unmanaged            
            where TDstPixel : unmanaged;

        sealed class _DirectCopy : PixelsTransform
        {
            internal override ITensorSpanBitmapBinaryOperation<TSrcPixel, TDstPixel> GetInstance<TSrcPixel, TDstPixel>()
            {
                return new _DirectCopyOperator<TSrcPixel, TDstPixel>();
            }
        }

        private sealed class _StretchToFit : PixelsTransform
        {
            internal override ITensorSpanBitmapBinaryOperation<TSrcPixel, TDstPixel> GetInstance<TSrcPixel, TDstPixel>()
            {
                return new _StretchToFitOperator<TSrcPixel, TDstPixel>();
            }
        }        

        private sealed class _ScaleToFit : PixelsTransform
        {
            private readonly float _overflowAmount;

            public _ScaleToFit(float overflowAmount)
            {
                _overflowAmount = overflowAmount;
            }

            internal override ITensorSpanBitmapBinaryOperation<TSrcPixel, TDstPixel> GetInstance<TSrcPixel, TDstPixel>()
            {
                return new _ScaleToFitOperator<TSrcPixel, TDstPixel>(_overflowAmount);
            }
        }

        #endif

    }
}

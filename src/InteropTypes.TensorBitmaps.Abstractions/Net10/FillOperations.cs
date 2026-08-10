using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using InteropTypes.TensorBitmaps.Operators;

namespace InteropTypes.TensorBitmaps
{
    partial class FillOperations
    {
        public static BitmapOperationFactory<Matrix3x2> StretchToFitStep { get; } = new _StretchToFitStep();

        public static BitmapOperationFactory<Matrix3x2> StretchToFitBicubic { get; } = new _StretchToFitBicubic();

        public static BitmapOperationFactory<Matrix3x2> StretchToFitLanczos { get; } = new _StretchToFitLanczos3();

        public static BitmapOperationFactory<Matrix3x2> ScaleToFit(float overflowAmount, BitmapOperationFactory<Matrix3x2> stretch = null) { return new _ScaleToFit(overflowAmount, stretch); }        

        private sealed class _StretchToFitStep : BitmapOperationFactory<Matrix3x2>
        {
            public override BITMAPOPERATORS.IBitmapOperation<TSrcPixel, TDstPixel, Matrix3x2> GetInstance<TSrcPixel, TDstPixel>()
            {
                return BITMAPOPERATORS._StretchToFitOperator<TSrcPixel, TDstPixel>.Instance;
            }
        }

        private sealed class _StretchToFitBicubic : BitmapOperationFactory<Matrix3x2>
        {
            public override BITMAPOPERATORS.IBitmapOperation<TSrcPixel, TDstPixel, Matrix3x2> GetInstance<TSrcPixel, TDstPixel>()
            {
                return BITMAPOPERATORS._InterpolatedStretchToFitOperator<TSrcPixel, TDstPixel>.CreateBicubic();
            }
        }

        private sealed class _StretchToFitLanczos3 : BitmapOperationFactory<Matrix3x2>
        {
            public override BITMAPOPERATORS.IBitmapOperation<TSrcPixel, TDstPixel, Matrix3x2> GetInstance<TSrcPixel, TDstPixel>()
            {
                return BITMAPOPERATORS._InterpolatedStretchToFitOperator<TSrcPixel, TDstPixel>.CreateLanczos3();
            }
        }

        private sealed class _ScaleToFit : BitmapOperationFactory<Matrix3x2>
        {
            public _ScaleToFit(float overflowAmount, BitmapOperationFactory<Matrix3x2> stretch = null)
            {
                _overflowAmount = overflowAmount;
                _Stretch = stretch ?? StretchToFitStep;
            }

            private readonly float _overflowAmount;
            private readonly BitmapOperationFactory<Matrix3x2> _Stretch;

            public override BITMAPOPERATORS.IBitmapOperation<TSrcPixel, TDstPixel, Matrix3x2> GetInstance<TSrcPixel, TDstPixel>()
            {
                var stretcher = _Stretch.GetInstance<TSrcPixel, TDstPixel>();

                return new _ScaleToFitOperator<TSrcPixel, TDstPixel>(_overflowAmount, stretcher);
            }
        }        
    }

    
}

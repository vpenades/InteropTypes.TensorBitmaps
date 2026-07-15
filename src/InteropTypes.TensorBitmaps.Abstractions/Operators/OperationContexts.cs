using System;
using System.Collections.Generic;
using System.Text;

using InteropTypes.Numerics;
using InteropTypes.TensorBitmaps.Operands;

namespace InteropTypes.TensorBitmaps.Operators
{
    

    public readonly ref struct BinaryOperatorContext<TDstBitmap, TDstPixel, TSrcPixel, TResult>
        where TDstBitmap : IBitmapOperand<TDstBitmap, TDstPixel>, allows ref struct
        where TDstPixel : unmanaged
        where TSrcPixel : unmanaged
    {
        public BinaryOperatorContext(TDstBitmap dstBitmap, IBinaryOperation<TSrcPixel, TDstPixel, TResult> operation)
        {
            _DstBitmap = dstBitmap;
            _Operation = operation;
        }

        private readonly TDstBitmap _DstBitmap;
        private readonly IBinaryOperation<TSrcPixel, TDstPixel, TResult> _Operation;        

        public TResult ApplyFrom<TSrcBitmap>(TSrcBitmap srcBmp, bool initPixels = true)
            where TSrcBitmap : IReadOnlyBitmapOperand<TSrcBitmap,TSrcPixel>, allows ref struct
        {
            return _Operation.Execute(srcBmp, _DstBitmap, initPixels);
        }

        public TResult ApplyFrom<TSrcBitmap>(TSrcBitmap srcBmp, IPixelConverter<TSrcPixel,TDstPixel> pixelConverter)
            where TSrcBitmap : IReadOnlyBitmapOperand<TSrcBitmap, TSrcPixel>, allows ref struct
        {
            return _Operation.Execute(srcBmp, _DstBitmap, pixelConverter);
        }
    }
}

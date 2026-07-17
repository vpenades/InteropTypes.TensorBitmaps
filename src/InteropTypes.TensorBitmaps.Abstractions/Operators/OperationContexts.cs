using System;
using System.Collections.Generic;
using System.Text;

using InteropTypes.Numerics;
using InteropTypes.TensorBitmaps.Operands;

namespace InteropTypes.TensorBitmaps.Operators
{
    public readonly ref struct BinaryOperatorContext<TBitmap, TPixel, TContextPixel>
        where TBitmap : IBitmapOperand<TBitmap, TPixel>, allows ref struct
        where TPixel : unmanaged
        where TContextPixel : unmanaged
    {
        public BinaryOperatorContext(TBitmap dstBitmap)
        {
            _DstBitmap = dstBitmap;            
        }

        private readonly TBitmap _DstBitmap;        

        public TResult Fill<TSrcBitmap,TResult>(BitmapBinaryOperation<TResult> transform, TSrcBitmap srcBmp, bool initPixels = true)
            where TSrcBitmap : IReadOnlyBitmapOperand<TSrcBitmap, TContextPixel>, allows ref struct
        {
            return transform.GetInstance<TContextPixel, TPixel>().Execute(srcBmp, _DstBitmap, initPixels);
        }

        public TResult Fill<TSrcBitmap, TResult>(BitmapBinaryOperation<TResult> transform, TSrcBitmap srcBmp, IPixelConverter<TContextPixel, TPixel> pixelConverter)
            where TSrcBitmap : IReadOnlyBitmapOperand<TSrcBitmap, TContextPixel>, allows ref struct
        {
            return transform.GetInstance<TContextPixel, TPixel>().Execute(srcBmp, _DstBitmap, pixelConverter);
        }
    }
    
}

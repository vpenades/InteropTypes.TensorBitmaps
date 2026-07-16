using System;
using System.Collections.Generic;
using System.Text;

using InteropTypes.Numerics;
using InteropTypes.TensorBitmaps.Operands;

namespace InteropTypes.TensorBitmaps.Operators
{
    public readonly ref struct BinaryOperatorContext<TDstBitmap, TDstPixel, TSrcPixel>
        where TDstBitmap : IBitmapOperand<TDstBitmap, TDstPixel>, allows ref struct
        where TDstPixel : unmanaged
        where TSrcPixel : unmanaged
    {
        public BinaryOperatorContext(TDstBitmap dstBitmap)
        {
            _DstBitmap = dstBitmap;            
        }

        private readonly TDstBitmap _DstBitmap;        

        public TResult Fill<TSrcBitmap,TResult>(PixelsTransform<TResult> transform, TSrcBitmap srcBmp, bool initPixels = true)
            where TSrcBitmap : IReadOnlyBitmapOperand<TSrcBitmap, TSrcPixel>, allows ref struct
        {
            return transform.GetInstance<TSrcPixel, TDstPixel>().Execute(srcBmp, _DstBitmap, initPixels);
        }

        public TResult Fill<TSrcBitmap, TResult>(PixelsTransform<TResult> transform, TSrcBitmap srcBmp, IPixelConverter<TSrcPixel, TDstPixel> pixelConverter)
            where TSrcBitmap : IReadOnlyBitmapOperand<TSrcBitmap, TSrcPixel>, allows ref struct
        {
            return transform.GetInstance<TSrcPixel, TDstPixel>().Execute(srcBmp, _DstBitmap, pixelConverter);
        }
    }
    
}

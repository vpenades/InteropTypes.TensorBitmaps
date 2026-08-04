using System;
using System.Collections.Generic;
using System.Text;

using InteropTypes.Numerics;
using InteropTypes.TensorBitmaps.Operands;

namespace InteropTypes.TensorBitmaps.Operators
{
    public readonly ref struct BinaryOperatorContext<TBitmap, TPixel, TContextPixel>
        where TBitmap : IBitmap<TBitmap, TPixel>, allows ref struct
        where TPixel : unmanaged
        where TContextPixel : unmanaged
    {
        public BinaryOperatorContext(TBitmap dstBitmap)
        {
            _DstBitmap = dstBitmap;            
        }

        private readonly TBitmap _DstBitmap;

        public void Fill(IReadOnlyBitmap<TContextPixel> srcBmp, bool initPixels = true)
        {
            var cvt = IPixelConverter<TContextPixel, TPixel>.Create(srcBmp.Format, _DstBitmap.Format, initPixels);

            var w = Math.Min(srcBmp.Width, _DstBitmap.Width);
            var h = Math.Min(srcBmp.Height, _DstBitmap.Height);

            for (int y = 0; y < h; ++y)
            {
                var sr = srcBmp.GetRowPixelsSpan(y).Slice(0, w);
                var sd = _DstBitmap.GetRowPixelsSpan(y).Slice(0, w);
                cvt.ConvertPixels(sr, sd);
            }
        }

        public TResult Fill<TResult>(BitmapBinaryOperation<TResult> transform, IReadOnlyBitmap<TContextPixel> srcBmp, bool initPixels = true)            
        {
            var srcRef = new ManagedReadOnlyBitmapOperand<TContextPixel>(srcBmp);
            return Fill(transform,srcRef, initPixels);
        }

        public TResult Fill<TSrcBitmap, TResult>(BitmapBinaryOperation<TResult> transform, IReadOnlyBitmap<TContextPixel> srcBmp, IPixelConverter<TContextPixel, TPixel> pixelConverter)            
        {
            var srcRef = new ManagedReadOnlyBitmapOperand<TContextPixel>(srcBmp);
            return Fill(transform, srcRef, pixelConverter);
        }


        public TResult Fill<TSrcBitmap,TResult>(BitmapBinaryOperation<TResult> transform, TSrcBitmap srcBmp, bool initPixels = true)
            where TSrcBitmap : IReadOnlyBitmap<TSrcBitmap, TContextPixel>, allows ref struct
        {
            return transform.GetInstance<TContextPixel, TPixel>().Execute(srcBmp, _DstBitmap, initPixels);
        }

        public TResult Fill<TSrcBitmap, TResult>(BitmapBinaryOperation<TResult> transform, TSrcBitmap srcBmp, IPixelConverter<TContextPixel, TPixel> pixelConverter)
            where TSrcBitmap : IReadOnlyBitmap<TSrcBitmap, TContextPixel>, allows ref struct
        {
            return transform.GetInstance<TContextPixel, TPixel>().Execute(srcBmp, _DstBitmap, pixelConverter);
        }
    }


    public readonly ref struct BinaryFillContext<TBitmap, TPixel, TContextPixel>
        where TBitmap : IBitmapWriter<TBitmap, TPixel>, allows ref struct
        where TPixel : unmanaged
        where TContextPixel : unmanaged
    {
        public BinaryFillContext(TBitmap dstBitmap)
        {
            _DstBitmap = dstBitmap;
        }

        private readonly TBitmap _DstBitmap;        

        public TResult Fill<TSrcBitmap, TResult>(BitmapFillOperation<TResult> transform, TSrcBitmap srcBmp, IPixelConverter<TContextPixel, TPixel> pixelConverter)
            where TSrcBitmap : IBitmapReader<TSrcBitmap, TContextPixel>, allows ref struct
        {
            return transform.GetInstance<TContextPixel, TPixel>().Execute(srcBmp, _DstBitmap, pixelConverter);
        }

        public TResult Fill2<TSrcBitmap, TResult>(BitmapFillOperation<TResult> transform, TSrcBitmap srcBmp, IPixelConverter<TContextPixel, TPixel> pixelConverter)
            where TSrcBitmap : IReadOnlyBitmap<TSrcBitmap, TContextPixel>, allows ref struct
        {
            return transform.GetInstance<TContextPixel, TPixel>().Execute2(srcBmp, _DstBitmap, pixelConverter);
        }
    }

}

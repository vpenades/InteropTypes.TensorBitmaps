using System;
using System.Collections.Generic;
using System.Text;

using InteropTypes.Numerics;
using InteropTypes.TensorBitmaps.Operands;

namespace InteropTypes.TensorBitmaps.Operators
{
    public readonly ref struct DrawingContext<TBitmap, TPixel, TContextPixel>
        where TBitmap : IBitmap<TBitmap, TPixel>, allows ref struct
        where TPixel : unmanaged
        where TContextPixel : unmanaged
    {
        public DrawingContext(TBitmap dstBitmap)
        {
            _DstBitmap = dstBitmap;            
        }

        private readonly TBitmap _DstBitmap;

        public void Draw(IReadOnlyBitmap<TContextPixel> srcBmp, bool initPixels = true)
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

        public TResult Draw<TResult>(BitmapDrawOperation<TResult> transform, IReadOnlyBitmap<TContextPixel> srcBmp, bool initPixels = true)            
        {
            var srcRef = new ManagedReadOnlyBitmapOperand<TContextPixel>(srcBmp);
            return Draw(transform,srcRef, initPixels);
        }

        public TResult Draw<TSrcBitmap, TResult>(BitmapDrawOperation<TResult> transform, IReadOnlyBitmap<TContextPixel> srcBmp, IPixelConverter<TContextPixel, TPixel> pixelConverter)
        {
            var srcRef = new ManagedReadOnlyBitmapOperand<TContextPixel>(srcBmp);
            return Draw(transform, srcRef, pixelConverter);
        }

        public TResult Draw<TSrcBitmap,TResult>(BitmapDrawOperation<TResult> transform, TSrcBitmap srcBmp, bool initPixels = true)
            where TSrcBitmap : IReadOnlyBitmap<TSrcBitmap, TContextPixel>, allows ref struct
        {
            return transform.GetInstance<TContextPixel, TPixel>().Draw(srcBmp, _DstBitmap, initPixels);
        }

        public TResult Draw<TSrcBitmap, TResult>(BitmapDrawOperation<TResult> transform, TSrcBitmap srcBmp, IPixelConverter<TContextPixel, TPixel> pixelConverter)
            where TSrcBitmap : IReadOnlyBitmap<TSrcBitmap, TContextPixel>, allows ref struct
        {
            return transform.GetInstance<TContextPixel, TPixel>().Draw(srcBmp, _DstBitmap, pixelConverter);
        }
    }


    public readonly ref struct FillerContext<TBitmap, TPixel, TContextPixel>
        where TBitmap : IBitmapWriter<TBitmap, TPixel>, allows ref struct
        where TPixel : unmanaged
        where TContextPixel : unmanaged
    {
        public FillerContext(TBitmap dstBitmap)
        {
            _DstBitmap = dstBitmap;
        }

        private readonly TBitmap _DstBitmap;

        public TResult FillEx<TSrcBitmap, TResult>(BitmapFillOperation<TResult> transform, TSrcBitmap srcBmp, bool initPixels = true)
            where TSrcBitmap : IReadOnlyBitmap<TSrcBitmap, TContextPixel>, allows ref struct
        {
            return transform.GetInstance<TContextPixel, TPixel>().FillEx(srcBmp, _DstBitmap, initPixels);
        }

        public TResult FillEx<TSrcBitmap, TResult>(BitmapFillOperation<TResult> transform, TSrcBitmap srcBmp, IPixelConverter<TContextPixel, TPixel> pixelConverter)
            where TSrcBitmap : IReadOnlyBitmap<TSrcBitmap, TContextPixel>, allows ref struct
        {
            return transform.GetInstance<TContextPixel, TPixel>().FillEx(srcBmp, _DstBitmap, pixelConverter);
        }

        public void Fill(IBitmapReader<TContextPixel> srcBmp, bool initPixels = true)
        {
            var srcRef = new ManagedBitmapReaderOperand<TContextPixel>(srcBmp);
            Fill(BitmapOperations.FillCopy, srcRef, initPixels);
        }

        public TResult Fill<TResult>(BitmapFillOperation<TResult> transform, IReadOnlyBitmap<TContextPixel> srcBmp, bool initPixels = true)
        {
            var srcRef = new ManagedReadOnlyBitmapOperand<TContextPixel>(srcBmp);
            return Fill(transform, srcRef, initPixels);
        }

        public TResult Fill<TSrcBitmap, TResult>(BitmapFillOperation<TResult> transform, TSrcBitmap srcBmp, bool initPixels = true)
            where TSrcBitmap : IBitmapReader<TSrcBitmap, TContextPixel>, allows ref struct
        {
            return transform.GetInstance<TContextPixel, TPixel>().Fill(srcBmp, _DstBitmap, initPixels);
        }

        public TResult Fill<TSrcBitmap, TResult>(BitmapFillOperation<TResult> transform, TSrcBitmap srcBmp, IPixelConverter<TContextPixel, TPixel> pixelConverter)
            where TSrcBitmap : IBitmapReader<TSrcBitmap, TContextPixel>, allows ref struct
        {
            return transform.GetInstance<TContextPixel, TPixel>().Fill(srcBmp, _DstBitmap, pixelConverter);
        }

        
    }

}

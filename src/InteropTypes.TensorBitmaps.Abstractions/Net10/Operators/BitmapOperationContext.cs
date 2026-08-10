using System;
using System.Collections.Generic;
using System.Text;

using InteropTypes.Numerics;
using InteropTypes.TensorBitmaps.Primitives;

namespace InteropTypes.TensorBitmaps.Operators
{
    public readonly ref struct BitmapOperationContext<TBitmap, TPixel, TContextPixel>
        where TBitmap : IBitmap<TBitmap, TPixel>, allows ref struct
        where TPixel : unmanaged
        where TContextPixel : unmanaged
    {
        public BitmapOperationContext(TBitmap dstBitmap)
        {
            _DstBitmap = dstBitmap;            
        }

        private readonly TBitmap _DstBitmap;

        public void Apply(IReadOnlyBitmap<TContextPixel> srcBmp, bool initPixels = true)
        {
            Apply(FillOperations.Copy, srcBmp, initPixels);
        }

        public TResult Apply<TResult>(BitmapOperationFactory<TResult> transform, IReadOnlyBitmap<TContextPixel> srcBmp, bool initPixels = true)            
        {
            var srcRef = new RefStructReadOnlyBitmap<TContextPixel>(srcBmp);
            return Apply(transform,srcRef, initPixels);
        }

        public TResult Apply<TSrcBitmap, TResult>(BitmapOperationFactory<TResult> transform, IReadOnlyBitmap<TContextPixel> srcBmp, IPixelConverter<TContextPixel, TPixel> pixelConverter)
        {
            var srcRef = new RefStructReadOnlyBitmap<TContextPixel>(srcBmp);
            return Apply(transform, srcRef, pixelConverter);
        }

        public TResult Apply<TSrcBitmap,TResult>(BitmapOperationFactory<TResult> transform, TSrcBitmap srcBmp, bool initPixels = true)
            where TSrcBitmap : IReadOnlyBitmap<TSrcBitmap, TContextPixel>, allows ref struct
        {
            return transform.GetInstance<TContextPixel, TPixel>().Apply(srcBmp, _DstBitmap, initPixels);
        }

        public TResult Apply<TSrcBitmap, TResult>(BitmapOperationFactory<TResult> transform, TSrcBitmap srcBmp, IPixelConverter<TContextPixel, TPixel> pixelConverter)
            where TSrcBitmap : IReadOnlyBitmap<TSrcBitmap, TContextPixel>, allows ref struct
        {
            return transform.GetInstance<TContextPixel, TPixel>().Apply(srcBmp, _DstBitmap, pixelConverter);
        }
    }   

}

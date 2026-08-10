using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using InteropTypes.Numerics;
using InteropTypes.TensorBitmaps.Primitives;

namespace InteropTypes.TensorBitmaps.Operators
{
    /// <summary>
    /// represents a pixel transformation factory
    /// </summary>
    /// <remarks>
    /// Used by  using <see cref="ReadOnlyTensorSpanBitmap{TElement, TPixel}.CopyPixelsTo{TDstElement, TDstPixel}(FillOperations, TensorSpanBitmap{TDstElement, TDstPixel}, bool)"/>
    /// </remarks>
    public abstract class BitmapOperationFactory<TResult>
    {
        public abstract IBitmapOperation<TSrcPixel, TDstPixel, TResult> GetInstance<TSrcPixel, TDstPixel>()
            where TSrcPixel : unmanaged
            where TDstPixel : unmanaged;
    }

    /// <summary>
    /// interface for an operation that perform a pixel transfer between a source and destination bitmap
    /// </summary>
    /// <typeparam name="TSrcPixel">The pixel type of source</typeparam>
    /// <typeparam name="TDstPixel">The pixel type of destination</typeparam>
    public interface IBitmapOperation<TSrcPixel, TDstPixel, TResult>
        where TSrcPixel : unmanaged
        where TDstPixel : unmanaged
    {       
        #if NET9_0_OR_GREATER
        TResult Apply(IReadOnlyBitmap<TSrcPixel> src, IBitmap<TDstPixel> dst, bool initPixels = true)
        {
            var msrc = new RefStructReadOnlyBitmap<TSrcPixel>(src);
            var mdst = new RefStructBitmap<TDstPixel>(dst);

            return Apply(msrc, mdst, initPixels);
        }

        TResult Apply(IReadOnlyBitmap<TSrcPixel> src, IBitmap<TDstPixel> dst, IPixelConverter<TSrcPixel, TDstPixel> pixelConverter)
        {
            var msrc = new RefStructReadOnlyBitmap<TSrcPixel>(src);
            var mdst = new RefStructBitmap<TDstPixel>(dst);

            return Apply(msrc, mdst, pixelConverter);
        }

        TResult Apply<TDstBmp>(IReadOnlyBitmap<TSrcPixel> src, TDstBmp dst, bool initPixels = true)
            where TDstBmp : IBitmap<TDstBmp, TDstPixel>, allows ref struct
        {
            var msrc = new RefStructReadOnlyBitmap<TSrcPixel>(src);            

            return Apply(msrc, dst, initPixels);
        }

        TResult Apply<TDstBmp>(IReadOnlyBitmap<TSrcPixel> src, TDstBmp dst, IPixelConverter<TSrcPixel, TDstPixel> pixelConverter)
            where TDstBmp : IBitmap<TDstBmp, TDstPixel>, allows ref struct
        {
            var msrc = new RefStructReadOnlyBitmap<TSrcPixel>(src);            

            return Apply(msrc, dst, pixelConverter);
        }

        #endif


        TResult Apply<TSrcBmp, TDstBmp>(TSrcBmp src, TDstBmp dst, bool initPixels = true)
            #if NET9_0_OR_GREATER
            where TSrcBmp : IReadOnlyBitmap<TSrcBmp, TSrcPixel>, allows ref struct
            where TDstBmp : IBitmap<TDstBmp, TDstPixel>, allows ref struct
            #else
            where TSrcBmp : IReadOnlyBitmap<TSrcPixel>
            where TDstBmp : IBitmap<TDstPixel>
            #endif

        {
            var pixelConverter = IPixelConverter<TSrcPixel, TDstPixel>.Create(src.Format, dst.Format, initPixels);
            return Apply(src, dst, pixelConverter);
        }

        TResult Apply<TSrcBmp, TDstBmp>(TSrcBmp src, TDstBmp dst, IPixelConverter<TSrcPixel, TDstPixel> pixelConverter)
            #if NET9_0_OR_GREATER
            where TSrcBmp : IReadOnlyBitmap<TSrcBmp, TSrcPixel>, allows ref struct
            where TDstBmp : IBitmap<TDstBmp, TDstPixel>, allows ref struct
            #else
            where TSrcBmp : IReadOnlyBitmap<TSrcPixel>
            where TDstBmp : IBitmap<TDstPixel>
            #endif
            ;
    }
}

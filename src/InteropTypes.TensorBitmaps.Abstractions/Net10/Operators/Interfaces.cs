using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using InteropTypes.Numerics;

namespace InteropTypes.TensorBitmaps.Operators
{
    /// <summary>
    /// interface for operator that perform a pixel transfer between a source and destination bitmap
    /// </summary>
    /// <typeparam name="TSrcPixel">The pixel type of source</typeparam>
    /// <typeparam name="TDstPixel">The pixel type od destination</typeparam>
    public interface IDrawOperation<TSrcPixel, TDstPixel, TResult>
        where TSrcPixel : unmanaged
        where TDstPixel : unmanaged
    {
        TResult Draw<TSrcBmp, TDstBmp>(TSrcBmp src, TDstBmp dst, bool initPixels = true)
            where TSrcBmp : IReadOnlyBitmap<TSrcBmp, TSrcPixel>, allows ref struct
            where TDstBmp : IBitmap<TDstBmp, TDstPixel>, allows ref struct
        {
            var pixelConverter = IPixelConverter<TSrcPixel, TDstPixel>.Create(src.Format, dst.Format, initPixels);
            return Draw(src, dst, pixelConverter);
        }

        TResult Draw<TSrcBmp, TDstBmp>(TSrcBmp src, TDstBmp dst, IPixelConverter<TSrcPixel, TDstPixel> pixelConverter)
            where TSrcBmp : IReadOnlyBitmap<TSrcBmp, TSrcPixel>, allows ref struct
            where TDstBmp : IBitmap<TDstBmp, TDstPixel>, allows ref struct;        
    }

    /// <summary>
    /// interface for operator that perform a pixel transfer between a source and destination bitmap
    /// </summary>
    /// <typeparam name="TSrcPixel">The pixel type of source</typeparam>
    /// <typeparam name="TDstPixel">The pixel type od destination</typeparam>
    public interface IFillOperation<TSrcPixel, TDstPixel, TResult>
        where TSrcPixel : unmanaged
        where TDstPixel : unmanaged
    {
        

        TResult FillEx<TSrcBmp, TDstBmp>(TSrcBmp src, TDstBmp dst, bool initPixels = true)
            where TSrcBmp : IReadOnlyBitmap<TSrcBmp, TSrcPixel>, allows ref struct
            where TDstBmp : IBitmapWriter<TDstBmp, TDstPixel>, allows ref struct
        {
            var pixelConverter = IPixelConverter<TSrcPixel, TDstPixel>.Create(src.Format, dst.Format, initPixels);
            return FillEx(src, dst, pixelConverter);
        }

        TResult FillEx<TSrcBmp, TDstBmp>(TSrcBmp src, TDstBmp dst, IPixelConverter<TSrcPixel, TDstPixel> pixelConverter)
            where TSrcBmp : IReadOnlyBitmap<TSrcBmp, TSrcPixel>, allows ref struct
            where TDstBmp : IBitmapWriter<TDstBmp, TDstPixel>, allows ref struct
        {
            return Fill(src, dst, pixelConverter);
        }

        TResult Fill<TSrcBmp, TDstBmp>(TSrcBmp src, TDstBmp dst, bool initPixels = true)
            where TSrcBmp : IBitmapReader<TSrcBmp, TSrcPixel>, allows ref struct
            where TDstBmp : IBitmapWriter<TDstBmp, TDstPixel>, allows ref struct
        {
            var pixelConverter = IPixelConverter<TSrcPixel, TDstPixel>.Create(src.Format, dst.Format, initPixels);
            return Fill(src, dst, pixelConverter);
        }

        TResult Fill<TSrcBmp, TDstBmp>(TSrcBmp src, TDstBmp dst, IPixelConverter<TSrcPixel, TDstPixel> pixelConverter)
            where TSrcBmp : IBitmapReader<TSrcBmp, TSrcPixel>, allows ref struct
            where TDstBmp : IBitmapWriter<TDstBmp, TDstPixel>, allows ref struct;
    }
}

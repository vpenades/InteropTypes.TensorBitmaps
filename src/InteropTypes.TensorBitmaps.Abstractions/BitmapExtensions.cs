using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using InteropTypes.TensorBitmaps.Primitives;

namespace InteropTypes.TensorBitmaps
{
    public static class BitmapExtensions
    {
        public static IBitmap<TPixel> GetCropped<TPixel>(this IBitmap<TPixel> bitmap, System.Drawing.Rectangle rect)
            where TPixel : unmanaged            
        {
            return new _BitmapCropped<TPixel>(bitmap, rect);
        }

        public static IReadOnlyBitmap<TPixel> GetCropped<TPixel>(this IReadOnlyBitmap<TPixel> bitmap, System.Drawing.Rectangle rect)
            where TPixel : unmanaged
        {
            return new _ReadOnlyBitmapCropped<TPixel>(bitmap, rect);
        }

        public static IBitmap<TDstPixel> Cast<TSrcPixel,TDstPixel>(this IBitmap<TSrcPixel> bitmap)
            where TSrcPixel: unmanaged
            where TDstPixel : unmanaged
        {
            return new _BitmapCasted<TSrcPixel,TDstPixel>(bitmap);
        }

        public static IReadOnlyBitmap<TDstPixel> Cast<TSrcPixel, TDstPixel>(this IReadOnlyBitmap<TSrcPixel> bitmap)
            where TSrcPixel : unmanaged
            where TDstPixel : unmanaged
        {
            return new _ReadOnlyBitmapCasted<TSrcPixel, TDstPixel>(bitmap);
        }        
    }
}

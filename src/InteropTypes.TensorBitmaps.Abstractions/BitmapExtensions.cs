using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace InteropTypes.TensorBitmaps
{
    public static class BitmapExtensions
    {
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

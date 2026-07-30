using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using InteropTypes.Numerics;

namespace InteropTypes.TensorBitmaps
{   

    [System.Diagnostics.DebuggerDisplay("_ReadOnlyBitmapCasted {Width}x{Height}x{Format}")]
    readonly struct _ReadOnlyBitmapCasted<TSrcPixel,TDstPixel> : IReadOnlyBitmap<TDstPixel>
        where TSrcPixel : unmanaged
        where TDstPixel : unmanaged
    {
        public _ReadOnlyBitmapCasted(IReadOnlyBitmap<TSrcPixel> source)
        {
            if (Unsafe.SizeOf<TSrcPixel>() != Unsafe.SizeOf<TDstPixel>()) throw new InvalidOperationException("pixel size mismatch");
            _Source = source;
        }

        private readonly IReadOnlyBitmap<TSrcPixel> _Source;
        

        public PixelFormat Format => _Source.Format;
        public int Width => _Source.Width;
        public int Height => _Source.Height;

        public ReadOnlySpan<TDstPixel> GetRowPixelsSpan(int y)
        {
            var row = _Source.GetRowPixelsSpan(y);
            return System.Runtime.InteropServices.MemoryMarshal.Cast<TSrcPixel, TDstPixel>(row);
        }        
    }

    [System.Diagnostics.DebuggerDisplay("_BitmapCasted {Width}x{Height}x{Format}")]
    readonly struct _BitmapCasted<TSrcPixel, TDstPixel> : IBitmap<TDstPixel>
        where TSrcPixel : unmanaged
        where TDstPixel : unmanaged
    {
        public _BitmapCasted(IBitmap<TSrcPixel> source)
        {
            if (Unsafe.SizeOf<TSrcPixel>() != Unsafe.SizeOf<TDstPixel>()) throw new InvalidOperationException("pixel size mismatch");
            _Source = source;         
        }

        internal readonly IBitmap<TSrcPixel> _Source;        

        public PixelFormat Format => _Source.Format;
        public int Width => _Source.Width;
        public int Height => _Source.Height;

        public Span<TDstPixel> GetRowPixelsSpan(int y)
        {
            var row = _Source.GetRowPixelsSpan(y);
            return System.Runtime.InteropServices.MemoryMarshal.Cast<TSrcPixel, TDstPixel>(row);
        }
    }
}

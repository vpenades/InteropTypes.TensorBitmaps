using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

using InteropTypes.Numerics;
using InteropTypes.TensorBitmaps.Operands;

namespace InteropTypes.TensorBitmaps
{
    class ManagedBitmap<TPixel> : IBitmap<TPixel>
        where TPixel: unmanaged
    {
        #region lifecycle        

        public ManagedBitmap(int width, int height , PixelFormat fmt)
        {
            fmt.ThrowIfBytesPerPixelMismatch<TPixel>();
            _Pixels = new TPixel[width * height];
            Width = width;
            Height = height;
            Format = fmt;
        }

        #endregion

        #region data

        internal readonly TPixel[] _Pixels;

        public PixelFormat Format { get; }

        public int Width { get; }

        public int Height { get; }

        #endregion

        #region API        

        public Span<TPixel> GetRowPixelsSpan(int y)
        {
            return _Pixels.AsSpan(y * Width, Width);
        }

        public void Fill<TSrcBitmap,TSrcPixel>(TSrcBitmap bmp)
            where TSrcBitmap : IBitmapReader<TSrcBitmap, TSrcPixel>, allows ref struct
            where TSrcPixel : unmanaged
        {
            var cvt = IPixelConverter<TSrcPixel, TPixel>.Create(bmp.Format, this.Format, true);

            var w = Math.Min(bmp.Width, this.Width);
            var h = Math.Min(bmp.Height, this.Height);

            Span<TSrcPixel> sr = stackalloc TSrcPixel[bmp.Width];

            for (int y = 0; y < h; ++y)
            {
                bmp.ReadRowPixelsSpan(y, sr);                
                var sd = this.GetRowPixelsSpan(y);
                cvt.ConvertPixels(sr.Slice(0,w), sd.Slice(0, w));
            }
        }

        public Operators.DrawingContext<ManagedBitmapOperand<TPixel>, TPixel, TSrcPixel> GetContext<TSrcPixel>()
            where TSrcPixel:unmanaged
        {
            var m = new ManagedBitmapOperand<TPixel>(this);
            return m.GetDrawingContext<TSrcPixel>();
        }

        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

using InteropTypes.Numerics;
using InteropTypes.TensorBitmaps.Operands;

using PhotoSauce.MagicScaler;

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
            where TSrcBitmap : IReadOnlyBitmap<TSrcBitmap, TSrcPixel>, allows ref struct
            where TSrcPixel : unmanaged
        {
            var cvt = IPixelConverter<TSrcPixel, TPixel>.Create(bmp.Format, this.Format, true);

            var w = Math.Min(bmp.Width, this.Width);
            var h = Math.Min(bmp.Height, this.Height);

            for (int y = 0; y < h; ++y)
            {
                var sr = bmp.GetRowPixelsSpan(y).Slice(0, w);
                var sd = this.GetRowPixelsSpan(y).Slice(0, w);
                cvt.ConvertPixels(sr, sd);
            }
        }

        public Operators.BinaryOperatorContext<ManagedBitmapOperand<TPixel>, TPixel, TSrcPixel> GetContext<TSrcPixel>()
            where TSrcPixel:unmanaged
        {
            var m = new ManagedBitmapOperand<TPixel>(this);
            return m.GetContext<TSrcPixel>();
        }

        #endregion
    }
}

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

        public Operators.DrawingContext<ManagedBitmapOperand<TPixel>, TPixel, TContextPixel> GetDrawingContext<TContextPixel>()
            where TContextPixel : unmanaged
        {
            return new ManagedBitmapOperand<TPixel>(this).GetDrawingContext<TContextPixel>();
        }

        public Operators.FillerContext<ManagedBitmapOperand<TPixel>, TPixel, TContextPixel> GetFillerContext<TContextPixel>()
            where TContextPixel : unmanaged
        {
            return new ManagedBitmapOperand<TPixel>(this).GetFillerContext<TContextPixel>();
        }

        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Text;

using InteropTypes.Numerics;
using InteropTypes.TensorBitmaps.Operators;
using InteropTypes.TensorBitmaps.Primitives;

namespace InteropTypes.TensorBitmaps
{
    public readonly struct ArrayBitmap<TPixel> : IBitmapFull<TPixel>
        where TPixel: unmanaged
    {
        #region lifecycle        

        public ArrayBitmap(int width, int height , PixelFormat fmt)
        {
            fmt.ThrowIfBytesPerPixelMismatch<TPixel>();
            _Stride = width * fmt.BytesPerPixel;
            _Pixels = new byte[height * _Stride];            
            Width = width;
            Height = height;
            Format = fmt;
        }

        public ArrayBitmap(ArraySegment<byte> pixels, int byteStride, PixelFormat format, int width, int height)
        {
            _Pixels = pixels;
            _Stride = byteStride;
            Format = format;
            Width = width;
            Height = height;
        }

        #endregion

        #region data

        private readonly ArraySegment<byte> _Pixels;
        private readonly int _Stride;

        public PixelFormat Format { get; }

        public int Width { get; }

        public int Height { get; }

        #endregion

        #region API        

        public Span<TPixel> GetRowPixelsSpan(int y)
        {
            if (y < 0 || y >= Height) throw new ArgumentOutOfRangeException(nameof(y));

            var row = _Pixels.AsSpan(_Stride * y);

            return System.Runtime.InteropServices.MemoryMarshal.Cast<byte, TPixel>(row).Slice(0, Width);
        }

        public void CopyFrom<TSrcPixel>(IReadOnlyBitmap<TSrcPixel> srcBitmap)
            where TSrcPixel: unmanaged
        {
            Apply(FillOperations.Copy, srcBitmap);
        }

        public ArrayBitmap<TPixel> GetCropped(System.Drawing.Rectangle rectangle)
        {
            rectangle.Intersect(new System.Drawing.Rectangle(0, 0, Width, Height));
            if (rectangle.IsEmpty) throw new ArgumentException("nothing to crop");

            var offset = _Stride * rectangle.Y + rectangle.X * Format.BytesPerPixel;

            var buff = _Pixels.Slice(offset);

            return new ArrayBitmap<TPixel>(buff, _Stride, Format, rectangle.Width, rectangle.Height);
        }

        public TResult Apply<TSrcPixel,TResult>(BitmapOperationFactory<TResult> operation, IReadOnlyBitmap<TSrcPixel> srcBitmap)
            where TSrcPixel: unmanaged
        {
            return operation.GetInstance<TSrcPixel, TPixel>().Apply(srcBitmap, this);
        }

        public TResult Apply<TSrcPixel, TResult>(BitmapOperationFactory<TResult> operation, IReadOnlyBitmap<TSrcPixel> srcBitmap, IPixelConverter<TSrcPixel,TPixel> converter)
            where TSrcPixel : unmanaged
        {
            return operation.GetInstance<TSrcPixel, TPixel>().Apply(srcBitmap, this, converter);
        }

        #if NET9_0_OR_GREATER
        public Operators.BitmapOperationContext<RefStructBitmap<TPixel>, TPixel, TContextPixel> GetContext<TContextPixel>()
            where TContextPixel : unmanaged
        {
            return new RefStructBitmap<TPixel>(this).GetContext<TContextPixel>();
        }
        #endif

        #endregion
    }
}

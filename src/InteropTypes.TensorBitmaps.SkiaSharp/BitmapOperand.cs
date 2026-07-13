using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using InteropTypes.Numerics;

using SkiaSharp;

namespace InteropTypes.TensorBitmaps
{
    public class SkiaSharpBitmapOperand<TPixel>
        : InteropTypes.Numerics.BitmapOperators.IDisposableBitmapOperand<SkiaSharpBitmapOperand<TPixel>, TPixel>        
        , IDisposable
        where TPixel : unmanaged
    {
        public static SkiaSharpBitmapOperand<TPixel> Read(Func<System.IO.Stream> stream)
        {
            using(var s = stream.Invoke())
            {
                return Read(s);
            }
        }

        public static SkiaSharpBitmapOperand<TPixel> Read(System.IO.Stream stream)
        {
            var skbmp = SKBitmap.Decode(stream);

            return new SkiaSharpBitmapOperand<TPixel>(skbmp, false);
        }

        public void Write(Func<System.IO.Stream> stream)
        {
            using(var s = stream.Invoke())
            {
                Write(s);
            }
        }

        public void Write(System.IO.Stream stream)
        {
            if (!SkiaSharpForTensorBitmapsExtensions.TryGetSKEncodedImageFormat(stream, out var fmt)) fmt = SKEncodedImageFormat.Png;

            _Bitmap.Encode(stream, fmt, 75);
        }

        public SkiaSharpBitmapOperand(SKBitmap bitmap, bool doNotDispose)
            : this(bitmap, new Rectangle(0, 0, bitmap.Width, bitmap.Height), doNotDispose) { }

        private SkiaSharpBitmapOperand(SKBitmap bitmap, Rectangle region, bool doNotDispose)
        {
            _Bitmap = bitmap;
            _DoNotDispose = doNotDispose;
            _Region = region;

            Format = (bitmap.ColorType, bitmap.AlphaType).ToPixelFormat();
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~SkiaSharpBitmapOperand()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        void IDisposable.Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_DoNotDispose) return;
            if (!disposing) return;

            var bmp = System.Threading.Interlocked.Exchange(ref _Bitmap, null);
            bmp?.Dispose();
        }

        private readonly bool _DoNotDispose;
        private SkiaSharp.SKBitmap _Bitmap;
        private readonly Rectangle _Region;

        public Span<TPixel> GetRowPixelsSpan(int y)
        {
            if (y < 0 || y >= _Region.Height) throw new ArgumentOutOfRangeException(nameof(y));

            y += _Region.Y;

            var buffer = _Bitmap.GetPixelSpan();
            buffer = buffer.Slice(y * _Bitmap.RowBytes, _Bitmap.BytesPerPixel * _Region.Width);

            return System.Runtime.InteropServices.MemoryMarshal.Cast<byte, TPixel>(buffer);
        }

        public PixelFormat Format { get; }

        public int Width => _Region.Width;

        public int Height => _Region.Height;

        public SkiaSharpBitmapOperand<TPixel> GetCropped(Rectangle rectangle)
        {
            rectangle.Intersect(_Region);
            return new SkiaSharpBitmapOperand<TPixel>(_Bitmap, rectangle, true); // this is a view, so do not dispose
        }

        public SkiaSharpBitmapOperand<TPixel> CreateStretched(int width, int height)
        {
            var newSize = new SKImageInfo(width, height);
            var options = SKSamplingOptions.Default;
            var newBitmap = _Bitmap.Resize(newSize, options);

            return new SkiaSharpBitmapOperand<TPixel>(newBitmap, false); // this is a new object, so DO dispose
        }
    }
}

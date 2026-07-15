using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using InteropTypes.Numerics;
using InteropTypes.TensorBitmaps.Operands;

using SkiaSharp;

namespace InteropTypes.TensorBitmaps
{
    /// <summary>
    /// Represents a wrapper over a Skiasharp bitmap
    /// </summary>
    /// <typeparam name="TPixel">The pixel type, most of the time this will be <see cref="uint"/></typeparam>
    public class SkiaSharpBitmapOperand<TPixel>
        : IDisposableBitmapOperand<SkiaSharpBitmapOperand<TPixel>, TPixel>        
        , IDisposable
        where TPixel : unmanaged
    {
        #region IO

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

        public void Write(System.IO.Stream stream, SKEncodedImageFormat? format = null)
        {
            format ??= SkiaSharpForTensorBitmapsExtensions.TryGetSKEncodedImageFormat(stream, out var fmt)
                    ? fmt
                    : SKEncodedImageFormat.Png;            

            _Bitmap.Encode(stream, format.Value, 75);
        }

        #endregion

        #region lifecycle

        public static SkiaSharpBitmapOperand<TDstPixel> Create<TSrcBitmap, TSrcPixel, TDstPixel>(TSrcBitmap src)
            where TSrcBitmap: IReadOnlyBitmapOperand<TSrcBitmap,TSrcPixel>, allows ref struct
            where TSrcPixel: unmanaged
            where TDstPixel : unmanaged
        {
            var skbmp = new SKBitmap(src.Width, src.Height);
            var dst = new SkiaSharpBitmapOperand<TDstPixel>(skbmp, false);

            dst.GetContext<TSrcPixel, int>(PixelsTransform.Copy).ApplyFrom(src);

            return dst;
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

        #endregion

        #region data

        private readonly bool _DoNotDispose;
        private SkiaSharp.SKBitmap _Bitmap;
        private readonly Rectangle _Region;

        public PixelFormat Format { get; }

        #endregion

        #region API

        public int Width => _Region.Width;

        public int Height => _Region.Height;

        public Span<TPixel> GetRowPixelsSpan(int y)
        {
            if (y < 0 || y >= _Region.Height) throw new ArgumentOutOfRangeException(nameof(y));

            y += _Region.Y;

            var buffer = _Bitmap.GetPixelSpan();
            buffer = buffer.Slice(y * _Bitmap.RowBytes, _Bitmap.BytesPerPixel * _Region.Width);

            return System.Runtime.InteropServices.MemoryMarshal.Cast<byte, TPixel>(buffer);
        }        

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

        public BITMAPOPERATORS.BinaryOperatorContext<SkiaSharpBitmapOperand<TPixel>, TPixel, TSrcPixel, TResult> GetContext<TSrcPixel, TResult>(PixelsTransform<TResult> transform) where TSrcPixel : unmanaged
        {
            return new BITMAPOPERATORS.BinaryOperatorContext<SkiaSharpBitmapOperand<TPixel>, TPixel, TSrcPixel, TResult>(this, transform.GetInstance<TSrcPixel, TPixel>());
        }

        #endregion
    }
}

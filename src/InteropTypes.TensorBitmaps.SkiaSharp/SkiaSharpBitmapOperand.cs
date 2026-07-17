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
        : IBitmapOperand<SkiaSharpBitmapOperand<TPixel>, TPixel>        
        , IDisposableBitmap<TPixel>
        where TPixel : unmanaged
    {
        #region IO

        public static SkiaSharpBitmapOperand<TPixel> Load(System.IO.FileInfo finfo)
        {
            return Read(finfo.OpenRead);
        }

        public void Save(System.IO.FileInfo finfo)
        {
            Write(finfo.Create);
        }

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

            return new SkiaSharpBitmapOperand<TPixel>(skbmp);
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

        public static SkiaSharpBitmapOperand<TPixel> Create<TSrcBitmap, TSrcPixel>(TSrcBitmap src)
            where TSrcBitmap: IReadOnlyBitmapOperand<TSrcBitmap,TSrcPixel>, allows ref struct
            where TSrcPixel: unmanaged            
        {
            var skbmp = new SKBitmap(src.Width, src.Height);
            var dst = new SkiaSharpBitmapOperand<TPixel>(skbmp);

            dst.GetContext<TSrcPixel>().Fill(BitmapOperations.Copy, src);

            return dst;
        }
        
        public SkiaSharpBitmapOperand(SKBitmap bitmap)
        {
            _Bitmap = bitmap;
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
            if (!disposing) return;

            var bmp = System.Threading.Interlocked.Exchange(ref _Bitmap, null);
            bmp?.Dispose();
        }

        #endregion

        #region data
        
        private SkiaSharp.SKBitmap _Bitmap;
        public PixelFormat Format { get; }

        #endregion

        #region properties

        public int Width => _Bitmap.Width;

        public int Height => _Bitmap.Height;

        #endregion

        #region API

        public Span<byte> GetRowBytesSpan(int y)
        {
            if (y < 0 || y >= _Bitmap.Height) throw new ArgumentOutOfRangeException(nameof(y));            

            var buffer = _Bitmap.GetPixelSpan();
            var len = Math.Min(_Bitmap.RowBytes, _Bitmap.BytesPerPixel * _Bitmap.Width);
            return buffer.Slice(y * _Bitmap.RowBytes, len);
        }

        public SkiaSharpBitmapOperand<TPixel> GetCropped(Rectangle rectangle)
        {            
            rectangle.Intersect(new Rectangle(0,0,Width,Height));

            var cropArea = SKRectI.Create(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);

            // Extract the subset (Shares memory!)
            var croppedBitmap = new SKBitmap();
            bool success = _Bitmap.ExtractSubset(croppedBitmap, cropArea);
            if (!success) throw new InvalidOperationException();

            return new SkiaSharpBitmapOperand<TPixel>(croppedBitmap); // this is a view, so do not dispose
        }        

        public SkiaSharpBitmapOperand<TPixel> CreateStretched(int width, int height)
        {
            var newSize = new SKImageInfo(width, height);
            var options = new SKSamplingOptions(SKFilterMode.Linear);

            var newBitmap = _Bitmap.Resize(newSize, options);            

            return new SkiaSharpBitmapOperand<TPixel>(newBitmap);
        }        

        public BITMAPOPERATORS.BinaryOperatorContext<SkiaSharpBitmapOperand<TPixel>, TPixel, TSrcPixel> GetContext<TSrcPixel>() where TSrcPixel : unmanaged
        {
            return new BITMAPOPERATORS.BinaryOperatorContext<SkiaSharpBitmapOperand<TPixel>, TPixel, TSrcPixel>(this);
        }

        public bool TryCreateStretchedClientBitmap(int width, int height, out IReadOnlyDisposableBitmap<TPixel> stretchedBitmap)
        {
            stretchedBitmap = CreateStretched(width, height);
            return true;
        }

        #endregion
    }
}

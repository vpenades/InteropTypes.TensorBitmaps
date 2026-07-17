using System;
using System.Collections.Generic;
using System.Text;

using InteropTypes.Numerics;
using InteropTypes.TensorBitmaps.Operands;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.PixelFormats;

using BITMAPOPERATORS = InteropTypes.TensorBitmaps.Operators;

namespace InteropTypes.TensorBitmaps
{
    /// <summary>
    /// Represents a wrapper over a ImageSharp bitmap
    /// </summary>
    /// <typeparam name="TPixel">The ImageSharp pixel type</typeparam>
    public class ImageSharpBitmapOperand<TPixel>
        : IBitmapOperand<ImageSharpBitmapOperand<TPixel>, TPixel>
        , IDisposableBitmap<TPixel>
        where TPixel : unmanaged, IPixel<TPixel>
    {
        #region IO

        public static ImageSharpBitmapOperand<TPixel> Load(System.IO.FileInfo finfo)
        {
            return Read(finfo.OpenRead);
        }

        public void Save(System.IO.FileInfo finfo)
        {
            Write(finfo.Create);
        }

        public static ImageSharpBitmapOperand<TPixel> Read(Func<System.IO.Stream> stream)
        {
            using (var s = stream.Invoke())
            {
                return Read(s);
            }
        }

        public static ImageSharpBitmapOperand<TPixel> Read(System.IO.Stream stream)
        {
            var img = Image.Load<TPixel>(stream);

            return new ImageSharpBitmapOperand<TPixel>(img, false);
        }

        public void Write(Func<System.IO.Stream> stream)
        {
            using (var s = stream.Invoke())
            {
                Write(s);
            }
        }

        public void Write(System.IO.Stream stream, SixLabors.ImageSharp.Formats.IImageFormat? format = null)
        {
            if (format == null)
            {
                if (SkiaSharpForTensorBitmapsExtensions.TryGetSKEncodedImageFormat(stream, out var fmt))
                {
                    switch(fmt)
                    {
                        case SkiaSharp.SKEncodedImageFormat.Png: format = SixLabors.ImageSharp.Formats.Png.PngFormat.Instance; break;
                        case SkiaSharp.SKEncodedImageFormat.Jpeg: format = SixLabors.ImageSharp.Formats.Jpeg.JpegFormat.Instance; break;
                        case SkiaSharp.SKEncodedImageFormat.Gif: format = SixLabors.ImageSharp.Formats.Gif.GifFormat.Instance; break;
                    }
                }
            }

            if (format == null) throw new ArgumentNullException(nameof(format));            

            _Bitmap.Save(stream, format);
        }

        #endregion

        #region lifecycle

        public static ImageSharpBitmapOperand<TPixel> Create<TSrcBitmap, TSrcPixel>(TSrcBitmap src)
            where TSrcBitmap : IReadOnlyBitmapOperand<TSrcBitmap, TSrcPixel>, allows ref struct
            where TSrcPixel : unmanaged            
        {
            var bmp = new Image<TPixel>(src.Width, src.Height);
            var dst = new ImageSharpBitmapOperand<TPixel>(bmp, false);

            dst.GetContext<TSrcPixel>().Fill(BitmapOperations.Copy, src);

            return dst;
        }


        public ImageSharpBitmapOperand(Image<TPixel> bitmap, bool doNotDispose)
            : this(bitmap, new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height), doNotDispose) { }

        private ImageSharpBitmapOperand(Image<TPixel> bitmap, System.Drawing.Rectangle region, bool doNotDispose)
        {
            _Bitmap = bitmap;
            _DoNotDispose = doNotDispose;
            _Region = region;

            Format = ImageSharpUtils.ToTensorPixelFormat(typeof(TPixel));
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~ImageSharpBitmapOperand()
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
        private Image<TPixel>? _Bitmap;
        private readonly System.Drawing.Rectangle _Region;

        public PixelFormat Format { get; }

        #endregion

        #region properties

        public int Width => _Region.Width;

        public int Height => _Region.Height;

        #endregion

        #region API

        public Span<byte> GetRowBytesSpan(int y)
        {            
            return System.Runtime.InteropServices.MemoryMarshal.AsBytes(GetRowPixelsSpan(y));
        }

        public Span<TPixel> GetRowPixelsSpan(int y)
        {
            if (y < 0 || y >= _Region.Height) throw new ArgumentOutOfRangeException(nameof(y));

            y += _Region.Y;

            var pixels = _Bitmap.Frames[0].PixelBuffer.DangerousGetRowSpan(y);
            return pixels.Slice(0, _Region.Width);            
        }

        public ImageSharpBitmapOperand<TPixel> GetCropped(System.Drawing.Rectangle rectangle)
        {
            rectangle.X += _Region.X;
            rectangle.Y += _Region.Y;
            rectangle.Intersect(_Region);

            return new ImageSharpBitmapOperand<TPixel>(_Bitmap, rectangle, true); // this is a view, so do not dispose
        }

        public ImageSharpBitmapOperand<TPixel> CreateStretched(int width, int height)
        {
            var r = new Rectangle(_Region.X, _Region.Y, _Region.Width, _Region.Height);
            using var crop = _Bitmap.Clone(item => item.Crop(r));

            var ropts = new ResizeOptions();
            ropts.Size = new Size(width, height);
            ropts.Compand = true;
            ropts.PremultiplyAlpha = true;
            ropts.Mode = ResizeMode.Stretch;
            

            var newBitmap = crop.Clone(dc => dc.Resize(ropts));

            return new ImageSharpBitmapOperand<TPixel>(newBitmap, false);
        }

        public BITMAPOPERATORS.BinaryOperatorContext<ImageSharpBitmapOperand<TPixel>, TPixel, TContextPixel> GetContext<TContextPixel>() where TContextPixel : unmanaged
        {
            return new BITMAPOPERATORS.BinaryOperatorContext<ImageSharpBitmapOperand<TPixel>, TPixel, TContextPixel>(this);
        }

        public bool TryCreateStretchedClientBitmap(int width, int height, out IReadOnlyDisposableBitmap<TPixel> stretchedBitmap)
        {
            stretchedBitmap = CreateStretched(width, height);
            return true;
        }        

        #endregion
    }
}

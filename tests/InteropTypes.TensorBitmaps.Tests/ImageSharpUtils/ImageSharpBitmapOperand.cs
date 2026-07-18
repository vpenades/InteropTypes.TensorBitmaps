using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

using InteropTypes.Numerics;
using InteropTypes.TensorBitmaps.Operands;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

using SkiaSharp;

namespace InteropTypes.TensorBitmaps
{
    /// <summary>
    /// Represents a wrapper over a ImageSharp bitmap
    /// </summary>
    /// <typeparam name="TPixel">The ImageSharp pixel type</typeparam>
    public class ImageSharpBitmapOperand<TPixel> : IClientBitmap<TPixel>        
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

            return new ImageSharpBitmapOperand<TPixel>(img);
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
            var dst = new ImageSharpBitmapOperand<TPixel>(bmp);

            dst.AsOperand().GetContext<TSrcPixel>().Fill(BitmapOperations.Copy, src);

            return dst;
        }

        private ImageSharpBitmapOperand(Image<TPixel> bitmap)
        {
            _Bitmap = bitmap;
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
            if (!disposing) return;

            var bmp = System.Threading.Interlocked.Exchange(ref _Bitmap, null);
            bmp?.Dispose();
        }

        #endregion

        #region data
        
        private Image<TPixel>? _Bitmap;
        public PixelFormat Format { get; }

        #endregion

        #region properties

        public int Width => _Bitmap?.Width ?? 0;
        public int Height => _Bitmap?.Height ?? 0;

        #endregion

        #region API        

        public ManagedBitmapOperand<TPixel> AsOperand()
        {
            ObjectDisposedException.ThrowIf(_Bitmap == null, typeof(Image<TPixel>));

            return new ManagedBitmapOperand<TPixel>(this);
        }

        public Span<TPixel> GetRowPixelsSpan(int y)
        {
            ObjectDisposedException.ThrowIf(_Bitmap == null, typeof(Image<TPixel>));
            return _Bitmap.Frames[0].PixelBuffer.DangerousGetRowSpan(y);
        }

        bool IClientBitmap<TPixel>.TryCreateCropped(System.Drawing.Rectangle rect, bool shareMemory, [NotNullWhen(true)] out IClientBitmap<TPixel>? croppedBitmap)
        {
            if (shareMemory)
            {
                croppedBitmap = null;
                return false;
            }
            else
            {
                croppedBitmap = CreateCropped(rect);
                return true;
            }
        }

        bool IClientBitmap<TPixel>.TryCreateStretched(int width, int height, System.Drawing.Rectangle? srcCrop, [NotNullWhen(true)] out IClientBitmap<TPixel>? stretchedBitmap)
        {
            if (srcCrop.HasValue)
            {
                using var cropped = CreateCropped(srcCrop.Value);
                stretchedBitmap = cropped.CreateStretched(width, height);
                return true;
            }

            stretchedBitmap = CreateStretched(width, height);
            return true;
        }

        public ImageSharpBitmapOperand<TPixel> CreateCropped(System.Drawing.Rectangle rectangle)
        {
            ObjectDisposedException.ThrowIf(_Bitmap == null, typeof(Image<TPixel>));

            var r = new Rectangle(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);

            var crop = _Bitmap.Clone(dc => dc.Crop(r));

            return new ImageSharpBitmapOperand<TPixel>(crop);
        }

        public ImageSharpBitmapOperand<TPixel> CreateStretched(int width, int height)
        {
            ObjectDisposedException.ThrowIf(_Bitmap == null, typeof(Image<TPixel>));

            var ropts = new ResizeOptions();
            ropts.Size = new Size(width, height);
            ropts.Compand = true;
            ropts.PremultiplyAlpha = true;
            ropts.Mode = ResizeMode.Stretch;            

            var newBitmap = _Bitmap.Clone(dc => dc.Resize(ropts));

            return new ImageSharpBitmapOperand<TPixel>(newBitmap);
        }        

        #endregion
    }
}

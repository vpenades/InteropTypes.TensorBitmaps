using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;

using InteropTypes.Numerics;
using InteropTypes.TensorBitmaps.Operands;
using InteropTypes.TensorBitmaps.Operators;

using PhotoSauce.MagicScaler;

namespace InteropTypes.TensorBitmaps
{
    public static class MagicScalerUtils
    {
        /// <summary>
        /// Determines the number of channels from a PhotoSauce pixel format GUID.
        /// </summary>
        public static PixelFormat GetPixelFormatFromMagicScaler(Guid format)
        {
            if (format == PixelFormats.Grey8bpp) return KnownPixelFormats.Luminance8;
            if (format == PixelFormats.Bgr24bpp) return KnownPixelFormats.Bgr8;
            if (format == PixelFormats.Bgra32bpp) return KnownPixelFormats.Bgra8;

            throw new NotImplementedException();
        }        
    }


    sealed class MagicScalerBitmap
        : IBitmap<Pixel888>
        , PhotoSauce.MagicScaler.IPixelSource
    {
        public static MagicScalerBitmap CreateFrom(PhotoSauce.MagicScaler.IPixelSource source)
        {
            if (source.Format != PixelFormats.Bgr24bpp) throw new ArgumentException("invalid format");

            var dst = new MagicScalerBitmap(source.Width, source.Height);

            var a = new System.Drawing.Rectangle(0, 0, dst.Width, dst.Height);

            var dstb = System.Runtime.InteropServices.MemoryMarshal.AsBytes(dst._Pixels.AsSpan());

            source.CopyPixels(a, Unsafe.SizeOf<Pixel888>() * dst.Width, dstb);

            return dst;
        }



        public MagicScalerBitmap(int width, int height)
        {
            _Pixels = new Pixel888[width * height];
            Width = width;
            Height = height;
        }

        private readonly Pixel888[] _Pixels;

        public PixelFormat Format => KnownPixelFormats.Bgr8;

        Guid IPixelSource.Format => PixelFormats.Bgr24bpp;

        public int Width { get; }

        public int Height { get; }

        public Span<Pixel888> GetRowPixelsSpan(int y)
        {
            return _Pixels.AsSpan(y * Width, Width);
        }

        public void CopyPixels(Rectangle sourceArea, int cbStride, Span<byte> buffer)
        {
            for(int i=0; i < sourceArea.Height; ++i)
            {
                var y = i + sourceArea.Y;
                var r = GetRowPixelsSpan(y).Slice(sourceArea.X, sourceArea.Width);
                var b = System.Runtime.InteropServices.MemoryMarshal.AsBytes(r);

                b.CopyTo(buffer);

                buffer = buffer.Slice(cbStride);
            }
        }

        public MagicScalerBitmap Resize(int w, int h)
        {
            var settings = new ProcessImageSettings();
            settings.Width = w;
            settings.Height = h;
            settings.ResizeMode = CropScaleMode.Stretch;

            using var pipeline = MagicImageProcessor.BuildPipeline(this, settings);

            return CreateFrom(pipeline.PixelSource);
        }

        
    }

    readonly struct MagicScalerOperator<TSrcPixel, TDstPixel>
        : IBinaryOperation<TSrcPixel, TDstPixel, Matrix3x2>
        where TSrcPixel : unmanaged
        where TDstPixel : unmanaged
    {
        public static MagicScalerOperator<TSrcPixel, TDstPixel> Instance { get; } = new MagicScalerOperator<TSrcPixel, TDstPixel>();

        public Matrix3x2 Execute<TSrcBmp, TDstBmp>(TSrcBmp src, TDstBmp dst, IPixelConverter<TSrcPixel, TDstPixel> pixelConverter)
            where TSrcBmp : IReadOnlyBitmapOperand<TSrcBmp, TSrcPixel>, allows ref struct
            where TDstBmp : IBitmapOperand<TDstBmp, TDstPixel>, allows ref struct
        {






            Span<TSrcPixel> tmpRow = stackalloc TSrcPixel[dst.Width];

            for (int y = 0; y < dst.Height; ++y)
            {
                var srcRow = src.GetRowPixelsSpan(y * src.Height / dst.Height);

                for (int x = 0; x < tmpRow.Length; ++x)
                {
                    tmpRow[x] = srcRow[x * srcRow.Length / tmpRow.Length];
                }

                var dstRow = dst.GetRowPixelsSpan(y);
                pixelConverter.ConvertPixels(tmpRow, dstRow);
            }

            return Matrix3x2.CreateScale(src.Width / (float)dst.Width, src.Height / (float)dst.Height);
        }
    }
}

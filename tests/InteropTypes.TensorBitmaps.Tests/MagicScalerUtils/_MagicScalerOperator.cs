using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.X86;
using System.Text;

using InteropTypes.Numerics;
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

            throw new NotImplementedException(format.ToString());
        }

        public static Guid GetMagicScalerFormat(PixelFormat format)
        {
            if (format == KnownPixelFormats.Luminance8) return PixelFormats.Grey8bpp;
            if (format == KnownPixelFormats.Bgr8) return PixelFormats.Bgr24bpp;
            if (format == KnownPixelFormats.Bgra8) return PixelFormats.Bgra32bpp;

            throw new NotImplementedException(format.ToString());
        }        

        public static BitmapFillOperation<Matrix3x2> StretchToFit { get; } = new _MagicScalerStretchToFit();


        static ManagedBitmap<TDstPixel> ReadBitmap<TDstPixel>(System.IO.Stream stream, ProcessImageSettings settings, PixelFormat dstFmt)
            where TDstPixel: unmanaged
        {
            dstFmt.ThrowIfBytesPerPixelMismatch<TDstPixel>();

            using var pipeline = MagicImageProcessor.BuildPipeline(stream, settings);

            var wrap = new MagicScalerToBitmapWrapper(pipeline);

            return wrap.ToManagedBitmap<TDstPixel>(dstFmt);
        }
    }   


    /// <summary>
    /// Wraps a <see cref="IBitmapReader"/> to expose a MagicScaler's <see cref="IPixelSource"/>
    /// </summary>
    readonly struct BitmapToMagicScalerWrapper : IPixelSource
    {
        #region lifecycle
        public BitmapToMagicScalerWrapper(IBitmapReader srcBitmap)
        {
            Format = MagicScalerUtils.GetMagicScalerFormat(srcBitmap.Format);

            _SrcBitmap = srcBitmap;
        }

        #endregion

        #region data

        private readonly IBitmapReader _SrcBitmap;

        public Guid Format { get; }

        #endregion

        #region properties

        public int Width => _SrcBitmap.Width;
        public int Height => _SrcBitmap.Height;

        #endregion

        #region API

        void IPixelSource.CopyPixels(Rectangle sourceArea, int cbStride, Span<byte> buffer)
        {
            var bpp = _SrcBitmap.Format.BytesPerPixel;

            Span<byte> row = stackalloc Byte[_SrcBitmap.Width * bpp];

            for(int i=0; i < sourceArea.Height; ++i)
            {
                _SrcBitmap.ReadRowBytesSpan(i + sourceArea.Y, row);

                row.Slice(sourceArea.X * bpp, sourceArea.Width * bpp).CopyTo(buffer);

                if (cbStride >= buffer.Length) break;
                buffer = buffer.Slice(cbStride);
            }
        }        

        public ManagedBitmap<DstTPixel> Resize<DstTPixel>(System.Drawing.Size dstSize, PixelFormat dstFmt)
            where DstTPixel: unmanaged
        {
            var settings = new ProcessImageSettings();
            settings.Width = dstSize.Width;
            settings.Height = dstSize.Height;
            settings.ResizeMode = CropScaleMode.Stretch;

            return Resize<DstTPixel>(settings, dstFmt);
        }

        public ManagedBitmap<DstTPixel> Resize<DstTPixel>(ProcessImageSettings settings, PixelFormat dstFmt)
            where DstTPixel : unmanaged
        {
            using var pipeline = MagicImageProcessor.BuildPipeline(this, settings);

            var ps = pipeline.PixelSource;

            System.Diagnostics.Debug.Assert(ps.Width == settings.Width);
            System.Diagnostics.Debug.Assert(ps.Height == settings.Height);

            return new MagicScalerToBitmapWrapper(ps).ToManagedBitmap<DstTPixel>(dstFmt);
        }

        #endregion
    }

    /// <summary>
    /// Wraps a MagicScaler <see cref="IPixelSource"/> to expose a <see cref="IBitmapReader"/> interface
    /// </summary>
    /// <remarks>
    /// This is the mirror of <see cref="BitmapToMagicScalerWrapper"/>
    /// </remarks>
    class MagicScalerToBitmapWrapper : IBitmapReader
    {
        #region lifecycle

        public MagicScalerToBitmapWrapper(ProcessingPipeline srcPipeline)
            : this(srcPipeline.PixelSource) { }

        public MagicScalerToBitmapWrapper(IPixelSource srcBitmap)
        {
            Format = MagicScalerUtils.GetPixelFormatFromMagicScaler(srcBitmap.Format);
            _SrcBitmap = srcBitmap;
        }

        #endregion

        #region data

        private readonly IPixelSource _SrcBitmap;

        public PixelFormat Format { get; }

        #endregion

        #region properties

        public int Width => _SrcBitmap.Width;
        public int Height => _SrcBitmap.Height;

        #endregion

        #region API

        public void ReadRowBytesSpan(int y, scoped Span<byte> dst)
        {
            var rect = new System.Drawing.Rectangle(0, y, Width, 1);
            _SrcBitmap.CopyPixels(rect, dst.Length, dst);
        }        

        public ManagedBitmap<TDstPixel> ToManagedBitmap<TDstPixel>(PixelFormat dstFmt)
            where TDstPixel: unmanaged
        {
            var bmp = new ManagedBitmap<TDstPixel>(_SrcBitmap.Width, _SrcBitmap.Height, dstFmt);

            if (_SrcBitmap.Format == PixelFormats.Bgra32bpp)
            {
                var wrap = new MagicScalerToBitmapWrapper<uint>(_SrcBitmap);
                bmp.GetFillerContext<uint>().Fill(wrap);
            }

            if (_SrcBitmap.Format == PixelFormats.Bgr24bpp)
            {
                var wrap = new MagicScalerToBitmapWrapper<_XYZ888>(_SrcBitmap);
                bmp.GetFillerContext<_XYZ888>().Fill(wrap);
            }

            if (_SrcBitmap.Format == PixelFormats.Grey8bpp)
            {
                var wrap = new MagicScalerToBitmapWrapper<byte>(_SrcBitmap);
                bmp.GetFillerContext<byte>().Fill(wrap);
            }

            return bmp;
        }

        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 1)]
        private struct _XYZ888 { public byte X, Y, Z; }

        #endregion
    }

    /// <summary>
    /// Wraps a MagicScaler <see cref="IPixelSource"/> to expose a <see cref="IBitmapReader{TPixel}"/> interface
    /// </summary>
    /// <remarks>
    /// This is the mirror of <see cref="BitmapToMagicScalerWrapper"/>
    /// </remarks>
    sealed class MagicScalerToBitmapWrapper<TPixel>
        : MagicScalerToBitmapWrapper
        , IBitmapReader<TPixel>
        where TPixel: unmanaged
    {
        public MagicScalerToBitmapWrapper(IPixelSource srcBitmap)
            : base(srcBitmap)
        {
            Format.ThrowIfBytesPerPixelMismatch<TPixel>();            
        }        

        public void ReadRowPixelsSpan(int y, scoped Span<TPixel> dst)
        {
            var bytes = System.Runtime.InteropServices.MemoryMarshal.AsBytes(dst);
            ReadRowBytesSpan(y, bytes);
        }
    }


    sealed class _MagicScalerStretchToFit : BitmapFillOperation<Matrix3x2>
    {        
        public override IFillOperation<TSrcPixel, TDstPixel, Matrix3x2> GetInstance<TSrcPixel, TDstPixel>()
        {
            return _MagicScalerStretchOperator<TSrcPixel, TDstPixel>.Instance;
        }
    }

    readonly struct _MagicScalerStretchOperator<TSrcPixel, TDstPixel>
        : IFillOperation<TSrcPixel, TDstPixel, Matrix3x2>
        where TSrcPixel : unmanaged
        where TDstPixel : unmanaged
    {
        public static _MagicScalerStretchOperator<TSrcPixel, TDstPixel> Instance { get; } = new _MagicScalerStretchOperator<TSrcPixel, TDstPixel>();

        public Matrix3x2 Fill<TSrcBmp, TDstBmp>(TSrcBmp src, TDstBmp dst, IPixelConverter<TSrcPixel, TDstPixel> pixelConverter)
            where TSrcBmp : IBitmapReader<TSrcBmp, TSrcPixel>, allows ref struct
            where TDstBmp : IBitmapWriter<TDstBmp, TDstPixel>, allows ref struct
        {
            BitmapToMagicScalerWrapper? wrap = null;

            if (src.Format.Components.Length == 1)
            {
                // we need a managed bitmap
                var tmp = new ManagedBitmap<byte>(src.Width, src.Height, KnownPixelFormats.Luminance8);
                tmp.GetFillerContext<TSrcPixel>().Fill(BitmapOperations.FillCopy, src);

                // convert src to magicScaler, stretch to dst size, and copy to dst
                wrap = new BitmapToMagicScalerWrapper(tmp);
            }

            if (src.Format.Components.Length == 3)
            {
                // we need a managed bitmap
                var tmp = new ManagedBitmap<_XYZ888>(src.Width, src.Height, KnownPixelFormats.Bgr8);
                tmp.GetFillerContext<TSrcPixel>().Fill(BitmapOperations.FillCopy, src);

                // convert src to magicScaler, stretch to dst size, and copy to dst
                wrap = new BitmapToMagicScalerWrapper(tmp);                
            }

            if (src.Format.Components.Length == 4)
            {
                // we need a managed bitmap
                var tmp = new ManagedBitmap<uint>(src.Width, src.Height, KnownPixelFormats.Bgra8);
                tmp.GetFillerContext<TSrcPixel>().Fill(BitmapOperations.FillCopy, src);

                // convert src to magicScaler, stretch to dst size, and copy to dst
                wrap = new BitmapToMagicScalerWrapper(tmp);
            }

            if (!wrap.HasValue) throw new NotSupportedException(src.Format.ToString());

            var resized = wrap.Value.Resize<TDstPixel>(new System.Drawing.Size(dst.Width, dst.Height), dst.Format);

            dst.GetFillerContext<TDstPixel>().Fill(resized);

            return Matrix3x2.CreateScale(src.Width / (float)dst.Width, src.Height / (float)dst.Height);
        }


        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential,Pack =1)]
        struct _XYZ888
        {
            public byte X;
            public byte Y;
            public byte Z;
        }
    }
}

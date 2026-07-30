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
using PhotoSauce.MagicScaler.Transforms;

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

        public static BitmapBinaryOperation<Matrix3x2> StretchToFit { get; } = new _MagicScalerStretchToFit();

        public static BitmapBinaryOperation<Matrix3x2> ScaleToFit(float overflowAmount) { return new _MagicScalerScaleToFit(overflowAmount); }

    }


    sealed class MagicScalerBitmap : ManagedBitmap<Pixel888>
        , IPixelSource
    {
        #region lifecycle

        public static MagicScalerBitmap CreateFrom<TBitmap,TPixel>(TBitmap bitmap)
            where TBitmap: IReadOnlyBitmap<TBitmap, TPixel>, allows ref struct
            where TPixel: unmanaged
        {
            var dst = new MagicScalerBitmap(bitmap.Width, bitmap.Height);

            dst.Fill<TBitmap,TPixel>(bitmap);

            return dst;
        }

        public static MagicScalerBitmap CreateFrom(PhotoSauce.MagicScaler.IPixelSource source)
        {
            if (source.Format != PixelFormats.Bgr24bpp) throw new ArgumentException("invalid format");

            var dst = new MagicScalerBitmap(source.Width, source.Height);
            var dstb = System.Runtime.InteropServices.MemoryMarshal.AsBytes(dst._Pixels.AsSpan());

            var srcArea = new System.Drawing.Rectangle(0, 0, dst.Width, dst.Height);
            source.CopyPixels(srcArea, Unsafe.SizeOf<Pixel888>() * dst.Width, dstb);

            return dst;
        }

        public MagicScalerBitmap(int width, int height) : base(width,height, KnownPixelFormats.Bgr8) { }

        #endregion

        #region data        

        Guid IPixelSource.Format => PixelFormats.Bgr24bpp;

        #endregion

        #region API        

        void IPixelSource.CopyPixels(Rectangle sourceArea, int cbStride, Span<byte> buffer)
        {
            if (sourceArea.X + sourceArea.Width > this.Width) throw new ArgumentException(nameof(sourceArea));
            if (sourceArea.Y + sourceArea.Height > this.Height) throw new ArgumentException(nameof(sourceArea));

            System.Diagnostics.Debug.Assert(cbStride < buffer.Length);

            for(int i=0; i < sourceArea.Height; ++i)
            {
                var y = i + sourceArea.Y;
                var r = GetRowPixelsSpan(y).Slice(sourceArea.X, sourceArea.Width);
                var b = System.Runtime.InteropServices.MemoryMarshal.AsBytes(r);

                b.CopyTo(buffer);

                System.Diagnostics.Debug.Assert(cbStride <= buffer.Length);
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

        #endregion
    }


    sealed class _MagicScalerScaleToFit : BitmapBinaryOperation<Matrix3x2>
    {
        public _MagicScalerScaleToFit(float overflowAmount)
        {
            _overflowAmount = overflowAmount;
        }

        private readonly float _overflowAmount;
        public override IBinaryOperation<TSrcPixel, TDstPixel, Matrix3x2> GetInstance<TSrcPixel, TDstPixel>()
        {
            return new _MagicScalerScaleOperator<TSrcPixel, TDstPixel>(_overflowAmount);
        }
    }

    readonly struct _MagicScalerScaleOperator<TSrcPixel, TDstPixel>
        : IBinaryOperation<TSrcPixel, TDstPixel, Matrix3x2>
        where TSrcPixel : unmanaged
        where TDstPixel : unmanaged
    {
        public _MagicScalerScaleOperator(float overflowAmount)
        {
            _OverflowAmount = overflowAmount;
        }

        private readonly float _OverflowAmount;        

        public Matrix3x2 Execute<TSrcBmp, TDstBmp>(TSrcBmp src, TDstBmp dst, IPixelConverter<TSrcPixel, TDstPixel> pixelConverter)
            where TSrcBmp : IReadOnlyBitmap<TSrcBmp, TSrcPixel>, allows ref struct
            where TDstBmp : IBitmap<TDstBmp, TDstPixel>, allows ref struct
        {
            var l = new System.Drawing.Size(src.Width, src.Height);
            var r = new System.Drawing.Size(dst.Width, dst.Height);
            var crops = ScaledIntersectionCrop.CreateFrom(l, r, _OverflowAmount);

            src = src.GetCropped(crops.SourceCrop);
            dst = dst.GetCropped(crops.TargetCrop);

            var xform = _MagicScalerStretchOperator<TSrcPixel, TDstPixel>.Instance.Execute(src, dst, pixelConverter);

            return crops.GetTransform(xform);
        }
    }



    sealed class _MagicScalerStretchToFit : BitmapBinaryOperation<Matrix3x2>
    {        
        public override IBinaryOperation<TSrcPixel, TDstPixel, Matrix3x2> GetInstance<TSrcPixel, TDstPixel>()
        {
            return _MagicScalerStretchOperator<TSrcPixel, TDstPixel>.Instance;
        }
    }

    readonly struct _MagicScalerStretchOperator<TSrcPixel, TDstPixel>
        : IBinaryOperation<TSrcPixel, TDstPixel, Matrix3x2>
        where TSrcPixel : unmanaged
        where TDstPixel : unmanaged
    {
        public static _MagicScalerStretchOperator<TSrcPixel, TDstPixel> Instance { get; } = new _MagicScalerStretchOperator<TSrcPixel, TDstPixel>();

        public Matrix3x2 Execute<TSrcBmp, TDstBmp>(TSrcBmp src, TDstBmp dst, IPixelConverter<TSrcPixel, TDstPixel> pixelConverter)
            where TSrcBmp : IReadOnlyBitmap<TSrcBmp, TSrcPixel>, allows ref struct
            where TDstBmp : IBitmap<TDstBmp, TDstPixel>, allows ref struct
        {
            // convert src to magicScaler, stretch to dst size, and copy to dst
            var tmp = MagicScalerBitmap.CreateFrom<TSrcBmp, TSrcPixel>(src);
            tmp = tmp.Resize(dst.Width, dst.Height);
            dst.GetContext<Pixel888>().Fill(tmp);

            return Matrix3x2.CreateScale(src.Width / (float)dst.Width, src.Height / (float)dst.Height);
        }
    }
}

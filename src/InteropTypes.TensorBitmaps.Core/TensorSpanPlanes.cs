using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Numerics.Tensors;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

using InteropTypes.Numerics;
using InteropTypes.TensorBitmaps.Operands;
using InteropTypes.TensorBitmaps.Operators;

namespace InteropTypes.TensorBitmaps
{
    /// <summary>
    /// A CHW 3 planar bitmap backed by three <see cref="TensorSpanBitmap{TElement, TElement}"/>
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("TensorSpanPlanes3 {Width}x{Height}")]
    public readonly ref struct TensorSpanPlanes3<TElement>
        where TElement : unmanaged , INumber<TElement>
    {
        #region lifecycle

        public static TensorSpanPlanes3<TElement> Create(int width, int height, PixelFormat format)
        {
            var t = Tensor.Create(new TElement[3 * height * width], [3, height, width]);

            return Create(t, format);
        }

        public static TensorSpanPlanes3<TElement> Create(TensorSpan<TElement> tensor, PixelFormat format)
        {
            if (tensor.Lengths[0] < 3) throw new ArgumentOutOfRangeException("the tensor has less than 3 planes", nameof(tensor));
            if (format.Components.Length < 3) throw new ArgumentOutOfRangeException("the format has less than 3 components", nameof(format));

            return new TensorSpanPlanes3<TElement>(tensor, format, 0, 1, 2);
        }

        public static TensorSpanPlanes3<TElement> Create(TensorSpan<TElement> tensor, PixelFormat format, string fx, string fy, string fz)
        {
            if (tensor.Lengths[0] < 3) throw new ArgumentOutOfRangeException("the tensor has less than 3 planes", nameof(tensor));
            if (format.Components.Length < 3) throw new ArgumentOutOfRangeException("the format has less than 3 components", nameof(format));

            int x_idx = format.IndexOf(fx);
            if (x_idx < 0) throw new ArgumentException($"semantic {fx} not found in format", nameof(fx));

            int y_idx = format.IndexOf(fy);
            if (y_idx < 0) throw new ArgumentException($"semantic {fy} not found in format", nameof(fy));

            int z_idx = format.IndexOf(fz);
            if (z_idx < 0) throw new ArgumentException($"semantic {fz} not found in format", nameof(fz));

            return new TensorSpanPlanes3<TElement>(tensor, format, x_idx, y_idx, z_idx);
        }

        private TensorSpanPlanes3(TensorSpan<TElement> tensor, PixelFormat format, int x,int y, int z)
        {
            var planes = tensor.GetDimensionSpan(0);

            _PlaneX = new TensorSpanBitmap<TElement, TElement>(planes[x], new PixelFormat(format.Components[x]));
            _PlaneY = new TensorSpanBitmap<TElement, TElement>(planes[y], new PixelFormat(format.Components[y]));
            _PlaneZ = new TensorSpanBitmap<TElement, TElement>(planes[z], new PixelFormat(format.Components[z]));

            _Format = new PixelFormat(format.Components[x], format.Components[y], format.Components[z]);
        }

        public TensorSpanPlanes3(TensorSpanBitmap<TElement, TElement> planeX, TensorSpanBitmap<TElement, TElement> planeY, TensorSpanBitmap<TElement, TElement> planeZ, PixelFormat format)
        {
            _PlaneX = planeX;
            _PlaneY = planeY;
            _PlaneZ = planeZ;
            _Format = format;
        }

        #endregion

        #region data

        private readonly TensorSpanBitmap<TElement, TElement> _PlaneX;
        private readonly TensorSpanBitmap<TElement, TElement> _PlaneY;
        private readonly TensorSpanBitmap<TElement, TElement> _PlaneZ;
        private readonly PixelFormat _Format;

        #endregion

        #region properties

        public int Width => _PlaneX.Width;
        public int Height => _PlaneX.Height;

        public PixelFormat Format => _Format;

        public TensorSpanBitmap<TElement, TElement> PlaneX => _PlaneX;
        public TensorSpanBitmap<TElement, TElement> PlaneY => _PlaneY;
        public TensorSpanBitmap<TElement, TElement> PlaneZ => _PlaneZ;

        #endregion

        #region API

        public void GetRowPixelSpans(int y, out Span<TElement> planex, out Span<TElement> planey, out Span<TElement> planez)
        {
            planex = _PlaneX.GetRowPixelsSpan(y);
            planey = _PlaneY.GetRowPixelsSpan(y);
            planez = _PlaneZ.GetRowPixelsSpan(y);
        }

        public TensorSpanPlanes3<TElement> GetCropped(System.Drawing.Rectangle rectangle)
        {
            rectangle.Intersect(new System.Drawing.Rectangle(0, 0, Width, Height));
            if (rectangle.IsEmpty) throw new ArgumentException("nothing to crop");

            var x = _PlaneX.GetCropped(rectangle);
            var y = _PlaneY.GetCropped(rectangle);
            var z = _PlaneZ.GetCropped(rectangle);

            return new TensorSpanPlanes3<TElement>(x, y, z, Format);
        }

        public TensorSpanPlanes3OperatorContext<TensorSpanBitmap<TElement, TElement>, TElement, TContextPixel> GetContext<TContextPixel>()
            where TContextPixel : unmanaged
        {
            return new TensorSpanPlanes3OperatorContext<TensorSpanBitmap<TElement, TElement>, TElement, TContextPixel>(_PlaneX,_PlaneY,_PlaneZ);
        }

        #endregion
    }

    /// <summary>
    /// Transform context for <see cref="TensorSpanPlanes3{TElement}"/>
    /// </summary>
    /// <typeparam name="TPlaneBitmap">The underlaying bitmap type for each plane.</typeparam>
    /// <typeparam name="TPlanePixel">The bitmap plane pixel type.</typeparam>
    /// <typeparam name="TContextPixel">The pixel type in which this context operates.</typeparam>
    public readonly ref struct TensorSpanPlanes3OperatorContext<TPlaneBitmap, TPlanePixel, TContextPixel>
        where TPlaneBitmap : IBitmapOperand<TPlaneBitmap, TPlanePixel>, allows ref struct
        where TPlanePixel : unmanaged
        where TContextPixel : unmanaged
    {
        #region lifecycle
        public TensorSpanPlanes3OperatorContext(TPlaneBitmap planex, TPlaneBitmap planey, TPlaneBitmap planez)
        {
            _DstPlaneX = planex;
            _DstPlaneY = planey;
            _DstPlaneZ = planez;
        }

        #endregion

        #region data

        private readonly TPlaneBitmap _DstPlaneX;
        private readonly TPlaneBitmap _DstPlaneY;
        private readonly TPlaneBitmap _DstPlaneZ;

        #endregion

        #region API

        public int Width => _DstPlaneX.Width;

        public int Height => _DstPlaneX.Height;

        public TResult Fill<TResult>(BitmapBinaryOperation<TResult> transform, IReadOnlyBitmap<TContextPixel> srcBmp)
        {
            var srcRef = new ManagedReadOnlyBitmapOperand<TContextPixel>(srcBmp);
            return Fill(transform, srcRef);            
        }

        public TResult Fill<TSrcBitmap, TResult>(BitmapBinaryOperation<TResult> transform, TSrcBitmap srcBmp)
            where TSrcBitmap : IReadOnlyBitmapOperand<TSrcBitmap, TContextPixel>, allows ref struct
        {
            var xform = transform.GetInstance<TContextPixel, TPlanePixel>();

            // we need to create a pixel converter for each plane
            var x = xform.Execute(srcBmp, _DstPlaneX, true);
            var y = xform.Execute(srcBmp, _DstPlaneY, true);
            var z = xform.Execute(srcBmp, _DstPlaneZ, true);

            return x;
        }

        public TResult CopyTo<TResult>(BitmapBinaryOperation<TResult> transform, IBitmap<TContextPixel> dstBmp)            
        {
            var dstRef = new ManagedBitmapOperand<TContextPixel>(dstBmp);
            return CopyTo(transform, dstRef);
        }

        public TResult CopyTo<TDstBitmap, TResult>(BitmapBinaryOperation<TResult> transform, TDstBitmap dstBmp)
            where TDstBitmap : IBitmapOperand<TDstBitmap, TContextPixel>, allows ref struct
        {
            var x = dstBmp.GetContext<TPlanePixel>().Fill(transform, _DstPlaneX, false);
            var y = dstBmp.GetContext<TPlanePixel>().Fill(transform, _DstPlaneY, false);
            var z = dstBmp.GetContext<TPlanePixel>().Fill(transform, _DstPlaneZ, false);
            return x;
        }

        /// <summary>
        /// Optimized path for <see cref="Fill{TSrcBitmap, TResult}(BitmapBinaryOperation{TResult}, TSrcBitmap)"/> with <see cref="BitmapOperations.StretchToFit"/>
        /// </summary>        
        public Matrix3x2 FillStretched<TSrcBmp>(TSrcBmp src)
            where TSrcBmp : IReadOnlyBitmapOperand<TSrcBmp, TContextPixel>, allows ref struct            
        {
            var pcx = IPixelConverter<TContextPixel, TPlanePixel>.Create(src.Format, _DstPlaneX.Format, true);
            var pcy = IPixelConverter<TContextPixel, TPlanePixel>.Create(src.Format, _DstPlaneY.Format, true);
            var pcz = IPixelConverter<TContextPixel, TPlanePixel>.Create(src.Format, _DstPlaneZ.Format, true);

            if (src.TryCastTo<IReadOnlyBitmap<TContextPixel>>(out var srcManaged) && 
                IClientReadOnlyBitmap<TContextPixel>.TryCreateStretched(srcManaged, this.Width, this.Height, out var stretchedBitmap))
            {
                System.Diagnostics.Debug.Assert(this.Width == stretchedBitmap.Width);
                System.Diagnostics.Debug.Assert(this.Height == stretchedBitmap.Height);

                var h = Math.Min(stretchedBitmap.Height, this.Height);                

                for (int y = 0; y < h; ++y)
                {
                    var srcRow = stretchedBitmap.GetRowPixelsSpan(y);
                    pcx.ConvertPixels(srcRow, _DstPlaneX.GetRowPixelsSpan(y));
                    pcy.ConvertPixels(srcRow, _DstPlaneY.GetRowPixelsSpan(y));
                    pcz.ConvertPixels(srcRow, _DstPlaneZ.GetRowPixelsSpan(y));
                }

                stretchedBitmap.Dispose();
            }
            else
            {
                Span<TContextPixel> tmpRow = stackalloc TContextPixel[Width];

                for (int y = 0; y < Height; ++y)
                {
                    var srcRow = src.GetRowPixelsSpan(y * src.Height / Height);

                    for (int x = 0; x < tmpRow.Length; ++x)
                    {
                        tmpRow[x] = srcRow[x * srcRow.Length / tmpRow.Length];
                    }

                    pcx.ConvertPixels(tmpRow, _DstPlaneX.GetRowPixelsSpan(y));
                    pcy.ConvertPixels(tmpRow, _DstPlaneY.GetRowPixelsSpan(y));
                    pcz.ConvertPixels(tmpRow, _DstPlaneZ.GetRowPixelsSpan(y));
                }
            }

            return Matrix3x2.CreateScale(src.Width / (float)Width, src.Height / (float)Height);
        }

        /// <summary>
        /// Optimized path for <see cref="Fill{TSrcBitmap, TResult}(BitmapBinaryOperation{TResult}, TSrcBitmap)"/> with <see cref="BitmapOperations.ScaleToFit(float)"/>
        /// </summary>        
        public Matrix3x2 FillScaled<TSrcBmp>(TSrcBmp src, float overflowAmount)
            where TSrcBmp : IReadOnlyBitmapOperand<TSrcBmp, TContextPixel>, allows ref struct
        {
            var crops = ScaledIntersectionCrop.CreateFrom(new System.Drawing.Size(src.Width, src.Height), new System.Drawing.Size(this.Width, this.Height), overflowAmount);
            
            src = src.GetCropped(crops.SourceCrop);
            
            var dstx = _DstPlaneX.GetCropped(crops.TargetCrop);
            var dsty = _DstPlaneY.GetCropped(crops.TargetCrop);
            var dstz = _DstPlaneZ.GetCropped(crops.TargetCrop);
            var dst = new TensorSpanPlanes3OperatorContext<TPlaneBitmap, TPlanePixel, TContextPixel>(dstx, dsty, dstz);            

            return crops.GetTransform(dst.FillStretched(src));
        }

        #endregion
    }
}

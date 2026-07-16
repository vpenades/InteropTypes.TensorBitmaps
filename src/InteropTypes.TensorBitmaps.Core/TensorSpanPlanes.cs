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

        public TensorSpanPlanes3<TElement> GetCropped(System.Drawing.Rectangle rectangle)
        {
            rectangle.Intersect(new System.Drawing.Rectangle(0, 0, Width, Height));
            if (rectangle.IsEmpty) throw new ArgumentException("nothing to crop");

            var x = _PlaneX.GetCropped(rectangle);
            var y = _PlaneY.GetCropped(rectangle);
            var z = _PlaneZ.GetCropped(rectangle);

            return new TensorSpanPlanes3<TElement>(x, y, z, Format);
        }

        public TensorSpanPlanes3OperatorContext<TensorSpanBitmap<TElement, TElement>, TElement, TSrcPixel> GetContext<TSrcPixel>()
            where TSrcPixel : unmanaged
        {
            return new TensorSpanPlanes3OperatorContext<TensorSpanBitmap<TElement, TElement>, TElement, TSrcPixel>(_PlaneX,_PlaneY,_PlaneZ);
        }

        #endregion
    }

    public readonly ref struct TensorSpanPlanes3OperatorContext<TBitmap, TPixel, TContextPixel>
        where TBitmap : IBitmapOperand<TBitmap, TPixel>, allows ref struct
        where TPixel : unmanaged
        where TContextPixel : unmanaged
    {
        public TensorSpanPlanes3OperatorContext(TBitmap planex, TBitmap planey, TBitmap planez)
        {
            _DstPlaneX = planex;
            _DstPlaneY = planey;
            _DstPlaneZ = planez;
        }

        private readonly TBitmap _DstPlaneX;
        private readonly TBitmap _DstPlaneY;
        private readonly TBitmap _DstPlaneZ;

        public TResult Fill<TSrcBitmap, TResult>(PixelsTransform<TResult> transform, TSrcBitmap srcBmp, bool initPixels = true)
            where TSrcBitmap : IReadOnlyBitmapOperand<TSrcBitmap, TContextPixel>, allows ref struct
        {
            var xform = transform.GetInstance<TContextPixel, TPixel>();

            var x = xform.Execute(srcBmp, _DstPlaneX, initPixels);
            var y = xform.Execute(srcBmp, _DstPlaneY, initPixels);
            var z = xform.Execute(srcBmp, _DstPlaneZ, initPixels);

            return x;
        }

        public TResult Fill<TSrcBitmap, TResult>(PixelsTransform<TResult> transform, TSrcBitmap srcBmp, IPixelConverter<TContextPixel, TPixel> pixelConverter)
            where TSrcBitmap : IReadOnlyBitmapOperand<TSrcBitmap, TContextPixel>, allows ref struct
        {
            var xform = transform.GetInstance<TContextPixel, TPixel>();

            var x = xform.Execute(srcBmp, _DstPlaneX, pixelConverter);
            var y = xform.Execute(srcBmp, _DstPlaneY, pixelConverter);
            var z = xform.Execute(srcBmp, _DstPlaneZ, pixelConverter);

            return x;
        }

        public TResult CopyTo<TDstBitmap, TResult>(PixelsTransform<TResult> transform, TDstBitmap dst)
            where TDstBitmap : IBitmapOperand<TDstBitmap, TContextPixel>, allows ref struct
        {
            var x = dst.GetContext<TPixel>().Fill(transform, _DstPlaneX, false);
            var y = dst.GetContext<TPixel>().Fill(transform, _DstPlaneY, false);
            var z = dst.GetContext<TPixel>().Fill(transform, _DstPlaneZ, false);
            return x;
        }
    }    
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Numerics.Tensors;
using System.Text;
using System.Threading.Tasks;

namespace InteropTypes.TensorBitmaps
{

    /// <summary>
    /// A CHW 3 planar bitmap backed by three <see cref="TensorSpanBitmap{TElement, TElement}"/>
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("TensorSpanPlanes3 {Width}x{Height}")]
    public readonly ref struct TensorSpanPlanes3<TElement>
        where TElement : unmanaged , INumber<TElement>
    {
        public static TensorSpanPlanes3<TElement> Create(int width, int height, TensorPixelFormat format)
        {
            var t = System.Numerics.Tensors.Tensor.Create(new TElement[3 * height * width], [3, height, width]);

            return Create(t, format);
        }

        public static TensorSpanPlanes3<TElement> Create(System.Numerics.Tensors.TensorSpan<TElement> tensor, TensorPixelFormat format)
        {
            if (tensor.Lengths[0] < 3) throw new ArgumentOutOfRangeException("the tensor has less than 3 planes", nameof(tensor));
            if (format.Components.Count < 3) throw new ArgumentOutOfRangeException("the format has less than 3 components", nameof(format));

            return new TensorSpanPlanes3<TElement>(tensor, format, 0, 1, 2);
        }

        public static TensorSpanPlanes3<TElement> Create(System.Numerics.Tensors.TensorSpan<TElement> tensor, TensorPixelFormat format, string fx, string fy, string fz)
        {
            if (tensor.Lengths[0] < 3) throw new ArgumentOutOfRangeException("the tensor has less than 3 planes", nameof(tensor));
            if (format.Components.Count < 3) throw new ArgumentOutOfRangeException("the format has less than 3 components", nameof(format));

            int x_idx = format.IndexOf(fx);
            if (x_idx < 0) throw new ArgumentException($"semantic {fx} not found in format", nameof(fx));

            int y_idx = format.IndexOf(fy);
            if (y_idx < 0) throw new ArgumentException($"semantic {fy} not found in format", nameof(fy));

            int z_idx = format.IndexOf(fz);
            if (z_idx < 0) throw new ArgumentException($"semantic {fz} not found in format", nameof(fz));

            return new TensorSpanPlanes3<TElement>(tensor, format, x_idx, y_idx, z_idx);
        }

        private TensorSpanPlanes3(System.Numerics.Tensors.TensorSpan<TElement> tensor, TensorPixelFormat format, int x,int y, int z)
        {
            var planes = tensor.GetDimensionSpan(0);

            _PlaneX = new TensorSpanBitmap<TElement, TElement>(planes[x], new TensorPixelFormat(format.Components[x]));
            _PlaneY = new TensorSpanBitmap<TElement, TElement>(planes[y], new TensorPixelFormat(format.Components[y]));
            _PlaneZ = new TensorSpanBitmap<TElement, TElement>(planes[z], new TensorPixelFormat(format.Components[z]));

            _Format = new TensorPixelFormat(format.Components[x], format.Components[y], format.Components[z]);
        }

        public TensorSpanPlanes3(TensorSpanBitmap<TElement, TElement> planeX, TensorSpanBitmap<TElement, TElement> planeY, TensorSpanBitmap<TElement, TElement> planeZ, TensorPixelFormat format)
        {
            _PlaneX = planeX;
            _PlaneY = planeY;
            _PlaneZ = planeZ;
            _Format = format;
        }

        private readonly TensorSpanBitmap<TElement, TElement> _PlaneX;
        private readonly TensorSpanBitmap<TElement, TElement> _PlaneY;
        private readonly TensorSpanBitmap<TElement, TElement> _PlaneZ;
        private readonly TensorPixelFormat _Format;

        public int Width => _PlaneX.Width;
        public int Height => _PlaneX.Height;

        public TensorPixelFormat Format => _Format;

        public TensorSpanBitmap<TElement, TElement> PlaneX => _PlaneX;
        public TensorSpanBitmap<TElement, TElement> PlaneY => _PlaneY;
        public TensorSpanBitmap<TElement, TElement> PlaneZ => _PlaneZ;

        public void CopyPixelsFrom<TSrcElement, TSrcPixel>(ReadOnlyTensorSpanBitmap<TSrcElement, TSrcPixel> srcBitmap)
            where TSrcElement : unmanaged, INumber<TSrcElement>
            where TSrcPixel : unmanaged
        {
            srcBitmap.CopyPixelsTo(_PlaneX);
            srcBitmap.CopyPixelsTo(_PlaneY);
            srcBitmap.CopyPixelsTo(_PlaneZ);
        }

        public void CopyPixelsTo<TDstElement, TDstPixel>(TensorBitmap<TDstElement, TDstPixel> dstBitmap)
            where TDstElement : unmanaged, INumber<TDstElement>
            where TDstPixel : unmanaged
        {
            CopyPixelsTo(dstBitmap.AsTensorSpanBitmap());
        }

        public void CopyPixelsTo<TDstElement,TDstPixel>(TensorSpanBitmap<TDstElement, TDstPixel> dstBitmap)
            where TDstElement:unmanaged, INumber<TDstElement>
            where TDstPixel:unmanaged
        {
            _PlaneX.CopyPixelsTo(dstBitmap, false);
            _PlaneY.CopyPixelsTo(dstBitmap, false);
            _PlaneZ.CopyPixelsTo(dstBitmap, false);
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

    }
}

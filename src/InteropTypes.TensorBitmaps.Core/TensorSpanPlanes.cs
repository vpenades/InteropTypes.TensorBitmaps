using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Numerics.Tensors;
using System.Text;
using System.Threading.Tasks;

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

        private readonly TensorSpanBitmap<TElement, TElement> _PlaneX;
        private readonly TensorSpanBitmap<TElement, TElement> _PlaneY;
        private readonly TensorSpanBitmap<TElement, TElement> _PlaneZ;
        private readonly PixelFormat _Format;

        public int Width => _PlaneX.Width;
        public int Height => _PlaneX.Height;

        public PixelFormat Format => _Format;

        public TensorSpanBitmap<TElement, TElement> PlaneX => _PlaneX;
        public TensorSpanBitmap<TElement, TElement> PlaneY => _PlaneY;
        public TensorSpanBitmap<TElement, TElement> PlaneZ => _PlaneZ;

        public TensorSpanPlanes3<TElement> GetCropped(System.Drawing.Rectangle rectangle)
        {
            rectangle.Intersect(new System.Drawing.Rectangle(0, 0, Width, Height));
            if (rectangle.IsEmpty) throw new ArgumentException("nothing to crop");

            var x = _PlaneX.GetCropped(rectangle);
            var y = _PlaneY.GetCropped(rectangle);
            var z = _PlaneZ.GetCropped(rectangle);

            return new TensorSpanPlanes3<TElement>(x, y, z, Format);
        }



        public void CopyPixelsFrom<TSrcBitmap, TSrcPixel>(TSrcBitmap srcBitmap, TSrcPixel defVal)
            where TSrcBitmap : IReadOnlyBitmapOperand<TSrcBitmap, TSrcPixel>, allows ref struct
            where TSrcPixel : unmanaged
        {
            CopyPixelsFrom<TSrcBitmap, TSrcPixel, int>(PixelsTransform.Copy, srcBitmap);
        }


        public TResult CopyPixelsFrom<TSrcBitmap, TSrcPixel, TResult>(PixelsTransformFrom<TSrcPixel,TResult> xform, TSrcBitmap srcBitmap)
            where TSrcBitmap : IReadOnlyBitmapOperand<TSrcBitmap, TSrcPixel>, allows ref struct
            where TSrcPixel : unmanaged
        {
            var x = _PlaneX.GetContext(xform).ApplyFrom(srcBitmap, true);
            var y = _PlaneY.GetContext(xform).ApplyFrom(srcBitmap, true);
            var z = _PlaneZ.GetContext(xform).ApplyFrom(srcBitmap, true);

            return z;
        }

        public TResult CopyPixelsFrom<TSrcBitmap, TSrcPixel, TResult>(PixelsTransform<TResult> transform, TSrcBitmap srcBitmap)
            where TSrcBitmap : IReadOnlyBitmapOperand<TSrcBitmap,TSrcPixel>, allows ref struct
            where TSrcPixel : unmanaged
        {
            var x = _PlaneX.GetContext<TSrcPixel, TResult>(transform).ApplyFrom(srcBitmap, true);
            var y = _PlaneY.GetContext<TSrcPixel, TResult>(transform).ApplyFrom(srcBitmap, true);
            var z = _PlaneZ.GetContext<TSrcPixel, TResult>(transform).ApplyFrom(srcBitmap, true);

            return z;
        }
        public void CopyPixelsTo<TDstBitmap, TDstPixel>(TDstBitmap dstBitmap, TDstPixel defVal)
            where TDstBitmap : IBitmapOperand<TDstBitmap, TDstPixel>, allows ref struct
            where TDstPixel : unmanaged
        {
            CopyPixelsTo<TDstBitmap, TDstPixel, int>(PixelsTransform.Copy, dstBitmap);
        }
        public TResult CopyPixelsTo<TDstBitmap, TDstPixel, TResult>(PixelsTransform<TResult> transform, TDstBitmap dstBitmap)
            where TDstBitmap : IBitmapOperand<TDstBitmap, TDstPixel>, allows ref struct
            where TDstPixel : unmanaged
        {
            var x = dstBitmap.GetContext<TElement, TResult>(transform).ApplyFrom(_PlaneX, false);
            var y = dstBitmap.GetContext<TElement, TResult>(transform).ApplyFrom(_PlaneY, false);
            var z = dstBitmap.GetContext<TElement, TResult>(transform).ApplyFrom(_PlaneZ, false);

            return x;
        }

    }
}

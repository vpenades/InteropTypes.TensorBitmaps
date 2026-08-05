using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Numerics.Tensors;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

using InteropTypes.Numerics;
using InteropTypes.TensorBitmaps.Operators;

namespace InteropTypes.TensorBitmaps
{

    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack =1)]
    public struct PlanesPixel3<TElement>
    {
        public PlanesPixel3(TElement x, TElement y, TElement z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public TElement X;
        public TElement Y;
        public TElement Z;        
    }


    /// <summary>
    /// A CHW 3 planar bitmap backed by three <see cref="TensorSpanBitmap{TElement, TElement}"/>
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("TensorSpanPlanes3 {Width}x{Height}")]
    public readonly ref struct TensorSpanPlanes3<TElement> : IBitmapWriter<TensorSpanPlanes3<TElement>, PlanesPixel3<TElement>>
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

        #region API - Rows

        public void WriteRowPixelsSpan(int y, scoped ReadOnlySpan<PlanesPixel3<TElement>> src)
        {
            var r0 = _PlaneX.GetRowPixelsSpan(y);
            var r1 = _PlaneY.GetRowPixelsSpan(y);
            var r2 = _PlaneZ.GetRowPixelsSpan(y);

            var w = Math.Min(r0.Length, src.Length);

            for(int x=0; x < w; ++x)
            {
                r0[x] = src[x].X;
                r1[x] = src[x].Y;
                r2[x] = src[x].Z;
            }
        }
        public void ReadRowPixelsSpan(int y, scoped Span<PlanesPixel3<TElement>> dst)
        {
            var r0 = _PlaneX.GetRowPixelsSpan(y);
            var r1 = _PlaneY.GetRowPixelsSpan(y);
            var r2 = _PlaneZ.GetRowPixelsSpan(y);

            var w = Math.Min(r0.Length, dst.Length);

            for (int x = 0; x < w; ++x)
            {
                dst[x] = new PlanesPixel3<TElement>(r0[x], r1[x], r2[x]);
            }
        }

        public void WriteRowBytesSpan(int y, ReadOnlySpan<byte> src) => WriteRowPixelsSpan(y, System.Runtime.InteropServices.MemoryMarshal.Cast<byte, PlanesPixel3<TElement>>(src));
        public void ReadRowBytesSpan(int y, Span<byte> dst) => ReadRowPixelsSpan(y, System.Runtime.InteropServices.MemoryMarshal.Cast<byte, PlanesPixel3<TElement>>(dst));


        public void GetRowPixelSpans(int y, out Span<TElement> planex, out Span<TElement> planey, out Span<TElement> planez)
        {
            planex = _PlaneX.GetRowPixelsSpan(y);
            planey = _PlaneY.GetRowPixelsSpan(y);
            planez = _PlaneZ.GetRowPixelsSpan(y);
        }

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

        public FillerContext<TensorSpanPlanes3<TElement>, PlanesPixel3<TElement>, TContextPixel> GetFillerContext<TContextPixel>() where TContextPixel : unmanaged
        {
            return new FillerContext<TensorSpanPlanes3<TElement>, PlanesPixel3<TElement>, TContextPixel>(this);
        }        

        #endregion
    }
}

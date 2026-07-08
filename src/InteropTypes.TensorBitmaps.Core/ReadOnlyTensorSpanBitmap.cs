using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics.Tensors;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace InteropTypes.TensorBitmaps
{
    /// <summary>
    /// A bitmap backed by a <see cref="ReadOnlyTensorSpanBitmap{TElement}"/>
    /// </summary>
    /// <typeparam name="TElement">The type of the backing tensor</typeparam>
    /// <typeparam name="TPixel">The type of the bitmap's pixel</typeparam>
    public readonly ref struct ReadOnlyTensorSpanBitmap<TElement, TPixel>
        where TElement : unmanaged
        where TPixel : unmanaged
    {
        public static implicit operator ReadOnlyTensorSpanBitmap<TElement, TPixel>(TensorBitmap<TElement, TPixel> bitmap)
        {
            var tensor = bitmap.Tensor.AsReadOnlyTensorSpan();
            return new ReadOnlyTensorSpanBitmap<TElement, TPixel>(tensor, bitmap._Info, bitmap.Format);
        }

        public static implicit operator ReadOnlyTensorSpanBitmap<TElement, TPixel>(TensorSpanBitmap<TElement, TPixel> bitmap)
        {
            var tensor = bitmap.Tensor.AsReadOnlyTensorSpan();
            return new ReadOnlyTensorSpanBitmap<TElement, TPixel>(tensor, bitmap._Info, bitmap.Format);
        }

        private ReadOnlyTensorSpanBitmap(System.Numerics.Tensors.ReadOnlyTensorSpan<TElement> tensor, _TensorBitmapInfo info, TensorPixelFormat format)
        {
            _Info = info;
            Format = format;
            Width = _Info.GetWidthFrom(tensor);
            Height = _Info.GetHeightFrom(tensor);
            Tensor = tensor;
        }

        public ReadOnlyTensorSpanBitmap(System.Numerics.Tensors.ReadOnlyTensorSpan<TElement> tensor, TensorPixelFormat format)
        {
            if (tensor == null) throw new ArgumentNullException(nameof(tensor));

            _Info = new _TensorBitmapInfo(tensor.Lengths);

            _Info._Validate<TElement, TPixel>(tensor, format);

            Format = format;
            Width = _Info.GetWidthFrom(tensor);
            Height = _Info.GetHeightFrom(tensor);
            Tensor = tensor;
            _Rows = _Info.GetRows(tensor);
        }        

        internal readonly _TensorBitmapInfo _Info;
        public TensorPixelFormat Format { get; }
        public int Width { get; }
        public int Height { get; }

        public System.Numerics.Tensors.ReadOnlyTensorSpan<TElement> Tensor { get; }

        private readonly ReadOnlyTensorDimensionSpan<TElement> _Rows;


        [System.Runtime.CompilerServices.MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public ReadOnlySpan<TPixel> GetRowPixelsSpan(int y)
        {
            var trow = _Rows[y];

            var row = trow.GetSpan(_Info._RowIndices, (int)trow.FlattenedLength);

            var pixels = System.Runtime.InteropServices.MemoryMarshal.Cast<TElement, TPixel>(row);

            System.Diagnostics.Debug.Assert(pixels.Length == Width);

            return pixels;
        }

        /// <summary>
        /// Gets a new cropped bitmap that references the original surface without allocating new memory.
        /// </summary>
        public ReadOnlyTensorSpanBitmap<TElement, TPixel> GetCropped(System.Drawing.Rectangle rectangle)
        {
            rectangle.Intersect(new System.Drawing.Rectangle(0, 0, Width, Height));
            if (rectangle.IsEmpty) throw new ArgumentException("nothing to crop");

            var ranges = _Info.CalculateSlice(Tensor.Rank, rectangle);

            return new ReadOnlyTensorSpanBitmap<TElement, TPixel>(Tensor.Slice(ranges), Format);
        }

        public ReadOnlyTensorSpanBitmap<TElement, TPixelOut> Cast<TPixelOut>()
            where TPixelOut : unmanaged
        {
            if (Unsafe.SizeOf<TPixel>() != Unsafe.SizeOf<TPixelOut>()) throw new InvalidOperationException("Pixel size mismatch");
            return new ReadOnlyTensorSpanBitmap<TElement, TPixelOut>(this.Tensor, this.Format);
        }

        public void CopyPixelsTo<TOtherElement, TOtherPixel>(TensorSpanBitmap<TOtherElement, TOtherPixel> dstBitmap, bool initPixels = true)
            where TOtherElement : unmanaged
            where TOtherPixel: unmanaged
        {
            var pixelConverter = PixelConverters.Create<TElement, TPixel, TOtherElement, TOtherPixel>(this.Format, dstBitmap.Format, initPixels);

            CopyPixelsTo<TOtherElement, TOtherPixel>(dstBitmap, pixelConverter);
        }

        public void CopyPixelsTo<TOtherElement, TOtherPixel>(TensorSpanBitmap<TOtherElement, TOtherPixel> dstBitmap, IPixelConverter<TPixel,TOtherPixel> pixelConverter)
            where TOtherElement : unmanaged
            where TOtherPixel : unmanaged
        {
            var h = Math.Min(this.Height, dstBitmap.Height);            

            for (int y = 0; y < h; ++y)
            {
                var srcRow = this.GetRowPixelsSpan(y);
                var dstRow = dstBitmap.GetRowPixelsSpan(y);
                pixelConverter.ConvertPixels(srcRow, dstRow);
            }
        }
    }
}

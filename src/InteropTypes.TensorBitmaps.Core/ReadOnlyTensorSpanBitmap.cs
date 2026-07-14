using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Numerics.Tensors;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using InteropTypes.Numerics;
using InteropTypes.TensorBitmaps.Operands;

namespace InteropTypes.TensorBitmaps
{
    /// <summary>
    /// A bitmap backed by a <see cref="ReadOnlyTensorSpanBitmap{TElement}"/>
    /// </summary>
    /// <typeparam name="TElement">The type of the backing tensor</typeparam>
    /// <typeparam name="TPixel">The type of the bitmap's pixel</typeparam>
    public readonly ref struct ReadOnlyTensorSpanBitmap<TElement, TPixel>
        : IReadOnlyBitmapOperand<ReadOnlyTensorSpanBitmap<TElement, TPixel>, TPixel>        
        where TElement : unmanaged, INumber<TElement>
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

        private ReadOnlyTensorSpanBitmap(ReadOnlyTensorSpan<TElement> tensor, _TensorBitmapInfo info, PixelFormat format)
        {
            _Info = info;
            Format = format;
            Width = _Info.GetWidthFrom(tensor);
            Height = _Info.GetHeightFrom(tensor);
            Tensor = tensor;
        }

        public ReadOnlyTensorSpanBitmap(ReadOnlyTensorSpan<TElement> tensor, PixelFormat format)
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
        public PixelFormat Format { get; }
        public int Width { get; }
        public int Height { get; }

        public ReadOnlyTensorSpan<TElement> Tensor { get; }

        private readonly ReadOnlyTensorDimensionSpan<TElement> _Rows;


        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
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

        public void CopyPixelsTo<TDstElement, TDstPixel>(TensorBitmap<TDstElement, TDstPixel> dstBitmap, bool initPixels = true)
            where TDstElement : unmanaged, INumber<TDstElement>
            where TDstPixel : unmanaged
        {
            var pixelConverter = IPixelConverter<TPixel,TDstPixel>.Create(this.Format, dstBitmap.Format, initPixels);

            CopyPixelsTo(dstBitmap.AsTensorSpanBitmap(), pixelConverter);
        }

        public void CopyPixelsTo<TDstElement, TDstPixel>(TensorSpanBitmap<TDstElement, TDstPixel> dstBitmap, bool initPixels = true)
            where TDstElement : unmanaged, INumber<TDstElement>
            where TDstPixel: unmanaged
        {
            var pixelConverter = IPixelConverter<TPixel, TDstPixel>.Create(this.Format, dstBitmap.Format, initPixels);

            CopyPixelsTo(dstBitmap, pixelConverter);
        }

        public void CopyPixelsTo<TDstElement, TDstPixel>(TensorSpanBitmap<TDstElement, TDstPixel> dstBitmap, IPixelConverter<TPixel,TDstPixel> pixelConverter)
            where TDstElement : unmanaged, INumber<TDstElement>
            where TDstPixel : unmanaged
        {
            CopyPixelsTo(PixelsTransform.Copy, dstBitmap, pixelConverter);
        }

        public TResult CopyPixelsTo<TDstElement, TDstPixel, TResult>(PixelsTransform<TResult> transform, TensorSpanBitmap<TDstElement, TDstPixel> dstBitmap, bool initPixels = true)
            where TDstElement : unmanaged, INumber<TDstElement>
            where TDstPixel : unmanaged
        {
            var pixelConverter = IPixelConverter<TPixel, TDstPixel>.Create(this.Format, dstBitmap.Format, initPixels);
            var transformer = transform.GetInstance<TPixel, TDstPixel>();
            return transformer.Execute(this, dstBitmap, pixelConverter);
        }

        public TResult CopyPixelsTo<TDstBitmap, TDstPixel, TResult>(PixelsTransform<TResult> transform, TDstBitmap dstBitmap, IPixelConverter<TPixel, TDstPixel> pixelConverter)
            where TDstBitmap : IBitmapOperand<TDstBitmap, TDstPixel>, allows ref struct
            where TDstPixel : unmanaged
        {
            var transformer = transform.GetInstance<TPixel, TDstPixel>();
            return transformer.Execute(this, dstBitmap, pixelConverter);
        }        
    }
}

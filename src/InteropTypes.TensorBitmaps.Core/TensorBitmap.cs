using System;
using System.Buffers;
using System.Drawing;
using System.Numerics;
using System.Numerics.Tensors;
using System.Runtime.CompilerServices;
using System.Threading;

using InteropTypes.Numerics;

namespace InteropTypes.TensorBitmaps
{
    /// <summary>
    /// A bitmap backed by a <see cref="Tensor{TElement}"/>
    /// </summary>
    /// <typeparam name="TElement">The type of the backing tensor</typeparam>
    /// <typeparam name="TPixel">The type of the bitmap's pixel</typeparam>
    [System.Diagnostics.DebuggerDisplay("TensorBitmap {Width}x{Height}")]
    public class TensorBitmap<TElement,TPixel>
        : ITensorBitmap
        , InteropTypes.Numerics.BitmapOperators.IBitmapOperand<TensorBitmap<TElement, TPixel>, TPixel>        
        where TElement: unmanaged, INumber<TElement>
        where TPixel: unmanaged
    {
        public static TensorBitmap<TElement, TPixel> Create(int width, int height, PixelFormat format)
        {
            var channels = _TensorBitmapInfo.GetChannelsFrom<TElement, TPixel>();
            var buffer = new TElement[height * width * channels];
            var tensor = System.Numerics.Tensors.Tensor.Create(buffer, [height, width, channels]);

            return new TensorBitmap<TElement, TPixel>(tensor, format);
        }

        public TensorBitmap(Tensor<TElement> tensor, PixelFormat format)
        {
            if (tensor == null) throw new ArgumentNullException(nameof(tensor));

            _Info = new _TensorBitmapInfo(tensor.Lengths);            

            _Info._Validate<TElement, TPixel>(tensor, format);
            
            Format = format;
            Width = _Info.GetWidthFrom(tensor);
            Height = _Info.GetHeightFrom(tensor);
            Tensor = tensor;
        }

        internal readonly _TensorBitmapInfo _Info;
        public PixelFormat Format { get; }

        public int BytesPerPixel => Unsafe.SizeOf<TPixel>();

        public int Width { get; }
        public int Height { get; }

        ITensor ITensorBitmap.Tensor => Tensor;
        IReadOnlyTensor IReadOnlyTensorBitmap.Tensor => Tensor;
        public Tensor<TElement> Tensor { get; }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        ReadOnlySpan<byte> IReadOnlyBitmap.GetRowSpan(int y)
        {
            return GetRowSpan(y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public Span<byte> GetRowSpan(int y)
        {
            var pixels = GetRowPixelsSpan(y);
            return System.Runtime.InteropServices.MemoryMarshal.Cast<TPixel, byte>(pixels);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public Span<TPixel> GetRowPixelsSpan(int y)
        {
            var row = _Info.GetRow(Tensor,y);

            var pixels = System.Runtime.InteropServices.MemoryMarshal.Cast<TElement, TPixel>(row);

            System.Diagnostics.Debug.Assert(pixels.Length == Width);

            return pixels;            
        }

        ITensorBitmap ITensorBitmap.GetCropped(Rectangle rectangle)
        {
            return GetCropped(rectangle);
        }

        IReadOnlyTensorBitmap IReadOnlyTensorBitmap.GetCropped(Rectangle rectangle)
        {
            return GetCropped(rectangle);
        }


        /// <summary>
        /// Gets a new cropped bitmap that references the original surface without allocating new memory.
        /// </summary>
        public TensorBitmap<TElement, TPixel> GetCropped(System.Drawing.Rectangle rectangle)
        {
            rectangle.Intersect(new System.Drawing.Rectangle(0, 0, Width, Height));
            if (rectangle.IsEmpty) throw new ArgumentException("nothing to crop");

            var ranges = _Info.CalculateSlice(Tensor.Rank, rectangle);

            return new TensorBitmap<TElement, TPixel>(Tensor.Slice(ranges), Format);
        }

        public void CopyPixelsTo<TOtherElement, TOtherPixel>(TensorSpanBitmap<TOtherElement, TOtherPixel> dstBitmap, bool initPixels = true)
            where TOtherElement : unmanaged, INumber<TOtherElement>
            where TOtherPixel : unmanaged
        {
            this.AsReadOnlyTensorSpanBitmap().CopyPixelsTo(dstBitmap, initPixels);
        }

        public TensorBitmap<TElement,TPixelOut> Cast<TPixelOut>()
            where TPixelOut:unmanaged
        {
            if (Unsafe.SizeOf<TPixel>() != Unsafe.SizeOf<TPixelOut>()) throw new InvalidOperationException("Pixel size mismatch");
            return new TensorBitmap<TElement, TPixelOut>(this.Tensor, this.Format);
        }

        public TensorSpanBitmap<TElement, TPixel> AsTensorSpanBitmap()
        {
            return new TensorSpanBitmap<TElement, TPixel>(this.Tensor, this.Format);
        }

        public ReadOnlyTensorSpanBitmap<TElement,TPixel> AsReadOnlyTensorSpanBitmap()
        {
            return new ReadOnlyTensorSpanBitmap<TElement, TPixel>(this.Tensor, this.Format);
        }
        
    }
}

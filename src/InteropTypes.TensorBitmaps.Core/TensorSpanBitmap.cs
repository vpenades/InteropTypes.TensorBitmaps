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
    /// A bitmap backed by a <see cref="TensorSpan{TElement}"/>
    /// </summary>
    /// <typeparam name="TElement">The type of the backing tensor</typeparam>
    /// <typeparam name="TPixel">The type of the bitmap's pixel</typeparam>
    [System.Diagnostics.DebuggerDisplay("TensorBitmap {Width}x{Height}")]
    public readonly ref struct TensorSpanBitmap<TElement, TPixel>        
        : IBitmapOperand<TensorSpanBitmap<TElement, TPixel>, TPixel>        
        where TElement : unmanaged, INumber<TElement>
        where TPixel : unmanaged
    {
        #region lifecycle

        public static implicit operator TensorSpanBitmap<TElement, TPixel>(TensorBitmap<TElement, TPixel> bitmap)
        {
            var tensor = bitmap.Tensor.AsTensorSpan();
            return new TensorSpanBitmap<TElement, TPixel>(tensor, bitmap._Info, bitmap.Format);
        }

        private TensorSpanBitmap(TensorSpan<TElement> tensor, _TensorBitmapInfo info, PixelFormat format)
        {
            _Info = info;
            Format = format;
            Width = _Info.GetWidthFrom(tensor);
            Height = _Info.GetHeightFrom(tensor);
            Tensor = tensor;
        }

        public TensorSpanBitmap(TensorSpan<TElement> tensor, PixelFormat format)
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

        #endregion

        #region data

        internal readonly _TensorBitmapInfo _Info;
        public PixelFormat Format { get; }
        public int Width { get; }
        public int Height { get; }
        public TensorSpan<TElement> Tensor { get; }

        private readonly TensorDimensionSpan<TElement> _Rows;

        #endregion

        #region API - Rows

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public Span<TPixel> GetRowPixelsSpan(int y)
        {
            var trow = _Rows[y];

            var row = trow.GetSpan(_Info._RowIndices, (int)trow.FlattenedLength);

            var pixels = System.Runtime.InteropServices.MemoryMarshal.Cast<TElement, TPixel>(row);

            System.Diagnostics.Debug.Assert(pixels.Length == Width);

            return pixels;
        }

        ReadOnlySpan<TPixel> IReadOnlyBitmap<TPixel>.GetRowPixelsSpan(int y) { return GetRowPixelsSpan(y); }

        Span<TPixel> IBitmap<TPixel>.GetRowPixelsSpan(int y) { return GetRowPixelsSpan(y); }

        ReadOnlySpan<byte> IReadOnlyBitmap.GetRowBytesSpan(int y)
        {
            var pixels = GetRowPixelsSpan(y);
            return System.Runtime.InteropServices.MemoryMarshal.Cast<TPixel, Byte>(pixels);
        }
        Span<byte> IBitmap.GetRowBytesSpan(int y)
        {
            var pixels = GetRowPixelsSpan(y);
            return System.Runtime.InteropServices.MemoryMarshal.Cast<TPixel, Byte>(pixels);
        }

        #endregion

        #region API

        /// <summary>
        /// Gets a new cropped bitmap that references the original surface without allocating new memory.
        /// </summary>
        public TensorSpanBitmap<TElement, TPixel> GetCropped(System.Drawing.Rectangle rectangle)
        {
            rectangle.Intersect(new System.Drawing.Rectangle(0, 0, Width, Height));
            if (rectangle.IsEmpty) throw new ArgumentException("nothing to crop");

            var ranges = _Info.CalculateSlice(Tensor.Rank, rectangle);

            return new TensorSpanBitmap<TElement, TPixel>(Tensor.Slice(ranges), Format);
        }

        public TensorSpanBitmap<TElement, TPixelOut> Cast<TPixelOut>()
            where TPixelOut : unmanaged
        {
            if (Unsafe.SizeOf<TPixel>() != Unsafe.SizeOf<TPixelOut>()) throw new InvalidOperationException("Pixel size mismatch");
            return new TensorSpanBitmap<TElement, TPixelOut>(this.Tensor, this.Format);
        }

        public ReadOnlyTensorSpanBitmap<TElement, TPixel> AsReadOnlyTensorSpanBitmap()
        {
            return new ReadOnlyTensorSpanBitmap<TElement, TPixel>(this.Tensor, this.Format);
        }

        

        public void FillPixels(TPixel value)
        {
            var h = this.Height;
            for (int y = 0; y < h; ++y)
            {
                var srcRow = this.GetRowPixelsSpan(y);
                srcRow.Fill(value);
            }
        }

        /// <summary>
        /// Ensures that each pixel component falls inside the range defined by the format.
        /// </summary>
        public void ClampPixelComponents()
        {
            var components = Format.Components.Cast<PixelComponent<TElement>>().ToArray();

            // todo: if all components have the same min and max value, apply a raw clamp

            var h = this.Height;
            for (int y = 0; y < h; ++y)
            {
                var srcRow = this.GetRowPixelsSpan(y);
                var cmpRow = System.Runtime.InteropServices.MemoryMarshal.Cast<TPixel,TElement>(srcRow);

                for (int i = 0; i < components.Length; ++i)
                {
                    var c = components[i];
                    cmpRow[0] = TElement.Clamp(cmpRow[0], c.MinValue, c.MaxValue);
                    cmpRow = cmpRow.Slice(1);
                }                
            }
        }        

        public BITMAPOPERATORS.BinaryOperatorContext<TensorSpanBitmap<TElement, TPixel>, TPixel, TSrcPixel> GetContext<TSrcPixel>() where TSrcPixel : unmanaged
        {
            return new Operators.BinaryOperatorContext<TensorSpanBitmap<TElement, TPixel>, TPixel, TSrcPixel>(this);
        }

        #endregion
    }
}

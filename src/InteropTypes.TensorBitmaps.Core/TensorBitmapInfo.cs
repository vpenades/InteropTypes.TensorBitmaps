using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Numerics.Tensors;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using InteropTypes.Numerics;

namespace InteropTypes.TensorBitmaps
{
    /// <summary>
    /// Represents the internal Bitmap's "Shape"
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("TensorBitmap {Width}x{Height}")]
    internal readonly struct _TensorBitmapInfo
    {
        public _TensorBitmapInfo(ReadOnlySpan<nint> lengths)
        {
            _WidthIndex = -1;
            _HeightIndex = -1;
            _ChannelsIndex = -1;

            for (int i = 0; i < lengths.Length; i++)
            {
                var l = lengths[i];
                if (l == 0) throw new ArgumentException($"lengths[{i}] is zero", nameof(lengths));
                if (l == 1) continue;
                if (_HeightIndex < 0) { _HeightIndex = i; continue; }
                if (_WidthIndex < 0) { _WidthIndex = i; continue; }
                if (_ChannelsIndex < 0) { _ChannelsIndex = i; continue; }
                throw new ArgumentException($"only 2 or 3 dimensions allowed", nameof(lengths));
            }

            if (_HeightIndex < 0) throw new ArgumentException("Height not found", nameof(lengths));
            if (_WidthIndex < 0) throw new ArgumentException("Width not found", nameof(lengths));
            // channels is optional

            _RowIndices = new nint[lengths.Length - _WidthIndex];            
        }

        private readonly int _WidthIndex;
        private readonly int _HeightIndex;
        private readonly int _ChannelsIndex;

        internal readonly nint[] _RowIndices; // we could avoid this with a stackalloc        

        private void _ValidateFormat<TElement, TPixel>(nint tensorChannelCount, PixelFormat format)
            where TElement : unmanaged, INumber<TElement>
            where TPixel : unmanaged
        {
            var cmp = format.Components.FirstOrDefault(item => item is not PixelComponent<TElement>);

            if (cmp != null)
            {
                throw new ArgumentException($"Component type mismatch; Tensor is {typeof(TElement).Name} and {nameof(format)}.{cmp.Semantic} is {cmp.ComponentType.Name}", nameof(format));
            }

            if (format.Components.Count != tensorChannelCount)
            {
                throw new ArgumentException($"Channels count mismatch; Tensor is {tensorChannelCount} and {nameof(format)} is {format.Components.Count}", nameof(tensorChannelCount));
            }

            if (Unsafe.SizeOf<TPixel>() != format.BytesPerPixel)
            {
                throw new ArgumentException($"Pixel size mismatch; {typeof(TPixel).Name} is {Unsafe.SizeOf<TPixel>()} and {nameof(format)} is {format.BytesPerPixel}", nameof(format));
            }
        }

        public static int GetChannelsFrom<TElement, TPixel>()
            where TElement:unmanaged
            where TPixel:unmanaged
        {
            var (q, r) = Math.DivRem(Unsafe.SizeOf<TPixel>(), Unsafe.SizeOf<TElement>());
            if (q == 0 || r != 0) throw new InvalidOperationException($"{typeof(TElement).Name} and {typeof(TPixel).Name} mismatch");
            return q;            
        }

        #region Tensor

        public void _Validate<TElement, TPixel>(Tensor<TElement> tensor, PixelFormat format)
            where TElement : unmanaged , INumber<TElement>
            where TPixel : unmanaged
        {
            if (!tensor.GetDimensionSpan(_HeightIndex)[0].IsDense)
            {
                throw new ArgumentException("Rows must be dense", nameof(tensor));
            }

            _ValidateFormat<TElement, TPixel>(GetChannelsCountFrom(tensor), format);
        }

        public int GetWidthFrom<T>(Tensor<T> tensor) { return (int) tensor.Lengths[_WidthIndex]; }
        public int GetHeightFrom<T>(Tensor<T> tensor) { return (int)tensor.Lengths[_HeightIndex]; }
        public int GetChannelsCountFrom<T>(Tensor<T> tensor) { return _ChannelsIndex < 0 ? 1 : (int)tensor.Lengths[_ChannelsIndex]; }

        [System.Runtime.CompilerServices.MethodImpl(MethodImplOptions.AggressiveInlining|MethodImplOptions.AggressiveOptimization)]
        public Span<T> GetRow<T>(Tensor<T> tensor, int y)
        {
            var row = tensor.GetDimensionSpan(_HeightIndex)[y];
            return row.GetSpan(_RowIndices, (int)row.FlattenedLength);
        }

        #endregion

        #region TensorSpan

        public void _Validate<TElement, TPixel>(in TensorSpan<TElement> tensor, PixelFormat format)
           where TElement : unmanaged, INumber<TElement>
           where TPixel : unmanaged
        {
            if (!tensor.GetDimensionSpan(_HeightIndex)[0].IsDense)
            {
                throw new ArgumentException("Rows must be dense", nameof(tensor));
            }

            _ValidateFormat<TElement, TPixel>(GetChannelsCountFrom(tensor), format);
        }

        public int GetWidthFrom<T>(in TensorSpan<T> tensor) { return (int)tensor.Lengths[_WidthIndex]; }
        public int GetHeightFrom<T>(in TensorSpan<T> tensor) { return (int)tensor.Lengths[_HeightIndex]; }
        public int GetChannelsCountFrom<T>(in TensorSpan<T> tensor) { return _ChannelsIndex < 0 ? 1 : (int)tensor.Lengths[_ChannelsIndex]; }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public Span<T> GetRow<T>(in TensorSpan<T> tensor, int y)
        {
            var row = GetRows(tensor)[y];
            return row.GetSpan(_RowIndices, (int)row.FlattenedLength);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public TensorDimensionSpan<T> GetRows<T>(TensorSpan<T> tensor)
        {
            return tensor.GetDimensionSpan(_HeightIndex);
        }

        #endregion

        #region ReadOnlyTensorSpan

        public void _Validate<TElement, TPixel>(in ReadOnlyTensorSpan<TElement> tensor, PixelFormat format)
           where TElement : unmanaged, INumber<TElement>
           where TPixel : unmanaged
        {
            if (!tensor.GetDimensionSpan(_HeightIndex)[0].IsDense)
            {
                throw new ArgumentException("Rows must be dense", nameof(tensor));
            }

            _ValidateFormat<TElement, TPixel>(GetChannelsCountFrom(tensor), format);
        }
        public int GetWidthFrom<T>(in ReadOnlyTensorSpan<T> tensor) { return (int)tensor.Lengths[_WidthIndex]; }
        public int GetHeightFrom<T>(in ReadOnlyTensorSpan<T> tensor) { return (int)tensor.Lengths[_HeightIndex]; }
        public int GetChannelsCountFrom<T>(in ReadOnlyTensorSpan<T> tensor) { return _ChannelsIndex < 0 ? 1 : (int)tensor.Lengths[_ChannelsIndex]; }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public ReadOnlySpan<T> GetRow<T>(in ReadOnlyTensorSpan<T> tensor, int y)
        {
            var row = GetRows(tensor)[y];
            return row.GetSpan(_RowIndices, (int)row.FlattenedLength);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public ReadOnlyTensorDimensionSpan<T> GetRows<T>(ReadOnlyTensorSpan<T> tensor)
        {
            return tensor.GetDimensionSpan(_HeightIndex);
        }

        #endregion

        #region slicing

        public NRange[] CalculateSlice(int rank, System.Drawing.Rectangle rect)
        {
            NRange[] ranges = new NRange[rank];
            for (int i = 0; i < ranges.Length; i++)
            {
                ranges[i] = NRange.All;
            }

            // Set the crop ranges for height and width dimensions
            ranges[_HeightIndex] = new NRange(
                new NIndex(rect.Y),
                new NIndex(rect.Y + rect.Height));

            ranges[_WidthIndex] = new NRange(
                new NIndex(rect.X),
                new NIndex(rect.X + rect.Width));

            return ranges;
        }

        #endregion
    }
}

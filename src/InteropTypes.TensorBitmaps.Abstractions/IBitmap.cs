using System;
using System.Collections.Generic;
using System.Text;

using InteropTypes.Numerics;

namespace InteropTypes.TensorBitmaps
{
    /// <summary>
    /// Minimal readonly bitmap interface
    /// </summary>
    public interface IReadOnlyBitmap
    {
        /// <summary>
        /// The pixel layout
        /// </summary>
        /// <remarks>
        /// <typeparamref name="TPixel"/> ByteSize must match the format's bytesize
        /// </remarks>
        PixelFormat Format { get; }

        /// <summary>
        /// The width of the bitmap, in pixels
        /// </summary>
        int Width { get; }

        /// <summary>
        /// The height of the bitmap, in pixels
        /// </summary>
        int Height { get; }        

        ReadOnlySpan<byte> GetRowBytesSpan(int y);
    }

    /// <summary>
    /// Minimal readonly bitmap interface
    /// </summary>
    public interface IBitmap : IReadOnlyBitmap
    {
        new Span<byte> GetRowBytesSpan(int y);

        ReadOnlySpan<byte> IReadOnlyBitmap.GetRowBytesSpan(int y)
        {
            return GetRowBytesSpan(y);
        }
    }

    /// <summary>
    /// Minimal readonly bitmap interface
    /// </summary>
    /// <typeparam name="TPixel">The pixel type. It can be anything as long as it has the same ByteSize declared by <see cref="IReadOnlyBitmap.Format"/> </typeparam>
    public interface IReadOnlyBitmap<TPixel>
        : IReadOnlyBitmap
        where TPixel : unmanaged
    {
        /// <summary>
        /// Gets the pixels of a row.
        /// </summary>
        /// <param name="y">The row index</param>
        /// <returns>A span with pixels</returns>
        ReadOnlySpan<TPixel> GetRowPixelsSpan(int y)
        {
            var bytes = GetRowBytesSpan(y);
            return System.Runtime.InteropServices.MemoryMarshal.Cast<byte, TPixel>(bytes);
        }
    }

    /// <summary>
    /// Minimal bitmap interface
    /// </summary>
    /// <typeparam name="TPixel">The pixel type. It can be anything as long as it has the same ByteSize declared by <see cref="IReadOnlyBitmap.Format"/> </typeparam>
    public interface IBitmap<TPixel>
        : IReadOnlyBitmap<TPixel>
        , IBitmap
        where TPixel : unmanaged
    {
        /// <summary>
        /// Gets the pixels of a row.
        /// </summary>
        /// <param name="y">The row index</param>
        /// <returns>A span with pixels</returns>
        new Span<TPixel> GetRowPixelsSpan(int y)
        {
            var bytes = GetRowBytesSpan(y);
            return System.Runtime.InteropServices.MemoryMarshal.Cast<byte, TPixel>(bytes);
        }

        ReadOnlySpan<TPixel> IReadOnlyBitmap<TPixel>.GetRowPixelsSpan(int y)
        {
            return GetRowPixelsSpan(y);
        }
    }

    public interface IReadOnlyDisposableBitmap<TPixel> : IReadOnlyBitmap<TPixel>, IDisposable
        where TPixel : unmanaged
    { }

    public interface IDisposableBitmap<TPixel>
        : IReadOnlyDisposableBitmap<TPixel>
        , IBitmap<TPixel>
        where TPixel: unmanaged { }
}

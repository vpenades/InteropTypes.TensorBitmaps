using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

using InteropTypes.Numerics;

namespace InteropTypes.TensorBitmaps
{
    public interface IBitmapDimensions
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
    }

    /// <summary>
    /// Minimal readonly bitmap interface
    /// </summary>
    public interface IReadOnlyBitmap : IBitmapDimensions
    {
        ReadOnlySpan<byte> GetRowBytesSpan(int y);        
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
        ReadOnlySpan<TPixel> GetRowPixelsSpan(int y);

        ReadOnlySpan<byte> IReadOnlyBitmap.GetRowBytesSpan(int y)
        {
            var pixels = GetRowPixelsSpan(y);
            return System.Runtime.InteropServices.MemoryMarshal.AsBytes(pixels);
        }
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
        new Span<TPixel> GetRowPixelsSpan(int y);

        ReadOnlySpan<TPixel> IReadOnlyBitmap<TPixel>.GetRowPixelsSpan(int y)
        {
            return GetRowPixelsSpan(y);
        }

        Span<byte> IBitmap.GetRowBytesSpan(int y)
        {
            var pixels = GetRowPixelsSpan(y);
            return System.Runtime.InteropServices.MemoryMarshal.AsBytes(pixels);
        }

        ReadOnlySpan<byte> IReadOnlyBitmap.GetRowBytesSpan(int y)
        {
            var pixels = GetRowPixelsSpan(y);
            return System.Runtime.InteropServices.MemoryMarshal.AsBytes(pixels);
        }        
    }


    public interface IBitmapReader<TSelf, TPixel> : IBitmapDimensions
        where TSelf : IBitmapReader<TSelf, TPixel>
        #if NET9_0_OR_GREATER
        , allows ref struct
        #endif
        where TPixel : unmanaged
    {
        void ReadRowPixelsSpan(int y, scoped Span<TPixel> dst);

        /// <summary>
        /// Gets a cropped bitmap.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The returned bitmap must reference the pixels of the source bitmap.
        /// So any modification to the cropped bitmap is reflected into the source bitmap.
        /// </para>
        /// <para>
        /// The method should do: <c>rectangle.Intersect(new System.Drawing.Rectangle(0, 0, Width, Height));</c><br/>
        /// So the returned bitmap may be smalled than the requested region if they partially intersect.
        /// </para>
        /// </remarks>
        /// <param name="rect">The region to crop</param>
        /// <returns>A cropped bitmap</returns>
        TSelf GetCropped(System.Drawing.Rectangle rectangle);
    }

    public interface IBitmapWriter<TSelf, TPixel>
        : IBitmapDimensions
        , IBitmapReader<TSelf, TPixel>
        where TSelf : IBitmapWriter<TSelf, TPixel>
        #if NET9_0_OR_GREATER
        , allows ref struct
        #endif
        where TPixel : unmanaged
    {
        void WriteRowPixelsSpan(int y, scoped ReadOnlySpan<TPixel> dst);

        #if NET10_0_OR_GREATER
        /// <summary>
        /// Returns a context that can be used to perform bulk operations on this bitmap
        /// </summary>
        /// <typeparam name="TContextPixel">The pixel format to be used in the operations of the context.</typeparam>
        /// <returns>It must return: <c>new Operators.BinaryOperatorContext<TSelf, TPixel, TSrcPixel>(this);</c> </returns>
        public Operators.BinaryFillContext<TSelf, TPixel, TContextPixel> GetFiller<TContextPixel>()
            where TContextPixel : unmanaged;
        #endif
    }


    /// <summary>
    /// Represents an ByRef read only bitmap
    /// </summary>
    /// <typeparam name="TSelf">The type of the class or structure implementing this interface</typeparam>
    /// <typeparam name="TPixel">The pixel type. It can be anything as long as it has the same ByteSize declared by <see cref="Format"/> </typeparam>
    public interface IReadOnlyBitmap<TSelf, TPixel>
        : IReadOnlyBitmap<TPixel>
        , IBitmapReader<TSelf, TPixel>
        where TSelf : IReadOnlyBitmap<TSelf, TPixel>
        #if NET9_0_OR_GREATER
        , allows ref struct
        #endif
        where TPixel : unmanaged
    {
        public bool TryCastTo<TBitmap>(out TBitmap managedBitmap)
            where TBitmap : IReadOnlyBitmap<TPixel>
        {
            managedBitmap = default;
            return false;
        }

        void IBitmapReader<TSelf, TPixel>.ReadRowPixelsSpan(int y, scoped Span<TPixel> dst)
        {
            GetRowPixelsSpan(y).CopyTo(dst);
        }        
    }

    /// <summary>
    /// Represents ByRef bitmap
    /// </summary>
    /// <typeparam name="TSelf">The type of the class or structure implementing this interface</typeparam>
    /// <typeparam name="TPixel">The pixel type. It can be anything as long as it has the same ByteSize declared by <see cref="Format"/> </typeparam>
    public interface IBitmap<TSelf, TPixel>
        : IReadOnlyBitmap<TSelf, TPixel>
        , IBitmapReader<TSelf, TPixel>
        , IBitmapWriter<TSelf, TPixel>
        , IBitmap<TPixel>
        where TSelf : IBitmap<TSelf, TPixel>
        #if NET9_0_OR_GREATER
        , allows ref struct
        #endif
        where TPixel : unmanaged
    {

        void IBitmapWriter<TSelf, TPixel>.WriteRowPixelsSpan(int y, scoped ReadOnlySpan<TPixel> dst)
        {
            dst.CopyTo(GetRowPixelsSpan(y));
        }

        #if NET10_0_OR_GREATER
        /// <summary>
        /// Returns a context that can be used to perform bulk operations on this bitmap
        /// </summary>
        /// <typeparam name="TContextPixel">The pixel format to be used in the operations of the context.</typeparam>
        /// <returns>It must return: <c>new Operators.BinaryOperatorContext<TSelf, TPixel, TSrcPixel>(this);</c> </returns>
        public Operators.BinaryOperatorContext<TSelf, TPixel, TContextPixel> GetContext<TContextPixel>()
            where TContextPixel : unmanaged;
        #endif
    }
}

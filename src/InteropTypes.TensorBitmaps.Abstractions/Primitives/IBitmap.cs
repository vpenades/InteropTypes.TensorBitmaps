using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

using InteropTypes.Numerics;

namespace InteropTypes.TensorBitmaps.Primitives
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
        /// <summary>
        /// Gets the a bitmap full row.
        /// </summary>
        /// <param name="y">The row index</param>
        /// <returns>A <see cref="ReadOnlySpan{T}"/> representing the bytes of the row</returns>
        /// <exception cref="NotSupportedException">When this feature is not supported by the underlaying bitmap</exception>
        ReadOnlySpan<byte> GetRowBytesSpan(int y);

        void ReadRowBytesSpan(int y, int x, scoped Span<Byte> dst)
        {            
            GetRowBytesSpan(y).Slice(x * Format.BytesPerPixel).CopyTo(dst);
        }
    }

    /// <summary>
    /// Minimal readonly bitmap interface
    /// </summary>
    public interface IBitmap
        : IReadOnlyBitmap
    {
        /// <summary>
        /// Gets the a bitmap full row.
        /// </summary>
        /// <param name="y">The row index</param>
        /// <returns>A <see cref="Span{T}"/> representing the bytes of the row</returns>
        /// <exception cref="NotSupportedException">When this feature is not supported by the underlaying bitmap</exception>
        new Span<byte> GetRowBytesSpan(int y);

        ReadOnlySpan<byte> IReadOnlyBitmap.GetRowBytesSpan(int y)
        {
            return GetRowBytesSpan(y);
        }

        void WriteRowBytesSpan(int y, int x, scoped ReadOnlySpan<byte> src)
        {
            src.CopyTo(GetRowBytesSpan(y).Slice(x * Format.BytesPerPixel));
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
        /// Gets the a bitmap full row.
        /// </summary>
        /// <param name="y">The row index</param>
        /// <returns>A <see cref="ReadOnlySpan{T}"/> representing the pixels of the row</returns>
        /// <exception cref="NotSupportedException">When this feature is not supported by the underlaying bitmap</exception>
        ReadOnlySpan<TPixel> GetRowPixelsSpan(int y);

        void ReadRowPixelsSpan(int y, int x, scoped Span<TPixel> dst);

        ReadOnlySpan<byte> IReadOnlyBitmap.GetRowBytesSpan(int y)
        {            
            return System.Runtime.InteropServices.MemoryMarshal.AsBytes(GetRowPixelsSpan(y));
        }

        void IReadOnlyBitmap.ReadRowBytesSpan(int y, int x, scoped Span<Byte> dst)
        {
            ReadRowPixelsSpan(y, x, System.Runtime.InteropServices.MemoryMarshal.Cast<byte, TPixel>(dst));
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
        /// Gets the a bitmap full row.
        /// </summary>
        /// <param name="y">The row index</param>
        /// <returns>A <see cref="Span{T}"/> representing the pixels of the row</returns>
        /// <exception cref="NotSupportedException">When this feature is not supported by the underlaying bitmap</exception>
        new Span<TPixel> GetRowPixelsSpan(int y);

        void WriteRowPixelsSpan(int y, int x, scoped ReadOnlySpan<TPixel> src);

        ReadOnlySpan<TPixel> IReadOnlyBitmap<TPixel>.GetRowPixelsSpan(int y)
        {
            return GetRowPixelsSpan(y);
        }

        ReadOnlySpan<byte> IReadOnlyBitmap.GetRowBytesSpan(int y)
        {            
            return System.Runtime.InteropServices.MemoryMarshal.AsBytes(GetRowPixelsSpan(y));
        }

        Span<byte> IBitmap.GetRowBytesSpan(int y)
        {            
            return System.Runtime.InteropServices.MemoryMarshal.AsBytes(GetRowPixelsSpan(y));
        }

        void IBitmap.WriteRowBytesSpan(int y, int x, scoped ReadOnlySpan<byte> src)
        {
            WriteRowPixelsSpan(y, x, System.Runtime.InteropServices.MemoryMarshal.Cast<byte, TPixel>(src));
        }
    }    

    /// <summary>
    /// Represents an ByRef read only bitmap
    /// </summary>
    /// <typeparam name="TSelf">The type of the class or structure implementing this interface</typeparam>
    /// <typeparam name="TPixel">The pixel type. It can be anything as long as it has the same ByteSize declared by <see cref="Format"/> </typeparam>
    public interface IReadOnlyBitmap<TSelf, TPixel>
        : IReadOnlyBitmap<TPixel>
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

    /// <summary>
    /// Represents ByRef bitmap
    /// </summary>
    /// <typeparam name="TSelf">The type of the class or structure implementing this interface</typeparam>
    /// <typeparam name="TPixel">The pixel type. It can be anything as long as it has the same ByteSize declared by <see cref="Format"/> </typeparam>
    public interface IBitmap<TSelf, TPixel>
        : IReadOnlyBitmap<TSelf,TPixel>
        , IBitmap<TPixel>        
        where TSelf : IBitmap<TSelf, TPixel>
        #if NET9_0_OR_GREATER
        , allows ref struct
        #endif
        where TPixel : unmanaged
    {
        #if NET10_0_OR_GREATER

        /// <summary>
        /// Returns a context that can be used to perform bulk operations on this bitmap
        /// </summary>
        /// <typeparam name="TContextPixel">The pixel format to be used in the operations of the context.</typeparam>
        /// <returns>It must return: <c>new Operators.BinaryOperatorContext<TSelf, TPixel, TSrcPixel>(this);</c> </returns>
        public Operators.BitmapOperationContext<TSelf, TPixel, TContextPixel> GetContext<TContextPixel>()
            where TContextPixel : unmanaged;       
        

        #endif     
    }
}

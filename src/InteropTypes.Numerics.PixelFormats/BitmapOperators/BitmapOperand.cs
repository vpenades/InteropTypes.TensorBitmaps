using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InteropTypes.Numerics.BitmapOperators
{
    /// <summary>
    /// Represents an interface to a read only bitmap
    /// </summary>
    /// <typeparam name="TSelf">The type of the class or structure implementing this interface</typeparam>
    /// <typeparam name="TPixel">The pixel type. It can be anything as long as it has the same ByteSize declared by <see cref="Format"/> </typeparam>
    public interface IReadOnlyBitmapOperand<TSelf, TPixel>
        where TPixel : unmanaged
        where TSelf: IReadOnlyBitmapOperand<TSelf, TPixel>
        #if NET9_0_OR_GREATER
        , allows ref struct
        #endif        
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

        /// <summary>
        /// Gets the pixels of a row.
        /// </summary>
        /// <param name="y">The row index</param>
        /// <returns>A span with pixels</returns>
        ReadOnlySpan<TPixel> GetRowPixelsSpan(int y);

        /// <summary>
        /// Gets a cropped view of the bitmap.
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
    /// Represents an interface to a bitmap
    /// </summary>
    /// <typeparam name="TSelf">The type of the class or structure implementing this interface</typeparam>
    /// <typeparam name="TPixel">The pixel type. It can be anything as long as it has the same ByteSize declared by <see cref="Format"/> </typeparam>
    public interface IBitmapOperand<TSelf, TPixel>
        : IReadOnlyBitmapOperand<TSelf, TPixel>
        where TPixel : unmanaged
        where TSelf : IBitmapOperand<TSelf, TPixel>
        #if NET9_0_OR_GREATER
        , allows ref struct
        #endif
    {
        /// <summary>
        /// Gets the pixels of a row.
        /// </summary>
        /// <param name="y">The row index</param>
        /// <returns>A span with pixels</returns>
        new Span<TPixel> GetRowPixelsSpan(int y);

        ReadOnlySpan<TPixel> IReadOnlyBitmapOperand<TSelf, TPixel>.GetRowPixelsSpan(int y) => GetRowPixelsSpan(y);
    }   
    
    public interface IDisposableReadOnlyBitmapOperand<TSelf,TPixel>
        : IReadOnlyBitmapOperand<TSelf,TPixel>
        , IDisposable
        where TPixel : unmanaged
        where TSelf : IDisposableReadOnlyBitmapOperand<TSelf, TPixel>
        #if NET9_0_OR_GREATER
        , allows ref struct
        #endif
    {

    }

    public interface IStretchedBitmapSource<TSelf, TPixel> : IDisposable
        where TPixel : unmanaged
        where TSelf : IDisposableReadOnlyBitmapOperand<TSelf, TPixel>
    {
        TSelf CreateStretched(int width, int height);
    }
}

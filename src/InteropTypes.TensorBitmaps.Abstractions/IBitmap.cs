using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

using InteropTypes.Numerics;

using RECT = System.Drawing.Rectangle;

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

    public interface IClientReadOnlyBitmap<TPixel> : IReadOnlyBitmap<TPixel>, IDisposable
        where TPixel : unmanaged
    {
        public bool TryCreateCropped(RECT rect, bool shareMemory, out IClientReadOnlyBitmap<TPixel> croppedBitmap)
        {
            croppedBitmap = default;
            return false;
        }

        public bool TryCreateStretched(int width, int height, RECT? srcCrop, out IClientReadOnlyBitmap<TPixel> stretchedBitmap)
        {
            stretchedBitmap = default;
            return false;
        }

        public static bool TryCreateStretched(IReadOnlyBitmap<TPixel> src, int width, int height, out IClientReadOnlyBitmap<TPixel> stretchedBitmap)
        {
            switch(src)
            {
                case IClientReadOnlyBitmap<TPixel> client:
                    return client.TryCreateStretched(width, height, null, out stretchedBitmap);                

                case _ReadOnlyBitmapCropped<TPixel> rocropped:
                    return rocropped.TryCreateStretchedClient(width, height, out stretchedBitmap);

                case _BitmapCropped<TPixel> rocropped:
                    if (rocropped.TryCreateStretchedClient(width, height, out var stretched))
                    {
                        stretchedBitmap = stretched;
                        return true;
                    }
                    break;
            }

            stretchedBitmap = default;
            return false;
        }
    }

    public interface IClientBitmap<TPixel>
        : IClientReadOnlyBitmap<TPixel>
        , IBitmap<TPixel>
        where TPixel: unmanaged
    {
        bool IClientReadOnlyBitmap<TPixel>.TryCreateCropped(RECT rect, bool shareMemory, out IClientReadOnlyBitmap<TPixel> croppedBitmap)
        {
            if (TryCreateCropped(rect, shareMemory, out var cropped)) { croppedBitmap = cropped; return true; }
            croppedBitmap = default;
            return false;
        }

        bool IClientReadOnlyBitmap<TPixel>.TryCreateStretched(int width, int height, RECT? srcCrop, out IClientReadOnlyBitmap<TPixel> stretchedBitmap)
        {
            if (TryCreateStretched(width, height, srcCrop, out var stretched)) { stretchedBitmap = stretched; return true; }
            stretchedBitmap = default;
            return false;
        }


        public bool TryCreateCropped(RECT rect, bool shareMemory, out IClientBitmap<TPixel> croppedBitmap)
        {
            croppedBitmap = default;
            return false;
        }

        public bool TryCreateStretched(int width, int height, RECT? srcCrop, out IClientBitmap<TPixel> stretchedBitmap)
        {
            stretchedBitmap = default;
            return false;
        }
    }
}

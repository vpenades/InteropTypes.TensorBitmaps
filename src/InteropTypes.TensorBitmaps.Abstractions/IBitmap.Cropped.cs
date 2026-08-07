using System;
using System.Collections.Generic;
using System.Text;

using InteropTypes.Numerics;

using RECT = System.Drawing.Rectangle;
using SIZE = System.Drawing.Size;

namespace InteropTypes.TensorBitmaps
{
    [System.Diagnostics.DebuggerDisplay("_BitmapReaderCropped {Width}x{Height}x{Format}")]
    readonly struct _BitmapReaderCropped : IBitmapReader
    {
        #region lifecycle
        public _BitmapReaderCropped(IBitmapReader source, RECT cropRect)
        {
            if (source is _BitmapReaderCropped nested)
            {
                source = nested._Source;
                cropRect.X += nested._Rect.X;
                cropRect.Y += nested._Rect.Y;
            }

            cropRect.Intersect(new RECT(0, 0, source.Width, source.Height));
            _Source = source;
            _Rect = cropRect;
        }

        #endregion

        #region data

        private readonly IBitmapReader _Source;
        private readonly RECT _Rect;

        #endregion

        #region properties
        public PixelFormat Format => _Source.Format;
        public int Width => _Rect.Width;
        public int Height => _Rect.Height;

        #endregion

        #region API

        public void ReadRowBytesSpan(int y, scoped Span<byte> dst)
        {            
            var k = Format.BytesPerPixel;

            Span<byte> fullRow = stackalloc byte[_Source.Width * k];
            _Source.ReadRowBytesSpan(y, fullRow);

            fullRow.Slice(_Rect.X * k, _Rect.Width * k).CopyTo(dst);
        }

        #endregion
    }

    [System.Diagnostics.DebuggerDisplay("_BitmapWriterCropped {Width}x{Height}x{Format}")]
    readonly struct _BitmapWriterCropped : IBitmapWriter
    {
        #region lifecycle
        public _BitmapWriterCropped(IBitmapWriter source, RECT cropRect)
        {
            if (source is _BitmapWriterCropped nested)
            {
                source = nested._Source;
                cropRect.X += nested._Rect.X;
                cropRect.Y += nested._Rect.Y;
            }

            cropRect.Intersect(new RECT(0, 0, source.Width, source.Height));
            _Source = source;
            _Rect = cropRect;
        }

        #endregion

        #region data

        private readonly IBitmapWriter _Source;
        private readonly RECT _Rect;

        #endregion

        #region properties
        public PixelFormat Format => _Source.Format;
        public int Width => _Rect.Width;
        public int Height => _Rect.Height;

        #endregion

        #region API

        public void ReadRowBytesSpan(int y, scoped Span<byte> dst)
        {
            var k = Format.BytesPerPixel;

            Span<byte> fullRow = stackalloc byte[_Source.Width * k];
            _Source.ReadRowBytesSpan(y, fullRow);

            fullRow.Slice(_Rect.X * k, _Rect.Width * k).CopyTo(dst);
        }

        public void WriteRowBytesSpan(int y, scoped ReadOnlySpan<byte> src)
        {
            var k = Format.BytesPerPixel;

            Span<byte> fullRow = stackalloc byte[_Source.Width * k];

            _Source.ReadRowBytesSpan(y, fullRow);
            src.Slice(0, _Rect.Width * k).CopyTo(fullRow.Slice(_Rect.X * k));
            _Source.WriteRowBytesSpan(y, fullRow);
        }

        #endregion
    }

    [System.Diagnostics.DebuggerDisplay("_BitmapReaderCropped {Width}x{Height}x{Format}")]
    readonly struct _BitmapReaderCropped<TPixel> : IBitmapReader<TPixel>
        where TPixel: unmanaged
    {
        #region lifecycle
        public _BitmapReaderCropped(IBitmapReader<TPixel> source, RECT cropRect)
        {
            if (source is _BitmapReaderCropped<TPixel> nested)
            {
                source = nested._Source;
                cropRect.X += nested._Rect.X;
                cropRect.Y += nested._Rect.Y;
            }

            cropRect.Intersect(new RECT(0, 0, source.Width, source.Height));
            _Source = source;
            _Rect = cropRect;
        }

        #endregion

        #region data

        private readonly IBitmapReader<TPixel> _Source;
        private readonly RECT _Rect;

        #endregion

        #region properties
        public PixelFormat Format => _Source.Format;
        public int Width => _Rect.Width;
        public int Height => _Rect.Height;

        #endregion

        #region API

        public void ReadRowPixelsSpan(int y, scoped Span<TPixel> dst)
        {
            Span<TPixel> fullRow = stackalloc TPixel[_Source.Width];
            _Source.ReadRowPixelsSpan(y, fullRow);
            fullRow.Slice(_Rect.X, _Rect.Width).CopyTo(dst);
        }

        #endregion
    }

    [System.Diagnostics.DebuggerDisplay("_BitmapWriterCropped {Width}x{Height}x{Format}")]
    readonly struct _BitmapWriterCropped<TPixel> : IBitmapWriter<TPixel>
        where TPixel : unmanaged
    {
        #region lifecycle
        public _BitmapWriterCropped(IBitmapWriter<TPixel> source, RECT cropRect)
        {
            if (source is _BitmapWriterCropped<TPixel> nested)
            {
                source = nested._Source;
                cropRect.X += nested._Rect.X;
                cropRect.Y += nested._Rect.Y;
            }

            cropRect.Intersect(new RECT(0, 0, source.Width, source.Height));
            _Source = source;
            _Rect = cropRect;
        }

        #endregion

        #region data

        private readonly IBitmapWriter<TPixel> _Source;
        private readonly RECT _Rect;

        #endregion

        #region properties
        public PixelFormat Format => _Source.Format;
        public int Width => _Rect.Width;
        public int Height => _Rect.Height;

        #endregion

        #region API

        public void ReadRowPixelsSpan(int y, scoped Span<TPixel> dst)
        {
            Span<TPixel> fullRow = stackalloc TPixel[_Source.Width];
            _Source.ReadRowPixelsSpan(y, fullRow);
            fullRow.Slice(_Rect.X, _Rect.Width).CopyTo(dst);
        }

        public void WriteRowPixelsSpan(int y, scoped ReadOnlySpan<TPixel> src)
        {
            Span<TPixel> fullRow = stackalloc TPixel[_Source.Width];

            _Source.ReadRowPixelsSpan(y, fullRow);
            src.Slice(0, _Rect.Width).CopyTo(fullRow.Slice(_Rect.X));
            _Source.WriteRowPixelsSpan(y, fullRow);
        }

        #endregion
    }

    [System.Diagnostics.DebuggerDisplay("_ReadOnlyBitmapCropped {Width}x{Height}x{Format}")]
    readonly struct _ReadOnlyBitmapCropped : IReadOnlyBitmap
    {
        #region lifecycle

        public _ReadOnlyBitmapCropped(IReadOnlyBitmap source, RECT cropRect)
        {
            if (source is _ReadOnlyBitmapCropped nested)
            {
                source = nested._Source;
                cropRect.X += nested._Rect.X;
                cropRect.Y += nested._Rect.Y;
            }

            cropRect.Intersect(new RECT(0, 0, source.Width, source.Height));
            _Source = source;
            _Rect = cropRect;
        }

        #endregion

        #region data

        private readonly IReadOnlyBitmap _Source;
        private readonly RECT _Rect;

        #endregion

        #region properties
        public PixelFormat Format => _Source.Format;
        public int Width => _Rect.Width;
        public int Height => _Rect.Height;

        #endregion

        #region API

        public ReadOnlySpan<byte> GetRowBytesSpan(int y)
        {
            if (y < 0 || y >= _Rect.Height) throw new ArgumentOutOfRangeException(nameof(y));
            var k = Format.BytesPerPixel;
            return _Source
                .GetRowBytesSpan(y + _Rect.Y)
                .Slice(_Rect.X * k, _Rect.Width * k);
        }

        #endregion
    }

    [System.Diagnostics.DebuggerDisplay("_BitmapCropped {Width}x{Height}x{Format}")]
    readonly struct _BitmapCropped : IBitmap
    {
        #region lifecycle

        public _BitmapCropped(IBitmap source, RECT cropRect)
        {
            if (source is _BitmapCropped nested)
            {
                source = nested._Source;
                cropRect.X += nested._Rect.X;
                cropRect.Y += nested._Rect.Y;
            }

            cropRect.Intersect(new RECT(0, 0, source.Width, source.Height));
            _Source = source;
            _Rect = cropRect;
        }

        #endregion

        #region data

        private readonly IBitmap _Source;
        private readonly RECT _Rect;

        #endregion

        #region properties

        public PixelFormat Format => _Source.Format;

        public int Width => _Rect.Width;

        public int Height => _Rect.Height;

        #endregion

        #region API

        public Span<byte> GetRowBytesSpan(int y)
        {
            if (y < 0 || y >= _Rect.Height) throw new ArgumentOutOfRangeException(nameof(y));            
            var k = Format.BytesPerPixel;
            return _Source
                .GetRowBytesSpan(y + _Rect.Y)
                .Slice(_Rect.X * k, _Rect.Width * k);
        }

        #endregion
    }

    [System.Diagnostics.DebuggerDisplay("_ReadOnlyBitmapCropped {Width}x{Height}x{Format}")]
    readonly struct _ReadOnlyBitmapCropped<TPixel> : IReadOnlyBitmap<TPixel>
        where TPixel: unmanaged
    {
        #region lifecycle

        public _ReadOnlyBitmapCropped(IReadOnlyBitmap<TPixel> source, RECT cropRect)
        {
            if (source is _ReadOnlyBitmapCropped<TPixel> nested)
            {
                source = nested._Source;
                cropRect.X += nested._Rect.X;
                cropRect.Y += nested._Rect.Y;
            }

            cropRect.Intersect(new RECT(0, 0, source.Width, source.Height));
            _Source = source;
            _Rect = cropRect;
        }

        #endregion

        #region data

        private readonly IReadOnlyBitmap<TPixel> _Source;
        private readonly RECT _Rect;

        #endregion

        #region properties

        public PixelFormat Format => _Source.Format;
        public int Width => _Rect.Width;
        public int Height => _Rect.Height;

        #endregion

        #region API

        public ReadOnlySpan<TPixel> GetRowPixelsSpan(int y)
        {
            if (y < 0 || y >= _Rect.Height) throw new ArgumentOutOfRangeException(nameof(y));
            return _Source
                .GetRowPixelsSpan(y + _Rect.Y)
                .Slice(_Rect.X, _Rect.Width);
        }        

        public bool TryCreateCroppedClient(out IClientReadOnlyBitmap<TPixel> croppedClient)
        {
            if (_Source is not IClientReadOnlyBitmap<TPixel> client) { croppedClient = null; return false; }
            return client.TryGetCropped(_Rect, out croppedClient);
        }

        public bool TryCreateStretchedClient(SIZE dstSize, out IClientReadOnlyBitmap<TPixel> stretchedClient)
        {
            if (_Source is not IClientReadOnlyBitmap<TPixel> client) { stretchedClient = null; return false; }
            return client.TryCreateStretched(_Rect, dstSize, out stretchedClient);
        }

        #endregion
    }

    [System.Diagnostics.DebuggerDisplay("_BitmapCropped {Width}x{Height}x{Format}")]
    readonly struct _BitmapCropped<TPixel> : IBitmap<TPixel>
        where TPixel : unmanaged
    {
        #region lifecycle

        public _BitmapCropped(IBitmap<TPixel> source, RECT cropRect)
        {
            if (source is _BitmapCropped<TPixel> nested)
            {
                source = nested._Source;
                cropRect.X += nested._Rect.X;
                cropRect.Y += nested._Rect.Y;
            }

            cropRect.Intersect(new RECT(0, 0, source.Width, source.Height));
            _Source = source;
            _Rect = cropRect;
        }

        #endregion

        #region data

        internal readonly IBitmap<TPixel> _Source;
        private readonly RECT _Rect;

        #endregion

        #region properties

        public PixelFormat Format => _Source.Format;
        public int Width => _Rect.Width;
        public int Height => _Rect.Height;

        #endregion

        #region API

        public Span<TPixel> GetRowPixelsSpan(int y)
        {
            if (y < 0 || y >= _Rect.Height) throw new ArgumentOutOfRangeException(nameof(y));
            return _Source
                .GetRowPixelsSpan(y + _Rect.Y)
                .Slice(_Rect.X, _Rect.Width);
        }

        public bool TryCreateCroppedClient(out IClientBitmap<TPixel> croppedClient)
        {
            if (_Source is not IClientBitmap<TPixel> client) { croppedClient = null; return false; }
            return client.TryGetCropped(_Rect, out croppedClient);
        }

        public bool TryCreateStretchedClient(SIZE dstSize, out IClientBitmap<TPixel> stretchedClient)
        {
            if (_Source is not IClientBitmap<TPixel> client) { stretchedClient = null; return false; }
            return client.TryCreateStretched(_Rect, dstSize, out stretchedClient);
        }

        #endregion
    }
}

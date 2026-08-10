using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

using InteropTypes.Numerics;

using RECT = System.Drawing.Rectangle;
using SIZE = System.Drawing.Size;

namespace InteropTypes.TensorBitmaps.Primitives
{
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
            y += _Rect.Y;

            var k = Format.BytesPerPixel;
            return _Source
                .GetRowBytesSpan(y)
                .Slice(_Rect.X * k, _Rect.Width * k);
        }

        public void ReadRowBytesSpan(int y, int x, scoped Span<Byte> dst)
        {
            if (y < 0 || y >= _Rect.Height) throw new ArgumentOutOfRangeException(nameof(y));
            if (x < 0 || x >= _Rect.Width) throw new ArgumentOutOfRangeException(nameof(x));
            _Source.ReadRowBytesSpan(y + _Rect.Y, x + _Rect.X, dst);
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
            y += _Rect.Y;

            var k = Format.BytesPerPixel;
            return _Source
                .GetRowBytesSpan(y)
                .Slice(_Rect.X * k, _Rect.Width * k);
        }

        void ReadRowBytesSpan(int y, int x, scoped Span<Byte> dst)
        {
            if (y < 0 || y >= _Rect.Height) throw new ArgumentOutOfRangeException(nameof(y));
            if (x < 0 || x >= _Rect.Width) throw new ArgumentOutOfRangeException(nameof(x));
            _Source.ReadRowBytesSpan(y + _Rect.Y, x + _Rect.X, dst);
        }

        void WriteRowBytesSpan(int y, int x, scoped ReadOnlySpan<byte> src)
        {
            if (y < 0 || y >= _Rect.Height) throw new ArgumentOutOfRangeException(nameof(y));
            if (x < 0 || x >= _Rect.Width) throw new ArgumentOutOfRangeException(nameof(x));
            _Source.WriteRowBytesSpan(y + _Rect.Y, x + _Rect.X, src);
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
            y += _Rect.Y;

            return _Source
                .GetRowPixelsSpan(y)
                .Slice(_Rect.X, _Rect.Width);
        }

        public void ReadRowPixelsSpan(int y, int x, scoped Span<TPixel> dst)
        {
            if (y < 0 || y >= _Rect.Height) throw new ArgumentOutOfRangeException(nameof(y));
            if (x < 0 || x >= _Rect.Width) throw new ArgumentOutOfRangeException(nameof(x));
            _Source.ReadRowPixelsSpan(y + _Rect.Y, x + _Rect.X, dst);
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
            y += _Rect.Y;

            return _Source
                .GetRowPixelsSpan(y)
                .Slice(_Rect.X, _Rect.Width);
        }

        public void ReadRowPixelsSpan(int y, int x, scoped Span<TPixel> dst)
        {
            if (y < 0 || y >= _Rect.Height) throw new ArgumentOutOfRangeException(nameof(y));
            if (x < 0 || x >= _Rect.Width) throw new ArgumentOutOfRangeException(nameof(x));
            _Source.ReadRowPixelsSpan(y + _Rect.Y, x + _Rect.X, dst);
        }

        public void WriteRowPixelsSpan(int y, int x, scoped ReadOnlySpan<TPixel> src)
        {
            if (y < 0 || y >= _Rect.Height) throw new ArgumentOutOfRangeException(nameof(y));
            if (x < 0 || x >= _Rect.Width) throw new ArgumentOutOfRangeException(nameof(x));
            _Source.WriteRowPixelsSpan(y + _Rect.Y, x + _Rect.X, src);
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

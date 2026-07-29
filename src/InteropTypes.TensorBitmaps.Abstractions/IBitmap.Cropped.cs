using System;
using System.Collections.Generic;
using System.Text;

using InteropTypes.Numerics;

using RECT = System.Drawing.Rectangle;
using SIZE = System.Drawing.Size;

namespace InteropTypes.TensorBitmaps
{
    [System.Diagnostics.DebuggerDisplay("_ReadOnlyBitmapCropped {Width}x{Height}x{Format}")]
    readonly struct _ReadOnlyBitmapCropped : IReadOnlyBitmap
    {
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

        private readonly IReadOnlyBitmap _Source;
        private readonly RECT _Rect;
        public PixelFormat Format => _Source.Format;
        public int Width => _Source.Width;
        public int Height => _Source.Height;

        public ReadOnlySpan<byte> GetRowBytesSpan(int y)
        {
            if (y < 0 || y >= _Rect.Height) throw new ArgumentOutOfRangeException(nameof(y));
            var k = Format.BytesPerPixel;
            return _Source
                .GetRowBytesSpan(y + _Rect.Y)
                .Slice(_Rect.X * k, _Rect.Width * k);
        }
    }

    [System.Diagnostics.DebuggerDisplay("_BitmapCropped {Width}x{Height}x{Format}")]
    readonly struct _BitmapCropped : IBitmap
    {
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

        private readonly IBitmap _Source;
        private readonly RECT _Rect;

        public PixelFormat Format => _Source.Format;

        public int Width => _Source.Width;

        public int Height => _Source.Height;

        public Span<byte> GetRowBytesSpan(int y)
        {
            if (y < 0 || y >= _Rect.Height) throw new ArgumentOutOfRangeException(nameof(y));            
            var k = Format.BytesPerPixel;
            return _Source
                .GetRowBytesSpan(y + _Rect.Y)
                .Slice(_Rect.X * k, _Rect.Width * k);
        }
    }

    [System.Diagnostics.DebuggerDisplay("_ReadOnlyBitmapCropped {Width}x{Height}x{Format}")]
    readonly struct _ReadOnlyBitmapCropped<TPixel> : IReadOnlyBitmap<TPixel>
        where TPixel: unmanaged
    {
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

        private readonly IReadOnlyBitmap<TPixel> _Source;
        private readonly RECT _Rect;

        public PixelFormat Format => _Source.Format;
        public int Width => _Source.Width;
        public int Height => _Source.Height;

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
    }

    [System.Diagnostics.DebuggerDisplay("_BitmapCropped {Width}x{Height}x{Format}")]
    readonly struct _BitmapCropped<TPixel> : IBitmap<TPixel>
        where TPixel : unmanaged
    {
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

        internal readonly IBitmap<TPixel> _Source;
        private readonly RECT _Rect;

        public PixelFormat Format => _Source.Format;
        public int Width => _Source.Width;
        public int Height => _Source.Height;

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
    }
}

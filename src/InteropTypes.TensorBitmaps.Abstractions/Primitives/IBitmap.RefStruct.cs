using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

using InteropTypes.Numerics;
using InteropTypes.TensorBitmaps.Operators;

namespace InteropTypes.TensorBitmaps.Primitives
{
    /// <summary>
    /// This class is used to wrap a managed <see cref="IReadOnlyBitmap{TPixel}"/> as a ref struct
    /// </summary>    
    [System.Diagnostics.DebuggerDisplay("ManagedReadOnlyBitmapOperand {Width}x{Height} {Format}")]
    public readonly ref struct RefStructReadOnlyBitmap<TPixel>
        #if NET9_0_OR_GREATER
        : IReadOnlyBitmap<RefStructReadOnlyBitmap<TPixel>,TPixel>
        #endif
        where TPixel: unmanaged
    {
        #region lifecycle
        public RefStructReadOnlyBitmap(IReadOnlyBitmap<TPixel> managed)
        {
            _Managed = managed;
        }

        #endregion

        #region data

        private readonly IReadOnlyBitmap<TPixel> _Managed;
        public PixelFormat Format => _Managed.Format;
        public int Width => _Managed.Width;
        public int Height => _Managed.Height;

        #endregion

        #region API - Rows
        public ReadOnlySpan<TPixel> GetRowPixelsSpan(int y) => _Managed.GetRowPixelsSpan(y);        
        public ReadOnlySpan<byte> GetRowBytesSpan(int y) => _Managed.GetRowBytesSpan(y);

        public void ReadRowPixelsSpan(int y, int x, scoped Span<TPixel> dst) => _Managed.ReadRowPixelsSpan(y, x, dst);
        public void ReadRowBytesSpan(int y, int x, scoped Span<byte> dst) => _Managed.ReadRowBytesSpan(y, x, dst);        

        #endregion

        #region API

        public bool TryCastTo<T>(out T managedBitmap)
            where T : IReadOnlyBitmap<TPixel>
        {
            if (_Managed is T typed)
            {
                managedBitmap = typed;
                return true;
            }
            else
            {
                managedBitmap = default;
                return false;
            }
        }

        public RefStructReadOnlyBitmap<TPixel> GetCropped(Rectangle rectangle)
        {
            var cropped = new _ReadOnlyBitmapCropped<TPixel>(_Managed, rectangle);

            return new RefStructReadOnlyBitmap<TPixel>(cropped);
        }

        #endregion

    }

    /// <summary>
    /// This class is used to wrap a managed <see cref="IBitmap{TPixel}"/> as a ref struct
    /// </summary>    
    [System.Diagnostics.DebuggerDisplay("ManagedBitmapOperand {Width}x{Height} {Format}")]
    public readonly ref struct RefStructBitmap<TPixel>
        #if NET9_0_OR_GREATER
        : IBitmap<RefStructBitmap<TPixel>, TPixel>
        #endif
        where TPixel : unmanaged
    {
        #region lifecycle
        public RefStructBitmap(IBitmap<TPixel> managed)
        {
            _Managed = managed;
        }

        #endregion

        #region data

        private readonly IBitmap<TPixel> _Managed; 

        public PixelFormat Format => _Managed.Format;
        public int Width => _Managed.Width;
        public int Height => _Managed.Height;

        #endregion

        #region API - Rows        

        public Span<TPixel> GetRowPixelsSpan(int y) => _Managed.GetRowPixelsSpan(y);
        public Span<byte> GetRowBytesSpan(int y) => _Managed.GetRowBytesSpan(y);

        #if NET9_0_OR_GREATER
        ReadOnlySpan<TPixel> IReadOnlyBitmap<TPixel>.GetRowPixelsSpan(int y) => GetRowPixelsSpan(y);
        ReadOnlySpan<byte> IReadOnlyBitmap.GetRowBytesSpan(int y) => GetRowBytesSpan(y);
        #endif

        public void ReadRowPixelsSpan(int y, int x, scoped Span<TPixel> dst) => _Managed.ReadRowPixelsSpan(y, x, dst);
        public void WriteRowPixelsSpan(int y, int x, scoped ReadOnlySpan<TPixel> src) => _Managed.WriteRowPixelsSpan(y, x, src);

        public void ReadRowBytesSpan(int y, int x,scoped Span<byte> dst) => _Managed.ReadRowBytesSpan(y, x, dst);
        public void WriteRowBytesSpan(int y, int x, scoped ReadOnlySpan<byte> src) => _Managed.WriteRowBytesSpan(y, x, src);

        #endregion

        #region API

        public bool TryCastTo<T>(out T managedBitmap)
            where T : IReadOnlyBitmap<TPixel>
        {
            if (_Managed is T typed)
            {
                managedBitmap = typed;
                return true;
            }
            else
            {
                managedBitmap = default;
                return false;
            }
        }

        public RefStructBitmap<TPixel> GetCropped(Rectangle rectangle)
        {
            var cropped = new _BitmapCropped<TPixel>(_Managed, rectangle);

            return new RefStructBitmap<TPixel>(cropped);
        }

        #if NET9_0_OR_GREATER
        public BitmapOperationContext<RefStructBitmap<TPixel>, TPixel, TContextPixel> GetContext<TContextPixel>()
            where TContextPixel : unmanaged
        {
            return new BitmapOperationContext<RefStructBitmap<TPixel>, TPixel, TContextPixel>(this);
        }
        #endif

        #endregion
    }
}

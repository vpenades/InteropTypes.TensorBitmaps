using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

using InteropTypes.Numerics;

namespace InteropTypes.TensorBitmaps.Operands
{
    /// <summary>
    /// This class is used to wrap a managed <see cref="IBitmapReader{TPixel}"/> with a ByRef operand
    /// </summary>    
    [System.Diagnostics.DebuggerDisplay("ManagedBitmapReaderOperand {Width}x{Height} {Format}")]
    public readonly ref struct ManagedBitmapReaderOperand<TPixel>
        : IBitmapReader<ManagedBitmapReaderOperand<TPixel>, TPixel>
        where TPixel : unmanaged
    {
        #region lifecycle
        public ManagedBitmapReaderOperand(IBitmapReader<TPixel> managed)
        {
            _Managed = managed;
        }

        #endregion

        #region data

        private readonly IBitmapReader<TPixel> _Managed;
        public PixelFormat Format => _Managed.Format;
        public int Width => _Managed.Width;
        public int Height => _Managed.Height;

        #endregion

        #region API - Rows

        public void ReadRowPixelsSpan(int y, Span<TPixel> dst) => _Managed.ReadRowPixelsSpan(y,dst);
        public void ReadRowBytesSpan(int y, Span<byte> dst) => _Managed.ReadRowBytesSpan(y,dst);

        #endregion

        #region API

        public bool TryCastTo<T>(out T managedBitmap)
            where T : IBitmapReader<TPixel>
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

        public ManagedBitmapReaderOperand<TPixel> GetCropped(Rectangle rectangle)
        {
            var cropped = new _BitmapReaderCropped<TPixel>(_Managed, rectangle);
            return new ManagedBitmapReaderOperand<TPixel>(cropped);
        }

        #endregion

    }

    /// <summary>
    /// This class is used to wrap a managed <see cref="IBitmapWriter{TPixel}"/> with a ByRef operand
    /// </summary>    
    [System.Diagnostics.DebuggerDisplay("ManagedBitmapWriterOperand {Width}x{Height} {Format}")]
    public readonly ref struct ManagedBitmapWriterOperand<TPixel>
        : IBitmapWriter<ManagedBitmapWriterOperand<TPixel>, TPixel>
        where TPixel : unmanaged
    {
        #region lifecycle
        public ManagedBitmapWriterOperand(IBitmapWriter<TPixel> managed)
        {
            _Managed = managed;
        }

        #endregion

        #region data

        private readonly IBitmapWriter<TPixel> _Managed;

        public PixelFormat Format => _Managed.Format;
        public int Width => _Managed.Width;
        public int Height => _Managed.Height;

        #endregion

        #region API - Rows

        public void ReadRowPixelsSpan(int y, Span<TPixel> dst) => _Managed.ReadRowPixelsSpan(y, dst);
        public void ReadRowBytesSpan(int y, Span<byte> dst) => _Managed.ReadRowBytesSpan(y, dst);

        public void WriteRowPixelsSpan(int y, ReadOnlySpan<TPixel> src) => _Managed.WriteRowPixelsSpan(y, src);        
        public void WriteRowBytesSpan(int y, ReadOnlySpan<byte> src) => _Managed.WriteRowBytesSpan(y, src);

        #endregion

        #region API

        public bool TryCastTo<T>(out T managedBitmap)
            where T : IBitmapWriter<TPixel>
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

        public ManagedBitmapWriterOperand<TPixel> GetCropped(Rectangle rectangle)
        {
            var cropped = new _BitmapWriterCropped<TPixel>(_Managed, rectangle);
            return new ManagedBitmapWriterOperand<TPixel>(cropped);
        }

        public BITMAPOPERATORS.FillerContext<ManagedBitmapWriterOperand<TPixel>, TPixel, TContextPixel> GetFillerContext<TContextPixel>() where TContextPixel : unmanaged
        {
            return new BITMAPOPERATORS.FillerContext<ManagedBitmapWriterOperand<TPixel>, TPixel, TContextPixel>(this);
        }

        #endregion
    }




    /// <summary>
    /// This class is used to wrap a managed <see cref="IReadOnlyBitmap{TPixel}"/> with a ByRef operand
    /// </summary>    
    [System.Diagnostics.DebuggerDisplay("ManagedReadOnlyBitmapOperand {Width}x{Height} {Format}")]
    public readonly ref struct ManagedReadOnlyBitmapOperand<TPixel>
        : IReadOnlyBitmap<ManagedReadOnlyBitmapOperand<TPixel>,TPixel>        
        where TPixel: unmanaged
    {
        #region lifecycle
        public ManagedReadOnlyBitmapOperand(IReadOnlyBitmap<TPixel> managed)
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
        public ReadOnlySpan<byte> GetRowBytesSpan(int y) => System.Runtime.InteropServices.MemoryMarshal.AsBytes(GetRowPixelsSpan(y));

        public void ReadRowPixelsSpan(int y, Span<TPixel> dst) => GetRowPixelsSpan(y).CopyTo(dst);
        public void ReadRowBytesSpan(int y, Span<byte> dst) => GetRowBytesSpan(y).CopyTo(dst);        

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

        public ManagedReadOnlyBitmapOperand<TPixel> GetCropped(Rectangle rectangle)
        {
            var cropped = new _ReadOnlyBitmapCropped<TPixel>(_Managed, rectangle);

            return new ManagedReadOnlyBitmapOperand<TPixel>(cropped);
        }

        #endregion

    }

    /// <summary>
    /// This class is used to wrap a managed <see cref="IBitmap{TPixel}"/> with a ByRef operand
    /// </summary>    
    [System.Diagnostics.DebuggerDisplay("ManagedBitmapOperand {Width}x{Height} {Format}")]
    public readonly ref struct ManagedBitmapOperand<TPixel>
        : IBitmap<ManagedBitmapOperand<TPixel>, TPixel>
        where TPixel : unmanaged
    {
        #region lifecycle
        public ManagedBitmapOperand(IBitmap<TPixel> managed)
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
        public Span<byte> GetRowBytesSpan(int y) => System.Runtime.InteropServices.MemoryMarshal.AsBytes(GetRowPixelsSpan(y));

        ReadOnlySpan<TPixel> IReadOnlyBitmap<TPixel>.GetRowPixelsSpan(int y) => GetRowPixelsSpan(y);
        ReadOnlySpan<byte> IReadOnlyBitmap.GetRowBytesSpan(int y) => System.Runtime.InteropServices.MemoryMarshal.AsBytes(GetRowPixelsSpan(y));        

        public void ReadRowPixelsSpan(int y, Span<TPixel> dst) => GetRowPixelsSpan(y).CopyTo(dst);
        public void WriteRowPixelsSpan(int y, ReadOnlySpan<TPixel> src) => src.CopyTo(GetRowPixelsSpan(y));

        public void ReadRowBytesSpan(int y, Span<byte> dst) => GetRowBytesSpan(y).CopyTo(dst);
        public void WriteRowBytesSpan(int y, ReadOnlySpan<byte> src) => src.CopyTo(GetRowBytesSpan(y));

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

        public ManagedBitmapOperand<TPixel> GetCropped(Rectangle rectangle)
        {
            var cropped = new _BitmapCropped<TPixel>(_Managed, rectangle);

            return new ManagedBitmapOperand<TPixel>(cropped);
        }

        public BITMAPOPERATORS.DrawingContext<ManagedBitmapOperand<TPixel>, TPixel, TContextPixel> GetDrawingContext<TContextPixel>()
            where TContextPixel : unmanaged
        {
            return new BITMAPOPERATORS.DrawingContext<ManagedBitmapOperand<TPixel>, TPixel, TContextPixel>(this);
        }        

        public BITMAPOPERATORS.FillerContext<ManagedBitmapOperand<TPixel>, TPixel, TContextPixel> GetFillerContext<TContextPixel>() where TContextPixel : unmanaged
        {
            return new BITMAPOPERATORS.FillerContext<ManagedBitmapOperand<TPixel>, TPixel, TContextPixel>(this);
        }

        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

using InteropTypes.Numerics;

namespace InteropTypes.TensorBitmaps.Operands
{
    /// <summary>
    /// This class is used to wrap a managed <see cref="IReadOnlyBitmap{TPixel}"/> with a ByRef operand
    /// </summary>    
    [System.Diagnostics.DebuggerDisplay("ManagedReadOnlyBitmapOperand {Width}x{Height} {Format}")]
    public readonly ref struct ManagedReadOnlyBitmapOperand<TPixel>
        : IReadOnlyBitmapOperand<ManagedReadOnlyBitmapOperand<TPixel>,TPixel>
        where TPixel: unmanaged
    {
        public ManagedReadOnlyBitmapOperand(IReadOnlyBitmap<TPixel> managed)
        {
            _Managed = managed;
        }

        private readonly IReadOnlyBitmap<TPixel> _Managed;
        public PixelFormat Format => _Managed.Format;
        public int Width => _Managed.Width;
        public int Height => _Managed.Height;

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

        public ReadOnlySpan<byte> GetRowBytesSpan(int y) => _Managed.GetRowBytesSpan(y);
        public ReadOnlySpan<TPixel> GetRowPixelsSpan(int y) => _Managed.GetRowPixelsSpan(y);


        public ManagedReadOnlyBitmapOperand<TPixel> GetCropped(Rectangle rectangle)
        {
            var cropped = new _ReadOnlyBitmapCropped<TPixel>(_Managed, rectangle);

            return new ManagedReadOnlyBitmapOperand<TPixel>(cropped);
        }               
    }

    /// <summary>
    /// This class is used to wrap a managed <see cref="IBitmap{TPixel}"/> with a ByRef operand
    /// </summary>    
    [System.Diagnostics.DebuggerDisplay("ManagedBitmapOperand {Width}x{Height} {Format}")]
    public readonly ref struct ManagedBitmapOperand<TPixel>
        : IBitmapOperand<ManagedBitmapOperand<TPixel>, TPixel>
        where TPixel : unmanaged
    {
        public ManagedBitmapOperand(IBitmap<TPixel> managed)
        {
            _Managed = managed;
        }

        private readonly IBitmap<TPixel> _Managed; 

        public PixelFormat Format => _Managed.Format;
        public int Width => _Managed.Width;
        public int Height => _Managed.Height;

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

        ReadOnlySpan<byte> IReadOnlyBitmap.GetRowBytesSpan(int y) => _Managed.GetRowBytesSpan(y);
        ReadOnlySpan<TPixel> IReadOnlyBitmap<TPixel>.GetRowPixelsSpan(int y) => _Managed.GetRowPixelsSpan(y);
        public Span<byte> GetRowBytesSpan(int y) => _Managed.GetRowBytesSpan(y);
        public Span<TPixel> GetRowPixelsSpan(int y) => _Managed.GetRowPixelsSpan(y);

        public ManagedBitmapOperand<TPixel> GetCropped(Rectangle rectangle)
        {
            var cropped = new _BitmapCropped<TPixel>(_Managed, rectangle);

            return new ManagedBitmapOperand<TPixel>(cropped);
        }

        public BITMAPOPERATORS.BinaryOperatorContext<ManagedBitmapOperand<TPixel>, TPixel, TContextPixel> GetContext<TContextPixel>()
            where TContextPixel : unmanaged
        {
            return new BITMAPOPERATORS.BinaryOperatorContext<ManagedBitmapOperand<TPixel>, TPixel, TContextPixel>(this);
        }
    }
}

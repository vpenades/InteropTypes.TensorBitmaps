using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InteropTypes.TensorBitmaps.Primitives
{

    /// <summary>
    /// interface to be implemented by actual managed bitmaps, not wrappers
    /// </summary>
    /// <typeparam name="TPixel"></typeparam>
    public interface IReadOnlyBitmapFull<TPixel>
        : IReadOnlyBitmap<TPixel>
        where TPixel : unmanaged
    {
        void IReadOnlyBitmap<TPixel>.ReadRowPixelsSpan(int y, int x, scoped Span<TPixel> dst)
        {
            var row = GetRowPixelsSpan(y).Slice(x);
            row = row.Slice(0, Math.Min(row.Length, dst.Length));
            row.CopyTo(dst);
        }
    }

    // <summary>
    /// interface to be implemented by actual managed bitmaps, not wrappers
    /// </summary>
    /// <typeparam name="TPixel"></typeparam>
    public interface IBitmapFull<TPixel>
        : IReadOnlyBitmapFull<TPixel>
        , IBitmap<TPixel>
        where TPixel : unmanaged
    {
        void IBitmap<TPixel>.WriteRowPixelsSpan(int y, int x, scoped ReadOnlySpan<TPixel> src)
        {
            var row = GetRowPixelsSpan(y).Slice(x);
            src.CopyTo(row);
        }

        #if NET9_0_OR_GREATER
        public Operators.BitmapOperationContext<RefStructBitmap<TPixel>, TPixel, TContextPixel> GetContext<TContextPixel>()
            where TContextPixel : unmanaged
        {
            return new RefStructBitmap<TPixel>(this).GetContext<TContextPixel>();
        }
        #endif
    }
}

using System;
using System.Collections.Generic;
using System.Text;

using InteropTypes.TensorBitmaps.Operands;

namespace InteropTypes.TensorBitmaps
{
    public static class TensorBitmapExtensions
    {
        public static TensorBitmap<byte, TPixel> ToBytesTensorBitmap<TBitmap,TPixel>(this TBitmap src)
            where TBitmap: IReadOnlyBitmapOperand<TBitmap,TPixel>
            where TPixel: unmanaged
        {
            var dst = TensorBitmap<byte, TPixel>.Create(src.Width, src.Height, src.Format);

            dst.GetContext<TPixel>().Fill(PixelsTransform.Copy, src);

            return dst;
        }
    }
}

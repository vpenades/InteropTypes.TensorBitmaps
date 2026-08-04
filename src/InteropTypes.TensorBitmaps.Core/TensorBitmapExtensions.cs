using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

using InteropTypes.TensorBitmaps.Operands;

namespace InteropTypes.TensorBitmaps
{
    public static class TensorBitmapExtensions
    {
        public static void ToTensorBitmap<TSrcBitmap, TSrcPixel, TDstElement>(this TSrcBitmap src, out TensorBitmap<TDstElement, TSrcPixel> dst)
            where TSrcBitmap : IReadOnlyBitmap<TSrcBitmap, TSrcPixel>
            where TSrcPixel : unmanaged
            where TDstElement : unmanaged, INumber<TDstElement>
        {
            dst = TensorBitmap<TDstElement, TSrcPixel>.Create(src.Width, src.Height, src.Format);

            dst.GetDrawingContext<TSrcPixel>().Draw(BitmapOperations.DrawCopy, src);
        }

        public static void ToTensorBitmap<TSrcPixel, TDstElement>(this IReadOnlyBitmap<TSrcPixel> src, out TensorBitmap<TDstElement, TSrcPixel> dst)            
            where TSrcPixel: unmanaged
            where TDstElement: unmanaged, INumber<TDstElement>
        {
            dst = TensorBitmap<TDstElement, TSrcPixel>.Create(src.Width, src.Height, src.Format);
            var srcx = new ManagedReadOnlyBitmapOperand<TSrcPixel>(src);

            dst.GetDrawingContext<TSrcPixel>().Draw(BitmapOperations.DrawCopy, srcx);
        }
    }
}

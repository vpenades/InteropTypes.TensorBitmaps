using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using SkiaSharp;

namespace InteropTypes.TensorBitmaps
{
    public static partial class SkiaSharpForTensorBitmapsExtensions
    {
        /*
        public static SKBitmap ToSkiaSharp<TElement, TPixel>(this TensorBitmap<TElement, TPixel> srcBitmap)
            where TElement : unmanaged, INumber<TElement>
            where TPixel : unmanaged
        {
            return ToSkiaSharp(srcBitmap.AsReadOnlyTensorSpanBitmap());
        }

        public static SKBitmap ToSkiaSharp<TElement, TPixel>(this TensorSpanBitmap<TElement, TPixel> srcBitmap)
            where TElement : unmanaged, INumber<TElement>
            where TPixel : unmanaged
        {
            return ToSkiaSharp(srcBitmap.AsReadOnlyTensorSpanBitmap());
        }

        public static SKBitmap ToSkiaSharp<TElement, TPixel>(this ReadOnlyTensorSpanBitmap<TElement, TPixel> srcBitmap)
            where TElement : unmanaged, INumber<TElement>
            where TPixel : unmanaged
        {
            var dstImage = new SKBitmap(srcBitmap.Width, srcBitmap.Height, true); // todo: check srcBitmap.Format for alpha

            if (DangerousTryCastToTensorBitmap<byte>(dstImage, out var dstTensor1))
            {
                srcBitmap.CopyPixelsTo(dstTensor1);
                return dstImage;
            }

            if (DangerousTryCastToTensorBitmap<int>(dstImage, out var dstTensor2))
            {
                srcBitmap.CopyPixelsTo(dstTensor2);
                return dstImage;
            }

            throw new NotImplementedException();
        }*/
    }
}

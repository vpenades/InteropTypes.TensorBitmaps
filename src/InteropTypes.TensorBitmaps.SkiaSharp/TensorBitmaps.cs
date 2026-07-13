using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using InteropTypes.Numerics;

using SkiaSharp;

namespace InteropTypes.TensorBitmaps
{
    public static partial class SkiaSharpForTensorBitmapsExtensions
    {
        public static TensorBitmap<TElement, TPixel> ToResizedTensorBitmap<TElement, TPixel>(this SKBitmap srcImage, int newWidth, int newHeight, PixelFormat dstFmt, SKSamplingOptions? options = null)
            where TElement : unmanaged, INumber<TElement>
            where TPixel : unmanaged
        {
            options ??= SKSamplingOptions.Default;

            var s = new SKSizeI(newWidth, newHeight);

            using (var resized = srcImage.Resize(s, options.Value))
            {
                return resized.ToTensorBitmap<TElement, TPixel>(dstFmt);
            }
        }

        public static TensorBitmap<TElement, TPixel> ToTensorBitmap<TElement, TPixel>(this SKBitmap srcImage, PixelFormat dstFmt)
            where TElement : unmanaged, INumber<TElement>
            where TPixel : unmanaged
        {
            if (DangerousTryCastToTensorBitmap<byte>(srcImage, out var srcTensor1))
            {
                var dstBitmap = TensorBitmap<TElement, TPixel>.Create(srcImage.Width, srcImage.Height, dstFmt);
                srcTensor1.CopyPixelsTo(dstBitmap.AsTensorSpanBitmap());
                return dstBitmap;
            }

            if (DangerousTryCastToTensorBitmap<int>(srcImage, out var srcTensor4))
            {
                var dstBitmap = TensorBitmap<TElement, TPixel>.Create(srcImage.Width, srcImage.Height, dstFmt);
                srcTensor4.CopyPixelsTo(dstBitmap.AsTensorSpanBitmap());
                return dstBitmap;
            }

            throw new NotImplementedException();
        }

        public static bool DangerousTryCastToTensorBitmap<TPixel>(this SKBitmap srcImage, out TensorSpanBitmap<byte, TPixel> tensorBitmap)
            where TPixel : unmanaged
        {
            tensorBitmap = default;

            var srcFmt = ToPixelFormat((srcImage.ColorType, srcImage.AlphaType));
            if (srcFmt.BytesPerPixel != srcImage.BytesPerPixel) return false;
            if (!srcFmt.HasSameBytesPerPixelAs<TPixel>()) return false;
            if (!srcFmt.HasSameCommonComponentTypeAs<byte>()) return false;            

            if (srcImage.BytesPerPixel == 1)
            {
                var srcTensor = DangerousGetTensorSpan(srcImage);
                tensorBitmap = new TensorSpanBitmap<byte, byte>(srcTensor, srcFmt).Cast<TPixel>();
                return true;
            }

            if (srcImage.BytesPerPixel == 4)
            {
                var srcTensor = DangerousGetTensorSpan(srcImage);
                tensorBitmap = new TensorSpanBitmap<byte, uint>(srcTensor, srcFmt).Cast<TPixel>();
                return true;
            }
            
            return false;
        }        
    }
}

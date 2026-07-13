using System;
using System.Numerics.Tensors;
using System.Text;
using System.Threading.Tasks;

using SkiaSharp;

namespace InteropTypes.TensorBitmaps
{
    public static partial class SkiaSharpForTensorBitmapsExtensions
    {
        /// <summary>
        /// Gets a <see cref="TensorSpan{T}"/> representing the contents of the <see cref="SKBitmap"/>
        /// </summary>
        /// <param name="srcBitmap">the source bitmap</param>
        /// <returns>A tensorSpan that is valid only as long <see cref="SKBitmap"/> is not disposed.</returns>
        public static TensorSpan<byte> DangerousGetTensorSpan(this SKBitmap srcBitmap)
        {
            ArgumentNullException.ThrowIfNull(srcBitmap);

            var srcBuffer = srcBitmap.GetPixelSpan();

            nint w = srcBitmap.Width;
            nint h = srcBitmap.Height;
            nint stride = srcBitmap.RowBytes;
            nint bpp = srcBitmap.BytesPerPixel;

            return new TensorSpan<byte>(srcBuffer, [h, w, bpp], [stride, bpp, bpp > 1 ? 1 : 0]);
        }
    }
}

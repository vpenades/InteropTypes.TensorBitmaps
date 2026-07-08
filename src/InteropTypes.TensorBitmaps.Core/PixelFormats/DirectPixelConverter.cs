using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InteropTypes.TensorBitmaps
{
    /// <summary>
    /// When src and dst pixels match in type and format, use fast copy.
    /// </summary>
    /// <typeparam name="TSrcPixel"></typeparam>
    /// <typeparam name="TDstPixel"></typeparam>
    sealed class DirectPixelConverter<TSrcPixel, TDstPixel> : IPixelConverter<TSrcPixel, TDstPixel>
        where TSrcPixel : unmanaged
        where TDstPixel : unmanaged
    {
        public void ConvertPixels(ReadOnlySpan<TSrcPixel> source, Span<TDstPixel> target)
        {
            var sourcex = System.Runtime.InteropServices.MemoryMarshal.Cast<TSrcPixel, TDstPixel>(source);

            var len = Math.Min(sourcex.Length, target.Length);
            sourcex.Slice(0, len).CopyTo(target);
        }
    }
}

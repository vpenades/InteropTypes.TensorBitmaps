using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace InteropTypes.Numerics.Internal
{
    /// <summary>
    /// Fast copy for when src and dst pixels match in type and format.
    /// </summary>
    /// <typeparam name="TSrcPixel"></typeparam>
    /// <typeparam name="TDstPixel"></typeparam>
    sealed class DirectPixelConverter<TSrcPixel, TDstPixel> : IPixelConverter<TSrcPixel, TDstPixel>
        where TSrcPixel : unmanaged
        where TDstPixel : unmanaged
    {
        public static bool TryCreate(PixelFormat srcFmt, PixelFormat dstFmt, out DirectPixelConverter<TSrcPixel, TDstPixel> converter)
        {
            if (Unsafe.SizeOf<TSrcPixel>() == Unsafe.SizeOf<TDstPixel>())
            {
                converter = default;
                return false;
            }

            if (srcFmt != dstFmt)
            {
                converter = default;
                return false;
            }

            converter = new DirectPixelConverter<TSrcPixel, TDstPixel>();
            return true;
        }

        public void ConvertPixels(ReadOnlySpan<TSrcPixel> source, Span<TDstPixel> target)
        {
            var sourcex = System.Runtime.InteropServices.MemoryMarshal.Cast<TSrcPixel, TDstPixel>(source);

            var len = Math.Min(sourcex.Length, target.Length);
            sourcex.Slice(0, len).CopyTo(target);
        }
    }
}

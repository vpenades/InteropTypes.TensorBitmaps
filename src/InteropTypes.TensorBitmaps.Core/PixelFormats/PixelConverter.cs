using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace InteropTypes.TensorBitmaps
{
    public interface IPixelConverter<TSrcPixel, TDstPixel>
        where TSrcPixel : unmanaged
        where TDstPixel : unmanaged
    {
        void ConvertPixels(ReadOnlySpan<TSrcPixel> source, Span<TDstPixel> target);
    }           

    static class PixelConverters
    {
        public static IPixelConverter<TSrcPixel, TDstPixel> Create<TSrc, TSrcPixel, TDst, TDstPixel>(TensorPixelFormat sourceFmt, TensorPixelFormat targetFmt, bool initPixels)
            where TSrc : unmanaged, INumber<TSrc>
            where TSrcPixel : unmanaged
            where TDst : unmanaged, INumber<TDst>
            where TDstPixel : unmanaged
        {
            // direct converter when both types and formats are exactly the same:

            if (typeof(TSrcPixel) == typeof(TDstPixel) && sourceFmt == targetFmt)
            {
                return new DirectPixelConverter<TSrcPixel, TDstPixel>();
            }

            // converters using floating point:

            if (typeof(TSrc) == typeof(double) || typeof(TDst) == typeof(double))
            {
                return new ComponentWisePixelConverter<TSrc, TSrcPixel, TDst, TDstPixel, ComponentConverter<TSrc, double, TDst>>(sourceFmt.Components, targetFmt.Components, initPixels);
            }

            if (typeof(TSrc) == typeof(float) || typeof(TDst) == typeof(float))
            {
                return new ComponentWisePixelConverter<TSrc, TSrcPixel, TDst, TDstPixel, ComponentConverter<TSrc, float, TDst>>(sourceFmt.Components, targetFmt.Components, initPixels);
            }

            if (typeof(TSrc) == typeof(Half) || typeof(TDst) == typeof(Half))
            {
                return new ComponentWisePixelConverter<TSrc, TSrcPixel, TDst, TDstPixel, ComponentConverter<TSrc, Half, TDst>>(sourceFmt.Components, targetFmt.Components, initPixels);
            }

            // converter using integer:
            
            return new ComponentWisePixelConverter<TSrc, TSrcPixel, TDst, TDstPixel, ComponentConverterInteger<TSrc, TDst>>(sourceFmt.Components, targetFmt.Components, initPixels);            
        }
    }    
}

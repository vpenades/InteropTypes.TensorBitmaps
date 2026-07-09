using System;
using System.Numerics;
using System.Runtime.CompilerServices;

using InteropTypes.Numerics.Internal;

namespace InteropTypes.Numerics
{
    public interface IPixelConverterSource<TSrcPixel>
        where TSrcPixel : unmanaged
    {
        public bool CanConvertTo<TDstPixel>(out IPixelConverter<TSrcPixel, TDstPixel> converter)
            where TDstPixel : unmanaged;
    }

    public interface IPixelConverter<TSrcPixel, TDstPixel>
        where TSrcPixel : unmanaged
        where TDstPixel : unmanaged
    {
        public static IPixelConverter<TSrcPixel, TDstPixel> Create(PixelFormat sourceFmt, PixelFormat targetFmt, bool initPixels)            
        {
            return Internal.PixelConverters.Create<TSrcPixel, TDstPixel>(sourceFmt, targetFmt, initPixels);
        }

        void ConvertPixels(ReadOnlySpan<TSrcPixel> source, Span<TDstPixel> target);
    }    
}

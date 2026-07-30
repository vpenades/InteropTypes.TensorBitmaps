using System;
using System.Numerics;
using System.Runtime.CompilerServices;

using InteropTypes.Numerics.Internal;

namespace InteropTypes.Numerics
{
    /// <summary>
    /// This may be implemented by pixels to expose converters to other types.
    /// </summary>
    /// <typeparam name="TSrcPixel"></typeparam>
    public interface IPixelConverterSource<TSrcPixel>
        where TSrcPixel : unmanaged
    {
        public bool CanConvertTo<TDstPixel>(out IPixelConverter<TSrcPixel, TDstPixel> converter)
            where TDstPixel : unmanaged;
    }

    /// <summary>
    /// Base interface to convert between pixel types and formats
    /// </summary>
    /// <typeparam name="TSrcPixel">The source pixel type. It can be anything, since the actual layout is defined by <see cref="PixelFormat"/></typeparam>
    /// <typeparam name="TDstPixel">The target pixel type. It can be anything, since the actual layout is defined by <see cref="PixelFormat"/></typeparam>
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

using System;
using System.Numerics;
using System.Runtime.CompilerServices;

using InteropTypes.Numerics.Internal;

namespace InteropTypes.Numerics
{
    public interface IPixelConverter<TSrcPixel, TDstPixel>
        where TSrcPixel : unmanaged
        where TDstPixel : unmanaged
    {
        public static IPixelConverter<TSrcPixel, TDstPixel> Create(PixelFormat sourceFmt, PixelFormat targetFmt, bool initPixels)            
        {
            return PixelConverters.Create<TSrcPixel, TDstPixel>(sourceFmt, targetFmt, initPixels);
        }

        void ConvertPixels(ReadOnlySpan<TSrcPixel> source, Span<TDstPixel> target);
    }           

    static class PixelConverters
    {
        public static IPixelConverter<TSrcPixel, TDstPixel> Create<TSrcPixel, TDstPixel>(PixelFormat sourceFmt, PixelFormat targetFmt, bool initPixels)
            where TSrcPixel : unmanaged
            where TDstPixel : unmanaged
        {
            if (Unsafe.SizeOf<TSrcPixel>() != sourceFmt.BytesPerPixel) throw new ArgumentException("BytesPerPixel mismatch", nameof(sourceFmt));
            if (Unsafe.SizeOf<TDstPixel>() != targetFmt.BytesPerPixel) throw new ArgumentException("BytesPerPixel mismatch", nameof(targetFmt));

            // direct converter when both types and formats are exactly the same:

            if (DirectPixelConverter<TSrcPixel, TDstPixel>.TryCreate(sourceFmt, targetFmt, out var directCvt)) return directCvt;
            
            // component wise converters

            if (!sourceFmt.TryGetCommonType(out var st)) throw new ArgumentException("invalid format", nameof(sourceFmt));            

            if (st == typeof(byte))   return Create<byte, TSrcPixel, TDstPixel>(sourceFmt, targetFmt, initPixels);
            if (st == typeof(sbyte))  return Create<sbyte, TSrcPixel, TDstPixel>(sourceFmt, targetFmt, initPixels);
            if (st == typeof(short))  return Create<short, TSrcPixel, TDstPixel>(sourceFmt, targetFmt, initPixels);
            if (st == typeof(ushort)) return Create<ushort, TSrcPixel, TDstPixel>(sourceFmt, targetFmt, initPixels);
            if (st == typeof(int))    return Create<int, TSrcPixel, TDstPixel>(sourceFmt, targetFmt, initPixels);
            if (st == typeof(uint))   return Create<uint, TSrcPixel, TDstPixel>(sourceFmt, targetFmt, initPixels);
            if (st == typeof(long))   return Create<long, TSrcPixel, TDstPixel>(sourceFmt, targetFmt, initPixels);
            if (st == typeof(ulong))  return Create<ulong, TSrcPixel, TDstPixel>(sourceFmt, targetFmt, initPixels);

            if (st == typeof(Half))   return Create<Half, TSrcPixel, TDstPixel>(sourceFmt, targetFmt, initPixels);
            if (st == typeof(Single)) return Create<Single, TSrcPixel, TDstPixel>(sourceFmt, targetFmt, initPixels);
            if (st == typeof(Double)) return Create<Double, TSrcPixel, TDstPixel>(sourceFmt, targetFmt, initPixels);

            throw new NotSupportedException(st.Name);
        }

        public static IPixelConverter<TSrcPixel, TDstPixel> Create<TSrc, TSrcPixel, TDstPixel>(PixelFormat sourceFmt, PixelFormat targetFmt, bool initPixels)
            where TSrc : unmanaged, INumber<TSrc>
            where TSrcPixel : unmanaged            
            where TDstPixel : unmanaged
        {
            if (!targetFmt.TryGetCommonType(out var dt)) throw new ArgumentException("invalid format", nameof(targetFmt));

            if (dt == typeof(byte))   return Create<TSrc, TSrcPixel, byte, TDstPixel>(sourceFmt, targetFmt, initPixels);
            if (dt == typeof(sbyte))  return Create<TSrc, TSrcPixel, sbyte, TDstPixel>(sourceFmt, targetFmt, initPixels);
            if (dt == typeof(short))  return Create<TSrc, TSrcPixel, short, TDstPixel>(sourceFmt, targetFmt, initPixels);
            if (dt == typeof(ushort)) return Create<TSrc, TSrcPixel, ushort, TDstPixel>(sourceFmt, targetFmt, initPixels);
            if (dt == typeof(int))    return Create<TSrc, TSrcPixel, int, TDstPixel>(sourceFmt, targetFmt, initPixels);
            if (dt == typeof(uint))   return Create<TSrc, TSrcPixel, uint, TDstPixel>(sourceFmt, targetFmt, initPixels);
            if (dt == typeof(long))   return Create<TSrc, TSrcPixel, long, TDstPixel>(sourceFmt, targetFmt, initPixels);
            if (dt == typeof(ulong))  return Create<TSrc, TSrcPixel, ulong, TDstPixel>(sourceFmt, targetFmt, initPixels);

            if (dt == typeof(Half))   return Create<TSrc, TSrcPixel, Half, TDstPixel>(sourceFmt, targetFmt, initPixels);
            if (dt == typeof(Single)) return Create<TSrc, TSrcPixel, Single, TDstPixel>(sourceFmt, targetFmt, initPixels);
            if (dt == typeof(Double)) return Create<TSrc, TSrcPixel, Double, TDstPixel>(sourceFmt, targetFmt, initPixels);

            throw new NotSupportedException(dt.Name);
        }

        public static IPixelConverter<TSrcPixel, TDstPixel> Create<TSrc, TSrcPixel, TDst, TDstPixel>(PixelFormat sourceFmt, PixelFormat targetFmt, bool initPixels)
            where TSrc : unmanaged, INumber<TSrc>
            where TSrcPixel : unmanaged
            where TDst : unmanaged, INumber<TDst>
            where TDstPixel : unmanaged
        {
            // direct converter when both types and formats are exactly the same:

            if (DirectPixelConverter<TSrcPixel, TDstPixel>.TryCreate(sourceFmt, targetFmt, out var directCvt)) return directCvt;

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

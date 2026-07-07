using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace InteropTypes.TensorBitmaps
{
    

    /// <summary>
    /// this is a naive, component by component pixel converter
    /// </summary>
    /// <remarks>
    /// A more complex converter could take advantage of SIMD to convert entire pixel rows, but having this now is better than nothing.
    /// </remarks>    
    class PixelConverter<TSrc, TDst, TConverter>
        where TSrc: unmanaged
        where TDst: unmanaged
        where TConverter : IComponentConverter<TSrc, TDst>
    {
        public PixelConverter(IReadOnlyList<TensorPixelComponent> src, IReadOnlyList<TensorPixelComponent> dst, bool initPixels)
        {
            var ccc = new List<TConverter>();
            var uuu = new List<(int, TDst)>();

            for (int i = 0; i < dst.Count; ++i)
            {
                var srcIdx = -1;
                for (int j = 0; j < src.Count; ++j)
                {
                    if (src[j].Semantic == dst[i].Semantic) { srcIdx = j; break; }
                }                

                
                var dc = dst[i] as TensorPixelComponent<TDst>;
                if (dc == null) continue;

                if (srcIdx < 0)
                {                    
                    uuu.Add((i, dc.DefaultValue));
                    continue;
                }

                var sc = src[srcIdx] as TensorPixelComponent<TSrc>;
                if (sc == null) continue;

                var c = (TConverter)IComponentConverter<TSrc, TDst>.CreateFrom(sc, dc, srcIdx, i);
                ccc.Add(c);
            }

            if (!initPixels) uuu.Clear();

            _Converters = ccc.ToArray();
            if (uuu.Count > 0) _Unfilled = uuu.ToArray();
        }

        private readonly TConverter[] _Converters;

        private readonly (int, TDst)[]? _Unfilled;

        public void ConvertPixels<TSrcPixel, TDstPixel>(ReadOnlySpan<TSrcPixel> src, Span<TDstPixel> dst)
            where TSrcPixel : unmanaged
            where TDstPixel : unmanaged
        {
            var len = Math.Min(src.Length, dst.Length);            

            var srcc = System.Runtime.InteropServices.MemoryMarshal.Cast<TSrcPixel, TSrc>(src.Slice(0, len));
            var dstc = System.Runtime.InteropServices.MemoryMarshal.Cast<TDstPixel, TDst>(dst.Slice(0, len));
            int srcstride = Unsafe.SizeOf<TSrcPixel>() / Unsafe.SizeOf<TSrc>();
            int dststride = Unsafe.SizeOf<TDstPixel>() / Unsafe.SizeOf<TDst>();

            if (_Unfilled == null)
            {
                while (dstc.Length > 0)
                {
                    CopyPixel(srcc, dstc);
                    srcc = srcc.Slice(srcstride);
                    dstc = dstc.Slice(dststride);
                }
            }
            else
            {
                while (dstc.Length > 0)
                {
                    CopyPixel(srcc, dstc);
                    foreach(var (idx,v) in _Unfilled)
                    {
                        dstc[idx] = v;
                    }

                    srcc = srcc.Slice(srcstride);
                    dstc = dstc.Slice(dststride);
                }
            }
        }

        private void CopyPixel(ReadOnlySpan<TSrc> srcPixel, Span<TDst> dstPixel)
        {
            for (int i = 0; i < _Converters.Length; ++i)
            {
                var cvt = _Converters[i];

                dstPixel[cvt.TargetIndex] = cvt.Convert(srcPixel[cvt.SourceIndex]);
            }
        }
    }

    interface IComponentConverter<TSrc, TDst>
        where TSrc:unmanaged
        where TDst:unmanaged
    {
        public static IComponentConverter<TSrc,TDst> CreateFrom(TensorPixelComponent<TSrc> src, TensorPixelComponent<TDst> dst, int srcIdx,int tgtIdx)
        {
            if (src is TensorPixelComponent<byte> srcB)
            {
                switch(dst)
                {
                    case TensorPixelComponent<byte> dstB: return new ComponentConverterByteByte(srcB, dstB, srcIdx,tgtIdx) as IComponentConverter<TSrc, TDst>;
                    case TensorPixelComponent<float> dstF: return new ComponentConverterByteFloat(srcB, dstF, srcIdx, tgtIdx) as IComponentConverter<TSrc, TDst>;
                }

            }

            if (src is TensorPixelComponent<float> srcF)
            {
                switch (dst)
                {
                    case TensorPixelComponent<byte> dstB: return new ComponentConverterFloatByte(srcF, dstB, srcIdx, tgtIdx) as IComponentConverter<TSrc, TDst>;
                    case TensorPixelComponent<float> dstF: return new ComponentConverterFloatFloat(srcF, dstF, srcIdx, tgtIdx) as IComponentConverter<TSrc, TDst>;
                }
            }

            throw new NotImplementedException();
        }

        int SourceIndex { get; }
        int TargetIndex { get; }
        TDst Convert(TSrc value);
    }

    class ComponentConverterFloatFloat : IComponentConverter<float, float>
    {
        public ComponentConverterFloatFloat(TensorPixelComponent<float> src, TensorPixelComponent<float> dst, int srcIdx, int tgtIdx)
        {
            _SrcOffset = -src.MinValue;
            _Scale = (dst.MaxValue - dst.MinValue) / (src.MaxValue - src.MinValue);
            _DstOffset = dst.MinValue;
            SourceIndex = srcIdx;
            TargetIndex = tgtIdx;
        }

        public int SourceIndex { get; }
        public int TargetIndex { get; }

        private readonly float _SrcOffset;
        private readonly float _Scale;                
        private readonly float _DstOffset;

        [System.Runtime.CompilerServices.MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public float Convert(float value)
        {
            value += _SrcOffset;
            value *= _Scale;
            value += _DstOffset;
            return value;            
        }        
    }

    class ComponentConverterFloatByte : IComponentConverter<float, byte>
    {
        public ComponentConverterFloatByte(TensorPixelComponent<float> src, TensorPixelComponent<byte> dst, int srcIdx, int tgtIdx)
        {
            _SrcOffset = -src.MinValue;
            _Scale = (dst.MaxValue - dst.MinValue) / (src.MaxValue - src.MinValue);
            _DstOffset = dst.MinValue;
            SourceIndex = srcIdx;
            TargetIndex = tgtIdx;
        }

        public int SourceIndex { get; }
        public int TargetIndex { get; }

        private readonly float _SrcOffset;
        private readonly float _Scale;
        private readonly float _DstOffset;

        [System.Runtime.CompilerServices.MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public byte Convert(float value)
        {
            value += _SrcOffset;
            value *= _Scale;
            value += _DstOffset;
            return (byte)value;
        }
    }

    class ComponentConverterByteFloat : IComponentConverter<byte, float>
    {
        public ComponentConverterByteFloat(TensorPixelComponent<byte> src, TensorPixelComponent<float> dst, int srcIdx, int tgtIdx)
        {
            _SrcOffset = -src.MinValue;
            _Scale = (dst.MaxValue - dst.MinValue) / (src.MaxValue - src.MinValue);
            _DstOffset = dst.MinValue;
            SourceIndex = srcIdx;
            TargetIndex = tgtIdx;
        }

        public int SourceIndex { get; }
        public int TargetIndex { get; }

        private readonly float _SrcOffset;
        private readonly float _Scale;
        private readonly float _DstOffset;

        [System.Runtime.CompilerServices.MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public float Convert(byte value)
        {
            float t = value;
            t += _SrcOffset;
            t *= _Scale;
            t += _DstOffset;
            return t;
        }
    }

    class ComponentConverterByteByte : IComponentConverter<byte, byte>
    {
        public ComponentConverterByteByte(TensorPixelComponent<byte> src, TensorPixelComponent<byte> dst, int srcIdx,int tgtIdx)
        {
            _SrcOffset = -(int)src.MinValue;
            _Scale = ((int)dst.MaxValue - (int)dst.MinValue) * 65536 / ((int)src.MaxValue - (int)src.MinValue);
            _DstOffset = (int)dst.MinValue;
            SourceIndex = srcIdx;
            TargetIndex = tgtIdx;
        }

        public int SourceIndex { get; }
        public int TargetIndex { get; }

        private readonly int _SrcOffset;
        private readonly int _Scale;
        private readonly int _DstOffset;

        [System.Runtime.CompilerServices.MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public byte Convert(byte value)
        {
            int t = value;
            t += _SrcOffset;
            t *= _Scale;
            t >>= 16;
            t += _DstOffset;
            return (byte)t;
        }
    }
}

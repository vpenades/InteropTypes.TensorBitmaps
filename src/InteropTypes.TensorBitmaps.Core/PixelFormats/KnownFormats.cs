using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CMPK = InteropTypes.TensorBitmaps.KnownComponentFormats;

using TPF = InteropTypes.TensorBitmaps.TensorPixelFormat;
using CMP8 = InteropTypes.TensorBitmaps.TensorPixelComponent<byte>;
using CMP16 = InteropTypes.TensorBitmaps.TensorPixelComponent<ushort>;
using CMPH = InteropTypes.TensorBitmaps.TensorPixelComponent<System.Half>;
using CMPS = InteropTypes.TensorBitmaps.TensorPixelComponent<float>;

namespace InteropTypes.TensorBitmaps
{
    public static class KnownPixelFormats
    {
        public static readonly TPF Alpha8 = new TPF(CMPK.AlphaByte);
        public static readonly TPF Luminance8 = new TPF(CMPK.LuminanceByte);

        public static readonly TPF Rgb888 = new TPF(CMPK.RedByte, CMPK.GreenByte, CMPK.BlueByte);
        public static readonly TPF Bgr888 = new TPF(CMPK.BlueByte, CMPK.GreenByte, CMPK.RedByte);

        public static readonly TPF Rgbx8888 = new TPF(CMPK.RedByte, CMPK.GreenByte, CMPK.BlueByte, CMPK.OpaqueAlphaByte);
        public static readonly TPF Rgba8888 = new TPF(CMPK.RedByte, CMPK.GreenByte, CMPK.BlueByte, CMPK.AlphaByte);
        public static readonly TPF Rgbp8888 = new TPF(CMPK.RedByte, CMPK.GreenByte, CMPK.BlueByte, CMPK.PremulByte);

        public static readonly TPF Bgrx8888 = new TPF(CMPK.BlueByte, CMPK.GreenByte, CMPK.RedByte, CMPK.OpaqueAlphaByte);
        public static readonly TPF Bgra8888 = new TPF(CMPK.BlueByte, CMPK.GreenByte, CMPK.RedByte, CMPK.AlphaByte);
        public static readonly TPF Bgrp8888 = new TPF(CMPK.BlueByte, CMPK.GreenByte, CMPK.RedByte, CMPK.PremulByte);

        public static readonly TPF Argb8888 = new TPF(CMPK.AlphaByte, CMPK.RedByte, CMPK.GreenByte, CMPK.BlueByte);
        public static readonly TPF Abgr8888 = new TPF(CMPK.AlphaByte, CMPK.BlueByte, CMPK.GreenByte, CMPK.RedByte);

        public static readonly TPF Rg1616 = new TPF(CMPK.RedShort, CMPK.GreenShort);

        public static readonly TPF RgbF32 = new TPF(CMPK.RedSingle, CMPK.GreenSingle, CMPK.BlueSingle);
        public static readonly TPF BgrF32 = new TPF(CMPK.BlueSingle, CMPK.GreenSingle, CMPK.RedSingle);
        public static readonly TPF RgbaF32 = new TPF(CMPK.RedSingle, CMPK.GreenSingle, CMPK.BlueSingle, CMPK.AlphaSingle);
        public static readonly TPF RgbpF32 = new TPF(CMPK.RedSingle, CMPK.GreenSingle, CMPK.BlueSingle, CMPK.PremulSingle);
    }

    public static class KnownComponentFormats
    {
        public static readonly CMP8 RedByte = new CMP8("Red", 0, 255);
        public static readonly CMP8 GreenByte = new CMP8("Green", 0, 255);
        public static readonly CMP8 BlueByte = new CMP8("Blue", 0, 255);
        public static readonly CMP8 AlphaByte = new CMP8("Alpha", 0, 255);
        public static readonly CMP8 PremulByte = new CMP8("Premultiplied", 0, 255);
        public static readonly CMP8 LuminanceByte = new CMP8("Luminance", 0, 255);

        public static readonly CMP16 RedShort = new CMP16("Red", ushort.MinValue, ushort.MaxValue);
        public static readonly CMP16 GreenShort = new CMP16("Green", ushort.MinValue, ushort.MaxValue);
        public static readonly CMP16 BlueShort = new CMP16("Blue", ushort.MinValue, ushort.MaxValue);
        public static readonly CMP16 AlphaShort = new CMP16("Alpha", ushort.MinValue, ushort.MaxValue);
        public static readonly CMP16 PremulShort = new CMP16("Premultiplied", ushort.MinValue, ushort.MaxValue);
        public static readonly CMP16 LuminanceShort = new CMP16("Luminance", ushort.MinValue, ushort.MaxValue);

        public static readonly CMPH RedHalf = new CMPH("Red", Half.Zero, Half.One);
        public static readonly CMPH GreenHalf = new CMPH("Green", Half.Zero, Half.One);
        public static readonly CMPH BlueHalf = new CMPH("Blue", Half.Zero, Half.One);
        public static readonly CMPH AlphaHalf = new CMPH("Alpha", Half.Zero, Half.One);
        public static readonly CMPH PremulHalf = new CMPH("Premultiplied", Half.Zero, Half.One);
        public static readonly CMPH LuminanceHalf = new CMPH("Luminance", Half.Zero, Half.One);

        public static readonly CMPS RedSingle = new CMPS("Red", 0, 1);
        public static readonly CMPS GreenSingle = new CMPS("Green", 0, 1);
        public static readonly CMPS BlueSingle = new CMPS("Blue", 0, 1);
        public static readonly CMPS AlphaSingle = new CMPS("Alpha", 0, 1);
        public static readonly CMPS PremulSingle = new CMPS("Premultiplied", 0, 1);
        public static readonly CMPS LuminanceSingle = new CMPS("Luminance", 0, 1);

        // these are special values for rgbX types where the value assigned to alpha(X) is unused and usually zero, but needs to be interpreted as 1 (opaque)

        public static readonly CMP8 OpaqueAlphaByte = new CMP8("Alpha", 255, 255);
        public static readonly CMP16 OpaqueAlphaShort = new CMP16("Alpha", ushort.MaxValue, ushort.MaxValue);
        public static readonly CMPH OpaqueAlphaHalf = new CMPH("Alpha", Half.One, Half.One);
        public static readonly CMPS OpaqueAlphaSingle = new CMPS("Alpha", 1, 1);
    }

    
}

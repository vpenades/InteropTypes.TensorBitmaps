using System;

using TPF = InteropTypes.Numerics.PixelFormat;

using CMPK = InteropTypes.Numerics.KnownComponentFormats;
using CMPBYTE = InteropTypes.Numerics.PixelComponent<byte>;
using CMPSHORT = InteropTypes.Numerics.PixelComponent<ushort>;
using CMPHALF = InteropTypes.Numerics.PixelComponent<System.Half>;
using CMPSINGLE = InteropTypes.Numerics.PixelComponent<float>;

namespace InteropTypes.Numerics
{
    public static class KnownPixelFormats
    {
        public static readonly TPF Alpha8 = new TPF(CMPK.AlphaByte);
        public static readonly TPF Luminance8 = new TPF(CMPK.LuminanceByte);
        public static readonly TPF La8 = new TPF(CMPK.LuminanceByte, CMPK.AlphaByte);        

        public static readonly TPF Alpha16 = new TPF(CMPK.AlphaShort);
        public static readonly TPF Luminance16 = new TPF(CMPK.LuminanceShort);
        public static readonly TPF La16 = new TPF(CMPK.LuminanceShort, CMPK.AlphaShort);

        public static readonly TPF AlphaF16 = new TPF(CMPK.AlphaHalf);
        public static readonly TPF LuminanceF16 = new TPF(CMPK.LuminanceHalf);
        public static readonly TPF LaF16 = new TPF(CMPK.LuminanceHalf, CMPK.AlphaHalf);

        public static readonly TPF Rgb8 = new TPF(CMPK.RedByte, CMPK.GreenByte, CMPK.BlueByte);
        public static readonly TPF Bgr8 = new TPF(CMPK.BlueByte, CMPK.GreenByte, CMPK.RedByte);
        public static readonly TPF RgbF32 = new TPF(CMPK.RedSingle, CMPK.GreenSingle, CMPK.BlueSingle);
        public static readonly TPF BgrF32 = new TPF(CMPK.BlueSingle, CMPK.GreenSingle, CMPK.RedSingle);

        public static readonly TPF Rgbx8 = new TPF(CMPK.RedByte, CMPK.GreenByte, CMPK.BlueByte, CMPK.OpaqueAlphaByte);
        public static readonly TPF Rgba8 = new TPF(CMPK.RedByte, CMPK.GreenByte, CMPK.BlueByte, CMPK.AlphaByte);
        public static readonly TPF RgbaF32 = new TPF(CMPK.RedSingle, CMPK.GreenSingle, CMPK.BlueSingle, CMPK.AlphaSingle);

        public static readonly TPF Bgrx8 = new TPF(CMPK.BlueByte, CMPK.GreenByte, CMPK.RedByte, CMPK.OpaqueAlphaByte);
        public static readonly TPF Bgra8 = new TPF(CMPK.BlueByte, CMPK.GreenByte, CMPK.RedByte, CMPK.AlphaByte);
        public static readonly TPF BgraF32 = new TPF(CMPK.BlueSingle, CMPK.GreenSingle, CMPK.RedSingle, CMPK.AlphaSingle);

        public static readonly TPF Argb8 = new TPF(CMPK.AlphaByte, CMPK.RedByte, CMPK.GreenByte, CMPK.BlueByte);
        public static readonly TPF Abgr8 = new TPF(CMPK.AlphaByte, CMPK.BlueByte, CMPK.GreenByte, CMPK.RedByte);

        public static readonly TPF Rg8 = new TPF(CMPK.RedByte, CMPK.GreenByte);
        public static readonly TPF Rg16 = new TPF(CMPK.RedShort, CMPK.GreenShort);
        public static readonly TPF RgF16 = new TPF(CMPK.RedHalf, CMPK.GreenHalf);
        public static readonly TPF SignedRgF16 = new TPF(CMPK.SignedRedHalf, CMPK.SignedGreenHalf);        

        public static readonly TPF RgbPremul8 = new TPF(CMPK.RedByte, CMPK.GreenByte, CMPK.BlueByte, CMPK.PremulByte);
        public static readonly TPF BgrPremul8 = new TPF(CMPK.BlueByte, CMPK.GreenByte, CMPK.RedByte, CMPK.PremulByte);
        public static readonly TPF RgbPremulF32 = new TPF(CMPK.RedSingle, CMPK.GreenSingle, CMPK.BlueSingle, CMPK.PremulSingle);
        public static readonly TPF BgrPremulF32 = new TPF(CMPK.BlueSingle, CMPK.GreenSingle, CMPK.RedSingle, CMPK.PremulSingle);
    }

    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static class KnownComponentFormats
    {
        public static readonly CMPBYTE RedByte = new CMPBYTE("Red");
        public static readonly CMPBYTE GreenByte = new CMPBYTE("Green");
        public static readonly CMPBYTE BlueByte = new CMPBYTE("Blue");
        public static readonly CMPBYTE AlphaByte = new CMPBYTE("Alpha");
        public static readonly CMPBYTE PremulByte = new CMPBYTE("Premultiplied");
        public static readonly CMPBYTE LuminanceByte = new CMPBYTE("Luminance");

        public static readonly CMPSHORT RedShort = new CMPSHORT("Red");
        public static readonly CMPSHORT GreenShort = new CMPSHORT("Green");
        public static readonly CMPSHORT BlueShort = new CMPSHORT("Blue");
        public static readonly CMPSHORT AlphaShort = new CMPSHORT("Alpha");
        public static readonly CMPSHORT PremulShort = new CMPSHORT("Premultiplied");
        public static readonly CMPSHORT LuminanceShort = new CMPSHORT("Luminance");

        public static readonly CMPHALF RedHalf = new CMPHALF("Red");
        public static readonly CMPHALF GreenHalf = new CMPHALF("Green");
        public static readonly CMPHALF BlueHalf = new CMPHALF("Blue");
        public static readonly CMPHALF AlphaHalf = new CMPHALF("Alpha");
        public static readonly CMPHALF PremulHalf = new CMPHALF("Premultiplied");
        public static readonly CMPHALF LuminanceHalf = new CMPHALF("Luminance");

        public static readonly CMPHALF SignedRedHalf = new CMPHALF("Red", -Half.One, Half.One);
        public static readonly CMPHALF SignedGreenHalf = new CMPHALF("Green", -Half.One, Half.One);
        public static readonly CMPHALF SignedBlueHalf = new CMPHALF("Blue", -Half.One, Half.One);
        public static readonly CMPHALF SignedAlphaHalf = new CMPHALF("Alpha", -Half.One, Half.One);
        public static readonly CMPHALF SignedPremulHalf = new CMPHALF("Premultiplied", -Half.One, Half.One);
        public static readonly CMPHALF SignedLuminanceHalf = new CMPHALF("Luminance", -Half.One, Half.One);

        public static readonly CMPSINGLE RedSingle = new CMPSINGLE("Red");
        public static readonly CMPSINGLE GreenSingle = new CMPSINGLE("Green");
        public static readonly CMPSINGLE BlueSingle = new CMPSINGLE("Blue");
        public static readonly CMPSINGLE AlphaSingle = new CMPSINGLE("Alpha");
        public static readonly CMPSINGLE PremulSingle = new CMPSINGLE("Premultiplied");
        public static readonly CMPSINGLE LuminanceSingle = new CMPSINGLE("Luminance");

        // these are special values for rgbX types where the X value is unused and usually
        // has a value of zero, but needs to be interpreted as opaque, which is Alpha=1

        public static readonly CMPBYTE OpaqueAlphaByte = new CMPBYTE("Alpha", 255, 255);
        public static readonly CMPSHORT OpaqueAlphaShort = new CMPSHORT("Alpha", ushort.MaxValue, ushort.MaxValue);
        public static readonly CMPHALF OpaqueAlphaHalf = new CMPHALF("Alpha", Half.One, Half.One);
        public static readonly CMPSINGLE OpaqueAlphaSingle = new CMPSINGLE("Alpha", 1, 1);

        // packed formats (not supported yet)        

        public static readonly CMPSHORT PackedRgb565 = new CMPSHORT("Rgb565");
        public static readonly CMPSHORT PackedBgr565 = new CMPSHORT("Bgr565");
    }    
}

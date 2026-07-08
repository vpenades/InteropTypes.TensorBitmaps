using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace InteropTypes.TensorBitmaps
{
    /// <summary>
    /// Represents a pixel component within <see cref="TensorPixelFormat"/>
    /// </summary>
    public abstract record TensorPixelComponent
    {        
        public static TensorPixelComponent<byte> RedByte = new TensorPixelComponent<byte>("Red", 0, 255);
        public static TensorPixelComponent<byte> GreenByte = new TensorPixelComponent<byte>("Green", 0, 255);
        public static TensorPixelComponent<byte> BlueByte = new TensorPixelComponent<byte>("Blue", 0, 255);
        public static TensorPixelComponent<byte> AlphaByte = new TensorPixelComponent<byte>("Alpha", 0, 255);
        public static TensorPixelComponent<byte> PremulByte = new TensorPixelComponent<byte>("Premultiplied", 0, 255);
        public static TensorPixelComponent<byte> LuminanceByte = new TensorPixelComponent<byte>("Luminance", 0, 255);
        
        public static TensorPixelComponent<ushort> RedShort = new TensorPixelComponent<ushort>("Red", ushort.MinValue, ushort.MaxValue);
        public static TensorPixelComponent<ushort> GreenShort = new TensorPixelComponent<ushort>("Green", ushort.MinValue, ushort.MaxValue);
        public static TensorPixelComponent<ushort> BlueShort = new TensorPixelComponent<ushort>("Blue", ushort.MinValue, ushort.MaxValue);
        public static TensorPixelComponent<ushort> AlphaShort = new TensorPixelComponent<ushort>("Alpha", ushort.MinValue, ushort.MaxValue);
        public static TensorPixelComponent<ushort> PremulShort = new TensorPixelComponent<ushort>("Premultiplied", ushort.MinValue, ushort.MaxValue);
        public static TensorPixelComponent<ushort> LuminanceShort = new TensorPixelComponent<ushort>("Luminance", ushort.MinValue, ushort.MaxValue);
        
        public static TensorPixelComponent<Half> RedHalf = new TensorPixelComponent<Half>("Red", Half.Zero, Half.One);
        public static TensorPixelComponent<Half> GreenHalf = new TensorPixelComponent<Half>("Green", Half.Zero, Half.One);
        public static TensorPixelComponent<Half> BlueHalf = new TensorPixelComponent<Half>("Blue", Half.Zero, Half.One);
        public static TensorPixelComponent<Half> AlphaHalf = new TensorPixelComponent<Half>("Alpha", Half.Zero, Half.One);
        public static TensorPixelComponent<Half> PremulHalf = new TensorPixelComponent<Half>("Premultiplied", Half.Zero, Half.One);
        public static TensorPixelComponent<Half> LuminanceHalf = new TensorPixelComponent<Half>("Luminance", Half.Zero, Half.One);
        
        public static TensorPixelComponent<float> RedSingle = new TensorPixelComponent<float>("Red", 0, 1);
        public static TensorPixelComponent<float> GreenSingle = new TensorPixelComponent<float>("Green", 0, 1);
        public static TensorPixelComponent<float> BlueSingle = new TensorPixelComponent<float>("Blue", 0, 1);
        public static TensorPixelComponent<float> AlphaSingle = new TensorPixelComponent<float>("Alpha", 0, 1);
        public static TensorPixelComponent<float> PremulSingle = new TensorPixelComponent<float>("Premultiplied", 0, 1);
        public static TensorPixelComponent<float> LuminanceSingle = new TensorPixelComponent<float>("Luminance", 0, 1);

        // these are special values for rgbX types where the value assigned to alpha(X) is unused and usually zero, but still opaque

        public static TensorPixelComponent<byte> OpaqueAlphaByte = new TensorPixelComponent<byte>("Alpha", 255, 255);
        public static TensorPixelComponent<ushort> OpaqueAlphaShort = new TensorPixelComponent<ushort>("Alpha", ushort.MaxValue, ushort.MaxValue);
        public static TensorPixelComponent<Half> OpaqueAlphaHalf = new TensorPixelComponent<Half>("Alpha", Half.One, Half.One);
        public static TensorPixelComponent<float> OpaqueAlphaSingle = new TensorPixelComponent<float>("Alpha", 1, 1);

        protected TensorPixelComponent(string semantic)
        {
            Semantic = semantic;
        }

        /// <summary>
        /// Red, Green, Blue, Alpha, PremulAlpha, Luminance, etc
        /// </summary>
        public string Semantic { get; }

        /// <summary>
        /// Gets the type of the component, usually Byte or Float.
        /// </summary>
        public abstract Type ComponentType { get; }

        /// <summary>
        /// Gets the size in bytes of the component.
        /// </summary>
        public abstract int ByteSize { get; }

        public bool HasAlphaComponent => Semantic == "Alpha" || Semantic == "Premultiplied" || Semantic == "Opacity";
    }

    /// <summary>
    /// Represents a pixel component within <see cref="TensorPixelFormat"/>
    /// </summary>
    /// <typeparam name="T">The type of the pixel component, usually <see cref="byte"/> or <see cref="float"/></typeparam>
    public record TensorPixelComponent<T> : TensorPixelComponent
        where T:unmanaged, INumber<T>
    {
        public TensorPixelComponent(string semantic, T minValue, T maxValue) : base(semantic)
        {            
            MinValue = minValue;
            MaxValue = maxValue;

            DefaultValue = semantic == "Alpha" || semantic == "Premultiplied" ? maxValue : minValue;
        }        

        /// <summary>
        /// the minimum value expected to be found in this component
        /// </summary>
        /// <remarks>
        /// This is typically 0, but it can be a negative value if the pixels have been transformed by a std-mean
        /// </remarks>
        public T MinValue { get; }

        /// <summary>
        /// the maximum value expected to be found in this component
        /// </summary>
        /// <remarks>
        /// This is typically 1 or 255, but it can be a different value if the pixels have been transformed by a std-mean
        /// </remarks>
        public T MaxValue { get; }


        public T DefaultValue { get; }

        public override Type ComponentType => typeof(T);

        public override int ByteSize => Unsafe.SizeOf<T>();
    }

    
}

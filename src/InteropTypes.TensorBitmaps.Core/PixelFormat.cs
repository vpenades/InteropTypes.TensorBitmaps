using System;
using System.Collections.Generic;
using System.Linq;
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
        public static TensorPixelComponent<byte> Undefined255 = new TensorPixelComponent<byte>("Undefined", 0, 255);
        public static TensorPixelComponent<byte> Red255 = new TensorPixelComponent<byte>("Red", 0, 255);
        public static TensorPixelComponent<byte> Green255 = new TensorPixelComponent<byte>("Green", 0, 255);
        public static TensorPixelComponent<byte> Blue255 = new TensorPixelComponent<byte>("Blue", 0, 255);
        public static TensorPixelComponent<byte> Alpha255 = new TensorPixelComponent<byte>("Alpha", 0, 255);
        public static TensorPixelComponent<byte> Premul255 = new TensorPixelComponent<byte>("Premultiplied", 0, 255);
        public static TensorPixelComponent<byte> Luminance255 = new TensorPixelComponent<byte>("Luminance", 0, 255);

        public static TensorPixelComponent<float> UndefinedScalar = new TensorPixelComponent<float>("Undefined", 0, 1);
        public static TensorPixelComponent<float> RedScalar = new TensorPixelComponent<float>("Red", 0, 1);
        public static TensorPixelComponent<float> GreenScalar = new TensorPixelComponent<float>("Green", 0, 1);
        public static TensorPixelComponent<float> BlueScalar = new TensorPixelComponent<float>("Blue", 0, 1);
        public static TensorPixelComponent<float> AlphaScalar = new TensorPixelComponent<float>("Alpha", 0, 1);
        public static TensorPixelComponent<float> PremulScalar = new TensorPixelComponent<float>("Premultiplied", 0, 1);
        public static TensorPixelComponent<float> LuminanceScalar = new TensorPixelComponent<float>("Luminance", 0, 1);

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
    }

    /// <summary>
    /// Represents a pixel component within <see cref="TensorPixelFormat"/>
    /// </summary>
    /// <typeparam name="T">The type of the pixel component, usually <see cref="byte"/> or <see cref="float"/></typeparam>
    public record TensorPixelComponent<T> : TensorPixelComponent
        where T:unmanaged
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

    /// <summary>
    /// Represents a pixel format
    /// </summary>
    public record TensorPixelFormat
    {
        public static TensorPixelFormat Rgb24 = new TensorPixelFormat(TensorPixelComponent.Red255, TensorPixelComponent.Green255, TensorPixelComponent.Blue255);
        public static TensorPixelFormat Bgr24 = new TensorPixelFormat(TensorPixelComponent.Blue255, TensorPixelComponent.Green255, TensorPixelComponent.Red255);
        public static TensorPixelFormat Rgba32 = new TensorPixelFormat(TensorPixelComponent.Red255, TensorPixelComponent.Green255, TensorPixelComponent.Blue255, TensorPixelComponent.Alpha255);
        public static TensorPixelFormat Bgra32 = new TensorPixelFormat(TensorPixelComponent.Blue255, TensorPixelComponent.Green255, TensorPixelComponent.Red255, TensorPixelComponent.Alpha255);
        public static TensorPixelFormat Argb32 = new TensorPixelFormat(TensorPixelComponent.Alpha255, TensorPixelComponent.Red255, TensorPixelComponent.Green255, TensorPixelComponent.Blue255);

        public static TensorPixelFormat Rgb96f = new TensorPixelFormat(TensorPixelComponent.RedScalar, TensorPixelComponent.GreenScalar, TensorPixelComponent.BlueScalar);
        public static TensorPixelFormat Bgr96f = new TensorPixelFormat(TensorPixelComponent.BlueScalar, TensorPixelComponent.GreenScalar, TensorPixelComponent.RedScalar);
        public static TensorPixelFormat Rgba128f = new TensorPixelFormat(TensorPixelComponent.RedScalar, TensorPixelComponent.GreenScalar, TensorPixelComponent.BlueScalar, TensorPixelComponent.AlphaScalar);

        public TensorPixelFormat(IReadOnlyList<TensorPixelComponent> components)
        {
            Components = components;

            BytesPerPixel = Components.Sum(item => item.ByteSize);
        }

        public TensorPixelFormat(TensorPixelComponent x)
        {
            Components = [x];

            BytesPerPixel = Components.Sum(item => item.ByteSize);
        }

        public TensorPixelFormat(TensorPixelComponent x, TensorPixelComponent y)
        {
            Components = [x, y];

            BytesPerPixel = Components.Sum(item => item.ByteSize);
        }

        public TensorPixelFormat(TensorPixelComponent x, TensorPixelComponent y, TensorPixelComponent z)
        {
            Components = [x, y, z];

            BytesPerPixel = Components.Sum(item => item.ByteSize);
        }

        public TensorPixelFormat(TensorPixelComponent x, TensorPixelComponent y, TensorPixelComponent z, TensorPixelComponent w)
        {
            Components = [x, y, z, w];

            BytesPerPixel = Components.Sum(item => item.ByteSize);
        }

        public IReadOnlyList<TensorPixelComponent> Components { get; }

        public int BytesPerPixel { get; }

        public int IndexOf(string semantic)
        {            
            for (int i = 0; i < Components.Count; i++)
            {
                if (Components[i].Semantic.Equals(semantic, StringComparison.Ordinal)) return i;
            }

            return -1;
        }
    }    
}

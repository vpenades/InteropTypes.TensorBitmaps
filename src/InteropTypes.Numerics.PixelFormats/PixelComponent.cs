using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace InteropTypes.Numerics
{
    /// <summary>
    /// Represents a pixel component within <see cref="PixelFormat"/>
    /// </summary>
    [type: System.Diagnostics.DebuggerDisplay("{ToString(),nq}")]
    public abstract record PixelComponent
    {
        protected PixelComponent(string semantic)
        {
            Semantic = semantic;
        }

        /// <summary>
        /// Red, Green, Blue, Alpha, PremulAlpha, Luminance, etc
        /// </summary>
        public string Semantic { get; }

        /// <summary>
        /// Gets the type of the component, usually Byte UShort, Half or Float.
        /// </summary>
        public abstract Type ComponentType { get; }

        /// <summary>
        /// Gets the size in bytes of the component.
        /// </summary>
        public abstract int ByteSize { get; }

        public bool HasAlphaComponent => Semantic == "Alpha" || Semantic == "Premultiplied" || Semantic == "Opacity";

        public override string ToString()
        {
            return $"{Semantic}:{ComponentType.Name}";
        }        
    }

    /// <summary>
    /// Represents a pixel component within <see cref="PixelFormat"/>
    /// </summary>
    /// <typeparam name="T">The type of the pixel component, usually <see cref="byte"/> or <see cref="float"/></typeparam>
    [type: System.Diagnostics.DebuggerDisplay("{ToString(),nq}")]
    public sealed record PixelComponent<T> : PixelComponent
        where T:unmanaged, INumber<T>
    {
        public PixelComponent(string semantic, T minValue, T maxValue) : base(semantic)
        {            
            MinValue = minValue;
            MaxValue = maxValue;

            DefaultValue = HasAlphaComponent
                ? MaxValue
                : MinValue;
        }

        public PixelComponent(string semantic) : base(semantic)
        {
            if (typeof(T) == typeof(byte)) { MinValue = T.CreateChecked(byte.MinValue); MaxValue = T.CreateChecked(byte.MaxValue); }
            else if (typeof(T) == typeof(ushort)) { MinValue = T.CreateChecked(ushort.MinValue); MaxValue = T.CreateChecked(ushort.MaxValue); }
            else if (typeof(T) == typeof(uint)) { MinValue = T.CreateChecked(uint.MinValue); MaxValue = T.CreateChecked(uint.MaxValue); }
            else if (typeof(T) == typeof(ulong)) { MinValue = T.CreateChecked(ulong.MinValue); MaxValue = T.CreateChecked(ulong.MaxValue); }

            else if (typeof(T) == typeof(Half)) { MinValue = T.Zero; MaxValue = T.One; }
            else if (typeof(T) == typeof(Single)) { MinValue = T.Zero; MaxValue = T.One; }
            else if (typeof(T) == typeof(Double)) { MinValue = T.Zero; MaxValue = T.One; }

            else throw new InvalidOperationException("Specify min and max values in constructor");

            DefaultValue = HasAlphaComponent
                ? MaxValue
                : MinValue;
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

        /// <summary>
        /// The default value to be used for filling
        /// </summary>
        public T DefaultValue { get; }

        public override Type ComponentType => typeof(T);

        public override int ByteSize => Unsafe.SizeOf<T>();        
    }

    
}



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
    public sealed record TensorPixelComponent<T> : TensorPixelComponent
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

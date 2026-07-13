using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Transactions;

namespace InteropTypes.Numerics
{
    /// <summary>
    /// Represents a pixel component within <see cref="PixelFormat"/>
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("{ToString(),nq}")]    
    public abstract class PixelComponent : IEquatable<PixelComponent>
    {
        protected PixelComponent(string semantic)
        {
            this.Semantic = semantic;
        }

        /// <summary>
        /// Red, Green, Blue, Alpha, PremulAlpha, Luminance, etc
        /// </summary>
        public string Semantic { get; }

        public override int GetHashCode()
        {
            return Semantic == null
                ? 0
                : Semantic.GetHashCode(StringComparison.Ordinal);
        }

        public override bool Equals(object obj) => obj is PixelComponent other && this.Equals(other);

        public virtual bool Equals(PixelComponent other)
        {
            if (Object.ReferenceEquals(this, other)) return true;
            if (Object.ReferenceEquals(other, null)) return false;

            return String.Equals(this.Semantic, other.Semantic, StringComparison.Ordinal);
        }

        public static bool AreEqual(PixelComponent left, PixelComponent right)
        {
            if (Object.ReferenceEquals(left, null)) return false;
            return left.Equals(right);
        }

        public static bool operator ==(PixelComponent left, PixelComponent right) => AreEqual(left, right);
        public static bool operator !=(PixelComponent left, PixelComponent right) => !AreEqual(left, right);

        public static int GetHashCodeFrom(IEnumerable<PixelComponent> components)
        {
            return components.Aggregate(0, (h,c) => (h*17) ^ c.GetHashCode());
        }

        public static bool AreStructurallyEqual(IReadOnlyList<PixelComponent> left, IReadOnlyList<PixelComponent> right)
        {
            if (left.Count != right.Count) return false;
            return left.SequenceEqual(right);
        }

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
    [System.Diagnostics.DebuggerDisplay("{ToString(),nq}")]    
    public sealed class PixelComponent<T> : PixelComponent, IEquatable<PixelComponent<T>>
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

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), MinValue, MaxValue, DefaultValue);
        }

        public override bool Equals(object obj) => obj is PixelComponent<T> other && this.Equals(other);
        public override bool Equals(PixelComponent obj) => obj is PixelComponent<T> other && this.Equals(other);

        public bool Equals(PixelComponent<T> other)
        {
            if (Object.ReferenceEquals(this, other)) return true;

            if (!base.Equals(other)) return false;
            if (!this.MinValue.Equals(other.MinValue)) return false;
            if (!this.MaxValue.Equals(other.MaxValue)) return false;
            if (!this.DefaultValue.Equals(other.DefaultValue)) return false;
            return true;
        }

        public static bool AreEqual(PixelComponent<T> left, PixelComponent<T> right)
        {
            if (Object.ReferenceEquals(left, null)) return false;
            return left.Equals(right);
        }

        public static bool operator ==(PixelComponent<T> left, PixelComponent<T> right) => AreEqual(left, right);
        public static bool operator !=(PixelComponent<T> left, PixelComponent<T> right) => !AreEqual(left, right);

        public override Type ComponentType => typeof(T);

        public override int ByteSize => Unsafe.SizeOf<T>();
    }    
}



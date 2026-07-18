using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace InteropTypes.TensorBitmaps.Operands
{
    /// <summary>
    /// Represents an ByRef read only bitmap
    /// </summary>
    /// <typeparam name="TSelf">The type of the class or structure implementing this interface</typeparam>
    /// <typeparam name="TPixel">The pixel type. It can be anything as long as it has the same ByteSize declared by <see cref="Format"/> </typeparam>
    public interface IReadOnlyBitmapOperand<TSelf, TPixel> : IReadOnlyBitmap<TPixel>
        where TSelf : IReadOnlyBitmapOperand<TSelf, TPixel>, allows ref struct
        where TPixel : unmanaged        
    {
        public bool TryCastTo<T>(out T managedBitmap)
            where T: IReadOnlyBitmap<TPixel>
        {
            managedBitmap = default;
            return false;
        }

        /// <summary>
        /// Gets a cropped view of the bitmap.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The returned bitmap must reference the pixels of the source bitmap.
        /// So any modification to the cropped bitmap is reflected into the source bitmap.
        /// </para>
        /// <para>
        /// The method should do: <c>rectangle.Intersect(new System.Drawing.Rectangle(0, 0, Width, Height));</c><br/>
        /// So the returned bitmap may be smalled than the requested region if they partially intersect.
        /// </para>
        /// </remarks>
        /// <param name="rect">The region to crop</param>
        /// <returns>A cropped bitmap</returns>
        TSelf GetCropped(System.Drawing.Rectangle rectangle);        
    }

    /// <summary>
    /// Represents ByRef bitmap
    /// </summary>
    /// <typeparam name="TSelf">The type of the class or structure implementing this interface</typeparam>
    /// <typeparam name="TPixel">The pixel type. It can be anything as long as it has the same ByteSize declared by <see cref="Format"/> </typeparam>
    public interface IBitmapOperand<TSelf, TPixel>
        : IReadOnlyBitmapOperand<TSelf, TPixel>
        , IBitmap<TPixel>
        where TSelf : IBitmapOperand<TSelf, TPixel>, allows ref struct
        where TPixel : unmanaged        
    {
        /// <summary>
        /// Returns a context that can be used to perform bulk operations on this bitmap
        /// </summary>
        /// <typeparam name="TContextPixel">The pixel format to be used in the operations of the context.</typeparam>
        /// <returns>It must return: <c>new Operators.BinaryOperatorContext<TSelf, TPixel, TSrcPixel>(this);</c> </returns>
        public Operators.BinaryOperatorContext<TSelf, TPixel, TContextPixel> GetContext<TContextPixel>()
            where TContextPixel : unmanaged;        
    }    
}

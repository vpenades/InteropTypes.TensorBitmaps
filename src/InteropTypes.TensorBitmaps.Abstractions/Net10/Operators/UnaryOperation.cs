using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using InteropTypes.TensorBitmaps.Operands;

namespace InteropTypes.TensorBitmaps.Operators
{
    public interface IReadOnlyUnaryOperation<TPixel, TResult>
        where TPixel : unmanaged
    {
        TResult Execute<TBmp>(TBmp dst) where TBmp : IReadOnlyBitmap<TBmp, TPixel>;
    }

    public interface IUnaryOperation<TPixel, TResult>
        where TPixel : unmanaged        
    {
        TResult Execute<TBmp>(TBmp dst) where TBmp : IBitmap<TBmp, TPixel>;
    }
}

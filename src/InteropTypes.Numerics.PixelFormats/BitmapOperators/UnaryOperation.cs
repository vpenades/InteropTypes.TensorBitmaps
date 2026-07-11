using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace InteropTypes.Numerics.BitmapOperators
{
    public interface IUnaryOperation<TPixel>        
        where TPixel : unmanaged        
    {
        void Execute<TBmp>(TBmp dst) where TBmp : IBitmapOperand<TBmp, TPixel>;
    }
}

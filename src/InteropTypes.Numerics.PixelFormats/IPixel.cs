using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace InteropTypes.Numerics
{
    public interface IPixel<TSelf>
        : IPixelFormatSource
        //: IEqualityOperators<TSelf, TSelf, bool>
        , IAdditionOperators<TSelf, TSelf, TSelf>
        , IMultiplyOperators<TSelf, float, TSelf>     
        
        where TSelf : unmanaged, IPixel<TSelf>        
    {
        static abstract TSelf Zero { get; }

        TSelf Saturated();

        bool IsNaN();
    }
}

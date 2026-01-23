using System;
using System.Collections.Generic;

namespace TongbaoExchangeCalc.DataModel
{
    public interface ITongbaoSelector : ICloneable
    {
        int SelectTongbao(IReadOnlyList<int> tongbaoIds);
    }
}

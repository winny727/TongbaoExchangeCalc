using System;
using System.Collections.Generic;

namespace TongbaoExchangeCalc.DataModel
{
    public interface ITongbaoSelector
    {
        int SelectTongbao(IReadOnlyList<int> tongbaoIds);
    }
}

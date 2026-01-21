using System;
using System.Collections.Generic;

namespace TongbaoExchangeCalc.DataModel
{
    public interface IReadOnlyPlayerData
    {
        SquadType SquadType { get; }
        int ExchangeCount { get; }
        int MaxTongbaoCount { get; }
        int NextExchangeCostLifePoint { get; }
        IReadOnlyDictionary<ResType, int> ResValues { get; }

        Tongbao GetTongbao(int slotIndex);
        bool IsTongbaoLocked(int id);
        int GetResValue(ResType type);
        bool HasSpecialCondition(SpecialConditionFlag specialCondition);
    }
}

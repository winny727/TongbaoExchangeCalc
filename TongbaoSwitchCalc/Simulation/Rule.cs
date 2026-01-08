using System;
using System.Collections.Generic;
using TongbaoSwitchCalc.DataModel;

namespace TongbaoSwitchCalc
{
    public abstract class Rule
    {
        public bool Enabled { get; set; } = true;
        public abstract void ExecuteRule(PlayerData playerData);
    }

    public class TongbaoPickRule : Rule
    {
        public int TargetPosIndex { get; private set; }

        public TongbaoPickRule(int targetPosIndex)
        {
            TargetPosIndex = targetPosIndex;
        }

        public override void ExecuteRule(PlayerData playerData)
        {
            throw new NotImplementedException();
        }
    }

    public class AutoStopRule : Rule
    {
        public int TargetTongbaoId { get; private set; }

        public AutoStopRule(int targetTongbaoId)
        {
            TargetTongbaoId = targetTongbaoId;
        }

        public override void ExecuteRule(PlayerData playerData)
        {
            throw new NotImplementedException();
        }
    }
}

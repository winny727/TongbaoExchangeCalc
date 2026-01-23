using System;
using System.Collections.Generic;

namespace TongbaoExchangeCalc.DataModel.Simulation
{
    public class SimulationOptions
    {
        public SimulationType SimulationType;
        public int TotalSimulationCount;
        public int MinimumLifePoint;
        public int ExchangeSlotIndex;
        public ISimulationRuleController RuleController;
        public bool UseMultiThreadOptimize;
    }
}

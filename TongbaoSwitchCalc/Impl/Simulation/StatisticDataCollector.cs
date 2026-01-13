using System;
using System.Collections.Generic;
using TongbaoSwitchCalc.DataModel;
using TongbaoSwitchCalc.DataModel.Simulation;

namespace TongbaoSwitchCalc.Impl.Simulation
{
    public class StatisticDataCollector : IDataCollector<SimulateContext>
    {
        public List<SwitchRecord> SwitchRecords { get; private set; } = new List<SwitchRecord>();

        public void OnSimulateBegin(SimulationType type, int totalSimCount, in IReadOnlyPlayerData playerData)
        {

        }

        public void OnSimulateEnd(int executedSimCount, float simCostTimeMS, in IReadOnlyPlayerData playerData)
        {

        }

        public void OnSimulateStepBegin(in SimulateContext context)
        {

        }

        public void OnSimulateStepEnd(in SimulateContext context, SimulateStepResult result)
        {

        }

        public void OnSwitchStepBegin(in SimulateContext context)
        {

        }

        public void OnSwitchStepEnd(in SimulateContext context, SwitchStepResult result)
        {

        }

        public void ClearData()
        {
            SwitchRecords.Clear();
        }
    }
}

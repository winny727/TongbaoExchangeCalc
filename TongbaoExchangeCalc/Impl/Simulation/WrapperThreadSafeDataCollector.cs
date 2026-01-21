using System;
using TongbaoExchangeCalc.DataModel;
using TongbaoExchangeCalc.DataModel.Simulation;

namespace TongbaoExchangeCalc.Impl.Simulation
{
    public class WrapperThreadSafeDataCollector : IThreadSafeDataCollector<SimulateContext>
    {
        private readonly IDataCollector<SimulateContext> mInner;
        private readonly object mLock = new object();

        public WrapperThreadSafeDataCollector(IDataCollector<SimulateContext> inner)
        {
            mInner = inner ?? throw new ArgumentNullException(nameof(inner));
        }

        public void OnSimulateBegin(SimulationType type, int totalSimStep, in IReadOnlyPlayerData playerData)
        {
            lock (mLock)
            {
                mInner.OnSimulateBegin(type, totalSimStep, playerData);
            }
        }

        public void OnSimulateEnd(int executedSimStep, float simCostTimeMS, in IReadOnlyPlayerData playerData)
        {
            lock (mLock)
            {
                mInner.OnSimulateEnd(executedSimStep, simCostTimeMS, playerData);
            }
        }

        public void OnSimulateParallel(int estimatedLeftExchangeStep, int curSimStep)
        {
            lock (mLock)
            {
                mInner.OnSimulateParallel(estimatedLeftExchangeStep, curSimStep);
            }
        }

        public void OnSimulateStepBegin(in SimulateContext context)
        {
            lock (mLock)
            {
                mInner.OnSimulateStepBegin(context);
            }
        }

        public void OnSimulateStepEnd(in SimulateContext context, SimulateStepResult result)
        {
            lock (mLock)
            {
                mInner.OnSimulateStepEnd(context, result);
            }
        }

        public void OnExchangeStepBegin(in SimulateContext context)
        {
            lock (mLock)
            {
                mInner.OnExchangeStepBegin(context);
            }
        }

        public void OnExchangeStepEnd(in SimulateContext context, ExchangeStepResult result)
        {
            lock (mLock)
            {
                mInner.OnExchangeStepEnd(context, result);
            }
        }

        public void ClearData()
        {
            lock (mLock)
            {
                mInner.ClearData();
            }
        }
    }
}

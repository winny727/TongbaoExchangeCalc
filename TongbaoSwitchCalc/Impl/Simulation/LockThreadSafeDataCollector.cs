using System;
using System.Collections.Generic;
using TongbaoExchangeCalc.DataModel;
using TongbaoExchangeCalc.DataModel.Simulation;

namespace TongbaoExchangeCalc.Impl.Simulation
{
    public class LockThreadSafeDataCollector : DataCollector, IThreadSafeDataCollector<SimulateContext>
    {
        private readonly object mTongbaoRecordsLock = new object();
        private readonly object mResValueRecordsLock = new object();
        private readonly object mExchangeStepResultsLock = new object();
        private readonly object mSimulateStepResultsLock = new object();

        public override void OnSimulateStepEnd(in SimulateContext context, SimulateStepResult result)
        {
            lock (mSimulateStepResultsLock)
            {
                mSimulateStepResults[context.SimulationStepIndex] = result;
            }
        }

        public override void OnExchangeStepBegin(in SimulateContext context)
        {
            if (!RecordEachExchange)
            {
                return;
            }

            var indexes = new StepIndexes
            {
                SimulateStepIndex = context.SimulationStepIndex,
                ExchangeStepIndex = context.ExchangeStepIndex,
            };

            Tongbao tongbao = context.PlayerData.GetTongbao(context.SlotIndex);
            int tongbaoId = tongbao != null ? tongbao.Id : -1;

            lock (mTongbaoRecordsLock)
            {
                mTongbaoRecords[indexes] = new TongbaoRecordValue
                {
                    SlotIndex = context.SlotIndex,
                    BeforeId = tongbaoId,
                    AfterId = tongbaoId,
                };
            }

            foreach (var item in context.PlayerData.ResValues)
            {
                var key = new ResRecordKey
                {
                    Indexes = indexes,
                    ResType = item.Key,
                };

                lock (mResValueRecordsLock)
                {
                    mResValueRecords[key] = new ResRecordValue
                    {
                        BeforeValue = item.Value,
                        AfterValue = item.Value,
                    };
                }
            }
        }

        public override void OnExchangeStepEnd(in SimulateContext context, ExchangeStepResult result)
        {
            if (!RecordEachExchange)
            {
                return;
            }

            var indexes = new StepIndexes
            {
                SimulateStepIndex = context.SimulationStepIndex,
                ExchangeStepIndex = context.ExchangeStepIndex,
            };

            lock (mExchangeStepResultsLock)
            {
                mExchangeStepResults[indexes] = result;
            }

            Tongbao tongbao = context.PlayerData.GetTongbao(context.SlotIndex);
            int tongbaoId = tongbao != null ? tongbao.Id : -1;

            lock (mTongbaoRecordsLock)
            {
                int beforeId = -1;
                if (mTongbaoRecords.TryGetValue(indexes, out var record))
                {
                    beforeId = record.BeforeId;
                }

                mTongbaoRecords[indexes] = new TongbaoRecordValue
                {
                    SlotIndex = context.SlotIndex,
                    BeforeId = beforeId,
                    AfterId = tongbaoId,
                };
            }

            lock (mResValueRecordsLock)
            {
                foreach (var item in context.PlayerData.ResValues)
                {
                    var key = new ResRecordKey
                    {
                        Indexes = indexes,
                        ResType = item.Key,
                    };

                    int beforeValue = 0;
                    if (mResValueRecords.TryGetValue(key, out var resRecord))
                    {
                        beforeValue = resRecord.BeforeValue;
                    }

                    mResValueRecords[key] = new ResRecordValue
                    {
                        BeforeValue = beforeValue,
                        AfterValue = item.Value,
                    };
                }
            }
        }

        public override void ClearData()
        {
            TotalSimulateStep = 0;
            ExecSimulateStep = 0;
            TotalSimulateTime = 0;
            EstimatedExchangeStep = 0;
            lock (mTongbaoRecordsLock)
            {
                mTongbaoRecords.Clear();
            }
            lock (mResValueRecordsLock)
            {
                mResValueRecords.Clear();
            }
            lock (mSimulateStepResultsLock)
            {
                mSimulateStepResults.Clear();
            }
            lock (mExchangeStepResultsLock)
            {
                mExchangeStepResults.Clear();
            }
        }
    }
}

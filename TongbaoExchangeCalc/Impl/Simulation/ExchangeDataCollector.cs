using System;
using System.Collections.Generic;
using TongbaoExchangeCalc.DataModel;
using TongbaoExchangeCalc.DataModel.Simulation;

namespace TongbaoExchangeCalc.Impl.Simulation
{
    public class ExchangeDataCollector : IDataCollector<SimulateContext>
    {
        public bool RecordEachExchange { get; set; } = true;
        public SimulationType SimulationType { get; protected set; }
        public int TotalSimulateStep { get; protected set; }
        public int ExecSimulateStep { get; protected set; }
        public float TotalSimulateTime { get; protected set; }
        public int TotalExchangeStep => mExchangeStepResults.Count;
        public int EstimatedExchangeStep { get; protected set; }

        // TODO capacity
        protected readonly Dictionary<StepIndexes, TongbaoRecordValue> mTongbaoRecords = new Dictionary<StepIndexes, TongbaoRecordValue>();
        protected readonly Dictionary<ResRecordKey, ResRecordValue> mResValueRecords = new Dictionary<ResRecordKey, ResRecordValue>();
        protected readonly Dictionary<StepIndexes, ExchangeStepResult> mExchangeStepResults = new Dictionary<StepIndexes, ExchangeStepResult>();
        protected readonly Dictionary<int, SimulateStepResult> mSimulateStepResults = new Dictionary<int, SimulateStepResult>();


        public IReadOnlyDictionary<StepIndexes, TongbaoRecordValue> TongbaoRecords => mTongbaoRecords;
        public IReadOnlyDictionary<ResRecordKey, ResRecordValue> ResValueRecords => mResValueRecords;
        public IReadOnlyDictionary<StepIndexes, ExchangeStepResult> ExchangeStepResults => mExchangeStepResults;
        public IReadOnlyDictionary<int, SimulateStepResult> SimulateStepResults => mSimulateStepResults;

        public virtual void OnSimulateBegin(SimulationType type, int totalSimStep, in IReadOnlyPlayerData playerData)
        {
            ClearData();
            SimulationType = type;
            TotalSimulateStep = totalSimStep;
        }

        public virtual void OnSimulateEnd(int executedSimStep, float simCostTimeMS, in IReadOnlyPlayerData playerData)
        {
            ExecSimulateStep = executedSimStep;
            TotalSimulateTime = simCostTimeMS;
        }

        public virtual void OnSimulateParallel(int estimatedLeftExchangeStep, int remainSimStep)
        {
            EstimatedExchangeStep = estimatedLeftExchangeStep;
        }

        public virtual void OnSimulateStepBegin(in SimulateContext context)
        {

        }

        public virtual void OnSimulateStepEnd(in SimulateContext context, SimulateStepResult result)
        {
            mSimulateStepResults[context.SimulationStepIndex] = result;
        }

        public virtual void OnExchangeStepBegin(in SimulateContext context)
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

            mTongbaoRecords[indexes] = new TongbaoRecordValue
            {
                SlotIndex = context.SlotIndex,
                BeforeId = tongbaoId,
                AfterId = tongbaoId,
            };

            foreach (var item in context.PlayerData.ResValues)
            {
                var key = new ResRecordKey
                {
                    Indexes = indexes,
                    ResType = item.Key,
                };

                mResValueRecords[key] = new ResRecordValue
                {
                    BeforeValue = item.Value,
                    AfterValue = item.Value,
                };
            }
        }

        public virtual void OnExchangeStepEnd(in SimulateContext context, ExchangeStepResult result)
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

            mExchangeStepResults[indexes] = result;

            Tongbao tongbao = context.PlayerData.GetTongbao(context.SlotIndex);
            int tongbaoId = tongbao != null ? tongbao.Id : -1;

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

        public virtual IDataCollector<SimulateContext> CloneAsEmpty()
        {
            var collector = new ExchangeDataCollector
            {
                RecordEachExchange = RecordEachExchange,
                SimulationType = SimulationType,
                TotalSimulateStep = TotalSimulateStep,
            };
            return collector;
        }

        public virtual void MergeData(IDataCollector<SimulateContext> other)
        {
            if (other is ExchangeDataCollector collector)
            {
                foreach (var item in collector.mTongbaoRecords)
                {
                    mTongbaoRecords.Add(item.Key, item.Value);
                }
                foreach (var item in collector.mResValueRecords)
                {
                    mResValueRecords.Add(item.Key, item.Value);
                }
                foreach (var item in collector.mExchangeStepResults)
                {
                    mExchangeStepResults.Add(item.Key, item.Value);
                }
                foreach (var item in collector.mSimulateStepResults)
                {
                    mSimulateStepResults.Add(item.Key, item.Value);
                }
            }
        }

        public virtual void ClearData()
        {
            TotalSimulateStep = 0;
            ExecSimulateStep = 0;
            TotalSimulateTime = 0;
            EstimatedExchangeStep = 0;
            mTongbaoRecords.Clear();
            mResValueRecords.Clear();
            mSimulateStepResults.Clear();
            mExchangeStepResults.Clear();
        }
    }
}

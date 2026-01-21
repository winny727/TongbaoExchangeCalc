using System;
using System.Collections.Generic;
using System.Text;
using TongbaoExchangeCalc.DataModel;
using TongbaoExchangeCalc.DataModel.Simulation;

namespace TongbaoExchangeCalc.Impl.Simulation
{
    public class PrintDataCollector : IDataCollector<SimulateContext>
    {
        public bool RecordEachExchange { get; set; } = true;
        public bool OmitExcessiveExchanges { get; set; } = true; // 省略过多的交换信息

        private SimulationType mSimulationType;
        private int mTotalSimulateStep;
        private readonly Dictionary<int, string> mTempBeforeTongbaoName = new Dictionary<int, string>();
        private readonly Dictionary<int, Dictionary<ResType, int>> mTempResBefore = new Dictionary<int, Dictionary<ResType, int>>(); // key: simulationStepIndex

        private const int OMIITED_EXCHANGE_INDEX = 2000; // 交换次数过多则省略

        private readonly Stack<Dictionary<ResType, int>> mResDictPool = new Stack<Dictionary<ResType, int>>();

        // 避免大量模拟时字符串拼接导致频繁GC，用StringBuilder
        private readonly StringBuilder mExchangeResultSB = new StringBuilder();
        private readonly StringBuilder mResChangedTempSB = new StringBuilder();
        public string LastExchangeResult => mExchangeResultSB.ToString();

        private readonly StringBuilder mOutputResult = new StringBuilder();
        public string OutputResult => mOutputResult.ToString();

        private Dictionary<ResType, int> AllocateResDict()
        {
            if (mResDictPool.Count > 0)
            {
                return mResDictPool.Pop();
            }
            return new Dictionary<ResType, int>();
        }

        private void RecycleResDict(Dictionary<ResType, int> resDict)
        {
            if (resDict != null && !mResDictPool.Contains(resDict))
            {
                resDict.Clear();
                mResDictPool.Push(resDict);
            }
        }

        //因为外部可以单步调用OnExchangeStepBegin/OnExchangeStepEnd，这里提供个接口初始化
        public void InitSimulateStep(int simulationStepIndex)
        {
            if (!mTempResBefore.ContainsKey(simulationStepIndex))
            {
                mTempResBefore.Add(simulationStepIndex, AllocateResDict());
            }
        }

        public void OnSimulateBegin(SimulationType type, int totalSimStep, in IReadOnlyPlayerData playerData)
        {
            ClearData();
            mSimulationType = type;
            mTotalSimulateStep = totalSimStep;
            mOutputResult.Clear();
            mOutputResult.Append('[')
                         .Append(SimulationDefine.GetSimulationName(type))
                         .Append("]模拟开始，共计")
                         .Append(totalSimStep)
                         .AppendLine("次模拟");
        }

        public void OnSimulateEnd(int executedSimStep, float simCostTimeMS, in IReadOnlyPlayerData playerData)
        {
            mOutputResult.Append('[')
                         .Append(SimulationDefine.GetSimulationName(mSimulationType))
                         .Append("]模拟完成，模拟次数: ")
                         .Append(executedSimStep)
                         .Append(", 模拟耗时: ")
                         .Append(simCostTimeMS)
                         .AppendLine("ms");
        }

        public void OnSimulateParallel(int estimatedLeftExchangeStep, int curSimStep)
        {
            mOutputResult.Append("预计剩余交换次数过多(")
                         .Append(estimatedLeftExchangeStep)
                         .AppendLine(")，触发多线程优化");
        }

        public void OnSimulateStepBegin(in SimulateContext context)
        {
            mTempResBefore.Add(context.SimulationStepIndex, AllocateResDict());
            if (!RecordEachExchange)
            {
                RecordCurrentExchange(context);
            }

            mOutputResult.Append("========第")
                         .Append(context.SimulationStepIndex + 1)
                         .Append('/')
                         .Append(mTotalSimulateStep)
                         .AppendLine("次模拟开始========");
        }

        public void OnSimulateStepEnd(in SimulateContext context, SimulateStepResult result)
        {
            mExchangeResultSB.Clear();
            mResChangedTempSB.Clear();

            if (!RecordEachExchange)
            {
                GenerateResChangedString(context);
                mOutputResult.Append('(')
                             .Append(context.SimulationStepIndex + 1)
                             .Append("|");

                if (context.ExchangeStepIndex > 1)
                {
                    mOutputResult.Append("1-");
                }

                mOutputResult.Append(context.ExchangeStepIndex + 1)
                             .Append(") ")
                             .Append("总共经过了")
                             .Append(context.ExchangeStepIndex + 1)
                             .AppendLine("次交换");

                if (mResChangedTempSB.Length > 0)
                {
                    mOutputResult.Append('(')
                             .Append(mResChangedTempSB)
                             .Append(')')
                             .AppendLine();
                }
            }
            else if (OmitExcessiveExchanges && context.ExchangeStepIndex > OMIITED_EXCHANGE_INDEX)
            {
                GenerateResChangedString(context);
                mOutputResult.Append('(')
                             .Append(context.SimulationStepIndex + 1)
                             .Append('|');

                if (OMIITED_EXCHANGE_INDEX != context.ExchangeStepIndex)
                {
                    mOutputResult.Append(OMIITED_EXCHANGE_INDEX + 1)
                             .Append('-')
                             .Append(context.ExchangeStepIndex + 1);
                }
                else
                {
                    mOutputResult.Append(context.ExchangeStepIndex + 1);
                }

                mOutputResult.Append(") ")
                             .Append("交换次数过多，省略了")
                             .Append(context.ExchangeStepIndex - OMIITED_EXCHANGE_INDEX + 1)
                             .Append("次交换信息");

                if (mResChangedTempSB.Length > 0)
                {
                    mOutputResult.Append(" (")
                             .Append(mResChangedTempSB)
                             .Append(')')
                             .AppendLine();
                }
            }

            string breakReason = SimulationDefine.GetSimulateStepEndReason(result);
            mOutputResult.Append("模拟结束，结束原因: ")
                         .Append(breakReason)
                         .AppendLine()
                         .Append("========第")
                         .Append(context.SimulationStepIndex + 1)
                         .Append('/')
                         .Append(mTotalSimulateStep)
                         .AppendLine("次模拟结束========");

            RecycleResDict(mTempResBefore[context.SimulationStepIndex]);
            mTempResBefore.Remove(context.SimulationStepIndex);
            mTempBeforeTongbaoName.Remove(context.SimulationStepIndex);
        }

        public void OnExchangeStepBegin(in SimulateContext context)
        {
            if (!RecordEachExchange)
            {
                return;
            }

            if (OmitExcessiveExchanges && context.ExchangeStepIndex >= OMIITED_EXCHANGE_INDEX + 1) // 多记录一次，用于最终计算差值
            {
                return;
            }

            RecordCurrentExchange(context);
        }

        public void OnExchangeStepEnd(in SimulateContext context, ExchangeStepResult result)
        {
            if (!RecordEachExchange)
            {
                return;
            }

            if (OmitExcessiveExchanges && context.ExchangeStepIndex >= OMIITED_EXCHANGE_INDEX)
            {
                return;
            }

            mExchangeResultSB.Clear();
            mResChangedTempSB.Clear();

            mTempBeforeTongbaoName.TryGetValue(context.SimulationStepIndex, out var beforeTongbaoName);

            if (result == ExchangeStepResult.Success)
            {
                Tongbao afterTongbao = context.PlayerData.GetTongbao(context.SlotIndex);
                GenerateResChangedString(context);

                mExchangeResultSB.Append("将位置[")
                               .Append(context.SlotIndex + 1)
                               .Append("]上的[")
                               .Append(beforeTongbaoName)
                               .Append("]交换为[")
                               .Append(afterTongbao.Name)
                               .Append("] (")
                               .Append(mResChangedTempSB)
                               .Append(')');
            }
            else
            {
                switch (result)
                {
                    case ExchangeStepResult.SelectedEmpty:
                        mExchangeResultSB.Append("交换失败，选中的位置[")
                                       .Append(context.SlotIndex + 1)
                                       .Append("]上的通宝为空");
                        break;
                    case ExchangeStepResult.TongbaoUnexchangeable:
                        mExchangeResultSB.Append("交换失败，通宝[")
                                       .Append(beforeTongbaoName)
                                       .Append("]不可交换");
                        break;
                    case ExchangeStepResult.LifePointNotEnough:
                        mExchangeResultSB.Append("交换失败，交换所需生命值不足");
                        break;
                    case ExchangeStepResult.ExchangeableTongbaoNotExist:
                        mExchangeResultSB.Append("交换失败，通宝[")
                                       .Append(beforeTongbaoName)
                                       .Append("]无可交换通宝");
                        break;
                    case ExchangeStepResult.UnknownError:
                        mExchangeResultSB.Append("交换失败，未知错误");
                        break;
                    default:
                        break;
                }
            }

            mOutputResult.Append('(')
                         .Append(context.SimulationStepIndex + 1)
                         .Append('|')
                         .Append(context.ExchangeStepIndex + 1)
                         .Append(") ")
                         .Append(LastExchangeResult)
                         .AppendLine();
        }

        private void RecordCurrentExchange(in SimulateContext context)
        {
            Tongbao beforeTongbao = context.PlayerData.GetTongbao(context.SlotIndex);
            mTempBeforeTongbaoName[context.SimulationStepIndex] = beforeTongbao?.Name;

            var resDict = mTempResBefore[context.SimulationStepIndex];
            resDict.Clear();
            foreach (var item in context.PlayerData.ResValues)
            {
                resDict.Add(item.Key, item.Value);
            }
        }

        private void GenerateResChangedString(in SimulateContext context)
        {
            // PlayerData的项只增加不删除，所以这里不需要考虑并集
            foreach (var item in context.PlayerData.ResValues)
            {
                ResType type = item.Key;
                mTempResBefore[context.SimulationStepIndex].TryGetValue(type, out int beforeValue);
                int afterValue = item.Value;
                int changedValue = afterValue - beforeValue;
                if (beforeValue != afterValue)
                {
                    if (mResChangedTempSB.Length > 0)
                    {
                        mResChangedTempSB.Append("，");
                    }
                    mResChangedTempSB.Append(Define.GetResName(type));

                    if (changedValue > 0)
                    {
                        mResChangedTempSB.Append('+');
                    }
                    mResChangedTempSB.Append(changedValue);

                    mResChangedTempSB.Append(": ")
                                     .Append(beforeValue)
                                     .Append("->")
                                     .Append(afterValue);
                }
            }
        }

        public void ClearData()
        {
            mTempBeforeTongbaoName.Clear();
            mExchangeResultSB.Clear();
            mResChangedTempSB.Clear();
            mOutputResult.Clear();

            foreach (var item in mTempResBefore)
            {
                RecycleResDict(item.Value);
            }
            mTempResBefore.Clear();
        }
    }
}

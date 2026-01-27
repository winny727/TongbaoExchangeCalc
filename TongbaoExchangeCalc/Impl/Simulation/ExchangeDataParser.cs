using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TongbaoExchangeCalc.DataModel;
using TongbaoExchangeCalc.DataModel.Simulation;

namespace TongbaoExchangeCalc.Impl.Simulation
{
    public class ExchangeDataParser
    {
        private readonly ExchangeDataCollector mExchangeDataCollector;
        private CancellationTokenSource mCancellationTokenSource;
        private IProgress<int> mAsyncProgress;

        public bool IsAsyncBuilding => mCancellationTokenSource != null;

        public StringBuilder OutputResultSB { get; private set; } = new StringBuilder();
        public string OutputResult => OutputResultSB.ToString();
        private int mLastSimulationStepIndex = -1;

        public ExchangeDataParser(ExchangeDataCollector collector)
        {
            mExchangeDataCollector = collector ?? throw new ArgumentNullException(nameof(collector));
        }

        public async Task BuildOutputResultAsync(IProgress<int> progress = null)
        {
            mCancellationTokenSource = new CancellationTokenSource();
            mAsyncProgress = progress;
            try
            {
                using CodeTimer ct = new CodeTimer("BuildOutputResult");
                await Task.Run(BuildOutputResult, mCancellationTokenSource.Token);
            }
            finally
            {
                OutputResultSB.AppendLine("交换结果未构建完成: 用户取消");
                mCancellationTokenSource?.Dispose();
                mCancellationTokenSource = null;
                mAsyncProgress = null;
            }
        }

        public void CancelBuild()
        {
            mCancellationTokenSource?.Cancel();
        }

        public void BuildOutputResult()
        {
            mLastSimulationStepIndex = -1;
            var sb = OutputResultSB;
            var collector = mExchangeDataCollector;

            sb.Clear();
            sb.Append('[')
              .Append(SimulationDefine.GetSimulationName(collector.SimulationType))
              .Append("]模拟开始，共计")
              .Append(collector.TotalSimulateStep)
              .AppendLine("次模拟");

            mAsyncProgress?.Report(0);
            collector.ForEachExchangeRecords(ExchangeRecordCallback);
            AppendSimulateStepEnd(mLastSimulationStepIndex);
            mAsyncProgress?.Report(mLastSimulationStepIndex + 1); // Count = Index + 1

            sb.Append('[')
              .Append(SimulationDefine.GetSimulationName(collector.SimulationType))
              .Append("]模拟完成，模拟次数: ")
              .Append(collector.ExecSimulateStep)
              .Append(", 模拟耗时: ")
              .Append(collector.TotalSimulateTime)
              .AppendLine("ms");
        }

        public void Clear()
        {
            mLastSimulationStepIndex = -1;
            OutputResultSB.Clear();
        }

        private bool ExchangeRecordCallback(ExchangeRecord record)
        {
            var sb = OutputResultSB;
            var collector = mExchangeDataCollector;

            if (mCancellationTokenSource != null && mCancellationTokenSource.IsCancellationRequested)
            {
                return false;
            }

            if (mLastSimulationStepIndex != record.SimulationStepIndex)
            {
                if (record.SimulationStepIndex > 0)
                {
                    AppendSimulateStepEnd(mLastSimulationStepIndex);
                }

                AppendSimulateStepBegin(record.SimulationStepIndex);
                mLastSimulationStepIndex = record.SimulationStepIndex;
                mAsyncProgress?.Report(record.SimulationStepIndex);
            }

            if (!collector.RecordEachExchange)
            {
                AppendSkippedExchangeStep(record);
                return true;
            }

            if (collector.OmitExcessiveExchanges && record.ExchangeStepIndex >= collector.MaxExchangeRecord)
            {
                AppendSkippedExchangeStep(record);
                return true;
            }

            AppendExchangeStep(record);

            return true;
        }

        private void AppendSimulateStepBegin(int simulationStepIndex)
        {
            if (simulationStepIndex < 0)
            {
                return;
            }

            var sb = OutputResultSB;
            var collector = mExchangeDataCollector;

            if (simulationStepIndex == collector.SwitchParallelSimStepIndex)
            {
                sb.Append("预计剩余交换次数过多(")
                  .Append(collector.EstimatedExchangeStep)
                  .AppendLine(")，触发多线程优化");
            }

            sb.Append("========第")
              .Append(simulationStepIndex + 1)
              .Append('/')
              .Append(collector.TotalSimulateStep)
              .AppendLine("次模拟开始========");
        }

        private void AppendSkippedExchangeStep(in ExchangeRecord record)
        {
            var sb = OutputResultSB;
            var collector = mExchangeDataCollector;

            if (!collector.RecordEachExchange)
            {
                sb.Append('(')
                   .Append(record.SimulationStepIndex + 1)
                   .Append("|");

                if (record.ExchangeStepIndex > 1)
                {
                    sb.Append("1-");
                }

                sb.Append(record.ExchangeStepIndex + 1)
                  .Append(") ")
                  .Append("总共经过了")
                  .Append(record.ExchangeStepIndex + 1)
                  .AppendLine("次交换");

                AppendResChanged(record.ResValueRecords);
                sb.AppendLine();
            }
            else if (collector.OmitExcessiveExchanges && record.ExchangeStepIndex >= collector.MaxExchangeRecord)
            {
                sb.Append('(')
                  .Append(record.SimulationStepIndex + 1)
                  .Append('|');

                if (collector.MaxExchangeRecord != record.ExchangeStepIndex)
                {
                    sb.Append(collector.MaxExchangeRecord + 1)
                      .Append('-')
                      .Append(record.ExchangeStepIndex + 1);
                }
                else
                {
                    sb.Append(record.ExchangeStepIndex + 1);
                }

                sb.Append(") ")
                  .Append("交换次数过多，省略了")
                  .Append(record.ExchangeStepIndex - collector.MaxExchangeRecord + 1)
                  .Append("次交换信息");

                AppendResChanged(record.ResValueRecords);
                sb.AppendLine();
            }
        }

        private void AppendSimulateStepEnd(int simulationStepIndex)
        {
            if (simulationStepIndex < 0)
            {
                return;
            }

            var sb = OutputResultSB;
            var collector = mExchangeDataCollector;

            var result = collector.GetSimulateStepResult(simulationStepIndex);
            string reason = SimulationDefine.GetSimulateStepEndReason(result);

            sb.Append("模拟结束，结束原因: ")
              .Append(reason)
              .AppendLine()
              .Append("========第")
              .Append(simulationStepIndex + 1)
              .Append('/')
              .Append(collector.TotalSimulateStep)
              .AppendLine("次模拟结束========");
        }

        private void AppendExchangeStep(in ExchangeRecord record)
        {
            var sb = OutputResultSB;
            var collector = mExchangeDataCollector;

            sb.Append('(')
              .Append(record.SimulationStepIndex + 1)
              .Append('|')
              .Append(record.ExchangeStepIndex + 1)
              .Append(") ");

            if (record.ExchangeStepResult == ExchangeStepResult.Success)
            {
                string beforeName = GetTongbaoName(record.BeforeTongbaoId);
                string afterName = GetTongbaoName(record.AfterTongbaoId);

                sb.Append("将位置[")
                  .Append(record.SlotIndex + 1)
                  .Append("]上的[")
                  .Append(beforeName)
                  .Append("]交换为[")
                  .Append(afterName)
                  .Append(']');

                AppendResChanged(record.ResValueRecords);
            }
            else
            {
                switch (record.ExchangeStepResult)
                {
                    case ExchangeStepResult.SelectedEmpty:
                        sb.Append("交换失败，选中的位置[")
                          .Append(record.SlotIndex + 1)
                          .Append("]上的通宝为空");
                        break;

                    case ExchangeStepResult.TongbaoUnexchangeable:
                        sb.Append("交换失败，通宝[")
                          .Append(GetTongbaoName(record.BeforeTongbaoId))
                          .Append("]不可交换");
                        break;

                    case ExchangeStepResult.LifePointNotEnough:
                        sb.Append("交换失败，交换所需生命值不足");
                        break;

                    case ExchangeStepResult.ExchangeableTongbaoNotExist:
                        sb.Append("交换失败，通宝[")
                          .Append(GetTongbaoName(record.BeforeTongbaoId))
                          .Append("]无可交换通宝");
                        break;

                    default:
                        sb.Append("交换失败，未知错误");
                        break;
                }
            }

            sb.AppendLine();
        }

        private void AppendResChanged(ResValueRecord[] records)
        {
            bool empty = true;
            var sb = OutputResultSB;

            foreach (var r in records)
            {
                if (r.BeforeValue == r.AfterValue)
                    continue;

                if (empty)
                    sb.Append('(');
                else
                    sb.Append("，");

                sb.Append(Define.GetResName(r.ResType));

                int changedValue = r.AfterValue - r.BeforeValue;
                if (changedValue > 0)
                    sb.Append('+');

                sb.Append(changedValue)
                  .Append(": ")
                  .Append(r.BeforeValue)
                  .Append("->")
                  .Append(r.AfterValue);

                empty = false;
            }

            if (!empty)
                sb.Append(')');
        }

        private string GetTongbaoName(int tongbaoId)
        {
            TongbaoConfig config = TongbaoConfig.GetTongbaoConfigById(tongbaoId);
            return config?.Name ?? string.Empty;
        }
    }
}

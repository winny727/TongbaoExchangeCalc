using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TongbaoExchangeCalc.DataModel.Simulation
{
    public class SimulationController
    {
        public ExchangeSimulator ExchangeSimulator { get; }

        private CancellationTokenSource mCancellationTokenSource;
        public bool IsAsyncSimulating => mCancellationTokenSource != null;

        public SimulationController(PlayerData playerData, ISimulationTimer timer, IDataCollector<SimulateContext> dataCollector = null)
        {
            if (playerData == null)
            {
                throw new ArgumentNullException(nameof(playerData));
            }
            if (timer == null)
            {
                throw new ArgumentNullException(nameof(timer));
            }
            //ExchangeSimulator = new ExchangeSimulator(playerData, timer, dataCollector);
            ExchangeSimulator = new ParallelExchangeSimulator(playerData, timer, dataCollector);
        }

        public void Simulate(SimulationOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            ApplySimulationOptions(ExchangeSimulator, options);
            SimulateInternal(CancellationToken.None);
        }

        public async Task SimulateAsync(SimulationOptions options, IProgress<int> progress = null)
        {
            if (IsAsyncSimulating)
            {
                throw new InvalidOperationException("Simulation Executing.");
            }

            ApplySimulationOptions(ExchangeSimulator, options);
            mCancellationTokenSource = new CancellationTokenSource();

            try
            {
                await Task.Run(() => SimulateInternal(mCancellationTokenSource.Token, progress), mCancellationTokenSource.Token);
            }
            finally
            {
                mCancellationTokenSource.Dispose();
                mCancellationTokenSource = null;
            }
        }

        public void CancelSimulate()
        {
            mCancellationTokenSource?.Cancel();
        }

        public void RevertPlayerData()
        {
            ExchangeSimulator?.RevertPlayerData();
        }

        private void SimulateInternal(CancellationToken token, IProgress<int> progress = null)
        {
            if (ExchangeSimulator is ParallelExchangeSimulator parallelSimulator)
            {
                parallelSimulator.Simulate(token, progress);
            }
            else
            {
                ExchangeSimulator.Simulate();
            }
        }

        private void ApplySimulationOptions(ExchangeSimulator simulator, SimulationOptions options)
        {
            if (simulator == null || options == null)
            {
                return;
            }

            simulator.SimulationType = options.SimulationType;
            simulator.TotalSimulationCount = options.TotalSimulationCount;
            simulator.MinimumLifePoint = options.MinimumLifePoint;
            simulator.ExchangeSlotIndex = options.ExchangeSlotIndex;
            if (simulator is ParallelExchangeSimulator parallelSimulator)
            {
                parallelSimulator.UseMultiThreadOptimize = options.UseMultiThreadOptimize;
            }
            options.RuleController?.ApplySimulationRule(simulator); // ApplyRule
        }
    }
}

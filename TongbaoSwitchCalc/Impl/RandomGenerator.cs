using System;
using System.Collections.Generic;
using TongbaoSwitchCalc.DataModel;

namespace TongbaoSwitchCalc.Impl
{
    public class RandomGenerator : IRandomGenerator
    {
        private Random mRandom = new Random();

        public int Next(int minValue, int maxValue)
        {
            return mRandom.Next(minValue, maxValue);
        }

        public double NextDouble()
        {
            return mRandom.NextDouble();
        }

        public void SetSeed(int seed)
        {
            mRandom = new Random(seed);
        }
    }
}

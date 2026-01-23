using System;
using System.Collections.Generic;
using TongbaoExchangeCalc.DataModel;

namespace TongbaoExchangeCalc.Impl
{
    public class RandomGenerator : IRandomGenerator
    {
        private readonly Random mRandom = new Random();

        public virtual int Next(int minValue, int maxValue)
        {
            return mRandom.Next(minValue, maxValue);
        }

        public virtual double NextDouble()
        {
            return mRandom.NextDouble();
        }

        public virtual object Clone()
        {
            return new RandomGenerator();
        }
    }
}

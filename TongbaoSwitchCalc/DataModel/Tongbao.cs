using System;
using System.Collections.Generic;

namespace TongbaoSwitchCalc.DataModel
{
    [Serializable]
    public class TongbaoConfig
    {
        public int Id;
        public string Name;
        public string Description;
        public TongbaoType Type;
        public int SwitchInPool;
        public List<int> SwitchOutPools;
        public ResType ExtraResType; // 通宝自带效果
        public int ExtraResCount;

        private static readonly Dictionary<int, TongbaoConfig> mTongbaoConfigDict = new Dictionary<int, TongbaoConfig>();

        public static void AddTongbaoConfig(TongbaoConfig config)
        {
            if (config == null) return;
            mTongbaoConfigDict[config.Id] = config;
            SwitchPool.SetupTongbaoSwitchPool(config);
        }

        public static TongbaoConfig GetTongbaoConfigById(int id)
        {
            if (mTongbaoConfigDict.ContainsKey(id))
            {
                return mTongbaoConfigDict[id];
            }
            return null;
        }

        public static void ClearTongbaoConfig()
        {
            mTongbaoConfigDict.Clear();
        }
    }

    public class Tongbao
    {
        public int Id;
        public string Name;
        public string Description;
        public TongbaoType Type;
        public int SwitchInPool;
        public List<int> SwitchOutPools;
        public ResType ExtraResType; // 通宝自带效果
        public int ExtraResCount;
        public ResType RandomResType; // 品相效果
        public int RandomResCount;

        public bool CanSwitch()
        {
            return SwitchInPool > 0;
        }

        public static Tongbao CreateTongbao(IRandomGenerator random, int id)
        {
            TongbaoConfig config = TongbaoConfig.GetTongbaoConfigById(id);
            if (config == null)
            {
                return null;
            }

            Tongbao tongbao = new Tongbao
            {
                Id = config.Id,
                Name = config.Name,
                Description = config.Description,
                Type = config.Type,
                SwitchInPool = config.SwitchInPool,
                SwitchOutPools = config.SwitchOutPools,
                ExtraResType = config.ExtraResType,
                ExtraResCount = config.ExtraResCount,
            };

            tongbao.SetupRandomRes(random);

            return tongbao;
        }

        private void SetupRandomRes(IRandomGenerator random)
        {
            if (random == null)
            {
                return;
            }

            float randomValue = (float)random.NextDouble();
            float cumulativeProbability = 0f;
            RandomRes randomRes = null;
            foreach (var item in Define.RandomResDefines)
            {
                cumulativeProbability += item.Probability;
                if (cumulativeProbability > randomValue)
                {
                    randomRes = item;
                    break;
                }
            }
            if (randomRes != null)
            {
                RandomResType = randomRes.ResType;
                RandomResCount = randomRes.ResCount;
            }
        }
    }

    public enum TongbaoType
    {
        Unknown = 0,
        Balance = 1, //衡钱
        Flower = 2, //花钱
        Risk = 3, //历钱
    }
}

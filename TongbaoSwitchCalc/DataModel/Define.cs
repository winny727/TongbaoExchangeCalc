using System;
using System.Collections.Generic;

namespace TongbaoSwitchCalc.DataModel
{
    public static class Define
    {
        public static readonly List<RandomRes> RandomResDefines = new List<RandomRes>()
        {
            new RandomRes(0.0393f, ResType.Shield, 2),
            new RandomRes(0.0291f, ResType.Hope, 1),
            new RandomRes(0.0094f, ResType.Candles, 1),
        };

        // 不同分队的钱盒容量
        public static readonly Dictionary<SquadType, SquadAttribute> SquadAttributeDefines = new Dictionary<SquadType, SquadAttribute>()
        {
            { SquadType.Flower, new SquadAttribute(10, 1) },
            { SquadType.Tourist, new SquadAttribute(10, 1, 1, 2, 2, 3) },
            { SquadType.Other, new SquadAttribute(10, 1, 1, 2, 2, 3) },
        };
    }

    public enum ResType
    {
        None = 0,
        LifePoint = 1, //生命值
        OriginiumIngots = 2, //源石锭
        Coupon = 3, //票券
        Candles = 4, //烛火
        Hope = 5, //希望
        Shield = 6, //护盾
    }

    public enum SquadType
    {
        Flower = 0, //花团锦簇分队
        Tourist = 1, //游客分队
        Other = 2, //其它分队
    }

    public class RandomRes
    {
        public readonly float Probability;
        public readonly ResType ResType;
        public readonly int ResCount;

        public RandomRes(float probability, ResType resType, int resCount)
        {
            Probability = probability;
            ResType = resType;
            ResCount = resCount;
        }
    }

    public class SquadAttribute
    {
        public readonly int MaxTongbaoCount;
        public readonly int[] CostLifePoints;

        public SquadAttribute(int maxTongbaoCount, params int[] costLifePoints)
        {
            MaxTongbaoCount = maxTongbaoCount;
            CostLifePoints = costLifePoints;
        }

        // 从0开始
        public int GetCostLifePoint(int switchCount)
        {
            if (switchCount < 0 || CostLifePoints == null || CostLifePoints.Length == 0)
                return 0;

            int index = Math.Min(switchCount, CostLifePoints.Length - 1);
            return CostLifePoints[index];
        }
    }
}

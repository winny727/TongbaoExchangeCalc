using System;
using System.Collections.Generic;

namespace TongbaoSwitchCalc.DataModel
{
    public class PlayerData
    {
        private readonly IRandomGenerator mRandom;
        private readonly Dictionary<ResType, int> mResValues = new Dictionary<ResType, int>(); // 资源数值

        private Tongbao[] mTongbao;
        public SquadType SquadType { get; private set; }
        private SquadAttribute mSquadAttribute;
        public int SwitchCount { get; set; } // 已交换次数
        public int MaxTongbaoCount { get; private set; } // 最大通宝数量
        public bool HasSpecialCollectible { get; set; } // 福祸相依（交换后的通宝如果是厉钱，则获得票券+1）

        public PlayerData(IRandomGenerator random)
        {
            mRandom = random ?? throw new ArgumentNullException(nameof(random));
        }

        public void Init(SquadType squadType, Dictionary<ResType, int> resValues)
        {
            SquadType = squadType;
            mSquadAttribute = Define.SquadAttributeDefines[squadType];
            SwitchCount = 0;
            MaxTongbaoCount = mSquadAttribute.MaxTongbaoCount;
            mTongbao = new Tongbao[MaxTongbaoCount];

            mResValues.Clear();
            if (resValues != null)
            {
                foreach (var item in resValues)
                {
                    mResValues.Add(item.Key, item.Value);
                }
            }

            if (GetResValue(ResType.LifePoint) <= 0)
            {
                SetResValue(ResType.LifePoint, 1); // 默认1血
            }
        }

        public bool IsTongbaoFull()
        {
            for (int i = 0; i < mTongbao.Length; i++)
            {
                if (mTongbao[i] == null)
                {
                    return false;
                }
            }
            return true;
        }

        public Tongbao GetTongbao(int posIndex)
        {
            if (posIndex >= 0 && posIndex < mTongbao.Length)
            {
                return mTongbao[posIndex];
            }
            return null;
        }

        public void AddTongbao(Tongbao tongbao)
        {
            if (IsTongbaoFull())
            {
                return;
            }

            int posIndex = -1;
            for (int i = 0; i < mTongbao.Length; i++)
            {
                if (mTongbao[i] == null)
                {
                    posIndex = i;
                    break;
                }
            }

            InsertTongbao(tongbao, posIndex);
        }

        public void InsertTongbao(Tongbao tongbao, int posIndex)
        {
            if (tongbao == null)
            {
                return;
            }

            if (posIndex >= 0 && posIndex < mTongbao.Length)
            {
                mTongbao[posIndex] = null;
                if (!IsTongbaoExist(tongbao))
                {
                    mTongbao[posIndex] = tongbao;
                    if (tongbao.ExtraResType != ResType.None && tongbao.ExtraResCount > 0)
                    {
                        AddResValue(tongbao.ExtraResType, tongbao.ExtraResCount);
                    }
                }
            }
        }

        public void RemoveTongbaoAt(int posIndex)
        {
            if (posIndex >= 0 && posIndex < mTongbao.Length)
            {
                mTongbao[posIndex] = null;
            }
        }

        public void RemoveTongbao(Tongbao tongbao)
        {
            if (tongbao == null)
            {
                return;
            }

            for (int i = 0; i < mTongbao.Length; i++)
            {
                if (mTongbao[i].Id == tongbao.Id)
                {
                    mTongbao[i] = null;
                    return;
                }
            }
        }

        public bool IsTongbaoExist(int id)
        {
            for (int i = 0; i < mTongbao.Length; i++)
            {
                if (mTongbao[i].Id == id)
                {
                    return true;
                }
            }
            return false;
        }

        public bool IsTongbaoExist(Tongbao tongbao)
        {
            return tongbao != null && IsTongbaoExist(tongbao.Id);
        }

        public void ClearTongbao()
        {
            for (int i = 0; i < mTongbao.Length; i++)
            {
                mTongbao[i] = null;
            }
        }

        public void AddResValue(ResType type, int value)
        {
            if (type != ResType.None)
            {
                if (!mResValues.ContainsKey(type))
                {
                    mResValues.Add(type, 0);
                }
                mResValues[type] += value;
            }
        }

        public void SetResValue(ResType type, int value)
        {
            if (type != ResType.None)
            {
                if (!mResValues.ContainsKey(type))
                {
                    mResValues.Add(type, 0);
                }
                mResValues[type] = value;
            }
        }

        public int GetResValue(ResType type)
        {
            if (type == ResType.None)
            {
                return 0;
            }

            if (mResValues.TryGetValue(type, out var value))
            {
                return value;
            }

            return 0;
        }

        public void SwitchTongbao(int posIndex, bool force = false)
        {
            if (mSquadAttribute == null)
            {
                return;
            }

            Tongbao tongbao = GetTongbao(posIndex);
            if (tongbao == null || !tongbao.CanSwitch())
            {
                // 当前通宝不可交换
                return;
            }

            int costLifePoint = mSquadAttribute.GetCostLifePoint(SwitchCount);
            if (GetResValue(ResType.LifePoint) > costLifePoint || force)
            {
                int newTongbaoId = SwitchPool.SwitchTongbao(mRandom, this, tongbao);
                Tongbao newTongbao = Tongbao.CreateTongbao(mRandom, newTongbaoId);
                if (newTongbao != null)
                {
                    InsertTongbao(newTongbao, posIndex);
                    SwitchCount++;
                    AddResValue(ResType.LifePoint, -costLifePoint);
                    AddResValue(newTongbao.RandomResType, newTongbao.RandomResCount);

                    // 福祸相依
                    if (HasSpecialCollectible)
                    {
                        if (newTongbao.Type == TongbaoType.Risk)
                        {
                            AddResValue(ResType.Coupon, 1);
                        }
                    }
                }
            }
        }
    }
}

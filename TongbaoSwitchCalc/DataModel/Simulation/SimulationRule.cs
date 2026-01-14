using System;
using System.Collections;
using System.Collections.Generic;

namespace TongbaoSwitchCalc.DataModel.Simulation
{
    public abstract class SimulationRule
    {
        public abstract SimulationRuleType Type { get; }
        public abstract void ApplyRule(SwitchSimulator simulator);
        public abstract void UnapplyRule(SwitchSimulator simulator);
        public abstract bool Equals(SimulationRule other);
        public abstract string GetRuleString();
    }

    public class SimulationRuleCollection : IEnumerable<SimulationRule>
    {
        private readonly List<SimulationRule> mItems = new List<SimulationRule>();
        public IReadOnlyList<SimulationRule> Items => mItems;

        public int Count => mItems.Count;
        public SimulationRule this[int index] => mItems[index];

        private bool CanAddRule(SimulationRule rule)
        {
            if (rule == null)
            {
                return false;
            }

            foreach (var item in mItems)
            {
                if (item.Equals(rule))
                {
                    return false;
                }
            }
            return true;
        }

        public bool Add(SimulationRule item)
        {
            if (CanAddRule(item))
            {
                mItems.Add(item);
                return true;
            }
            return false;
        }

        public bool Insert(int index, SimulationRule item)
        {
            if (CanAddRule(item))
            {
                mItems.Insert(index, item);
                return true;
            }
            return false;
        }

        public bool Contains(SimulationRule item)
        {
            return mItems.Contains(item);
        }

        public int IndexOf(SimulationRule item)
        {
            return mItems.IndexOf(item);
        }

        public bool Remove(SimulationRule item)
        {
            return mItems.Remove(item);
        }

        public void RemoveAt(int index)
        {
            mItems.RemoveAt(index);
        }

        public void Clear()
        {
            mItems.Clear();
        }

        public void MoveUp(int index)
        {
            if (index > 0 && index < mItems.Count)
            {
                var item = mItems[index];
                mItems.RemoveAt(index);
                mItems.Insert(index - 1, item);
            }
        }

        public void MoveDown(int index)
        {
            if (index >= 0 && index < mItems.Count - 1)
            {
                var item = mItems[index];
                mItems.RemoveAt(index);
                mItems.Insert(index + 1, item);
            }
        }

        public IEnumerator<SimulationRule> GetEnumerator()
        {
            return mItems.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class PrioritySlotRule : SimulationRule
    {
        public override SimulationRuleType Type { get; } = SimulationRuleType.PrioritySlot;
        public int PrioritySlotIndex { get; private set; }

        public PrioritySlotRule(int slotIndex)
        {
            PrioritySlotIndex = slotIndex;
        }

        public override void ApplyRule(SwitchSimulator simulator)
        {
            if (!simulator.SlotIndexPriority.Contains(PrioritySlotIndex))
            {
                simulator.SlotIndexPriority.Add(PrioritySlotIndex);
            }
        }

        public override void UnapplyRule(SwitchSimulator simulator)
        {
            simulator.SlotIndexPriority.Remove(PrioritySlotIndex);
        }

        public override bool Equals(SimulationRule other)
        {
            return other is PrioritySlotRule otherRule && otherRule.PrioritySlotIndex == PrioritySlotIndex;
        }

        public override string GetRuleString()
        {
            return $"优先交换钱盒槽位{PrioritySlotIndex + 1}里的通宝";
        }
    }

    public class AutoStopRule : SimulationRule
    {
        public override SimulationRuleType Type { get; } = SimulationRuleType.AutoStop;
        public int TargetTongbaoId { get; private set; }

        public AutoStopRule(int targetId)
        {
            TargetTongbaoId = targetId;
        }

        public override void ApplyRule(SwitchSimulator simulator)
        {
            simulator.TargetTongbaoIds.Add(TargetTongbaoId);
        }

        public override void UnapplyRule(SwitchSimulator simulator)
        {
            simulator.TargetTongbaoIds.Remove(TargetTongbaoId);
        }

        public override bool Equals(SimulationRule other)
        {
            return other is AutoStopRule otherRule && otherRule.TargetTongbaoId == TargetTongbaoId;
        }

        public override string GetRuleString()
        {
            TongbaoConfig config = TongbaoConfig.GetTongbaoConfigById(TargetTongbaoId);
            if (config == null)
            {
                return "无效规则，通宝配置错误";
            }
            return $"交换出{config.Name}就停止";
        }
    }

    public class ExpectationTongbaoRule : SimulationRule
    {
        public override SimulationRuleType Type { get; } = SimulationRuleType.ExpectationTongbao;
        public int ExpectedTongbaoId { get; private set; }

        public ExpectationTongbaoRule(int id)
        {
            ExpectedTongbaoId = id;
        }

        public override void ApplyRule(SwitchSimulator simulator)
        {
            simulator.ExpectedTongbaoId = ExpectedTongbaoId;
        }

        public override void UnapplyRule(SwitchSimulator simulator)
        {
            if (simulator.ExpectedTongbaoId == ExpectedTongbaoId)
            {
                simulator.ExpectedTongbaoId = -1;
            }
        }

        public override bool Equals(SimulationRule other)
        {
            return true; // 限制期望通宝只能存在一个
        }

        public override string GetRuleString()
        {
            TongbaoConfig config = TongbaoConfig.GetTongbaoConfigById(ExpectedTongbaoId);
            if (config == null)
            {
                return "无效规则，通宝配置错误";
            }
            return $"期望获得{config.Name}";
        }
    }
}

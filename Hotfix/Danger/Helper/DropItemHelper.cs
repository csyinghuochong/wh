using System.Collections.Generic;

namespace ET
{
    public static class DropItemHelper
    {
        /// <summary>创建服务端掉落实例；装备按数量拆为单件并各自生成属性，普通道具保持原条目、不合并。</summary>
        public static List<DropInfo> CreateDropInfos(List<RewardItem> rewards)
        {
            List<DropInfo> drops = new List<DropInfo>();
            long count = 0;
            foreach (RewardItem reward in rewards)
            {
                if (reward.ItemNum <= 0) continue;
                count += reward.ItemType == ItemBigType.Type_Equip ? reward.ItemNum : 1;
            }
            // 展开装备数量前检查，避免错误配置创建海量对象。
            if (count > 100)
            {
                Log.Error($"Drop instance count exceeds 100: {count}");
                return drops;
            }
            foreach (RewardItem reward in rewards)
            {
                if (reward.ItemNum <= 0) continue;
                bool isEquip = reward.ItemType == ItemBigType.Type_Equip;
                int copies = isEquip ? reward.ItemNum : 1;
                for (int i = 0; i < copies; i++)
                {
                    DropInfo drop = new DropInfo
                    {
                        UnitId = IdGenerater.Instance.GenerateId(),
                        ItemType = reward.ItemType,
                        ItemID = reward.ItemID,
                        ItemNum = isEquip ? 1 : reward.ItemNum,
                        ItemFlags = ItemNewHelper.ResolveAddItemFlags(reward),
                    };
                    if (isEquip)
                    {
                        drop.EquipInfo = new BagInfo
                        {
                            BagInfoID = IdGenerater.Instance.GenerateId(),
                            ItemType = drop.ItemType,
                            ItemID = drop.ItemID,
                            ItemNum = 1,
                            ItemFlags = drop.ItemFlags,
                        };
                        ItemAddHelper.InitEquipAttributes(drop.EquipInfo);
                    }
                    drops.Add(drop);
                }
            }
            return drops;
        }
    }
}

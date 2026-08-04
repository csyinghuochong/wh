using System.Collections.Generic;

namespace ET
{

    /// <summary>
    /// 附加方法
    /// </summary>
    public static class ItemAddHelper
    {

        public static void OnItemUpdate( Unit self, BagInfo bagInfo)
        {
            //通知客户端背包道具发生改变
            M2C_RoleBagUpdate m2c_bagUpdate = new M2C_RoleBagUpdate();
            m2c_bagUpdate.BagInfoUpdate = new List<BagInfo>();
            m2c_bagUpdate.BagInfoUpdate.Add(bagInfo);
            MessageHelper.SendToClient(self, m2c_bagUpdate);
        }
        
        
        public static void OnGetItem(this Unit self, int getWay, BagInfo bagInfo)
        {
            if (bagInfo == null)
            {
                return;
            }
            TaskComponentServer task = self.GetComponent<TaskComponentServer>();
            task.OnGetItem_2(bagInfo.ItemType, bagInfo.ItemID);
            task.OnGetItemNumber(getWay, bagInfo.ItemType, bagInfo.ItemID, bagInfo.ItemNum);
        }
        
        public static void OnGetItem(this Unit self, int getWay, int itemType, int itemId, long itemNumber)
        {
            TaskComponentServer task = self.GetComponent<TaskComponentServer>();
            task.OnGetItem_2(itemType, itemId);
            task.OnGetItemNumber(getWay, itemType, itemId, (int)itemNumber);
        }
        
        public static void OnGetItem(this Unit self, int getWay, RewardItem rewardItem)
        {
            if (rewardItem == null)
            {
                return;
            }
            TaskComponentServer task = self.GetComponent<TaskComponentServer>();
            task.OnGetItem_2(rewardItem.ItemType, rewardItem.ItemID);
            task.OnGetItemNumber(getWay, rewardItem.ItemType, rewardItem.ItemID, rewardItem.ItemNum);
        }

        /// <summary>
        /// 任务类型2要检测一下道具数量
        /// </summary>
        /// <param name="self"></param>
        /// <param name="itemId"></param>
        public static void OnCostItem(this Unit self, int itemType, int itemId)
        {
            self.GetComponent<TaskComponentServer>().OnGetItem_2(itemType, itemId);
        }
    }
}

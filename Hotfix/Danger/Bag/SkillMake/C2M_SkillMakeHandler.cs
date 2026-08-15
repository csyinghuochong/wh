using System;
using System.Collections.Generic;

namespace ET
{

    [ActorMessageHandler]
    public class C2M_SkillMakeHandler : AMActorLocationRpcHandler<Unit, C2M_SkillMakeRequest, M2C_SkillMakeResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_SkillMakeRequest request, M2C_SkillMakeResponse response, Action reply)
        {
            if (!LDSkill_MakeCategory.Instance.Contain(request.SkillMakeId))
            {
                response.Error = ErrorCode.ERR_ModifyData;
                reply();
                return;
            }

            RoleInfo roleInfo = unit.GetComponent<RoleInfoComponentServer>().RoleInfo;
            if (roleInfo.MakeList == null || !roleInfo.MakeList.Contains(request.SkillMakeId))
            {
                response.Error = ErrorCode.ERR_MakeTypeError;
                reply();
                return;
            }

            LDSkill_Make cfg = LDSkill_MakeCategory.Instance.Get(request.SkillMakeId);
            if (!TryRollMakeItem(cfg, out RewardItem reward))
            {
                response.Error = ErrorCode.ERR_ModifyData;
                reply();
                return;
            }

            BagComponentServer bag = unit.GetComponent<BagComponentServer>();
            List<RewardItem> consumeItems = ItemNewHelper.GetRewardItems(cfg.Consume);
            if (consumeItems.Count > 0 && !bag.CheckNeedItem(consumeItems))
            {
                response.Error = ErrorCode.ERR_ItemNotEnoughError;
                reply();
                return;
            }

            if (!HasBagSpace(bag, reward))
            {
                response.Error = ErrorCode.ERR_BagIsFull;
                reply();
                return;
            }

            const int getWay = ItemGetWay.SkillMake;
            if (consumeItems.Count > 0 && !bag.OnCostItemData(consumeItems, ItemLocType.ItemLocBag, getWay))
            {
                response.Error = ErrorCode.ERR_ItemNotEnoughError;
                reply();
                return;
            }

            List<RewardItem> rewards = new List<RewardItem> { reward };
            if (!bag.OnAddItemData(rewards, string.Empty, $"{getWay}_{TimeHelper.ServerNow()}"))
            {
                Log.Warning($"C2M_SkillMake add fail unit={unit.Id} makeId={cfg.Id} item={reward.ItemID}");
                response.Error = ErrorCode.ERR_BagIsFull;
            }

            reply();
            await ETTask.CompletedTask;
        }

        private static bool TryRollMakeItem(LDSkill_Make cfg, out RewardItem reward)
        {
            reward = null;
            int[] types = { cfg.Make_Type_1, cfg.Make_Type_2, cfg.Make_Type_3, cfg.Make_Type_4, cfg.Make_Type_5 };
            int[] ids = { cfg.Make_Id_1, cfg.Make_Id_2, cfg.Make_Id_3, cfg.Make_Id_4, cfg.Make_Id_5 };
            int[] nums = { cfg.Make_Num_1, cfg.Make_Num_2, cfg.Make_Num_3, cfg.Make_Num_4, cfg.Make_Num_5 };
            int[] weights = { cfg.Make_Weight_1, cfg.Make_Weight_2, cfg.Make_Weight_3, cfg.Make_Weight_4, cfg.Make_Weight_5 };

            List<int> validIndex = new List<int>(5);
            List<int> validWeights = new List<int>(5);
            for (int i = 0; i < 5; i++)
            {
                if (types[i] <= 0 || ids[i] <= 0 || nums[i] <= 0 || weights[i] <= 0)
                {
                    continue;
                }

                validIndex.Add(i);
                validWeights.Add(weights[i]);
            }

            if (validWeights.Count == 0)
            {
                return false;
            }

            int pick = RandomHelper.RandomByWeight(validWeights);
            if (pick < 0 || pick >= validIndex.Count)
            {
                return false;
            }

            int slot = validIndex[pick];
            reward = new RewardItem
            {
                ItemType = types[slot],
                ItemID = ids[slot],
                ItemNum = nums[slot]
            };
            return true;
        }

        private static bool HasBagSpace(BagComponentServer bag, RewardItem reward)
        {
            if (ItemNewHelper.GetItemToUserDataType(reward) != UserDataType.None)
            {
                return true;
            }

            ItemLocType toLoc = ItemNewHelper.GetToItemLocType(reward);
            List<BagInfo> locList = bag.GetItemByLoc(toLoc);
            if (locList == null)
            {
                return false;
            }

            int pileSum = ItemNewHelper.GetNewItemPileSum(reward);
            int need = ItemNewHelper.CalcNeedNewCells(locList, reward.ItemType, reward.ItemID, reward.ItemNum, pileSum);
            return need <= bag.GetBagLeftCell((int)toLoc);
        }
    }
}

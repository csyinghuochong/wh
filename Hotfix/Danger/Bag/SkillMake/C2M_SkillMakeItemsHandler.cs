using System;
using System.Collections.Generic;

namespace ET
{

    [ActorMessageHandler]
    public class C2M_SkillMakeItemsHandler : AMActorLocationRpcHandler<Unit, C2M_SkillMakeItemsRequest, M2C_SkillMakeItemsResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_SkillMakeItemsRequest request, M2C_SkillMakeItemsResponse response, Action reply)
        {
            if (!LDSkill_MakeCategory.Instance.Contain(request.SkillMakeId))
            {
                response.Error = ErrorCode.ERR_ModifyData;
                reply();
                return;
            }

            RoleInfoComponentServer roleInfoComponent = unit.GetComponent<RoleInfoComponentServer>();
            RoleInfo roleInfo = roleInfoComponent.RoleInfo;
            if (roleInfo.MakeIdList == null || !roleInfo.MakeIdList.Contains(request.SkillMakeId))
            {
                response.Error = ErrorCode.ERR_MakeNoLearnError;
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
            List<RewardItem> consumeItems = new List<RewardItem>();
            SkillMakeHelper.CollectConsumeItems(cfg, roleInfo.MakeIdList, 1, consumeItems, out int vitalityCost, out int bindGoldCost);
            Dictionary<int, List<RewardItem>> locCosts = consumeItems.GroupCostByLoc();
            if (!bag.CanCostByLoc(locCosts))
            {
                response.Error = ErrorCode.ERR_ItemNotEnoughError;
                reply();
                return;
            }

            if (vitalityCost > 0 && bag.GetItemNumber(ItemBigType.Type_Item, UserDataType.HuoLi) < vitalityCost)
            {
                response.Error = ErrorCode.ERR_VitalityNotEnoughError;
                reply();
                return;
            }

            if (bindGoldCost > 0 && bag.GetItemNumber(ItemBigType.Type_Item, UserDataType.BindGold) < bindGoldCost)
            {
                response.Error = ErrorCode.ERR_GoldNotEnoughError;
                reply();
                return;
            }

            long makeTime = TimeHelper.ServerNow();
            string makePlayer = roleInfo.Name ?? string.Empty;
            if (!HasBagSpace(bag, reward, makePlayer, makeTime))
            {
                response.Error = ErrorCode.ERR_BagIsFull;
                reply();
                return;
            }

            const int getWay = ItemGetWay.SkillMake;
            if (!bag.CostByLoc(locCosts, getWay))
            {
                response.Error = ErrorCode.ERR_ItemNotEnoughError;
                reply();
                return;
            }

            if (vitalityCost > 0)
            {
                roleInfoComponent.UpdateRoleData(UserDataType.HuoLi, (-vitalityCost).ToString(), true, getWay);
            }

            if (bindGoldCost > 0)
            {
                roleInfoComponent.UpdateRoleData(UserDataType.BindGold, (-bindGoldCost).ToString(), true, getWay);
            }

            bag.OnAddItemData(new List<RewardItem> { reward }, new AddItemArg($"{getWay}_{makeTime}")
            {
                MakePlayer = makePlayer,
                MakeTime = makeTime,
            });
            int makeType = RoleInfoComponentServerSystem.GetSkillMakeType(cfg);
            roleInfoComponent.AddMakeExp(makeType, cfg.Exp);
            response.MakeLevel = roleInfo.MakeLevelList;
            response.MakeExp = roleInfo.MakeExpList;
            response.MakeIdList = roleInfo.MakeIdList;
            reply();
            await ETTask.CompletedTask;
        }

        private static bool TryRollMakeItem(LDSkill_Make cfg, out RewardItem reward)
        {
            reward = null;
            int[] types = { cfg.Make_Type_1, cfg.Make_Type_2, cfg.Make_Type_3, cfg.Make_Type_4, cfg.Make_Type_5, cfg.Make_Type_6 };
            int[] ids = { cfg.Make_Id_1, cfg.Make_Id_2, cfg.Make_Id_3, cfg.Make_Id_4, cfg.Make_Id_5, cfg.Make_Id_6 };
            int[] nums = { cfg.Make_Num_1, cfg.Make_Num_2, cfg.Make_Num_3, cfg.Make_Num_4, cfg.Make_Num_5, cfg.Make_Num_6 };
            int[] weights = { cfg.Make_Weight_1, cfg.Make_Weight_2, cfg.Make_Weight_3, cfg.Make_Weight_4, cfg.Make_Weight_5, cfg.Make_Weight_6 };
            int[] trades = { cfg.Trade_Type_1, cfg.Trade_Type_2, cfg.Trade_Type_3, cfg.Trade_Type_4, cfg.Trade_Type_5, cfg.Trade_Type_6 };

            List<int> validIndex = new List<int>(6);
            List<int> validWeights = new List<int>(6);
            for (int i = 0; i < ids.Length; i++)
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
                ItemNum = nums[slot],
                ItemFlags = trades[slot]
            };
            return true;
        }

        private static bool HasBagSpace(BagComponentServer bag, RewardItem reward, string makePlayer, long makeTime)
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
            int need = ItemNewHelper.CalcNeedNewCells(locList, reward.ItemType, reward.ItemID, reward.ItemNum, pileSum, reward.ItemFlags, makePlayer, makeTime);
            return need <= bag.GetBagLeftCell((int)toLoc);
        }
    }
}

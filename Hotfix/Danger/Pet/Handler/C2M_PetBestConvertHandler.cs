using System;
using System.Collections.Generic;

namespace ET
{
    /// <summary>
    /// 神兽转换。消耗读 Pet_Star.Best_Convert，按神兽当前 Star 取。
    /// </summary>
    [ActorMessageHandler]
    public class C2M_PetBestConvertHandler : AMActorLocationRpcHandler<Unit, C2M_PetBestConvertRequest, M2C_PetBestConvertResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_PetBestConvertRequest request, M2C_PetBestConvertResponse response, Action reply)
        {
            PetComponentServer petComponentServer = unit.GetComponent<PetComponentServer>();
            PetInfo rolePetInfo = petComponentServer.GetPetInfo(request.PetInfoId);
            if (rolePetInfo == null)
            {
                response.Error = ErrorCode.ERR_Pet_NoExist;
                reply();
                return;
            }

            if (rolePetInfo.PetStatus == 1)
            {
                response.Error = ErrorCode.ERR_Pet_HeCheng_FightError;
                reply();
                return;
            }

            if (request.TargetConfigId == rolePetInfo.ConfigId
                || !LDPetCategory.Instance.Contain(rolePetInfo.ConfigId)
                || !PetHelper.IsShenShou(rolePetInfo.ConfigId)
                || !LDPetCategory.Instance.Contain(request.TargetConfigId)
                || !PetHelper.IsShenShou(request.TargetConfigId))
            {
                response.Error = ErrorCode.ERR_Pet_HeCheng_IdError;
                reply();
                return;
            }

            if (!LDPet_StarCategory.Instance.Contain(rolePetInfo.Star))
            {
                response.Error = ErrorCode.ERR_Pet_HeCheng_StarError;
                reply();
                return;
            }

            List<RewardItem> costs = ItemNewHelper.GetRewardItems(LDPet_StarCategory.Instance.Get(rolePetInfo.Star).Best_Convert);
            BagComponentServer bag = unit.GetComponent<BagComponentServer>();
            Dictionary<int, List<RewardItem>> locCosts = GroupCostByLoc(costs);
            if (!CanCostByLoc(bag, locCosts))
            {
                response.Error = ErrorCode.ERR_ItemNotEnoughError;
                reply();
                return;
            }

            foreach (KeyValuePair<int, List<RewardItem>> kv in locCosts)
            {
                if (!bag.OnCostItemData(kv.Value, (ItemLocType)kv.Key, ItemGetWay.CostItem))
                {
                    response.Error = ErrorCode.ERR_ItemNotEnoughError;
                    reply();
                    return;
                }
            }

            rolePetInfo.ConfigId = request.TargetConfigId;
            petComponentServer.UpdatePetAttribute(rolePetInfo, true);

            M2C_PetListUpdate update = new M2C_PetListUpdate();
            update.PetInfoUpdate.Add(rolePetInfo);
            MessageHelper.SendToClient(unit, update);

            response.PetInfo = rolePetInfo;
            reply();
            await ETTask.CompletedTask;
        }

        static Dictionary<int, List<RewardItem>> GroupCostByLoc(List<RewardItem> costs)
        {
            Dictionary<int, List<RewardItem>> locCosts = new Dictionary<int, List<RewardItem>>();
            for (int i = 0; i < costs.Count; i++)
            {
                RewardItem cost = costs[i];
                int itemType = cost.ItemType > 0 ? cost.ItemType : ItemBigType.Type_Item;
                int loc = (int)ItemNewHelper.GetToItemLocType(itemType, cost.ItemID);
                if (!locCosts.TryGetValue(loc, out List<RewardItem> list))
                {
                    list = new List<RewardItem>();
                    locCosts.Add(loc, list);
                }

                list.Add(cost);
            }

            return locCosts;
        }

        static bool CanCostByLoc(BagComponentServer bag, Dictionary<int, List<RewardItem>> locCosts)
        {
            foreach (KeyValuePair<int, List<RewardItem>> kv in locCosts)
            {
                ItemLocType loc = (ItemLocType)kv.Key;
                for (int i = 0; i < kv.Value.Count; i++)
                {
                    RewardItem cost = kv.Value[i];
                    int itemType = cost.ItemType > 0 ? cost.ItemType : ItemBigType.Type_Item;
                    if (bag.GetItemNumberForCost(itemType, cost.ItemID, loc) < cost.ItemNum)
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}

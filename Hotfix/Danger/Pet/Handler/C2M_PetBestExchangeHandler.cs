using System;
using System.Collections.Generic;

namespace ET
{
    /// <summary>
    /// 神兽兑换。消耗读 GlobalValue.Global_Pet_Best_Exchange。
    /// </summary>
    [ActorMessageHandler]
    public class C2M_PetBestExchangeHandler : AMActorLocationRpcHandler<Unit, C2M_PetBestExchangeRequest, M2C_PetBestExchangeResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_PetBestExchangeRequest request, M2C_PetBestExchangeResponse response, Action reply)
        {
            if (!LDPetCategory.Instance.Contain(request.PetConfigId) || !PetHelper.IsShenShou(request.PetConfigId))
            {
                response.Error = ErrorCode.ERR_Pet_HeCheng_IdError;
                reply();
                return;
            }

            List<RewardItem> costs = ItemNewHelper.GetRewardItems(
                    LDGlobalValueCategory.Instance.GetByKey(GlobalValueKey.Global_Pet_Best_Exchange).Value);
            BagComponentServer bag = unit.GetComponent<BagComponentServer>();
            if (costs.Count > 0 && !bag.OnCostItemData(costs, ItemLocType.ItemLocBag, ItemGetWay.CostItem))
            {
                response.Error = ErrorCode.ERR_ItemNotEnoughError;
                reply();
                return;
            }

            response.PetInfo = unit.GetComponent<PetComponentServer>().OnAddPet(ItemGetWay.DuiHuan, request.PetConfigId);
            reply();
            await ETTask.CompletedTask;
        }
    }
}

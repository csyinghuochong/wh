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
            List<int> shenshouIds = PetHelper.GetAllShenShou();
            if (shenshouIds.Count == 0)
            {
                Console.WriteLine("shenshouIds.Count == 0");
            }
           
            List<RewardItem> costs = ItemNewHelper.GetRewardItems(
                    LDGlobalValueCategory.Instance.GetByKey(GlobalValueKey.Global_Pet_Best_Exchange).Value);
            BagComponentServer bag = unit.GetComponent<BagComponentServer>();
            Dictionary<int, List<RewardItem>> locCosts = costs.GroupCostByLoc();
            if (!bag.CanCostByLoc(locCosts) || !bag.CostByLoc(locCosts, ItemGetWay.CostItem))
            {
                response.Error = ErrorCode.ERR_ItemNotEnoughError;
                reply();
                return;
            }

            int petConfigId = shenshouIds[RandomHelper.RandomNumber(0, shenshouIds.Count)];
            response.PetInfo = unit.GetComponent<PetComponentServer>().OnAddPet(ItemGetWay.DuiHuan, petConfigId);
            reply();
            await ETTask.CompletedTask;
        }
    }
}

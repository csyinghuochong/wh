using System;
using System.Collections.Generic;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_PetHeXinChouKaHandler: AMActorLocationRpcHandler<Unit, C2M_PetHeXinChouKaRequest, M2C_PetHeXinChouKaResponse>
    {
        private static string[] CachedExploreDiscountSet;
        private static string[] CachedChouKaConfigParts;
        private static string[] CachedTenChouKaConfigParts;
        private static string[] CachedTenCostItemParts;

        private static void EnsureChouKaConfigCache()
        {
            if (CachedExploreDiscountSet != null)
            {
                return;
            }
            CachedExploreDiscountSet = LDGlobalValueCategory.Instance.Get(112).Value.Split(';');
            CachedChouKaConfigParts = LDGlobalValueCategory.Instance.Get(110).Value.Split('@');
            CachedTenChouKaConfigParts = LDGlobalValueCategory.Instance.Get(111).Value.Split('@');
            CachedTenCostItemParts = CachedTenChouKaConfigParts[0].Split(';');
        }

        protected override async ETTask Run(Unit unit, C2M_PetHeXinChouKaRequest request, M2C_PetHeXinChouKaResponse response, Action reply)
        {
            BagComponentServer bagComponentServer = unit.GetComponent<BagComponentServer>();
            NumericComponent numericComponent = unit.GetComponent<NumericComponent>();
            EnsureChouKaConfigCache();
            string[] exploreDiscountSet = CachedExploreDiscountSet;
            if (bagComponentServer.GetBagLeftCell() < request.ChouKaType)
            {
                response.Error = ErrorCode.ERR_BagIsFull;
                reply();
                return;
            }

            int dropId = 0;
            int exlporeNumber = numericComponent.GetAsInt(NumericType.PetHeXinExploreNumber);
            int exploreDiscountThreshold = int.Parse(exploreDiscountSet[0]);
            float discount;
            if (exlporeNumber < exploreDiscountThreshold) // 超过300次打8折
            {
                discount = 1;
            }
            else
            {
                discount = float.Parse(exploreDiscountSet[1]);
            }

            if (request.ChouKaType == 1)
            {
                string[] chouKaConfig = CachedChouKaConfigParts;
                string needItems = chouKaConfig[0];
                dropId = int.Parse(chouKaConfig[1]);
                bool sucess = bagComponentServer.OnCostItemData(needItems, ItemLocType.ItemLocBag, ItemGetWay.PetHeXinExplore);
                if (!sucess)
                {
                    response.Error = ErrorCode.ERR_ItemNotEnoughError;
                    reply();
                    return;
                }

                //unit.GetComponent<NumericComponent>().ApplyChange(null, NumericType.PetExploreNumber, 1, 0);
            }
            else if (request.ChouKaType == 10)
            {
                string[] itemInfo10 = CachedTenCostItemParts;
                dropId = int.Parse(CachedTenChouKaConfigParts[1]);
                bool sucess = bagComponentServer.OnCostItemData($"{itemInfo10[0]};{(int)(int.Parse(itemInfo10[1]) * discount)}", ItemLocType.ItemLocBag, ItemGetWay.PetChouKa);
                if (!sucess)
                {
                    response.Error = ErrorCode.ERR_ItemNotEnoughError;
                    reply();
                    return;
                }
                numericComponent.ApplyChange(null, NumericType.PetHeXinExploreNumber, 10, 0);
            }
            else
            {
                Log.Error($"C2M_PetHeXinChouKaRequest 1");
                response.Error = ErrorCode.ERR_ModifyData;
                reply();
                return;
            }

            List<RewardItem> rewardItems = new List<RewardItem>();
            for (int i = 0; i < request.ChouKaType; i++)
            {
                DropHelper.DropIDToDropItem_2(dropId, rewardItems);
            }
            
            bagComponentServer
                    .OnAddItemData(rewardItems, string.Empty, $"{ItemGetWay.PetExplore}_{TimeHelper.ServerNow()}");
            response.ReardList = rewardItems;
            reply();
            await ETTask.CompletedTask;
        }
    }
}
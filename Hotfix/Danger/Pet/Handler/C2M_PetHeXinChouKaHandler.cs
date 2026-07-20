using System;
using System.Collections.Generic;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_PetHeXinChouKaHandler: AMActorLocationRpcHandler<Unit, C2M_PetHeXinChouKaRequest, M2C_PetHeXinChouKaResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_PetHeXinChouKaRequest request, M2C_PetHeXinChouKaResponse response, Action reply)
        {
            BagComponentServer bagComponentServer = unit.GetComponent<BagComponentServer>();
            NumericComponent numericComponent = unit.GetComponent<NumericComponent>();
            if (bagComponentServer.GetBagLeftCell() < request.ChouKaType)
            {
                response.Error = ErrorCode.ERR_BagIsFull;
                reply();
                return;
            }

            int dropId = 0;
            int exlporeNumber = numericComponent.GetAsInt(NumericType.PetHeXinExploreNumber);
            string[] set = LDGlobalValueCategory.Instance.Get(112).Value.Split(';');
            float discount;
            if (exlporeNumber < int.Parse(set[0])) // 超过300次打8折
            {
                discount = 1;
            }
            else
            {
                discount = float.Parse(set[1]);
            }

            if (request.ChouKaType == 1)
            {
                string[] chouKaConfig = LDGlobalValueCategory.Instance.Get(110).Value.Split('@');
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
                string[] chouKaConfig = LDGlobalValueCategory.Instance.Get(111).Value.Split('@');
                string[] itemInfo10 = chouKaConfig[0].Split(';');
                dropId = int.Parse(chouKaConfig[1]);
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
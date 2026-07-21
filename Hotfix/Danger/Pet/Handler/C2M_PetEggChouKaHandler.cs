using System;
using System.Collections.Generic;

namespace ET
{

    [ActorMessageHandler]
    public class C2M_PetEggChouKaHandler : AMActorLocationRpcHandler<Unit, C2M_PetEggChouKaRequest, M2C_PetEggChouKaResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_PetEggChouKaRequest request, M2C_PetEggChouKaResponse response, Action reply)
        {
            BagComponentServer bagComponentServer = unit.GetComponent<BagComponentServer>();
            NumericComponent numericComponent = unit.GetComponent<NumericComponent>();
            RoleInfoComponentServer roleInfoComponentServer = unit.GetComponent<RoleInfoComponentServer>();
            RoleInfo roleInfo = roleInfoComponentServer.RoleInfo;
            string[] exploreDiscountSet = LDGlobalValueCategory.Instance.Get(107).Value.Split(';');
            string chouKaConfigValue = LDGlobalValueCategory.Instance.Get(39).Value;
            string tenChouKaConfigValue = LDGlobalValueCategory.Instance.Get(40).Value;

            if (bagComponentServer.GetBagLeftCell() < request.ChouKaType)
            {
                response.Error = ErrorCode.ERR_BagIsFull;
                reply();
                return;
            }
            if(request.ChouKaType!=1 && request.ChouKaType!= 10)
            {
                Log.Error($"C2M_PetEggChouKaRequest 1");
                response.Error = ErrorCode.ERR_ModifyData;
                reply();
                return;
            }

            int dropId = 0;
            int exlporeNumber = numericComponent.GetAsInt(NumericType.PetExploreNumber);
            float discount;
            if (exlporeNumber < int.Parse(exploreDiscountSet[0])) // 超过300次打8折
            {
                discount = 1;
            }
            else
            {
                discount = float.Parse(exploreDiscountSet[1]);
            }

            if (request.ChouKaType == 1)
            {
                string[] chouKaConfig = chouKaConfigValue.Split('@');
                string needItems = chouKaConfig[0];
                dropId = int.Parse(chouKaConfig[1]);
                bool sucess = bagComponentServer.OnCostItemData(needItems, ItemLocType.ItemLocBag, ItemGetWay.PetEggDuiHuan);
                if (!sucess)
                {
                    response.Error = ErrorCode.ERR_ItemNotEnoughError;
                    reply();
                    return;
                }

                numericComponent.ApplyChange(null, NumericType.PetExploreNumber, 1, 0);
            }
            else if (request.ChouKaType == 10)
            {
                string[] tenChouKaConfig = tenChouKaConfigValue.Split('@');
                int needDimanond = int.Parse(tenChouKaConfig[0]);
                dropId = int.Parse(tenChouKaConfig[1]);

                if (request.CostType == 2)
                {
                    if (bagComponentServer.GetItemNumber(ItemBigType.Type_Item, CommonConfig.ZuanShiTenChoukaItem) < 1)
                    {
                        response.Error = ErrorCode.ERR_ItemNotEnoughError;
                        reply();
                        return;
                    }

                    bagComponentServer.OnCostItemData($"{CommonConfig.ZuanShiTenChoukaItem};1", ItemLocType.ItemLocBag, ItemGetWay.ChouKa);
                    numericComponent.ApplyChange(null, NumericType.PetExploreNumber, 10, 0);
                }
                else
                {
                    if (roleInfo.Diamond < (int)(needDimanond * discount))
                    {
                        response.Error = ErrorCode.ERR_DiamondNotEnoughError;
                        reply();
                        return;
                    }
                    roleInfoComponentServer.UpdateRoleMoneySub(UserDataType.Diamond, (-1 * (int)(needDimanond * discount)).ToString(), true, ItemGetWay.PetChouKa);
                    numericComponent.ApplyChange(null, NumericType.PetExploreNumber, 10, 0);
                }
            }

            int oldValue = exlporeNumber / 10;
            int newValue = (exlporeNumber + request.ChouKaType ) / 10;

            if (newValue > oldValue)
            {
                numericComponent.ApplyChange(null, NumericType.PetExploreLuckly, RandomHelper.RandomNumber(5,16), 0);
            }
            int exploreLuck = numericComponent.GetAsInt(NumericType.PetExploreLuckly);
            List <RewardItem> rewardItems = new List<RewardItem>();
            for (int i = 0; i < request.ChouKaType; i++)
            {
                DropHelper.DropIDToDropItem_2(dropId, rewardItems);
            }
            bagComponentServer.OnAddItemData(rewardItems, string.Empty, $"{ItemGetWay.PetExplore}_{TimeHelper.ServerNow()}_{exploreLuck}");
            response.ReardList = rewardItems;
            reply();
            await ETTask.CompletedTask;
        }
    }
}

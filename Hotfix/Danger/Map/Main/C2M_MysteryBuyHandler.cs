using System;

namespace ET
{

    [ActorMessageHandler]
    public class C2M_MysteryBuyHandler : AMActorLocationRpcHandler<Unit, C2M_MysteryBuyRequest, M2C_MysteryBuyResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_MysteryBuyRequest request, M2C_MysteryBuyResponse response, Action reply)
        {
            using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.Buy, unit.Id))
            {
                int mysteryId = request.MysteryItemInfo.MysteryId;
                /*
                if (!MysteryConfigCategory.Instance.Contain(mysteryId))
                {
                    Log.Error($"C2M_MysteryBuyRequest 1");
                    response.Error = ErrorCode.ERR_ModifyData;
                    reply();
                    return;
                }
                MysteryConfig mysteryConfig = MysteryConfigCategory.Instance.Get(mysteryId);
                if (mysteryConfig == null)
                {
                    response.Error = ErrorCode.ERR_NetWorkError;
                    reply();
                    return;
                }
                RoleInfoComponentServer roleInfo = unit.GetComponent<RoleInfoComponentServer>();
                BagComponentServer bag = unit.GetComponent<BagComponentServer>();
                if (roleInfo.GetMysteryBuy(mysteryId) >= mysteryConfig.BuyNumMax)
                {
                    response.Error = ErrorCode.ERR_MysteryItem_Max;
                    reply();
                    return;
                }
                if (bag.GetBagLeftCell() < 1)
                {
                    response.Error = ErrorCode.ERR_BagIsFull;
                    reply();
                    return;
                }

                if (!bag.CheckNeedItem($"{mysteryConfig.SellType};{mysteryConfig.SellValue}"))
                {
                    response.Error = ErrorCode.ERR_ItemNotEnoughError;
                    reply();
                    return;
                }

                long chargeServerId = DBHelper.GetActivityServerId(unit);
                request.MysteryItemInfo.ItemID = mysteryConfig.SellItemID;
                request.MysteryItemInfo.ItemNumber = 1;
                A2M_MysteryBuyResponse r_GameStatusResponse = (A2M_MysteryBuyResponse)await ActorMessageSenderComponent.Instance.Call
                    (chargeServerId, new M2A_MysteryBuyRequest()
                    {
                        MysteryItemInfo = request.MysteryItemInfo
                    });

                if (r_GameStatusResponse.Error != ErrorCode.ERR_Success)
                {
                    response.Error = r_GameStatusResponse.Error;
                    reply();
                    return;
                }

                LogHelper.LogWarning($"神秘商人购买道具: {unit.DomainZone()} {unit.Id} {mysteryId}");
                roleInfo.OnMysteryBuy(mysteryId);
                bag.OnCostItemData($"{mysteryConfig.SellType};{mysteryConfig.SellValue}", ItemLocType.ItemLocBag, ItemGetWay.MysteryBuy);
                bag.OnAddItemData($"{mysteryConfig.SellItemID};{1}",
                    $"{ItemGetWay.MysteryBuy}_{TimeHelper.ServerNow()}");
                    */

                reply();
            }
        }
    }
}

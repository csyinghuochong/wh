using System;
using System.Collections.Generic;

namespace ET
{

    [ActorMessageHandler]
    public class C2M_ActivityChouKaHandler : AMActorLocationRpcHandler<Unit, C2M_ActivityChouKaRequest, M2C_ActivityChouKaResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_ActivityChouKaRequest request, M2C_ActivityChouKaResponse response, Action reply)
        {
            BagComponentServer bagComponentServer = unit.GetComponent<BagComponentServer>();
            NumericComponent numericComponent = unit.GetComponent<NumericComponent>();
            if (bagComponentServer.GetBagLeftCell() < 1)
            {
                response.Error = ErrorCode.ERR_BagIsFull;
                reply();
                return;
            }
            if (!bagComponentServer.CheckNeedItem(ActivityV1Config.ChouKaCostItem))
            {
                response.Error = ErrorCode.ERR_ItemNotEnoughError;
                reply();
                return;
            }

            numericComponent.ApplyChange( null,NumericType.V1ChouKaNumber, 1, 0 );

            int dropId = ActivityV1Config.ChouKaDropId[0];
            ServerInfo serverInfo = ConfigData.ServerInfoList[UnitZoneHelper.GetHomeZone(unit)];
            if (serverInfo != null)
            {
                dropId = serverInfo.ChouKaDropId;
            }

            List<RewardItem> rewardItems = new List<RewardItem>();  
            DropHelper.DropIDToDropItem_2(dropId, rewardItems);
            bagComponentServer.OnCostItemData(ActivityV1Config.ChouKaCostItem,  ItemLocType.ItemLocBag, ItemGetWay.Activity);
            bagComponentServer.OnAddItemData(rewardItems, string.Empty, $"{ItemGetWay.ActivityChouKa}_{TimeHelper.ServerNow()}");

            reply();
            await ETTask.CompletedTask;
        }
    }
}

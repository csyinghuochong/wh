using System;
using System.Collections.Generic;


namespace ET
{

    [ActorMessageHandler]
    public class C2M_ChouKa2RefreshHandler : AMActorLocationRpcHandler<Unit, C2M_ChouKa2RefreshRequest, M2C_ChouKa2RefreshResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_ChouKa2RefreshRequest request, M2C_ChouKa2RefreshResponse response, Action reply)
        {
            ActivityComponent activityComponent = unit.GetComponent <ActivityComponent>();

            if (activityComponent.ActivityV1Info.ChouKa2RewardIds.Count < activityComponent.ActivityV1Info.ChouKa2ItemList.Split('@').Length / 2)
            {
                BagComponentServer bagComponentServer  = unit.GetComponent <BagComponentServer>();
                if (!bagComponentServer.CheckNeedItem(ActivityV1Config.Chou2FreshItem))
                {
                    response.Error = ErrorCode.ERR_ItemNotEnoughError;
                    reply();
                    return;
                }
                bagComponentServer.OnCostItemData(ActivityV1Config.Chou2FreshItem, ItemLocType.ItemLocBag, ItemGetWay.ActivityChouKa);
            }

            activityComponent.ActivityV1Info.ChouKa2ItemList = ActivityV1Config.GetChouKa2RewardList();
            activityComponent.ActivityV1Info.ChouKa2RewardIds.Clear();
            response.ActivityV1Info = activityComponent.ActivityV1Info;

            reply();
            await ETTask.CompletedTask;
        }
    }
}

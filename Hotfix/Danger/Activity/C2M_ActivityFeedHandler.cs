using System;
using System.Collections.Generic;

namespace ET
{

    [ActorMessageHandler]
    public class C2M_ActivityFeedHandler : AMActorLocationRpcHandler<Unit, C2M_ActivityFeedRequest, M2C_ActivityFeedResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_ActivityFeedRequest request, M2C_ActivityFeedResponse response, Action reply)
        {
            int costItemId = request.ItemID;
            if (!ActivityV1Config.FeedItemReward.TryGetValue(costItemId, out KeyValuePairLong feedReward))
            {
                response.Error = ErrorCode.ERR_ItemNotExist;
                reply();
                return;
            }
            BagComponentServer bagComponentServer = unit.GetComponent<BagComponentServer>();
            ActivityComponentServer activityComponentServer = unit.GetComponent<ActivityComponentServer>();
            if (bagComponentServer.GetBagLeftCell() < 1)
            {
                response.Error = ErrorCode.ERR_BagIsFull;
                reply();
                return;
            }
            if (bagComponentServer.GetItemNumber(ItemBigType.Type_Item, costItemId) < 1)
            {
                response.Error = ErrorCode.ERR_ItemNotEnoughError;
                reply();
                return;
            }

            List<RewardItem> droplist = new List<RewardItem>();
            int dropid = (int)feedReward.KeyId;
            DropHelper.DropIDToDropItem_2(dropid, droplist);

            bagComponentServer.OnCostItemData($"{costItemId};1", ItemLocType.ItemLocBag, ItemGetWay.Activity);
            bagComponentServer.OnAddItemData(droplist, string.Empty, $"{ItemGetWay.Activity}_{TimeHelper.ServerNow()}");

            long activitySceneid = DBHelper.GetActivityServerId(unit);
            A2M_ActivityFeedResponse r_GameStatusResponse = (A2M_ActivityFeedResponse)await ActorMessageSenderComponent.Instance.Call
                 (activitySceneid, new M2A_ActivityFeedRequest()
                 {
                     UnitID = unit.Id,
                     ItemID = request.ItemID,
                 });

            response.ActivityV1Info = activityComponentServer.ActivityV1Info;
            response.ActivityV1Info.BaoShiDu = r_GameStatusResponse.BaoShiDu;
            reply();
            await ETTask.CompletedTask;
        }
    }
}

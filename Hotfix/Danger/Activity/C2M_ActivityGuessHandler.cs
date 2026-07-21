using System;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_ActivityGuessHandler : AMActorLocationRpcHandler<Unit, C2M_ActivityGuessRequest, M2C_ActivityGuessResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_ActivityGuessRequest request, M2C_ActivityGuessResponse response, Action reply)
        {
            long activitySceneid = DBHelper.GetActivityServerId(unit);
            ActivityComponentServer activityComponentServer = unit.GetComponent<ActivityComponentServer>();
            ActivityV1Info activityV1Info = activityComponentServer.ActivityV1Info;
            if (activityV1Info.GuessIds.Contains(request.GuessId))
            {
                response.Error = ErrorCode.ERR_Already_Guess;
                reply();
                return;
            }
            string costItem = ActivityV1Config.GetGuessCostItem(activityV1Info.GuessIds.Count);
            BagComponentServer bag = unit.GetComponent<BagComponentServer>();
            if (!bag.CheckNeedItem(costItem))
            {
                response.Error = ErrorCode.ERR_ItemNotEnoughError;
                reply();
                return;
            }

            A2M_ActivityGuessResponse r_GameStatusResponse = (A2M_ActivityGuessResponse)await ActorMessageSenderComponent.Instance.Call
                   (activitySceneid, new M2A_ActivityGuessRequest()
                   {
                       UnitId = unit.Id,
                       GuessId = request.GuessId,   
                   });
            if (r_GameStatusResponse.Error != ErrorCode.ERR_Success)
            {
                response.Error = r_GameStatusResponse.Error;
                reply();
                return;
            }
            activityV1Info.GuessIds.Add(request.GuessId);
            bag.OnCostItemData(costItem, ItemLocType.ItemLocBag, ItemGetWay.Activity );
            reply();
            await ETTask.CompletedTask;
        }
    }
}

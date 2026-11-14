using System;
using System.Collections.Generic;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_ActivityReceiveHandler : AMActorLocationRpcHandler<Unit, C2M_ActivityReceiveRequest, M2C_ActivityReceiveResponse>
    {

        protected override async ETTask Run(Unit unit, C2M_ActivityReceiveRequest request, M2C_ActivityReceiveResponse response, Action reply)
        {
            if (request.ActivityType != ActivityHelper.RedeemCodeActivityType)
            {
                response.Error = ErrorCode.ERR_ModifyData;
                reply();
                return;
            }

            if (request.ActivityId < 1 || request.ActivityId > 3)
            {
                response.Error = ErrorCode.ERR_SerialNoExist;
                reply();
                return;
            }

            using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.Received, unit.Id))
            {

                ActivityComponentServer activityComponentServer = unit.GetComponent<ActivityComponentServer>();
                if (activityComponentServer.ActivityReceiveIds.Contains(request.ActivityId))
                {
                    response.Error = ErrorCode.ERR_AlreadyReceived;
                    reply();
                    return;
                }

                List<RewardItem> rewards = ItemNewHelper.GetRewardItems("3~4000~10|3~4401~10|3~4501~10|3~4530~10");
                if (!unit.GetComponent<BagComponentServer>().OnAddItemData(rewards, $"{ItemGetWay.Serial}_{TimeHelper.ServerNow()}"))
                {
                    response.Error = ErrorCode.ERR_BagIsFull;
                    reply();
                    return;
                }

                activityComponentServer.ActivityReceiveIds.Add(request.ActivityId);
                unit.GetComponent<DBSaveComponent>().UpdateCacheDB();
                response.ReardList.AddRange(rewards);
                response.Error = ErrorCode.ERR_Success;
            }

            reply();
        }
    }
}


using System;
using System.Collections.Generic;

namespace ET
{

    [ActorMessageHandler]
    public class C2M_TimerChouKaHandler : AMActorLocationRpcHandler<Unit, C2M_TimerChouKaRequest, M2C_TimerChouKaResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_TimerChouKaRequest request, M2C_TimerChouKaResponse response, Action reply)
        {
            BagComponentServer bagComponentServer = unit.GetComponent<BagComponentServer>();
            if (bagComponentServer.GetBagLeftCell() < 1)
            {
                response.Error = ErrorCode.ERR_BagIsFull;
                reply();
                return;
            }
            
            ActivityComponentServer activityComponentServer = unit.GetComponent<ActivityComponentServer>();
            int receNum = activityComponentServer.TimerChouKaReceiveIndex;
            if (receNum >= CommonConfig.TimerChouKaRewardList.Count)
            {
                response.Error = ErrorCode.ERR_AlreadyFinish;
                reply();
                return;
            }

            long passtime = activityComponentServer.LastTimerChouKaPassTime;
            long validTime =  CommonConfig.TimerChouKaRewardList[receNum].Interval * TimeHelper.Minute;

            if (passtime < validTime)
            {
                response.Error = ErrorCode.ERR_NotTimeToGet;
                reply();
                return;
            }

            //List<int> validids = new List<int>();
            //List<int> weights = new List<int>();
            //for (int i = 0; i < ConfigHelper.TimerChouKaRewardList.Count; i++)
            //{
            //    if (!activityComponentServer.TimerChouKaReceiveIds.Contains(i))
            //    {
            //        validids.Add(i);
            //        weights.Add(ConfigHelper.TimerChouKaRewardList[i].Weight);
            //    }
            //}
            //int index = RandomHelper.RandomByWeight(weights);
            //int recvid = validids[index];

            int recvid = activityComponentServer.TimerChouKaReceiveIndex;
            string getitem = CommonConfig.TimerChouKaRewardList[recvid].ItemInfo;
            bagComponentServer.OnAddItemData(getitem, $"{ItemGetWay.ChouKa}_{TimeHelper.ServerNow()}");
            activityComponentServer.TimerChouKaReceiveIndex++;
            activityComponentServer.LastTimerChouKaPassTime = 0;


            response.LastTimerChouKaPassTime = activityComponentServer.LastTimerChouKaPassTime;
            response.TimerChouKaReceiveIndex = activityComponentServer.TimerChouKaReceiveIndex;
            reply();
            await ETTask.CompletedTask;
        }
    }
}

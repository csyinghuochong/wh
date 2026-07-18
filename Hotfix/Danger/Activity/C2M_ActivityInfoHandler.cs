using System;

namespace ET
{

    [ActorMessageHandler]
    public class C2M_ActivityInfoHandler : AMActorLocationRpcHandler<Unit, C2M_ActivityInfoRequest, M2C_ActivityInfoResponse>
    {

        protected override async ETTask Run(Unit unit, C2M_ActivityInfoRequest request, M2C_ActivityInfoResponse response, Action reply)
        {
            ActivityComponentServer activityComponentServer = unit.GetComponent<ActivityComponentServer>();
            if (activityComponentServer.TotalSignNumber == 0)
            {
                for (int i = activityComponentServer.ActivityReceiveIds.Count - 1; i >= 0; i--)
                {
                   
                }
            }

            TaskComponentServer taskComponentServer = unit.GetComponent<TaskComponentServer>();   
            

            //if (ConfigData.V1ActivityList.Contains(ActivityConfigHelper.ActivityV1_Task)
            //    && taskComponent.GetTaskCountryByType(TaskCountryType.ActivityV1).Count == 0)
            //{
            //    taskComponent.InitActivityV1Task(true);
            //}

            response.ReceiveIds = activityComponentServer.ActivityReceiveIds;
            response.LastSignTime = activityComponentServer.LastSignTime;
            response.TotalSignNumber = activityComponentServer.TotalSignNumber;
            response.QuTokenRecvive = activityComponentServer.QuTokenRecvive;
            response.LastLoginTime = activityComponentServer.LastLoginTime;
            response.DayTeHui = activityComponentServer.DayTeHui;
            response.TimerChouKaReceiveIndex = activityComponentServer.TimerChouKaReceiveIndex;
            response.LastTimerChouKaPassTime = activityComponentServer.LastTimerChouKaPassTime;

            ActivityV1Info activityV1Info = activityComponentServer.ActivityV1Info;
            long servertime = TimeHelper.ServerNow();
            if (servertime - activityV1Info.OrderLastFefreshTime >= ActivityV1Config.ActivityOrderRefreshTime)
            {
                activityV1Info.OrderLastFefreshTime = TimeHelper.ServerNow();
                activityV1Info.OrderId  = ActivityV1Config.GenerateActivityOrderId();
            }

            ServerInfo dBServerInfo = ConfigData.ServerInfoList[UnitZoneHelper.GetHomeZone(unit)];
            activityV1Info.ChouKaDropId = dBServerInfo.ChouKaDropId;
            activityV1Info.GuessIds.Clear();

            long activitySceneid = DBHelper.GetActivityServerId(unit);
            A2M_ActivitySelfInfo r_GameStatusResponse = (A2M_ActivitySelfInfo)await ActorMessageSenderComponent.Instance.Call
                   (activitySceneid, new M2A_ActivitySelfInfo()
                   {
                        UnitId = unit.Id,   
                   });
            activityV1Info.GuessIds = r_GameStatusResponse.GuessIds;
            activityV1Info.LastGuessReward = r_GameStatusResponse.LastGuessReward;
            activityV1Info.BaoShiDu = r_GameStatusResponse.BaoShiDu;
            activityV1Info.OpenGuessIds = r_GameStatusResponse.OpenGuessIds;
            response.ActivityV1Info = activityV1Info;

            reply();
            await ETTask.CompletedTask;
        }
    }
}

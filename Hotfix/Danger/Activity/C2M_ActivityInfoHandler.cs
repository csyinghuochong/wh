using System;

namespace ET
{

    [ActorMessageHandler]
    public class C2M_ActivityInfoHandler : AMActorLocationRpcHandler<Unit, C2M_ActivityInfoRequest, M2C_ActivityInfoResponse>
    {

        protected override async ETTask Run(Unit unit, C2M_ActivityInfoRequest request, M2C_ActivityInfoResponse response, Action reply)
        {
            ActivityComponentServer activityComponentServer = unit.GetComponent<ActivityComponentServer>();
            if (activityComponentServer.ActivityInfo.TotalSignNumber == 0)
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
            response.LastSignTime = activityComponentServer.ActivityInfo.LastSignTime;
            response.TotalSignNumber = activityComponentServer.ActivityInfo.TotalSignNumber;
            response.QuTokenRecvive = activityComponentServer.QuTokenRecvive;
            response.LastLoginTime = activityComponentServer.ActivityInfo.LastLoginTime;
            response.DayTeHui = activityComponentServer.ActivityInfo.DayTeHui;
            response.TimerChouKaReceiveIndex = activityComponentServer.TimerChouKaReceiveIndex;
            response.LastTimerChouKaPassTime = activityComponentServer.LastTimerChouKaPassTime;

            long servertime = TimeHelper.ServerNow();
          
            ServerInfo dBServerInfo = ConfigData.ServerInfoList[UnitZoneHelper.GetHomeZone(unit)];
           
            long activitySceneid = DBHelper.GetActivityServerId(unit);
            A2M_ActivitySelfInfo r_GameStatusResponse = (A2M_ActivitySelfInfo)await ActorMessageSenderComponent.Instance.Call
                   (activitySceneid, new M2A_ActivitySelfInfo()
                   {
                        UnitId = unit.Id,   
                   });
        
            reply();
            await ETTask.CompletedTask;
        }
    }
}

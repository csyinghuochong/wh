
namespace ET
{

    [ActorMessageHandler]
    public class G2M_ActivityUpdateHandler : AMActorLocationHandler<Unit, G2M_ActivityUpdate>
    {

        protected override async ETTask Run(Unit unit, G2M_ActivityUpdate message)
        {
            RoleInfoComponentServer roleInfoComponentServer = unit.GetComponent<RoleInfoComponentServer>();
            switch (message.ActivityType)
            {
                case 0:
                    Log.Debug($"OnZeroClockUpdate [零点刷新]: {unit.Id}");
                    RoleInfo roleInfo = roleInfoComponentServer.RoleInfo;
                    unit.GetComponent<RoleDailyDataComponent>()?.OnZeroClockUpdate(true);
                    roleInfoComponentServer.OnHourUpdate(0, true);
                    roleInfoComponentServer.OnZeroClockUpdate(true);
                    TaskComponentServer taskComponentServer = unit.GetComponent<TaskComponentServer>();
                    taskComponentServer.CheckWeeklyUpdate();
                    taskComponentServer.OnZeroClockUpdate(true);
                    ActivityComponentServer activityComponentServer = unit.GetComponent<ActivityComponentServer>();
                    activityComponentServer.OnZeroClockUpdate(roleInfo.Lv);
                    ChengJiuComponentServer chengJiuComponentServer = unit.GetComponent<ChengJiuComponentServer>();
                    chengJiuComponentServer.OnZeroClockUpdate();
                    JiaYuanComponentServer jiaYuanComponentServer = unit.GetComponent<JiaYuanComponentServer>();
                    jiaYuanComponentServer.OnZeroClockUpdate(true);
                    DataCollationComponent dataCollationComponent = unit.GetComponent<DataCollationComponent>();
                    dataCollationComponent.OnZeroClockUpdate(true);
                    break;
                case -1:
                    LocationProxyComponent.Instance.Remove(unit.Id).Coroutine();
                    break;
                default:
                    //if (message.ActivityType == 18  && unit.DomainZone() == 81)
                    //{
                    //    RoleInfoComponent roleInfoComponent = unit.GetComponent<RoleInfoComponentServer>();
                    //    DataCollationComponent dataCollationComponent = unit.GetComponent<DataCollationComponent>();
                    //    ChengJiuComponent chengJiuComponent = unit.GetComponent<ChengJiuComponent>();

                    //    int chengjiuNumber = 0;
                    //    if (chengJiuComponent.ChengJiuCompleteList.Contains(10000002))
                    //    {
                    //        chengjiuNumber++;
                    //    }
                    //    if (chengJiuComponent.ChengJiuCompleteList.Contains(10000003))
                    //    {
                    //        chengjiuNumber++;
                    //    }
                    //    if (chengJiuComponent.ChengJiuCompleteList.Contains(10000004))
                    //    {
                    //        chengjiuNumber++;
                    //    }
                    //    if (chengJiuComponent.ChengJiuCompleteList.Contains(10000005))
                    //    {
                    //        chengjiuNumber++;
                    //    }

                    //    string gongzuoshiInfo = $"账号: {roleInfoComponent.Account}  \t名称：{roleInfoComponent.RoleInfo.Name}  \t等级:{roleInfoComponent.RoleInfo.Level}   \t充值:{dataCollationComponent.Recharge}" +
                    //      $"\t体力:{roleInfoComponent.RoleInfo.PiLao}  \t金币:{roleInfoComponent.RoleInfo.Gold}   \t成就值:{chengJiuComponent.TotalChengJiuPoint}   \t成就任务:{chengjiuNumber}" +
                    //      $"\t拍卖消耗:{dataCollationComponent.GetCostByType(ItemGetWay.PaiMaiBuy)}" +
                    //      $"\t当前主线:{dataCollationComponent.MainTask}  \t角色天数:{roleInfoComponent.GetCrateDay()} \n";

                    //    LogHelper.OnLineInfo(gongzuoshiInfo);    
                    //}

                    roleInfoComponentServer.OnHourUpdate(message.ActivityType, true);
                    break;
            }
   
            await ETTask.CompletedTask;
        }
    }
}

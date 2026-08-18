
using System;

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
                case 5:
                    Console.WriteLine($"OnDailyReset [日清]: {unit.Id}");
                    unit.GetComponent<TaskComponentServer>().CheckWeeklyUpdate();
                    PlayerDailyResetHelper.RunDailyReset(unit, 2);
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
                    break;
            }
   
            await ETTask.CompletedTask;
        }
    }
}

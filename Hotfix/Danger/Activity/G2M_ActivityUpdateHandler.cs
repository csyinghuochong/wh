
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
                    if (ActivityHelper.IsGameWeekResetDay(TimeHelper.ServerNow()))
                    {
                        unit.GetComponent<TaskComponentServer>().OnWeeklyReset(true);
                    }

                    PlayerDailyResetHelper.RunDailyReset(unit, 2);
                    break;
                default:
                    break;
            }
   
            await ETTask.CompletedTask;
        }
    }
}

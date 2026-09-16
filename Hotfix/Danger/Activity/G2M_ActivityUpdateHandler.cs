
using System;

namespace ET
{

    [ActorMessageHandler]
    public class G2M_ActivityUpdateHandler : AMActorLocationHandler<Unit, G2M_ActivityUpdate>
    {

        protected override async ETTask Run(Unit unit, G2M_ActivityUpdate message)
        {
            if (message.ActivityType == ActivityHelper.GetDailyResetHour())
            {
                Console.WriteLine($"OnDailyReset [日清]: {unit.Id}");
                if (ActivityHelper.IsGameWeekResetDay(TimeHelper.ServerNow()))
                {
                    unit.GetComponent<TaskComponentServer>().OnWeeklyReset(true);
                }

                PlayerDailyResetHelper.RunDailyReset(unit, 2);
            }
   
            await ETTask.CompletedTask;
        }
    }
}

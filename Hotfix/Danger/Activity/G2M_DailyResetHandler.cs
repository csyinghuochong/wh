using System;

namespace ET
{
    [ActorMessageHandler]
    public class G2M_DailyResetHandler : AMActorLocationHandler<Unit, G2M_DailyReset>
    {
        protected override async ETTask Run(Unit unit, G2M_DailyReset message)
        {
            Console.WriteLine($"OnDailyReset [日清]: {unit.Id}");
            if (ActivityHelper.IsGameWeekResetDay(TimeHelper.ServerNow()))
            {
                PlayerDailyResetHelper.OnWeeklyReset(unit, true);
            }

            PlayerDailyResetHelper.OnDailyReset(unit, 2);
            await ETTask.CompletedTask;
        }
    }
}

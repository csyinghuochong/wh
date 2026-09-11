namespace ET
{

	[ActorMessageHandler]
    public class C2M_SingingUpdateHandler : AMActorLocationHandler<Unit, C2M_SingingUpdate>
    {
		protected override async ETTask Run(Unit unit, C2M_SingingUpdate message)
		{
			if (message.StateOperateType == 1 && message.StateType == SingingUpdateKind.Singing)
			{
            }

            MessageHelper.Broadcast(unit, new M2C_SingingUpdate()
			{
				UnitId = unit.Id,
				StateType = message.StateType,
				StateOperateType = message.StateOperateType,
				StateTime = message.StateTime,
				StateValue = message.StateValue,
			});
			
			await ETTask.CompletedTask;
		}
	}
}

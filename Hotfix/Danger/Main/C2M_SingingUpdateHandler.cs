namespace ET
{

	[ActorMessageHandler]
    public class C2M_SingingUpdateHandler : AMActorLocationHandler<Unit, C2M_SingingUpdate>
    {
		protected override async ETTask Run(Unit unit, C2M_SingingUpdate message)
		{
			if (message.StateOperateType == 1 && message.StateType == SingingUpdateKind.Singing)
			{
                int buffid = 0;
                string[] stateParts = message.StateValue.Split('_');
                int skillid = int.Parse(stateParts[0]);
			
				CommonConfig.SingingBuffList.TryGetValue(skillid, out buffid);
				if (buffid != 0)
				{
                    BuffData buffData_1 = new BuffData();
                    buffData_1.SkillId = 67000278;
                    buffData_1.BuffId = buffid;
                    BuffManagerComponent buffManagerComponent = unit.GetComponent<BuffManagerComponent>();
                    buffManagerComponent.BuffFactory(buffData_1, unit, null);
                }
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

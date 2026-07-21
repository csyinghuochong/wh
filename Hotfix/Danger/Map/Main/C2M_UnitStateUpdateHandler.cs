namespace ET
{

	[ActorMessageHandler]
    public class C2M_UnitStateUpdateHandler : AMActorLocationHandler<Unit, C2M_UnitStateUpdate>
    {
		protected override async ETTask Run(Unit unit, C2M_UnitStateUpdate message)
		{
			//驭剑的光能击吟唱前可以给自己加buff
			if (message.StateOperateType == 1 &&  message.StateType == StateTypeEnum.Singing)
			{
                //"StateValue":"61022102_0
                int buffid = 0;
                string[] stateParts = message.StateValue.Split('_');
                int skillid = int.Parse(stateParts[0]);
			
				CommonConfig.SingingBuffList.TryGetValue(skillid, out buffid);
				if (buffid != 0)
				{
                    BuffData buffData_1 = new BuffData();
                    buffData_1.SkillId = 67000278;
                    buffData_1.BuffId = buffid;
                    unit.GetComponent<BuffManagerComponent>().BuffFactory(buffData_1, unit, null);
                }
            }

            StateComponent stateComponent = unit.GetComponent<StateComponent>();
            if (message.StateOperateType == 1)
			{
				//增加
				stateComponent.StateTypeAdd(message.StateType, message.StateValue);
			}
			else
			{
				//移除
				stateComponent.StateTypeRemove(message.StateType);
			}
			
			await ETTask.CompletedTask;
		}
	}
}

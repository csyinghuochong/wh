using System;
using System.Collections.Generic;

namespace ET
{
    //游戏设置
    [ActorMessageHandler]
    public class C2M_GameSettingHandler : AMActorLocationRpcHandler<Unit, C2M_GameSettingRequest, M2C_GameSettingResponse>
    {
		protected override async ETTask Run(Unit unit, C2M_GameSettingRequest request, M2C_GameSettingResponse response, Action reply)
		{
			//读取数据库
			RoleInfoComponentServer roleInfoComponentServer = unit.GetComponent<RoleInfoComponentServer>();
			NumericComponent numericComponent = unit.GetComponent<NumericComponent>();
			RoleInfo roleInfo = roleInfoComponentServer.GetUserInfo();

			Dictionary<int, int> settingIndexMap = new Dictionary<int, int>();
			for (int k = 0; k < roleInfo.GameSettingInfos.Count; k++)
			{
				settingIndexMap[roleInfo.GameSettingInfos[k].KeyId] = k;
			}

			for (int i = 0; i < request.GameSettingInfos.Count; i++)
			{
				if (request.GameSettingInfos[i].KeyId == (int)GameSettingEnum.AttackMode)
				{
					int attackMode = int.Parse(request.GameSettingInfos[i].Value);
					numericComponent.ApplyValue(NumericType.AttackMode, attackMode);

					List<Unit> unitlist = unit.GetParent<UnitComponent>().GetAll();
                    for (int u = 0; u < unitlist.Count; u++)
					{
						if (unitlist[u].MasterId == unit.Id)
						{
                            unitlist[u].GetComponent<NumericComponent>().ApplyValue(NumericType.AttackMode, attackMode);
                        }
					}
				}
				if (request.GameSettingInfos[i].KeyId == (int)GameSettingEnum.FirstUnionName)
				{
					//1显示家族称号 2其他称号
                    numericComponent.ApplyValue(NumericType.FirstUnionName, int.Parse(request.GameSettingInfos[i].Value));
                }

				if (settingIndexMap.TryGetValue(request.GameSettingInfos[i].KeyId, out int settingIndex))
				{
					roleInfo.GameSettingInfos[settingIndex].Value = request.GameSettingInfos[i].Value;
				}
				else
				{
					settingIndexMap[request.GameSettingInfos[i].KeyId] = roleInfo.GameSettingInfos.Count;
					roleInfo.GameSettingInfos.Add(request.GameSettingInfos[i]);
				}
			}
			reply();
			await ETTask.CompletedTask;
		}
	}
}

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
			UnitComponent unitComponent = unit.GetParent<UnitComponent>();
			RoleInfo roleInfo = roleInfoComponentServer.GetUserInfo();

			Dictionary<int, int> settingIndexMap = new Dictionary<int, int>();
			for (int k = 0; k < roleInfo.GameSettingInfos.Count; k++)
			{
				settingIndexMap[roleInfo.GameSettingInfos[k].KeyId] = k;
			}

			for (int i = 0; i < request.GameSettingInfos.Count; i++)
			{
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

using System;

namespace ET
{
    //游戏设置
    [ActorMessageHandler]
    public class C2M_GameSettingHandler : AMActorLocationRpcHandler<Unit, C2M_GameSettingRequest, M2C_GameSettingResponse>
    {
		protected override async ETTask Run(Unit unit, C2M_GameSettingRequest request, M2C_GameSettingResponse response, Action reply)
		{
			RoleInfoComponentServer roleInfoComponentServer = unit.GetComponent<RoleInfoComponentServer>();
			roleInfoComponentServer.RoleInfo.UpdateGameSetting(request.GameSettingInfos);
			DBHelper.SaveComponentCache(UnitZoneHelper.GetHomeZone(unit), unit.Id, roleInfoComponentServer).Coroutine();
			reply();
			await ETTask.CompletedTask;
		}
	}
}

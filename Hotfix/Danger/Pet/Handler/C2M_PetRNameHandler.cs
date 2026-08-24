using System;

namespace ET
{
    //玩家宠物
    [ActorMessageHandler]
	public class C2M_PetRNameHandler : AMActorLocationRpcHandler<Unit, C2M_PetRName, M2C_PetRName>
	{
		protected override async ETTask Run(Unit unit, C2M_PetRName request, M2C_PetRName response, Action reply)
		{
			PetComponentServer petComponentServer = unit.GetComponent<PetComponentServer>();
			PetInfo petinfo = petComponentServer.GetPetInfo(request.PetInfoId);
			if (petinfo==null)
			{
				reply();
				return;
			}

			petinfo.PetName = request.PetName;

			//通知客户端
			MessageHelper.SendToClient(unit, new M2C_PetDataUpdate() { UpdateType = (int)UserDataType.Name, PetId = request.PetInfoId, UpdateTypeValue = request.PetName });
			MessageHelper.Broadcast(unit, new M2C_PetDataBroadcast() { UnitId = unit.Id, UpdateType = (int)UserDataType.Name, PetId = request.PetInfoId, UpdateTypeValue = request.PetName });
			reply();
			await ETTask.CompletedTask;
		}
	}
}
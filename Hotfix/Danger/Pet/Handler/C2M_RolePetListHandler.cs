using System;

namespace ET
{
    //玩家宠物
    [ActorMessageHandler]
	public class C2M_RolePetListHandler : AMActorLocationRpcHandler<Unit, C2M_PetList, M2C_PetList>
	{
		protected override async ETTask Run(Unit unit, C2M_PetList request, M2C_PetList response, Action reply)
		{
			PetComponentServer petComponentServer = unit.GetComponent<PetComponentServer>();
			petComponentServer.InitPetInfo();
			response.PetInfos = petComponentServer.GetAllPets();
			response.PetFormations = petComponentServer.PetFormations;
			response.FightPetId = petComponentServer.FightPetId;

            reply();
			await ETTask.CompletedTask;
		}

	}
}
using System;

namespace ET
{
    //玩家宠物
    [ActorMessageHandler]
	public class C2M_RolePetListHandler : AMActorLocationRpcHandler<Unit, C2M_RolePetList, M2C_RolePetList>
	{
		protected override async ETTask Run(Unit unit, C2M_RolePetList request, M2C_RolePetList response, Action reply)
		{
			PetComponentServer petComponentServer = unit.GetComponent<PetComponentServer>();
			petComponentServer.InitPetInfo();
			response.RolePetInfos = petComponentServer.GetAllPets();
			response.TeamPetList = petComponentServer.TeamPetList;
			response.RolePetEggs = petComponentServer.RolePetEggs;
			response.PetFormations = petComponentServer.PetFormations;
			response.PetFubenInfos = petComponentServer.PetFubenInfos;
			response.PetFubeRewardId = petComponentServer.PetFubeRewardId;
			response.PetSkinList = petComponentServer.PetSkinList;
			response.PetShouHuList = petComponentServer.PetShouHuList;
			response.PetShouHuActive = petComponentServer.PetShouHuActive;
            response.PetCangKuOpen = petComponentServer.PetCangKuOpen;
			response.PetMingList = petComponentServer.PetMingList;
			response.PetMingPosition = petComponentServer.PetMingPosition;
			response.RolePetBag = petComponentServer.RolePetBag;
			response.FightPetId = petComponentServer.FightPetId;

            reply();
			await ETTask.CompletedTask;
		}

	}
}
using System;
using System.Collections.Generic;

namespace ET
{
    //玩家宠物
    [ActorMessageHandler]
	public class C2M_PetFightHandler : AMActorLocationRpcHandler<Unit, C2M_PetFight, M2C_PetFight>
	{
		protected override async ETTask Run(Unit unit, C2M_PetFight request, M2C_PetFight response, Action reply)
		{
			PetComponentServer petComponentServer = unit.GetComponent<PetComponentServer>();
			UnitComponent unitComponent = unit.GetParent<UnitComponent>();
            PetInfo petinfo = petComponentServer.GetPetInfo(request.PetInfoId);
			if (petinfo == null)
			{
                response.Error = ErrorCode.ERR_Pet_NoExist;
				reply();
				return;
			}
            if (petinfo.PetStatus == 2 || petinfo.PetStatus == 3)
            {
                reply();
                return;
            }

            if (request.PetStatus == 1)
            {
                //出战要清掉之前的
                PetInfo fightpet = petComponentServer.GetFightPet();
                if (fightpet != null)
                {
                    fightpet.PetStatus = 0;
                    unitComponent.Remove(fightpet.Id);
                }
                Unit existingPetUnit = unitComponent.Get(petinfo.Id);
                if (existingPetUnit == null)
                {
                    petComponentServer.UpdatePetAttribute(petinfo, false);
                    UnitFactory.CreatePet(unit, petinfo);
                }

                petinfo.PetStatus = request.PetStatus;
                petComponentServer.FightPetId = request.PetInfoId;
            }
            else
            {
                //休息
                petinfo.PetStatus = request.PetStatus;
                petComponentServer.FightPetId = 0;
                unitComponent.Remove(petinfo.Id);
            }

         
            reply();
			await ETTask.CompletedTask;
		}
	}
}
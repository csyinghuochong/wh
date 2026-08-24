using System;
using System.Collections.Generic;

namespace ET
{
    //玩家宠物
    [ActorMessageHandler]
	public class C2M_PetFightHandler : AMActorLocationRpcHandler<Unit, C2M_RolePetFight, M2C_RolePetFight>
	{
		protected override async ETTask Run(Unit unit, C2M_RolePetFight request, M2C_RolePetFight response, Action reply)
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

            ///移除有问题的宠物
            //List<Unit> entities = unit.GetParent<UnitComponent>().GetAll();
            //{
            //	for (int i = entities.Count - 1; i >= 0; i--)
            //	{
            //                    if (entities[i].Id == petinfo.Id)
            //                    {
            //                        continue;
            //                    }
            //                    if (entities[i].Type != UnitType.Pet)
            //		{
            //			continue;
            //		}
            //		if (entities[i].GetMasterId() != unit.Id)
            //		{
            //                        continue;
            //                    }

            //                    unit.GetParent<UnitComponent>().Remove(entities[i].Id);
            //                }
            //}

            reply();
			await ETTask.CompletedTask;
		}
	}
}
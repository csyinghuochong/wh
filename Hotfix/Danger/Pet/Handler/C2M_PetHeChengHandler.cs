using System;

namespace ET
{
    [ActorMessageHandler]
	public class C2M_PetHeChengHandler : AMActorLocationRpcHandler<Unit, C2M_PetHeCheng, M2C_PetHeCheng>
	{
		protected override async ETTask Run(Unit unit, C2M_PetHeCheng request, M2C_PetHeCheng response, Action reply)
		{
			PetComponentServer petComponentServer = unit.GetComponent<PetComponentServer>();
			ChengJiuComponentServer chengJiuComponentServer = unit.GetComponent<ChengJiuComponentServer>();
			TaskComponentServer taskComponentServer = unit.GetComponent<TaskComponentServer>();

			PetInfo petinfo_1 = petComponentServer.GetPetInfo(request.PetInfoId1);
            PetInfo petinfo_2 = petComponentServer.GetPetInfo(request.PetInfoId2);
			if (petinfo_1 == null || petinfo_2 == null || petinfo_1.Id == petinfo_2.Id)
			{
				response.Error = ErrorCode.ERR_Pet_NoExist;
				reply();
				return;
			}

            if (petinfo_1.PetStatus == 1 || petinfo_2.PetStatus == 1)
            {
                response.Error = ErrorCode.ERR_Pet_HeCheng_FightError;
                reply();
                return;
            }

			// Pet表备注：合体前对副宠进行重置，把药退出来
			if (!petComponentServer.ResetSubPetRefundItems(petinfo_2))
			{
				response.Error = ErrorCode.ERR_BagIsFull;
				reply();
				return;
			}

			int error = PetHelper.HeCheng(petinfo_1, petinfo_2);
			if (error != ErrorCode.ERR_Success)
			{
				response.Error = error;
				reply();
				return;
			}

			long deletePetId = petinfo_2.Id;
			petComponentServer.RemovePet(deletePetId, 1);
			petComponentServer.UpdatePetAttribute(petinfo_1, true);
			chengJiuComponentServer.OnPetHeCheng(petinfo_1);
			taskComponentServer.OnPetHeCheng(petinfo_1);


			response.PetInfo = petinfo_1;
			response.DeletePetInfoId = deletePetId;
			reply();
			await ETTask.CompletedTask;
		}
	}
}

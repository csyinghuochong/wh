using System;
using System.Collections.Generic;
using UnityEngine;

namespace ET
{
    //玩家宠物
    [ActorMessageHandler]
	public class C2M_PetHeChengHandler : AMActorLocationRpcHandler<Unit, C2M_PetHeCheng, M2C_PetHeCheng>
	{
		protected override async ETTask Run(Unit unit, C2M_PetHeCheng request, M2C_PetHeCheng response, Action reply)
		{
			//读取数据库
			PetComponentServer petComponentServer = unit.GetComponent<PetComponentServer>();
			DataCollationComponent dataCollationComponent = unit.GetComponent<DataCollationComponent>();
			ChengJiuComponentServer chengJiuComponentServer = unit.GetComponent<ChengJiuComponentServer>();
			TaskComponentServer taskComponentServer = unit.GetComponent<TaskComponentServer>();

			PetInfo petinfo_1 = petComponentServer.GetPetInfo(request.PetInfoId1);
            PetInfo petinfo_2 = petComponentServer.GetPetInfo(request.PetInfoId2);
			if (petinfo_1 == null || petinfo_2 == null)
			{
				response.Error = ErrorCode.ERR_Pet_NoExist;
				reply();
				return;
			}
            if (petinfo_1.PetStatus == 1 || petinfo_2.PetStatus == 1)
            {
                response.Error = ErrorCode.ERR_Pet_Hint_4;
                reply();
                return;
            }
			//错误码

			int petHeChengNumber = dataCollationComponent.PetHeCheng;

            petComponentServer.OnPetScoreChanged();
            Function_Fight.UnitUpdateProperty_Base(unit, true, true);
			response.DeletePetInfoId = -1;

			await ETTask.CompletedTask;
		}

	}
}
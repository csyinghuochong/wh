using System;

namespace ET
{
    //玩家宠物
    [ActorMessageHandler]
	public class C2M_PetFenjieHandler : AMActorLocationRpcHandler<Unit, C2M_PetFenjie, M2C_PetFenjie>
	{
		protected override async ETTask Run(Unit unit, C2M_PetFenjie request, M2C_PetFenjie response, Action reply)
		{
			PetComponentServer pet = unit.GetComponent<PetComponentServer>();
			BagComponentServer bagComponentServer = unit.GetComponent<BagComponentServer>();
			JiaYuanComponentServer jiaYuanComponentServer = unit.GetComponent<JiaYuanComponentServer>();
			UnitComponent unitComponent = unit.GetParent<UnitComponent>();
			//判断背包是否满
			if (bagComponentServer.GetBagLeftCell() <= 1)
			{
				response.Error = ErrorCode.ERR_BagIsFull;       //提示背包已满
				reply();
				return;
			}


			int petType = 1;
			PetInfo rolePetInfo = pet.GetPetInfo(request.PetInfoId);

	
			if (rolePetInfo == null)
			{
				response.Error = ErrorCode.ERR_Pet_NoExist;
				reply();
				return;
			}
            if (rolePetInfo.PetStatus != 0)
			{
                response.Error = ErrorCode.ERR_Pet_Hint_4;
                reply();
                return;
            }

            //获取宠物碎片
            LDPet ldPetCof = LDPetCategory.Instance.Get(rolePetInfo.ConfigId);
			/*if (ldPetCof.ReleaseReward != null && ldPetCof.ReleaseReward.Length > 2)
			{
				unit.GetComponent<BagComponentServer>().OnAddItemData(ldPetCof.ReleaseReward, $"{ItemGetWay.PetFenjie}_{TimeHelper.ServerNow()}");
			}
			*/

			if (petType == 1)
			{
                pet.OnRolePetFenjie(request.PetInfoId);
            }
			else
			{
                
            }

			
			jiaYuanComponentServer.OnJiaYuanPetWalk(rolePetInfo, 0, -1);
			Unit existingPetUnit = unitComponent.Get(rolePetInfo.Id);
			if (existingPetUnit != null)
			{
				Log.Warning($"宠物还在出战中！！");
				unitComponent.Remove(rolePetInfo.Id);
			}

			Function_Fight.UnitUpdateProperty_Base( unit, true, true );

            reply();
			await ETTask.CompletedTask;
		}
	}
}
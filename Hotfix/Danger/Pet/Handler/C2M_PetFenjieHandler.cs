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
			HomeComponentServer homeComponentServer = unit.GetComponent<HomeComponentServer>();
			UnitComponent unitComponent = unit.GetParent<UnitComponent>();
			//判断背包是否满
			if (bagComponentServer.GetBagLeftCell() <= 1)
			{
				response.Error = LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Bag_Full);       //提示背包已满
				reply();
				return;
			}


			int petType = 1;
			PetInfo rolePetInfo = pet.GetPetInfo(request.PetInfoId);

	
			if (rolePetInfo == null)
			{
				response.Error = LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_Operation_Fail);
				reply();
				return;
			}
            if (rolePetInfo.PetStatus != 0)
			{
                response.Error = LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_Operation_Fail);
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
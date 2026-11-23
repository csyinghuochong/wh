using System;

namespace ET
{

	/// <summary>
	/// 宠物洗练保存
	/// </summary>
	[ActorMessageHandler]
	public class C2M_PetRefinedSaveHandler : AMActorLocationRpcHandler<Unit, C2M_PetRefinedSave, M2C_PetRefinedSave>
	{
		protected override async ETTask Run(Unit unit, C2M_PetRefinedSave request, M2C_PetRefinedSave response, Action reply)
		{
			PetComponentServer pet = unit.GetComponent<PetComponentServer>();
			PetInfo petInfo = pet.GetPetInfo(request.PetInfoId);
			if (petInfo == null)
			{
				response.Error = LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_Operation_Fail);
				reply();
				return;
			}

			if (!PetHelper.SaveXiLianResult(petInfo))
			{
				response.Error = LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_Operation_Fail);
				reply();
				return;
			}

			pet.UpdatePetAttribute(petInfo, true);
			response.PetInfo = petInfo;
			reply();
			await ETTask.CompletedTask;
		}
	}
}

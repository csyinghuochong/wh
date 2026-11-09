using System;

namespace ET
{
	[ActorMessageHandler]
	public class C2M_PetLearnSkillHandler : AMActorLocationRpcHandler<Unit, C2M_PetLearnSkill, M2C_PetLearnSkill>
	{
		protected override async ETTask Run(Unit unit, C2M_PetLearnSkill request, M2C_PetLearnSkill response, Action reply)
		{
			PetComponentServer pet = unit.GetComponent<PetComponentServer>();
			BagComponentServer bag = unit.GetComponent<BagComponentServer>();
			PetInfo petInfo = pet.GetPetInfo(request.PetInfoId);
			if (petInfo == null)
			{
				response.Error = ErrorCode.ERR_Pet_NoExist;
				reply();
				return;
			}

			BagInfo bagInfo = bag.GetItemByUId(request.BagInfoID);
			if (bagInfo == null || !ItemNewHelper.IsValideBagLoc(bagInfo.Loc))
			{
				response.Error = ErrorCode.ERR_ItemNotExist;
				reply();
				return;
			}

			if (!LDItemCategory.Instance.Contain(bagInfo.ItemID))
			{
				response.Error = ErrorCode.ERR_ItemNotExist;
				reply();
				return;
			}

			LDItem ldItem = LDItemCategory.Instance.Get(bagInfo.ItemID);
			if (ldItem.ItemType != ItemTypeEnum.SubType_PetSkillBook_39)
			{
				response.Error = ErrorCode.ERR_Pet_NoUseItem;
				reply();
				return;
			}

			if (bagInfo.ItemNum < 1)
			{
				response.Error = ErrorCode.ERR_ItemNotEnoughError;
				reply();
				return;
			}

			int bookError = PetHelper.LearnSkillByBook(petInfo, ldItem.GetTypeParam1());
			if (bookError != ErrorCode.ERR_Success)
			{
				response.Error = bookError;
				reply();
				return;
			}

			if (!bag.OnCostItemData($"{ItemBigType.Type_Item}~{bagInfo.ItemID}~1", (ItemLocType)bagInfo.Loc, ItemGetWay.PetHeXinExplore))
			{
				response.Error = ErrorCode.ERR_ItemNotEnoughError;
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

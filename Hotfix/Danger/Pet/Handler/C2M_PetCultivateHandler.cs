using System;

namespace ET
{


	/// <summary>
	/// 宠物培养
	/// </summary>
	[ActorMessageHandler]
	public class C2M_PetCultivateHandler : AMActorLocationRpcHandler<Unit, C2M_PetCultivate, M2C_PetCultivate>
	{
		protected override async ETTask Run(Unit unit, C2M_PetCultivate request, M2C_PetCultivate response, Action reply)
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
			if (ldItem.ItemType != ItemTypeEnum.SubType_PetZiZhi_G_38 && ldItem.ItemType != ItemTypeEnum.SubType_PetZiZhi_F_380)
			{
				response.Error = ErrorCode.ERR_Pet_NoUseItem;
				reply();
				return;
			}

			int costNum = request.CostItemNum > 0 ? request.CostItemNum : 1;
			if (bagInfo.ItemNum < costNum)
			{
				response.Error = ErrorCode.ERR_ItemNotEnoughError;
				reply();
				return;
			}

			if (ldItem.SumUseNum > 0 && PetHelper.GetEatItemNum(petInfo, ldItem.Id) + costNum > ldItem.SumUseNum)
			{
				response.Error = ErrorCode.ERR_ItemNoUseTime;
				reply();
				return;
			}

			int eatError = PetHelper.EatAptitudeItem(petInfo, ldItem, costNum);
			if (eatError != ErrorCode.ERR_Success)
			{
				response.Error = eatError;
				reply();
				return;
			}

			if (!bag.OnCostItemData($"{ItemBigType.Type_Item}~{bagInfo.ItemID}~{costNum}", (ItemLocType)bagInfo.Loc, ItemGetWay.ItemXiLian))
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

using System;

namespace ET
{


	/// <summary>
	/// 宠物洗练
	/// </summary>
	[ActorMessageHandler]
	public class C2M_PetRefinedHandler : AMActorLocationRpcHandler<Unit, C2M_PetRefined, M2C_PetRefined>
	{
		protected override async ETTask Run(Unit unit, C2M_PetRefined request, M2C_PetRefined response, Action reply)
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
			if (ldItem.ItemType != ItemTypeEnum.SubType_PetXiLian_35)
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

			RoleDailyDataComponentServer daily = unit.GetComponent<RoleDailyDataComponentServer>();
			RoleInfoComponentServer roleInfo = unit.GetComponent<RoleInfoComponentServer>();
			int useError = daily.CheckDayItemUse(ldItem, costNum);
			if (useError != ErrorCode.ERR_Success)
			{
				response.Error = useError;
				reply();
				return;
			}

			useError = roleInfo.CheckTotalItemUse(ldItem, costNum);
			if (useError != ErrorCode.ERR_Success)
			{
				response.Error = useError;
				reply();
				return;
			}

			PetRefinedResult result = PetHelper.BuildXiLianResult(petInfo);
			if (result == null)
			{
				response.Error = ErrorCode.ERR_Pet_NoUseItem;
				reply();
				return;
			}

			if (!bag.OnCostItemData($"{ItemBigType.Type_Item}~{bagInfo.ItemID}~{costNum}", (ItemLocType)bagInfo.Loc, ItemGetWay.ItemXiLian))
			{
				response.Error = ErrorCode.ERR_ItemNotEnoughError;
				reply();
				return;
			}

			daily.AddDayItemUse(ldItem.Id, costNum);
			roleInfo.AddTotalItemUse(ldItem.Id, costNum);
			petInfo.XiLianResult = result;
			unit.GetComponent<TaskComponentServer>().OnPetXiLian(petInfo);
			response.PetInfo = petInfo;
			response.XiLianResult = result;
			reply();
			await ETTask.CompletedTask;
		}
	}
}

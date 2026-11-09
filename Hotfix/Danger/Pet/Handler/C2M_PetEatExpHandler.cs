using System;

namespace ET
{
	[ActorMessageHandler]
	public class C2M_PetEatExpHandler : AMActorLocationRpcHandler<Unit, C2M_PetEatExp, M2C_PetEatExp>
	{
		protected override async ETTask Run(Unit unit, C2M_PetEatExp request, M2C_PetEatExp response, Action reply)
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
			if (ldItem.ItemType != ItemTypeEnum.SubType_Pet_Exp_36)
			{
				response.Error = ErrorCode.ERR_Pet_NoUseItem;
				reply();
				return;
			}

			int costNum = request.CostItemNum > 0 ? request.CostItemNum : 1;
			int addExp = ldItem.GetTypeParam1() * costNum;
			if (addExp <= 0)
			{
				response.Error = ErrorCode.ERR_Pet_NoUseItem;
				reply();
				return;
			}

			if (bagInfo.ItemNum < costNum)
			{
				response.Error = ErrorCode.ERR_ItemNotEnoughError;
				reply();
				return;
			}

			if (!bag.OnCostItemData($"{ItemBigType.Type_Item}~{bagInfo.ItemID}~{costNum}", (ItemLocType)bagInfo.Loc, ItemGetWay.ItemXiLian))
			{
				response.Error = ErrorCode.ERR_ItemNotEnoughError;
				reply();
				return;
			}

			AddExp(unit, pet, petInfo, addExp);
			response.PetInfo = petInfo;
			reply();
			await ETTask.CompletedTask;
		}

		static void AddExp(Unit unit, PetComponentServer pet, PetInfo petInfo, int addExp)
		{
			int newExp = petInfo.PetExp + addExp;
			int playerLv = unit.GetComponent<RoleInfoComponentServer>().RoleInfo.Lv;
			int levelCap = playerLv + 5;
			int maxLv = LDExp_LvCategory.Instance.GetAll().Values.Count;
			while (petInfo.PetLv < maxLv && petInfo.PetLv < levelCap && LDExp_LvCategory.Instance.Contain(petInfo.PetLv))
			{
				int need = LDExp_LvCategory.Instance.Get(petInfo.PetLv).Exp_Role;
				if (need <= 0 || newExp < need)
				{
					break;
				}

				newExp -= need;
				pet.PetAddLv(petInfo, 1);
			}

			petInfo.PetExp = newExp;
			MessageHelper.SendToClient(unit, new M2C_PetDataUpdate()
			{
				UpdateType = (int)UserDataType.Exp,
				PetId = petInfo.Id,
				UpdateTypeValue = petInfo.PetExp.ToString()
			});
		}
	}
}

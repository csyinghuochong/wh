using System;
using System.Collections.Generic;

namespace ET
{
    //玩家宠物
    [ActorMessageHandler]
	public class C2M_PetXiLianHandler : AMActorLocationRpcHandler<Unit, C2M_PetXiLian, M2C_PetXiLian>
	{
		protected override async ETTask Run(Unit unit, C2M_PetXiLian request, M2C_PetXiLian response, Action reply)
		{
			//读取数据库
			PetComponentServer pet = unit.GetComponent<PetComponentServer>();
			BagComponentServer bag = unit.GetComponent<BagComponentServer>();
			ChengJiuComponentServer chengJiuComponentServer = unit.GetComponent<ChengJiuComponentServer>();
			TaskComponentServer taskComponentServer = unit.GetComponent<TaskComponentServer>();
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

			bool ifCost = false;
			int costNum = 1;
			int itemGetWay = ItemGetWay.PetHeXinExplore;

			// 97 打书；96 洗练石；94 吃 F；95 吃 G。资质洗练（重随 D/E）后续再接。
			LDItem ldItem = LDItemCategory.Instance.Get(bagInfo.ItemID);
			int itemType = ldItem.ItemType;

            switch (itemType)
			{
				case ItemTypeEnum.SubType_PetSkillBook_39:
					// Pet表备注：消耗技能书学习技能：已有技能或已有技能的低级不能学。有空槽则占空槽。没有空槽则随机顶掉一个已有技能（目前完全随机）
					int bookError = PetHelper.LearnSkillByBook(petInfo, ldItem.ItemTypeParam1);
					if (bookError != ErrorCode.ERR_Success)
					{
						response.Error = bookError;
						reply();
						return;
					}
					ifCost = true;
                    break;
				case ItemTypeEnum.SubType_PetXiSun_31:
					costNum = request.CostItemNum > 0 ? request.CostItemNum : 1;
					itemGetWay = ItemGetWay.ItemXiLian;
					if (bagInfo.ItemNum < costNum)
					{
						response.Error = ErrorCode.ERR_ItemNotEnoughError;
						reply();
						return;
					}
					// Pet表备注：洗练时，会同时随机 槽位 和 A
					int xiLianError = PetHelper.XiLianExtraSkills(petInfo);
					if (xiLianError != ErrorCode.ERR_Success)
					{
						response.Error = xiLianError;
						reply();
						return;
					}
					ifCost = true;
					break;
				case ItemTypeEnum.SubType_PetZiZhi_G_38:
				case ItemTypeEnum.SubType_PetZiZhi_F_380:
					// Pet表备注：稀有道具记 F，超稀有道具记 G；达最终值后不能再吃对应道具。吃过记 EatItems
					costNum = request.CostItemNum > 0 ? request.CostItemNum : 1;
					itemGetWay = ItemGetWay.ItemXiLian;
					if (bagInfo.ItemNum < costNum)
					{
						response.Error = ErrorCode.ERR_ItemNotEnoughError;
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
					ifCost = true;
					break;
				default:
					response.Error = ErrorCode.ERR_Pet_NoUseItem;
					reply();
					return;
			}


            //扣除相关道具
            if (ifCost)
			{
				bag.OnCostItemData($"{bagInfo.ItemID};{costNum}", (ItemLocType)bagInfo.Loc, itemGetWay);		
				chengJiuComponentServer.OnPetXiLian(petInfo);		//激活成就
				taskComponentServer.OnPetXiLian(petInfo);                    //激活任务

            }
   
            pet.UpdatePetAttribute(petInfo, true);
			response.PetInfo = petInfo;

            reply();
			await ETTask.CompletedTask;
		}

	}
}
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
			BagInfo bagInfo = bag.GetItemByLoc(ItemLocType.ItemLocBag, request.BagInfoID);

			bool ifCost = false;

            //扣除相关道具
            if (ifCost)
			{
				//扣除道具
				bag.OnCostItemData($"{bagInfo.ItemID};1", ItemLocType.ItemLocBag, ItemGetWay.PetHeXinExplore);		
				chengJiuComponentServer.OnPetXiLian(petInfo);		//激活成就
				taskComponentServer.OnPetXiLian(petInfo);                    //激活任务

            }
            pet.OnPetScoreChanged();

            reply();
			await ETTask.CompletedTask;
		}

	}
}
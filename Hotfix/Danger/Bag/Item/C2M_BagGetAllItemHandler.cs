using System;
using System.Collections.Generic;

namespace ET
{
	//游戏背包
	[ActorMessageHandler]
	public class C2M_BagGetAllItemHandler : AMActorLocationRpcHandler<Unit, C2M_BagGetAllItemRequest, M2C_BagGetAllItemResponse>
	{
		protected override async ETTask Run(Unit unit, C2M_BagGetAllItemRequest request, M2C_BagGetAllItemResponse response, Action reply)
		{

			BagComponentServer bagComponentServer = unit.GetComponent<BagComponentServer>();



            //读取数据库
            int occ = unit.GetComponent<RoleInfoComponentServer>().RoleInfo.Occ;
            int occTwo = unit.GetComponent<RoleInfoComponentServer>().RoleInfo.OccTwo;

            // 老号补齐默认时装（幂等），保证拉背包时 FashionActiveList 里一定有默认时装
            bagComponentServer.InitDefaultFashion(occ, unit.GetComponent<RoleInfoComponentServer>().RoleInfo.Sex);

            List<BagInfo> bagInfos = bagComponentServer.GetAllItems();

			
			
			//初始化
			for (int i = 0; i < bagInfos.Count; i++)
			{
			
			}


            List<BagInfo> equipList = bagComponentServer.GetItemByLoc(ItemLocType.ItemLocEquip);
            /*List<BagInfo> equipList_2 = bagComponentServer.GetItemByLoc(ItemLocType.ItemLocEquip_2);*/

            if (bagComponentServer.FashionEquipList.Count == 0)
			{
                LDOccupation ldOccupationConfig = LDOccupationCategory.Instance.Get(occ);
             
			}
           
            List<int> fashionTypes = new List<int>();
			for (int i = bagComponentServer.FashionEquipList.Count - 1; i >= 0; i--)
			{
				if(!LDFashionCategory.Instance.Contain(bagComponentServer.FashionEquipList[i]))
				{
                    bagComponentServer.FashionEquipList.RemoveAt(i);	
                    continue;
                }

				LDFashion ldFashion = LDFashionCategory.Instance.Get(bagComponentServer.FashionEquipList[i]);
			
            }
			for (int i = bagComponentServer.FashionActiveList.Count - 1; i >= 0; i--)
			{
				if (!LDFashionCategory.Instance.Contain(bagComponentServer.FashionActiveList[i].FashionId))
				{
					bagComponentServer.FashionActiveList.RemoveAt(i);
					continue;
				}
			}

            response.BagInfos = bagInfos;
			response.FashionActiveList = bagComponentServer.FashionActiveList;	
			response.FashionEquipList = bagComponentServer.FashionEquipList;
            response.AddedCellNum = BagCellNumHelper.ToProto(bagComponentServer.AddedCellNum);
            response.CangKuNumber = bagComponentServer.CangKuNumber;
            reply();
			await ETTask.CompletedTask;
		}
	}
}
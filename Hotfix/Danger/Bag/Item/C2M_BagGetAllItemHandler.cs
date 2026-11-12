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

            RoleInfo roleInfo = unit.GetComponent<RoleInfoComponentServer>().RoleInfo;

            // 老号补齐默认时装（幂等），保证 FashionActiveList 有记录、FashionEquipList 是完整一套
            bagComponentServer.InitDefaultFashion(occ, roleInfo.Sex);

            List<BagInfo> bagInfos = bagComponentServer.GetAllItems();

			
			
			//初始化
			for (int i = 0; i < bagInfos.Count; i++)
			{
			
			}


            List<BagInfo> equipList = bagComponentServer.GetItemByLoc(ItemLocType.ItemLocEquip);
            /*List<BagInfo> equipList_2 = bagComponentServer.GetItemByLoc(ItemLocType.ItemLocEquip_2);*/

            // 默认整套由 InitDefaultFashion -> EnsureFashionEquipList 补进 FashionEquipList，
            // 下发的穿戴列表不为空、不缺部位，客户端直接渲染即可。
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

            // 上面的清理循环可能移除无效项空出部位，下发前再补齐一次，保证客户端拿到完整一套
            bagComponentServer.EnsureFashionEquipList(occ, roleInfo.Sex);

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
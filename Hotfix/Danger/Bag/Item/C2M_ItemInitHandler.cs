using System;
using System.Collections.Generic;

namespace ET
{
	//游戏背包
	[ActorMessageHandler]
	public class C2M_ItemInitHandler : AMActorLocationRpcHandler<Unit, C2M_ItemInitRequest, M2C_ItemInitResponse>
	{
		protected override async ETTask Run(Unit unit, C2M_ItemInitRequest request, M2C_ItemInitResponse response, Action reply)
		{
			BagComponentServer bagComponentServer = unit.GetComponent<BagComponentServer>();

			//测试 送一个转职道具
			//if (ComHelp.IsInnerNet() && bagComponentServer.GetItemNumber(90000014) < 10)
			//{
   //             bagComponentServer.OnAddItemData($"90000014;10", $"{ItemGetWay.GM}_{TimeHelper.ServerNow()}", false);
   //         }

            //读取数据库
            int occ = unit.GetComponent<RoleInfoComponentServer>().RoleInfo.Occ;
            int occTwo = unit.GetComponent<RoleInfoComponentServer>().RoleInfo.OccTwo;
            List<BagInfo> bagInfos = bagComponentServer.GetAllItems(occ, occTwo);

			/*
			for (int i = 0; i < bagInfos.Count; i++) {
				Log.Info("道具ID：" + bagInfos[i]  + bagInfos[i].GetWay);
			}
			*/

			
			//初始化
			for (int i = 0; i < bagInfos.Count; i++)
			{
				if (bagInfos[i].FumoProLists.Count > 0
					&& bagInfos[i].FumoProLists[0].HideValue > 10000)
				{
					bagInfos[i].FumoProLists.Clear();
				}
				if (string.IsNullOrEmpty(bagInfos[i].GemIDNew))
				{
					bagInfos[i].GemIDNew = ItemHelper.DefaultGem;
                    bagInfos[i].GemHole = ItemHelper.DefaultGem;
                }

				//鉴定符错误
				//Item Item = ItemCategory.Instance.Get(bagInfos[i].ItemID);
				//if(Item.ItemSubType == 121)
				//{
				//	try
				//	{
				//		int quality = int.Parse(bagInfos[i].ItemPar);
				//	}
				//	catch (Exception ex)
				//	{
				//		Log.Debug(ex.ToString()+ "_____" + bagInfos[i].ItemPar);
				//	}
				//	bagInfos[i].ItemPar = "99";
				//}
			}


            List<BagInfo> equipList = bagComponentServer.GetItemByLoc(ItemLocType.ItemLocEquip);
            /*List<BagInfo> equipList_2 = bagComponentServer.GetItemByLoc(ItemLocType.ItemLocEquip_2);*/

            if (bagComponentServer.FashionEquipList.Count == 0)
			{
                LDOccupation ldOccupationConfig = LDOccupationCategory.Instance.Get(occ);
                if (ldOccupationConfig.FashionBase != null)
                {
	                for (int i = 0; i < ldOccupationConfig.FashionBase.Length; i++)
	                {
		                bagComponentServer.FashionEquipList.Add(ldOccupationConfig.FashionBase[i]);
	                }
                }
			}
            if (bagComponentServer.FashionActiveIds.Count == 0)
            {
                LDOccupation ldOccupationConfig = LDOccupationCategory.Instance.Get(occ);
                if (ldOccupationConfig.FashionBase != null)
                {
	                for (int i = 0; i < ldOccupationConfig.FashionBase.Length; i++)
	                {
		                bagComponentServer.FashionActiveIds.Add(ldOccupationConfig.FashionBase[i]);
	                }
                }
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
			for (int i = bagComponentServer.FashionActiveIds.Count - 1; i >= 0; i--)
			{
				if (!LDFashionCategory.Instance.Contain(bagComponentServer.FashionActiveIds[i]))
				{
					bagComponentServer.FashionActiveIds.RemoveAt(i);
					continue;
				}
			}

            response.BagInfos = bagInfos;
			//response.BagAddedCell = bagComponentServer.BagAddedCell;
			response.WarehouseAddedCell = bagComponentServer.WarehouseAddedCell;
			response.FashionActiveIds = bagComponentServer.FashionActiveIds;	
			response.FashionEquipList = bagComponentServer.FashionEquipList;
            response.SeasonJingHePlan = bagComponentServer.SeasonJingHePlan;
			response.AdditionalCellNum = bagComponentServer.AdditionalCellNum;	
            reply();
			await ETTask.CompletedTask;
		}
	}
}
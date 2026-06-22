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
			BagComponent bagComponent = unit.GetComponent<BagComponent>();

			//测试 送一个转职道具
			//if (ComHelp.IsInnerNet() && bagComponent.GetItemNumber(90000014) < 10)
			//{
   //             bagComponent.OnAddItemData($"90000014;10", $"{ItemGetWay.GM}_{TimeHelper.ServerNow()}", false);
   //         }

            //读取数据库
            int occ = unit.GetComponent<RoleInfoComponent>().RoleInfo.Occ;
            int occTwo = unit.GetComponent<RoleInfoComponent>().RoleInfo.OccTwo;
            List<BagInfo> bagInfos = bagComponent.GetAllItems(occ, occTwo);

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


            List<BagInfo> equipList = bagComponent.GetItemByLoc(ItemLocType.ItemLocEquip);
            /*List<BagInfo> equipList_2 = bagComponent.GetItemByLoc(ItemLocType.ItemLocEquip_2);*/

            if (bagComponent.FashionEquipList.Count == 0)
			{
                LDOccupation ldOccupationConfig = LDOccupationCategory.Instance.Get(occ);
                if (ldOccupationConfig.FashionBase != null)
                {
	                for (int i = 0; i < ldOccupationConfig.FashionBase.Length; i++)
	                {
		                bagComponent.FashionEquipList.Add(ldOccupationConfig.FashionBase[i]);
	                }
                }
			}
            if (bagComponent.FashionActiveIds.Count == 0)
            {
                LDOccupation ldOccupationConfig = LDOccupationCategory.Instance.Get(occ);
                if (ldOccupationConfig.FashionBase != null)
                {
	                for (int i = 0; i < ldOccupationConfig.FashionBase.Length; i++)
	                {
		                bagComponent.FashionActiveIds.Add(ldOccupationConfig.FashionBase[i]);
	                }
                }
            }

            List<int> fashionTypes = new List<int>();
			for (int i = bagComponent.FashionEquipList.Count - 1; i >= 0; i--)
			{
				if(!LDFashionCategory.Instance.Contain(bagComponent.FashionEquipList[i]))
				{
                    bagComponent.FashionEquipList.RemoveAt(i);	
                    continue;
                }

				LDFashion ldFashion = LDFashionCategory.Instance.Get(bagComponent.FashionEquipList[i]);
				if (fashionTypes.Contains(ldFashion.SubType))
				{
                    fashionTypes.RemoveAt(i);	
                    continue;
				}

				fashionTypes.Add(ldFashion.SubType);
            }
			for (int i = bagComponent.FashionActiveIds.Count - 1; i >= 0; i--)
			{
				if (!LDFashionCategory.Instance.Contain(bagComponent.FashionActiveIds[i]))
				{
					bagComponent.FashionActiveIds.RemoveAt(i);
					continue;
				}
			}

            response.BagInfos = bagInfos;
			//response.BagAddedCell = bagComponent.BagAddedCell;
			response.WarehouseAddedCell = bagComponent.WarehouseAddedCell;
			response.FashionActiveIds = bagComponent.FashionActiveIds;	
			response.FashionEquipList = bagComponent.FashionEquipList;
            response.SeasonJingHePlan = bagComponent.SeasonJingHePlan;
			response.AdditionalCellNum = bagComponent.AdditionalCellNum;	
            reply();
			await ETTask.CompletedTask;
		}
	}
}
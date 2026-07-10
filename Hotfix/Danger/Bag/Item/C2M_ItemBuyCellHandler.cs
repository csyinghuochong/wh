using System;
using System.Collections.Generic;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_ItemBuyCellHandler : AMActorLocationRpcHandler<Unit, C2M_ItemBuyCellRequest, M2C_ItemBuyCellResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_ItemBuyCellRequest request, M2C_ItemBuyCellResponse response, Action reply)
        {
            BagComponentServer bagComponentServer = unit.GetComponent<BagComponentServer>();
            //string costitems = GlobalValueConfigCategory.Instance.Get(83).Value;
            //if (!bagComponentServer.CheckNeedItem(costitems))
            //{
            //    response.Error = ErrorCode.ERR_DiamondNotEnoughError;
            //    reply();
            //    return;
            //}

            if (request.OperateType == (int)ItemLocType.ItemLocBag)
            {
                if (bagComponentServer.GetBagTotalCell() >= LDGlobalValueCategory.Instance.BagMaxCapacity)
                {
                    response.Error = ErrorCode.ERR_AleardyMaxCell;
                    reply();
                    return;
                }
                BuyCellCost buyCellCost = CommonConfig.BuyBagCellCosts[bagComponentServer.WarehouseAddedCell[0]];
                if (!bagComponentServer.OnCostItemData(buyCellCost.Cost, ItemLocType.ItemLocBag, ItemGetWay.CostItem))
                {
                    response.Error = ErrorCode.ERR_ItemNotEnoughError;
                    reply();
                    return;
                }


                string[] iteminfo = buyCellCost.Get.Split(';');
                response.GetItem = buyCellCost.Get;
                bagComponentServer.WarehouseAddedCell[0] += 1;

                RewardItem rewardItem = new RewardItem()
                {
                    ItemID = int.Parse(iteminfo[0]),
                    ItemNum = int.Parse(iteminfo[1]),
                };
                List<RewardItem> rewardItems = new List<RewardItem>() { rewardItem };
                bagComponentServer.OnAddItemData(rewardItems, String.Empty, $"{ItemGetWay.CostItem}_{TimeHelper.ServerNow()}", true, false, (ItemLocType)request.OperateType);
            }
            else
            {
                int storeindex = request.OperateType;
                if (storeindex < 5 || storeindex > 9)
                {
                    Log.Error($"C2M_ItemBuyCellRequest.1");
                    response.Error = ErrorCode.ERR_ModifyData;
                    reply();
                    return;
                }


                if (bagComponentServer.GetHourseTotalCell(request.OperateType) >= LDGlobalValueCategory.Instance.HourseMaxCapacity)
                {
                    response.Error = ErrorCode.ERR_AleardyMaxCell;
                    reply();
                    return;
                }
                
                int addcell = bagComponentServer.WarehouseAddedCell[storeindex];
                BuyCellCost buyCellCost = CommonConfig.BuyStoreCellCosts[(storeindex - 5) * 10 + addcell];
                if (!bagComponentServer.OnCostItemData(buyCellCost.Cost,ItemLocType.ItemLocBag, ItemGetWay.CostItem))
                {
                    response.Error = ErrorCode.ERR_ItemNotEnoughError;
                    reply();
                    return;
                }

                string[] iteminfo = buyCellCost.Get.Split(';');
                response.GetItem = buyCellCost.Get;
                bagComponentServer.WarehouseAddedCell[storeindex] += 1;

                RewardItem rewardItem = new RewardItem()
                {
                    ItemID = int.Parse(iteminfo[0]),
                    ItemNum = int.Parse(iteminfo[1]),
                };
                List<RewardItem> rewardItems = new List<RewardItem>() { rewardItem };
                bagComponentServer.OnAddItemData(rewardItems, String.Empty, $"{ItemGetWay.CostItem}_{TimeHelper.ServerNow()}", true, false, (ItemLocType)request.OperateType);
            }

            response.WarehouseAddedCell = bagComponentServer.WarehouseAddedCell;
            //response.BagAddedCell = bagComponentServer.BagAddedCell;
            reply();
            await ETTask.CompletedTask;
        }
    }
}

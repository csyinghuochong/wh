using System;
using System.Collections.Generic;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_BagBuyCellHandler : AMActorLocationRpcHandler<Unit, C2M_BagBuyCellRequest, M2C_BagBuyCellResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_BagBuyCellRequest request, M2C_BagBuyCellResponse response, Action reply)
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
                //if (bagComponentServer.GetBagTotalCell() >= LDGlobalValueCategory.Instance.BagMaxCapacity[(int)ItemLocType.ItemLocBag])
                //{
                //    response.Error = ErrorCode.ERR_AleardyMaxCell;
                //    reply();
                //    return;
                //}
                BuyCellCost buyCellCost = CommonConfig.BuyBagCellCosts[bagComponentServer.AdditionalCellNum[0]];
                if (!bagComponentServer.OnCostItemData(buyCellCost.Cost, ItemLocType.ItemLocBag, ItemGetWay.CostItem))
                {
                    response.Error = ErrorCode.ERR_ItemNotEnoughError;
                    reply();
                    return;
                }


                response.GetItem = buyCellCost.Get;
                bagComponentServer.AdditionalCellNum[0] += 1;

                bagComponentServer.OnAddItemData(buyCellCost.Get, $"{ItemGetWay.CostItem}_{TimeHelper.ServerNow()}", true);
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


                //if (bagComponentServer.GetHourseTotalCell(request.OperateType) >= LDGlobalValueCategory.Instance.BagMaxCapacity[request.OperateType])
                //{
                //    response.Error = ErrorCode.ERR_AleardyMaxCell;
                //    reply();
                //    return;
                //}
                
                int addcell = bagComponentServer.AdditionalCellNum[storeindex];
                BuyCellCost buyCellCost = CommonConfig.BuyStoreCellCosts[(storeindex - 5) * 10 + addcell];
                if (!bagComponentServer.OnCostItemData(buyCellCost.Cost,ItemLocType.ItemLocBag, ItemGetWay.CostItem))
                {
                    response.Error = ErrorCode.ERR_ItemNotEnoughError;
                    reply();
                    return;
                }

                response.GetItem = buyCellCost.Get;
                bagComponentServer.AdditionalCellNum[storeindex] += 1;

                bagComponentServer.OnAddItemData(
                    buyCellCost.Get,
                    $"{ItemGetWay.CostItem}_{TimeHelper.ServerNow()}",
                    true,
                    (ItemLocType)request.OperateType);
            }

            response.AdditionalCellNum = bagComponentServer.AdditionalCellNum;
            //response.BagAddedCell = bagComponentServer.BagAddedCell;
            reply();
            await ETTask.CompletedTask;
        }
    }
}

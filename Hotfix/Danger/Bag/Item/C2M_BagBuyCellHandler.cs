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
                int loc = (int)ItemLocType.ItemLocBag;
                int addcell = BagCellNumHelper.Get(bagComponentServer.AddedCellNum, loc);
                BuyCellCost buyCellCost = CommonConfig.BuyBagCellCosts[addcell];
                if (!bagComponentServer.OnCostItemData(buyCellCost.Cost, ItemLocType.ItemLocBag, ItemGetWay.CostItem))
                {
                    response.Error = ErrorCode.ERR_ItemNotEnoughError;
                    reply();
                    return;
                }


                response.GetItem = buyCellCost.Get;
                BagCellNumHelper.Add(bagComponentServer.AddedCellNum, loc);

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

                int addcell = BagCellNumHelper.Get(bagComponentServer.AddedCellNum, storeindex);
                BuyCellCost buyCellCost = CommonConfig.BuyStoreCellCosts[(storeindex - 5) * 10 + addcell];
                if (!bagComponentServer.OnCostItemData(buyCellCost.Cost,ItemLocType.ItemLocBag, ItemGetWay.CostItem))
                {
                    response.Error = ErrorCode.ERR_ItemNotEnoughError;
                    reply();
                    return;
                }

                response.GetItem = buyCellCost.Get;
                BagCellNumHelper.Add(bagComponentServer.AddedCellNum, storeindex);

                bagComponentServer.OnAddItemData(
                    buyCellCost.Get,
                    $"{ItemGetWay.CostItem}_{TimeHelper.ServerNow()}",
                    true,
                    (ItemLocType)request.OperateType);
            }

            response.AddedCellNum = BagCellNumHelper.ToProto(bagComponentServer.AddedCellNum);
            //response.BagAddedCell = bagComponentServer.BagAddedCell;
            reply();
            await ETTask.CompletedTask;
        }
    }
}

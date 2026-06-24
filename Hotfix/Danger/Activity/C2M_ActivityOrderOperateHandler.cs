using System;
using System.Collections.Generic;

namespace ET
{

    [ActorMessageHandler]
    public class C2M_ActivityOrderOperateHandler : AMActorLocationRpcHandler<Unit, C2M_ActivityOrderOperateRequest, M2C_ActivityOrderOperateResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_ActivityOrderOperateRequest request, M2C_ActivityOrderOperateResponse response, Action reply)
        {
            BagComponentServer bagComponentServer  = unit.GetComponent<BagComponentServer>(); 
            ActivityComponentServer activityComponentServer = unit.GetComponent<ActivityComponentServer>();
            
            switch (request.OperatateType)
            {
                case 1:
                    if (!bagComponentServer.CheckNeedItem(ActivityV1Config.ActivityOrderRefreshItem))
                    {
                        response.Error = ErrorCode.ERR_ItemNotEnoughError;
                        reply();
                        return;
                    }

                    bagComponentServer.OnCostItemData(ActivityV1Config.ActivityOrderRefreshItem, ItemLocType.ItemLocBag, ItemGetWay.Activity);
                    activityComponentServer.ActivityV1Info.OrderId = ActivityV1Config.GenerateActivityOrderId();
                    activityComponentServer.ActivityV1Info.OrderLastFefreshTime = TimeHelper.ServerNow();

                    break;
                case 2:
                    int orderId = activityComponentServer.ActivityV1Info.OrderId;
                    if (orderId < 0 || orderId >= ActivityV1Config.ActivityOrderItemList.Count)
                    {
                        response.Error = ErrorCode.ERR_Parameter;
                        reply();
                        return;
                    }
                    ActivityOrderItem activityOrderItem = ActivityV1Config.ActivityOrderItemList[orderId];
                   
                    if (!bagComponentServer.CheckNeedItem(activityOrderItem.Give))
                    {
                        response.Error = ErrorCode.ERR_ItemNotEnoughError;
                        reply();
                        return;
                    }

                    int needcell = ItemHelper.GetNeedCell(activityOrderItem.Get);
                    if (bagComponentServer.GetBagLeftCell() < needcell + 1)
                    {
                        response.Error = ErrorCode.ERR_BagIsFull;
                        reply();
                        return;
                    }

                    bagComponentServer.OnCostItemData(activityOrderItem.Give, ItemLocType.ItemLocBag, ItemGetWay.Activity);
                    bagComponentServer.OnAddItemData(activityOrderItem.Get, $"{ItemGetWay.Activity}_{TimeHelper.ServerNow()}");

                    List<RewardItem> droplist = new List<RewardItem>();
                    int dropid = int.Parse(activityOrderItem.DropID);
                    DropHelper.DropIDToDropItem_2(dropid, droplist);
                    bagComponentServer.OnAddItemData(droplist, string.Empty, $"{ItemGetWay.Activity}_{TimeHelper.ServerNow()}");
                    //activityComponentServer.ActivityV1Info.OrderId = ActivityConfigHelper.GenerateActivityOrderId();
                    //activityComponentServer.ActivityV1Info.OrderLastFefreshTime = TimeHelper.ServerNow();
                    break;
                case 3:  //自动刷新
                    activityComponentServer.ActivityV1Info.OrderId = ActivityV1Config.GenerateActivityOrderId();
                    activityComponentServer.ActivityV1Info.OrderLastFefreshTime = TimeHelper.ServerNow();
                    break;
            }

            response.ActivityV1Info = activityComponentServer.ActivityV1Info;
            reply();
            await ETTask.CompletedTask;
        }
    }
}

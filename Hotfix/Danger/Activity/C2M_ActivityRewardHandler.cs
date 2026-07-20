using System;
using System.Collections.Generic;
using System.Linq;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_ActivityRewardHandler : AMActorLocationRpcHandler<Unit, C2M_ActivityRewardRequest, M2C_ActivityRewardResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_ActivityRewardRequest request, M2C_ActivityRewardResponse response, Action reply)
        {
            BagComponentServer bagComponentServer = unit.GetComponent<BagComponentServer>();
            NumericComponent numericComponent = unit.GetComponent<NumericComponent>();
            if (bagComponentServer.GetBagLeftCell() < 1)
            {
                response.Error = ErrorCode.ERR_BagIsFull;
                reply();
                return;
            }

            string rewarditem = string.Empty;
            ActivityComponentServer activityComponentServer = unit.GetComponent<ActivityComponentServer>();   

            switch (request.ActivityType)
            {
                case ActivityV1Config.ActivityV1_ChouKa:
                    if (numericComponent.GetAsInt(NumericType.V1ChouKaNumber) < request.RewardId)
                    {
                        response.Error = ErrorCode.Pre_Condition_Error;
                        reply();
                        return;
                    }

                    if (!ActivityV1Config.ChouKaNumberReward.ContainsKey(request.RewardId))
                    {
                        Log.Error($"C2M_ActivityReceiveRequest.4");
                        response.Error = ErrorCode.ERR_ModifyData;
                        reply();
                        return;
                    }
                    if (activityComponentServer.ActivityV1Info.ChouKaNumberReward.Contains(request.RewardId))
                    {
                        response.Error = ErrorCode.ERR_AlreadyReceived;
                        reply();
                        return;
                    }
                    rewarditem = ActivityV1Config.ChouKaNumberReward[request.RewardId];
                    bagComponentServer.OnAddItemData(rewarditem, $"{ItemGetWay.ActivityChouKa}_{TimeHelper.ServerNow()}");
                    activityComponentServer.ActivityV1Info.ChouKaNumberReward.Add(request.RewardId);
                    break;
                case ActivityV1Config.ActivityV1_Consume:
                    if (!ActivityV1Config.ConsumeDiamondReward.ContainsKey(request.RewardId))
                    {
                        Log.Error($"C2M_ActivityReceiveRequest.5");
                        response.Error = ErrorCode.ERR_ModifyData;
                        reply();
                        return;
                    }
                    if (activityComponentServer.ActivityV1Info.ConsumeDiamondReward.Contains(request.RewardId))
                    {
                        response.Error = ErrorCode.ERR_AlreadyReceived;
                        reply();
                        return;
                    }
                    if (numericComponent.GetAsLong(NumericType.V1DayCostDiamond) < request.RewardId)
                    {
                        response.Error = ErrorCode.Pre_Condition_Error;
                        reply();
                        return;
                    }
                    rewarditem = ActivityV1Config.ConsumeDiamondReward[request.RewardId];
                    bagComponentServer.OnAddItemData(rewarditem, $"{ItemGetWay.ActivityConsume}_{TimeHelper.ServerNow()}");
                    activityComponentServer.ActivityV1Info.ConsumeDiamondReward.Add(request.RewardId);
                    break;
                case ActivityV1Config.ActivityV1_Points:

                    if (!ActivityV1Config.PointsRewardList.ContainsKey(request.RewardId))
                    {
                        Log.Error($"C2M_ActivityReceiveRequest.6");
                        response.Error = ErrorCode.ERR_ModifyData;
                        reply();
                        return;
                    }
                    if (activityComponentServer.ActivityV1Info.PointsReward.Contains(request.RewardId))
                    {
                        response.Error = ErrorCode.ERR_AlreadyReceived;
                        reply();
                        return;
                    }
            
                    rewarditem = ActivityV1Config.PointsRewardList[request.RewardId];
                    int needcell = ItemNewHelper.GetNeedCell(rewarditem);
                    if (bagComponentServer.GetBagLeftCell() < needcell)
                    {
                        response.Error = ErrorCode.ERR_BagIsFull;
                        reply();
                        return;
                    }

                    bagComponentServer.OnAddItemData(rewarditem, $"{ItemGetWay.ActivityConsume}_{TimeHelper.ServerNow()}");
                    activityComponentServer.ActivityV1Info.PointsReward.Add(request.RewardId);
                    break;

                case ActivityV1Config.ActivityV1_PointsShunXu:
                    if (!ActivityV1Config.PointsShunXuRewardList.ContainsKey(request.RewardId))
                    {
                        Log.Error($"C2M_ActivityReceiveRequest.7");
                        response.Error = ErrorCode.ERR_ModifyData;
                        reply();
                        return;
                    }

                    int getnextRewardId = ActivityV1Config.GetNextShunXuReward(activityComponentServer.ActivityV1Info.PointsShuxuReward);
                    if (getnextRewardId!= request.RewardId)
                    {
                        response.Error = ErrorCode.ERR_AlreadyReceived;
                        reply();
                        return;
                    }
                    //if (unit.GetComponent<RoleInfoComponentServer>().RoleInfo.V1TotalPoints < request.RewardId)
                    //{
                    //    response.Error = ErrorCode.Pre_Condition_Error;
                    //    reply();
                    //    return;
                    //}
                    rewarditem = ActivityV1Config.PointsShunXuRewardList[request.RewardId];
                    needcell = ItemNewHelper.GetNeedCell(rewarditem);
                    if (bagComponentServer.GetBagLeftCell() < needcell)
                    {
                        response.Error = ErrorCode.ERR_BagIsFull;
                        reply();
                        return;
                    }

                    bagComponentServer.OnAddItemData(rewarditem, $"{ItemGetWay.ActivityConsume}_{TimeHelper.ServerNow()}");
                    activityComponentServer.ActivityV1Info.PointsShuxuReward = request.RewardId;
                    break;
                case ActivityV1Config.ActivityV1_PointsChouKa:

                    //if (unit.GetComponent<RoleInfoComponentServer>().RoleInfo.V1TotalPoints < 200f)
                    //{
                    //    response.Error = ErrorCode.Pre_Condition_Error;
                    //    reply();
                    //    return;
                    //}
               
                    break;
                case ActivityV1Config.ActivityV1_HongBao:
                    int hongbaoNumber = numericComponent.GetAsInt(NumericType.V1HongBaoNumber);
                    long v1rechargeNumber = numericComponent.GetAsInt(NumericType.V1RechageNumber);
                    int totalHongBa0 = (int)(v1rechargeNumber / 98);
                    if (hongbaoNumber >= totalHongBa0)
                    {
                        response.Error = ErrorCode.ERR_AlreadyReceived;
                        reply();
                        return;
                    }
                    List<RewardItem> rewardItems = new List<RewardItem>();  
                    DropHelper.DropIDToDropItem_2(ActivityV1Config.HongBaoDropId, rewardItems);
                    if (bagComponentServer.GetBagLeftCell() < rewardItems.Count)
                    {
                        response.Error = ErrorCode.ERR_BagIsFull;
                        reply();
                        return;
                    }
                    bagComponentServer.OnAddItemData(rewardItems, string.Empty, $"{ItemGetWay.ItemBox_9}_{TimeHelper.ServerNow()}");
                    numericComponent.ApplyChange(null, NumericType.V1HongBaoNumber, 1, 0);
                    break;
                case ActivityV1Config.ActivityV1_DuiHuanWord:
                    if (bagComponentServer.GetBagLeftCell() < 1)
                    {
                        response.Error = ErrorCode.ERR_BagIsFull;
                        reply();
                        return;
                    }
                    if (request.RewardId > 0 && !ActivityV1Config.DuiHuanWordReward.ContainsKey(request.RewardId))
                    {
                        response.Error = ErrorCode.ERR_ItemNotEnoughError;
                        reply();
                        return;
                    }

                    List<RewardItem> costItemList = new List<RewardItem>();
                    string rewardItem = string.Empty;
                    if (request.RewardId == 0)
                    {
                        List<int> allword = ActivityV1Config.DuiHuanWordReward.Keys.ToList();
                        for (int i = 0; i < allword.Count; i++)
                        {
                            costItemList.Add( new RewardItem() { ItemID = allword[i], ItemNum = 1 } );
                        }
                        rewardItem = ActivityV1Config.GroupsWordReward;
                    }
                    else
                    {
                        costItemList.Add( new RewardItem() { ItemID = request.RewardId, ItemNum = 1 } );
                        rewardItem = ActivityV1Config.DuiHuanWordReward[request.RewardId];
                    }
                    if (!bagComponentServer.OnCostItemData(costItemList, ItemLocType.ItemLocBag, ItemGetWay.Activity))
                    {
                        response.Error = ErrorCode.ERR_ItemNotEnoughError;
                        reply();
                        return;
                    }
                    bagComponentServer.OnAddItemData(rewardItem, $"{ItemGetWay.Activity}_{TimeHelper.ServerNow()}");
                    break;
                case ActivityV1Config.ActivityV1_ChouKa2:
                    if (bagComponentServer.GetBagLeftCell() < 1)
                    {
                        response.Error = ErrorCode.ERR_BagIsFull;
                        reply();
                        return;
                    }
                    if (!bagComponentServer.CheckNeedItem(ActivityV1Config.Chou2CostItem))
                    {
                        response.Error = ErrorCode.ERR_ItemNotEnoughError;
                        reply();
                        return;
                    }
                    int rewardIndex = ActivityV1Config.GetChouKa2RewardIndex(activityComponentServer.ActivityV1Info.ChouKa2ItemList, activityComponentServer.ActivityV1Info.ChouKa2RewardIds);
                    activityComponentServer.ActivityV1Info.ChouKa2RewardIds.Add(rewardIndex);
                    string[] rewardList = activityComponentServer.ActivityV1Info.ChouKa2ItemList.Split('@');
                    rewardItem = rewardList[rewardIndex];
                    bagComponentServer.OnCostItemData(ActivityV1Config.Chou2CostItem, ItemLocType.ItemLocBag, ItemGetWay.Activity);
                    bagComponentServer.OnAddItemData(rewardItem, $"{ItemGetWay.Activity}_{TimeHelper.ServerNow()}");
                    //全部抽完则自动刷新
                    if (activityComponentServer.ActivityV1Info.ChouKa2RewardIds.Count >= rewardList.Length )
                    {
                        activityComponentServer.ActivityV1Info.ChouKa2RewardIds.Clear();
                        activityComponentServer.ActivityV1Info.ChouKa2ItemList = ActivityV1Config.GetChouKa2RewardList();
                    }
                    break;
                case ActivityV1Config.ActivityV1_GoldWeeklyCard:
                    long servertimer = TimeHelper.ServerNow();
                    
                    break;
                case ActivityV1Config.ActivityV1_DiamondWeeklyCard:
                    servertimer = TimeHelper.ServerNow();
                 
                    break;
                case ActivityV1Config.ActivityV1_LiBao:
                    if (bagComponentServer.GetBagLeftCell() < 6)
                    {
                        response.Error = ErrorCode.ERR_BagIsFull;
                        reply();
                        return;
                    }
                    if (!activityComponentServer.ActivityV1Info.LiBaoAllIds.Contains(request.RewardId))
                    {
                        Log.Error($"C2M_ActivityReceiveRequest.6");
                        response.Error = ErrorCode.ERR_ModifyData;
                        reply();
                        return;
                    }
                    if (activityComponentServer.ActivityV1Info.LiBaoBuyIds.Contains(request.RewardId))
                    {
                        response.Error = ErrorCode.ERR_AlreadyReceived;
                        reply();
                        return;
                    }
                    LiBaoListItem keyValuePair = ActivityV1Config.LiBaoList[request.RewardId];
                    if (!bagComponentServer.OnCostItemData(keyValuePair.Value, ItemLocType.ItemLocBag, ItemGetWay.Activity    ))
                    {
                        response.Error = ErrorCode.ERR_ItemNotEnoughError;
                        reply();
                        return;
                    }
                    bagComponentServer.OnAddItemData(keyValuePair.Value2, $"{ItemGetWay.Activity}_{TimeHelper.ServerNow()}");
                    activityComponentServer.ActivityV1Info.LiBaoBuyIds.Add(request.RewardId);
                    break;
                default:
                    break;
            }
            response.ActivityV1Info = activityComponentServer.ActivityV1Info;
            reply();
            await ETTask.CompletedTask;
        }
    }
}

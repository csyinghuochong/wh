using System;
using System.Collections.Generic;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_ActivityReceiveHandler : AMActorLocationRpcHandler<Unit, C2M_ActivityReceiveRequest, M2C_ActivityReceiveResponse>
    {

        protected override async ETTask Run(Unit unit, C2M_ActivityReceiveRequest request, M2C_ActivityReceiveResponse response, Action reply)
        {
            using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.Received, unit.Id))
            {
                if (!ActivityConfigCategory.Instance.Contain(request.ActivityId))
                {
                    Log.Error($"C2M_ActivityReceiveRequest.1");
                    response.Error = ErrorCode.ERR_ModifyData;
                    reply();
                    return;
                }

                ActivityComponentServer activityComponentServer = unit.GetComponent<ActivityComponentServer>();
                if (!ActivityHelper.HaveReceiveTimes(activityComponentServer.ActivityReceiveIds, request.ActivityId))
                {
                    response.Error = ErrorCode.ERR_AlreadyReceived;
                    reply();
                    return;
                }
                LogHelper.LogWarning($"C2M_ActivityReceive:  {unit.Id} {request.ActivityId} {request.ReceiveIndex} {TimeHelper.ServerNow().ToString()}", true);
                ActivityConfig activityConfig = ActivityConfigCategory.Instance.Get(request.ActivityId);
                if (activityConfig.ActivityType!= request.ActivityType)
                {
                    Log.Error($"C2M_ActivityReceiveRequest.2");
                    response.Error = ErrorCode.ERR_ModifyData;
                    reply();
                    return;
                }

                switch (activityConfig.ActivityType)
                {
                    case 2: //每日特惠
                        string[] needList = activityConfig.Par_3.Split('@');
                        if (unit.GetComponent<BagComponentServer>().GetBagLeftCell() < needList.Length)
                        {
                            response.Error = ErrorCode.ERR_BagIsFull;
                            reply();
                            return;
                        }

                        if (!unit.GetComponent<BagComponentServer>().OnCostItemData(activityConfig.Par_2, ItemLocType.ItemLocBag, ItemGetWay.Activity))
                        {
                            response.Error = ErrorCode.ERR_DiamondNotEnoughError;
                            reply();
                            return;
                        }
                        if (!unit.GetComponent<BagComponentServer>().OnAddItemData(activityConfig.Par_3, $"{ItemGetWay.Activity_DayTeHui}_{TimeHelper.ServerNow()}"))
                        {
                            response.Error = ErrorCode.ERR_BagIsFull;
                            reply();
                            return;
                        }

                        activityComponentServer.ActivityReceiveIds.Add(request.ActivityId);
                        
                        break;
                    case 23:    //签到
                        if (activityComponentServer.TotalSignNumber == 30)
                        {
                            response.Error = ErrorCode.ERR_AlreadyReceived;
                            reply();
                            return;
                        }
                        long serverNow = TimeHelper.ServerNow();
                        if (CommonHelper.GetDayByTime(serverNow) == CommonHelper.GetDayByTime(activityComponentServer.LastSignTime))
                        {
                            response.Error = ErrorCode.ERR_AlreadyReceived;
                            reply();
                            return;
                        }
                        string[] rewarditems = activityConfig.Par_3.Split('@');
                        if (rewarditems.Length > unit.GetComponent<BagComponentServer>().GetBagLeftCell())
                        {
                            response.Error = ErrorCode.ERR_BagIsFull;
                            reply();
                            return;
                        }

                        activityComponentServer.TotalSignNumber++;
                        activityComponentServer.LastSignTime = TimeHelper.ServerNow();
                        activityComponentServer.ActivityReceiveIds.Add(request.ActivityId);
                        unit.GetComponent<BagComponentServer>().OnAddItemData(activityConfig.Par_3, $"{ItemGetWay.Activity}_{TimeHelper.ServerNow()}");
                        break;
                    case 24:    //令牌
                        List<TokenRecvive> zhanQuTokenRecvives = activityComponentServer.QuTokenRecvive;
                        for (int i = 0; i < zhanQuTokenRecvives.Count; i++)
                        {
                            if (zhanQuTokenRecvives[i].ActivityId == request.ActivityId
                                && zhanQuTokenRecvives[i].ReceiveIndex == (request.ReceiveIndex))
                            {
                                response.Error = ErrorCode.ERR_AlreadyReceived;
                                reply();
                                return;
                            }
                        }
                        RoleInfoComponent roleInfoComponent = unit.GetComponent<RoleInfoComponent>();
                        if (roleInfoComponent.RoleInfo.Lv < int.Parse(activityConfig.Par_1))
                        {
                            reply();
                            return;
                        }
                        int selfRechage = unit.GetComponent<NumericComponent>().GetAsInt(NumericType.RechargeNumber);
                        if (ServerHelper.IsGoogleServer(unit.DomainZone()))
                        {
                            if (request.ReceiveIndex == 3 && selfRechage < 50)
                            {
                                reply();
                                return;
                            }
                            if (request.ReceiveIndex == 2 && selfRechage < 20)
                            {
                                reply();
                                return;
                            }
                        }
                        else
                        {
                            if (request.ReceiveIndex == 3 && selfRechage < 298)
                            {
                                reply();
                                return;
                            }
                            if (request.ReceiveIndex == 2 && selfRechage < 98)
                            {
                                reply();
                                return;
                            }
                        }

                        activityConfig = ActivityConfigCategory.Instance.Get(request.ActivityId);
                        string rewards = "";
                        if (request.ReceiveIndex == 1) rewards = activityConfig.Par_2;
                        if (request.ReceiveIndex == 2) rewards = activityConfig.Par_3;
                        if (request.ReceiveIndex == 3) rewards = activityConfig.Par_4;

                        rewarditems = rewards.Split('@');
                        if (rewarditems.Length > unit.GetComponent<BagComponentServer>().GetBagLeftCell())
                        {
                            response.Error = ErrorCode.ERR_BagIsFull;
                            reply();
                            return;
                        }

                        zhanQuTokenRecvives.Add(new TokenRecvive() { ActivityId = request.ActivityId, ReceiveIndex = request.ReceiveIndex });
                        unit.GetComponent<BagComponentServer>().OnAddItemData(rewards, $"{ItemGetWay.Activity}_{TimeHelper.ServerNow()}");
                        break;
                    case 31:    //登录奖励
                        roleInfoComponent = unit.GetComponent<RoleInfoComponent>();
                        if (roleInfoComponent.RoleInfo.Lv < 10)
                        {
                            reply();
                            return;
                        }
                        serverNow = TimeHelper.ServerNow();
                        if (CommonHelper.GetDayByTime(serverNow) == CommonHelper.GetDayByTime(activityComponentServer.LastLoginTime))
                        {
                            response.Error = ErrorCode.ERR_AlreadyReceived;
                            reply();
                            return;
                        }

                        rewarditems = activityConfig.Par_3.Split('@');
                        if (rewarditems.Length > unit.GetComponent<BagComponentServer>().GetBagLeftCell())
                        {
                            response.Error = ErrorCode.ERR_BagIsFull;
                            reply();
                            return;
                        }

                        activityComponentServer.LastLoginTime = serverNow;
                        unit.GetComponent<ActivityComponentServer>().ActivityReceiveIds.Add(request.ActivityId);
                        unit.GetComponent<BagComponentServer>().OnAddItemData(activityConfig.Par_3, $"{ItemGetWay.Activity}_{TimeHelper.ServerNow()}");
                        break;
                    case 32:    //新年集字
                        rewarditems = activityConfig.Par_3.Split('@');
                        if (rewarditems.Length > unit.GetComponent<BagComponentServer>().GetBagLeftCell())
                        {
                            response.Error = ErrorCode.ERR_BagIsFull;
                            reply();
                            return;
                        }
                        if (unit.GetComponent<BagComponentServer>().OnCostItemData(activityConfig.Par_2, ItemLocType.ItemLocBag, ItemGetWay.Activity))
                        {
                            unit.GetComponent<ActivityComponentServer>().ActivityReceiveIds.Add(request.ActivityId);
                            unit.GetComponent<BagComponentServer>().OnAddItemData(activityConfig.Par_3, $"{97}_{TimeHelper.ServerNow()}");
                        }
                        else
                        {
                            response.Error = ErrorCode.ERR_ItemNotEnoughError;
                        }
                        break;
                    case 33://节日活动
                        if (unit.GetComponent<RoleInfoComponent>().TodayOnLine < 30)
                        {
                            response.Error = ErrorCode.Err_OnLineTimeNot;
                            reply();
                            return;
                        }
                        string rewardItemlist = ActivityHelper.GetJieRiReward(unit.GetComponent<RoleInfoComponent>());
                        if (unit.GetComponent<BagComponentServer>().GetBagLeftCell() < rewardItemlist.Split('@').Length)
                        {
                            response.Error = ErrorCode.ERR_BagIsFull;
                            reply();
                            return;
                        }
                        if (!ActivityHelper.IsJieRiActivityId(request.ActivityId))
                        {
                            response.Error = ErrorCode.ERR_AlreadyFinish;
                            reply();
                            return;
                        }

                        if (unit.GetComponent<RoleInfoComponent>().RoleInfo.Lv < 20)
                        {
                            response.Error = ErrorCode.ERR_EquipLvLimit;
                            reply();
                            return;
                        }

                        unit.GetComponent<ActivityComponentServer>().ActivityReceiveIds.Add(request.ActivityId);
                        unit.GetComponent<BagComponentServer>().OnAddItemData(rewardItemlist, $"{ItemGetWay.Activity}_{TimeHelper.ServerNow()}");
                        break;
                    case 34:
                        roleInfoComponent = unit.GetComponent<RoleInfoComponent>();
                        if (roleInfoComponent.RoleInfo.Lv < int.Parse(activityConfig.Par_1))
                        {
                            response.Error = ErrorCode.ERR_EquipLvLimit;
                            reply();
                            return;
                        }

                        rewarditems = activityConfig.Par_3.Split('@');
                        if (rewarditems.Length > unit.GetComponent<BagComponentServer>().GetBagLeftCell())
                        {
                            response.Error = ErrorCode.ERR_BagIsFull;
                            reply();
                            return;
                        }

                        unit.GetComponent<ActivityComponentServer>().ActivityReceiveIds.Add(request.ActivityId);
                        unit.GetComponent<BagComponentServer>().OnAddItemData(activityConfig.Par_3, $"{ItemGetWay.Serial}_{TimeHelper.ServerNow()}");
                        break;
                    case 35:   //微信公众号绑定角色成功
                        if (ItemHelper.GetNeedCell(activityConfig.Par_3) > unit.GetComponent<BagComponentServer>().GetBagLeftCell())
                        {
                            response.Error = ErrorCode.ERR_BagIsFull;
                            reply();
                            return;
                        }

                        unit.GetComponent<ActivityComponentServer>().ActivityReceiveIds.Add(request.ActivityId);
                        unit.GetComponent<BagComponentServer>().OnAddItemData(activityConfig.Par_3, $"{ItemGetWay.Serial}_{TimeHelper.ServerNow()}");
                        break;
                    case 101:   //冒险家
                                //需要从dbaccountinfo中获取当前角色重置额度
                        long needrecharge = int.Parse(activityConfig.Par_2);
                        int rechargeNum = unit.GetComponent<NumericComponent>().GetAsInt(NumericType.RechargeNumber);
                        rechargeNum *= 10;
                        rechargeNum += unit.GetComponent<NumericComponent>().GetAsInt(NumericType.MaoXianExp);
                        if (rechargeNum < needrecharge)
                        {
                            Log.Error($"C2M_ActivityReceiveRequest.3");
                            response.Error = ErrorCode.ERR_ModifyData;
                            reply();
                            return;
                        }
                        rewarditems = activityConfig.Par_3.Split('@');
                        if (rewarditems.Length > unit.GetComponent<BagComponentServer>().GetBagLeftCell())
                        {
                            response.Error = ErrorCode.ERR_BagIsFull;
                            reply();
                            return;
                        }
                        if (!unit.GetComponent<BagComponentServer>().OnAddItemData(activityConfig.Par_3, $"{ItemGetWay.Activity_MaoXianJia}_{TimeHelper.ServerNow()}"))
                        {
                            response.Error = ErrorCode.ERR_BagIsFull;
                            reply();
                            return;
                        }
                        unit.GetComponent<ActivityComponentServer>().ActivityReceiveIds.Add(request.ActivityId);
                        break;
                    default:
                        bool success = unit.GetComponent<BagComponentServer>().OnCostItemData(activityConfig.Par_2, ItemLocType.ItemLocBag, ItemGetWay.Activity   );
                        if (success)
                        {
                            unit.GetComponent<ActivityComponentServer>().ActivityReceiveIds.Add(request.ActivityId);
                            unit.GetComponent<BagComponentServer>().OnAddItemData(activityConfig.Par_3, $"{ItemGetWay.Activity}_{TimeHelper.ServerNow()}");
                        }
                        else
                        {
                            response.Error = ErrorCode.ERR_GoldNotEnoughError;
                        }
                        break;
                }
            }
            LogHelper.LogWarning($"C2M_ActivityReceive[成功]:  {unit.Id} {request.ActivityId} {request.ReceiveIndex} {TimeHelper.ServerNow().ToString()}", true);
            reply();
            await ETTask.CompletedTask;
        }
    }
}

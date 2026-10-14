using Alipay.AopSdk.Core.Domain;
using System;
using System.Collections.Generic;

namespace ET
{
    public static class RechargeHelp
    {

        public static void  SendDiamondToUnit(Unit unit, int payid, int rechargeType, string orderInfo)
        {
            OnRechage(unit, payid, rechargeType, true, orderInfo);
            unit.GetComponent<DBSaveComponent>().UpdateCacheDB();
        }


        public static int GetRechargeNumber(int payId, int zone)
        {
            LDPay ldPay = LDPayCategory.Instance.Get(payId);
            return ldPay.Tier_Id;
        }


        public static void OnRechage(Unit unit, int playId, int rechargetType, bool notice, string orderInfo = null)
        {
            if (playId <= 0)
            { 
                return; 
            }

            if (!LDPayCategory.Instance.Contain(playId))
            {
                Log.Error($"OnRechage Pay配置不存在: {unit.Id} payId:{playId}");
                return;
            }
        
            RechargeComponentServer rechargeComponentServer = unit.GetComponent<RechargeComponentServer>();
            if (!string.IsNullOrEmpty(orderInfo))
            {
                //if (rechargeComponentServer.PaidOrderIds.Contains(orderInfo))
                //{
                //    return;
                //}
                //rechargeComponentServer.PaidOrderIds.Add(orderInfo);
            }

            int homeZone = UnitZoneHelper.GetHomeZone(unit);
            LDPay ldPay = LDPayCategory.Instance.Get(playId);
            string reward = ldPay.Reward;
            LDActivity_1 activity1 = null;
            bool canFirstBuy = false;
            if (LDActivity_1Category.Instance.Contain(playId))
            {
                activity1 = LDActivity_1Category.Instance.Get(playId);
                canFirstBuy = activity1.Is_First > 0 && !rechargeComponentServer.HasFirstBuy(playId);
            }

            if (canFirstBuy && !string.IsNullOrEmpty(activity1.First_Reward))
            {
                reward = reward + "|" + activity1.First_Reward;
            }

            if (!canFirstBuy && activity1 != null && !string.IsNullOrEmpty(activity1.Extra_Reward))
            {
                reward = reward + "|" + activity1.Extra_Reward;
            }

            List<RewardItem> rewardItems = ItemNewHelper.GetRewardItems(reward);
            TaskComponentServer taskComponentServer = unit.GetComponent<TaskComponentServer>();

            //0 钻石  1周卡
            if (rechargetType == RechargeBizTypeEnum.Diamond)
            {
                unit.GetComponent<BagComponentServer>().OnAddItemData(rewardItems, $"{ItemGetWay.Recharge}_{TimeHelper.ServerNow()}");
                if (canFirstBuy)
                {
                    rechargeComponentServer.AddFirstBuy(playId);
                }
            }
            else
            {
                Console.WriteLine($"OnRechage: {unit.Id}   {rechargetType}  {playId}");
            }

            int rechargeNumber = GetRechargeNumber(playId, homeZone);
            long serverTime = TimeHelper.ServerNow();
            long lastRechargeTime = rechargeComponentServer.RechargePro.LastRechargeTime;
            bool newRechargeDay = !ActivityHelper.IsSameGameResetDay(lastRechargeTime, serverTime);
            rechargeComponentServer.RechargePro.LastRechargeTime = serverTime;
            rechargeComponentServer.RechargePro.TotalRechargeNum += rechargeNumber;

            VipHelp.OnVipExpAdd(unit, rechargeNumber * 10);

            taskComponentServer.OnRechargeDay(rechargeNumber, newRechargeDay);

            RoleDailyDataComponentServer daily = unit.GetComponent<RoleDailyDataComponentServer>();
            List<RewardItem> grantRewards = rechargetType == RechargeBizTypeEnum.Diamond ? rewardItems : null;
            rechargeComponentServer.NotifyClient(grantRewards);
        }

        public static async ETTask SendToAccountCenter(long accountId, long userId, int rechargeNumber, string ordinfo, int rechargeType)
        {
            Other2R_RechargeRequest rechargeRequest = new Other2R_RechargeRequest()
            {
                AccountId = accountId,
                RechargeInfo = new RechargeInfo()
                {
                    Amount = rechargeNumber,
                    Time = TimeHelper.ServerNow(),
                    UserId = userId,
                    OrderInfo = ordinfo,
                    RechargeType = rechargeType
                }
            };
            long accountZone = DBHelper.GetRealmCenter();
            R2Other_RechargeResponse saveAccount = (R2Other_RechargeResponse)await ActorMessageSenderComponent.Instance.Call(accountZone, rechargeRequest);
        }


        /// <summary>
        /// /
        /// </summary>
        /// <param name="zone"></param>
        /// <param name="userId"></param>
        /// <param name="rechargeNumber"></param>
        /// <param name="orderInfo"></param>
        /// <param name="paytype"></param>
        /// <param name="rechargeType">0充值钻石 1购买周卡</param>
        /// <returns></returns>
        public static async ETTask OnPaySucessToUnit( int zone, long userId, int rechargeNumber, string orderInfo, int paytype,  int rechargeType)
        {
            Log.Warning($"充值OnPaySucess PlayerState.Game: {zone}   {userId}  rechargeNumber:{rechargeNumber}", true);
            G2M_RechargeResultRequest r2M_RechargeRequest = new G2M_RechargeResultRequest() { RechargeNumber = rechargeNumber, OrderInfo = orderInfo, RechargeType = rechargeType };
            M2G_RechargeResultResponse m2G_RechargeResponse = (M2G_RechargeResultResponse)await ActorLocationSenderComponent.Instance.Call(userId, r2M_RechargeRequest);

            if (m2G_RechargeResponse.Error != ErrorCode.ERR_Success)
            {
                Log.Warning($"充值OnPaySucess PlayerState.None: {zone}   {userId}  rechargeNumber:{rechargeNumber}");
                RoleInfoComponentServer roleInfoComponentServer = await DBHelper.GetPlayerComponent<RoleInfoComponentServer>(userId);
                if (roleInfoComponentServer != null)
                {
                    roleInfoComponentServer.RechargeBuChang = 1;
                    await DBHelper.SavePlayerComponent(userId, roleInfoComponentServer);

                    long accountId = roleInfoComponentServer.RoleInfo.AccInfoID;
                    SendToAccountCenter(accountId, userId, rechargeNumber, orderInfo, rechargeType).Coroutine();
                }
            }

            //&& gateUnitInfo.ClientSession!=null
            await ETTask.CompletedTask;

        }
    }
}

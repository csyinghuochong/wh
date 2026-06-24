using System;
using System.Collections.Generic;

namespace ET
{
    public static class RechargeHelp
    {

        public static void  SendDiamondToUnit(Unit unit, int payid, string orderInfo, int rechargeType)
        {
            //Log.Warning($"RechargeHelp.SendDiamond {unit.Id} {rechargeNumber} {orderInfo}");
            OnRechage(unit, payid, rechargeType, true);
            //long accountId = unit.GetComponent<RoleInfoComponent>().RoleInfo.AccInfoID;
            //long userId = unit.GetComponent<RoleInfoComponent>().RoleInfo.UserId;
            //SendToAccountCenter(accountId, userId, payid, orderInfo, rechargeType).Coroutine();
            unit.GetComponent<DBSaveComponent>().UpdateCacheDB();
        }

        public static void OnRechage(Unit unit, int playId, int rechargetType, bool notice)
        {
            if (playId <= 0)
            { 
                return; 
            }
        
            NumericComponent numericComponent = unit.GetComponent<NumericComponent>();

            Log.Debug($"OnRechage: {unit.Id}   {rechargetType}  {playId}  rechargetType:{rechargetType}");

            string diamondNumber = CommonConfig.GetDiamondNumber(playId, unit.DomainZone());
            List<RewardItem> rewardItems = ItemHelper.GetRewardItems(diamondNumber);
            
            //0 砖石  1周卡
            if (rechargetType == 0)
            {
                unit.GetComponent<BagComponentServer>().OnAddItemData(rewardItems, string.Empty, $"{ItemGetWay.Recharge}_{TimeHelper.ServerNow()}");
            }
            else
            {
                Console.WriteLine($"OnRechage: {unit.Id}   {rechargetType}  {playId}");
            }

            RechargeComponent rechargeComponent = unit.GetComponent<RechargeComponent>();
        
            int rechargeNumber = CommonConfig.GetRechargeNumber(playId, unit.DomainZone());

            long lastRechargeTime = rechargeComponent.RechargePro.LastRechargeTime;
            long serverTime = TimeHelper.ServerNow();
            
            rechargeComponent.OnRecharge(rechargeNumber);

            bool isSameDay = lastRechargeTime > 0
                    && TimeInfo.Instance.ToDateTime(lastRechargeTime).Date
                    == TimeInfo.Instance.ToDateTime(serverTime).Date;
            
            TaskComponent taskComponent = unit.GetComponent<TaskComponent>();

           // if (lastRechargeTime == 0 || !isSameDay)
            {
                taskComponent.TriggerTaskEvent(TastConditionType.RechageDayNumber_113, 1, 30);
            }


            numericComponent.ApplyChange(null, NumericType.RechargeNumber, rechargeNumber, 1, notice);    
            numericComponent.ApplyChange(null, NumericType.V1RechageNumber, rechargeNumber, 0, notice);    
            //充值签到标记，已经领取的不充值
            if (numericComponent.GetAsInt(NumericType.RechargeSign) != 2)
            {
                numericComponent.ApplyValue(NumericType.RechargeSign, 1, notice);
            }
            // 单笔充值奖励记录
            if (!unit.GetComponent<RoleInfoComponent>().RoleInfo.SingleRechargeIds.Contains(rechargeNumber))
            {
                unit.GetComponent<RoleInfoComponent>().RoleInfo.SingleRechargeIds.Add(rechargeNumber);
            }
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
        /// 
        /// </summary>
        /// <param name="scene"></param>
        /// <param name="userId"></param>
        /// <param name="rechargeNumber"></param>
        /// <param name="orderInfo"></param>
        /// <param name="rechargeType">//0充值钻石   1购买周卡</param>
        /// <returns></returns>
        public static async ETTask OnPaySucessToUnit(Scene scene,  long userId, int rechargeNumber, string orderInfo, int rechargeType)
        {
            Player gateUnitInfo = scene.GetComponent<PlayerComponent>().GetByUserId(userId);
            //&& gateUnitInfo.ClientSession!=null
            if (gateUnitInfo != null  && gateUnitInfo.PlayerState == PlayerState.Game && gateUnitInfo.InstanceId > 0)
            {
                Log.Warning($"充值OnPaySucess PlayerState.Game: {scene.DomainZone()}   {userId}  rechargeNumber:{rechargeNumber}  rechargeType:{rechargeType}", true);
                G2M_RechargeResultRequest r2M_RechargeRequest = new G2M_RechargeResultRequest() { RechargeNumber = rechargeNumber , OrderInfo = orderInfo, RechargeType = rechargeType};
                M2G_RechargeResultResponse m2G_RechargeResponse = (M2G_RechargeResultResponse)await ActorLocationSenderComponent.Instance.Call(gateUnitInfo.UnitId, r2M_RechargeRequest);
            }
            else
            {
                Log.Warning($"充值OnPaySucess PlayerState.None: {scene.DomainZone()}   {userId}  rechargeNumber:{rechargeNumber}  rechargeType:{rechargeType}");
                //直接存数据库
                //int number = ComHelp.GetDiamondNumber(rechargeNumber);
                long dbCacheId = DBHelper.GetDbCacheId(scene.DomainZone());
                D2G_GetComponent d2GGetUnit = (D2G_GetComponent)await ActorMessageSenderComponent.Instance.Call(dbCacheId, new G2D_GetComponent() { UnitId = userId, Component = DBHelper.NumericComponent });
                NumericComponent numericComponent = (d2GGetUnit.Component as NumericComponent);
                numericComponent.ApplyChange(null, NumericType.RechargeBuChang, rechargeNumber, 1, false);
                D2M_SaveComponent d2GSave = (D2M_SaveComponent)await ActorMessageSenderComponent.Instance.Call(dbCacheId, new M2D_SaveComponent()
                {
                    UnitId = userId,
                    EntityByte = MongoHelper.ToBson(numericComponent),
                    ComponentType = DBHelper.NumericComponent
                });

                d2GGetUnit = (D2G_GetComponent)await ActorMessageSenderComponent.Instance.Call(dbCacheId, new G2D_GetComponent() { UnitId = userId, Component = DBHelper.RoleInfoComponent });
                RoleInfoComponent roleInfoComponent = (d2GGetUnit.Component as RoleInfoComponent);
                
                long accountId = roleInfoComponent.RoleInfo.AccInfoID;
                SendToAccountCenter(accountId, userId, rechargeNumber, orderInfo, rechargeType).Coroutine();
                await ETTask.CompletedTask;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="scene"></param>
        /// <param name="userId"></param>
        /// <param name="rechargeNumber"></param>
        /// <param name="orderInfo"></param>
        /// <param name="rechargeType">//0充值钻石   1购买周卡</param>
        /// <returns></returns>
        public static async ETTask OnPaySucessToUnit_2(Scene scene, long userId, int rechargeNumber, string orderInfo, int rechargeType)
        {
            Log.Warning($"充值OnPaySucess PlayerState.Game: {scene.DomainZone()}   {userId}  rechargeNumber:{rechargeNumber}", true);
            G2M_RechargeResultRequest r2M_RechargeRequest = new G2M_RechargeResultRequest() { RechargeNumber = rechargeNumber, OrderInfo = orderInfo, RechargeType = rechargeType };
            M2G_RechargeResultResponse m2G_RechargeResponse = (M2G_RechargeResultResponse)await ActorLocationSenderComponent.Instance.Call(userId, r2M_RechargeRequest);

            if (m2G_RechargeResponse.Error != ErrorCode.ERR_Success)
            {
                Log.Warning($"充值OnPaySucess PlayerState.None: {scene.DomainZone()}   {userId}  rechargeNumber:{rechargeNumber}");
                //直接存数据库
                //int number = ComHelp.GetDiamondNumber(rechargeNumber);
                long dbCacheId = DBHelper.GetDbCacheId(scene.DomainZone());
                D2G_GetComponent d2GGetUnit = (D2G_GetComponent)await ActorMessageSenderComponent.Instance.Call(dbCacheId, new G2D_GetComponent() { UnitId = userId, Component = DBHelper.NumericComponent });
                NumericComponent numericComponent = (d2GGetUnit.Component as NumericComponent);
                numericComponent.ApplyChange(null, NumericType.RechargeBuChang, rechargeNumber, 1, false);
                D2M_SaveComponent d2GSave = (D2M_SaveComponent)await ActorMessageSenderComponent.Instance.Call(dbCacheId, new M2D_SaveComponent()
                {
                    UnitId = userId,
                    EntityByte = MongoHelper.ToBson(numericComponent),
                    ComponentType = DBHelper.NumericComponent
                });

                d2GGetUnit = (D2G_GetComponent)await ActorMessageSenderComponent.Instance.Call(dbCacheId, new G2D_GetComponent() { UnitId = userId, Component = DBHelper.RoleInfoComponent });
                RoleInfoComponent roleInfoComponent = (d2GGetUnit.Component as RoleInfoComponent);

                long accountId = roleInfoComponent.RoleInfo.AccInfoID;
                SendToAccountCenter(accountId, userId, rechargeNumber, orderInfo, rechargeType  ).Coroutine();
            }

            //&& gateUnitInfo.ClientSession!=null
            await ETTask.CompletedTask;
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
        public static async ETTask OnPaySucessToGate( int zone, long userId, int rechargeNumber, string orderInfo, int paytype,  int rechargeType)
        {
            long gateServerId = DBHelper.GetGateServerId(zone);
            R2G_RechargeResultRequest r2M_RechargeRequest = new R2G_RechargeResultRequest() {
                RechargeNumber = rechargeNumber,
                UserID = userId ,
                OrderInfo = orderInfo, 
                PayType = paytype,
                RechargeType = rechargeType};
            G2R_RechargeResultResponse m2G_RechargeResponse = (G2R_RechargeResultResponse)await ActorMessageSenderComponent.Instance.Call(gateServerId, r2M_RechargeRequest);
        }
    }
}

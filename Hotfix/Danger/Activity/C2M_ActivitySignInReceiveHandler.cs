using System;
using System.Collections.Generic;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_ActivitySignInReceiveHandler : AMActorLocationRpcHandler<Unit, C2M_ActivitySignInReceiveRequest, M2C_ActivitySignInReceiveResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_ActivitySignInReceiveRequest request, M2C_ActivitySignInReceiveResponse response, Action reply)
        {
            int activityId = ActivityHelper.DailySignActivityId;
            int signInId = request.SignInId > 0 ? request.SignInId : request.ActivityId;
            long now = TimeHelper.ServerNow();

            if (signInId <= 0 || !LDActivity_Sign_InCategory.Instance.Contain(signInId))
            {
                response.Error = ErrorCode.ERR_Error;
                reply();
                await ETTask.CompletedTask;
                return;
            }

            LDActivity_Sign_In cfg = LDActivity_Sign_InCategory.Instance.Get(signInId);
            if (cfg.ActivityId > 0)
            {
                activityId = cfg.ActivityId;
            }

            ActivityComponentServer activity = unit.GetComponent<ActivityComponentServer>();
            RoleInfoComponentServer role = unit.GetComponent<RoleInfoComponentServer>();
            if (ActivityHelper.EnsureSignInLoginDay(activity.ActivityInfo, ref role.LastDailyCountTime, now, activityId))
            {
                unit.GetComponent<DBSaveComponent>()?.UpdateCacheDB();
            }

            if (!ActivityHelper.CanReceiveSignIn(activity.ActivityInfo, cfg, activityId))
            {
                response.Error = ActivityHelper.IsSignInReceived(activity.ActivityInfo, signInId, activityId)
                        ? ErrorCode.ERR_AlreadyReceived
                        : ErrorCode.ERR_Error;
                reply();
                await ETTask.CompletedTask;
                return;
            }

            List<RewardItem> rewards = ItemNewHelper.GetRewardItems(cfg.Reward);
            if (rewards == null || rewards.Count == 0)
            {
                response.Error = ErrorCode.ERR_Error;
                reply();
                await ETTask.CompletedTask;
                return;
            }

            if (!unit.GetComponent<BagComponentServer>().OnAddItemData(rewards, string.Empty, $"{ItemGetWay.Activity}_{now}"))
            {
                response.Error = ErrorCode.ERR_BagIsFull;
                reply();
                await ETTask.CompletedTask;
                return;
            }

            activity.ActivityInfo.SignInReceivedId = signInId;
            unit.GetComponent<DBSaveComponent>()?.UpdateCacheDB();

            response.ReceiveId = signInId;
            response.SignInLoginDays = activity.ActivityInfo.SignInLoginDays;
            response.SignInReceivedId = signInId;
            response.Error = ErrorCode.ERR_Success;
            reply();
            await ETTask.CompletedTask;
        }
    }
}

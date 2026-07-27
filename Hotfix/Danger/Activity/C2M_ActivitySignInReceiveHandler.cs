using System;
using System.Collections.Generic;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_ActivitySignInReceiveHandler : AMActorLocationRpcHandler<Unit, C2M_ActivitySignInReceiveRequest, M2C_ActivitySignInReceiveResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_ActivitySignInReceiveRequest request, M2C_ActivitySignInReceiveResponse response, Action reply)
        {
            int activityId = request.ActivityId > 0 ? request.ActivityId : ActivityHelper.DailySignActivityId;
            long createTime = unit.GetComponent<RoleInfoComponentServer>().RoleInfo.CreateTime;
            long now = TimeHelper.ServerNow();
            int todayId = ActivityHelper.GetTodaySignInId(createTime, activityId, now);
            if (todayId <= 0)
            {
                response.Error = ErrorCode.ERR_Error;
                reply();
                await ETTask.CompletedTask;
                return;
            }

            int signInId = request.SignInId > 0 ? request.SignInId : todayId;
            // 只允许领今天这一档
            if (signInId != todayId)
            {
                response.Error = ErrorCode.ERR_Error;
                reply();
                await ETTask.CompletedTask;
                return;
            }

            if (!LDActivity_Sign_InCategory.Instance.Contain(signInId))
            {
                response.Error = ErrorCode.ERR_Error;
                reply();
                await ETTask.CompletedTask;
                return;
            }

            LDActivity_Sign_In cfg = LDActivity_Sign_InCategory.Instance.Get(signInId);
            if (cfg.ActivityId != activityId)
            {
                response.Error = ErrorCode.ERR_Error;
                reply();
                await ETTask.CompletedTask;
                return;
            }

            ActivityComponentServer activity = unit.GetComponent<ActivityComponentServer>();
            // 同一天不可重复领（跨周期同 Id 也靠日历天区分）
            if (activity.ActivityInfo.LastSignTime > 0 && CommonHelper.GetDaysDiffByDate(activity.ActivityInfo.LastSignTime, now) == 0)
            {
                response.Error = ErrorCode.ERR_AlreadyReceived;
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

            activity.ActivityInfo.SignInReceiveId = signInId;
            activity.ActivityInfo.LastSignTime = now;
            activity.ActivityInfo.TotalSignNumber += 1;
            unit.GetComponent<DBSaveComponent>()?.UpdateCacheDB();

            response.ReceiveId = signInId;
            response.Error = ErrorCode.ERR_Success;
            reply();
            await ETTask.CompletedTask;
        }
    }
}

using System;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_ActivitySignInListHandler : AMActorLocationRpcHandler<Unit, C2M_ActivitySignInListRequest, M2C_ActivitySignInListResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_ActivitySignInListRequest request, M2C_ActivitySignInListResponse response, Action reply)
        {
            int activityId = request.ActivityId > 0 ? request.ActivityId : ActivityHelper.DailySignActivityId;
            ActivityComponentServer activity = unit.GetComponent<ActivityComponentServer>();
            RoleInfoComponentServer role = unit.GetComponent<RoleInfoComponentServer>();
            if (ActivityHelper.EnsureSignInLoginDay(activity.ActivityInfo, ref role.LastDailyCountTime, 0, activityId))
            {
                unit.GetComponent<DBSaveComponent>()?.UpdateCacheDB();
            }

            ActivityInfo info = activity.ActivityInfo;
            int group = ActivityHelper.GetCurrentSignInGroup(info, activityId);
            response.SignInIds = ActivityHelper.GetSignInIdsByGroup(activityId, group);
            response.SignInLoginDays = info.SignInLoginDays;
            response.SignInReceivedId = info.SignInReceivedId;
            response.Error = ErrorCode.ERR_Success;
            reply();
            await ETTask.CompletedTask;
        }
    }
}

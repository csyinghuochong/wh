using System;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_ActivitySignInListHandler : AMActorLocationRpcHandler<Unit, C2M_ActivitySignInListRequest, M2C_ActivitySignInListResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_ActivitySignInListRequest request, M2C_ActivitySignInListResponse response, Action reply)
        {
            int activityId = request.ActivityId > 0 ? request.ActivityId : ActivityHelper.DailySignActivityId;
            long createTime = unit.GetComponent<RoleInfoComponentServer>().RoleInfo.CreateTime;
            long now = TimeHelper.ServerNow();
            int createDays = ActivityHelper.GetCreateRoleDays(createTime, now);
            int group = ActivityHelper.GetSignInGroupByCreateDays(createDays, activityId);

            ActivityComponentServer activity = unit.GetComponent<ActivityComponentServer>();
            response.SignInIds = ActivityHelper.GetSignInIdsByGroup(activityId, group);
            response.ReceiveId = activity.ActivityInfo.SignInReceiveId;
            response.TodayId = ActivityHelper.GetTodaySignInId(createTime, activityId, now);
            response.Group = group;
            response.Error = ErrorCode.ERR_Success;
            reply();
            await ETTask.CompletedTask;
        }
    }
}

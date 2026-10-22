using System;

namespace ET
{

    /// <summary>
    /// 申请添加别人好友
    /// </summary>
    [ActorMessageHandler]
    public class C2F_FriendApplyHandler : AMActorRpcHandler<Scene, C2F_FriendApplyRequest, F2C_FriendApplyResponse>
    {

        protected override async ETTask Run(Scene scene, C2F_FriendApplyRequest request, F2C_FriendApplyResponse response, Action reply)
        {
            int friendZone = FriendHelper.PrepareHomeZone(request.FriendId);
            if (friendZone == 0)
            {
                response.Error = ErrorCode.ERR_PlayerNoFind;
                reply();
                return;
            }
            DBFriendInfo dBFriendInfo = await DBHelper.GetComponent<DBFriendInfo>(friendZone, request.FriendId);

            if (dBFriendInfo == null)
            {
                response.Error = ErrorCode.ERR_PlayerNoFind;
                reply();
                return;
            }
            long applyUserId = request.RoleInfo.UserId;
            if (dBFriendInfo.FriendList.Contains(applyUserId))
            {
                reply();
                return;
            }
            if (!dBFriendInfo.ApplyList.Contains(applyUserId))
            {
                dBFriendInfo.ApplyList.Add(applyUserId);
                DBHelper.SaveComponent(friendZone, dBFriendInfo.Id, dBFriendInfo).Coroutine();
                await ServerMessageHelper.SendToClient(friendZone, request.FriendId,
                    new M2C_FriendApplyResult() { FriendInfo = request.RoleInfo });
                if (applyUserId > 0)
                {
                    MessageHelper.SendToLocationActor(applyUserId, new Friend2M_SendApply());
                }
            }

            int applyZone = FriendHelper.PrepareHomeZone(applyUserId);
            DBFriendInfo applicantInfo = applyZone == 0 ? null : await DBHelper.GetComponent<DBFriendInfo>(applyZone, applyUserId);
            if (applicantInfo != null && !applicantInfo.SendApplyList.Contains(request.FriendId))
            {
                applicantInfo.SendApplyList.Add(request.FriendId);
                DBHelper.SaveComponent(applyZone, applicantInfo.Id, applicantInfo).Coroutine();
            }

            reply();
        }
    }
}

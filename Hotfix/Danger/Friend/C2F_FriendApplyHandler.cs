using System;

namespace ET
{

    [ActorMessageHandler]
    public class C2F_FriendApplyHandler : AMActorRpcHandler<Scene, C2F_FriendApplyRequest, F2C_FriendApplyResponse>
    {

        protected override async ETTask Run(Scene scene, C2F_FriendApplyRequest request, F2C_FriendApplyResponse response, Action reply)
        {
            DBFriendInfo dBFriendInfo = await DBHelper.GetComponent<DBFriendInfo>(scene.DomainZone(), request.UserID);

            if (dBFriendInfo == null)
            {
                ///dBFriendInfo = (DBFriendInfo)await DBHelper.AddDataComponent<DBFriendInfo>(scene.DomainZone(), request.UserID, DBHelper.DBFriendInfo);
                //Log.Error($"C2F_FriendApplyRequest.1");
                response.Error = ErrorCode.ERR_NonePlayerError;
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
                DBHelper.SaveComponent(scene.DomainZone(), dBFriendInfo.Id, dBFriendInfo).Coroutine();
                await ServerMessageHelper.SendToClient(scene.DomainZone(), request.UserID,
                    new M2C_FriendApplyResult() { FriendInfo = request.RoleInfo });
            }

            reply();
        }
    }
}

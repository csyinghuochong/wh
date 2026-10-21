using System;


namespace ET
{

    //回复别人的好友申请
    [ActorMessageHandler]
    public class C2F_FriendApplyReplyHandler : AMActorRpcHandler<Scene, C2F_FriendApplyReplyRequest, F2C_FriendApplyReplyResponse>
    {
        protected override async ETTask Run(Scene scene, C2F_FriendApplyReplyRequest request, F2C_FriendApplyReplyResponse response, Action reply)
        {
            DBFriendInfo dBFriendInfo = await DBHelper.GetComponent<DBFriendInfo>(scene.DomainZone(), request.UserID);
            if (dBFriendInfo == null)
            {
                response.Error = ErrorCode.ERR_UserNoFind;
                reply();
                return;
            }

            bool pending = dBFriendInfo.ApplyList.Remove(request.FriendID);
            if (request.ReplyCode == 1 && !pending)
            {
                response.Error = ErrorCode.ERR_ModifyData;
                reply();
                return;
            }

            DBFriendInfo applicantInfo = await DBHelper.GetComponent<DBFriendInfo>(scene.DomainZone(), request.FriendID);
            bool applicantChanged = false;
            if (request.ReplyCode == 1)
            {
                if (!dBFriendInfo.FriendList.Contains(request.FriendID))
                {
                    dBFriendInfo.FriendList.Add(request.FriendID);
                }

                if (applicantInfo != null && !applicantInfo.FriendList.Contains(request.UserID))
                {
                    applicantInfo.FriendList.Add(request.UserID);
                    applicantChanged = true;
                }
            }

            if (applicantInfo != null && applicantInfo.SendApplyList.Remove(request.UserID))
            {
                applicantChanged = true;
                await ServerMessageHelper.SendToClient(scene.DomainZone(), request.FriendID,
                    new M2C_FriendSendApplyChange() { UserId = request.UserID, ReplyCode = request.ReplyCode });
            }

            if (applicantChanged)
            {
                await DBHelper.SaveComponent(scene.DomainZone(), request.FriendID, applicantInfo);
            }

            await DBHelper.SaveComponent(scene.DomainZone(), request.UserID, dBFriendInfo);
            reply();
        }
    }
}

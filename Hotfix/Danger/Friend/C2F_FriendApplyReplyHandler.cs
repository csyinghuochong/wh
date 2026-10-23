using System;


namespace ET
{

    //回复别人的好友申请
    [ActorMessageHandler]
    public class C2F_FriendApplyReplyHandler : AMActorRpcHandler<Scene, C2F_FriendApplyReplyRequest, F2C_FriendApplyReplyResponse>
    {
        protected override async ETTask Run(Scene scene, C2F_FriendApplyReplyRequest request, F2C_FriendApplyReplyResponse response, Action reply)
        {
            int selfZone = FriendHelper.PrepareHomeZone(request.UserID);
            if (selfZone == 0)
            {
                response.Error = ErrorCode.ERR_PlayerNoFind;
                reply();
                return;
            }
            DBFriendInfo dBFriendInfo = await DBHelper.GetComponent<DBFriendInfo>(selfZone, request.UserID);
            if (dBFriendInfo == null)
            {
                response.Error = ErrorCode.ERR_PlayerNoFind;
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

            int friendZone = FriendHelper.PrepareHomeZone(request.FriendID);
            DBFriendInfo applicantInfo = friendZone == 0 ? null : await DBHelper.GetComponent<DBFriendInfo>(friendZone, request.FriendID);
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
                await ServerMessageHelper.SendToClient(friendZone, request.FriendID,
                    new M2C_FriendSendApplyChange() { UserId = request.UserID, ReplyCode = request.ReplyCode });
            }

            if (applicantChanged)
            {
                await DBHelper.SaveComponent(friendZone, request.FriendID, applicantInfo);
            }

            await DBHelper.SaveComponent(selfZone, request.UserID, dBFriendInfo);
            if (request.ReplyCode == 1)
            {
                M2C_SyncChatInfo m2C_SyncChatInfo = new M2C_SyncChatInfo()
                {
                    ChatInfo = new ChatInfo()
                    {
                        UserId = request.UserID,
                        ChannelId = ChannelEnum.System,
                        ChatMsg = "添加好友成功",
                        Time = TimeHelper.ServerNow(),
                    }
                };
                await ServerMessageHelper.SendToClient(selfZone, request.UserID, m2C_SyncChatInfo);
            }
            reply();
        }
    }
}

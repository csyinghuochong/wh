using System;
using System.Collections.Generic;

namespace ET
{

    [ActorMessageHandler]
    public class C2Chat_SendChatHandler : AMActorRpcHandler<ChatInfoUnit, C2C_SendChatRequest, C2C_SendChatResponse>
    {

        protected override async ETTask Run(ChatInfoUnit chatInfoUnit, C2C_SendChatRequest request, C2C_SendChatResponse response, Action reply)
        {
            using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.Chat, chatInfoUnit.Id))
            {
                if (string.IsNullOrEmpty(request.ChatInfo.ChatMsg))
                {
                    Log.Error($"C2Chat_SendChatHandler.1");
                    response.Error = ErrorCode.ERR_ModifyData;
                    reply();
                    return;
                }

                long serverTime = TimeHelper.ServerNow();
              
                chatInfoUnit.LastSendChat = serverTime;
                request.ChatInfo.UserId = chatInfoUnit.Id;
                M2C_SyncChatInfo m2C_SyncChatInfo = new M2C_SyncChatInfo();
                request.ChatInfo.Time = serverTime;
                request.ChatInfo.PlayerName = chatInfoUnit.Name;
                request.ChatInfo.ServerId = UnitZoneHelper.GetHomeZone(chatInfoUnit.Id);
                request.ChatInfo.RoleHeadId = chatInfoUnit.RoleHeadId;
                request.ChatInfo.RoleHeadFrame = chatInfoUnit.RoleHeadFrame;
                m2C_SyncChatInfo.ChatInfo = request.ChatInfo;
                ChatSceneComponent chatScene = chatInfoUnit.DomainScene().GetComponent<ChatSceneComponent>();
                int zone = chatInfoUnit.DomainZone();
                bool chatSent = false;
                switch (request.ChatInfo.ChannelId)
                {
                    case (int)ChannelEnum.WarZone:
                    {
                        WZChatSceneComponent wzChat = chatInfoUnit.DomainScene().GetComponent<WZChatSceneComponent>();
                        if (wzChat == null)
                        {
                            response.Error = ErrorCode.ERR_ModifyData;
                            reply();
                            return;
                        }

                        LogHelper.ChatInfo($"战区:{chatInfoUnit.DomainZone()}    {request.ChatInfo.PlayerName}:  {request.ChatInfo.ChatMsg} ");
                        wzChat.BroadcastWarChat(request.ChatInfo);
                        chatSent = true;
                        break;
                    }
                    case (int)ChannelEnum.Zone:
                        BeReportedInfo bePortedNumber = null;
                        chatScene.BeReportedNumber.TryGetValue(request.ChatInfo.UserId, out bePortedNumber);
                        if (bePortedNumber != null && bePortedNumber.JinYanTime > serverTime)
                        {
                            long leftTime = bePortedNumber.JinYanTime - serverTime;
                            int hour = (int)(leftTime * 1f / TimeHelper.Hour);
                            hour = Math.Max(hour, 1);
                            response.Error = ErrorCode.ERR_Chat_JinYan_3;
                            response.Message = $"你当前已经被禁言，{hour}小时后解封！";
                            reply();
                            return;
                        }
                        if (bePortedNumber != null && bePortedNumber.JinYanTime != 0 && bePortedNumber.JinYanTime <= serverTime)
                        {
                            chatScene.BeReportedNumber.Remove(request.ChatInfo.UserId);
                        }

                        LogHelper.ChatInfo($"区:{chatInfoUnit.DomainZone()}    {request.ChatInfo.PlayerName}:  {request.ChatInfo.ChatMsg} ");

                        foreach (var otherUnit in chatScene.ChatInfoUnitsDict.Values)
                        {
                            MessageHelper.SendActor(otherUnit.GateSessionActorId, m2C_SyncChatInfo);
                        }

                        chatScene.WordChatInfos.Add(request.ChatInfo);
                        if (chatScene.WordChatInfos.Count > 10)
                        {
                            chatScene.WordChatInfos.RemoveAt(0);
                        }
                        chatSent = true;
                        break;
                    case (int)ChannelEnum.Team:
                        long teamServerId = StartSceneConfigCategory.Instance.GetBySceneName(chatInfoUnit.DomainZone(), Enum.GetName(SceneType.Team)).InstanceId;
                        T2Chat_GetTeamInfoResponse g_SendChatRequest1 = (T2Chat_GetTeamInfoResponse)await ActorMessageSenderComponent.Instance.Call
                            (teamServerId, new Chat2T_GetTeamInfoRequest() { UserID = request.ChatInfo.UserId });

                        if (g_SendChatRequest1.Error == 0 && g_SendChatRequest1.TeamInfo != null)
                        {
                            for (int i = 0; i < g_SendChatRequest1.TeamInfo.PlayerList.Count; i++)
                            {
                                await ServerMessageHelper.SendToClient(zone, g_SendChatRequest1.TeamInfo.PlayerList[i].UserID, m2C_SyncChatInfo);
                            }
                        }

                        chatSent = true;
                        break;
                    case (int)ChannelEnum.Union:
                        long unionid = chatInfoUnit.UnionId;
                        if (unionid == 0)
                        {
                            response.Error = ErrorCode.ERR_Union_Not_Exist;
                            reply();
                            return;
                        }
                        if (chatScene.UnionUnits.TryGetValue(unionid, out List<ChatInfoUnit> members))
                        {
                            for (int i = 0; i < members.Count; i++)
                            {
                                MessageHelper.SendActor(members[i].GateSessionActorId, m2C_SyncChatInfo);
                            }
                        }
                        chatSent = true;
                        break;
                    case (int)ChannelEnum.Friend:
                        long friendId = request.ChatInfo.ParamId;
                        int myZone = FriendHelper.PrepareHomeZone(chatInfoUnit.Id);
                        DBFriendInfo myFriendInfo = myZone == 0 ? null : await DBHelper.GetComponent<DBFriendInfo>(myZone, chatInfoUnit.Id);
                        if (friendId == 0 || myFriendInfo == null || !myFriendInfo.FriendList.Contains(friendId))
                        {
                            response.Error = ErrorCode.ERR_ModifyData;
                            reply();
                            return;
                        }
                        int friendZone = FriendHelper.PrepareHomeZone(friendId);
                        if (friendZone == 0 || !await ServerMessageHelper.SendToClient(friendZone, friendId, m2C_SyncChatInfo))
                        {
                            DBFriendInfo dBFriendInfo = friendZone == 0 ? null : await DBHelper.GetComponent<DBFriendInfo>(friendZone, friendId);
                            if (dBFriendInfo != null && dBFriendInfo.FriendChats.Count < 10)
                            {
                                dBFriendInfo.FriendChats.Add(request.ChatInfo);
                                DBHelper.SaveComponent(friendZone, friendId, dBFriendInfo).Coroutine();
                            }
                        }

                        await ServerMessageHelper.SendToClient(myZone, chatInfoUnit.Id, m2C_SyncChatInfo);
                        chatSent = true;
                        break;
                }

                if (chatSent)
                {
                    MessageHelper.SendToLocationActor(chatInfoUnit.Id, new Chat2M_SendChat()
                    {
                        ChannelId = request.ChatInfo.ChannelId,
                    });
                }

                reply();
            }
        }
    }
}

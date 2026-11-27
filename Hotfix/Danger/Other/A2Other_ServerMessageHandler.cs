using System;
using System.Collections.Generic;

namespace ET
{

    //服务器之间通用通信协议
    [ActorMessageHandler]
    public class A2Other_ServerMessageHandler : AMActorRpcHandler<Scene, A2Other_ServerMessageRequest, Other2A_ServerMessageRResponse>
    {

        protected override async ETTask Run(Scene scene, A2Other_ServerMessageRequest request, Other2A_ServerMessageRResponse response, Action reply)
        {
            try
            {
                switch (scene.SceneType)
                {
                    case SceneType.Gate:
                        if (request.MessageType == ServerNoticeType.StopSever)
                        {
                            string[] shutdown = (request.MessageValue ?? string.Empty).Split('_');
                            if (shutdown.Length == 2 && shutdown[0] == "notify")
                            {
                                if (!long.TryParse(shutdown[1], out long stopAt) || stopAt <= 0)
                                {
                                    response.Error = LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_Invalid_Data);
                                    break;
                                }
                                StopServerConsoleHandler.StopGate(scene);
                                M2C_GameNotice notice = new M2C_GameNotice
                                {
                                    Error = LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Login_Stop_Server),
                                    UpdateType = ServerNoticeType.StopSever,
                                    UpdateTypeValue = stopAt.ToString(),
                                    Message = "服务器即将停服维护，请返回登录。",
                                };
                                foreach (Player player in scene.GetComponent<PlayerComponent>().GetAll())
                                {
                                    player.ClientSession?.Send(notice);
                                }
                                break;
                            }
                            StopServerConsoleHandler.SaveScene(scene, request.MessageValue, response);
                        }
                        if (request.MessageType == ServerNoticeType.Archive)
                        {
                            // MessageValue = $"{acccout} {unitid} {archive}",
                            string[] sssvalue = request.MessageValue.Split(' ');
                            string account = sssvalue[0];
                            long unitid = long.Parse(sssvalue[1]);
                            int archive = int.Parse(sssvalue[2]);
                            scene.GetComponent<PlayerInfoListComponent>().OnAddArchive(account, unitid, archive);
                        }
                        break;
                    case SceneType.Team:
                        if (request.MessageType == ServerNoticeType.PlayerExit)
                        {
                            long userId = long.Parse(request.MessageValue);
                            TeamSceneComponent team = scene.GetComponent<TeamSceneComponent>();
                            foreach (long teamId in new List<long>(team.ApplyDict.Keys))
                            {
                                team.RemoveApply(teamId, userId);
                            }
                            await team.OnRecvUnitLeave(userId);
                        }
                        break;
                    case SceneType.FubenWork:
                        if (request.MessageType == ServerNoticeType.PlayerExit)
                        {
                            scene.GetComponent<SoloSceneComponent>().OnRecvUnitLeave(long.Parse(request.MessageValue));
                        }
                        break;
                    case SceneType.Rank:
                    case SceneType.WZRank:
                    case SceneType.Consign:
                    case SceneType.Union:
                    case SceneType.Mail:
                    case SceneType.Activity:
                    case SceneType.DBCache:
                        if (request.MessageType == ServerNoticeType.StopSever)
                        {
                            StopServerConsoleHandler.SaveScene(scene, request.MessageValue, response);
                        }
                        break;
                    case SceneType.WZChat:
                        WZChatSceneComponent wzChat = scene.GetComponent<WZChatSceneComponent>();
                        if (request.MessageType == ServerNoticeType.PlayerExit)
                        {
                            long wzUnitId = long.Parse(request.MessageValue);
                            if (wzChat.Get(wzUnitId) != null)
                            {
                                wzChat.Remove(wzUnitId);
                            }
                        }
                        break;
                    case SceneType.Chat:
                        if (request.MessageType == ServerNoticeType.StopSever)
                        {
                            StopServerConsoleHandler.SaveScene(scene, request.MessageValue, response);
                            break;
                        }
                        ChatSceneComponent chatInfoUnitsComponent = scene.GetComponent<ChatSceneComponent>();
                        if (request.MessageType == ServerNoticeType.PlayerExit)
                        {
                            long unitid = long.Parse(request.MessageValue);
                            if (chatInfoUnitsComponent.Get(unitid) != null)
                            {
                                chatInfoUnitsComponent.Remove(unitid);
                            }
                        }
                        else if (request.MessageType == ServerNoticeType.ClearChat)
                        {
                            Console.WriteLine("清空聊天:  " + scene.DomainZone());
                            chatInfoUnitsComponent.WordChatInfos.Clear();
                        }
                        else if (request.MessageType == ServerNoticeType.JinYan)
                        {
                            BeReportedInfo bePortedNumber;
                            string[] jinyaninfo = request.MessageValue.Split(' ');
                            long JinYanId = long.Parse(jinyaninfo[2]);
                            chatInfoUnitsComponent.BeReportedNumber.TryGetValue(JinYanId, out bePortedNumber);

                            if (bePortedNumber == null)
                            {
                                bePortedNumber = new BeReportedInfo();
                                chatInfoUnitsComponent.BeReportedNumber.Add(JinYanId, bePortedNumber);
                            }
                            bePortedNumber.JinYanTime = TimeHelper.ServerNow() + TimeHelper.OneDay * 3;
                        }
                        break;
                    case SceneType.Realm:
                        if (request.MessageType != ServerNoticeType.StopSever)
                        {
                            break;
                        }
                        string[] messagevalue = (request.MessageValue ?? string.Empty).Split('_');
                        if (messagevalue[0] == "0")
                        {
                            if (messagevalue.Length != 2 || !int.TryParse(messagevalue[1], out int minutes) || minutes < 0)
                            {
                                response.Error = LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_Invalid_Data);
                                response.Message = "倒计时分钟数须为非负整数。";
                                break;
                            }
                            StopServerConsoleHandler.StartMaintenance(scene, minutes, response);
                        }
                        else if (messagevalue[0] == "1" && messagevalue.Length == 1)
                        {
                            scene.GetComponent<CenterServerComponent>().StopServer = false;
                        }
                        else if (messagevalue[0] == "2" && messagevalue.Length == 1)
                        {
                            Game.EventSystem.Publish(new EventType.GenerateSerials() { AccountCenterScene = scene });
                        }
                        else
                        {
                            response.Error = LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_Invalid_Data);
                        }
                        break;
                    default:
                        break;
                }

                reply();
                await ETTask.CompletedTask;
            }
            catch (Exception e)
            { 
                Log.Error(e);
                response.Error = ErrorCore.ERR_RpcFail;
                response.Message = "服务器指令执行失败，请检查服务端日志。";
                reply();
            }
        }
    }
}

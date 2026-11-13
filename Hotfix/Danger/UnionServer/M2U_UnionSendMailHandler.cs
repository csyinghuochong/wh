using System;
using System.Collections.Generic;

namespace ET
{
    [ActorMessageHandler]
    public class M2U_UnionSendMailHandler : AMActorRpcHandler<Scene, M2U_UnionSendMailRequest, U2M_UnionSendMailResponse>
    {
        protected override async ETTask Run(Scene scene, M2U_UnionSendMailRequest request, U2M_UnionSendMailResponse response, Action reply)
        {
            DBUnionInfo dbUnionInfo = await scene.GetComponent<UnionSceneComponent>().GetDBUnionInfo(request.UnionId);
            UnionInfo unionInfo = dbUnionInfo?.UnionInfo;
            if (unionInfo == null)
            {
                response.Error = ErrorCode.ERR_Union_Not_Exist;
                reply();
                return;
            }

            UnionPlayerInfo sender = UnionHelper.GetUnionPlayerInfo(unionInfo.UnionPlayerList, request.UnitId);
            if (sender == null || unionInfo.LeaderId != request.UnitId)
            {
                response.Error = ErrorCode.ERR_Union_NotLeader;
                reply();
                return;
            }

            if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Content))
            {
                response.Error = ErrorCode.ERR_ModifyData;
                reply();
                return;
            }

            // 发送前固定当前成员名单，去重；异步发信期间成员列表可能发生变化。
            HashSet<long> recipients = new HashSet<long>();
            foreach (UnionPlayerInfo member in unionInfo.UnionPlayerList)
            {
                if (member != null && member.UserID > 0)
                {
                    recipients.Add(member.UserID);
                }
            }

            long sendTime = TimeHelper.ServerNow();
            string from = sender.PlayerName;
            foreach (long userId in recipients)
            {
                // 每位成员使用独立的邮件、附件列表和参数列表。
                MailInfo mailInfo = new MailInfo
                {
                    Status = 0,
                    ConfigId = 0,
                    MailId = IdGenerater.Instance.GenerateId(),
                    BelongId = BelongReddot.Belong_10203,
                    Form = from,
                    SendTime = sendTime,
                    ValidTime = sendTime + TimeHelper.OneDay * 30,
                };
                mailInfo.ParamList.Add(request.Title);
                mailInfo.ParamList.Add(request.Content);

                try
                {
                    int error = await MailHelp.SendUserMail(UnitZoneHelper.GetHomeZone(userId), userId, mailInfo);
                    if (error != ErrorCode.ERR_Success)
                    {
                        if (response.Error == ErrorCode.ERR_Success)
                        {
                            response.Error = error;
                        }
                        Log.Warning($"公会邮件发送失败 unionId={request.UnionId} userId={userId} error={error}");
                    }
                }
                catch (Exception e)
                {
                    if (response.Error == ErrorCode.ERR_Success)
                    {
                        response.Error = ErrorCore.ERR_RpcFail;
                    }
                    Log.Error($"公会邮件发送异常 unionId={request.UnionId} userId={userId}: {e}");
                }
            }

            reply();
        }
    }
}

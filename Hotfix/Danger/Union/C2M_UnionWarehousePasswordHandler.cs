using System;
using System.Collections.Generic;

namespace ET
{

    [ActorMessageHandler]
    public class C2M_UnionWarehousePasswordHandler : AMActorLocationRpcHandler<Unit, C2M_UnionWarehousePasswordRequest, M2C_UnionWarehousePasswordResponse>
    {

        protected override async ETTask Run(Unit unit, C2M_UnionWarehousePasswordRequest request, M2C_UnionWarehousePasswordResponse response, Action reply)
        {
            long unionId = unit.GetComponent<NumericComponent>().GetAsLong(NumericType.UnionId_0);
            if (unionId == 0)
            {
                response.Error = ErrorCode.ERR_Union_Not_Exist;
                reply();
                return;
            }

            if (string.IsNullOrEmpty(request.NewPassword))
            {
                response.Error = ErrorCode.ERR_Union_WarehousePassword;
                reply();
                return;
            }

            U2M_UnionWarehousePasswordResponse inner = (U2M_UnionWarehousePasswordResponse)await ActorMessageSenderComponent.Instance.Call(
                DBHelper.GetUnionServerId(unit),
                new M2U_UnionWarehousePasswordRequest()
                {
                    UnionId = unionId,
                    UnitId = unit.Id,
                    OldPassword = request.OldPassword ?? string.Empty,
                    NewPassword = request.NewPassword,
                });
            response.Error = inner.Error;
            response.HasWarehousePassword = inner.HasWarehousePassword;
            reply();

            // 先回包：邮件属于附加逻辑，发失败不影响改密码的结果
            if (inner.Error == ErrorCode.ERR_Success)
            {
                await SendPasswordChangedMail(unit, unionId);
            }
        }

        /// <summary>
        /// 修改成功后给公会全体成员各发一封通知邮件。
        /// 成员名单取公会服保存的 DBUnionInfo（直连 Mongo 读，避免为此新增一条跨服协议）；
        /// 若要以公会服内存缓存为准，可把这段逻辑挪到 M2U_UnionWarehousePasswordHandler。
        /// </summary>
        private static async ETTask SendPasswordChangedMail(Unit unit, long unionId)
        {
            try
            {
                DBUnionInfo dBUnionInfo = await DBHelper.GetComponent<DBUnionInfo>(UnitZoneHelper.GetHomeZone(unit), unionId);
                List<UnionPlayerInfo> members = dBUnionInfo?.UnionInfo?.UnionPlayerList;
                if (members == null || members.Count == 0)
                {
                    return;
                }

                // 每人一封独立邮件：MailId 必须唯一，ParamList / ItemList 不能共用同一个对象
                HashSet<long> sentUserIds = new HashSet<long>();
                for (int i = 0; i < members.Count; i++)
                {
                    long userId = members[i]?.UserID ?? 0;
                    if (userId <= 0 || !sentUserIds.Add(userId))
                    {
                        continue;
                    }

                    MailInfo mailInfo = new MailInfo();
                    mailInfo.Status = 0;
                    mailInfo.ConfigId = 0;
                    mailInfo.MailId = IdGenerater.Instance.GenerateId();
                    mailInfo.Form = "工会邮件";
                    mailInfo.ValidTime = TimeHelper.ServerNow() + TimeHelper.OneDay * 30;
                    // TODO: 配置表补齐后换成真实模板参数（如会长名、修改时间）
                    mailInfo.ParamList.Add("工会仓库密码已修改");
                    mailInfo.ParamList.Add("工会仓库密码已修改");

                    int error = await MailHelp.SendUserMail(UnitZoneHelper.GetHomeZone(userId), userId, mailInfo);
                    if (error != ErrorCode.ERR_Success)
                    {
                        Log.Warning($"公会仓库密码邮件发送失败 unionId={unionId} userId={userId} error={error}");
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error($"公会仓库密码邮件异常 unionId={unionId}: {e}");
            }
        }
    }
}

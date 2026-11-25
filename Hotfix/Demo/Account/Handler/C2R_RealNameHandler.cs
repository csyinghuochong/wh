using System;

namespace ET
{

    [MessageHandler]
    public class C2R_RealNameHandler : AMRpcHandler<C2R_RealNameRequest, R2C_RealNameResponse>
    {

        protected override async ETTask Run(Session session, C2R_RealNameRequest request, R2C_RealNameResponse response, Action reply)
        {
            response.Error = LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_Operation_Fail);
            if (session.DomainScene().SceneType != SceneType.Realm || request.AccountId <= 0
                || string.IsNullOrWhiteSpace(request.IdCardNO) || string.IsNullOrWhiteSpace(request.Name))
            {
                Console.WriteLine($"[实名认证] 请求校验失败: AccountId={request.AccountId}, SceneType={session.DomainScene().SceneType}, 姓名为空={string.IsNullOrWhiteSpace(request.Name)}, 身份证为空={string.IsNullOrWhiteSpace(request.IdCardNO)}");
                reply();
                return;
            }
            if (request.IdCardNO.Length != 18)
            {
                Console.WriteLine($"[实名认证] 身份证长度错误: AccountId={request.AccountId}, Length={request.IdCardNO.Length}");
                reply();
                return;
            }

            // 外部核验可能超过接入连接的 5 秒超时；同账号的实名请求依次执行。
            session.RemoveComponent<SessionAcceptTimeoutComponent>();
            using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.LoginAccount, request.AccountId))
            {
                if (session.IsDisposed)
                {
                    Console.WriteLine($"[实名认证] 等待账号锁后连接已断开: AccountId={request.AccountId}");
                    reply();
                    return;
                }

                using DBCenterAccountInfo dbCenterAccountInfo = await DBHelper.GetComponent<DBCenterAccountInfo>(CommonConfig.CenterZoneId, request.AccountId);
                if (session.IsDisposed || dbCenterAccountInfo == null)
                {
                    Console.WriteLine($"[实名认证] 账号读取失败: AccountId={request.AccountId}, 连接断开={session.IsDisposed}, 账号不存在={dbCenterAccountInfo == null}");
                    reply();
                    return;
                }

                // 内网沿用测试放行；外网直接等待阿里云核验，只有明确认证通过才保存实名信息。
                if (!CommonHelper.IsInnerNet())
                {
                    RealNameCode result = await FangChenMiHelper.OnDoFangchenmi_2(request.IdCardNO, request.Name);
                    if (session.IsDisposed || result == null || result.errcode != 0
                        || result.data == null || result.data.result == null || result.data.result.status != 0)
                    {
                        Console.WriteLine($"[实名认证] 阿里云核验未通过: AccountId={request.AccountId}, 连接断开={session.IsDisposed}, 无有效结果={result == null}, Error={response.Error}");
                        reply();
                        return;
                    }
                }

                PlayerInfo playerInfo = dbCenterAccountInfo.PlayerInfo;
                playerInfo.Name = request.Name;
                playerInfo.IdCardNo = request.IdCardNO;
                playerInfo.RealNameV2 = 1;
                await DBHelper.SaveComponent(CommonConfig.CenterZoneId, dbCenterAccountInfo.Id, dbCenterAccountInfo);
                response.Error = ErrorCode.ERR_Success;
                Console.WriteLine($"[实名认证] 核验通过并保存成功: AccountId={request.AccountId}, 内网={CommonHelper.IsInnerNet()}");
            }
            reply();
        }
    } 
}

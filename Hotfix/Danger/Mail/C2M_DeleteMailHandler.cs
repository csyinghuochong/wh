using System;

namespace ET
{
    /// <summary>
    /// 删除当前这一封邮件。
    /// </summary>
    [ActorMessageHandler]
    public class C2M_DeleteMailHandler : AMActorLocationRpcHandler<Unit, C2M_DeleteMailRequest, M2C_DeleteMailResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_DeleteMailRequest request, M2C_DeleteMailResponse response, Action reply)
        {
            using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.Received, unit.Id))
            {
                int zone = UnitZoneHelper.GetHomeZone(unit);
                DBMailInfo dBMailInfo = await DBHelper.GetComponent<DBMailInfo>(zone, unit.Id);
                if (dBMailInfo == null)
                {
                    response.Error = LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Mail_Not_Exist);
                    reply();
                    return;
                }

                MailInfo mailInfo = MailHelp.FindMail(dBMailInfo.MailInfoList, request.MailId);
                if (mailInfo == null)
                {
                    response.Error = LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Mail_Not_Exist);
                    reply();
                    return;
                }

                dBMailInfo.MailInfoList.Remove(mailInfo);
                await DBHelper.SaveComponent(zone, unit.Id, dBMailInfo);
            }

            reply();
        }
    }
}

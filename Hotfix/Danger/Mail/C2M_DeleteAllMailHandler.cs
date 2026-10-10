using System;
using System.Collections.Generic;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_DeleteAllMailHandler : AMActorLocationRpcHandler<Unit, C2M_DeleteAllMailRequest, M2C_DeleteAllMailResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_DeleteAllMailRequest request, M2C_DeleteAllMailResponse response, Action reply)
        {
            int noDelete = LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Mail_No_Delete);
            using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.Received, unit.Id))
            {
                if (request.BelongId <= 0)
                {
                    response.Error = noDelete;
                    reply();
                    return;
                }

                int zone = UnitZoneHelper.GetHomeZone(unit);
                DBMailInfo dBMailInfo = await DBHelper.GetComponent<DBMailInfo>(zone, unit.Id);
                if (dBMailInfo == null)
                {
                    response.Error = noDelete;
                    reply();
                    return;
                }

                bool changed = false;
                List<MailInfo> mailList = dBMailInfo.MailInfoList;
                for (int i = mailList.Count - 1; i >= 0; i--)
                {
                    MailInfo mailInfo = mailList[i];
                    if (MailHelp.GetBelongId(mailInfo) != request.BelongId || MailHelp.HasUnclaimedReward(mailInfo))
                    {
                        continue;
                    }

                    mailList.RemoveAt(i);
                    changed = true;
                }

                if (!changed)
                {
                    response.Error = noDelete;
                    reply();
                    return;
                }

                await DBHelper.SaveComponent(zone, unit.Id, dBMailInfo);
            }

            reply();
        }
    }
}

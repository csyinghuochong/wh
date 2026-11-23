using System;

namespace ET
{
    /// <summary>
    /// 一键领取当前页签。清理过期及附件缺失类型的旧邮件，发放未领奖励并标已读。
    /// </summary>
    [ActorMessageHandler]
    public class C2M_ReceiveAllMailHandler : AMActorLocationRpcHandler<Unit, C2M_ReceiveAllMailRequest, M2C_ReceiveAllMailResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_ReceiveAllMailRequest request, M2C_ReceiveAllMailResponse response, Action reply)
        {
            using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.Received, unit.Id))
            {
                if (request.BelongId <= 0)
                {
                    reply();
                    return;
                }

                int zone = UnitZoneHelper.GetHomeZone(unit);
                DBMailInfo dBMailInfo = await DBHelper.GetComponent<DBMailInfo>(zone, unit.Id);
                if (dBMailInfo == null)
                {
                    reply();
                    return;
                }

                bool changed = MailHelp.RemoveExpiredMails(dBMailInfo.MailInfoList, TimeHelper.ServerNow());
                changed |= dBMailInfo.MailInfoList.RemoveAll(mailInfo => mailInfo != null
                    && MailHelp.GetBelongId(mailInfo) == request.BelongId
                    && mailInfo.ItemList.Exists(item => item.ItemType == ItemBigType.Type_None)) > 0;
                BagComponentServer bag = unit.GetComponent<BagComponentServer>();
                for (int i = 0; i < dBMailInfo.MailInfoList.Count; i++)
                {
                    MailInfo mailInfo = dBMailInfo.MailInfoList[i];
                    if (mailInfo == null || MailHelp.GetBelongId(mailInfo) != request.BelongId)
                    {
                        continue;
                    }

                    if (!mailInfo.IsRead)
                    {
                        mailInfo.IsRead = true;
                        changed = true;
                    }

                    if (MailHelp.HasUnclaimedReward(mailInfo))
                    {
                        if (!MailHelp.GrantMailReward(bag, mailInfo))
                        {
                            response.Error = LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Bag_Full);
                            break;
                        }

                        changed = true;
                    }
                }

                if (changed)
                {
                    await DBHelper.SaveComponent(zone, unit.Id, dBMailInfo);
                }
            }

            reply();
        }
    }
}

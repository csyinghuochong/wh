using System;

namespace ET
{

    [ActorMessageHandler]
    public class C2M_ReceiveMailHandler : AMActorLocationRpcHandler<Unit, C2M_ReceiveMailRequest, M2C_ReceiveMailResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_ReceiveMailRequest request, M2C_ReceiveMailResponse response, Action reply)
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

                // 旧附件缺失类型时删除整封邮件，不凭道具 ID 推测类型。
                if (MailHelp.IsMailExpired(mailInfo, TimeHelper.ServerNow())
                    || mailInfo.ItemList.Exists(item => item.ItemType == ItemBigType.Type_None))
                {
                    dBMailInfo.MailInfoList.Remove(mailInfo);
                    await DBHelper.SaveComponent(zone, unit.Id, dBMailInfo);
                    response.Error = LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Mail_Not_Exist);
                    reply();
                    return;
                }

                if (!MailHelp.HasUnclaimedReward(mailInfo))
                {
                    reply();
                    return;
                }

                if (!MailHelp.GrantMailReward(unit.GetComponent<BagComponentServer>(), mailInfo))
                {
                    response.Error = LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Bag_Full);
                    reply();
                    return;
                }
                await DBHelper.SaveComponent(zone, unit.Id, dBMailInfo);
            }

            reply();
            await ETTask.CompletedTask;
        }
    }
}

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
                    response.Error = ErrorCode.ERR_MailNotExist;
                    reply();
                    return;
                }

                MailInfo mailInfo = MailHelp.FindMail(dBMailInfo.MailInfoList, request.MailId);
                if (mailInfo == null)
                {
                    response.Error = ErrorCode.ERR_MailNotExist;
                    reply();
                    return;
                }

                if (MailHelp.IsMailExpired(mailInfo, TimeHelper.ServerNow()))
                {
                    dBMailInfo.MailInfoList.Remove(mailInfo);
                    await DBHelper.SaveComponent(zone, unit.Id, dBMailInfo);
                    response.Error = ErrorCode.ERR_MailNotExist;
                    reply();
                    return;
                }

                if (!MailHelp.HasUnclaimedReward(mailInfo))
                {
                    reply();
                    return;
                }

                MailHelp.GrantMailReward(unit.GetComponent<BagComponentServer>(), mailInfo);
                await DBHelper.SaveComponent(zone, unit.Id, dBMailInfo);
            }

            reply();
            await ETTask.CompletedTask;
        }
    }
}

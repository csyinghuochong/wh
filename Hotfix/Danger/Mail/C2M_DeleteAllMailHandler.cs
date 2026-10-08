using System;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_DeleteAllMailHandler : AMActorLocationRpcHandler<Unit, C2M_DeleteAllMailRequest, M2C_DeleteAllMailResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_DeleteAllMailRequest request, M2C_DeleteAllMailResponse response, Action reply)
        {
            using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.Received, unit.Id))
            {
                int zone = UnitZoneHelper.GetHomeZone(unit);
                DBMailInfo dBMailInfo = await DBHelper.GetComponent<DBMailInfo>(zone, unit.Id);
                if (dBMailInfo != null && request.BelongId > 0 && dBMailInfo.MailInfoList.Count > 0)
                {
                    bool changed = false;
                    for (int i = dBMailInfo.MailInfoList.Count - 1; i >= 0; i--)
                    {
                        if (MailHelp.GetMailTab(dBMailInfo.MailInfoList[i]) != request.BelongId)
                        {
                            continue;
                        }

                        dBMailInfo.MailInfoList.RemoveAt(i);
                        changed = true;
                    }

                    if (changed)
                    {
                        await DBHelper.SaveComponent(zone, unit.Id, dBMailInfo);
                    }
                }
            }

            reply();
            await ETTask.CompletedTask;
        }
    }
}


using System;

namespace ET
{

    [ActorMessageHandler]
    public class C2M_RelinkRecordHandler : AMActorLocationRpcHandler<Unit, C2M_RelinkRecordRequest, M2C_RelinkRecordResponse>
    {

        protected override async ETTask Run(Unit unit, C2M_RelinkRecordRequest request, M2C_RelinkRecordResponse response, Action reply)
        {
            long now = TimeHelper.ServerNow();
            Console.WriteLine($"C2M_RelinkRecordRequest:  {unit.Id}   {TimeInfo.Instance.ToDateTime(now)}");

            RoleInfoComponentServer roleInfoComponentServer = unit.GetComponent<RoleInfoComponentServer>();
            string accont =  roleInfoComponentServer.Account;
            if (!string.IsNullOrEmpty(request.MessageValue))
            {
                request.MessageValue = request.MessageValue.Replace("&", $"   {accont}\n");
            }
            LogHelper.RelinkInfo(request.MessageValue);

            reply();
            await ETTask.CompletedTask;
        }
    }
}

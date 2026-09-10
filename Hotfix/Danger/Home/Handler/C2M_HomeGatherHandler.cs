using System;


namespace ET
{

    /// <summary>
    /// 家园收获
    /// </summary>
    [ActorMessageHandler]
    public class C2M_HomeGatherHandler : AMActorLocationRpcHandler<Unit, C2M_HomeGatherRequest, M2C_HomeGatherResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_HomeGatherRequest request, M2C_HomeGatherResponse response, Action reply)
        {
            

            response.Error = ErrorCode.ERR_ModifyData;
            reply();
            await ETTask.CompletedTask;
         }
    }
}

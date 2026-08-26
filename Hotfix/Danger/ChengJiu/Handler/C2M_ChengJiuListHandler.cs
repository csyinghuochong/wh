using System;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_ChengJiuListHandler : AMActorLocationRpcHandler<Unit, C2M_ChengJiuListRequest, M2C_ChengJiuListResponse>
    {

        protected override async ETTask Run(Unit unit, C2M_ChengJiuListRequest request, M2C_ChengJiuListResponse response, Action reply)
        {
            ChengJiuComponentServer chengJiuComponentServer = unit.GetComponent<ChengJiuComponentServer>();

            response.JingLingList = chengJiuComponentServer.JingLingList;
            response.JingLingId = chengJiuComponentServer.JingLingId;
            response.RandomDrop = chengJiuComponentServer.RandomDrop;
        
            reply();
            await ETTask.CompletedTask;
        }
    }
}

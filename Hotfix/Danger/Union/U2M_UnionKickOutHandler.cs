using System;

namespace ET
{

    [ActorMessageHandler]
    public class U2M_UnionKickOutHandler : AMActorRpcHandler<Unit, U2M_UnionKickOutRequest, M2U_UnionKickOutResponse>
    {
        protected override async ETTask Run(Unit unit, U2M_UnionKickOutRequest request, M2U_UnionKickOutResponse response, Action reply)
        {
            NumericComponent numeric = unit.GetComponent<NumericComponent>();
            numeric.ApplyValue(NumericType.UnionLeader,0);
            numeric.ApplyValue(NumericType.UnionId_0, 0);
            // 等角色公会归属落库，公会服收到成功回包后即可查询新值。
            await unit.GetComponent<DBSaveComponent>().UpdateCacheDBAsync();

            reply();
        }
    }
}

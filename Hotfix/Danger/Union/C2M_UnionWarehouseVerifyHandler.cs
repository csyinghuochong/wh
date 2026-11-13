using System;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_UnionWarehouseVerifyHandler : AMActorLocationRpcHandler<Unit, C2M_UnionWarehouseVerifyRequest, M2C_UnionWarehouseVerifyResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_UnionWarehouseVerifyRequest request, M2C_UnionWarehouseVerifyResponse response, Action reply)
        {
            long unionId = unit.GetUnionId();
            if (unionId == 0)
            {
                response.Error = ErrorCode.ERR_Union_Not_Exist;
                reply();
                return;
            }

            U2M_UnionWarehouseVerifyResponse inner = (U2M_UnionWarehouseVerifyResponse)await ActorMessageSenderComponent.Instance.Call(
                DBHelper.GetUnionServerId(unit), new M2U_UnionWarehouseVerifyRequest
                {
                    UnionId = unionId,
                    UnitId = unit.Id,
                    Password = request.Password,
                });
            response.Error = inner.Error;
            if (inner.Error == ErrorCode.ERR_Success)
            {
                UnionWarehouseAccessComponent access = unit.GetComponent<UnionWarehouseAccessComponent>();
                if (access == null)
                {
                    access = unit.AddComponent<UnionWarehouseAccessComponent>();
                }
                access.UnionId = unionId;
                access.AccessToken = inner.AccessToken;
            }
            reply();
        }
    }
}

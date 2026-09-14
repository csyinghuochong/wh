using System;

namespace ET
{

    [ActorMessageHandler]
    public class C2M_UnionWarehousePasswordHandler : AMActorLocationRpcHandler<Unit, C2M_UnionWarehousePasswordRequest, M2C_UnionWarehousePasswordResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_UnionWarehousePasswordRequest request, M2C_UnionWarehousePasswordResponse response, Action reply)
        {
            long unionId = unit.GetComponent<NumericComponent>().GetAsLong(NumericType.UnionId_0);
            if (unionId == 0)
            {
                response.Error = ErrorCode.ERR_Union_Not_Exist;
                reply();
                return;
            }

            if (string.IsNullOrEmpty(request.NewPassword))
            {
                response.Error = ErrorCode.ERR_Union_WarehousePassword;
                reply();
                return;
            }

            U2M_UnionWarehousePasswordResponse inner = (U2M_UnionWarehousePasswordResponse)await ActorMessageSenderComponent.Instance.Call(
                DBHelper.GetUnionServerId(unit),
                new M2U_UnionWarehousePasswordRequest()
                {
                    UnionId = unionId,
                    UnitId = unit.Id,
                    OldPassword = request.OldPassword ?? string.Empty,
                    NewPassword = request.NewPassword,
                });
            response.Error = inner.Error;
            response.HasWarehousePassword = inner.HasWarehousePassword;
            reply();
            await ETTask.CompletedTask;
        }
    }
}

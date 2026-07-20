using System;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_UnionHandler : AMActorLocationRpcHandler<Unit, C2M_UnionLeaveRequest, M2C_UnionLeaveResponse>
    {

        protected override async ETTask Run(Unit unit, C2M_UnionLeaveRequest request, M2C_UnionLeaveResponse response, Action reply)
        {
            long dbCacheId = DBHelper.GetUnionServerId(unit);
            RoleInfoComponentServer roleInfoComponentServer = unit.GetComponent<RoleInfoComponentServer>();
            NumericComponent numericComponent = unit.GetComponent<NumericComponent>();  
            U2M_UnionLeaveResponse d2GGetUnit = (U2M_UnionLeaveResponse)await ActorMessageSenderComponent.Instance.Call(dbCacheId, new M2U_UnionLeaveRequest()
            {
                UnionId = numericComponent.GetAsLong(NumericType.UnionId_0),
                UserId = roleInfoComponentServer.RoleInfo.UserId,
            });

            if (d2GGetUnit.Error != ErrorCode.ERR_Success)
            {
                response.Error = d2GGetUnit.Error;
                reply();
                return;
            }

            numericComponent.ApplyValue(NumericType.UnionLeader, 0);
            numericComponent.ApplyValue(NumericType.UnionId_0, 0);
            numericComponent.ApplyValue(NumericType.UnionIdLeaveTime, TimeHelper.ServerNow());
            roleInfoComponentServer.UpdateRoleData(UserDataType.UnionName, "");
            roleInfoComponentServer.UpdateRoleDataBroadcast(UserDataType.UnionName, "");
            unit.GetComponent<DBSaveComponent>().UpdateCacheDB();

            unit.UpdateUnionToChat().Coroutine();

            reply();
            await ETTask.CompletedTask;
        }
    }
}

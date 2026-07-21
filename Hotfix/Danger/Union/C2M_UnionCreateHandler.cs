using System;

namespace ET
{

    [ActorMessageHandler]
    public class C2M_UnionCreateHandler : AMActorLocationRpcHandler<Unit, C2M_UnionCreateRequest, M2C_UnionCreateResponse>
    {
        private static int unionCreateNeedLevel;
        private static int unionCreateNeedDiamond;
        private static bool unionCreateCacheInit;

        private static void EnsureUnionCreateCache()
        {
            if (unionCreateCacheInit)
            {
                return;
            }

            unionCreateNeedLevel = int.Parse(LDGlobalValueCategory.Instance.Get(21).Value);
            unionCreateNeedDiamond = int.Parse(LDGlobalValueCategory.Instance.Get(22).Value);
            unionCreateCacheInit = true;
        }

        protected override async ETTask Run(Unit unit, C2M_UnionCreateRequest request, M2C_UnionCreateResponse response, Action reply)
        {
            EnsureUnionCreateCache();
            //判断等级、钻石
            NumericComponent numericComponent = unit.GetComponent<NumericComponent>();
            if (numericComponent.GetAsLong(NumericType.UnionId_0) != 0)
            {
                response.Error = ErrorCode.ERR_Error;
                reply();
                return;
            }
            RoleInfoComponentServer roleInfoComponentServer = unit.GetComponent<RoleInfoComponentServer>();
            RoleInfo roleInfo = roleInfoComponentServer.RoleInfo;
            if (roleInfo.Lv < unionCreateNeedLevel || roleInfo.Diamond < unionCreateNeedDiamond)
            {
                response.Error = ErrorCode.ERR_Error;
                reply();
                return;
            }

            long dbCacheId = DBHelper.GetUnionServerId(unit);
            U2M_UnionCreateResponse d2GGetUnit = (U2M_UnionCreateResponse)await ActorMessageSenderComponent.Instance.Call(dbCacheId, new M2U_UnionCreateRequest() 
            {
                UnionName =request.UnionName,
                UnionPurpose = request.UnionPurpose,
                UserID = roleInfo.UserId
            });

            if (d2GGetUnit.Error == ErrorCode.ERR_Success)
            {
                numericComponent.ApplyValue( NumericType.UnionLeader, 1, true);
                numericComponent.ApplyValue( NumericType.UnionId_0, d2GGetUnit.UnionId, true);
                roleInfoComponentServer.UpdateRoleData(UserDataType.UnionName, request.UnionName);
                roleInfoComponentServer.UpdateRoleDataBroadcast(UserDataType.UnionName, request.UnionName);
                unit.GetComponent<TaskComponentServer>().TriggerTaskEvent(TastConditionType.JoinUnion_9, 0, 1);
               
                unit.UpdateUnionToChat().Coroutine();
            }
            response.Error = d2GGetUnit.Error;
            reply();
            await ETTask.CompletedTask;
        }

    }
}

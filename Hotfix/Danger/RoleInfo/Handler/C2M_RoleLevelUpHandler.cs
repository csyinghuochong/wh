using System;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_RoleLevelUpHandler : AMActorLocationRpcHandler<Unit, C2M_RoleLevelUpRequest, M2C_RoleLevelUpResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_RoleLevelUpRequest request, M2C_RoleLevelUpResponse response, Action reply)
        {
            RoleInfoComponentServer roleInfoComponent = unit.GetComponent<RoleInfoComponentServer>();
            RoleInfo roleInfo = roleInfoComponent.RoleInfo;
            if (!RoleLevelHelper.CanLevelUp(roleInfo.Lv, roleInfo.Exp))
            {
                response.Error = ErrorCode.ERR_ModifyData;
                reply();
                return;
            }

            int needExp = RoleLevelHelper.GetLevelUpNeedExp(roleInfo.Lv);
            roleInfo.Exp -= needExp;
            roleInfoComponent.UpdateRoleData(UserDataType.Level, "1");

            M2C_RoleDataUpdate expUpdate = new M2C_RoleDataUpdate();
            expUpdate.UpdateType = (int)UserDataType.Exp;
            expUpdate.UpdateTypeValue = "0";
            expUpdate.UpdateValueLong = roleInfo.Exp;
            MessageHelper.SendToClient(unit, expUpdate);

            reply();
            await ETTask.CompletedTask;
        }
    }
}

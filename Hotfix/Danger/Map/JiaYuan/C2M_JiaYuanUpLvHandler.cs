using System;

namespace ET
{

    [ActorMessageHandler]
    public class C2M_JiaYuanUpLvHandler : AMActorLocationRpcHandler<Unit, C2M_JiaYuanUpLvRequest, M2C_JiaYuanUpLvResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_JiaYuanUpLvRequest request, M2C_JiaYuanUpLvResponse response, Action reply)
        {
            RoleInfoComponent roleInfoComponent = unit.GetComponent<RoleInfoComponent>();
            int lvid = roleInfoComponent.RoleInfo.JiaYuanLv;
            LDHome ldHome = LDHomeCategory.Instance.Get(lvid);
            if ( !LDHomeCategory.Instance.Contain(lvid) )
            {
                reply();
                return;
            }
            /*if (roleInfoComponent.RoleInfo.Level < ldHome.NeedRoseLv)
            {
                response.Error = ErrorCode.ERR_LevelIsNot;
                reply();
                return;
            }*/
            if (roleInfoComponent.RoleInfo.JiaYuanExp < ldHome.Exp)
            {
                response.Error = ErrorCode.ERR_ExpNoEnough;
                reply();
                return;
            }

            roleInfoComponent.UpdateRoleData(UserDataType.JiaYuanExp, (ldHome.Exp * -1).ToString());
            roleInfoComponent.UpdateRoleData(UserDataType.JiaYuanLv, "1");

            reply();
            await ETTask.CompletedTask;
        }
    }
}

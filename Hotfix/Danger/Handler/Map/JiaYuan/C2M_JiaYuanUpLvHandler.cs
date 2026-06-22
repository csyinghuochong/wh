using System;

namespace ET
{

    [ActorMessageHandler]
    public class C2M_JiaYuanUpLvHandler : AMActorLocationRpcHandler<Unit, C2M_JiaYuanUpLvRequest, M2C_JiaYuanUpLvResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_JiaYuanUpLvRequest request, M2C_JiaYuanUpLvResponse response, Action reply)
        {
            UserInfoComponent userInfoComponent = unit.GetComponent<RoleInfoComponent>();
            int lvid = userInfoComponent.UserInfo.JiaYuanLv;
            LDHome ldHome = LDHomeCategory.Instance.Get(lvid);
            if ( !LDHomeCategory.Instance.Contain(lvid) )
            {
                reply();
                return;
            }
            /*if (userInfoComponent.UserInfo.Lv < ldHome.NeedRoseLv)
            {
                response.Error = ErrorCode.ERR_LevelIsNot;
                reply();
                return;
            }*/
            if (userInfoComponent.UserInfo.JiaYuanExp < ldHome.Exp)
            {
                response.Error = ErrorCode.ERR_ExpNoEnough;
                reply();
                return;
            }

            userInfoComponent.UpdateRoleData(UserDataType.JiaYuanExp, (ldHome.Exp * -1).ToString());
            userInfoComponent.UpdateRoleData(UserDataType.JiaYuanLv, "1");

            reply();
            await ETTask.CompletedTask;
        }
    }
}

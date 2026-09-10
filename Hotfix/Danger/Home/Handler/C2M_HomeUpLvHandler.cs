using System;

namespace ET
{

    [ActorMessageHandler]
    public class C2M_HomeUpLvHandler : AMActorLocationRpcHandler<Unit, C2M_HomeUpLvRequest, M2C_HomeUpLvResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_HomeUpLvRequest request, M2C_HomeUpLvResponse response, Action reply)
        {
            HomeComponentServer homeComponentServer = unit.GetComponent<HomeComponentServer>();
            int lvid = homeComponentServer.HomeLv;
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
            if (homeComponentServer.HomeExp < ldHome.Exp)
            {
                response.Error = ErrorCode.ERR_ExpNoEnough;
                reply();
                return;
            }

            homeComponentServer.AddHomeExp(ldHome.Exp * -1);
            homeComponentServer.AddHomeLv(1);

            reply();
            await ETTask.CompletedTask;
        }
    }
}

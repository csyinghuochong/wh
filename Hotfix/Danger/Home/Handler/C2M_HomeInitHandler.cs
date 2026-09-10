using System;
using System.Collections.Generic;
using System.Linq;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_HomeInitHandler : AMActorLocationRpcHandler<Unit, C2M_HomeInitRequest, M2C_HomeInitResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_HomeInitRequest request, M2C_HomeInitResponse response, Action reply)
        {
            int masterHomeZone = UnitZoneHelper.GetHomeZone(request.MasterId);
            HomeComponentServer homeComponentServer = await DBHelper.GetComponent<HomeComponentServer>(masterHomeZone, request.MasterId);
            RoleInfoComponentServer roleInfoComponentServer = await DBHelper.GetComponent<RoleInfoComponentServer>(masterHomeZone, request.MasterId);
            if (homeComponentServer == null || roleInfoComponentServer == null)
            {
                response.Error = ErrorCode.ERR_Error;
                reply();
                return;
            }
            if (unit.Id != request.MasterId)
            {
                string playerName = unit.GetComponent<RoleInfoComponentServer>().RoleInfo.Name;
            }
            else
            {
               
            }

            homeComponentServer.EnsureHomeData();
            response.PlanOpenList = homeComponentServer.InitOpenList();
          
            response.HomePastureList = homeComponentServer.HomePastureList_7;
            response.HomePlantList = homeComponentServer.HomePlantList_7;
            response.HomeLv = homeComponentServer.HomeLv;
            response.HomeFund = homeComponentServer.HomeFund;
            response.HomeExp = homeComponentServer.HomeExp;
            response.MasterName = roleInfoComponentServer.RoleInfo.Name;
            reply();
        }
    }
}

using System;
namespace ET
{
    [ActorMessageHandler]
    public class M2M_HomeOperateHandler : AMActorLocationRpcHandler<Unit, M2M_HomeOperateRequest, M2M_HomeOperateResponse>
    {
        protected override async ETTask Run(Unit unit, M2M_HomeOperateRequest request, M2M_HomeOperateResponse response, Action reply)
        {
            HomeComponentServer homeComponentServer = unit.GetComponent<HomeComponentServer>();
            HomeOperate operateType = request.OperateType;
            switch (operateType.OperateType)
            {
                case HomeOperateType.Visit:
                   
                    break;
                case HomeOperateType.GatherPlant:
                    HomePlant homePlan = homeComponentServer.GetHomePlant(operateType.UnitId);
                    if (homePlan == null)
                    {
                        reply();
                        return;
                    }
                    homePlan.StealNumber += 1;
                    homePlan.GatherNumber += 1;
                    homePlan.GatherLastTime = TimeHelper.ServerNow();
                    break;
                case HomeOperateType.GatherPasture:
                    HomePastures homePasture = homeComponentServer.GetHomePastures(operateType.UnitId);
                    if (homePasture == null)
                    {
                        reply();
                        return;
                    }
                    homePasture.StealNumber += 1;
                    homePasture.GatherNumber += 1;
                    homePasture.GatherLastTime = TimeHelper.ServerNow();
                    break;
                case HomeOperateType.Pick:
                    unit.GetComponent<HomeComponentServer>().OnRemoveUnit(operateType.UnitId);
                    break;
            }

            await DBHelper.SaveComponentCache(UnitZoneHelper.GetHomeZone(unit), unit.Id, homeComponentServer);
            reply();
        }
    }
}

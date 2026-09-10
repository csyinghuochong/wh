using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_HomeVisitListHandler : AMActorLocationRpcHandler<Unit, C2M_HomeVisitListRequest, M2C_HomeVisitListResponse>
    {

        private async ETTask<HomeVisit> GetHomeVisit(int zone, long id)
        {
            List<HomeComponentServer> resultHomes = await Game.Scene.GetComponent<DBComponent>().Query<HomeComponentServer>(zone, _account => _account.Id == id);
            if (resultHomes == null || resultHomes.Count == 0)
            {
                return null;
            }
            
            List<RoleInfoComponentServer> resultUser = await Game.Scene.GetComponent<DBComponent>().Query<RoleInfoComponentServer>(zone, _account => _account.Id == id);
            if (resultUser[0].RoleInfo.Lv < 10)
            {
                return null;
            }
            HomeVisit homeVisit = new HomeVisit() ;
            homeVisit.Occ = resultUser[0].RoleInfo.Occ;
            homeVisit.OccTwo = resultUser[0].RoleInfo.OccTwo;
            homeVisit.PlayerName = resultUser[0].RoleInfo.Name;
            homeVisit.UnitId = resultHomes[0].Id;
            homeVisit.Rubbish = resultHomes[0].GetRubbishNumber();
            homeVisit.Gather = resultHomes[0].GetCanGatherNumber();
            return homeVisit;
        }

        protected override async ETTask Run(Unit unit, C2M_HomeVisitListRequest request, M2C_HomeVisitListResponse response, Action reply)
        {
         
            using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.Home, unit.Id))
            {
            }
            reply();
            await ETTask.CompletedTask;
        }
    }
}

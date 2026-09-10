using System;
using System.Collections.Generic;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_HomePlanOpenHandler : AMActorLocationRpcHandler<Unit, C2M_HomePlanOpenRequest, M2C_HomePlanOpenResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_HomePlanOpenRequest request, M2C_HomePlanOpenResponse response, Action reply)
        {
            HomeComponentServer homeComponentServer = unit.GetComponent<HomeComponentServer>();
            List<int> PlanOpenList_2 = homeComponentServer.PlanOpenList_7;
            if (PlanOpenList_2.Contains(request.CellIndex))
            {
                response.PlanOpenList = PlanOpenList_2; 
                reply();
                return;
            }
            LDHome ldHome = LDHomeCategory.Instance.Get(homeComponentServer.HomeLv);
            

            int costNumber = 0;
            BagComponentServer bagComponentServer = unit.GetComponent<BagComponentServer>();
            if (!bagComponentServer.CheckNeedItem($"13;{costNumber}"))
            {
                response.PlanOpenList = PlanOpenList_2;
                response.Error = ErrorCode.ERR_ItemNotEnoughError;
                reply();
                return;
            }

            PlanOpenList_2.Add(request.CellIndex);
            response.PlanOpenList = PlanOpenList_2;
            bagComponentServer.OnCostItemData($"13;{costNumber}", ItemLocType.ItemLocBag, ItemGetWay.HomeCost);
            DBHelper.SaveComponentCache(UnitZoneHelper.GetHomeZone(unit), unit.Id, homeComponentServer).Coroutine();
            reply();
            await ETTask.CompletedTask;
        }
    }
}

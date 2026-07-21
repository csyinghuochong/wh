using System;
using System.Collections.Generic;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_JiaYuanPlanOpenHandler : AMActorLocationRpcHandler<Unit, C2M_JiaYuanPlanOpenRequest, M2C_JiaYuanPlanOpenResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_JiaYuanPlanOpenRequest request, M2C_JiaYuanPlanOpenResponse response, Action reply)
        {
            JiaYuanComponentServer jiaYuanComponentServer = unit.GetComponent<JiaYuanComponentServer>();
            List<int> PlanOpenList_2 = jiaYuanComponentServer.PlanOpenList_7;
            if (PlanOpenList_2.Contains(request.CellIndex))
            {
                response.PlanOpenList = PlanOpenList_2; 
                reply();
                return;
            }
            RoleInfoComponentServer roleInfoComponentServer = unit.GetComponent<RoleInfoComponentServer>();
            LDHome ldHome = LDHomeCategory.Instance.Get(roleInfoComponentServer.RoleInfo.JiaYuanLv);
            /*if (jiaYuanComponent.GetOpenPlanNumber() >= ldHome.FarmNumMax)
            {
                response.Error = ErrorCode.ERR_JiaYuanLevel;
                reply();
                return;
            }*/

            int costNumber = CommonConfig.JiaYuanFarmOpen[request.CellIndex];
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
            bagComponentServer.OnCostItemData($"13;{costNumber}", ItemLocType.ItemLocBag, ItemGetWay.JiaYuanCost);
            DBHelper.SaveComponentCache(UnitZoneHelper.GetHomeZone(unit), unit.Id, jiaYuanComponentServer).Coroutine();
            reply();
            await ETTask.CompletedTask;
        }
    }
}

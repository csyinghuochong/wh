using System;


namespace ET
{
    [ActorMessageHandler]
    public class C2M_JiaYuanUprootHandler : AMActorLocationRpcHandler<Unit, C2M_JiaYuanUprootRequest, M2C_JiaYuanUprootResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_JiaYuanUprootRequest request, M2C_JiaYuanUprootResponse response, Action reply)
        {            response.Error = ErrorCode.ERR_ModifyData;
            reply();
            await ETTask.CompletedTask;
#if false // TODO: migrate to LD config

            Unit unitPlan = unit.GetParent<UnitComponent>().Get(request.UnitId);
            if (unitPlan == null)
            {
                reply();
                return;
            }
           
            switch (request.OperateType)
            {
                case 1:
                    unit.GetComponent<JiaYuanComponentServer>().UprootPlant(request.CellIndex);
                    break;
                case 2:
                    JiaYuanPastureConfig jiaYuanPastureConfig = JiaYuanPastureConfigCategory.Instance.Get(unitPlan.ConfigId);
                    unit.GetComponent<BagComponentServer>().OnAddItemData($"13;{jiaYuanPastureConfig.SellGold}", $"{ItemGetWay.JiaYuanGather}_{TimeHelper.ServerFrameTime()}");
                    unit.GetComponent<JiaYuanComponentServer>().UprootPasture(request.UnitId);

                    break;
            }

            unit.GetParent<UnitComponent>().Remove(request.UnitId);
            response.JiaYuanPastureList = unit.GetComponent<JiaYuanComponentServer>().JiaYuanPastureList_7;
            DBHelper.SaveComponentCache(UnitZoneHelper.GetHomeZone(unit), unit.Id, unit.GetComponent<JiaYuanComponentServer>()).Coroutine();
            reply();
            await ETTask.CompletedTask;
        #endif
}
    }
}

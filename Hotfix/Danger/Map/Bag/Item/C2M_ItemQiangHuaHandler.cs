using System;

namespace ET
{

    [ActorMessageHandler]
    public class C2M_ItemQiangHuaHandler : AMActorLocationRpcHandler<Unit, C2M_ItemQiangHuaRequest, M2C_ItemQiangHuaResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_ItemQiangHuaRequest request, M2C_ItemQiangHuaResponse response, Action reply)
        {
            int maxLevel = QiangHuaHelper.GetQiangHuaMaxLevel(request.WeiZhi);
            BagComponentServer bagComponentServer = unit.GetComponent<BagComponentServer>();
          
            unit.GetComponent<TaskComponent>().TriggerTaskEvent(TastConditionType.QiangHuaLevel_17, 0, response.QiangHuaLevel);
            Function_Fight.UnitUpdateProperty_Base(unit, true, true);
            reply();
            await ETTask.CompletedTask;
        }
    }
}

using System;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_HomePlantHandler : AMActorLocationRpcHandler<Unit, C2M_HomePlantRequest, M2C_HomePlantResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_HomePlantRequest request, M2C_HomePlantResponse response, Action reply)
        {
            HomeComponentServer homeComponentServer = unit.GetComponent<HomeComponentServer>();
            if (homeComponentServer.GetCellPlant(request.CellIndex)!=null)
            {
                response.Error = ErrorCode.ERR_AlreadyPlant;
                reply();
                return;
            }
            BagComponentServer bagComponentServer = unit.GetComponent<BagComponentServer>();
            if (bagComponentServer.GetItemNumber(ItemBigType.Type_Item,request.ItemId) < 1)
            {
                response.Error = ErrorCode.ERR_ItemNotEnoughError;
                reply();
                return;
            }

            LDItem ldItem = LDItemCategory.Instance.Get(request.ItemId);
            HomePlant homePlant = new HomePlant()
            {
                CellIndex = request.CellIndex,
                ItemId = 0,
                StartTime = TimeHelper.ServerNow(),
                UnitId = IdGenerater.Instance.GenerateId(),
            };

            homeComponentServer.HomePlantList_7.Add(homePlant);
            Unit plan = UnitFactory.CreatePlan( unit.DomainScene(), homePlant, unit.Id);
            homePlant.UnitId = plan.Id;
            DBHelper.SaveComponentCache(UnitZoneHelper.GetHomeZone(unit), unit.Id, homeComponentServer).Coroutine();
            reply();
            await ETTask.CompletedTask;
        }
    }
}

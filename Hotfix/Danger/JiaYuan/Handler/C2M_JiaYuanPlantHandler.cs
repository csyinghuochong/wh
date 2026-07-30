using System;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_JiaYuanPlantHandler : AMActorLocationRpcHandler<Unit, C2M_JiaYuanPlantRequest, M2C_JiaYuanPlantResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_JiaYuanPlantRequest request, M2C_JiaYuanPlantResponse response, Action reply)
        {
            JiaYuanComponentServer jianYuanComponentServer = unit.GetComponent<JiaYuanComponentServer>();
            if (jianYuanComponentServer.GetCellPlant(request.CellIndex)!=null)
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

            bagComponentServer.OnCostItemData($"{request.ItemId};1", ItemLocType.ItemLocBag, ItemGetWay.JiaYuanCost);
            LDItem ldItem = LDItemCategory.Instance.Get(request.ItemId);
            JiaYuanPlant jiaYuanPlant = new JiaYuanPlant()
            {
                CellIndex = request.CellIndex,
                ItemId = 0,
                StartTime = TimeHelper.ServerNow(),
                UnitId = IdGenerater.Instance.GenerateId(),
            };

            jianYuanComponentServer.JianYuanPlantList_7.Add(jiaYuanPlant);
            Unit plan = UnitFactory.CreatePlan( unit.DomainScene(), jiaYuanPlant, unit.Id);
            jiaYuanPlant.UnitId = plan.Id;
            DBHelper.SaveComponentCache(UnitZoneHelper.GetHomeZone(unit), unit.Id, jianYuanComponentServer).Coroutine();
            reply();
            await ETTask.CompletedTask;
        }
    }
}

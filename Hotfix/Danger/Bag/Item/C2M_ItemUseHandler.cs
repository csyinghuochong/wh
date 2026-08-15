using System;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_ItemUseHandler : AMActorLocationRpcHandler<Unit, C2M_ItemUseRequest, M2C_ItemUseResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_ItemUseRequest request, M2C_ItemUseResponse response, Action reply)
        {
            try
            {
                M2C_RoleBagUpdate bagUpdate = new M2C_RoleBagUpdate();
                BagInfo useBagInfo = unit.GetComponent<BagComponentServer>().GetItemByLoc(ItemLocType.ItemLocBag, request.OperateBagID);
                response.Error = ItemUseHelper.UseItem(unit, 0, useBagInfo, bagUpdate, out string usePar);
                response.OperatePar = usePar;
                if (bagUpdate.BagInfoDelete.Count > 0 || bagUpdate.BagInfoUpdate.Count > 0)
                {
                    MessageHelper.SendToClient(unit, bagUpdate);
                    DBHelper.SaveComponentCache(UnitZoneHelper.GetHomeZone(unit), unit.Id, unit.GetComponent<BagComponentServer>()).Coroutine();
                }
                reply();
                await ETTask.CompletedTask;
            }
            catch (Exception ex)
            {
                Log.Debug(ex.ToString());
            }
        }
    }
}

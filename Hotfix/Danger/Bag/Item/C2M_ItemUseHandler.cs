using System;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_ItemUseHandler : AMActorLocationRpcHandler<Unit, C2M_ItemUseRequest, M2C_ItemUseResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_ItemUseRequest request, M2C_ItemUseResponse response, Action reply)
        {
            if (unit.Type != UnitType.Player || request.UseNum <= 0 || !Enum.IsDefined(typeof(ItemLocType), request.Loc)
                    || request.Loc == (int)ItemLocType.ItemLocMax)
            {
                response.Error = ErrorCode.ERR_Parameter;
                reply();
                return;
            }
            M2C_RoleBagUpdate bagUpdate = new M2C_RoleBagUpdate();
            ItemLocType locType = (ItemLocType)request.Loc;
            BagInfo useBagInfo = unit.GetComponent<BagComponentServer>().GetItemByLoc(locType, request.OperateBagID);
            if (useBagInfo == null)
            {
                response.Error = ErrorCode.ERR_ItemNotExist;
                reply();
                return;
            }
            if (useBagInfo.ItemType != ItemBigType.Type_Item)
            {
                response.Error = ErrorCode.ERR_Parameter;
                reply();
                return;
            }
            response.Error = ItemUseHelper.UseItem(unit, 0, useBagInfo, bagUpdate, request.UseNum, locType, request.OperatePar);
            if (bagUpdate.BagInfoDelete.Count > 0 || bagUpdate.BagInfoUpdate.Count > 0)
            {
                MessageHelper.SendToClient(unit, bagUpdate);
                DBHelper.SaveComponentCache(UnitZoneHelper.GetHomeZone(unit), unit.Id, unit.GetComponent<BagComponentServer>()).Coroutine();
            }
            reply();
            await ETTask.CompletedTask;
        }
    }

    [ActorMessageHandler]
    public class C2M_ItemTradeConfirmHandler : AMActorLocationRpcHandler<Unit, C2M_ItemTradeConfirmRequest, M2C_ItemTradeConfirmResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_ItemTradeConfirmRequest request, M2C_ItemTradeConfirmResponse response, Action reply)
        {
            BagComponentServer bag = unit.GetComponent<BagComponentServer>();
            if (unit.Type != UnitType.Player || request.BagInfoID <= 0 || !Enum.IsDefined(typeof(ItemLocType), request.Loc)
                    || request.Loc == (int)ItemLocType.ItemLocMax)
            {
                response.Error = ErrorCode.ERR_Parameter;
                reply();
                return;
            }
            BagInfo item = bag.GetItemByLoc((ItemLocType)request.Loc, request.BagInfoID);
            if (item == null)
            {
                response.Error = ErrorCode.ERR_ItemNotExist;
                reply();
                return;
            }
            if (item.GetTradeStatus() != ItemTradeStatus.DungeonTradable)
            {
                response.Error = ErrorCode.ERR_ItemBing;
                reply();
                return;
            }
            item.SetTradeStatus(ItemTradeStatus.NonTradable);
            M2C_RoleBagUpdate update = new M2C_RoleBagUpdate();
            update.BagInfoUpdate.Add(item);
            MessageHelper.SendToClient(unit, update);
            DBHelper.SaveComponentCache(UnitZoneHelper.GetHomeZone(unit), unit.Id, bag).Coroutine();
            reply();
            await ETTask.CompletedTask;
        }
    }
}

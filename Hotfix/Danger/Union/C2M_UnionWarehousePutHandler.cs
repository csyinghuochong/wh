using System;

namespace ET
{

    [ActorMessageHandler]
    public class C2M_UnionWarehousePutHandler : AMActorLocationRpcHandler<Unit, C2M_UnionWarehousePutRequest, M2C_UnionWarehousePutResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_UnionWarehousePutRequest request, M2C_UnionWarehousePutResponse response, Action reply)
        {
            long unionId = unit.GetComponent<NumericComponent>().GetAsLong(NumericType.UnionId_0);
            if (unionId == 0)
            {
                response.Error = ErrorCode.ERR_Union_Not_Exist;
                reply();
                return;
            }

            BagComponentServer bag = unit.GetComponent<BagComponentServer>();
            RoleInfoComponentServer roleInfo = unit.GetComponent<RoleInfoComponentServer>();
            M2U_UnionWarehousePutRequest innerRequest = new M2U_UnionWarehousePutRequest()
            {
                UnionId = unionId,
                UnitId = unit.Id,
            };

            if (request.BagInfoID > 0)
            {
                BagInfo src = bag.GetItemByUId(request.BagInfoID);
                if (src == null)
                {
                    response.Error = ErrorCode.ERR_ItemNotEnoughError;
                    reply();
                    return;
                }

                int num = request.ItemNum > 0 ? request.ItemNum : src.ItemNum;
                if (num <= 0 || num > src.ItemNum)
                {
                    response.Error = ErrorCode.ERR_ItemNotEnoughError;
                    reply();
                    return;
                }

                innerRequest.BagInfo = UnionWarehouseHelper.CloneBagInfo(src, num);
            }
            else
            {
                if (!UnionWarehouseHelper.IsUnbindGold(request.ItemID) || request.ItemNum <= 0)
                {
                    response.Error = ErrorCode.ERR_Union_WarehouseGoldOnly;
                    reply();
                    return;
                }

                long have = bag.GetItemNumber(ItemBigType.Type_Item, UserDataType.Gold);
                if (have < request.ItemNum)
                {
                    response.Error = ErrorCode.ERR_GoldNotEnoughError;
                    reply();
                    return;
                }

                innerRequest.ItemID = UserDataType.Gold;
                innerRequest.ItemNum = request.ItemNum;
            }

            U2M_UnionWarehousePutResponse inner = (U2M_UnionWarehousePutResponse)await ActorMessageSenderComponent.Instance.Call(
                DBHelper.GetUnionServerId(unit), innerRequest);
            if (inner.Error != ErrorCode.ERR_Success)
            {
                response.Error = inner.Error;
                reply();
                return;
            }

            if (request.BagInfoID > 0)
            {
                BagInfo src = bag.GetItemByUId(request.BagInfoID);
                int num = innerRequest.BagInfo != null ? innerRequest.BagInfo.ItemNum : request.ItemNum;
                bag.OnCostItemData(request.BagInfoID, num, src != null ? (ItemLocType)src.Loc : ItemLocType.ItemLocBag);
            }
            else
            {
                roleInfo.UpdateRoleData(UserDataType.Gold, (-request.ItemNum).ToString(), true, ItemGetWay.UnionWarehouse);
            }

            response.WarehouseList = inner.WarehouseList;
            response.WarehouseGold = inner.WarehouseGold;
            reply();
            await ETTask.CompletedTask;
        }
    }
}

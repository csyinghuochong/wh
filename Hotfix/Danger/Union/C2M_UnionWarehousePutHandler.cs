using System;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_UnionWarehousePutHandler : AMActorLocationRpcHandler<Unit, C2M_UnionWarehousePutRequest, M2C_UnionWarehousePutResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_UnionWarehousePutRequest request, M2C_UnionWarehousePutResponse response, Action reply)
        {
            long unionId = unit.GetUnionId();
            if (unionId == 0)
            {
                response.Error = ErrorCode.ERR_Union_Not_Exist;
                reply();
                return;
            }

            BagComponentServer bag = unit.GetComponent<BagComponentServer>();
            RoleInfoComponentServer role = unit.GetComponent<RoleInfoComponentServer>();
            M2U_UnionWarehousePutRequest innerRequest = new M2U_UnionWarehousePutRequest
            {
                UnionId = unionId,
                UnitId = unit.Id,
            };
            if (request.BagInfoID > 0)
            {
                BagInfo source = bag.GetItemByUId(request.BagInfoID);
                if (source == null || request.ItemNum < 0)
                {
                    response.Error = ErrorCode.ERR_ItemNotEnoughError;
                    reply();
                    return;
                }

                int count = request.ItemNum > 0 ? request.ItemNum : source.ItemNum;
                if (count <= 0 || count > source.ItemNum)
                {
                    response.Error = ErrorCode.ERR_ItemNotEnoughError;
                    reply();
                    return;
                }

                innerRequest.BagInfo = UnionWarehouseHelper.CloneBagInfo(source, count);
                if (!bag.OnCostItemData(source.BagInfoID, count, (ItemLocType)source.Loc))
                {
                    response.Error = ErrorCode.ERR_ItemNotEnoughError;
                    reply();
                    return;
                }
                innerRequest.BagInfo.BagInfoID = IdGenerater.Instance.GenerateId();
            }
            else
            {
                if (request.BagInfoID != 0 || !UnionWarehouseHelper.IsUnbindGold(request.ItemID) || request.ItemNum <= 0)
                {
                    response.Error = ErrorCode.ERR_Union_WarehouseGoldOnly;
                    reply();
                    return;
                }

                if (bag.GetItemNumber(ItemBigType.Type_Item, UserDataType.Gold) < request.ItemNum)
                {
                    response.Error = ErrorCode.ERR_GoldNotEnoughError;
                    reply();
                    return;
                }

                innerRequest.ItemID = UserDataType.Gold;
                innerRequest.ItemNum = request.ItemNum;
                role.UpdateRoleData(UserDataType.Gold, (-request.ItemNum).ToString(), true, ItemGetWay.UnionWarehouse);
            }

            U2M_UnionWarehousePutResponse inner = (U2M_UnionWarehousePutResponse)await ActorMessageSenderComponent.Instance.Call(
                DBHelper.GetUnionServerId(unit), innerRequest);
            if (inner.Error != ErrorCode.ERR_Success)
            {
                // 仅在公会服明确拒绝时退还，超时结果不确定时不能盲目退款。
                response.Error = inner.Error;
                if (innerRequest.BagInfo != null)
                {
                    int deliveryError = await UnionWarehouseServerHelper.DeliverItem(unit, innerRequest.BagInfo);
                    if (deliveryError != ErrorCode.ERR_Success)
                    {
                        response.Error = deliveryError;
                    }
                }
                else
                {
                    role.UpdateRoleData(UserDataType.Gold, request.ItemNum.ToString(), true, ItemGetWay.UnionWarehouse);
                }
                reply();
                return;
            }

            response.WarehouseList = inner.WarehouseList;
            response.WarehouseGold = inner.WarehouseGold;
            reply();
        }
    }
}

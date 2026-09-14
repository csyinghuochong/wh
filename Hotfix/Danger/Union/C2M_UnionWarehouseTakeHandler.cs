using System;

namespace ET
{

    [ActorMessageHandler]
    public class C2M_UnionWarehouseTakeHandler : AMActorLocationRpcHandler<Unit, C2M_UnionWarehouseTakeRequest, M2C_UnionWarehouseTakeResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_UnionWarehouseTakeRequest request, M2C_UnionWarehouseTakeResponse response, Action reply)
        {
            long unionId = unit.GetComponent<NumericComponent>().GetAsLong(NumericType.UnionId_0);
            if (unionId == 0)
            {
                response.Error = ErrorCode.ERR_Union_Not_Exist;
                reply();
                return;
            }

            if (request.BagInfoID <= 0)
            {
                if (!UnionWarehouseHelper.IsUnbindGold(request.ItemID) || request.ItemNum <= 0)
                {
                    response.Error = ErrorCode.ERR_Union_WarehouseGoldOnly;
                    reply();
                    return;
                }

                if (string.IsNullOrEmpty(request.Password))
                {
                    response.Error = ErrorCode.ERR_Union_WarehouseNeedPassword;
                    reply();
                    return;
                }
            }

            BagComponentServer bag = unit.GetComponent<BagComponentServer>();
            if (request.BagInfoID > 0 && bag.GetBagLeftCell() < 1)
            {
                response.Error = ErrorCode.ERR_BagIsFull;
                reply();
                return;
            }

            U2M_UnionWarehouseTakeResponse inner = (U2M_UnionWarehouseTakeResponse)await ActorMessageSenderComponent.Instance.Call(
                DBHelper.GetUnionServerId(unit),
                new M2U_UnionWarehouseTakeRequest()
                {
                    UnionId = unionId,
                    UnitId = unit.Id,
                    BagInfoID = request.BagInfoID,
                    ItemID = request.ItemID,
                    ItemNum = request.ItemNum,
                    Password = request.Password ?? string.Empty,
                });
            if (inner.Error != ErrorCode.ERR_Success)
            {
                response.Error = inner.Error;
                reply();
                return;
            }
            if (request.BagInfoID > 0)
            {
                if (inner.BagInfo != null)
                {
                    bag.OnAddItemData(inner.BagInfo, $"{ItemGetWay.UnionWarehouse}_0");
                }
            }
            else
            {
                unit.GetComponent<RoleInfoComponentServer>().UpdateRoleData(
                    UserDataType.Gold, request.ItemNum.ToString(), true, ItemGetWay.UnionWarehouse);
            }

            response.WarehouseList = inner.WarehouseList;
            response.WarehouseGold = inner.WarehouseGold;
            reply();
            await ETTask.CompletedTask;
        }
    }
}

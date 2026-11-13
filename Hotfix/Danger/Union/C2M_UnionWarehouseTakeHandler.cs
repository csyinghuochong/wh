using System;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_UnionWarehouseTakeHandler : AMActorLocationRpcHandler<Unit, C2M_UnionWarehouseTakeRequest, M2C_UnionWarehouseTakeResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_UnionWarehouseTakeRequest request, M2C_UnionWarehouseTakeResponse response, Action reply)
        {
            long unionId = unit.GetUnionId();
            if (unionId == 0)
            {
                response.Error = ErrorCode.ERR_Union_Not_Exist;
                reply();
                return;
            }

            RoleInfoComponentServer role = unit.GetComponent<RoleInfoComponentServer>();
            M2U_UnionWarehouseTakeRequest innerRequest = new M2U_UnionWarehouseTakeRequest
            {
                UnionId = unionId,
                UnitId = unit.Id,
                BagInfoID = request.BagInfoID,
                ItemID = request.ItemID,
                ItemNum = request.ItemNum,
            };
            UnionWarehouseAccessComponent access = unit.GetComponent<UnionWarehouseAccessComponent>();
            if (access != null && access.UnionId == unionId)
            {
                innerRequest.AccessToken = access.AccessToken;
            }
            if (request.BagInfoID > 0)
            {
                if (request.ItemNum < 0)
                {
                    response.Error = ErrorCode.ERR_ModifyData;
                    reply();
                    return;
                }

                innerRequest.Preview = true;
                U2M_UnionWarehouseTakeResponse preview = (U2M_UnionWarehouseTakeResponse)await ActorMessageSenderComponent.Instance.Call(
                    DBHelper.GetUnionServerId(unit), innerRequest);
                if (preview.Error != ErrorCode.ERR_Success)
                {
                    response.Error = preview.Error;
                    reply();
                    return;
                }

                if (!UnionWarehouseServerHelper.CanReceive(unit.GetComponent<BagComponentServer>(), preview.BagInfo))
                {
                    response.Error = ErrorCode.ERR_BagIsFull;
                    reply();
                    return;
                }
                innerRequest.ItemNum = preview.BagInfo.ItemNum;
                innerRequest.Preview = false;
            }
            else if (request.BagInfoID != 0 || !UnionWarehouseHelper.IsUnbindGold(request.ItemID) || request.ItemNum <= 0
                || request.ItemNum > long.MaxValue - unit.GetComponent<BagComponentServer>().GetItemNumber(ItemBigType.Type_Item, UserDataType.Gold))
            {
                response.Error = ErrorCode.ERR_Union_WarehouseGoldOnly;
                reply();
                return;
            }

            U2M_UnionWarehouseTakeResponse inner = (U2M_UnionWarehouseTakeResponse)await ActorMessageSenderComponent.Instance.Call(
                DBHelper.GetUnionServerId(unit), innerRequest);
            if (inner.Error != ErrorCode.ERR_Success)
            {
                response.Error = inner.Error;
                reply();
                return;
            }

            if (request.BagInfoID > 0)
            {
                // 预检后等待跨服回包期间背包可能变化，入包失败则用邮件交付原物。
                response.Error = await UnionWarehouseServerHelper.DeliverItem(unit, inner.BagInfo);
            }
            else
            {
                role.UpdateRoleData(UserDataType.Gold, request.ItemNum.ToString(), true, ItemGetWay.UnionWarehouse);
            }

            response.WarehouseList = inner.WarehouseList;
            response.WarehouseGold = inner.WarehouseGold;
            reply();
        }
    }
}

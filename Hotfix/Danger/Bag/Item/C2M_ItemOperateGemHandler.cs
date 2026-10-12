using System;
using System.Collections.Generic;

namespace ET
{

    [ActorMessageHandler]
    public class C2M_ItemOperateGemHandler : AMActorLocationRpcHandler<Unit, C2M_ItemOperateGemRequest, M2C_ItemOperateGemResponse>
    {

        protected override async ETTask Run(Unit unit, C2M_ItemOperateGemRequest request, M2C_ItemOperateGemResponse response, Action reply)
        {
            long bagInfoID = request.OperateBagID;
            BagComponentServer bagcComponentServer = unit.GetComponent<BagComponentServer>();
            BagInfo useBagInfo = bagcComponentServer.GetItemByLoc(ItemLocType.ItemLocBag, bagInfoID);
            if (useBagInfo == null)
            {
                useBagInfo = bagcComponentServer.GetItemByLoc(ItemLocType.ItemLocEquip, bagInfoID);
            }
            if (useBagInfo == null)
            {
                response.Error = ErrorCode.ERR_ItemUseError;
                reply();
                return;
            }

            if (bagcComponentServer.GetBagLeftCell() < 1)
            {
                response.Error = ErrorCode.ERR_BagIsFull;
                reply();
                return;
            }

            // 通知客户端背包刷新
            M2C_RoleBagUpdate m2c_bagUpdate = new M2C_RoleBagUpdate();

            //镶嵌宝石
            if (request.OperateType == 9)
            {
                bagcComponentServer.OnCostItemData(useBagInfo.BagInfoID, 1);
                Function_Fight.UnitUpdateProperty_Base(unit, true, true);
            }

            if (request.OperateType == 10)
            {
                int gemIndex = int.Parse(request.OperatePar);
            }
            MessageHelper.SendToClient(unit, m2c_bagUpdate);
            reply();
            await ETTask.CompletedTask;
        }
    }
}

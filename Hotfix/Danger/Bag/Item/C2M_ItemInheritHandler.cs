using System;
using System.Collections.Generic;

namespace ET    
{

    [ActorMessageHandler]
    public class C2M_ItemInheritHandler : AMActorLocationRpcHandler<Unit, C2M_ItemInheritRequest, M2C_ItemInheritResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_ItemInheritRequest request, M2C_ItemInheritResponse response, Action reply)
        {
            BagComponentServer bagComponentServer = unit.GetComponent<BagComponentServer>();
            BagInfo bagInfo = bagComponentServer.GetItemByLoc(ItemLocType.ItemLocBag, request.OperateBagID);
            if (bagInfo == null)
            {
                response.Error = ErrorCode.ERR_ItemNotExist;
                reply();
                return;
            }
            //if (bagInfo.InheritTimes >= LDGlobalValueCategory.Instance.TempValue)
            //{
            //    response.Error = ErrorCode.ERR_TimesIsNot;
            //    reply();
            //    return;
            //}

            LDEquip Item = LDEquipCategory.Instance.Get(bagInfo.ItemID);
            string costitem = null;
            if (!bagComponentServer.CheckNeedItem(costitem))
            {
                response.Error = ErrorCode.ERR_ItemNotEnoughError;
                reply();
                return;
            }
            bagComponentServer.OnCostItemData(costitem, ItemLocType.ItemLocBag, ItemGetWay.ItemXiLian  );


            int skillid = 0;
            if (skillid == 0) {
                response.Error = ErrorCode.ERR_EquipChuanChengFail;
                reply();
            }

            response.InheritSkills.Add(skillid);
            bagInfo.SetBinding(true);
            //bagInfo.InheritTimes += 1;
            //通知客户端背包道具发生改变
            M2C_RoleBagUpdate m2c_bagUpdate = new M2C_RoleBagUpdate();;
            m2c_bagUpdate.BagInfoUpdate.Add(bagInfo);
            MessageHelper.SendToClient(unit, m2c_bagUpdate);
            reply();
            await ETTask.CompletedTask;
        }
    }
}

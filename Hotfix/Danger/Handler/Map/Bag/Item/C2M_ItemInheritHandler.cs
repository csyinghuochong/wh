using System;
using System.Collections.Generic;

namespace ET    
{

    [ActorMessageHandler]
    public class C2M_ItemInheritHandler : AMActorLocationRpcHandler<Unit, C2M_ItemInheritRequest, M2C_ItemInheritResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_ItemInheritRequest request, M2C_ItemInheritResponse response, Action reply)
        {
            BagInfo bagInfo = unit.GetComponent<BagComponent>().GetItemByLoc(ItemLocType.ItemLocBag, request.OperateBagID);
            if (bagInfo == null)
            {
                response.Error = ErrorCode.ERR_ItemNotExist;
                reply();
                return;
            }
            if (bagInfo.InheritTimes >= LDGlobalValueCategory.Instance.TempValue)
            {
                response.Error = ErrorCode.ERR_TimesIsNot;
                reply();
                return;
            }

            LDEquip Item = LDEquipCategory.Instance.Get(bagInfo.ItemID);
            string costitem = ItemHelper.GetInheritCost(bagInfo.InheritTimes);
            BagComponent bagComponent = unit.GetComponent<BagComponent>();
            if (!bagComponent.CheckNeedItem(costitem))
            {
                response.Error = ErrorCode.ERR_ItemNotEnoughError;
                reply();
                return;
            }
            unit.GetComponent<BagComponent>().OnCostItemData(costitem, ItemLocType.ItemLocBag, ItemGetWay.ItemXiLian  );
          
          
            int skillid = XiLianHelper.XiLianChuanChengJianDing(Item, unit.GetComponent<RoleInfoComponent>().RoleInfo.Occ, unit.GetComponent<RoleInfoComponent>().RoleInfo.OccTwo);

            if (skillid == 0) {
                response.Error = ErrorCode.ERR_EquipChuanChengFail;
                reply();
            }

            response.InheritSkills.Add(skillid);
            bagInfo.isBinging = true;   
            bagInfo.InheritTimes += 1;
            unit.GetComponent<BagComponent>().InheritSkills = response.InheritSkills;
            //通知客户端背包道具发生改变
            M2C_RoleBagUpdate m2c_bagUpdate = new M2C_RoleBagUpdate();;
            m2c_bagUpdate.BagInfoUpdate.Add(bagInfo);
            MessageHelper.SendToClient(unit, m2c_bagUpdate);
            reply();
            await ETTask.CompletedTask;
        }
    }
}

using System;
using System.Collections.Generic;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_JingHeWearHandler : AMActorLocationRpcHandler<Unit, C2M_JingHeWearRequest, M2C_JingHeWearResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_JingHeWearRequest request, M2C_JingHeWearResponse response, Action reply)
        {
            int equipIndex = 0;
            try
            {
                equipIndex = int.Parse(request.OperatePar);
            }
            catch (Exception ex) 
            {
                Log.Error(ex);  
                Log.Error($"C2M_JingHeWearRequest 1");
                response.Error = ErrorCode.ERR_ModifyData;
                reply();
                return;
            }


            ItemLocType locType = ItemLocType.ItemLocBag;
            BagComponentServer bagComponentServer = unit.GetComponent<BagComponentServer>();
            RoleInfoComponentServer roleInfoComponentServer = unit.GetComponent<RoleInfoComponentServer>();
            RoleInfo roleInfo = roleInfoComponentServer.RoleInfo;
            BagInfo useBagInfo = bagComponentServer.GetItemByLoc(locType, request.OperateBagID);
            if (useBagInfo == null)
            {
                response.Error = ErrorCode.ERR_ItemNotExist;
                reply();
                return;
            }

            //通知客户端背包刷新
            M2C_RoleBagUpdate m2c_bagUpdate = new M2C_RoleBagUpdate();
            if (request.OperateType == 1)
            {
                LDItem ldItem = LDItemCategory.Instance.Get(useBagInfo.ItemID);
             
                if (roleInfo.Lv < ldItem.UseLv_Min)
                {
                    response.Error = ErrorCode.ERR_EquipLvLimit;
                    reply();
                    return;
                }
                if (ldItem.ItemType != ItemTypeEnum.Equipment )
                {
                    response.Error = ErrorCode.ERR_EquipType;
                    reply();
                    return;
                }
                if (bagComponentServer.IsEquipJingHe(useBagInfo.ItemID))
                {
                    response.Error = ErrorCode.ERR_EquipType;
                    reply();
                    return;
                }

                //穿戴 获取当前位置是否有装备
                BagInfo beforeequip = bagComponentServer.GetJingHeByWeiZhi(equipIndex);
               
                Function_Fight.UnitUpdateProperty_Base(unit, true, true);
                useBagInfo.IsBinging = true;
                m2c_bagUpdate.BagInfoUpdate.Add(useBagInfo);
            }
            if (request.OperateType == 2)
            {
                //卸下  判断背包格子是否足够
               
            }

            MessageHelper.SendToClient(unit, m2c_bagUpdate);
            //通知客户端属性刷新

            reply();
            await ETTask.CompletedTask;
        }
    }
}

using System;

namespace ET
{

    /// <summary>
    /// 装备武器
    /// </summary>
    [ActorMessageHandler]
    public class C2M_EquipWearHandler : AMActorLocationRpcHandler<Unit, C2M_EquipWearRequest, M2C_EquipWearResponse>
    {

        protected override async ETTask Run(Unit unit, C2M_EquipWearRequest request, M2C_EquipWearResponse response, Action reply)
        {
            RoleInfoComponentServer roleInfoComponentServer = unit.GetComponent<RoleInfoComponentServer>();
            RoleInfo useInfo = roleInfoComponentServer.RoleInfo;
            BagComponentServer bag = unit.GetComponent<BagComponentServer>();
            SkillSetComponentServer skillSet = unit.GetComponent<SkillSetComponentServer>();
            NumericComponent numeric = unit.GetComponent<NumericComponent>();
            ChengJiuComponentServer chengJiu = unit.GetComponent<ChengJiuComponentServer>();
            long bagInfoID = request.OperateBagID;
            int occ = useInfo.Occ;
            //通知客户端背包刷新
            M2C_RoleBagUpdate m2c_bagUpdate = new M2C_RoleBagUpdate();
            
            if (request.OperateType == 3)
            {
                ItemLocType locType = ItemLocType.ItemLocBag;
                BagInfo useBagInfo = bag.GetItemByLoc(locType, bagInfoID);
                if (useBagInfo == null)
                {
                    response.Error = ErrorCode.ERR_ItemNotExist;    
                    reply();
                    return;
                }
                
                LDEquip ldItem = LDEquipCategory.Instance.Get(useBagInfo.ItemID);

                bool canWearEquip = ItemNewHelper.CheckCanWearEquip(useBagInfo.ItemID, occ);
                if (!canWearEquip)
                {
                    response.Error = ErrorCode.ERR_Equip_NoMtach;    
                    reply();
                    return;
                }

                //判断等级
                int roleLv = useInfo.Lv;
                int equipLv = ldItem.UseLv;

                if (roleLv < equipLv)
                {
                    response.Error = ErrorCode.ERR_EquipLvLimit;
                    reply();
                    return;
                }

                int caowei = ItemNewHelper.GetNewEquipCaoWei(useBagInfo.ItemID);
              
              //获取之前的位置是否有装备
                BagInfo beforeequip = bag.GetEquipBySubType(ItemLocType.ItemLocEquip, caowei);

                if (beforeequip != null)
                {
                    bag.OnChangeItemLoc(beforeequip, ItemLocType.ItemLocBag, ItemLocType.ItemLocEquip);
                    bag.OnChangeItemLoc(useBagInfo, ItemLocType.ItemLocEquip, ItemLocType.ItemLocBag);

                    skillSet.OnTakeOffEquip(ItemLocType.ItemLocEquip, beforeequip);
                    skillSet.OnWearEquip(useBagInfo);
                    m2c_bagUpdate.BagInfoUpdate.Add(beforeequip);
                }
                else
                {
                    bag.OnChangeItemLoc(useBagInfo, ItemLocType.ItemLocEquip, ItemLocType.ItemLocBag);
                    skillSet.OnWearEquip(useBagInfo);
                }
               
                Function_Fight.UnitUpdateProperty_Base(unit, true, true);
                useBagInfo.IsBinging = true;
                m2c_bagUpdate.BagInfoUpdate.Add(useBagInfo);
            }
            else
            {
                //判断背包格子是否足够
                bool full = bag.IsBagFullByLoc((int)ItemLocType.ItemLocBag);
                if (full)
                {
                    response.Error = ErrorCode.ERR_BagIsFull;
                    reply();
                    return;
                }
                
                ItemLocType locType = ItemLocType.ItemLocEquip;
                BagInfo useBagInfo = bag.GetItemByLoc(locType, bagInfoID);
                if (useBagInfo == null)
                {
                    response.Error = ErrorCode.ERR_ItemNotExist;    
                    reply();
                    return;
                }

                bag.OnChangeItemLoc(useBagInfo, ItemLocType.ItemLocBag, ItemLocType.ItemLocEquip);
                skillSet.OnTakeOffEquip(ItemLocType.ItemLocEquip, useBagInfo);
                Function_Fight.UnitUpdateProperty_Base(unit, true, true);
                m2c_bagUpdate.BagInfoUpdate.Add(useBagInfo);
            }

            // 穿脱结束后统一同步当前武器 Numeric，避免分支内重复写
            BagInfo equip_0 = bag.GetEquipBySubType(ItemLocType.ItemLocEquip, (int)EquipCaoWeiTypeEnum.Wuqi_1);
            numeric.ApplyValue(NumericType.Now_Weapon, equip_0 !=null ? equip_0.ItemID : 0);

            MessageHelper.SendToClient(unit, m2c_bagUpdate);
            
            reply();
            await ETTask.CompletedTask;
        }
    }
}

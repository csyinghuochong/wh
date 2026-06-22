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
            RoleInfoComponent roleInfoComponent = unit.GetComponent<RoleInfoComponent>();
            RoleInfo useInfo = roleInfoComponent.RoleInfo;
            long bagInfoID = request.OperateBagID;
            int occ = useInfo.Occ;

            //通知客户端背包刷新
            M2C_RoleBagUpdate m2c_bagUpdate = new M2C_RoleBagUpdate();
            
            if (request.OperateType == 3)
            {

                ItemLocType locType = ItemLocType.ItemLocBag;
                BagInfo useBagInfo = unit.GetComponent<BagComponent>().GetItemByLoc(locType, bagInfoID);
                if (useBagInfo == null)
                {
                    response.Error = ErrorCode.ERR_ItemNotExist;    
                    reply();
                    return;
                }
                LDEquip ldItem = LDEquipCategory.Instance.Get(useBagInfo.ItemID);

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
                BagInfo beforeequip = unit.GetComponent<BagComponent>().GetEquipBySubType(ItemLocType.ItemLocEquip, caowei);

                if (beforeequip != null)
                {
                    unit.GetComponent<BagComponent>().OnChangeItemLoc(beforeequip, ItemLocType.ItemLocBag, ItemLocType.ItemLocEquip);
                    unit.GetComponent<BagComponent>().OnChangeItemLoc(useBagInfo, ItemLocType.ItemLocEquip, ItemLocType.ItemLocBag);

                    unit.GetComponent<SkillSetComponent>().OnTakeOffEquip(ItemLocType.ItemLocEquip, beforeequip);
                    unit.GetComponent<SkillSetComponent>().OnWearEquip(useBagInfo);
                    m2c_bagUpdate.BagInfoUpdate.Add(beforeequip);
                }
                else
                {
                    unit.GetComponent<BagComponent>().OnChangeItemLoc(useBagInfo, ItemLocType.ItemLocEquip, ItemLocType.ItemLocBag);
                    unit.GetComponent<SkillSetComponent>().OnWearEquip(useBagInfo);
                }
                int zodiacnumber = unit.GetComponent<BagComponent>().GetZodiacnumber();
                unit.GetComponent<ChengJiuComponent>().TriggerEvent(ChengJiuTargetEnum.ZodiacEquipNumber_215, 0, zodiacnumber);

                Function_Fight.UnitUpdateProperty_Base(unit, true, true);
                useBagInfo.isBinging = true;
                m2c_bagUpdate.BagInfoUpdate.Add(useBagInfo);

                if (caowei == (int)EquipCaoWeiTypeEnum.Wuqi_1)
                {
                    unit.GetComponent<SkillPassiveComponent>().OnTrigegerPassiveSkill(SkillPassiveTypeEnum.WandBuff_8, useBagInfo.ItemID);
                    unit.GetComponent<NumericComponent>().ApplyValue(NumericType.Now_Weapon, useBagInfo.ItemID);
                    unit.GetComponent<NumericComponent>().ApplyValue(NumericType.WearWeaponFisrt, 1, true, true);
                }
            }
            else
            {
                //判断背包格子是否足够
                bool full = unit.GetComponent<BagComponent>().IsBagFull();
                if (full)
                {
                    response.Error = ErrorCode.ERR_BagIsFull;
                    reply();
                    return;
                }
                
                ItemLocType locType = ItemLocType.ItemLocEquip;
                BagInfo useBagInfo = unit.GetComponent<BagComponent>().GetItemByLoc(locType, bagInfoID);
                if (useBagInfo == null)
                {
                    response.Error = ErrorCode.ERR_ItemNotExist;    
                    reply();
                    return;
                }

                int caowei = ItemNewHelper.GetNewEquipCaoWei(useBagInfo.ItemID);
                unit.GetComponent<BagComponent>().OnChangeItemLoc(useBagInfo, ItemLocType.ItemLocBag, ItemLocType.ItemLocEquip);
                unit.GetComponent<SkillSetComponent>().OnTakeOffEquip(ItemLocType.ItemLocEquip, useBagInfo);
                Function_Fight.UnitUpdateProperty_Base(unit, true, true);
                m2c_bagUpdate.BagInfoUpdate.Add(useBagInfo);
                if (caowei == (int)EquipCaoWeiTypeEnum.Wuqi_1)
                {
                    unit.GetComponent<SkillPassiveComponent>().OnTrigegerPassiveSkill(SkillPassiveTypeEnum.WandBuff_8, 0);
                    unit.GetComponent<NumericComponent>().ApplyValue(NumericType.Now_Weapon, 0);
                }
            }

            BagInfo equip_0 = unit.GetComponent<BagComponent>().GetEquipBySubType(ItemLocType.ItemLocEquip, (int)EquipCaoWeiTypeEnum.Wuqi_1);
            unit.GetComponent<NumericComponent>().ApplyValue(NumericType.Now_Weapon, equip_0 !=null ? equip_0.ItemID : 0);

            MessageHelper.SendToClient(unit, m2c_bagUpdate);
            
            reply();
            await ETTask.CompletedTask;
        }
    }
}

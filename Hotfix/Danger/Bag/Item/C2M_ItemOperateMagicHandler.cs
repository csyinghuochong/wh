using System;
using System.Collections.Generic;


namespace ET
{
    [ActorMessageHandler]
    public class C2M_ItemOperateMagicHandler : AMActorLocationRpcHandler<Unit, C2M_ItemOperateMagicRequest, M2C_ItemOperateMagicResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_ItemOperateMagicRequest request, M2C_ItemOperateMagicResponse response, Action reply)
        {
            RoleInfoComponentServer roleInfoComponentServer = unit.GetComponent<RoleInfoComponentServer>();
            RoleInfo useInfo = roleInfoComponentServer.RoleInfo;
            long bagInfoID = request.OperateBagID;

            ItemLocType locType = ItemLocType.ItemLocBag;
            if (request.OperateType == 3)
            {
                locType = ItemLocType.ItemLocBag;
            }
            else if (request.OperateType == 4)
            {
                locType = ItemLocType.ItemLocEquip;
            }
            else
            {
                reply();
                return;
            }
            
          
            BagInfo useBagInfo = unit.GetComponent<BagComponentServer>().GetItemByLoc(locType, bagInfoID);
            if (useBagInfo == null )
            {
                reply();
                return;
            }

            int weizhi = -1;
            LDItem ldItem = null;
            if (useBagInfo != null)
            {
                ldItem = LDItemCategory.Instance.Get(useBagInfo.ItemID);
                weizhi = ldItem.ItemType;
            }

            int equipType = ItemHelper.GetNewEquipType(useBagInfo);
            if (ldItem.ItemType != 3 || equipType != 401)
            {
                reply();
                return;
            }

            int equipposition = int.Parse(request.OperatePar);
            if (equipposition >= 9)
            {
                reply();
                return;
            }


            int subtype = ldItem.ItemType - 4001; //0 1 2
            int curtype = equipposition / 3;
            if (curtype != subtype && curtype != 2)
            {
                reply();
                return;
            }

            //通知客户端背包刷新
            M2C_RoleBagUpdate m2c_bagUpdate = new M2C_RoleBagUpdate();
            //穿戴装备
            if (request.OperateType == 3)
            {
                //判断等级
                int roleLv = useInfo.Lv;
                int equipLv = ldItem.UseLv;
               
                if (roleLv < equipLv)
                {
                    response.Error = ErrorCode.ERR_EquipLvLimit;
                    reply();
                    return;
                }

                //获取之前的位置是否有装备
                BagInfo beforeequip = unit.GetComponent<BagComponentServer>().GetMagicEquipBySubType(ItemLocType.ItemLocEquip,  equipposition);
                if (beforeequip != null)
                {
                    unit.GetComponent<BagComponentServer>().OnChangeItemLoc(beforeequip, ItemLocType.ItemLocBag, ItemLocType.ItemLocEquip);
                    unit.GetComponent<BagComponentServer>().OnChangeItemLoc(useBagInfo, ItemLocType.ItemLocEquip, ItemLocType.ItemLocBag);

                    unit.GetComponent<SkillSetComponent>().OnTakeOffEquip(ItemLocType.ItemLocEquip, beforeequip);
                    unit.GetComponent<SkillSetComponent>().OnWearEquip(useBagInfo);
                    beforeequip.EquipIndex = -1;
                    useBagInfo.EquipIndex = equipposition;
                    m2c_bagUpdate.BagInfoUpdate.Add(beforeequip);
                }
                else
                {
                    unit.GetComponent<BagComponentServer>().OnChangeItemLoc(useBagInfo, ItemLocType.ItemLocEquip, ItemLocType.ItemLocBag);
                    unit.GetComponent<SkillSetComponent>().OnWearEquip(useBagInfo);
                    useBagInfo.EquipIndex = equipposition;
                }
               
                Function_Fight.UnitUpdateProperty_Base(unit, true, true);
                useBagInfo.isBinging = true;
                m2c_bagUpdate.BagInfoUpdate.Add(useBagInfo);
                //if (weizhi == (int)EquipCaoWeiTypeEnum.Wuqi)
                //{
                //    unit.GetComponent<SkillPassiveComponent>().OnTrigegerPassiveSkill(SkillPassiveTypeEnum.WandBuff_8, useBagInfo.ItemID);
                //    unit.GetComponent<NumericComponent>().ApplyValue(NumericType.Now_Weapon, useBagInfo.ItemID);
                //    unit.GetComponent<NumericComponent>().ApplyValue(NumericType.WearWeaponFisrt, 1, true, true);
                //}
            }

            //卸下装备
            if (request.OperateType == 4)
            {
                //判断背包格子是否足够
                bool full = unit.GetComponent<BagComponentServer>().IsBagFull();
                if (full)
                {
                    response.Error = ErrorCode.ERR_BagIsFull;
                    reply();
                    return;
                }
               
                unit.GetComponent<BagComponentServer>().OnChangeItemLoc(useBagInfo, ItemLocType.ItemLocBag, ItemLocType.ItemLocEquip);
                unit.GetComponent<SkillSetComponent>().OnTakeOffEquip(ItemLocType.ItemLocEquip, useBagInfo);
                Function_Fight.UnitUpdateProperty_Base(unit, true, true);
                m2c_bagUpdate.BagInfoUpdate.Add(useBagInfo);
            }

            MessageHelper.SendToClient(unit, m2c_bagUpdate);
            reply();
            await ETTask.CompletedTask;
        }
    }
}
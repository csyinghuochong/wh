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
            BagComponentServer bagComponentServer = unit.GetComponent<BagComponentServer>();
            SkillSetComponentServer skillSetComponentServer = unit.GetComponent<SkillSetComponentServer>();
            long bagInfoID = request.OperateBagID;

            if (request.OperateType != 3 && request.OperateType != 4)
            {
                reply();
                return;
            }

            ItemLocType locType = request.OperateType == 4 ? ItemLocType.ItemLocEquip : ItemLocType.ItemLocBag;
            int equipposition = int.Parse(request.OperatePar);

            BagInfo useBagInfo = bagComponentServer.GetItemByLoc(locType, bagInfoID);
            if (useBagInfo == null )
            {
                reply();
                return;
            }

            LDItem ldItem = LDItemCategory.Instance.Get(useBagInfo.ItemID);
          
            if (ldItem.ItemType != 3 )
            {
                reply();
                return;
            }
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
                BagInfo beforeequip = bagComponentServer.GetMagicEquipBySubType(ItemLocType.ItemLocEquip,  equipposition);
                if (beforeequip != null)
                {
                    bagComponentServer.OnChangeItemLoc(beforeequip, ItemLocType.ItemLocBag, ItemLocType.ItemLocEquip);
                    bagComponentServer.OnChangeItemLoc(useBagInfo, ItemLocType.ItemLocEquip, ItemLocType.ItemLocBag);

                    skillSetComponentServer.OnTakeOffEquip(ItemLocType.ItemLocEquip, beforeequip);
                    skillSetComponentServer.OnWearEquip(useBagInfo);
                    m2c_bagUpdate.BagInfoUpdate.Add(beforeequip);
                }
                else
                {
                    bagComponentServer.OnChangeItemLoc(useBagInfo, ItemLocType.ItemLocEquip, ItemLocType.ItemLocBag);
                    skillSetComponentServer.OnWearEquip(useBagInfo);
                    //useBagInfo.EquipIndex = equipposition;
                }
               
                Function_Fight.UnitUpdateProperty_Base(unit, true, true);
                useBagInfo.IsBinging = true;
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
                bool full = bagComponentServer.IsBagFullByLoc((int)ItemLocType.ItemLocBag);
                if (full)
                {
                    response.Error = ErrorCode.ERR_BagIsFull;
                    reply();
                    return;
                }
               
                bagComponentServer.OnChangeItemLoc(useBagInfo, ItemLocType.ItemLocBag, ItemLocType.ItemLocEquip);
                skillSetComponentServer.OnTakeOffEquip(ItemLocType.ItemLocEquip, useBagInfo);
                Function_Fight.UnitUpdateProperty_Base(unit, true, true);
                m2c_bagUpdate.BagInfoUpdate.Add(useBagInfo);
            }

            MessageHelper.SendToClient(unit, m2c_bagUpdate);
            reply();
            await ETTask.CompletedTask;
        }
    }
}
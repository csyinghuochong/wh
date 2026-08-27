using System;
using System.Collections.Generic;
using System.Linq;

namespace ET
{

    [ActorMessageHandler]
    public class C2M_ChangeOccHandler : AMActorLocationRpcHandler<Unit, C2M_ChangeOccRequest, M2C_ChangeOccResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_ChangeOccRequest request, M2C_ChangeOccResponse response, Action reply)
        {            response.Error = ErrorCode.ERR_ModifyData;
            reply();
            await ETTask.CompletedTask;
#if false // TODO: migrate to LD config

            BagComponentServer bagComponentServer = unit.GetComponent<BagComponentServer>();
            BagInfo useBagInfo = bagComponentServer.GetItemByLoc(ItemLocType.ItemLocBag, request.BagInfoID);
            if (useBagInfo == null )
            {
                response.Error = ErrorCode.ERR_ItemNotExist;
                reply();
                return;
            }

            int equip1number = 0;
            HashSet<int> equiplist = new HashSet<int>();  
            for (int equip = 0; equip < bagComponentServer.EquipList.Count; equip++)
            {
                BagInfo equipInfo = bagComponentServer.EquipList[equip];
                LDItem ldItem = LDItemCategory.Instance.Get(equipInfo.ItemID);
                int equipType = ItemHelper.GetNewEquipType(equipInfo);
                if (equipType <= 100)
                {
                    equip1number++;
                }

                equiplist.Add(equipType);
            }

            foreach (int equiptype in equiplist)
            {
                Console.WriteLine($"EquipType：  {equiptype}");
            }

            int allequipNumber = equip1number + bagComponentServer.EquipList_2.Count + bagComponentServer.FashionActiveIds.Count;
            if (bagComponentServer.GetBagLeftCell() < allequipNumber)
            {
                response.Error = ErrorCode.ERR_BagIsFull;
                reply();
                return;
            }

            RoleInfoComponentServer roleInfoComponentServer = unit.GetComponent<RoleInfoComponentServer>();
            int oldOcc = roleInfoComponentServer.RoleInfo.Occ;
            if (oldOcc == request.Occ)
            {
                response.Error = ErrorCode.ERR_ModifyData;
                reply();
                return;
            }

            //装备(强制脱下)
            SkillSetComponentServer skillSetComponentServer = unit.GetComponent<SkillSetComponentServer>();
            long[] equipids = bagComponentServer.EquipList.Select(p=>p.BagInfoID).ToArray();
            foreach (long equipid in equipids)   
            { 
                BagInfo equipInfo = bagComponentServer.GetItemByLoc(ItemLocType.ItemLocEquip, equipid);
                if (equipInfo == null)
                {
                    Console.WriteLine($"xxxx: {equipid}");
                    continue;
                }

                LDItem ldItem = LDItemCategory.Instance.Get(equipInfo.ItemID);
                int equipType = ItemHelper.GetNewEquipType(equipInfo);
                if (equipType > 100)
                {
                    continue;
                }

                bagComponentServer.OnChangeItemLoc(equipInfo, ItemLocType.ItemLocBag, ItemLocType.ItemLocEquip);
                skillSetComponentServer.OnTakeOffEquip(ItemLocType.ItemLocEquip, equipInfo);
            }

            /*long[] equipids_2 = bagComponentServer.EquipList_2.Select(p => p.BagInfoID).ToArray();
            foreach (long equipid in equipids_2)
            {
                BagInfo equipInfo = bagComponentServer.GetItemByLoc(ItemLocType.ItemLocEquip_2, equipid);
                if (equipInfo == null)
                {
                    Console.WriteLine($"xxxxxx2: {equipid}");
                    continue;
                }

                unit.GetComponent<BagComponentServer>().OnChangeItemLoc(equipInfo, ItemLocType.ItemLocBag, ItemLocType.ItemLocEquip_2);
                unit.GetComponent<SkillSetComponent>().OnTakeOffEquip(ItemLocType.ItemLocEquip_2, equipInfo);
            }*/
            unit.GetComponent<SkillPassiveComponent>().OnTrigegerPassiveSkill(SkillPassiveTypeEnum.WandBuff_8, 0);
            unit.GetComponent<NumericComponent>().ApplyValue(NumericType.Now_Weapon, 0);

            //技能(清空 技能点重置)
            //觉醒转换成对应的
            int level = roleInfoComponentServer.RoleInfo.Lv;
            int sp = (int)unit.GetComponent<BagComponentServer>().GetItemNumber(ItemBigType.Type_Item, UserDataType.Sp);

            roleInfoComponentServer.UpdateRoleData(UserDataType.Sp, (level - sp).ToString());

            
            skillSetComponentServer.TianFuList.Clear();
            skillSetComponentServer.TianFuList1.Clear();
            skillSetComponentServer.TianFuPlan = 0;

            //觉醒技能先保留 转职的时候再转换
            for (int k = skillSetComponentServer.SkillList.Count - 1; k >= 0; k--)
            {
                SkillPro skillPro = skillSetComponentServer.SkillList[k];
                //if (skillPro.SkillSetType == SkillSetEnum.Item)
                //{
                //    continue;
                //}

                int skillid = skillPro.SkillID;
                if (OccupationJueXingConfigCategory.Instance.Contain(skillid))
                {
                    continue;
                }
                Console.WriteLine($"removeSkill:  {skillid}    {skillPro.SkillSetType}  {skillPro.SkillSource}");
                skillSetComponentServer.SkillList.RemoveAt(k);
            }

            //需要选择第二职业
            if (request.OccTwo != 0)
            {
                skillSetComponentServer.OnChangeJueXing(roleInfoComponentServer.RoleInfo.OccTwo, request.OccTwo);
                skillSetComponentServer.OnChangeOccTwoRequest(request.OccTwo);
            }

            //时装(清空 返回碎片或者其他)
            for (int fashionid = 0; fashionid < bagComponentServer.FashionActiveIds.Count; fashionid++)
            {
                LDFashion ldFashion = LDFashionCategory.Instance.Get(bagComponentServer.FashionActiveIds[fashionid]);

                if (CommonHelper.IfNull(ldFashion.ActiveCost) || ldFashion.ActiveCost.Equals("0;0"))
                {
                    continue;
                }
                bagComponentServer.OnAddItemData(ldFashion.ActiveCost, $"{ItemGetWay.HuiShou}_{TimeHelper.ServerNow()}", false);
            }
            bagComponentServer.FashionActiveIds.Clear();
            bagComponentServer.FashionEquipList.Clear();

            bagComponentServer.OnCostItemData(request.BagInfoID, 1);

            DataCollationComponent dataCollationComponent = unit.GetComponent<DataCollationComponent>();
            dataCollationComponent.OccOld = roleInfoComponentServer.RoleInfo.Occ;
            roleInfoComponentServer.RoleInfo.Occ = request.Occ;
            
            if (request.OccTwo != 0)
            {
                dataCollationComponent.OccTwoOld = 0;
                roleInfoComponentServer.RoleInfo.OccTwo = request.OccTwo;
            }
            else
            {
                dataCollationComponent.OccTwoOld = roleInfoComponentServer.RoleInfo.OccTwo;
                roleInfoComponentServer.RoleInfo.OccTwo = 0;
            }

            reply();
            await ETTask.CompletedTask;
        #endif
}
    }
}

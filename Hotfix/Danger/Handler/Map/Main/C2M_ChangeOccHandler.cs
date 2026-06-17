using System;
using System.Collections.Generic;
using System.Linq;

namespace ET
{

    [ActorMessageHandler]
    public class C2M_ChangeOccHandler : AMActorLocationRpcHandler<Unit, C2M_ChangeOccRequest, M2C_ChangeOccResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_ChangeOccRequest request, M2C_ChangeOccResponse response, Action reply)
        {
            BagComponent bagComponent = unit.GetComponent<BagComponent>();
            BagInfo useBagInfo = bagComponent.GetItemByLoc(ItemLocType.ItemLocBag, request.BagInfoID);
            if (useBagInfo == null )
            {
                response.Error = ErrorCode.ERR_ItemNotExist;
                reply();
                return;
            }

            int equip1number = 0;
            List<int> equiplist = new List<int>();  
            for (int equip = 0; equip < bagComponent.EquipList.Count; equip++)
            {
                BagInfo equipInfo = bagComponent.EquipList[equip];
                LDItem ldItem = LDItemCategory.Instance.Get(equipInfo.ItemID);
                int equipType = ItemHelper.GetNewEquipType(equipInfo);
                if (equipType <= 100)
                {
                    equip1number++;
                }

                if (!equiplist.Contains(equipType))
                {
                    equiplist.Add(equipType);
                }
            }

            foreach (int equiptype in equiplist)
            {
                Console.WriteLine($"EquipType：  {equiptype}");
            }

            int allequipNumber = equip1number + bagComponent.EquipList_2.Count + bagComponent.FashionActiveIds.Count;
            if (bagComponent.GetBagLeftCell() < allequipNumber)
            {
                response.Error = ErrorCode.ERR_BagIsFull;
                reply();
                return;
            }

            UserInfoComponent userInfoComponent = unit.GetComponent<UserInfoComponent>();
            int oldOcc = userInfoComponent.UserInfo.Occ;
            if (oldOcc == request.Occ)
            {
                response.Error = ErrorCode.ERR_ModifyData;
                reply();
                return;
            }

            //装备(强制脱下)
            long[] equipids = bagComponent.EquipList.Select(p=>p.BagInfoID).ToArray();
            foreach (long equipid in equipids)   
            { 
                BagInfo equipInfo = bagComponent.GetItemByLoc(ItemLocType.ItemLocEquip, equipid);
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

                unit.GetComponent<BagComponent>().OnChangeItemLoc(equipInfo, ItemLocType.ItemLocBag, ItemLocType.ItemLocEquip);
                unit.GetComponent<SkillSetComponent>().OnTakeOffEquip(ItemLocType.ItemLocEquip, equipInfo);
            }

            /*long[] equipids_2 = bagComponent.EquipList_2.Select(p => p.BagInfoID).ToArray();
            foreach (long equipid in equipids_2)
            {
                BagInfo equipInfo = bagComponent.GetItemByLoc(ItemLocType.ItemLocEquip_2, equipid);
                if (equipInfo == null)
                {
                    Console.WriteLine($"xxxxxx2: {equipid}");
                    continue;
                }

                unit.GetComponent<BagComponent>().OnChangeItemLoc(equipInfo, ItemLocType.ItemLocBag, ItemLocType.ItemLocEquip_2);
                unit.GetComponent<SkillSetComponent>().OnTakeOffEquip(ItemLocType.ItemLocEquip_2, equipInfo);
            }*/
            unit.GetComponent<SkillPassiveComponent>().OnTrigegerPassiveSkill(SkillPassiveTypeEnum.WandBuff_8, 0);
            unit.GetComponent<NumericComponent>().ApplyValue(NumericType.Now_Weapon, 0);

            //技能(清空 技能点重置)
            //觉醒转换成对应的
            int level = userInfoComponent.UserInfo.Lv;
            int sp = userInfoComponent.UserInfo.Sp;

            userInfoComponent.UpdateRoleData(UserDataType.Sp, (level - sp).ToString());

            
            SkillSetComponent skillSetComponent = unit.GetComponent<SkillSetComponent>();
            skillSetComponent.TianFuList.Clear();
            skillSetComponent.TianFuList1.Clear();
            skillSetComponent.TianFuPlan = 0;

            //觉醒技能先保留 转职的时候再转换
            for (int k = skillSetComponent.SkillList.Count - 1; k >= 0; k--)
            {
                SkillPro skillPro = skillSetComponent.SkillList[k];
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
                skillSetComponent.SkillList.RemoveAt(k);
            }

            int[] SkillList = LDOccupationCategory.Instance.Get(request.Occ).Skill_Base;
            if (SkillList != null)
            {
                for (int i = 0; i < SkillList.Length; i++)
                {
                    if (i == 0)
                    {
                        skillSetComponent.SkillList.Add(new SkillPro() { SkillID = SkillList[i], SkillPosition = 1, SkillSetType = (int)SkillSetEnum.Skill });
                    }
                    else
                    {
                        skillSetComponent.SkillList.Add(new SkillPro() { SkillID = SkillList[i] });
                    }
                }

            }


            //需要选择第二职业
            if (request.OccTwo != 0)
            {
                skillSetComponent.OnChangeJueXing(userInfoComponent.UserInfo.OccTwo, request.OccTwo);
                skillSetComponent.OnChangeOccTwoRequest(request.OccTwo);
            }

            //时装(清空 返回碎片或者其他)
            for (int fashionid = 0; fashionid < bagComponent.FashionActiveIds.Count; fashionid++)
            {
                LDFashion ldFashion = LDFashionCategory.Instance.Get(bagComponent.FashionActiveIds[fashionid]);

                if (CommonHelper.IfNull(ldFashion.ActiveCost) || ldFashion.ActiveCost.Equals("0;0"))
                {
                    continue;
                }
                bagComponent.OnAddItemData(ldFashion.ActiveCost, $"{ItemGetWay.HuiShou}_{TimeHelper.ServerNow()}", false);
            }
            bagComponent.FashionActiveIds.Clear();
            bagComponent.FashionEquipList.Clear();

            bagComponent.OnCostItemData(request.BagInfoID, 1);

            DataCollationComponent dataCollationComponent = unit.GetComponent<DataCollationComponent>();
            dataCollationComponent.OccOld = userInfoComponent.UserInfo.Occ;
            userInfoComponent.UserInfo.Occ = request.Occ;
            
            if (request.OccTwo != 0)
            {
                dataCollationComponent.OccTwoOld = 0;
                userInfoComponent.UserInfo.OccTwo = request.OccTwo;
            }
            else
            {
                dataCollationComponent.OccTwoOld = userInfoComponent.UserInfo.OccTwo;
                userInfoComponent.UserInfo.OccTwo = 0;
            }

            reply();
            await ETTask.CompletedTask;
        }
    }
}

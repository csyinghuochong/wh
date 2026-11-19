using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ET
{
    //[MessageHandler(AppType.Gate)]
    public static class Function_Fight
    {

        /// <summary>
        /// 全量重算角色静态战斗属性（职业+装备+加点+称号+坐骑），保留战斗 Buff 层。
        /// </summary>
        public static void UnitUpdateProperty_Base(Unit unit, bool notice, bool rank)
        {
            RoleInfoComponentServer roleInfoComponent = unit.GetComponent<RoleInfoComponentServer>();
            RoleInfo roleInfo = roleInfoComponent.RoleInfo;
            NumericComponent numeric = unit.GetComponent<NumericComponent>();

            // notice 时先快照一级属性，Reset 后再对比，避免无变化也推客户端
            Dictionary<int, long> beforeBaseAttrs = notice ? SnapshotBaseAttrs(numeric) : null;

            // 1. 清静态分项/一级结果（保留加点、运行时状态、战斗 Buff）
            numeric.ResetProperty();

            // 2. 职业初始属性 + 已穿装备属性
            Dictionary<int, long> staticAttrs = new Dictionary<int, long>();
            UnitStaticAttrBuilder.MergeOccupationAndEquip(unit, roleInfo.Occ, staticAttrs);

            // 3. 角色六维点数 + 称号：六维一起折算攻防与生命，其余属性直接累加
            int[] pointValues = UnitStaticAttrBuilder.CalcTotalPointValues(numeric, roleInfo.Lv);
            UnitStaticAttrBuilder.MergeTitleAttributes(unit, pointValues, staticAttrs);
            UnitStaticAttrBuilder.MergePointConvertAttrs(pointValues, staticAttrs);
            UnitStaticAttrBuilder.MergeBodyHpFixed(roleInfo, roleInfo.Lv, pointValues, staticAttrs);

            // 4. 所有坐骑按骑乘/非骑乘比例累加属性，速度仅骑乘时加入
            UnitStaticAttrBuilder.MergeMountAttributes(unit, staticAttrs);

            // 5. 写入汇总分项，统一重算生命、攻防、速度等最终属性
            numeric.ApplyAttributeDictionary(staticAttrs, false);

            // 6. 只推有变化的一级属性
            if (notice)
            {
                SendChangedBaseAttributeListUpdate(unit, numeric, beforeBaseAttrs);
            }

            // 7. 单独计算战力并同步角色数据
            int zhanliValue = CalcCombat(unit);
            roleInfoComponent.UpdateRoleData(UserDataType.Combat, zhanliValue.ToString(), notice);

        }

        /// <summary>快照 ForwardMap 一级属性存储值（Reset 前调用）。</summary>
        private static Dictionary<int, long> SnapshotBaseAttrs(NumericComponent numeric)
        {
            Dictionary<int, long> snap = new Dictionary<int, long>(AttrConfigManager.ForwardMap.Count);
            foreach (int baseAttr in AttrConfigManager.ForwardMap.Keys)
            {
                snap[baseAttr] = numeric.GetStoredValue(baseAttr);
            }

            return snap;
        }

        /// <summary>
        /// 对比快照，只同步有变化的一级属性存储值；全无变化则不发包。
        /// 必须发 GetStoredValue（NumericDic 原值：固定原样、千分比已是 ×1000），客户端 SetValueNoSync 直接写入。
        /// </summary>
        private static void SendChangedBaseAttributeListUpdate(Unit unit, NumericComponent numeric, Dictionary<int, long> before)
        {
            List<int> ks = new List<int>();
            List<long> vs = new List<long>();
            foreach (int baseAttr in AttrConfigManager.ForwardMap.Keys)
            {
                long now = numeric.GetStoredValue(baseAttr);
                if (before == null || !before.TryGetValue(baseAttr, out long old) || old != now)
                {
                    ks.Add(baseAttr);
                    vs.Add(now);
                }
            }

            if (ks.Count == 0)
            {
                return;
            }

            MessageHelper.SendToClient(unit, new M2C_UnitNumericListUpdate
            {
                UnitID = unit.Id,
                Ks = ks,
                Vs = vs,
            });
        }
        
 

        /// <summary>
        /// 当前战力 = 当前等级 Exp_Lv.CP_Role（取该级一行，不按等级累加）
        /// + 角色点数战力（初始、等级固定点按各自 Attribute.CP；全部自由点按六维统一 CP，分配与否均计入）
        /// + 已穿装备战力（UseLv × Equip_Type.CP + 基础、鉴定、强化原始属性值 × Attribute.CP）
        /// + 已激活坐骑战力（总点数 × 0.5 × 六维总资质 / 600 × 点数 Attribute.CP）。
        /// + SkillList 里每个技能当前等级 Skill_Battle_Lv.CP。
        /// 全部求和后统一取整，不读取面板、六维派生属性或 BUFF 属性。
        /// </summary>
        private static int CalcCombat(Unit unit)
        {
            // 各项先保留小数累加，返回角色总战力时再统一截去小数，避免逐项取整造成损失。
            double combat = 0;
            RoleInfo roleInfo = unit.GetComponent<RoleInfoComponentServer>().RoleInfo;

            double lvComabt = 0;
            // 1. 等级战力：直接取 Exp_Lv 当前等级这一行的 CP_Role，不累加之前等级的 CP_Role。
            // 等级生命 Hp_Standard × Occupation.Hp_Param 属于派生属性，不另算战力，保证各职业升级战力一致。
            if (LDExp_LvCategory.Instance != null
                && LDExp_LvCategory.Instance.Contain(roleInfo.Lv))
            {
                lvComabt += LDExp_LvCategory.Instance.Get(roleInfo.Lv).CP_Role;
            }

            combat += lvComabt;

            double pointComabt = 0;
            // 2. 角色初始点和等级固定点的战力；数组顺序为：体、力、智、念、敏、迅。
            // 初始点来自 GlobalValue.Add_Point_Init，是角色创建时各维度自带的点数。
            int[] initPoints = RoleAddPointHelper.GetInitPoints();
            // 固定点来自 GlobalValue.Add_Point_Level_UP_Fixed，按当前等级累计，1 级也计入一次。
            int[] fixedPoints = RoleAddPointHelper.GetCumulativeFixedPointsByLevel(roleInfo.Lv);
            for (int i = 0; i < RoleAddPointHelper.PointNumericTypes.Length; i++)
            {
                // 读取该维度 Attribute.CP，目前六维均为每点 5 战力，不在代码中写死系数。
                double pointCP = LDAttributeCategory.Instance.Get(RoleAddPointHelper.PointNumericTypes[i]).CP;
                // 只计算点数本身：(初始点 + 累计固定点) × CP；转为 long 后相加，避免 int 加法溢出。
                // 力转换出的物攻、体转换出的生命等派生属性不重复计算；装备六维在装备战力中单独计算。
                pointComabt += ((long)initPoints[i] + fixedPoints[i]) * pointCP;
            }

            // 3. 自由点战力：从 GlobalValue.Add_Point_Level_UP_Free 取当前等级累计获得的全部自由点。
            // 已分配、未分配都计入，因此加点和洗点不改变战力；这部分未包含在上面的初始点、固定点中。
            // 六维 CP 当前相同，统一读取力的 CP；不读取 Numeric 的当前六维值，避免混入 BUFF 修改的点数。
            pointComabt += RoleAddPointHelper.GetTotalFreePointByLevel(roleInfo.Lv)
                * LDAttributeCategory.Instance.Get(NumericType.Point_Li_2).CP;

            combat += pointComabt;

            double equipComabt = 0;
            List<BagInfo> equipList = unit.GetComponent<BagComponentServer>().GetItemByLoc(ItemLocType.ItemLocEquip);
            for (int i = 0; i < equipList.Count; i++)
            {
                equipComabt += ItemNewHelper.CalculateEquipCombatValue(equipList[i]);
            }

            combat += equipComabt;

            // 坐骑仅计算已激活的使用中/骑乘中实例，休息和未拥有的坐骑不计战力。
            // 总点数按坐骑等级读取，含已分配和未分配；总资质读取当前生效的六维资质，不读取派生属性。
            MountInfo activeMount = unit.GetComponent<MountComponentServer>().GetUseMount();
            if (activeMount != null)
            {
                combat += MountHelper.GetTotalAddPoint(activeMount) * 0.5d
                    * MountHelper.GetTotalAptitudeFightValue(activeMount) / 600d
                    * LDAttributeCategory.Instance.Get(NumericType.Point_Li_2).CP;
            }

            SkillSetComponentServer skillSet = unit.GetComponent<SkillSetComponentServer>();
            if (skillSet == null || LDSkill_Battle_LvCategory.Instance == null)
            {
                return (int)Math.Min(combat, int.MaxValue);
            }

            List<SkillPro> skillList = skillSet.SkillList;
            for (int i = 0; i < skillList.Count; i++)
            {
                SkillPro skill = skillList[i];
                if (skill == null || skill.Level <= 0)
                {
                    continue;
                }

                LDSkill_Battle_Lv skillLv = LDSkill_Battle_LvCategory.Instance.GetLDSkillLv(skill.SkillID, skill.Level);
                if (skillLv == null)
                {
                    continue;
                }

                combat += skillLv.CP;
            }

            return (int)Math.Min(combat, int.MaxValue);
        }
    }


}

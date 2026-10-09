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
           
            NumericComponent numeric = unit.GetComponent<NumericComponent>();

            // notice 时先快照一级属性，Reset 后再对比，避免无变化也推客户端
            Dictionary<int, long> beforeBaseAttrs = notice ? SnapshotBaseAttrs(numeric) : null;

            // 1. 清静态分项/一级结果（保留加点、运行时状态、战斗 Buff）
            numeric.ResetProperty();

            // 2. 组装分项字典 → 写入并重算一级属性
            Dictionary<int, long> staticAttrs = UnitStaticAttrBuilder.Build(unit);
            numeric.ApplyAttributeDictionary(staticAttrs, false);

            // 3. 只推有变化的一级属性
            if (notice)
            {
                SendChangedBaseAttributeListUpdate(unit, numeric, beforeBaseAttrs);
            }

            // 4. 刷新战力

            int zhanliValue = CalcCombat(unit);
            unit.GetComponent<RoleInfoComponentServer>().UpdateRoleData(UserDataType.Combat, zhanliValue.ToString(), notice);

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
        /// + SkillList 里每个技能当前等级 Skill_Battle_Lv.CP。
        /// </summary>
        private static int CalcCombat(Unit unit)
        {
            int combat = 0;
            RoleInfo roleInfo = unit.GetComponent<RoleInfoComponentServer>().RoleInfo;
            if (LDExp_LvCategory.Instance != null
                && LDExp_LvCategory.Instance.Contain(roleInfo.Lv))
            {
                combat += LDExp_LvCategory.Instance.Get(roleInfo.Lv).CP_Role;
            }

            SkillSetComponentServer skillSet = unit.GetComponent<SkillSetComponentServer>();
            if (skillSet == null || LDSkill_Battle_LvCategory.Instance == null)
            {
                return combat;
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

            return combat;
        }
    }


}

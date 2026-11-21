using System;
using System.Collections.Generic;

namespace ET
{
    public static partial class SkillSetComponentServerSystem
    {
        /// <summary>按当前穿戴同步套装技能。装备来源、BagInfoId=0、ParamId=套装ID 标记套装授予的技能。</summary>
        public static void RefreshEquipSuitSkills(this SkillSetComponentServer self, bool notice = true)
        {
            Unit unit = self.GetParent<Unit>();
            Dictionary<int, int> suitSkills = GetEquipSuitSkills(unit.GetComponent<BagComponentServer>().GetItemByLoc(ItemLocType.ItemLocEquip));
            HashSet<int> otherSkills = new HashSet<int>();
            for (int i = 0; i < self.SkillList.Count; i++)
            {
                SkillPro skill = self.SkillList[i];
                if (skill.SkillSource != SkillSourceEnum.Equip || skill.BagInfoId != 0 || skill.ParamId <= 0)
                {
                    otherSkills.Add(skill.SkillID);
                }
            }

            bool changed = false;
            HashSet<int> removedSkills = new HashSet<int>();
            for (int i = self.SkillList.Count - 1; i >= 0; i--)
            {
                SkillPro skill = self.SkillList[i];
                if (skill.SkillSource != SkillSourceEnum.Equip || skill.BagInfoId != 0 || skill.ParamId <= 0)
                {
                    continue;
                }

                if (suitSkills.TryGetValue(skill.SkillID, out int suitId) && !otherSkills.Contains(skill.SkillID))
                {
                    skill.ParamId = suitId;
                    continue;
                }

                removedSkills.Add(skill.SkillID);
                self.SkillList.RemoveAt(i);
                changed = true;
            }

            SkillPassiveComponent passive = unit.GetComponent<SkillPassiveComponent>();
            removedSkills.RemoveWhere(skillId => self.GetBySkillID(skillId) != null);
            foreach (int skillId in removedSkills)
            {
                passive.RemovePassiveSkill(skillId);
            }

            ClearBarSkills(self.SkillBarList0, removedSkills);
            ClearBarSkills(self.SkillBarList1, removedSkills);
            foreach (KeyValuePair<int, int> suitSkill in suitSkills)
            {
                if (self.GetBySkillID(suitSkill.Key) != null)
                {
                    continue;
                }

                SkillPro skill = self.AddSkillPro(suitSkill.Key, SkillSetEnum.Skill, SkillSourceEnum.Equip);
                skill.ParamId = suitSkill.Value;
                skill.Level = 1;
                skill.Actived = 1;
                passive.AddPassiveSkill(suitSkill.Key);
                changed = true;
            }

            if (changed && notice)
            {
                self.UpdateSkillSet();
            }
        }

        private static Dictionary<int, int> GetEquipSuitSkills(List<BagInfo> equipList)
        {
            Dictionary<int, int> suitPoints = new Dictionary<int, int>();
            for (int i = 0; i < equipList.Count; i++)
            {
                BagInfo item = equipList[i];
                if (!LDEquipCategory.Instance.Contain(item.ItemID))
                {
                    continue;
                }

                LDEquip equip = LDEquipCategory.Instance.Get(item.ItemID);
                if (equip.EquipSuitID == 0 || !LDEquip_SuitCategory.Instance.Contain(equip.EquipSuitID))
                {
                    continue;
                }

                LDEquip_Suit suit = LDEquip_SuitCategory.Instance.Get(equip.EquipSuitID);
                if (suit.Type_Id.Length > 0 && Array.IndexOf(suit.Type_Id, equip.Sub_Type) < 0)
                {
                    continue;
                }

                // 同一部位允许多件时逐件计分，不因 Type_Id 中重复列出部位而重复计分。
                suitPoints.TryGetValue(suit.Id, out int points);
                suitPoints[suit.Id] = points + equip.EquipSuitParam;
            }

            Dictionary<int, int> skills = new Dictionary<int, int>();
            foreach (KeyValuePair<int, int> points in suitPoints)
            {
                LDEquip_Suit suit = LDEquip_SuitCategory.Instance.Get(points.Key);
                string effects = GetEquipSuitEffects(suit, points.Value);
                if (string.IsNullOrEmpty(effects))
                {
                    continue;
                }

                foreach (string value in effects.Split('|'))
                {
                    if (!int.TryParse(value, out int effectId) || effectId <= 0 || !LDEquip_Suit_EffectCategory.Instance.Contain(effectId))
                    {
                        continue;
                    }

                    int skillId = LDEquip_Suit_EffectCategory.Instance.Get(effectId).Skill;
                    if (skillId > 0 && LDSkill_BattleCategory.Instance.Contain(skillId))
                    {
                        skills[skillId] = suit.Id;
                    }
                }
            }

            return skills;
        }

        private static string GetEquipSuitEffects(LDEquip_Suit suit, int points)
        {
            // 高档覆盖低档；Param_1..4 为 0 表示该档未配置，不能覆盖前面的有效档。
            if (suit.Param_4 > 0 && points >= suit.Param_4)
            {
                return suit.Effect_Id_4;
            }

            if (suit.Param_3 > 0 && points >= suit.Param_3)
            {
                return suit.Effect_Id_3;
            }

            if (suit.Param_2 > 0 && points >= suit.Param_2)
            {
                return suit.Effect_Id_2;
            }

            if (suit.Param_1 > 0 && points >= suit.Param_1)
            {
                return suit.Effect_Id_1;
            }

            return points >= suit.Param_0 ? suit.Effect_Id_0 : string.Empty;
        }
    }
}

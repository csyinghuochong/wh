using System.Collections.Generic;
using UnityEngine;

namespace ET
{
    public static class Skill_TreeEditorSystem
    {
        public static void OnInit(this Skill_TreeEditor self, SkillInfo skillcmd, Unit theUnitFrom)
        {
            self.SkillInfo = skillcmd;
            self.HurtIds.Clear();
            self.LdSkillConf = LDSkillCategory.Instance.Get(skillcmd.WeaponSkillID);
            self.TheUnitFrom = theUnitFrom;
            self.SkillState = SkillState.Running;
            self.SkillBeginTime = TimeHelper.ServerNow();
            self.treeLogicExecuted = false;
            self.GuideIntervalMs = 0;

            LDSkill ldSkill = self.LdSkillConf;
            double firstDelay = ldSkill.Time_1;
            if (firstDelay < 0)
            {
                firstDelay = 0;
            }

            self.SkillExcuteHurtTime = self.SkillBeginTime + (long)(1000 * firstDelay);

            // 技能结束时间统一：Begin + Time_3
            double endSec = ldSkill.Time_3 > 0 ? ldSkill.Time_3 : 1;
            self.SkillEndTime = self.SkillBeginTime + (long)(1000 * endSec);

            // 引导：Time_Interval 跳伤间隔（总窗口以 SkillEndTime/Time_3 为准；5s/1s → 0,1,2,3,4 共五次）
            if (ldSkill.Type == SkillTypeEnum.SkillTypeGuide_3 && ldSkill.Time_Interval > 0)
            {
                self.GuideIntervalMs = (long)(1000 * ldSkill.Time_Interval);
            }

            self.ActionPosition = new Vector3(skillcmd.PosX, skillcmd.PosY, skillcmd.PosZ);
            self.ICheckShape = self.CreateCheckShape(self.SkillInfo.TargetAngle);
        }

        public static void OnUpdate(this Skill_TreeEditor self)
        {
            long serverNow = TimeHelper.ServerNow();
            if (self.TheUnitFrom == null || self.TheUnitFrom.IsDisposed)
            {
                self.SetSkillState(SkillState.Finished);
                return;
            }

            if (serverNow >= self.SkillEndTime)
            {
                self.SetSkillState(SkillState.Finished);
                return;
            }

            if (serverNow < self.SkillExcuteHurtTime)
            {
                return;
            }

            // 到点执行一次（引导：下次间隔再把 treeLogicExecuted 打开）
            self.treeLogicExecuted = false;
            self.ExecuteSkillTreeOnce();
            self.treeLogicExecuted = true;

            if (self.GuideIntervalMs > 0)
            {
                self.SkillExcuteHurtTime += self.GuideIntervalMs;
                self.treeLogicExecuted = false;
            }
            else
            {
                // 非引导只跳一次，等 SkillEndTime 结束
                self.SkillExcuteHurtTime = self.SkillEndTime;
            }
        }

        static void ExecuteSkillTreeOnce(this Skill_TreeEditor self)
        {
            self.CollectSkillTargets();

            if (SkillEditorTreeRegistry.TryGetTree(self.LdSkillConf.Id, out SkillEditorSkillLogic logic))
            {
                SkillEditorTreeExecutor.Execute(self, logic);
            }
        }

        public static void OnAddHurtIds(this Skill_TreeEditor self, long unitid)
        {
            self.HurtIds.Add(unitid);
        }

        /// <summary>
        /// Collect targets for SkillEditor tree (v0 targets). Does not trigger hurt/buff - tree nodes handle that.
        /// </summary>
        public static void CollectSkillTargets(this Skill_TreeEditor self)
        {
            if (self.TheUnitFrom.IsDisposed)
            {
                return;
            }

            self.HurtIds.Clear();

            UnitComponent unitComponent = self.TheUnitFrom.GetParent<UnitComponent>();
            if (unitComponent == null)
            {
                return;
            }

            if (self.LdSkillConf.NeedTarget == (int)SkillNeedTargetType.NeedTarget_1)
            {
                Unit targetUnit = unitComponent.Get(self.SkillInfo.TargetID);
                if (targetUnit != null && self.SkillCanAttackUnit(targetUnit))
                {
                    self.TheUnitTarget = targetUnit;
                    self.OnAddHurtIds(targetUnit.Id);
                }

                return;
            }

            if (self.LdSkillConf.NeedTarget == (int)SkillNeedTargetType.NeedTargetOrForce_2)
            {
                Unit targetUnit = unitComponent.Get(self.SkillInfo.TargetID);
                if (targetUnit != null && self.SkillCanAttackUnit(targetUnit))
                {
                    self.TheUnitTarget = targetUnit;
                    self.OnAddHurtIds(targetUnit.Id);
                }
                else
                {
                    self.CollectSkillTargetsInShape(unitComponent);
                }

                return;
            }

            if (self.LdSkillConf.Range_Type == SkillRangeType.SkillRangeSingle_0)
            {
                Unit targetUnit = unitComponent.Get(self.SkillInfo.TargetID);
                if (targetUnit != null && LDSkillHelper.IsValidTarget(self.TheUnitFrom, targetUnit, self.LdSkillConf))
                {
                    self.TheUnitTarget = targetUnit;
                    self.OnAddHurtIds(targetUnit.Id);
                }

                return;
            }

            self.CollectSkillTargetsInShape(unitComponent);
        }

        private static void CollectSkillTargetsInShape(this Skill_TreeEditor self, UnitComponent unitComponent)
        {
            List<Unit> entities = unitComponent.GetAll();
            for (int i = entities.Count - 1; i >= 0; i--)
            {
                Unit uu = entities[i];

                if (!self.CheckShape(uu.Position))
                {
                    continue;
                }

                if (!self.SkillCanAttackUnit(uu))
                {
                    continue;
                }

                self.OnAddHurtIds(uu.Id);
            }
        }

        public static bool SkillCanAttackUnit(this Skill_TreeEditor self, Unit uu)
        {
            return LDSkillHelper.IsValidTarget(self.TheUnitFrom, uu, self.LdSkillConf);
        }

        public static bool CheckShape(this Skill_TreeEditor self, Vector3 t_positon)
        {
            return self.ICheckShape.Contains(t_positon);
        }

        /*范围类型
   0-单体
   1-圆形
   2-扇形
   3-基准点为一头的矩形
   4-基准点为中心的矩形*/
        public static Shape CreateCheckShape(this Skill_TreeEditor self, int targetAngle)
        {
            Shape ishape = null;

            switch (self.LdSkillConf.Range_Type)
            {
                case SkillRangeType.SkillRangeCicle_1:
                case SkillRangeType.SkillRangeSingle_0:
                    ishape = new Circle();
                    (ishape as Circle).s_position = self.ActionPosition;
                    (ishape as Circle).range = (float)(self.LdSkillConf.Range_Type_Param1);
                    break;
                case SkillRangeType.SkillRangeFan_2:
                    ishape = new Fan();
                    (ishape as Fan).s_position = self.ActionPosition;
                    (ishape as Fan).s_rotation = Quaternion.Euler(0, targetAngle, 0);
                    (ishape as Fan).skill_distance = (float)(self.LdSkillConf.Range_Type_Param1);
                    (ishape as Fan).skill_angle = (float)(self.LdSkillConf.Range_Type_Param2) * 0.5f;
                    break;
                case SkillRangeType.SkillRangeRectangle_3:
                    ishape = new Rectangle();
                    (ishape as Rectangle).s_position = self.ActionPosition;
                    (ishape as Rectangle).s_forward = (Quaternion.Euler(0, targetAngle, 0) * Vector3.forward).normalized;
                    (ishape as Rectangle).x_range = (float)(self.LdSkillConf.Range_Type_Param1) * 0.5f;
                    (ishape as Rectangle).z_range = (float)(self.LdSkillConf.Range_Type_Param2);
                    break;
                case SkillRangeType.SkillRangeRectangle_4:
                    ishape = new Rectangle_2();
                    (ishape as Rectangle_2).s_position = self.ActionPosition;
                    (ishape as Rectangle_2).s_forward = (Quaternion.Euler(0, targetAngle, 0) * Vector3.forward).normalized;
                    (ishape as Rectangle_2).x_range = (float)(self.LdSkillConf.Range_Type_Param1) * 0.5f;
                    (ishape as Rectangle_2).z_range = (float)(self.LdSkillConf.Range_Type_Param2);
                    break;
            }

            return ishape;
        }

        public static void SetSkillState(this Skill_TreeEditor self, SkillState skillState)
        {
            self.SkillState = skillState;
        }

        public static SkillState GetSkillState(this Skill_TreeEditor self)
        {
            return self.SkillState;
        }

        public static bool IsFinished(this Skill_TreeEditor self)
        {
            return self.SkillState == SkillState.Finished;
        }

        public static void OnFinished(this Skill_TreeEditor self)
        {
            self.ICheckShape = null;
            self.SkillInfo = null;
            self.GuideIntervalMs = 0;
            self.treeLogicExecuted = false;
        }
    }
}

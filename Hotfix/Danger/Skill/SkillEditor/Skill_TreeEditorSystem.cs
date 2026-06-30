using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ET
{
    public static  class Skill_TreeEditorSystem
    {


        public static void OnInit(this Skill_TreeEditor self,  SkillInfo skillcmd, Unit theUnitFrom)
        {
            self.SkillInfo = skillcmd;
            self.HurtIds.Clear();
            self.LdSkillConf = LDSkillCategory.Instance.Get(skillcmd.WeaponSkillID);
            self.TheUnitFrom = theUnitFrom;
            SkillSetComponentServer skillSetComponentServer = theUnitFrom.GetComponent<SkillSetComponentServer>();
            self.SkillState = SkillState.Running;
            self.SkillBeginTime = TimeHelper.ServerNow();
            self.DamgeChiXuLastTime = TimeHelper.ServerNow();
            self.SkillExcuteHurtTime = self.SkillBeginTime + (long)(1000 * self.LdSkillConf.Time_1);
            double totalTime = LDSkillHelper.GetSkillTotalTime(self.LdSkillConf);
            self.SkillEndTime = totalTime > 0
                ? self.SkillBeginTime + (long)(1000 * totalTime)
                : self.SkillBeginTime + 1000;
            self.TargetPosition = new Vector3(skillcmd.PosX, skillcmd.PosY, skillcmd.PosZ); //获取起始坐标
            self.ICheckShape = new List<Shape>() { self.CreateCheckShape(self.SkillInfo.TargetAngle) };
            self.NowPosition = self.TargetPosition;              //获取技能起始的坐标点
        }

        public static void OnUpdate(this Skill_TreeEditor self)
        {
            long serverNow = TimeHelper.ServerNow();
            if (serverNow < self.SkillExcuteHurtTime)
            {
                return;
            }

            if (self.TheUnitFrom.IsDisposed)
            {
                return;
            }

            if (!self.treeLogicExecuted)
            {
                self.treeLogicExecuted = true;
                self.CollectSkillTargets();

                if (self.LdSkillConf != null
                    && SkillEditorTreeRegistry.TryGetTree(self.LdSkillConf.Id, out SkillEditorSkillLogic logic))
                {
                    SkillEditorTreeExecutor.Execute(self, logic);
                }
            }

            if (serverNow > self.SkillEndTime)
            {
                self.SetSkillState(SkillState.Finished);
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
            for (int i = 0; i < self.ICheckShape.Count; i++)
            {
                if (self.ICheckShape[i].Contains(t_positon))
                {
                    return true;
                }
            }
            return false;
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
                case SkillRangeType.SkillRangeSingle_0:
                case SkillRangeType.SkillRangeCicle_1:
                    ishape = new Circle();
                    (ishape as Circle).s_position = self.TargetPosition;
                    (ishape as Circle).range = (float)(self.LdSkillConf.Range_Type_Param1);
                    break;
                case SkillRangeType.SkillRangeFan_2:
                    ishape = new Fan();
                    (ishape as Fan).s_position = self.TargetPosition;
                    (ishape as Fan).s_rotation = Quaternion.Euler(0, targetAngle, 0);
                    (ishape as Fan).skill_distance = (float)(self.LdSkillConf.Range_Type_Param1);
                    (ishape as Fan).skill_angle = (float)(self.LdSkillConf.Range_Type_Param2) * 0.5f;
                    break;
                case SkillRangeType.SkillRangeRectangle_3:
                    ishape = new Rectangle();
                    (ishape as Rectangle).s_position = self.TargetPosition;
                    (ishape as Rectangle).s_forward = (Quaternion.Euler(0, targetAngle, 0) * Vector3.forward).normalized;
                    (ishape as Rectangle).x_range = (float)(self.LdSkillConf.Range_Type_Param1) * 0.5f;
                    (ishape as Rectangle).z_range = (float)(self.LdSkillConf.Range_Type_Param2);
                    break;
                case SkillRangeType.SkillRangeRectangle_4:
                    ishape = new Rectangle_2();
                    (ishape as Rectangle_2).s_position = self.TargetPosition;
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


        //1：自身
        //2：队友
        //3：己方【同阵营】
        //4: 敌方
        //5：全部
        public static void SkillBuff(this Skill_TreeEditor self, int buffID, Unit uu)
        {
            if (uu == null || uu.IsDisposed)
            {
                return;
            }
            if (!LDSkillBuffCategory.Instance.Contain(buffID))
            {
                Log.Warning($"config==null： buffid{buffID}");
                return;
            }
            LDSkillBuff ldSkillBuff = LDSkillBuffCategory.Instance.Get(buffID);


            bool teshui = uu.Type == UnitType.JingLing && ldSkillBuff.TargetType == 1;
            if (!uu.IsCanBeAttack() && !teshui)
            {
                return;
            }

            if (ldSkillBuff.BuffBenefitType == 2
               && uu.GetComponent<StateComponent>().StateTypeGet(StateTypeEnum.WuDi))
            {
                //有无敌 
                return;
            }


            //检测类型
            //if (skillBuffConfig.BuffTargetType != 0 && skillBuffConfig.BuffTargetType != uu.Type)
            //{
            //    return;
            //}
            bool triggerbuff = false;
            int[] buffTargetTypes = ldSkillBuff.BuffTargetType;
            if (buffTargetTypes != null)
            {
                for (int i = 0; i < buffTargetTypes.Length; i++)
                {
                    if (buffTargetTypes[i] == 0 || buffTargetTypes[i] == uu.Type)
                    {
                        triggerbuff = true;
                    }
                }
            }
            if (!triggerbuff)
            {
                return;
            }
            //1：自身
            //2：队友
            //3：己方【同阵营】
            //4: 敌方
            //5：全部
            //6: 己方召唤兽，不包含宠物
            //7: 己方召唤兽，包含宠物
            bool canBuff = false;
            switch (ldSkillBuff.TargetType)
            {
                //对自己释放
                case 1:
                    canBuff = uu.Id == self.TheUnitFrom.Id;
                    if (uu.Type == UnitType.JingLing)
                    {
                        long masterid = uu.GetMasterId();
                        uu = uu.GetParent<UnitComponent>().Get(masterid);
                        if (uu == null || uu.IsDisposed)
                        {
                            return;
                        }
                    }
                    break;
                case 2:
                    PetComponentServer petComponentServer = self.TheUnitFrom.GetComponent<PetComponentServer>();
                    canBuff = self.TheUnitFrom.IsSameTeam(uu);
                    //if (canBuff && skillBuffConfig.Id == 92000032 && uu.Type == UnitType.Monster)
                    //{
                    //    Log.Console("怪物攻速！！！！");
                    //}
                    break;
                case 3:
                    canBuff = self.TheUnitFrom.GetBattleCamp() == uu.GetBattleCamp();
                    break;
                //敌方
                case 4:
                    canBuff = self.TheUnitFrom.IsCanAttackUnit(uu, true, false);
                    break;
                //全部
                case 5:
                    canBuff = true;
                    break;
                case 6:////6: 己方召唤兽，不包含宠物
                    canBuff = uu.Type == UnitType.Monster && uu.MasterId == self.TheUnitFrom.Id;
                    break;
                case 7://// 7: 己方召唤兽，包含宠物
                    canBuff = uu.MasterId == self.TheUnitFrom.Id;
                    break;
                default
                    :
                    break;
            }

            if (!canBuff)
            {
                return;
            }

            BuffData buffData = new BuffData();
            buffData.SkillId = self.LdSkillConf.Id;
            buffData.BuffId = ldSkillBuff.Id;
            uu.GetComponent<BuffManagerComponent>().BuffFactory(buffData, self.TheUnitFrom, self);
            //Log.Info("结束释放buff" + buffID);
        }



        public static void OnFinished(this Skill_TreeEditor self)
        {
            self.ICheckShape.Clear();
            self.SkillInfo = null;
        }
    }
}

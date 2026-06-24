using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ET
{
    public static class SkillHandlerSystem
    {

        public static void BaseOnInit(this SkillHandler self, SkillInfo skillcmd, Unit theUnitFrom)
        {
            self.SkillInfo = skillcmd;
            self.HurtIds.Clear();
            self.LastHurtTimes.Clear();
            self.LdSkillConf = LDSkillCategory.Instance.Get(skillcmd.WeaponSkillID);
            self.TheUnitFrom = theUnitFrom;
            SkillSetComponentServer skillSetComponentServer = theUnitFrom.GetComponent<SkillSetComponentServer>();
            self.TianfuProAdd = skillSetComponentServer != null ? skillSetComponentServer.GetSkillPropertyAdd(skillcmd.WeaponSkillID) : null;
            self.OnlyOnceBuffUnitID.Clear();
            self.IsExcuteHurt = false;
            self.SkillFirstHurtTime = 0;
            self.SkillTriggerInvelTime = 0;
            self.SkillTriggerLastTime = 0;
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
            self.SkillParValueHpUpAct.Clear();
            self.ActTargetAddPro = 0f;
            self.HurtAddPro = 0f;
            self.OnlyOncePassiveActionUnitID.Clear();
            self.OnlyHideBuffActionUnitID.Clear();
            
        }

        public static float GetTianfuProAdd(this SkillHandler self, int key)
        {
            if (self.TianfuProAdd == null)
                return 0f;

            List<float> valuelist = null;
            self.TianfuProAdd.TryGetValue(key, out valuelist);
            if (valuelist != null && valuelist.Count > 0)
            {
                return valuelist[0];
            }

            return 0;
        }

        public static List<float> GetTianfuProAdd_2(this SkillHandler self, int key)
        {
            if (self.TianfuProAdd == null)
                return null;

            List<float> valuelist = null;
            self.TianfuProAdd.TryGetValue(key, out valuelist);
            if (valuelist != null && valuelist.Count > 0)
            {
                return valuelist;
            }

            return null;
        }

        //初始化
        public static void InitSelfBuff(this SkillHandler self)
        {
            //触发初始化BUFF
            if (self.LdSkillConf == null)
            {
                Log.Error($"self.SkillConf == null {self.SkillInfo.WeaponSkillID}");
            }
            if (self.TheUnitFrom.IsDisposed)
            {
                Log.Debug($"self.TheUnitFrom.IsDisposed {self.TheUnitFrom.Id}");
                return;
            }

            /*
            if (self.LdSkillConf.InitBuffID != null && self.LdSkillConf.InitBuffID[0] != 0)
            {
                for (int y = 0; y < self.LdSkillConf.InitBuffID.Length; y++)
                {
                    self.SkillBuff(self.LdSkillConf.InitBuffID[y], self.TheUnitFrom);
                }
            }*/

            int[] initBuffIds = LDSkillHelper.GetInitBuffIds(self.LdSkillConf.Id);
            for (int i = 0; i < initBuffIds.Length; i++)
            {
                self.SkillBuff(initBuffIds[i], self.TheUnitFrom);
            }
            
            SkillSetComponentServer skillSetComponentServer = self.TheUnitFrom.GetComponent<SkillSetComponentServer>();
            List<int> buffInitAdd = skillSetComponentServer != null ? skillSetComponentServer.GetBuffInitIdAdd(self.LdSkillConf.Id) : null;
            if (buffInitAdd != null)
            {
                for (int i = 0; i < buffInitAdd.Count; i++)
                {
                    self.SkillBuff(buffInitAdd[i], self.TheUnitFrom);
                }
            }
        }

        //每帧检测
        public static void BaseOnUpdate(this SkillHandler self)
        {
            long serverNow = TimeHelper.ServerNow();
            //根据技能效果延迟触发伤害
            if (serverNow < self.SkillExcuteHurtTime)
            {
                return;
            }
            if (self.TheUnitFrom.IsDisposed)
            {
                return;
            }

            //只触发一次，需要多次触发的重写
            if (!self.IsExcuteHurt)
            {
                self.IsExcuteHurt = true;
                if (self.LdSkillConf.NeedTarget == (int)SkillNeedTargetType.NeedTarget_1)
                {
                    UnitComponent unitComponent = self.TheUnitFrom.GetParent<UnitComponent>();
                    if (unitComponent == null)
                    {
                        Log.Warning($"unitComponent == null:  {self.LdSkillConf.Id}");
                        return;
                    }
                    Unit targetUnit = unitComponent.Get(self.SkillInfo.TargetID);
                    if (targetUnit != null )
                    {
                        self.OnCollisionUnit(targetUnit);
                    }
                }
                else if ( self.LdSkillConf.NeedTarget == (int)SkillNeedTargetType.NeedTargetOrForce_2)
                {
                    UnitComponent unitComponent = self.TheUnitFrom.GetParent<UnitComponent>();
                    if (unitComponent == null)
                    {
                        Log.Warning($"unitComponent == null:  {self.LdSkillConf.Id}");
                        return;
                    }
                    Unit targetUnit = unitComponent.Get(self.SkillInfo.TargetID);
                    if (targetUnit != null )
                    {
                        self.OnCollisionUnit(targetUnit);
                    }
                    else
                    {
                        self.ExcuteSkillAction();
                    }
                }
                else if (self.LdSkillConf.Range_Type == SkillRangeType.SkillRangeSingle_0)
                {
                    UnitComponent unitComponent = self.TheUnitFrom.GetParent<UnitComponent>();
                    Unit targetUnit = unitComponent?.Get(self.SkillInfo.TargetID);
                    if (targetUnit != null && LDSkillHelper.IsValidTarget(self.TheUnitFrom, targetUnit, self.LdSkillConf))
                    {
                        self.OnCollisionUnit(targetUnit);
                    }
                    else
                    {
                        /*
                        float searchRange = self.LdSkillConf.Search_Range > 0
                            ? (float)self.LdSkillConf.Search_Range
                            : (float)self.LdSkillConf.Cast_Range;
                        if (searchRange > 0)
                        {
                            Unit selected = LDSkillHelper.SelectTarget(
                                self.TheUnitFrom,
                                LDSkillHelper.CollectCandidates(self.TheUnitFrom, self.LdSkillConf, searchRange),
                                self.LdSkillConf);
                            if (selected != null)
                            {
                                self.OnCollisionUnit(selected);
                            }
                        }*/
                    }
                }
                else
                {
                    self.ExcuteSkillAction();
                }
            }

            //根据技能存在时间设置其结束状态
            if (serverNow > self.SkillEndTime)
            {
                self.SetSkillState(SkillState.Finished);
            }
        }

        public static void ExcuteSkillAction(this SkillHandler self)
        {
            if (self.TheUnitFrom.IsDisposed)
            {
                Log.Debug($"self.TheUnitFrom.IsDisposed {self.TheUnitFrom.Id}");
                return;
            }

            //ListComponent<Unit> entities = ListComponent<Unit>.Create();
            //entities.AddRange(  self.TheUnitFrom.DomainScene().GetComponent<UnitComponent>().GetAll() );
            List<Unit> entities = self.TheUnitFrom.DomainScene().GetComponent<UnitComponent>().GetAll();
            for (int i = entities.Count - 1; i >= 0; i--)
            {
                Unit uu = entities[i];

                if (self.CheckMaxAttackNumber(uu.Id))
                {
                    continue;
                }
                if (self.IfHaveHurtId(uu.Id))
                {
                    continue;
                }

                //检测目标是否在技能范围
                if (!self.CheckShape(uu.Position))
                {
                    continue;
                }

                self.OnAddHurtIds(uu.Id);
                self.OnCollisionUnit(uu);
            }
        }

        public static bool IfHaveHurtId(this SkillHandler self, long unitid)
        {
            return self.HurtIds.Contains(unitid);
        }

        public static void OnAddHurtIds(this SkillHandler self, long unitid)
        {
            self.HurtIds.Add(unitid);
        }

        public static bool CheckMaxAttackNumber(this SkillHandler self, long unitid)
        {
            //MaxAttackNumber ==0 || -1不限制
            
            return false;    
        }

        public static void OnCollisionUnit(this SkillHandler self, Unit uu)
        {
            if (!self.SkillCanAttackUnit(uu))
            {
                return;
            }

            //触发伤害
            bool ishit = self.TriggeSkillHurt(uu, 0);

            //触发Buff
            if (ishit)
            {
                self.TriggerSkillBuff(uu);
            }
        }

        public static void CheckChiXuHurt(this SkillHandler self)
        {
            if (self.SkillTriggerInvelTime <= 0  || self.TheUnitFrom.IsDisposed)
            {
                return;
            }

            long servernow = TimeHelper.ServerNow();
            if (servernow - self.DamgeChiXuLastTime < self.SkillTriggerInvelTime)
            {
                return;
            }
            self.DamgeChiXuLastTime = servernow;
            List<Unit> entities = self.TheUnitFrom.GetParent<UnitComponent>().GetAll();
            for (int i = entities.Count - 1; i >= 0; i--)
            {
                Unit uu = entities[i];
                //检测目标是否在技能范围

                if (self.CheckMaxAttackNumber(uu.Id))
                {
                    continue;
                }
                if (!self.CheckShape(uu.Position))
                {
                    continue;
                }
                self.OnChiXuHurtCollision(uu);
            }
        }

        /// <summary>
        /// 特定技能没有附加伤害
        /// </summary>
        /// <param name="self"></param>
        /// <param name="uu"></param>
        /// <returns></returns>
        public static bool IsSpecifiedFight(this SkillHandler self, Unit uu)
        {
            return false;
        }

        public static bool SkillCanAttackUnit(this SkillHandler self, Unit uu)
        {
            return LDSkillHelper.IsValidTarget(self.TheUnitFrom, uu, self.LdSkillConf);
        }

        public static void OnChiXuHurtCollision(this SkillHandler self, Unit uu)
        {
            if (!self.SkillCanAttackUnit(uu))
            {
                return;
            }

            //触发伤害
            bool ishit = self.TriggeSkillHurt(uu, 1);

            //触发Buff
            if (ishit)
            {
                self.TriggerSkillBuff(uu);
            }
        }

        //目标附加Buff
        public static void TriggerSkillBuff(this SkillHandler self, Unit uu)
        {
          
            //触发Buff
            /*if (self.LdSkillConf.BuffID != null && self.LdSkillConf.BuffID[0] != 0)
            {
                for (int y = 0; y < self.LdSkillConf.BuffID.Length; y++)
                {
                    self.SkillBuff(self.LdSkillConf.BuffID[y], uu);
                }
            }
            if (self.LdSkillConf.OnlyOnceBuffID != null && !self.OnlyOnceBuffUnitID.Contains(uu.Id))
            {
                self.OnlyOnceBuffUnitID.Add(uu.Id);
                for (int y = 0; y < self.LdSkillConf.OnlyOnceBuffID.Length; y++)
                {
                    self.SkillBuff(self.LdSkillConf.OnlyOnceBuffID[y], uu);
                }
            }

            SkillSetComponent skillSetComponent = self.TheUnitFrom.GetComponent<SkillSetComponent>();
            List<int> buffInitAdd = skillSetComponent != null ? skillSetComponent.GetBuffIdAdd(self.LdSkillConf.Id) : null;
            if (buffInitAdd != null && buffInitAdd.Count > 0)
            {
                for (int k = 0; k < buffInitAdd.Count; k++)
                {
                    self.SkillBuff(buffInitAdd[k], uu);
                }
            }
            */
        }

        public static void SetSkillState(this SkillHandler self, SkillState state)
        {
            self.SkillState = state;
        }

        public static bool CheckShape(this SkillHandler self, Vector3 t_positon)
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

        public static bool TriggeSkillHurt(this SkillHandler self, Unit uu, int hurtMode = 0)
        {
            //技能伤害为0不执行
           
            if (!self.TheUnitFrom.IsCanAttackUnit(uu, false, false))
            {
                return true;
            }

         
            if (uu.GetComponent<BuffManagerComponent>().IsSkillImmune(self.LdSkillConf.Id))
            {
                return false;
            }

            Function_Fight.Fight(self.TheUnitFrom, uu, self, 0);
          
            //技能额外属性来自被动技能
            return true;
        }

        
        /*范围类型
   0-单体
   1-圆形
   2-扇形
   3-基准点为一头的矩形
   4-基准点为中心的矩形*/
        public static Shape CreateCheckShape(this SkillHandler self, int targetAngle)
        {
            Shape ishape = null;
            float addRange = self.GetTianfuProAdd((int)SkillAttributeEnum.AddDamageRange);

            switch (self.LdSkillConf.Range_Type)
            {
                case SkillRangeType.SkillRangeSingle_0:
                case SkillRangeType.SkillRangeCicle_1:
                    ishape = new Circle();
                    (ishape as Circle).s_position = self.TargetPosition;
                    (ishape as Circle).range = (float)(self.LdSkillConf.Range_Type_Param1) + addRange;
                    break;
                case SkillRangeType.SkillRangeFan_2:
                    ishape = new Fan();
                    (ishape as Fan).s_position = self.TargetPosition;
                    (ishape as Fan).s_rotation = Quaternion.Euler(0, targetAngle, 0);
                    (ishape as Fan).skill_distance = (float)(self.LdSkillConf.Range_Type_Param1) + addRange;
                    (ishape as Fan).skill_angle = (float)(self.LdSkillConf.Range_Type_Param2) * 0.5f;
                    break;
                case SkillRangeType.SkillRangeRectangle_3:
                    ishape = new Rectangle();
                    (ishape as Rectangle).s_position = self.TargetPosition;
                    (ishape as Rectangle).s_forward = (Quaternion.Euler(0, targetAngle, 0) * Vector3.forward).normalized;
                    (ishape as Rectangle).x_range = (float)(self.LdSkillConf.Range_Type_Param1) * 0.5f;
                    (ishape as Rectangle).z_range = (float)(self.LdSkillConf.Range_Type_Param2 + addRange);
                    break;
                case SkillRangeType.SkillRangeRectangle_4:
                    ishape = new Rectangle_2();
                    (ishape as Rectangle_2).s_position = self.TargetPosition;
                    (ishape as Rectangle_2).s_forward = (Quaternion.Euler(0, targetAngle, 0) * Vector3.forward).normalized;
                    (ishape as Rectangle_2).x_range = (float)(self.LdSkillConf.Range_Type_Param1) * 0.5f;
                    (ishape as Rectangle_2).z_range = (float)(self.LdSkillConf.Range_Type_Param2 + addRange);
                    break;
            }
            return ishape;
        }

        //目前只有冲锋技能用到。 
        public static void UpdateCheckPoint(this SkillHandler self, Vector3 vector3)
        {
            if (self.ICheckShape == null || self.ICheckShape.Count == 0)
            {
                //Log.Debug($"self.ICheckShape == null: {self.SkillConf.SkillName}");
                self.SetSkillState(SkillState.Finished);
                return;
            }

            switch (self.LdSkillConf.Range_Type)
            {
                default:
                    break;
            }
        }

        public static SkillState GetSkillState(this SkillHandler self)
        {
            return self.SkillState;
        }

        public static bool IsFinished(this SkillHandler self)
        {
            return self.SkillState == SkillState.Finished;
        }

        public static void Clear(this SkillHandler self)
        {
            self.ICheckShape.Clear();
            self.SkillInfo = null;
        }

        //1：自身
        //2：队友
        //3：己方【同阵营】
        //4: 敌方
        //5：全部
        public static void SkillBuff(this SkillHandler self, int buffID, Unit uu)
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
    }
}

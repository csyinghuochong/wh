using System;
using System.Collections.Generic;
using UnityEngine;

namespace ET
{

    [Timer(TimerType.SkillTimer)]
    public class SkillTimer : ATimer<SkillManagerComponent>
    {
        public override void Run(SkillManagerComponent self)
        {
            try
            {
                self.Check();
            }
            catch (Exception e)
            {
                Log.Error($"move timer error: {self.Id}\n{e}");
            }
        }
    }

    [ObjectSystem]
    public class SkillManagerComponentAwakeSystem : AwakeSystem<SkillManagerComponent>
    {
        public override void Awake(SkillManagerComponent self)
        {
            self.Skills.Clear();
            self.DelaySkillList.Clear();
            self.SkillCDs.Clear();
            self.FangunSkillId = 0;////int.Parse(LDGlobalValueCategory.Instance.Get(2).Value);
            self.SelfUnitComponent = self.DomainScene().GetComponent<UnitComponent>();
            self.SelfUnit = self.GetParent<Unit>();
        }
    }

    [ObjectSystem]
    public class SkillManagerComponentDestroySystem : DestroySystem<SkillManagerComponent>
    {
        public override void Destroy(SkillManagerComponent self)
        {
            self.OnDispose();
        }
    }

    /// <summary>
    /// 技能管理
    /// </summary>
    public static class SkillManagerComponentSystem
    {
        public static List<SkillInfo> GetRandomSkills(this SkillManagerComponent self, C2M_SkillCmd skillcmd, int weaponSkill)
        {
            Unit unit = self.GetParent<Unit>();
            List<SkillInfo> skillInfos = new List<SkillInfo>();
            SkillInfo skillInfo = new SkillInfo();
         
            if (self.SkillSecond.ContainsKey(skillcmd.SkillID))
            {
                //有对应的buff才能触发二段斩
                int buffId = (int)LDSkillCategory.Instance.BuffSecondSkill[self.SkillSecond[skillcmd.SkillID]].KeyId;

                List<Unit> allDefend = unit.GetParent<UnitComponent>().GetAll();
                for (int defend = 0; defend < allDefend.Count; defend++)
                {
                    BuffManagerComponent buffManagerComponent = allDefend[defend].GetComponent<BuffManagerComponent>();
                    if (buffManagerComponent == null || allDefend[defend].Id == unit.Id) //|| allDefend[defend].Id == request.TargetID 
                    {
                        continue;
                    }
                    int buffNum = buffManagerComponent.GetBuffSourceNumber(unit.Id, buffId);
                    if (buffNum <= 0)
                    {
                        continue;
                    }
                 
                    buffManagerComponent.BuffRemoveByUnit(0, buffId);
                    Vector3 direction = allDefend[defend].Position - unit.Position;
                    float ange = Mathf.Rad2Deg(Mathf.Atan2(direction.x, direction.z));
                    skillInfo = new SkillInfo();
                    skillInfo.TargetAngle = (int)Quaternion.QuaternionToEuler(unit.Rotation).y;
                    Vector3 targetPosition = allDefend[defend].Position;
                    skillInfo.WeaponSkillID = weaponSkill;
                    skillInfo.PosX = targetPosition.x;
                    skillInfo.PosY = targetPosition.y;
                    skillInfo.PosZ = targetPosition.z;
                    skillInfo.TargetID = skillcmd.TargetID;
                    skillInfo.TargetAngle = Mathf.FloorToInt(ange);
                    skillInfos.Add(skillInfo);
                }

                return skillInfos;
            }


            LDSkill ldSkill = LDSkillCategory.Instance.Get(weaponSkill);
            Unit target = unit.GetParent<UnitComponent>().Get(skillcmd.TargetID);

            switch (ldSkill.Type)
            {
                case (int)SkillNeedTargetType.NoTarget_0:
                    skillInfo = new SkillInfo();
                    skillInfo.WeaponSkillID = weaponSkill;
                    skillInfo.PosX = unit.Position.x;
                    skillInfo.PosY = unit.Position.y;
                    skillInfo.PosZ = unit.Position.z;
                    skillInfo.TargetID = skillcmd.TargetID;
                    skillInfo.TargetAngle = skillcmd.TargetAngle;
                    skillInfos.Add(skillInfo);
                    break;
                case (int)SkillNeedTargetType.NeedTarget_1:
                case (int)SkillNeedTargetType.NeedTargetOrForce_2:
                    skillInfo = new SkillInfo();
                    skillInfo.WeaponSkillID = weaponSkill;
                    skillInfo.PosX = target != null ? target.Position.x : unit.Position.x;
                    skillInfo.PosY = target != null ? target.Position.y : unit.Position.y;
                    skillInfo.PosZ = target != null ? target.Position.z : unit.Position.z;
                    skillInfo.TargetID = skillcmd.TargetID;
                    skillInfo.TargetAngle = skillcmd.TargetAngle;
                    skillInfos.Add(skillInfo);
                    break;
            default:
                    break;
            }
            //如果是闪现技能，并且目标点不能到达
            /*if (ldSkill.GameObjectName == "Skill_ShanXian_1" && skillInfos.Count > 0)
            {
                MapComponent mapComponent = self.DomainScene().GetComponent<MapComponent>();
                Vector3 vector3 = new Vector3(skillInfos[0].PosX, skillInfos[0].PosY, skillInfos[0].PosZ);
                Vector3 target3 = mapComponent.GetCanReachPath(unit, unit.Position, vector3);

                skillInfos[0].PosX = target3.x;
                skillInfos[0].PosY = target3.y;
                skillInfos[0].PosZ = target3.z;
            }
            //90010909
            if (ldSkill.GameObjectName == "Skill_ShanXian_2" && skillInfos.Count > 0 && target!=null)
            {
                Vector3 dir =  target.Rotation * Vector3.back;
                Vector3 vector3 = target.Position + dir * 1f;
                skillInfos[0].PosX = vector3.x;
                skillInfos[0].PosY = vector3.y;
                skillInfos[0].PosZ = vector3.z;
            }*/
            return skillInfos;
        }

        public static void OnDispose(this SkillManagerComponent self)
        {
            int skillcnt = self.Skills.Count;
            for (int i = skillcnt - 1; i >= 0; i--)
            {
                SkillHandler skillHandler = self.Skills[i];
                self.Skills.RemoveAt(i);
                ObjectPool.Instance.Recycle(skillHandler);
            }
            self.SkillCDs.Clear();
            TimerComponent.Instance?.Remove(ref self.Timer);
        }

        public static void OnFinish(this SkillManagerComponent self, bool notice)
        {
            Unit unit = self.GetParent<Unit>();
            int skillcnt = self.Skills.Count;
            for (int i = skillcnt - 1; i >= 0; i--)
            {
                SkillHandler skillHandler = self.Skills[i];
                self.Skills.RemoveAt(i);
                skillHandler.OnFinished();
                ObjectPool.Instance.Recycle(skillHandler);
            }
            self.DelaySkillList.Clear();
            TimerComponent.Instance?.Remove(ref self.Timer);
            if (notice && unit!=null && !unit.IsDisposed)
            {
                self.M2C_UnitFinishSkill.UnitId = unit.Id;
                MessageHelper.SendToClient(UnitHelper.GetUnitList(unit.DomainScene(), UnitType.Player), self.M2C_UnitFinishSkill);
            }
        }

        public static async ETTask OnContinueSkill(this SkillManagerComponent self, C2M_SkillCmd skillcmd)
        {
            long instanceid = self.InstanceId;
            await TimerComponent.Instance.WaitAsync(1000);
            if (instanceid != self.InstanceId)
            {
                return;
            }
            for (int i = 0; i < 1; i++)
            {
                self.OnUseSkill(skillcmd, false);
            }
        }

        public static void InterruptSkill(this SkillManagerComponent self, int skillId)
        {
            int skillcnt = self.Skills.Count;
            for (int i = skillcnt - 1; i >= 0; i--)
            {
                SkillHandler skillHandler = self.Skills[i];
                if (skillHandler.LdSkillConf.Id != skillId)
                {
                    continue;
                }
                skillHandler.SetSkillState(SkillState.Finished);
            }
            Unit unit = self.GetParent<Unit>();
            M2C_SkillInterruptResult m2C_SkillInterruptResult = new M2C_SkillInterruptResult() { UnitId = unit.Id, SkillId = skillId };
            MessageHelper.Broadcast(unit, m2C_SkillInterruptResult);
        }

        public static void InterruptSkill(this SkillManagerComponent self, string skillName)
        {
            Unit unit = self.GetParent<Unit>();

            int skillcnt = self.Skills.Count;
            for (int i = skillcnt - 1; i >= 0; i--)
            {
                SkillHandler skillHandler = self.Skills[i];
                self.InterruptSkill(skillHandler.LdSkillConf.Id);
            }
        }

        public static bool HaveSkillType(this SkillManagerComponent self, string skilltype)
        {
            int skillcnt = self.Skills.Count;
            for (int i = skillcnt - 1; i >= 0; i--)
            {

            }
            return false;
        }

        /// <summary>
        /// 不能重复释放冲锋技能
        /// </summary>
        /// <param name="self"></param>
        /// <param name="skillId"></param>
        /// <returns></returns>
        public static bool CheckChongJi(this SkillManagerComponent self, int skillId)
        {
            if (!LDSkillCategory.Instance.Contain(skillId))
            {
                return false;
            }
            LDSkill ldSkill = LDSkillCategory.Instance.Get(skillId);
            int skillcnt = self.Skills.Count;
            for (int i = skillcnt - 1; i >= 0; i--)
            {
    
            }
            return false;
        }

        /// <summary>
        /// 打断吟唱中， 吟唱前客户端处理
        /// </summary>
        /// <param name="self"></param>
        /// <param name="skillId"></param>
        public static void InterruptSing(this SkillManagerComponent self,int skillId,bool ifStop)
        {
            Unit unit =self.GetParent<Unit>();
            for (int i = self.Skills.Count - 1; i >= 0; i--)
            {
                SkillHandler skillHandler = self.Skills[i];
                if (skillHandler.LdSkillConf.Type != SkillTypeEnum.SkillTypeCast_2)
                {
                    continue;
                }
                
                if (skillHandler.LdSkillConf.Name.Equals(SkillHelp.Skill_XuanZhuan_Attack_2))
                {
                    ifStop = true;
                }

                //打断
                if (ifStop)
                {
                    skillHandler.SetSkillState(SkillState.Finished);
                    M2C_SkillInterruptResult m2C_SkillInterruptResult = new M2C_SkillInterruptResult() { UnitId = unit.Id, SkillId = skillHandler.LdSkillConf.Id };
                    //MessageHelper.Broadcast(unit, m2C_SkillInterruptResult);
                    self.BroadcastSkill(unit, m2C_SkillInterruptResult);
                }
            }
        }
        
        /// <summary>
        /// 服务器释放技能的点
        /// </summary>
        /// <param name="self"></param>
        /// <param name="skillcmd"></param>
        /// <param name="zhudong">被动触发</param>
        /// <returns></returns>
        public static M2C_SkillCmd OnUseSkill(this SkillManagerComponent self, C2M_SkillCmd skillcmd, bool zhudong = true, bool checkDead = true)
        {
            Unit unit = self.GetParent<Unit>();
            M2C_SkillCmd m2C_Skill = self.M2C_SkillCmd;
            m2C_Skill.Message = String.Empty;

            //判断技能是否可以释放
            int errorCode = self.IsCanUseSkill(skillcmd.SkillID, zhudong, checkDead);
            if (zhudong && errorCode != ErrorCode.ERR_Success)
            {
                m2C_Skill.Error = errorCode;
                return m2C_Skill;
            }

            SkillSetComponent skillSetComponent = unit.GetComponent<SkillSetComponent>();
            int weaponSkillid = unit.GetWeaponSkill(skillcmd.SkillID, skillSetComponent!=null ? skillSetComponent.SkillList : null );
            int tianfuSkill = skillSetComponent != null ? skillSetComponent.GetReplaceSkillId(weaponSkillid) : 0;
            if (tianfuSkill != 0)
            {
                weaponSkillid = tianfuSkill;
            }
            LDSkill weaponLdSkill = LDSkillCategory.Instance.Get(weaponSkillid);
            List<SkillInfo> skillList = self.GetRandomSkills(skillcmd, weaponSkillid);
            if (skillList == null ||  skillList.Count == 0)
            {
                m2C_Skill.Error = ErrorCode.ERR_UseSkillError;
                return m2C_Skill;
            }

            unit.Rotation = Quaternion.Euler(0, skillcmd.TargetAngle, 0);
            if ( !unit.GetComponent<MoveComponent>().IsArrived()) //weaponSkillConfig.IfStopMove == 0 &&
            {
                unit.Stop(weaponSkillid);
            }

            self.InterruptSing(skillcmd.SkillID, false);


            List<int> passiveTypeEnum_22 = null;
            SkillPassiveComponent skillPassiveComponent = unit.GetComponent<SkillPassiveComponent>();
            if (skillPassiveComponent == null)
            {
                Log.Debug($"skillPassiveComponent == null: {unit.Type}");
            }

            List<SkillHandler> handlerList = new List<SkillHandler>();  
            for (int i = 0; i < skillList.Count; i++)
            {
                skillList[i].SingValue = skillcmd.SingValue;
                SkillHandler skillAction = self.SkillFactory(skillList[i], unit);
                skillAction.OriginalSkill = skillcmd.SkillID;
                skillAction.PassiveTypeEnum_22 = passiveTypeEnum_22;
                skillList[i].SkillBeginTime = skillAction.SkillBeginTime;
                skillList[i].SkillEndTime = skillAction.SkillEndTime;
                handlerList.Add(skillAction);
            }

            //添加技能CD列表  给客户端发送消息 我创建了一个技能,客户端创建特效等相关功能
            SkillCDItem skillCd = self.AddSkillCD(skillcmd.ItemId, skillcmd.SkillID,  weaponLdSkill, zhudong);
            m2C_Skill.Error = ErrorCode.ERR_Success;
            m2C_Skill.CDEndTime = skillCd != null ? skillCd.CDEndTime : 0;
            m2C_Skill.PublicCDTime = self.SkillPublicCDTime;
            
            M2C_UnitUseSkill useSkill = MessageHelper.m2C_UnitUseSkill;
            useSkill.UnitId = unit.Id;
            useSkill.ItemId = skillcmd.ItemId;
            useSkill.SkillID = skillcmd.SkillID;
            useSkill.TargetAngle = skillcmd.TargetAngle;
            useSkill.SkillInfos = skillList;
            useSkill.CDEndTime = skillCd != null ? skillCd.CDEndTime : 0;
            useSkill.PublicCDTime = self.SkillPublicCDTime;
            self.BroadcastSkill(unit, useSkill);

            for (int i = 0; i < handlerList.Count; i++)
            {
                handlerList[i].OnExecute();
                self.Skills.Add(handlerList[i] );
            }
            if (zhudong)
            {
                
                //skillPassiveComponent?.OnTrigegerPassiveSkill(weaponLdSkill.SkillActType == 0 ? SkillPassiveTypeEnum.AckGaiLv_1 : SkillPassiveTypeEnum.SkillGaiLv_7, skillcmd.TargetID, skillcmd.SkillID);
                //skillPassiveComponent?.OnTrigegerPassiveSkill(weaponLdSkill.SkillRangeSize <= 4 ? SkillPassiveTypeEnum.AckDistance_9 : SkillPassiveTypeEnum.AckDistance_10, skillcmd.TargetID, skillcmd.SkillID);
                skillPassiveComponent?.OnTrigegerPassiveSkill(SkillPassiveTypeEnum.AllSkill_17, skillcmd.TargetID, skillcmd.SkillID);
                skillPassiveComponent?.OnTrigegerPassiveSkill(SkillPassiveTypeEnum.PassiveTypeEnum_22, skillcmd.TargetID, skillcmd.SkillID, passiveTypeEnum_22);
            }


            Unit unitTarget = unit.GetParent<UnitComponent>().Get(skillcmd.TargetID);
            if (weaponLdSkill.Type == SkillTypeEnum.SkillTypeInstant_1 &&  unitTarget !=null) 
            {
                unitTarget.GetComponent<AttackRecordComponent>().BeAttackId = unit.Id;  
            }
            if (weaponLdSkill.Type == SkillTypeEnum.SkillTypeInstant_1  && skillcmd.TargetID > 0)
            {
                unit.GetComponent<AttackRecordComponent>().AttackingId = skillcmd.TargetID;
            }

            float now_ZhuanZhuPro = unit.GetComponent<NumericComponent>().GetAsFloat(NumericType.Numeric_Error);
            if (zhudong && RandomHelper.RandFloat01() < now_ZhuanZhuPro
                && TimeHelper.ServerFrameTime() - self.LastLianJiTime >= 4000)
            {
                if (unit.Type == UnitType.Player)
                {
                    m2C_Skill.Message = "双重施法,触发法术连击!";
                }
                self.LastLianJiTime = TimeHelper.ServerFrameTime();
                self.OnContinueSkill(skillcmd).Coroutine();
            }

            self.TriggerAddSkill(skillcmd, weaponLdSkill.Id).Coroutine();
            self.AddSkillTimer();
            return m2C_Skill;
        }

        public static void AddSkillTimer(this SkillManagerComponent self)
        {
            if (self.Timer == 0)
            {
                TimerComponent.Instance.Remove(ref self.Timer);
                long repeatertime = 100;//// unit.Type == UnitType.Monster && LDMonsterCategory.Instance.NoSkillMonsterList.Contains(unit.ConfigId) ? 200 : 200;
                self.Timer = TimerComponent.Instance.NewRepeatedTimer(repeatertime, TimerType.SkillTimer, self);
            }
        }

        public static SkillCDItem AddSkillCD(this SkillManagerComponent self, int itemid, int skillid, LDSkill weapon, bool zhudong)
        {
            SkillCDItem skillCd = null;
            if (skillid == self.FangunSkillId)
            {
                skillCd = self.UpdateFangunSkillCD();
            }
            else
            {
                Unit unit = self.GetParent<Unit>();
                if (unit.Type == UnitType.Player)
                {
                    skillCd = self.UpdateNormalCD(skillid, weapon.Id, zhudong);
                }
                else
                {
                    skillCd = self.UpdateSkillCD(itemid, skillid, weapon.Id, zhudong);
                }
            }
            return skillCd;
        }

        public static async ETTask TriggerBuffSkill(this SkillManagerComponent self, KeyValuePairLong4 keyValuePair, long targetId, int buffNum)
        {
            for (int i = 0; i < buffNum; i++)
            {
                Unit unit = self.GetParent<Unit>();
                await TimerComponent.Instance.WaitAsync(keyValuePair.Value2);
                if (unit.IsDisposed)
                {
                    return;
                }
                LDSkill ldSkill = LDSkillCategory.Instance.Get((int)keyValuePair.Value);
                if (unit.GetComponent<StateComponent>().CanUseSkill(ldSkill, true) != ErrorCode.ERR_Success)
                {
                    return;
                }
                self.OnUseSkill(new C2M_SkillCmd() { SkillID = (int)keyValuePair.Value, TargetID = targetId }, false);
            }
        }

        public static  void TestTriggerBuffSkill(this SkillManagerComponent self, int skillId, int buffNum)
        {
            for (int i = 0; i < buffNum; i++)
            {
                Unit unit = self.GetParent<Unit>();
                if (unit.IsDisposed)
                {
                    return;
                }
                self.OnUseSkill(new C2M_SkillCmd() { SkillID = skillId, TargetID = 0 }, false);
            }
        }

        public static async ETTask TriggerAddSkill(this SkillManagerComponent self, C2M_SkillCmd c2M_SkillCmd, int skillId)
        {
            LDSkill ldSkill = LDSkillCategory.Instance.Get(skillId);
            await ETTask.CompletedTask;
        }

        private static int GetFirstComSkill(this SkillManagerComponent self, int skillId, int comskill)
        {

            return skillId;
        }

        public static SkillCDItem UpdateNormalCD(this SkillManagerComponent self, int skillId, int weaponSkill, bool zhudong)
        {
            Unit unit = self.GetParent<Unit>();
            //int equipType = UnitHelper.GetEquipType(unit);
            SkillCDItem skillcd = null;

            LDSkill ldSkill = LDSkillCategory.Instance.Get(skillId);
          
            self.SkillCDs.TryGetValue(skillId, out skillcd);
            if (skillcd == null)
            {
                skillcd = new SkillCDItem();
                self.SkillCDs.Add(skillId, skillcd);
            }
            skillcd.SkillID = skillId;

            NumericComponent numericComponent = unit.GetComponent<NumericComponent>();
            float attackSpped = 1f + numericComponent.GetAsFloat(NumericType.Numeric_Error);
            int EquipType = UnitHelper.GetEquipType(unit);
            List<int> normalskillCDs = EquipType == (int)ItemEquipType.Knife ? new List<int>() { 500, 1000, 1000 } : new List<int>() { 700, 700, 700 };
            for (int i = 0; i < normalskillCDs.Count; i++)
            {
                normalskillCDs[i] = (int)(normalskillCDs[i] / attackSpped);
            }

            int comindex = 0;
           
            comindex = Math.Clamp(comindex, 0, normalskillCDs.Count - 1);
            skillcd.CDEndTime = TimeHelper.ServerNow() + normalskillCDs[comindex] ;
            //Console.WriteLine($"add cd {skillId}   {skillcd.CDEndTime}");
            return null;
        }

        public static SkillCDItem UpdateSkillCD(this SkillManagerComponent self, int itemid, int skillId, int weaponSkill, bool zhudong)
        {
            Unit unit = self.GetParent<Unit>();
            SkillCDItem skillcd = null;
            LDSkill ldSkill = LDSkillCategory.Instance.Get(weaponSkill);
            NumericComponent numericComponent = unit.GetComponent<NumericComponent>();
            double skillcdTime = ldSkill.SkillCD;

            //减少的技能CD
            float reduceCD = 0f;
            List<float> reduceCDlist = null;
            SkillSetComponent skillSetComponent = unit.GetComponent<SkillSetComponent>();

            Dictionary<int, List<float>> keyValuePairs = skillSetComponent != null ? skillSetComponent.GetSkillPropertyAdd(weaponSkill) : null;
            if (keyValuePairs != null)
            {
                keyValuePairs.TryGetValue((int)SkillAttributeEnum.ReduceSkillCD, out reduceCDlist);
            }
            if (reduceCDlist != null && reduceCDlist.Count > 0)
            {
                reduceCD = reduceCDlist[0];
            }

            float nocdPro = numericComponent.GetAsFloat(NumericType.Numeric_Error);
            if (nocdPro > RandomHelper.RandFloat01())
            {
                skillcdTime = 1;  //1秒冷却CD
                skillcdTime -= reduceCD;
            }
            else
            {
                float now_cdpro= numericComponent.GetAsFloat(NumericType.Numeric_Error);
                //急速削减最多达到75%
                if (now_cdpro > 0.75f) {
                    now_cdpro = 0.75f;
                }
                skillcdTime -= reduceCD;
                skillcdTime *= ( 1f - now_cdpro);
            }

            //if (unit.Type != UnitType.Player && unit.MasterId != 0 && skillConfig.SkillActType == 0)
            if (unit.Type != UnitType.Player )
            {
                //float attackSpped = 1f - numericComponent.GetAsFloat(NumericType.Numeric_Error);
                //攻击速度调整
                float attackSpped = 1f / (1 +  numericComponent.GetAsFloat(NumericType.Numeric_Error));

                //最低是0.25秒触发一次
                if (attackSpped <= 0.25f)
                {
                    attackSpped = 0.25f;
                }
                skillcdTime = skillcdTime * attackSpped;
                skillcdTime -= reduceCD;
            }

            int cdRate = 1;
            if (itemid > 0 && unit.Type == UnitType.Player)
            {
                int sceneType = unit.DomainScene().GetComponent<MapComponent>().MapTypeEnum;
                cdRate = CommonHelper.GetSkillCdRate(sceneType); 
            }

            float nocdgailv = 0;
            List<float> noCdList = null;
            if (keyValuePairs != null)
            {
                keyValuePairs.TryGetValue((int)SkillAttributeEnum.NoSkillCD, out noCdList);
            }
            if (noCdList != null && noCdList.Count > 0)
            {
                nocdgailv = noCdList[0];
            }

            if (nocdgailv > 0f && nocdgailv >= RandomHelper.RandFloat01())
            {
                //无cD
                skillcdTime = -1;
            }

            self.SkillCDs.TryGetValue(skillId, out skillcd);
            if (skillcd == null)
            {
                skillcd = new SkillCDItem();
                self.SkillCDs.Add(skillId, skillcd);
            }
            if (zhudong)
            {
                skillcd.SkillID = skillId;
                skillcd.CDEndTime = TimeHelper.ServerNow() +  (int)(1000 *  skillcdTime* cdRate);
            }
            else
            {
                skillcd.SkillID = skillId;
                skillcd.CDPassive = TimeHelper.ServerNow() + (int)(1000 * skillcdTime);
            }

            if (zhudong && ldSkill.PublicCD  > 0f)
            {
                //添加技能公共CD
                self.SkillPublicCDTime = TimeHelper.ServerNow() + 500;  //公共1秒CD  
            }
            return skillcd;
        }

        //冲锋逻辑
        //1.连续释放3次技能,进入冷却状态
        //2.每次释放之间有5秒间隔时间,未超过间隔时间触发连击，如果超过时间重置为初始状态
        //初始状态 最开始的0次连击
        //冷却状态 10秒钟
        public static SkillCDItem UpdateFangunSkillCD(this SkillManagerComponent self)
        {
            SkillCDItem skillcd = null;
            long newTime = TimeHelper.ServerNow();
            if (newTime - self.FangunLastTime <= 5000)
            {
                self.FangunComboNumber++;
            }
            else
            {
                self.FangunComboNumber = 1;
            }

            if (self.FangunComboNumber >= 3)
            {
                int fangunskill = self.FangunSkillId;
                if (self.SkillCDs.ContainsKey(fangunskill))
                {
                    self.SkillCDs.Remove(fangunskill);  
                }
                self.FangunComboNumber = 0;
                skillcd = new SkillCDItem();
                skillcd.SkillID = fangunskill;
                skillcd.CDEndTime = newTime + 10000;
                self.SkillCDs.Add(fangunskill, skillcd);

                self.GetParent<Unit>().GetComponent<SkillPassiveComponent>().OnTrigegerPassiveSkill( SkillPassiveTypeEnum.FanGunCD_20, 0, 0 );
                //Unit unit = self.GetParent<Unit>();
                //BuffData buffData_2 = new BuffData();
                //buffData_2.BuffConfig = SkillBuffConfigCategory.Instance.Get(90106003);
                //buffData_2.BuffClassScript = buffData_2.BuffConfig.BuffScript;
                //unit.GetComponent<BuffManagerComponent>().BuffFactory(buffData_2, unit, null);
            }
            self.FangunLastTime = newTime;
            return skillcd;
        }

        //技能是否可以使用
        public static int IsCanUseSkill(this SkillManagerComponent self, int nowSkillID, bool zhudong = true, bool checkDead = true)
        {
            if (self.CheckChongJi(nowSkillID))
            { 
                return ErrorCode.ERR_SkillMoveTime;
            }
            if (!LDSkillCategory.Instance.Contain(nowSkillID))
            {
                return ErrorCode.ERR_ItemNotExist;
            }
            
            Unit unit = self.GetParent<Unit>();
            LDSkill ldSkill = LDSkillCategory.Instance.Get(nowSkillID);
            StateComponent stateComponent = unit.GetComponent<StateComponent>();

            //判断技能是否再冷却中
            long serverNow = TimeHelper.ServerNow();
            SkillCDItem skillCDItem = null;
            self.SkillCDs.TryGetValue(nowSkillID, out skillCDItem);
            //被动技能触发冷却CD
            if (!zhudong && skillCDItem != null && serverNow < skillCDItem.CDPassive)
            {
                return ErrorCode.ERR_UseSkillInCD4;
            }

            //主动技能触发冷却CD
            if (zhudong && skillCDItem != null && serverNow < skillCDItem.CDEndTime)
            {
                //Console.WriteLine($"check cd {nowSkillID}   {skillCDItem.CDEndTime}  {serverNow}   false");
                return ErrorCode.ERR_UseSkillInCD3;
            }

            //if (skillCDItem == null)
            //{
            //    Console.WriteLine($"check cd {nowSkillID}   skillCDItem == null");
            //}
            //else
            //{

            //    Console.WriteLine($"check cd {nowSkillID}   {skillCDItem.CDEndTime}  {serverNow}   true");

            //}

            if (unit.Type == UnitType.Monster)
            {
                if (stateComponent.IsRigidity())
                {
                    return ErrorCode.ERR_CanNotUseSkill_Rigidity;
                }
            }
            if (unit.Type != UnitType.Player)
            {
                //判断当前眩晕状态
                int errorCode = stateComponent.CanUseSkill(ldSkill, checkDead);
                if (ErrorCode.ERR_Success!= errorCode)
                {
                    return errorCode;
                }
                //判定是否再公共冷却时间
                if (serverNow < self.SkillPublicCDTime)
                {
                    return ErrorCode.ERR_UseSkillInCD2;
                }
            }
            return ErrorCode.ERR_Success;
        }
        
        public static SkillHandler SkillFactory(this SkillManagerComponent self, SkillInfo skillcmd, Unit from)
        {
            LDSkill ldSkill = LDSkillCategory.Instance.Get(skillcmd.WeaponSkillID);
            SkillHandler skillHandler = null;

            skillHandler = (SkillHandler)ObjectPool.Instance.Fetch(SkillDispatcherComponent.Instance.SkillTypes[ldSkill.GetSkillScript()]);
            skillHandler.OnInit(skillcmd, from);
            return skillHandler;
        }

        public static List<SkillInfo> GetMessageSkill(this SkillManagerComponent self)
        {
            List<SkillInfo> skillinfos = new List<SkillInfo>();
            for (int i = 0; i < self.Skills.Count; i++)
            {
                skillinfos.Add(self.Skills[i].SkillInfo);
            }
            return skillinfos;
        }

        /// <summary>
        /// 队友进入地图
        /// </summary>
        /// <param name="self"></param>
        public static void TriggerTeamBuff(this SkillManagerComponent self)
        {
            int skillcnt = self.Skills.Count;
            for (int i = skillcnt - 1; i >= 0; i--)
            {
                SkillHandler skillHandler = self.Skills[i];
                if (skillHandler == null)
                {
                    continue;
                }
                //self.Skills[i].OnUpdate();
               
            }
        }

        /// <summary>
        /// 清除所有技能和Cd
        /// </summary>
        /// <param name="self"></param>
        public static void ClearSkillAndCd(this SkillManagerComponent self)
        {
            self.SkillCDs.Clear();
            self.OnDispose();
        }

        /// <summary>
        /// 二段斩第一技能结束
        /// </summary>
        /// <param name="self"></param>
        /// <param name="skillConfig"></param>
        public static void CheckSkillSecond(this SkillManagerComponent self, SkillHandler skillHandler, long hurtId) 
        {
            KeyValuePairLong4 keyValuePairLong = null;
            //有二段斩则记录到self.SkillSecond， 无则返回
            LDSkillCategory.Instance.BuffSecondSkill.TryGetValue(skillHandler.LdSkillConf.Id, out keyValuePairLong);
            if (keyValuePairLong == null)
            {
                return;
            }

            UnitComponent unitComponent = self.DomainScene().GetComponent<UnitComponent>();
            Unit target = unitComponent.Get(hurtId);
            if (target == null)
            {
                return;
            }

            if (target.GetComponent<NumericComponent>().GetAsInt(NumericType.Now_Dead) == 1)
            {
                return;
            }

            int cdskillid = skillHandler.OriginalSkill > 0 ? skillHandler.OriginalSkill : skillHandler.LdSkillConf.Id;

            ///攻击到目标则暂时清除CD
            SkillCDItem skillCDItem = null;
            self.SkillCDs.TryGetValue(cdskillid, out skillCDItem);
            if (skillCDItem != null && skillCDItem.CDEndTime != 0)
            {
                skillCDItem.CDEndTime = 0;
                //有伤害才同步 打断CD. 只同步一次
                M2C_SkillSecondResult request = new M2C_SkillSecondResult() { UnitId = self.Id, SkillId = cdskillid, HurtIds = new List<long> { hurtId } };
                MessageHelper.SendToClient(self.GetParent<Unit>(), request);
            }
           

            self.SkillSecond[(int)(keyValuePairLong.Value2)] = skillHandler.LdSkillConf.Id;//702-302
        }

        public static void CheckEndSkill(this SkillManagerComponent self, int endSkillId)
        {
            if (endSkillId == 0)
            {
                return;
            }
            if (!LDSkillCategory.Instance.Contain(endSkillId))
            {
                return;
            }

            Unit unit = self.GetParent<Unit>();
            C2M_SkillCmd cmd = new C2M_SkillCmd();
            cmd.SkillID = endSkillId;
            cmd.TargetID = unit.Id;
            cmd.TargetAngle = (int)Quaternion.QuaternionToEuler(unit.Rotation).y;
            cmd.TargetDistance = 0f;
            self.OnUseSkill(cmd, false);
        }

        public static void Check(this SkillManagerComponent self)
        {
            int skillcnt = self.Skills.Count;
            for (int i = skillcnt - 1; i >= 0; i-- )
            {
                if (self.IsDisposed)
                {
                    return;
                }

                if (i >= self.Skills.Count)
                {
                    Unit unit = self.GetParent<Unit>();
                    Log.Warning($"SkillManagerComponentError11:  {unit.Type} {unit.ConfigId} {unit.InstanceId}");
                    break;
                }

                self.Skills[i].OnUpdate();

                if ( i >= self.Skills.Count)
                {
                    Unit unit = self.GetParent<Unit>();
                    Log.Warning($"SkillManagerComponentError22:  {unit.Type} {unit.ConfigId} {unit.InstanceId}");
                    break;
                }
                
            }

            int dalaycnt = self.DelaySkillList.Count;
            for (int i = dalaycnt - 1; i >= 0; i--)
            {
                SkillInfo skillInfo = self.DelaySkillList[i];
                
                Unit target = self.SelfUnitComponent.Get(skillInfo.TargetID);
                if (target != null && !target.IsDisposed)
                {
                    skillInfo.PosX = target.Position.x;
                    skillInfo.PosY = target.Position.y;
                    skillInfo.PosZ = target.Position.z;
                }
                if (TimeHelper.ServerNow() < skillInfo.SkillBeginTime)
                {
                    continue;
                }
                
                //Unit from = self.GetParent<Unit>();
                SkillHandler skillAction = self.SkillFactory(skillInfo, self.SelfUnit);
                skillAction.OriginalSkill = skillInfo.SkillID;
                skillInfo.SkillBeginTime = skillAction.SkillBeginTime;
                skillInfo.SkillEndTime = skillAction.SkillEndTime;
                self.Skills.Add(skillAction);

                //M2C_UnitUseSkill useSkill = new M2C_UnitUseSkill();
                //{
                //    UnitId = self.SelfUnit.Id,
                //    SkillID = 0,
                //    TargetAngle = 0,
                //    SkillInfos = new List<SkillInfo>() { skillInfo }
                //};
                M2C_UnitUseSkill useSkill = MessageHelper.m2C_UnitUseSkill;
                useSkill.UnitId = self.SelfUnit.Id;
                useSkill.SkillID = 0;
                useSkill.TargetAngle = 0;
                useSkill.SkillInfos = new List<SkillInfo>() { skillInfo };
                useSkill.PublicCDTime = 0;
                useSkill.CDEndTime = 0;
                //MessageHelper.Broadcast(self.SelfUnit, useSkill);
                self.BroadcastSkill(self.SelfUnit, useSkill);
                self.DelaySkillList.RemoveAt(i);
            }

            //循环检查冷却CD的技能
            /*
            if (self.SkillCDs.Count >= 1)
            {
                long nowTime = TimeHelper.ServerNow();
                List<int> removeList = new List<int>();
                foreach (SkillCDItem skillcd in self.SkillCDs.Values)
                {
                    if (nowTime >= skillcd.CDEndTime
                     && nowTime >= skillcd.CDPassive)
                    {
                        removeList.Add(skillcd.SkillID);
                    }
                }

                //移除技能cd结束的技能
                foreach (int removeID in removeList)
                {
                    self.SkillCDs.Remove(removeID);
                }
            }
            */
            
            if (self.Skills.Count == 0 && self.DelaySkillList.Count == 0)
            {
                TimerComponent.Instance.Remove( ref self.Timer );
            }
        }

        //技能广播
        public static void BroadcastSkill(this SkillManagerComponent self, Unit unit, IActorMessage message)
        {
            //主城不广播技能
            if (unit.SceneType != MapTypeEnum.MainCityScene)
            {
                MessageHelper.Broadcast(unit, message);
            }
        }
    }
}

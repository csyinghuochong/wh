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
            List<SkillInfo> skillInfos = self.TempSkillInfos;
            skillInfos.Clear();
         
            LDSkill_Battle ldSkill = LDSkill_BattleCategory.Instance.Get(weaponSkill);
            Unit target = unit.GetParent<UnitComponent>().Get(skillcmd.TargetID);
            Vector3 targetPosition = LDSkillHelper.ResolveSkillTargetPosition(unit, ldSkill, skillcmd, target);

            // SkillInfo 需随技能生命周期存活，不能跨次施法复用同一实例

            //计算技能作用的坐标
            //Vector3 sourcePoint = unit.Position;
            //Quaternion rotation = Quaternion.Euler(0, skillcmd.TargetAngle, 0);
            //Vector3 targetPoint = sourcePoint + rotation * Vector3.forward * skillcmd.TargetDistance;


            SkillInfo skillInfo = new SkillInfo();
            skillInfo.SkillID = skillcmd.SkillID;
            skillInfo.WeaponSkillID = weaponSkill;
            skillInfo.PosX = targetPosition.x;
            skillInfo.PosY = targetPosition.y;
            skillInfo.PosZ = targetPosition.z;
            skillInfo.TargetID = skillcmd.TargetID;
            skillInfo.TargetAngle = skillcmd.TargetAngle;
            skillInfos.Add(skillInfo);

            return skillInfos;
        }

        public static void OnDispose(this SkillManagerComponent self)
        {
            int skillcnt = self.Skills.Count;
            for (int i = skillcnt - 1; i >= 0; i--)
            {
                Skill_TreeEditor skillHandler = self.Skills[i];
                self.Skills.RemoveAt(i);
                skillHandler.OnFinished();
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
                Skill_TreeEditor skillHandler = self.Skills[i];
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

        public static void InterruptSkill(this SkillManagerComponent self, int skillId)
        {
            int skillcnt = self.Skills.Count;
            for (int i = skillcnt - 1; i >= 0; i--)
            {
                Skill_TreeEditor skillHandler = self.Skills[i];
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
                Skill_TreeEditor skillHandler = self.Skills[i];
                self.InterruptSkill(skillHandler.LdSkillConf.Id);
            }
        }

        /// <summary>
        /// 新技能 Interrupt_1=1 时，打断当前所有 Interrupt_2=1 的进行中技能。
        /// </summary>
        public static void InterruptSkillsByNewCast(this SkillManagerComponent self, LDSkill_Battle newSkill)
        {
            if (!LDSkillHelper.CanInterruptOtherSkills(newSkill))
            {
                return;
            }

            Unit unit = self.GetParent<Unit>();
            for (int i = self.Skills.Count - 1; i >= 0; i--)
            {
                Skill_TreeEditor skillHandler = self.Skills[i];
                LDSkill_Battle running = skillHandler.LdSkillConf;
                if (!LDSkillHelper.CanBeInterrupted(running))
                {
                    continue;
                }

                skillHandler.SetSkillState(SkillState.Finished);
                M2C_SkillInterruptResult msg = new M2C_SkillInterruptResult() { UnitId = unit.Id, SkillId = running.Id };
                MessageHelper.Broadcast(unit, msg);
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
            if (!LDSkill_BattleCategory.Instance.Contain(skillId))
            {
                return false;
            }
            LDSkill_Battle ldSkill = LDSkill_BattleCategory.Instance.Get(skillId);
            int skillcnt = self.Skills.Count;
            for (int i = skillcnt - 1; i >= 0; i--)
            {
    
            }
            return false;
        }

        /// <summary>
        /// 打断吟唱中， 吟唱前客户端处理。
        /// ifStop=true 时仅打断 Interrupt_2=1 的施法类技能（移动/受控等）。
        /// </summary>
        public static void InterruptSing(this SkillManagerComponent self,int skillId,bool ifStop)
        {
            if (!ifStop)
            {
                return;
            }

            Unit unit =self.GetParent<Unit>();
            for (int i = self.Skills.Count - 1; i >= 0; i--)
            {
                Skill_TreeEditor skillHandler = self.Skills[i];
                LDSkill_Battle running = skillHandler.LdSkillConf;
                if (running == null || running.Type != SkillTypeEnum.SkillTypeCast_2)
                {
                    continue;
                }

                if (!LDSkillHelper.CanBeInterrupted(running))
                {
                    continue;
                }

                if (skillId != 0 && running.Id != skillId)
                {
                    continue;
                }

                skillHandler.SetSkillState(SkillState.Finished);
                M2C_SkillInterruptResult m2C_SkillInterruptResult = new M2C_SkillInterruptResult() { UnitId = unit.Id, SkillId = running.Id };
                self.BroadcastSkill(unit, m2C_SkillInterruptResult);
            }
        }
        
        public static void ApplyConsume(Unit unit, LDSkill_Battle ldSkill)
        {
           
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

            LDSkillHelper.PrepareSkillCmd(unit, skillcmd);

            int errorCode = self.IsCanUseSkill(skillcmd, zhudong, checkDead);
            if (zhudong && errorCode != ErrorCode.ERR_Success)
            {
                m2C_Skill.Error = errorCode;
                return m2C_Skill;
            }

            LDSkill_Battle baseLdSkill = LDSkill_BattleCategory.Instance.Get(skillcmd.SkillID);
            //ApplyConsume(unit, baseLdSkill);

            SkillSetComponentServer skillSetComponentServer = unit.GetComponent<SkillSetComponentServer>();
            int weaponSkillid = unit.GetWeaponSkill(skillcmd.SkillID, skillSetComponentServer!=null ? skillSetComponentServer.SkillList : null );
            int tianfuSkill = skillSetComponentServer != null ? skillSetComponentServer.GetReplaceSkillId(weaponSkillid) : 0;
            if (tianfuSkill != 0)
            {
                weaponSkillid = tianfuSkill;
            }
            LDSkill_Battle weaponLdSkill = LDSkill_BattleCategory.Instance.Get(weaponSkillid);
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

            // Interrupt_1 / Interrupt_2：新技能可打断其它可被打断的进行中技能
            self.InterruptSkillsByNewCast(weaponLdSkill);

            SkillPassiveComponent skillPassiveComponent = unit.GetComponent<SkillPassiveComponent>();
            if (skillPassiveComponent == null)
            {
                Log.Debug($"skillPassiveComponent == null: {unit.Type}");
            }

            List<Skill_TreeEditor> handlerList = new List<Skill_TreeEditor>();  
            for (int i = 0; i < skillList.Count; i++)
            {
                skillList[i].SingValue = skillcmd.SingValue;
                Skill_TreeEditor skillAction = self.SkillFactory(skillList[i], unit);
                skillList[i].SkillBeginTime = skillAction.SkillBeginTime;
                skillList[i].SkillEndTime = skillAction.SkillEndTime;
                handlerList.Add(skillAction);
            }

            //添加技能CD列表  给客户端发送消息 我创建了一个技能,客户端创建特效等相关功能
            SkillCDItem skillCd = self.AddSkillCD(skillcmd.ItemId, skillcmd.SkillID,  weaponLdSkill, zhudong);
            m2C_Skill.Error = ErrorCode.ERR_Success;
            m2C_Skill.CDEndTime = skillCd != null ? skillCd.CDEndTime : 0;
            // ItemId>0：下发道具公共CD；否则下发技能公共CD（复用 PublicCDTime 字段）
            m2C_Skill.PublicCDTime = skillcmd.ItemId > 0 ? self.ItemPublicCDTime : self.SkillPublicCDTime;
            
            M2C_UnitUseSkill useSkill = MessageHelper.m2C_UnitUseSkill;
            useSkill.UnitId = unit.Id;
            useSkill.ItemId = skillcmd.ItemId;
            useSkill.SkillID = skillcmd.SkillID;
            useSkill.TargetAngle = skillcmd.TargetAngle;
            useSkill.SkillInfos = skillList;
            useSkill.CDEndTime = skillCd != null ? skillCd.CDEndTime : 0;
            useSkill.PublicCDTime = skillcmd.ItemId > 0 ? self.ItemPublicCDTime : self.SkillPublicCDTime;
            self.BroadcastSkill(unit, useSkill);

            for (int i = 0; i < handlerList.Count; i++)
            {
                handlerList[i].OnUpdate();
                self.Skills.Add(handlerList[i] );
            }
            if (zhudong)
            {
            }

            Unit unitTarget = unit.GetParent<UnitComponent>().Get(skillcmd.TargetID);
            if (weaponLdSkill.Type == SkillTypeEnum.SkillTypeInstant_1)
            {
                if (unitTarget != null)
                {
                    unitTarget.GetComponent<AttackRecordComponent>().BeAttackId = unit.Id;
                }
                if (skillcmd.TargetID > 0)
                {
                    unit.GetComponent<AttackRecordComponent>().AttackingId = skillcmd.TargetID;
                }
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

        public static SkillCDItem AddSkillCD(this SkillManagerComponent self, int itemid, int skillid, LDSkill_Battle weapon, bool zhudong)
        {
            self.ApplyPublicCD(itemid, weapon, zhudong);
            SkillCDItem skillCd = self.UpdateSkillCD(itemid, skillid, weapon.Id, zhudong);
            return skillCd;
        }

        /// <summary>
        /// 公共CD：按表 PublicCD（秒）写入结束时间。
        /// 道具技能 → ItemPublicCDTime；普通技能 → SkillPublicCDTime。
        /// </summary>
        public static void ApplyPublicCD(this SkillManagerComponent self, int itemId, LDSkill_Battle ldSkill, bool zhudong)
        {
            if (!zhudong || ldSkill == null || ldSkill.PublicCD <= 0f)
            {
                return;
            }

            long endTime = TimeHelper.ServerNow() + (long)(ldSkill.PublicCD * 1000d);
            if (itemId > 0)
            {
                self.ItemPublicCDTime = endTime;
            }
            else
            {
                self.SkillPublicCDTime = endTime;
            }
        }

        public static async ETTask TriggerBuffSkill(this SkillManagerComponent self, LongLongPair4 keyValuePair, long targetId, int buffNum)
        {
            for (int i = 0; i < buffNum; i++)
            {
                Unit unit = self.GetParent<Unit>();
                await TimerComponent.Instance.WaitAsync(keyValuePair.Value2);
                if (unit.IsDisposed)
                {
                    return;
                }
                LDSkill_Battle ldSkill = LDSkill_BattleCategory.Instance.Get((int)keyValuePair.Value);
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
            LDSkill_Battle ldSkill = LDSkill_BattleCategory.Instance.Get(skillId);
            await ETTask.CompletedTask;
        }

        public static SkillCDItem UpdateSkillCD(this SkillManagerComponent self, int itemid, int skillId, int weaponSkill, bool zhudong)
        {
            Unit unit = self.GetParent<Unit>();
            SkillCDItem skillcd = null;
            LDSkill_Battle ldSkill = LDSkill_BattleCategory.Instance.Get(weaponSkill);

            // 表未配个人CD：不上个人CD（公共CD由 AddSkillCD 统一 ApplyPublicCD）
            if (ldSkill.SkillCD <= 0)
            {
                return null;
            }

            NumericComponent numericComponent = unit.GetComponent<NumericComponent>();
            double skillcdTime = ldSkill.SkillCD;

            //减少技能CD
            float now_cdpro = numericComponent.GetAsFloat(NumericType.SKILL_CD_192);
            skillcdTime *= (1f - now_cdpro);

            self.SkillCDs.TryGetValue(skillId, out skillcd);
            if (skillcd == null)
            {
                skillcd = new SkillCDItem();
                self.SkillCDs.Add(skillId, skillcd);
            }
            if (zhudong)
            {
                skillcd.SkillID = skillId;
                skillcd.CDEndTime = TimeHelper.ServerNow() +  (int)(1000 *  skillcdTime);
            }
            else
            {
                skillcd.SkillID = skillId;
                skillcd.CDPassive = TimeHelper.ServerNow() + (int)(1000 * skillcdTime);
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
        public static int IsCanUseSkill(this SkillManagerComponent self, C2M_SkillCmd skillcmd, bool zhudong = true, bool checkDead = true)
        {
            return self.IsCanUseSkill(skillcmd.SkillID, skillcmd.TargetID, zhudong, checkDead);
        }

        public static int IsCanUseSkill(this SkillManagerComponent self, int nowSkillID, bool zhudong = true, bool checkDead = true)
        {
            return self.IsCanUseSkill(nowSkillID, 0, zhudong, checkDead);
        }

        public static int IsCanUseSkill(this SkillManagerComponent self, int nowSkillID, long targetId, bool zhudong = true, bool checkDead = true)
        {
            if (self.CheckChongJi(nowSkillID))
            { 
                return ErrorCode.ERR_SkillMoveTime;
            }
            if (!LDSkill_BattleCategory.Instance.Contain(nowSkillID))
            {
                return ErrorCode.ERR_ItemNotExist;
            }
            
            Unit unit = self.GetParent<Unit>();
            nowSkillID = LDSkillHelper.GetBuffReplacedSkillId(unit, nowSkillID);
            LDSkill_Battle ldSkill = LDSkill_BattleCategory.Instance.Get(nowSkillID);

            if (LDSkillHelper.IsPassiveSkill(ldSkill))
            {
                return ErrorCode.ERR_CanNotUseSkill_1;
            }

           /* int castError = LDSkillHelper.CheckCastCondition(unit, ldSkill, targetId);
            if (castError != ErrorCode.ERR_Success)
            {
                return castError;
            }*/

            StateComponent stateComponent = unit.GetComponent<StateComponent>();

            //判断技能是否再冷却中
            long serverNow = TimeHelper.ServerNow();
            SkillCDItem skillCDItem = null;
            self.SkillCDs.TryGetValue(nowSkillID, out skillCDItem);
            //被动技能触发冷却CD
            if (!zhudong && skillCDItem != null && serverNow < skillCDItem.CDPassive)
            {
                return LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Battle_Skill_CD);
            }

            //主动技能触发冷却CD
            if (zhudong && skillCDItem != null && serverNow < skillCDItem.CDEndTime)
            {
                //Console.WriteLine($"check cd {nowSkillID}   {skillCDItem.CDEndTime}  {serverNow}   false");
                return LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Battle_Skill_CD);
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
                    return LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Battle_Skill_CD);
                }
            }
            return ErrorCode.ERR_Success;
        }
        
        public static Skill_TreeEditor SkillFactory(this SkillManagerComponent self, SkillInfo skillcmd, Unit from)
        {
            LDSkill_Battle ldSkill = LDSkill_BattleCategory.Instance.Get(skillcmd.WeaponSkillID);
            Skill_TreeEditor skillHandler = (Skill_TreeEditor)ObjectPool.Instance.Fetch(typeof(Skill_TreeEditor));
            skillHandler.OnInit(skillcmd, from);
            return skillHandler;
        }

        public static List<SkillInfo> GetMessageSkill(this SkillManagerComponent self)
        {
            List<SkillInfo> skillinfos = self.MessageSkillInfos;
            skillinfos.Clear();
            for (int i = 0; i < self.Skills.Count; i++)
            {
                skillinfos.Add(self.Skills[i].SkillInfo);
            }
            return skillinfos;
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
        

        public static void CheckEndSkill(this SkillManagerComponent self, int endSkillId)
        {
            if (endSkillId == 0)
            {
                return;
            }
            if (!LDSkill_BattleCategory.Instance.Contain(endSkillId))
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

                Skill_TreeEditor skill_TreeEditor = self.Skills[i];
                skill_TreeEditor.OnUpdate();

                if (skill_TreeEditor.GetSkillState() == SkillState.Finished)
                {
                    // 与 OnFinish 一致：先收尾再还池，避免漏 Recycle 导致对象池泄漏
                    skill_TreeEditor.OnFinished();
                    self.Skills.RemoveAt(i);
                    ObjectPool.Instance.Recycle(skill_TreeEditor);
                }
            }


            //循环检查冷却CD的技能
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
            
            if (self.Skills.Count == 0 && self.DelaySkillList.Count == 0)
            {
                TimerComponent.Instance.Remove( ref self.Timer );
            }
        }

        //技能广播：施法者必收（BeSeePlayers 不含自己）；主城只给自己，其他场景再广播给视野内玩家
        public static void BroadcastSkill(this SkillManagerComponent self, Unit unit, IActorMessage message)
        {
            if (unit.Type == UnitType.Player)
            {
                MessageHelper.SendToClient(unit, message);
            }

            if (unit.SceneType != MapTypeEnum.MainCityScene)
            {
                MessageHelper.Broadcast(unit, message);
            }
        }
    }
}

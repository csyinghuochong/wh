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
            // 一次性定时器先移除；检查完成后按仍在运行的技能安排下一次唤醒。
            TimerComponent.Instance.Remove(ref self.Timer);
            try
            {
                self.Check();
            }
            catch (Exception e)
            {
                Log.Error($"move timer error: {self.Id}\n{e}");
            }
            finally
            {
                if (!self.IsDisposed)
                {
                    self.AddSkillTimer();
                }
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
            self.GroupCDEndTimes.Clear();
         
            self.SelfUnitComponent = self.DomainScene().GetComponent<UnitComponent>();
            self.SelfUnit = self.GetParent<Unit>();
             self.SingSkillCmd = new C2M_SkillCmd();
            self.SingSkillCmd.SkillID = 0;
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
    public static partial class SkillManagerComponentSystem
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
            self.GroupCDEndTimes.Clear();
            self.ClearMonsterSing();
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
        /// 移动等：清吟唱读条，并打断 Time_1 前、Interrupt_2=1 的技能。
        /// </summary>
        public static void InterruptSing(this SkillManagerComponent self,int skillId,bool ifStop)
        {
            if (!ifStop)
            {
                return;
            }

            self.InterruptPendingSing(skillId);
            self.InterruptSkillsBeforeTime1(skillId);
        }

        /// <summary>
        /// Time_1 生效前可被打断：Interrupt_2=1。吟唱未释放的不在这里（走 InterruptPendingSing）。
        /// </summary>
        public static void InterruptSkillsBeforeTime1(this SkillManagerComponent self, int skillId = 0)
        {
            Unit unit = self.GetParent<Unit>();
            for (int i = self.Skills.Count - 1; i >= 0; i--)
            {
                Skill_TreeEditor skillHandler = self.Skills[i];
                LDSkill_Battle running = skillHandler.LdSkillConf;
                if (running == null || !LDSkillHelper.CanBeInterrupted(running))
                {
                    continue;
                }

                if (skillId != 0 && running.Id != skillId)
                {
                    continue;
                }

                if (!skillHandler.IsBeforeTime1())
                {
                    continue;
                }

                skillHandler.SetSkillState(SkillState.Finished);
                self.BroadcastSkill(unit, new M2C_SkillInterruptResult() { UnitId = unit.Id, SkillId = running.Id });
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
                m2C_Skill.Error = LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_Operation_Fail);
                return m2C_Skill;
            }

            unit.Rotation = Quaternion.Euler(0, skillcmd.TargetAngle, 0);
            if ( !unit.GetComponent<MoveComponent>().IsArrived())
            {
                unit.Stop(weaponSkillid);
            }

            // Interrupt_1 / Interrupt_2：新技能可打断其它可被打断的进行中技能
            self.InterruptSkillsByNewCast(weaponLdSkill);

            SkillPassiveComponent skillPassiveComponent = unit.GetComponent<SkillPassiveComponent>();
            if (skillPassiveComponent == null && Log.IsDebugEnabled)
            {
                Log.Debug($"skillPassiveComponent == null: {unit.Type}");
            }

            List<Skill_TreeEditor> handlerList = self.TempSkillHandlers;
            handlerList.Clear();  
            for (int i = 0; i < skillList.Count; i++)
            {
                skillList[i].SingValue = skillcmd.SingValue;
                Skill_TreeEditor skillAction = self.SkillFactory(skillList[i], unit);
                skillList[i].SkillBeginTime = skillAction.SkillBeginTime;
                skillList[i].SkillEndTime = skillAction.SkillEndTime;
                handlerList.Add(skillAction);
            }

            if (zhudong)
            {
                // 主动施法成功后按 Time_2（秒）设置服务端硬直，拒绝硬直内再次主动施法；客户端据广播同步动作锁。
                unit.GetComponent<StateComponent>().SetRigidityEndTime(
                        TimeHelper.ClientNow() + (long)(weaponLdSkill.Time_2 * 1000), weaponLdSkill.Id);
            }

            //添加技能CD列表  给客户端发送消息 我创建了一个技能,客户端创建特效等相关功能
            SkillCDItem skillCd = self.AddSkillCD(skillcmd.SkillID, weaponLdSkill, zhudong);
            m2C_Skill.Error = ErrorCode.ERR_Success;
            m2C_Skill.CDEndTime = skillCd != null ? skillCd.CDEndTime : 0;
            // PublicCDTime 复用为本次技能所在组的组 CD 结束时间
            long groupCdEnd = self.GetGroupCDEndTime(weaponLdSkill.Group);
            m2C_Skill.PublicCDTime = groupCdEnd;
            
            M2C_UnitUseSkill useSkill = MessageHelper.m2C_UnitUseSkill;
            useSkill.UnitId = unit.Id;
            useSkill.ItemId = skillcmd.ItemId;
            useSkill.SkillID = skillcmd.SkillID;
            useSkill.TargetAngle = skillcmd.TargetAngle;
            useSkill.SkillInfos = skillList;
            useSkill.CDEndTime = skillCd != null ? skillCd.CDEndTime : 0;
            useSkill.PublicCDTime = groupCdEnd;
            if (SkillDispatcherComponent.EnableSkillTimingLog && weaponLdSkill.Id == 1001 && (Options.Instance.Console == 1 || Log.IsDebugEnabled))
            {
                Log.Console($"[技能计时][服务端][释放技能并广播] 时间={TimeHelper.DateTimeNow():HH:mm:ss.fff} skill={weaponLdSkill.Id} caster={unit.Id} target={skillcmd.TargetID}");
            }

            self.BroadcastSkill(unit, useSkill);

            // 敌对指向技能成功释放即记录攻击者，未扣血时宠物也能响应主人受击。
            Unit target = self.SelfUnitComponent.Get(skillcmd.TargetID);
            if (weaponLdSkill.Target_Type == LDSkillTargetType.Enemy_4
                && target != null && !target.IsDisposed && unit.IsCanAttackUnit(target))
            {
                target.GetComponent<AttackRecordComponent>().BeAttackId = unit.Id;
            }

            for (int i = 0; i < handlerList.Count; i++)
            {
                handlerList[i].OnUpdate();
                self.Skills.Add(handlerList[i] );
            }
            if (zhudong)
            {
            }

            if (weaponLdSkill.Type == SkillTypeEnum.SkillTypeInstant_1)
            {
                if (skillcmd.TargetID > 0)
                {
                    unit.GetComponent<AttackRecordComponent>().AttackingId = skillcmd.TargetID;
                }
            }


            self.TriggerAddSkill(skillcmd, weaponLdSkill.Id).Coroutine();
            self.AddSkillTimer();
            return m2C_Skill;
        }

        /// <summary>保留最多 100ms 的常规检查，在伤害或结束时刻提前唤醒；新施法重新计算，空闲时停表。</summary>
        public static void AddSkillTimer(this SkillManagerComponent self)
        {
            TimerComponent.Instance.Remove(ref self.Timer);
            if (self.IsDisposed || (self.Skills.Count == 0 && self.DelaySkillList.Count == 0))
            {
                return;
            }

            long now = TimeHelper.ServerNow();
            long nextTime = now + 100;
            foreach (Skill_TreeEditor skill in self.Skills)
            {
                long dueTime = skill.SkillEndTime;
                // 引导最后一跳之后只等待结束，避免用已过期的跳伤时刻反复唤醒。
                long guideEndTime = skill.SkillBeginTime + (long)(1000 * Math.Max(0, skill.LdSkillConf.Time_1))
                        + (long)(1000 * skill.LdSkillConf.Skill_Time);
                if (skill.GuideIntervalMs == 0 || skill.SkillExcuteHurtTime <= guideEndTime)
                {
                    dueTime = Math.Min(dueTime, skill.SkillExcuteHurtTime);
                }

                nextTime = Math.Min(nextTime, Math.Max(now + 1, dueTime));
            }

            self.Timer = TimerComponent.Instance.NewOnceTimer(nextTime, TimerType.SkillTimer, self);
        }

        public static SkillCDItem AddSkillCD(this SkillManagerComponent self, int skillid, LDSkill_Battle weapon, bool zhudong)
        {
            self.ApplyGroupCD(weapon, zhudong);
            SkillCDItem skillCd = self.UpdateSkillCD(0, skillid, weapon.Id, zhudong);
            return skillCd;
        }

        /// <summary>
        /// 组 CD：按初始化记下的 GroupCD（秒）写入该组结束时间。组内技能全部进入这段 CD。
        /// SkillCD 仍只作用于当前技能。
        /// </summary>
        public static void ApplyGroupCD(this SkillManagerComponent self, LDSkill_Battle ldSkill, bool zhudong)
        {
            if (!zhudong || ldSkill == null || ldSkill.Group <= 0)
            {
                return;
            }

            double groupCd = ldSkill.GroupCD;
            if (groupCd <= 0d)
            {
                return;
            }

            self.GroupCDEndTimes[ldSkill.Group] = TimeHelper.ServerNow() + (long)(groupCd * 1000d);
        }

        public static long GetGroupCDEndTime(this SkillManagerComponent self, int groupId)
        {
            if (groupId <= 0)
            {
                return 0;
            }

            self.GroupCDEndTimes.TryGetValue(groupId, out long endTime);
            return endTime;
        }

        public static bool IsInGroupCD(this SkillManagerComponent self, int groupId, long now)
        {
            return self.GetGroupCDEndTime(groupId) > now;
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

            // 表未配个人CD：不上个人CD（组 CD 由 AddSkillCD 统一 ApplyGroupCD）
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
                return LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_Operation_Fail);
            }
            if (!LDSkill_BattleCategory.Instance.Contain(nowSkillID))
            {
                return LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Item_NotExist);
            }
            
            Unit unit = self.GetParent<Unit>();
            nowSkillID = LDSkillHelper.GetBuffReplacedSkillId(unit, nowSkillID);
            LDSkill_Battle ldSkill = LDSkill_BattleCategory.Instance.Get(nowSkillID);

            if (LDSkillHelper.IsPassiveSkill(ldSkill))
            {
                return LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_Operation_Fail);
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

            // 组 CD：该组内所有技能共享。组 1 对应原技能公共 CD，组 2 对应药水 CD，其它组同样。
            if (self.IsInGroupCD(ldSkill.Group, serverNow))
            {
                return LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Battle_Skill_CD);
            }

            int stateError = stateComponent.CanUseSkill(ldSkill, checkDead);
            if (stateError != ErrorCode.ERR_Success)
            {
                return stateError;
            }
            return ErrorCode.ERR_Success;
        }
        
        public static Skill_TreeEditor SkillFactory(this SkillManagerComponent self, SkillInfo skillcmd, Unit from)
        {
            Skill_TreeEditor skillHandler = (Skill_TreeEditor)ObjectPool.Instance.Fetch(typeof(Skill_TreeEditor));
            skillHandler.OnInit(skillcmd, from);
            return skillHandler;
        }

        /// <summary>
        /// Buff / 技能体关联技能：立刻跑技能树，不走 OnUseSkill（无 CD / 动作 / Time_1）。
        /// TheUnitFrom=caster，TheUnitTarget=target。技能体 caster 无 SkillManager 时用 MasterId 主人。
        /// BUFF 回调的脚本 caster 使用持有者 target，buff.parent 仍为原施法者。
        /// 子技能必须有 Skill_Battle 行，没有直接 return。
        /// </summary>
        public static void ExecuteLinkedSkill(int skillId, Unit caster, Unit target, bool isBuffSkill = false)
        {
            if (skillId <= 0)
            {
                return;
            }

            if (!LDSkill_BattleCategory.Instance.Contain(skillId))
            {
                return;
            }

            Unit from = caster != null && !caster.IsDisposed ? caster : target;
            if (from == null || from.IsDisposed)
            {
                return;
            }

            if (target == null || target.IsDisposed)
            {
                target = from;
            }

            SkillManagerComponent skillManager = from.GetComponent<SkillManagerComponent>()
                                                ?? target.GetComponent<SkillManagerComponent>()
                                                ?? ResolveMasterSkillManager(from);
            if (skillManager == null)
            {
                return;
            }

            Vector3 center = target.Position;
            SkillInfo skillInfo = new SkillInfo
            {
                SkillID = skillId,
                WeaponSkillID = skillId,
                TargetID = target.Id,
                PosX = center.x,
                PosY = center.y,
                PosZ = center.z,
                TargetAngle = AngleHelper.GetQuaternionAngle(from.Rotation),
            };

            Skill_TreeEditor handler = skillManager.SkillFactory(skillInfo, from);
            handler.TheUnitTarget = target;
            handler.ActionPosition = center;
            handler.HurtIds.Clear();
            handler.OnAddHurtIds(target.Id);

            if (SkillEditorTreeRegistry.TryGetTree(skillId, out SkillEditorSkillLogic logic))
            {
                SkillEditorTreeExecutor.Execute(handler, logic, isBuffSkill ? target : from);
            }

            handler.SetSkillState(SkillState.Finished);
            handler.OnFinished();
            ObjectPool.Instance.Recycle(handler);
        }

        private static SkillManagerComponent ResolveMasterSkillManager(Unit from)
        {
            long masterId = from.MasterId;
            if (masterId <= 0)
            {
                return null;
            }

            Unit master = from.GetParent<UnitComponent>()?.Get(masterId);
            return master != null && !master.IsDisposed ? master.GetComponent<SkillManagerComponent>() : null;
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

        /// <summary>按时间门槛推进技能，执行异常的技能结束回收、不重试；再清理到期 CD，由 SkillTimer 安排下次唤醒。</summary>
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
                    continue;
                }

                Skill_TreeEditor skill_TreeEditor = self.Skills[i];
                SkillInfo skillInfo = skill_TreeEditor.SkillInfo;
                try
                {
                    skill_TreeEditor.OnUpdate();
                }
                catch (Exception e)
                {
                    // 技能可能已产生部分效果；结束本次实例，避免按过期时间重试并重复扣血。
                    if (skill_TreeEditor.SkillInfo == skillInfo)
                    {
                        skill_TreeEditor.SetSkillState(SkillState.Finished);
                    }
                    Log.Error($"skill update error: unit={self.Id} skill={skillInfo.WeaponSkillID}\n{e}");
                }

                if (self.IsDisposed)
                {
                    return;
                }

                // 技能执行可能触发死亡清场或被动施法；已移除、回收或复用的实例不能再次回收。
                if (i >= self.Skills.Count || self.Skills[i] != skill_TreeEditor || skill_TreeEditor.SkillInfo != skillInfo)
                {
                    continue;
                }

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
                List<int> removeList = self.TempRemoveCdIds;
                removeList.Clear();
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

        // 技能广播：施法者必收；AOI 已包含自己时不重复单发，主城只给自己。
        public static void BroadcastSkill(this SkillManagerComponent self, Unit unit, IActorMessage message)
        {
            if (unit.Type == UnitType.Player
                && (unit.SceneType == MapTypeEnum.MainCityScene || !unit.GetBeSeePlayers().ContainsKey(unit.Id)))
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

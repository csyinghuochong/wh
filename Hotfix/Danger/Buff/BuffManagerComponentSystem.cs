using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ET
{

    [Timer(TimerType.BuffTimer)]
    public class BuffTimer : ATimer<BuffManagerComponent>
    {
        public override void Run(BuffManagerComponent self)
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
    public class BuffManagerComponentAwakeSystem : AwakeSystem<BuffManagerComponent>
    {
        public override void Awake(BuffManagerComponent self)
        {
            self.m_Buffs.Clear();
            self.SceneType = self.DomainScene().GetComponent<MapComponent>().MapTypeEnum;
        }
    }

    [ObjectSystem]
    public class BuffManagerComponentDestroySystem : DestroySystem<BuffManagerComponent>
    {
        public override void Destroy(BuffManagerComponent self)
        {
            self.OnDispose();
        }
    }

    public static class BuffManagerComponentSystem
    {

        public static void OnDispose(this BuffManagerComponent self)
        {
            int buffcnt = self.m_Buffs.Count;
            for (int i = buffcnt - 1; i >= 0; i--)
            {
                Buff buffHandler = self.m_Buffs[i];
                ObjectPool.Instance.Recycle(buffHandler);
                self.m_Buffs.RemoveAt(i);
            }
            TimerComponent.Instance?.Remove(ref self.Timer);
        }

        public static void OnDeadRemoveBuffBy(this BuffManagerComponent self, long unitId)
        {
            self.MarkFinishedFromUnit(unitId);
            self.Check();
        }

        public static void OnRetreatRemoveBuff(this BuffManagerComponent self, long unitId)
        {
            self.MarkFinishedFromUnit(unitId);
            self.Check();
        }

        private static void MarkFinishedFromUnit(this BuffManagerComponent self, long unitId)
        {
            for (int i = 0; i < self.m_Buffs.Count; i++)
            {
                Buff buff = self.m_Buffs[i];
                if (buff.BuffState == BuffState.Finished)
                {
                    continue;
                }

                if (GetFromUnitId(buff) == unitId)
                {
                    buff.BuffState = BuffState.Finished;
                }
            }
        }

        private static void MarkInterrupt(Buff buff)
        {
            buff.BuffState = BuffState.Finished;
            buff.IsInterrupt = true;
        }

        private static long GetFromUnitId(Buff buff)
        {
            if (buff.TheUnitFrom != null)
            {
                return buff.TheUnitFrom.Id;
            }

            return buff.BuffData.UnitIdFrom;
        }

        /// <summary>先移出列表再 OnFinished。到期或 IsInterrupt 才放 Skill_Remove。</summary>
        private static void FinishAndRemoveBuff(this BuffManagerComponent self, Buff buffHandler, int index, bool notice)
        {
            int buffId = buffHandler.BuffData.BuffId;
            if (notice)
            {
                M2C_UnitBuffRemove m2C_UnitBuffUpdate = self.m2C_UnitBuffRemove;
                m2C_UnitBuffUpdate.UnitIdBelongTo = self.GetParent<Unit>().Id;
                m2C_UnitBuffUpdate.BuffID = buffHandler.MBuff.Id;
                MessageHelper.BroadcastBuff(self.GetParent<Unit>(), m2C_UnitBuffUpdate, buffHandler.MBuff, self.SceneType);
            }

            buffHandler.BuffState = BuffState.Finished;
            self.m_Buffs.RemoveAt(index);
            buffHandler.OnFinished(buffHandler.IsInterrupt);
            ObjectPool.Instance.Recycle(buffHandler);
            self.AddBuffRecord(0, buffId);
        }

        /// <summary>Id_Mutex / Group_Mutex。同 Id 由 Type_Add 处理。</summary>
        private static bool NeedMutexRemove(LDSkill_Battle_Buff incoming, LDSkill_Battle_Buff existing)
        {
            if (incoming.Id == existing.Id)
            {
                return false;
            }

            if (ContainsId(incoming.Id_Mutex, existing.Id) || ContainsId(existing.Id_Mutex, incoming.Id))
            {
                return true;
            }

            return StateTypeEnum.IdsOverlap(incoming.Group_Mutex, existing.Group)
                   || StateTypeEnum.IdsOverlap(existing.Group_Mutex, incoming.Group);
        }

        private static bool ContainsId(int[] ids, int id)
        {
            if (ids == null || id <= 0)
            {
                return false;
            }

            for (int i = 0; i < ids.Length; i++)
            {
                if (ids[i] == id)
                {
                    return true;
                }
            }

            return false;
        }

        public static void OnRemoveBuffItem(this BuffManagerComponent self, Buff buffHandler)
        {
            M2C_UnitBuffRemove m2C_UnitBuffUpdate = self.m2C_UnitBuffRemove;
            m2C_UnitBuffUpdate.UnitIdBelongTo = self.GetParent<Unit>().Id;
            m2C_UnitBuffUpdate.BuffID = buffHandler.MBuff.Id;
            MessageHelper.BroadcastBuff(self.GetParent<Unit>(), m2C_UnitBuffUpdate, buffHandler.MBuff, self.SceneType);

            buffHandler.BuffState = BuffState.Finished;
            buffHandler.OnFinished(true);
            int buffId = buffHandler.BuffData.BuffId;
            ObjectPool.Instance.Recycle(buffHandler);
            self.AddBuffRecord(0, buffId);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="self"></param>
        /// <param name="operate">1新增 0移除</param>
        /// <param name="buffHandler"></param>
        public static void AddBuffRecord(this BuffManagerComponent self, int operate, int buffId)
        {
            ////先屏蔽掉
            //if (buffId <= 0)
            //{
            //    return;
            //}
        }

        public static void OnRevive(this BuffManagerComponent self)
        {
            MapComponent mapComponent = self.DomainScene().GetComponent<MapComponent>();
        }

        // Remove_Dead 暂不处理，死亡先全清
        public static void OnDead(this BuffManagerComponent self, Unit attack)
        {
            for (int i = 0; i < self.m_Buffs.Count; i++)
            {
                self.m_Buffs[i].BuffState = BuffState.Finished;
            }
            self.Check();
        }

        public static void BuffRemoveByUnit(this BuffManagerComponent self, long unitId, int buffId)
        {
            for (int i = 0; i < self.m_Buffs.Count; i++)
            {
                Buff buffHandler = self.m_Buffs[i];
                if (buffHandler.BuffState == BuffState.Finished || buffHandler.MBuff.Id != buffId)
                {
                    continue;
                }

                if (unitId == 0 || GetFromUnitId(buffHandler) == unitId)
                {
                    MarkInterrupt(buffHandler);
                }
            }
            self.Check();
        }


        public static void AddTimer(this BuffManagerComponent self)
        {
            if (self.Timer == 0)
            {
                self.Timer = TimerComponent.Instance.NewRepeatedTimer(100, TimerType.BuffTimer, self);
            }
        }


        public static bool BuffFactory(this BuffManagerComponent self, BuffData buffData, Unit from, Skill_TreeEditor skillHandler, bool notice = true, bool ignoreImmune = false, long intervalMs = 0)
        {
            if (buffData.BuffId <= 0)
            {
                Log.Error("buffData.BuffId <= 0");
                return false;
            }

            Unit unit = self.GetParent<Unit>();
            LDSkill_Battle_Buff ldSkillBuff = LDSkill_Battle_BuffCategory.Instance.Get(buffData.BuffId);
            if (!ignoreImmune && self.IsControlImmune(ldSkillBuff))
            {
                if (Log.IsDebugEnabled)
                {
                    Log.Debug($"IsControlImmune unit={unit.Id} buff={ldSkillBuff.Id}");
                }
                return false;
            }

            List<Buff> nowAllBuffList = self.m_Buffs;
            for (int i = nowAllBuffList.Count - 1; i >= 0; i--)
            {
                Buff oldBuff = nowAllBuffList[i];
                if (oldBuff.BuffState == BuffState.Finished)
                {
                    continue;
                }

                if (NeedMutexRemove(ldSkillBuff, oldBuff.MBuff))
                {
                    MarkInterrupt(oldBuff);
                }
            }

            int addType = ldSkillBuff.Type_Add;
            int addParam = ldSkillBuff.Type_Add_Param;
            if (addType == BuffAddType.Replace_0)
            {
                self.MarkSameIdBuffs(ldSkillBuff.Id, 0);
            }
            else if ((addType == BuffAddType.Stack_1 || addType == BuffAddType.Coexist_3) && addParam > 0)
            {
                self.MarkSameIdBuffs(ldSkillBuff.Id, addParam - 1);
            }

            self.Check();

            Buff buffHandler = null;
            int operateType = 1;

            switch (addType)
            {
                case BuffAddType.Replace_0:
                    buffHandler = self.AddNewBuff(buffData, from, skillHandler, ldSkillBuff, intervalMs);
 
                    break;
                case BuffAddType.Extend_2:
                    buffHandler = self.FindSameIdBuff(ldSkillBuff.Id);
                    if (buffHandler != null)
                    {
                        self.RefreshBuff(buffHandler, buffData, from, unit, ldSkillBuff, intervalMs);
                        operateType = 3;
                    }
                    else
                    {
                        buffHandler = self.AddNewBuff(buffData, from, skillHandler, ldSkillBuff, intervalMs);
                    }
                    break;
                default:
                    buffHandler = self.AddNewBuff(buffData, from, skillHandler, ldSkillBuff, intervalMs);
   
                    break;
            }

            if (notice)
            {
                M2C_UnitBuffUpdate m2C_UnitBuffUpdate = self.m2C_UnitBuffUpdate;
                m2C_UnitBuffUpdate.UnitIdBelongTo = unit.Id;
                m2C_UnitBuffUpdate.BuffID = ldSkillBuff.Id;
                m2C_UnitBuffUpdate.BuffOperateType = operateType;
                m2C_UnitBuffUpdate.BuffEndTime = buffHandler.BuffEndTime;
                m2C_UnitBuffUpdate.TargetPostion.Clear();
                m2C_UnitBuffUpdate.TargetPostion.Add(buffHandler.TargetPosition.x);
                m2C_UnitBuffUpdate.TargetPostion.Add(buffHandler.TargetPosition.y);
                m2C_UnitBuffUpdate.TargetPostion.Add(buffHandler.TargetPosition.z);
                m2C_UnitBuffUpdate.Spellcaster = from.GetComponent<UnitInfoComponent>().UnitName;
                m2C_UnitBuffUpdate.UnitType = from.Type;
                m2C_UnitBuffUpdate.UnitConfigId = from.ConfigId;
                m2C_UnitBuffUpdate.SkillId = buffData.SkillId;
                m2C_UnitBuffUpdate.UnitIdFrom = from.Id;
                if (unit.GetComponent<AOIEntity>() == null)
                {
                    Log.Error($"unit.GetComponent<AOIEntity>() == null  {unit.Type} {unit.ConfigId}  {unit.Id}  {unit.IsDisposed}");
                    return true;
                }
                MessageHelper.BroadcastBuff(unit, m2C_UnitBuffUpdate, ldSkillBuff, self.SceneType);
            }

            return true;
        }

        private static Buff AddNewBuff(this BuffManagerComponent self, BuffData buffData, Unit from, Skill_TreeEditor skillHandler, LDSkill_Battle_Buff ldSkillBuff, long intervalMs)
        {
            Unit unit = self.GetParent<Unit>();
            Buff buffHandler = self.AddChild<Buff>();
            self.m_Buffs.Insert(0, buffHandler);
            buffHandler.OnInit(buffData, from, skillHandler, intervalMs);
            self.AddTimer();
            self.AddBuffRecord(1, buffHandler.BuffData.BuffId);
            SkillManagerComponentSystem.ExecuteLinkedSkill(ldSkillBuff.Skill_Init, from, unit);
            return buffHandler;
        }

        private static void RefreshBuff(this BuffManagerComponent self, Buff buffHandler, BuffData buffData, Unit from, Unit unit, LDSkill_Battle_Buff ldSkillBuff, long intervalMs)
        {
            buffHandler.BuffData = buffData;
            buffHandler.TheUnitFrom = from;
            buffHandler.BeginTime = TimeHelper.ServerNow();
            buffHandler.IsTimeEnd = false;
            buffHandler.IsInterrupt = false;
            buffHandler.IsTrigger = false;
            buffHandler.InterValTime = intervalMs;
            if (buffHandler.InterValTime > 0)
            {
                buffHandler.InterValTimeBegin = buffHandler.BeginTime + buffHandler.InterValTime;
            }
            if (buffData.BuffEndTime > 0)
            {
                buffHandler.BuffEndTime = buffData.BuffEndTime;
            }
            SkillManagerComponentSystem.ExecuteLinkedSkill(ldSkillBuff.Skill_Refresh, from, unit);
        }

        private static Buff FindSameIdBuff(this BuffManagerComponent self, int buffId)
        {
            for (int i = 0; i < self.m_Buffs.Count; i++)
            {
                Buff buff = self.m_Buffs[i];
                if (buff.BuffState != BuffState.Finished && buff.MBuff.Id == buffId)
                {
                    return buff;
                }
            }

            return null;
        }

        private static void MarkSameIdBuffs(this BuffManagerComponent self, int buffId, int keep)
        {
            int remain = 0;
            for (int i = 0; i < self.m_Buffs.Count; i++)
            {
                Buff buff = self.m_Buffs[i];
                if (buff.BuffState != BuffState.Finished && buff.MBuff.Id == buffId)
                {
                    remain++;
                }
            }

            for (int i = self.m_Buffs.Count - 1; i >= 0 && remain > keep; i--)
            {
                Buff buff = self.m_Buffs[i];
                if (buff.BuffState == BuffState.Finished || buff.MBuff.Id != buffId)
                {
                    continue;
                }

                buff.BuffState = BuffState.Finished;
                buff.IsInterrupt = true;
                remain--;
            }
        }



        public static bool IsSkillImmune(this BuffManagerComponent self, int skillid)
        {
            int buffcnt = self.m_Buffs.Count;
            for (int i = 0; i < buffcnt; i++)
            {
               
            }


            return false;
        }

        /// <summary>
        /// Immune_Group 对上 Group：整条 buff 加不上。
        /// Immune 只挡状态；Control 全部被免疫才整条加不上。
        /// </summary>
        public static bool IsControlImmune(this BuffManagerComponent self, LDSkill_Battle_Buff incoming)
        {
            if (incoming == null)
            {
                return false;
            }

            int buffcnt = self.m_Buffs.Count;
            bool hasGroup = incoming.Group != null && incoming.Group.Length > 0;
            if (hasGroup)
            {
                for (int i = 0; i < buffcnt; i++)
                {
                    Buff buff = self.m_Buffs[i];
                    if (buff == null || buff.BuffState == BuffState.Finished || buff.MBuff == null)
                    {
                        continue;
                    }

                    if (StateTypeEnum.IdsOverlap(buff.MBuff.Immune_Group, incoming.Group))
                    {
                        return true;
                    }
                }
            }

            return self.IsAllControlImmune(incoming.Control);
        }

        public static bool HasImmuneId(this BuffManagerComponent self, int id)
        {
            if (id <= 0)
            {
                return false;
            }

            int buffcnt = self.m_Buffs.Count;
            for (int i = 0; i < buffcnt; i++)
            {
                Buff buff = self.m_Buffs[i];
                if (buff == null || buff.BuffState == BuffState.Finished)
                {
                    continue;
                }

                int[] immune = buff.MBuff?.Immune;
                if (immune == null)
                {
                    continue;
                }

                for (int j = 0; j < immune.Length; j++)
                {
                    if (immune[j] == id)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static bool IsAllControlImmune(this BuffManagerComponent self, int[] control)
        {
            if (control == null || control.Length == 0)
            {
                return false;
            }

            bool any = false;
            for (int i = 0; i < control.Length; i++)
            {
                int id = control[i];
                if (id <= 0)
                {
                    continue;
                }

                any = true;
                if (!self.HasImmuneId(id))
                {
                    return false;
                }
            }

            return any;
        }

        public static long GetActiveImmuneMask(this BuffManagerComponent self)
        {
            long mask = 0;
            int buffcnt = self.m_Buffs.Count;
            for (int i = 0; i < buffcnt; i++)
            {
                Buff buff = self.m_Buffs[i];
                if (buff == null || buff.BuffState == BuffState.Finished)
                {
                    continue;
                }

                mask |= StateTypeEnum.FromControl(buff.MBuff?.Immune);
            }

            return mask;
        }

        public static int GetBuffNumber(this BuffManagerComponent self, int buffId)
        {
            int number = 0;
            int buffcnt = self.m_Buffs.Count;
            for (int i = buffcnt - 1; i >= 0; i--)
            {
                if (self.m_Buffs[i].BuffData.BuffId == buffId)
                {
                    number++;
                }
            }
            return number;
        }

        public static bool HaveBuff(this BuffManagerComponent self, int buffId)
        {
            return self.GetBuffNumber(buffId) > 0;
        }

        public static int GetBuffSourceNumber(this BuffManagerComponent self, long formId, int buffId)
        {
            int buffnumber = 0;
            int bufflist = self.m_Buffs.Count;

            for (int i = bufflist - 1; i >= 0; i--)
            {
                if (self.m_Buffs[i].BuffData.BuffId != buffId)
                {
                    continue;
                }
                if (formId != 0 && formId != self.m_Buffs[i].TheUnitFrom.Id)
                {
                    continue;
                }
                buffnumber++;
            }
            return buffnumber;
        }

        public static void Check(this BuffManagerComponent self)
        {
            int buffcnt = self.m_Buffs.Count;
            for (int i = buffcnt - 1; i >= 0; i--)
            {
                Buff buffHandler = self.m_Buffs[i];
                if (buffHandler.BuffState != BuffState.Finished)
                {
                    buffHandler.OnUpdate();
                }

                if (self.m_Buffs.Count == 0)
                {
                    break;
                }
                if (self.IsDisposed)
                {
                    return;
                }

                if (self.m_Buffs[i].BuffState == BuffState.Finished)
                {
                    self.FinishAndRemoveBuff(self.m_Buffs[i], i, true);
                }
            }
            if (self.m_Buffs.Count == 0)
            {
                TimerComponent.Instance?.Remove(ref self.Timer);
            }
        }

        public static List<IntStringPair> GetMessageBuff(this BuffManagerComponent self)
        {
            List<IntStringPair> Buffs = new List<IntStringPair>();
            for (int i = 0; i < self.m_Buffs.Count; i++)
            {
                Buff buffHandler = self.m_Buffs[i];
                LDSkill_Battle_Buff ldSkillBuff = buffHandler.MBuff;
                if (ldSkillBuff == null || ldSkillBuff.Id < 10) //子弹
                {
                    continue;
                }
                Buffs.Add(new IntStringPair()
                {
                    KeyId = ldSkillBuff.Id,
                    Value = $"{buffHandler.BuffData.SkillId}_{buffHandler.BuffData.Spellcaster}",
                    Value2 = buffHandler.BuffEndTime.ToString()
                }); ;
            }
            return Buffs;
        }

        public static void BeforeTransfer(this BuffManagerComponent self, int transfer)
        {
            Unit unit = self.GetParent<Unit>();
            if (unit.Type != UnitType.Player)
            {
                return;
            }
            RoleInfoComponentServer unitInfoComponentServer = unit.GetComponent<RoleInfoComponentServer>();
            unitInfoComponentServer.Buffs.Clear();
            int buffcnt = self.m_Buffs.Count;
            for (int i = buffcnt - 1; i >= 0; i--)
            {
                Buff buffHandler = self.m_Buffs[i];
                buffHandler.OnFinished(false);
                ObjectPool.Instance.Recycle(buffHandler);
                self.m_Buffs.RemoveAt(i);
               
                unitInfoComponentServer.Buffs.Add(new IntStringPair() { KeyId = buffHandler.MBuff.Id, Value2 = buffHandler.BuffEndTime.ToString() });
            }
        }
    }
}
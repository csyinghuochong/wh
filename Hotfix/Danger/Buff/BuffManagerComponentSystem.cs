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
            int buffcnt = self.m_Buffs.Count;
            for (int i = buffcnt - 1; i >= 0; i--)
            {
                if (self.m_Buffs[i].TheUnitFrom.Id == unitId)
                {
                    self.OnRemoveBuffItem(self.m_Buffs[i]);
                    self.m_Buffs.RemoveAt(i);
                }
            }
        }

        public static void OnRetreatRemoveBuff(this BuffManagerComponent self, long unitId)
        {
            int buffcnt = self.m_Buffs.Count;
            for (int i = buffcnt - 1; i >= 0; i--)
            {
                if (self.m_Buffs[i].TheUnitFrom.Id == unitId)
                {
                    self.OnRemoveBuffItem(self.m_Buffs[i]);
                    self.m_Buffs.RemoveAt(i);
                }
            }
        }

        public static bool HaveBuffByState(this BuffManagerComponent self, long state)
        {
            //移除buff要保持倒序移除
            int buffcnt = self.m_Buffs.Count;
            for (int i = buffcnt - 1; i >= 0; i--)
            {
                //判断当前状态是否为暴击状态的buff
                /*if (self.m_Buffs[i].MBuff.BuffType != 2)
                {
                    continue;
                }
                long curState = 1 << self.m_Buffs[i].MBuff.buffParameterType;
                if (state == curState)
                {
                    return true;
                }*/
            }
            return false;
        }


        //批量删除buff
        public static void BuffRemoveListBatch(this BuffManagerComponent self, int buffid)
        {
            //判断玩家身上是否有相同的buff,如果有就注销此Buff
            int buffcnt = self.m_Buffs.Count;
            for (int i = buffcnt - 1; i >= 0; i--)
            {
                if (buffid == self.m_Buffs[i].MBuff.Id)
                {
                    Buff buffHandler = self.m_Buffs[i];
                    buffHandler.BuffState = BuffState.Finished;
                    ObjectPool.Instance.Recycle(buffHandler);
                    buffHandler.OnFinished();
                    self.m_Buffs.RemoveAt(i);
                }
            }
            LDSkill_Battle_Buff ldSkillBuff = LDSkill_Battle_BuffCategory.Instance.Get(buffid);
            M2C_UnitBuffRemove m2C_UnitBuffUpdate = self.m2C_UnitBuffRemove;
            m2C_UnitBuffUpdate.UnitIdBelongTo = self.GetParent<Unit>().Id;
            m2C_UnitBuffUpdate.BuffID = buffid;
            MessageHelper.BroadcastBuff(self.GetParent<Unit>(), m2C_UnitBuffUpdate, ldSkillBuff, self.SceneType);
        }

        public static void OnRemoveBuffItem(this BuffManagerComponent self, Buff buffHandler)
        {
            M2C_UnitBuffRemove m2C_UnitBuffUpdate = self.m2C_UnitBuffRemove;
            m2C_UnitBuffUpdate.UnitIdBelongTo = self.GetParent<Unit>().Id;
            m2C_UnitBuffUpdate.BuffID = buffHandler.MBuff.Id;
            MessageHelper.BroadcastBuff(self.GetParent<Unit>(), m2C_UnitBuffUpdate, buffHandler.MBuff, self.SceneType);

            //移除目标buff
            buffHandler.BuffState = BuffState.Finished;
            ObjectPool.Instance.Recycle(buffHandler);
            buffHandler.OnFinished();

            self.AddBuffRecord(0, buffHandler.BuffData.BuffId);
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

            Unit unit = self.GetParent<Unit>();
            if (unit.Type != UnitType.Player)
            {
                return;
            }
        }

        //移除状态的所有buff 
        public static void OnRemoveBuffByState(this BuffManagerComponent self, long state)
        {
            //移除buff要保持倒序移除
            int buffcnt = self.m_Buffs.Count;
            for (int i = buffcnt - 1; i >= 0; i--)
            {
                //判断当前状态是否为暴击状态的buff
               
            }
        }

        public static void RemoveBuffByNumericType(this BuffManagerComponent self, long state)
        {
            int buffcnt = self.m_Buffs.Count;
            for (int i = buffcnt - 1; i >= 0; i--)
            {
               
            }
        }

        /// <summary>
        /// 隐身buff伤害加成, 技能效果内只加成一次
        /// </summary>
        /// <returns></returns>
        public static LDSkill_Battle_Buff GetHideBuffDamgePro(this BuffManagerComponent self)
        {
            int buffcnt = self.m_Buffs.Count;
            for (int i = buffcnt - 1; i >= 0; i--)
            {
                //判断当前状态是否为暴击状态的buff
                LDSkill_Battle_Buff ldSkillBuff = self.m_Buffs[i].MBuff;

                /*   if (ldSkillBuff.BuffType != 2)
                  {
                      continue;
                  }

                  if (ldSkillBuff.buffParameterType != 12)
                  {
                      continue;
                  }

                  if (ldSkillBuff.DamgePro <= 0)
                  {
                      continue;
                  }

                  return self.m_Buffs[i].MBuff;
                  */
            }
            return null;
        }


        public static void OnRevive(this BuffManagerComponent self)
        {
            MapComponent mapComponent = self.DomainScene().GetComponent<MapComponent>();
            if (mapComponent.MapTypeEnum != MapTypeEnum.RunRace)
            {
                self.InitBaoShiBuff();
                self.InitDonationBuff();
                self.InitMaoXianJiaBuff();
                self.InitCombatRankBuff();

                //99002003
                BuffData buffData_2 = new BuffData();
                buffData_2.SkillId = 67000278;
                buffData_2.BuffId = 99002003;
                self.BuffFactory(buffData_2, self.GetParent<Unit>(), null);
            }
        }

        //DeadNoRemove 0移除   1 不移除
        public static void OnDead(this BuffManagerComponent self, Unit attack)
        {
            int buffcnt = self.m_Buffs.Count;
            for (int i = buffcnt - 1; i >= 0; i--)
            {
                Buff buffHandler = self.m_Buffs[i];
               
                /*
                buffHandler.OnFinished();
                ObjectPool.Instance.Recycle(buffHandler);
                self.m_Buffs.RemoveAt(i);
                self.AddBuffRecord(0, buffHandler.BuffData.BuffId); ;
                */
            }
            if (self.m_Buffs.Count == 0)
            {
                TimerComponent.Instance?.Remove(ref self.Timer);
            }
        }

        public static void BuffRemoveList(this BuffManagerComponent self, List<int> buffIist)
        {
            //判断玩家身上是否有相同的buff,如果有就注销此Buff
            HashSet<int> buffIdSet = new HashSet<int>(buffIist);
            int buffcnt = self.m_Buffs.Count;
            for (int i = buffcnt - 1; i >= 0; i--)
            {
                if (buffIdSet.Contains(self.m_Buffs[i].MBuff.Id))
                {
                    self.OnRemoveBuffItem(self.m_Buffs[i]);
                    self.m_Buffs.RemoveAt(i);
                }
            }
        }


        /// <summary>
        /// removetype 1移动  2被攻击[目前用来移除沉睡buff]   3释放技能
        /// </summary>
        /// <param name="self"></param>
        /// <param name="removetype"></param>
        public static void BuffRemoveType(this BuffManagerComponent self, int removetype)
        {
            int buffcnt = self.m_Buffs.Count;
            for (int i = buffcnt - 1; i >= 0; i--)
            {
               
            }
        }

        public static void BuffRemoveByUnit(this BuffManagerComponent self, long unitId, int buffId)
        {
            //判断玩家身上是否有相同的buff,如果有就注销此Buff
            int buffcnt = self.m_Buffs.Count;
            for (int i = buffcnt - 1; i >= 0; i--)
            {
                if (self.m_Buffs[i].MBuff.Id == buffId &&
                    (self.m_Buffs[i].TheUnitFrom.Id == unitId || unitId == 0))
                {
                    self.OnRemoveBuffItem(self.m_Buffs[i]);
                    self.m_Buffs.RemoveAt(i);
                }
            }
        }

        public static void BuffRemoveBySkillid(this BuffManagerComponent self, int skillid)
        {
            //判断玩家身上是否有相同的buff,如果有就注销此Buff
            List<Buff> nowAllBuffList = self.m_Buffs;
            for (int i = nowAllBuffList.Count - 1; i >= 0; i--)
            {
                if (nowAllBuffList[i].MLdSkillConf.Id == skillid)
                {
                    self.OnRemoveBuffItem(self.m_Buffs[i]);
                    self.m_Buffs.RemoveAt(i);
                }
            }
        }

        public static void AddTimer(this BuffManagerComponent self)
        {
            if (self.Timer == 0)
            {
                self.Timer = TimerComponent.Instance.NewRepeatedTimer(500, TimerType.BuffTimer, self);
            }
        }

        public static void UpdateFuHuoStatus(this BuffManagerComponent self)
        {
            Unit unit = self.GetParent<Unit>();
            M2C_UnitBuffStatus m2C_UnitBuffStatus = new M2C_UnitBuffStatus();
            m2C_UnitBuffStatus.UnitID = unit.Id;
            m2C_UnitBuffStatus.FlyType = 101;
            m2C_UnitBuffStatus.BuffID = 0;
            MessageHelper.Broadcast(unit, m2C_UnitBuffStatus);
        }

        public static bool BuffFactory(this BuffManagerComponent self, BuffData buffData, Unit from, Skill_TreeEditor skillHandler, bool notice = true, bool ignoreImmune = false)
        {
            if (buffData.BuffId <= 0)
            {
                Log.Error("buffData.BuffId <= 0");
                return false;
            }

            Unit unit = self.GetParent<Unit>();
            NumericComponent numericComponent = unit.GetComponent<NumericComponent>();
            LDSkill_Battle_Buff ldSkillBuff = LDSkill_Battle_BuffCategory.Instance.Get(buffData.BuffId);
           

            int addBufStatus = 1;   //1新增buff  2 移除 3 重置 4同状态返回
            Buff buffHandler = null;
            List<Buff> nowAllBuffList = self.m_Buffs;

  
            //先移除互斥
            for (int i = nowAllBuffList.Count - 1; i >= 0; i--)
            {
                bool remove = false;
                buffHandler = nowAllBuffList[i];
                LDSkill_Battle_Buff tempBuff = buffHandler.MBuff;
               
                if (remove)
                {
                    M2C_UnitBuffRemove m2C_UnitBuffUpdate = self.m2C_UnitBuffRemove;
                    m2C_UnitBuffUpdate.UnitIdBelongTo = unit.Id;
                    m2C_UnitBuffUpdate.BuffID = tempBuff.Id;
                    MessageHelper.BroadcastBuff(self.GetParent<Unit>(), m2C_UnitBuffUpdate, tempBuff, self.SceneType);
                    buffHandler.BuffState = BuffState.Finished;
                    ObjectPool.Instance.Recycle(buffHandler);
                    buffHandler.OnFinished();
                    self.m_Buffs.RemoveAt(i);
                    self.AddBuffRecord(0, buffHandler.BuffData.BuffId);
                }
            }

            if (addBufStatus == 4)
            {
                return false;
            }
            if (!ignoreImmune && self.IsControlImmune(ldSkillBuff))
            {
                if (Log.IsDebugEnabled)
                {
                    Log.Debug($"IsControlImmune unit={unit.Id} buff={ldSkillBuff.Id}");
                }
                return false;
            }
            //添加Buff
            if (addBufStatus == 1)
            {
                buffHandler = self.AddChild<Buff>();

                self.m_Buffs.Insert(0, buffHandler);     //添加至buff列表中
                buffHandler.OnInit(buffData, from, unit, skillHandler);
                self.AddTimer();

                self.AddBuffRecord(1, buffHandler.BuffData.BuffId);
            }
            //发送改变属性的相关消息
            //buffData.BuffConfig==null 是子弹之类的buff不广播
            if (notice)
            {
                M2C_UnitBuffUpdate m2C_UnitBuffUpdate = self.m2C_UnitBuffUpdate;
                m2C_UnitBuffUpdate.UnitIdBelongTo = unit.Id;
                m2C_UnitBuffUpdate.BuffID = ldSkillBuff.Id;
                m2C_UnitBuffUpdate.BuffOperateType = addBufStatus;
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

            if (addBufStatus == 1 && unit.Type == UnitType.Player
                && ldSkillBuff.Id >= 92041030 && ldSkillBuff.Id <= 92041034)
            {
                long rolePetId = unit.GetComponent<PetComponentServer>().GetFightPetId();
                Unit unitpet = unit.GetParent<UnitComponent>().Get(rolePetId);
                if (unitpet != null)
                {
                    unitpet.GetComponent<BuffManagerComponent>().BuffFactory(buffData, from, skillHandler, notice, ignoreImmune);
                }
            }
            return true;
        }

        public static void BuffAddSyncTime(this BuffManagerComponent self, long endTime, LDSkill_Battle_Buff ldSkillBuff)
        {
            Unit unit = self.GetParent<Unit>();
            int buffcnt = self.m_Buffs.Count;
            for (int i = buffcnt - 1; i >= 0; i--)
            {
                Buff buffHandler = self.m_Buffs[i];
                if (buffHandler.MBuff.Id == ldSkillBuff.Id)
                {
                    buffHandler.BuffEndTime = endTime;
                }
            }
            M2C_UnitBuffUpdate m2C_UnitBuffUpdate = self.m2C_UnitBuffUpdate;
            m2C_UnitBuffUpdate.UnitIdBelongTo = unit.Id;
            m2C_UnitBuffUpdate.BuffID = ldSkillBuff.Id;
            m2C_UnitBuffUpdate.BuffOperateType = 3;
            m2C_UnitBuffUpdate.BuffEndTime = endTime;
            if (unit.GetComponent<AOIEntity>() == null)
            {
                Log.Error($"unit.GetComponent<AOIEntity>() == null  {unit.Type} {unit.ConfigId}  {unit.Id}  {unit.IsDisposed}");
                return;
            }
            MessageHelper.BroadcastBuff(unit, m2C_UnitBuffUpdate, ldSkillBuff, self.SceneType);
        }

      
        /// <summary>
        /// 
        /// </summary>
        /// <param name="self"></param>
        /// <param name="number">移除数量</param>
        /// <returns></returns>
        public static void RemoveFirstCritBuff(this BuffManagerComponent self)
        {
            int buffcnt = self.m_Buffs.Count;
            
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

        public static int GetBuffIndexById(this BuffManagerComponent self, Buff buffHandler)
        {
            int buffindex = 0;
            int bufflist = self.m_Buffs.Count;

            for (int i = bufflist - 1; i >= 0; i--)
            {
                if (self.m_Buffs[i] != buffHandler)
                {
                    continue;
                }
                buffindex = i;
                break;
            }
            return buffindex;
        }

        public static void Check(this BuffManagerComponent self)
        {
            int buffcnt = self.m_Buffs.Count;
            for (int i = buffcnt - 1; i >= 0; i--)
            {
                self.m_Buffs[i].OnUpdate();
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
                    Buff buffHandler = self.m_Buffs[i];
                    ObjectPool.Instance.Recycle(buffHandler);
                    buffHandler.OnFinished();
                    self.m_Buffs.RemoveAt(i);
                    self.AddBuffRecord(0, buffHandler.BuffData.BuffId);
                    continue;
                }
            }
            if (self.m_Buffs.Count == 0)
            {
                TimerComponent.Instance?.Remove(ref self.Timer);
            }
        }

        public static void OnMaoXianJiaUpdate(this BuffManagerComponent self)
        {
           
        }

        public static void InitMaoXianJiaBuff(this BuffManagerComponent self)
        {
            Unit unit = self.GetParent<Unit>();
            if (unit.Type != UnitType.Player)
            {
                return;
            }

            /*int jifen = unit.GetMaoXianExp();
            int activityid = unit.GetComponent<ActivityComponentServer>().GetMaxActivityId(jifen);
            if (activityid == 0)
            {
                return;
            }

            List<int> buffids = ActivityConfigCategory.Instance.GetBuffIds(activityid);
            for (int i = 0; i < buffids.Count; i++)
            {
                BuffData buffData_2 = new BuffData();
                buffData_2.SkillId = 67000278;
                buffData_2.BuffId = buffids[i];
                self.BuffFactory(buffData_2, unit, null);
            }*/
        }

        public static void InitCombatRankBuff(this BuffManagerComponent self)
        {
            /*Unit unit = self.GetParent<Unit>();
            if (unit.Type != UnitType.Player)
            {
                return;
            }

            self.BuffRemoveList(CommonConfig.CombatRankBuff);
            int rankId = unit.GetComponent<NumericComponent>().GetAsInt(NumericType.CombatRankID);
            int occRankId = unit.GetComponent<NumericComponent>().GetAsInt(NumericType.OccCombatRankID);
            //Log.Console($"战力排行buff: {rankId}");
            if (occRankId >= 1 && occRankId <= 3)
            {
                int occ = unit.GetComponent<RoleInfoComponentServer>().RoleInfo.Occ;
                BuffData buffData_2 = new BuffData();
                buffData_2.SkillId = 67000278;
                buffData_2.BuffId = CommonConfig.GetRankBuff(rankId, occRankId, occ);
                self.BuffFactory(buffData_2, unit, null);
            }*/
        }

        public static void InitBaoShiBuff(this BuffManagerComponent self)
        {
            Unit unit = self.GetParent<Unit>();
            if (unit.Type != UnitType.Player)
            {
                return;
            }
        }

        public static void InitBuff(this BuffManagerComponent self, int sceneType)
        {
            Unit unit = self.GetParent<Unit>();
            if (unit.Type != UnitType.Player)
            {
                return;
            }
            long serverTime = TimeHelper.ServerNow();
            RoleInfoComponentServer unitInfoComponentServer = unit.GetComponent<RoleInfoComponentServer>();
            for (int i = 0; i < unitInfoComponentServer.Buffs.Count; i++)
            {
                long endTime = long.Parse(unitInfoComponentServer.Buffs[i].Value2);
                if (endTime <= serverTime)
                {
                    continue;
                }
                BuffData buffData_1 = new BuffData();
                buffData_1.SkillId = 67000278;
                buffData_1.BuffId = unitInfoComponentServer.Buffs[i].KeyId;
                buffData_1.BuffEndTime = endTime;
                self.BuffFactory(buffData_1, self.GetParent<Unit>(), null, true);
            }
            unitInfoComponentServer.Buffs.Clear();

            if (sceneType != MapTypeEnum.RunRace)
            {
                self.InitBaoShiBuff();
                self.InitDonationBuff();
                self.InitSoloBuff(sceneType);
                self.InitMaoXianJiaBuff();
                self.InitCombatRankBuff();
            }
        }

        public static void InitSoloBuff(this BuffManagerComponent self, int sceneType)
        {
            if (sceneType != MapTypeEnum.Solo)
            {
                return;
            }

            Unit unit = self.GetParent<Unit>();
            if (unit.Type != UnitType.Player)
            {
                return;
            }

            for (int i = 0; i < CommonConfig.SoloBuffIds.Count; i++)
            {
                BuffData buffData_2 = new BuffData();
                buffData_2.SkillId = 67000278;
                buffData_2.BuffId = CommonConfig.SoloBuffIds[i];
                self.BuffFactory(buffData_2, unit, null);
            }

            //恢复血量
            NumericComponent numericComponent = unit.GetComponent<NumericComponent>();
            long max_hp = numericComponent.GetAsLong(NumericType.HP_Max_10);
            numericComponent.SetValueNoSync(NumericType.HP_Current_8, 0);
            numericComponent.ApplyChange(null, NumericType.HP_Current_8, max_hp, 0);
        }

        public static void InitDonationBuff(this BuffManagerComponent self)
        {
  
            Unit unit = self.GetParent<Unit>();
            int rankid = 0;
            if (rankid == 0)
            {
                return;
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
                buffHandler.OnFinished();
                ObjectPool.Instance.Recycle(buffHandler);
                self.m_Buffs.RemoveAt(i);
               
                unitInfoComponentServer.Buffs.Add(new IntStringPair() { KeyId = buffHandler.MBuff.Id, Value2 = buffHandler.BuffEndTime.ToString() });
            }
        }
    }
}
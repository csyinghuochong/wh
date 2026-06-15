using System;
using System.Collections.Generic;
using UnityEngine;

namespace ET
{

    [Timer(TimerType.SkillPassive)]
    public class SkillPassiveTimer : ATimer<SkillPassiveComponent>
    {
        public override void Run(SkillPassiveComponent self)
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


    [Timer(TimerType.MonsterSingingTimer)]
    public class MonsterSingingTimer : ATimer<SkillPassiveComponent>
    {
        public override void Run(SkillPassiveComponent self)
        {
            try
            {
                self.OnSingOver();
            }
            catch (Exception e)
            {
                Log.Error($"move timer error: {self.Id}\n{e}");
            }
        }
    }


    [ObjectSystem]
    public class SkillPassiveComponentAwakeSystem : AwakeSystem<SkillPassiveComponent>
    {
        public override void Awake(SkillPassiveComponent self)
        {

        }
    }

    [ObjectSystem]
    public class SkillPassiveComponentDestroySystem : DestroySystem<SkillPassiveComponent>
    {
        public override void Destroy(SkillPassiveComponent self)
        {
            TimerComponent.Instance?.Remove(ref self.Timer);
        }
    }

    public static class SkillPassiveComponentSystem
    {

        public static void Stop(this SkillPassiveComponent self)
        {
            TimerComponent.Instance?.Remove(ref self.Timer);
        }

        public static void Reset(this SkillPassiveComponent self)
        {
            for (int i = 0; i < self.SkillPassiveInfos.Count; i++)
            {
                self.SkillPassiveInfos[i].Reset();
            }
        }

        public static void Activeted(this SkillPassiveComponent self)
        {
            Unit unit = self.GetParent<Unit>();

            //缓存值
            self.UnitType = unit.Type;
            self.StateComponent = unit.GetComponent<StateComponent>();
            self.NumericComponent = unit.GetComponent<NumericComponent>();

            if (unit.GetComponent<NumericComponent>().GetAsInt(NumericType.Now_Dead) != 0)
            {
                return;
            }
            if (unit.Type == UnitType.Player)
            {
                if (unit.SceneType == MapTypeEnum.RunRace || unit.SceneType == MapTypeEnum.Demon)
                {
                    return;
                }

                int equipId = unit.GetWuqiItemID();
                self.OnTrigegerPassiveSkill(SkillPassiveTypeEnum.WandBuff_8, equipId);
                self.OnTrigegerPassiveSkill(SkillPassiveTypeEnum.EquipIndex_15 );
            }

            bool xueliangcheck = false;
            TimerComponent.Instance?.Remove(ref self.Timer);
            if (unit.Type == UnitType.Player || unit.Type == UnitType.Pet)
            {
                xueliangcheck = true;
            }
            else if (unit.Type == UnitType.Monster)
            {
                for (int i = 0; i < self.SkillPassiveInfos.Count; i++)
                {
                    if (self.SkillPassiveInfos[i].SkillPassiveTypeEnum.Contains( SkillPassiveTypeEnum.XueLiang_2))
                    {
                        xueliangcheck = true;
                        break;
                    }
                }
            }
            if (xueliangcheck)
            {
                self.Timer = TimerComponent.Instance.NewRepeatedTimer(1000, TimerType.SkillPassive, self);
            }
        }

        public static void CheckHuiXue(this SkillPassiveComponent self)
        {
            self.HuixueTimeNum = self.HuixueTimeNum + 1;
            //5秒触发一次回血
            if (self.HuixueTimeNum >= 5)
            {
                self.HuixueTimeNum = 0;
            }
            else
            {
                return;
            }

            //只有玩家和宠物有回血
            if (self.UnitType == UnitType.Pet)
            {
                long maxHp = self.NumericComponent.GetAsLong(NumericType.Numeric_Error);

                //满血不触发回血
                if (self.NumericComponent.GetAsLong((int)NumericType.Numeric_Error) >= maxHp)
                    return;

                long addHpValue = 0;
                float now_SecHpAddPro = self.NumericComponent.GetAsFloat(NumericType.Numeric_Error);
                if (now_SecHpAddPro > 0f)
                {
                    addHpValue = (long)(maxHp * now_SecHpAddPro);
                }
                addHpValue += (long)(maxHp * 0.05f);

                //每5秒恢复5%生命
                self.NumericComponent.ApplyChange(null, NumericType.Numeric_Error, addHpValue ,0, true);
            }

            if (self.UnitType == UnitType.Player)
            {
                long maxHp = self.NumericComponent.GetAsLong(NumericType.Numeric_Error);

                //满血不触发回血
                if (self.NumericComponent.GetAsLong((int)NumericType.Numeric_Error) >= maxHp)
                    return;

                long addHpValue = 0;
                float now_SecHpAddPro = self.NumericComponent.GetAsFloat(NumericType.Numeric_Error);
                if (now_SecHpAddPro > 0f)
                {
                    addHpValue = (long)(maxHp * now_SecHpAddPro);
                }

                long now_HuiXue = self.NumericComponent.GetAsLong(NumericType.Numeric_Error);
                if (now_HuiXue > 0f)
                {
                    addHpValue = now_HuiXue * 5;
                }

                if (addHpValue > 0)
                {
                    self.NumericComponent.ApplyChange(null, NumericType.Numeric_Error, addHpValue, 0, true);
                }
            }
        }

        public static void Check(this SkillPassiveComponent self)
        {
            Unit unit = self.GetParent<Unit>();
            self.CheckHuiXue();
            self.OnTrigegerPassiveSkill(SkillPassiveTypeEnum.XueLiang_2, unit.Id);
            self.OnTrigegerPassiveSkill(SkillPassiveTypeEnum.IdleStill_14, unit.Id);
            self.CheckSkillUseMP(unit);
            //self.CheckActGailvTime(unit);
        }

        public static void CheckActGailvTime(this SkillPassiveComponent self, Unit unit)
        {
            //if (self.LastAckGaiLv_1Time == 0 || unit.Type != UnitType.Player || unit.ConfigId != 5)
            //{
            //    return;
            //}

            //if (TimeHelper.ServerNow() - self.LastAckGaiLv_1Time >= 3000)
            //{
            //    self.LastAckGaiLv_1Time = 0;
            //    unit.GetComponent<BuffManagerComponent>().BuffRemoveListBatch(97050404);
            //}
        }

        public static void CheckSkillUseMP(this SkillPassiveComponent self, Unit unit)
        {
            if (unit.Type == UnitType.Player && (unit.ConfigId == 3 || unit.ConfigId == 5))
            {
                NumericComponent numericComponent = unit.GetComponent<NumericComponent>();
                int nowMp = numericComponent.GetAsInt(NumericType.SkillUseMP);
                int maxMp = numericComponent.GetAsInt(NumericType.Numeric_Error);
                float addMp = numericComponent.GetAsFloat(NumericType.Numeric_Error);
                int equipIndex = numericComponent.GetAsInt(NumericType.EquipIndex);
                //equipIndex 0弓   1剑
                int huifuspeed = equipIndex == 0 ? 1 : 2;
                if (addMp == 0f && nowMp < maxMp)
                {
                    unit.GetComponent<NumericComponent>().ApplyChange(null, NumericType.SkillUseMP, 10 * huifuspeed, 0);
                }
                if (addMp > 0f && nowMp < maxMp)
                {
                    unit.GetComponent<NumericComponent>().ApplyChange(null, NumericType.SkillUseMP, 10 * huifuspeed, 0);
                }
            }
        }

        public static void AddPassiveSkill(this SkillPassiveComponent self, int skillId, Dictionary<int, int> magicskills = null)
        {
            LDSkill ldSkill = LDSkillCategory.Instance.Get(skillId);
            self.AddPassiveSkillByType(ldSkill, magicskills);
        }

        public static void RemovePassiveSkill(this SkillPassiveComponent self, int skillId)
        {
            for (int i = self.SkillPassiveInfos.Count - 1; i >= 0; i--)
            {
                if (self.SkillPassiveInfos[i].SkillId != skillId)
                {
                    continue;
                }
                self.SkillPassiveInfos.RemoveAt(i);
                break;
            }
        }

        /// <summary>
        /// 更新角色被动技能
        /// </summary>
        /// <param name="self"></param>
        public static void UpdatePassiveSkill(this SkillPassiveComponent self)
        {
            self.SkillPassiveInfos.Clear();

            List<SkillPro> skillList = self.GetParent<Unit>().GetComponent<SkillSetComponent>().SkillList;
            for (int i = 0; i < skillList.Count; i++)
            {
                if (skillList[i].SkillSetType == (int)SkillSetEnum.Item)
                {
                    continue;
                }
                if (!LDSkillCategory.Instance.Contain(skillList[i].SkillID))
                {
                    continue;
                }
                LDSkill ldSkill = LDSkillCategory.Instance.Get(skillList[i].SkillID);
                self.AddPassiveSkillByType(ldSkill);
            }
        }

        /// <summary>
        /// 更新怪物被动技能
        /// </summary>
        /// <param name="self"></param>
        public static void UpdateMonsterPassiveSkill(this SkillPassiveComponent self)
        {
            self.SkillPassiveInfos.Clear();
            int configId = self.GetParent<Unit>().ConfigId;
            LDMonster ldMonsterCof = LDMonsterCategory.Instance.Get(configId);
            int[] aiSkillIDList = null;
            if (aiSkillIDList == null)
            {
                return;
            }
            for (int i = 0; i < aiSkillIDList.Length; i++)
            {
                if (aiSkillIDList[i] == 0)
                {
                    continue;
                }
                if (!LDSkillCategory.Instance.Contain(aiSkillIDList[i]))
                {
                    continue;
                }
                LDSkill ldSkill = LDSkillCategory.Instance.Get(aiSkillIDList[i]);
                self.AddPassiveSkillByType(ldSkill);
            }
        }

        public static void UpdatePastureSkill(this SkillPassiveComponent self)
        {

        }

        public static void UpdateJingLingSkill(this SkillPassiveComponent self, int jinglingid)
        {
            LDElf ldElf = LDElfCategory.Instance.Get(jinglingid);
            if (ldElf.FunctionType != JingLingFunctionType.AddSkill)
            {
                return;
            }

             LDSkill ldSkill = LDSkillCategory.Instance.Get(int.Parse(ldElf.FunctionValue));
            self.AddPassiveSkillByType(ldSkill);
        }

        public static bool HaveSkillId(this SkillPassiveComponent self, int skillId)
        {
            for (int i = 0; i < self.SkillPassiveInfos.Count; i++)
            {
                if (self.SkillPassiveInfos[i].SkillId == skillId)
                { 
                    return true;
                }
            }
            return false;
        }

        public static void UpdatePetPassiveSkill(this SkillPassiveComponent self, RolePetInfo rolePetInfo)
        {
            self.SkillPassiveInfos.Clear();
            int configId = self.GetParent<Unit>().ConfigId;
            LDPet MonsterCof = LDPetCategory.Instance.Get(configId);
            List<int> zhuanzhuids = new List<int>();
            string[] zhuanzhuskills = MonsterCof.ZhuanZhuSkillID.Split(';');
            for (int i = 0; i < zhuanzhuskills.Length; i++)
            {
                if (zhuanzhuskills[i].Length > 1)
                {
                    zhuanzhuids.Add(int.Parse(zhuanzhuskills[i]));
                }
            }

            for(int i = 0; i < zhuanzhuids.Count; i++)
            {
                LDSkill ldSkill = LDSkillCategory.Instance.Get(zhuanzhuids[i]);
                self.AddPassiveSkillByType(ldSkill);
            }

            string[] baseSkillID = MonsterCof.BaseSkillID.Split(';');
            for (int i = 0; i < baseSkillID.Length; i++)
            {
                int baseSkillId = int.Parse(baseSkillID[i]);
                if (baseSkillId == 0)
                {
                    continue;
                }

                LDSkill ldSkill = LDSkillCategory.Instance.Get(baseSkillId);
                self.AddPassiveSkillByType(ldSkill);
            }

            for (int i = 0; i < rolePetInfo.PetSkill.Count; i++)
            {
                int baseSkillId = rolePetInfo.PetSkill[i];
                if (baseSkillId == 0)
                {
                    continue;
                }

                LDSkill ldSkill = LDSkillCategory.Instance.Get(baseSkillId);
                self.AddPassiveSkillByType(ldSkill);
            }
        }

        public static void AddPassiveSkillByType(this SkillPassiveComponent self, LDSkill ldSkill, Dictionary<int, int> magicskills = null)
        {
            if (ldSkill.SkillType == 1 || SkillHelp.havePassiveSkillType(ldSkill.PassiveSkillType, 0))
            {
                return;
            }
            for (int i = 0; i < self.SkillPassiveInfos.Count; i++)
            {
                if (self.SkillPassiveInfos[i].SkillId == ldSkill.Id)
                {
                    return;
                }
            }

            List<int> PassiveSkillType = new List<int>();
            List<float> PassiveSkillPro = new List<float> {  };   
            for (int i = 0; i < ldSkill.PassiveSkillType.Length; i++)
            {
                PassiveSkillType.Add(ldSkill.PassiveSkillType[i]);
                PassiveSkillPro.Add((float)ldSkill.PassiveSkillPro[i]);  
            }

            int magicqulity = 0;
            if (magicskills!=null && magicskills.ContainsKey(ldSkill.Id))
            {
                magicqulity = magicskills[ldSkill.Id];
            }

            SkillPassiveInfo skillPassiveInfo = new SkillPassiveInfo(ldSkill.Id, PassiveSkillType,
               PassiveSkillPro, ldSkill.PassiveSkillTriggerOnce, ldSkill.SkillCD);
            skillPassiveInfo.MagicQulity = magicqulity; 
            self.SkillPassiveInfos.Add(skillPassiveInfo);
        }

        public static void UpdateMagicQulity(this SkillPassiveComponent self, int skillid, int magicqulity)
        {
            for (int i = 0; i < self.SkillPassiveInfos.Count; i++)
            {
                if (self.SkillPassiveInfos[i].SkillId == skillid)
                {
                    self.SkillPassiveInfos[i].MagicQulity = magicqulity;
                }
            }
        }

        public static  void BeginSingSkill(this SkillPassiveComponent self, SkillPassiveInfo skillIfo, long targetId = 0)
        {
            self.SingSkillIfo = skillIfo;
            self.SingTargetId = targetId;   
            TimerComponent.Instance.Remove(ref self.SingTimer);

            Unit unit = self.GetParent<Unit>();
            LDSkill ldSkill = LDSkillCategory.Instance.Get(skillIfo.SkillId);
            int angle = (int)Quaternion.QuaternionToEuler(unit.Rotation).y;
            self.StateComponent.StateTypeAdd(StateTypeEnum.Singing, $"{skillIfo.SkillId}_{angle}");
            self.SingTimer = TimerComponent.Instance.NewOnceTimer(TimeHelper.ServerNow() + (long)(ldSkill.SkillFrontSingTime * 1000), TimerType.MonsterSingingTimer, self);
        }

        public static void OnSingOver(this SkillPassiveComponent self)
        {
            self.StateComponent.StateTypeRemove(StateTypeEnum.Singing);
            if (self.SingSkillIfo != null)
            {
                self.ImmediateUseSkill(self.SingSkillIfo, self.SingTargetId);
            }
        }

        public static void StateTypeAdd(this SkillPassiveComponent self, long nowStateType)
        {
            if (self.SingTimer > 0  && (nowStateType == StateTypeEnum.Silence || nowStateType == StateTypeEnum.Dizziness))
            {
                TimerComponent.Instance.Remove( ref self.SingTimer );
                self.StateComponent.StateTypeRemove(StateTypeEnum.Singing);
            }
        }

        public static void ImmediateUseSkill(this SkillPassiveComponent self,SkillPassiveInfo skillIfo, long targetId = 0)
        {
            if (self.InstanceId == 0)
            {
                return;
            }
            Unit unit = self.GetParent<Unit>();
            List<long> targetIdList = new List<long>();
            AIComponent aIComponent = unit.GetComponent<AIComponent>();
            LDSkill ldSkill = LDSkillCategory.Instance.Get(skillIfo.SkillId);
            if (aIComponent != null)
            {
                targetId = aIComponent.TargetID;
                Unit aiTarget = unit.GetParent<UnitComponent>().Get(targetId);
                if (aiTarget != null && ldSkill.SkillTargetType == (int)SkillTargetType.TargetOnly
                    && PositionHelper.Distance2D(unit.Position, aiTarget.Position) > aIComponent.ActDistance)
                {
                    return;
                }

                if (ldSkill.SkillTargetTypeNum == 0)
                {
                    targetIdList.Add(targetId);
                }
                else
                {
                    List<long> enemyids = AIGetTargetHelp.GetNearestEnemyIds(unit, (float)aIComponent.ActRange, ldSkill.SkillTargetTypeNum);
                    if ( ( ldSkill.SkillTargetTypeNum == 2 || ldSkill.SkillTargetTypeNum == 3) && enemyids.Count > 0)
                    {
                        aIComponent.ChangeTarget(enemyids[0]);
                    }

                    targetIdList.AddRange(enemyids);
                }
            }
            if (targetIdList.Count == 0)
            {
                targetId = targetId > 0 ? targetId : self.GetParent<Unit>().Id;
                targetIdList.Add(targetId);
            }

            int targetAngle = (int)Quaternion.QuaternionToEuler(unit.Rotation).y;
            Unit target = unit.GetParent<UnitComponent>().Get(targetId);
            if (target != null && target.Id != targetId)
            {
                Vector3 direction = target.Position - unit.Position;
                targetAngle = (int)Mathf.Rad2Deg(Mathf.Atan2(direction.x, direction.z));
            }
            SkillManagerComponent skillManagerComponent = unit.GetComponent<SkillManagerComponent>();
            for (int i = 0; i < targetIdList.Count; i++)
            {
                C2M_SkillCmd cmd = self.C2M_SkillCmd;
                cmd.TargetAngle = targetAngle;

                if (unit.Type == UnitType.Monster)
                {
                    cmd.SkillID = skillIfo.SkillId; 
                }
                else
                {
                    cmd.SkillID = skillIfo.SkillId;
                }
                
                cmd.TargetID = targetIdList[i];
                skillManagerComponent.OnUseSkill(cmd, false);
            }

            long serverTime = TimeHelper.ServerNow();
            long rigidityEndTime  = (long)(ldSkill.SkillRigidity * 1000) + serverTime;
            if (unit.IsDisposed)
            {
                Log.Debug("SkillPassiveComponent :unit.IsDisposed ");
                return;
            }
            self.StateComponent.SetRigidityEndTime(rigidityEndTime);
        }

        public static void OnPlayerMove(this SkillPassiveComponent self)
        {
            for (int i = 0; i < self.SkillPassiveInfos.Count; i++)
            {
                if (self.SkillPassiveInfos[i].SkillPassiveTypeEnum .Contains( SkillPassiveTypeEnum.IdleStill_14) )
                {
                    self.SkillPassiveInfos[i].LastTriggerTime = TimeHelper.ServerNow(); 
                }
            }
        }

        public static int IsTrigegerPassiveTypeEnum_21(this SkillPassiveComponent self)
        {
            for (int i = 0; i < self.SkillPassiveInfos.Count; i++)
            {
                SkillPassiveInfo skillIfo = self.SkillPassiveInfos[i];

                if (!skillIfo.SkillPassiveTypeEnum.Contains(SkillPassiveTypeEnum.PassiveTypeEnum_21))
                {
                    continue;
                }

                float skillproValue = skillIfo.SkillPro[skillIfo.SkillPassiveTypeEnum.IndexOf(SkillPassiveTypeEnum.PassiveTypeEnum_21)];
                bool trigger = skillproValue >= RandomHelper.RandFloat01();
                if (trigger)
                {
                    return skillIfo.SkillId;
                }
            }

            return 0;
        }

        public static List<int> IsTrigegerPassiveTypeEnum_22(this SkillPassiveComponent self)
        {
            Unit unit = self.GetParent<Unit>();

            if (unit.Type != UnitType.Player)
            {
                return null;
            }
            List<int> listskills = new List<int>();
            using ListComponent<SkillPassiveInfo> skillPassiveInfos = ListComponent<SkillPassiveInfo>.Create();
            for (int i = 0; i < self.SkillPassiveInfos.Count; i++)
            {
                if (!self.SkillPassiveInfos[i].SkillPassiveTypeEnum.Contains(SkillPassiveTypeEnum.PassiveTypeEnum_22))
                {
                    continue;
                }

                skillPassiveInfos.Add(self.SkillPassiveInfos[i]);
            }
            if (skillPassiveInfos.Count == 0)
            {
                return listskills;
            }

            for (int s = 0; s < skillPassiveInfos.Count; s++)
            {
                SkillPassiveInfo skillIfo = skillPassiveInfos[s];

                float skillproValue = skillIfo.SkillPro[skillIfo.SkillPassiveTypeEnum.IndexOf(SkillPassiveTypeEnum.PassiveTypeEnum_22)];

                bool trigger = skillproValue >= RandomHelper.RandFloat01();

                if (trigger)
                {
                    listskills.Add(skillIfo.SkillId);
                }
            }
            return listskills;
        }

        public static void OnTrigegerPassiveSkill(this SkillPassiveComponent self, int skillPassiveTypeEnum, long targetId = 0, int skillid = 0, List<int> passiveTypeEnum_22 = null)
        {
            Unit unit = self.GetParent<Unit>();

            if (unit.Type == UnitType.Player)
            {
                ChengJiuComponent chengJiuComponent = unit.GetComponent<ChengJiuComponent>();
                if (chengJiuComponent.JingLingUnitId != 0 && unit.GetParent<UnitComponent>().Get(chengJiuComponent.JingLingUnitId) != null)
                {
                    Unit jingling = unit.GetParent<UnitComponent>().Get(chengJiuComponent.JingLingUnitId);
                    jingling.GetComponent<SkillPassiveComponent>().OnTrigegerPassiveSkill(skillPassiveTypeEnum, targetId, skillid);
                }
            }

         
            using ListComponent<SkillPassiveInfo> skillPassiveInfos = ListComponent<SkillPassiveInfo>.Create();
            for (int i = 0; i < self.SkillPassiveInfos.Count; i++)
            {
                if (!self.SkillPassiveInfos[i].SkillPassiveTypeEnum.Contains(skillPassiveTypeEnum) )
                {
                    continue;
                }

                if (self.SkillPassiveInfos[i].SkillPassiveTypeEnum.Contains(SkillPassiveTypeEnum.AckNumber_16))
                {
                    self.SkillPassiveInfos[i].TriggerNumber++;
                }

                skillPassiveInfos.Add(self.SkillPassiveInfos[i]);
            }
            if (skillPassiveInfos.Count == 0)
            {
                return;
            }

            long serverTime = TimeHelper.ServerNow();
           
            for (int s = 0; s < skillPassiveInfos.Count; s++)
            {
                SkillPassiveInfo skillIfo = skillPassiveInfos[s];

                float skillproValue = skillIfo.SkillPro[skillIfo.SkillPassiveTypeEnum.IndexOf(skillPassiveTypeEnum)];
                if (skillPassiveTypeEnum == SkillPassiveTypeEnum.WandBuff_8)
                {
                    int weapontype = Convert.ToInt32(skillproValue);
                    int buffId = LDSkillCategory.Instance.Get(skillIfo.SkillId).InitBuffID[0];
                    int equipType = ItemHelper.GetNewEquipType(ItemBigType.Type_Item, (int)targetId);
                    int weaponType = targetId == 0 ? ItemEquipType.Common : equipType;
                    if (weaponType != weapontype)
                    {
                        unit.GetComponent<BuffManagerComponent>().BuffRemoveByUnit(0, buffId);
                    }
                    if (weaponType == weapontype && buffId!=0)
                    {
                        BuffData buffData_1 = new BuffData();
                        buffData_1.SkillId = skillIfo.SkillId;
                        buffData_1.BuffId = buffId;
                        unit.GetComponent<BuffManagerComponent>().BuffFactory(buffData_1, unit, null);
                    }
                    continue;
                }
                //只触发一次
                if (skillIfo.LastTriggerTime > 0 && skillIfo.TriggerOnce == 1)
                {
                    continue;
                }
                //触发限制
                if (skillIfo.TriggerInterval > 0 && serverTime - skillIfo.LastTriggerTime < skillIfo.TriggerInterval)
                {
                    continue;
                }
                bool trigger = false;

                switch (skillPassiveTypeEnum)
                {
                    case SkillPassiveTypeEnum.AckGaiLv_1:
                        trigger = skillproValue >= RandomHelper.RandFloat01();
                        //self.LastAckGaiLv_1Time = serverTime;
                        break;
                    case SkillPassiveTypeEnum.XueLiang_2:
                        NumericComponent numCom = unit.GetComponent<NumericComponent>();
                        if (unit.Type == UnitType.JingLing)
                        {
                            Unit master = unit.GetParent<UnitComponent>().Get(unit.MasterId);
                            numCom = (master != null && !master.IsDisposed) ? master.GetComponent<NumericComponent>() : numCom;
                        }

                        long nowHp = numCom.GetAsLong((int)NumericType.Numeric_Error);
                        long maxHp = numCom.GetAsLong((int)NumericType.Numeric_Error);
                        float hpPro = (float)nowHp / (float)maxHp;
                        trigger = hpPro <= skillproValue;
                        break;
                    case SkillPassiveTypeEnum.BeHurt_3:
                    case SkillPassiveTypeEnum.Critical_4:
                    case SkillPassiveTypeEnum.ShanBi_5:
                    case SkillPassiveTypeEnum.WillDead_6:
                    case SkillPassiveTypeEnum.SkillGaiLv_7:
                    case SkillPassiveTypeEnum.AckDistance_9:
                    case SkillPassiveTypeEnum.AckDistance_10:
                    case SkillPassiveTypeEnum.IdleStill_14:
                    case SkillPassiveTypeEnum.EquipIndex_15:
                    case SkillPassiveTypeEnum.AllSkill_17:
                    case SkillPassiveTypeEnum.PetBattleBegin_18:
                    case SkillPassiveTypeEnum.AckCritical_19:
                    case SkillPassiveTypeEnum.FanGunCD_20:
                        if (skillIfo.MagicQulity > 0)
                        {
                            //Console.WriteLine($"skillIfo.MagicQulity > 0:  {skillIfo.MagicQulity}");
                            //skillproValue为SkillConfig的 PassiveSkillPro（被动技能触发参数）
                            //实际触发概率=(原始触发概率/2)+((原始触发概率/2)*(品质/80))
                            skillproValue = (skillproValue / 2f) + ((skillproValue / 2f) * (skillIfo.MagicQulity / 100f));
                        }
                        trigger = skillproValue >= RandomHelper.RandFloat01();

                        break;
                    case SkillPassiveTypeEnum.PassiveTypeEnum_22:
                        trigger = passiveTypeEnum_22.Contains(skillIfo.SkillId ); 
                        break;
                    case SkillPassiveTypeEnum.TeamerEnter_12:
                        trigger = true;
                        break;
                    case SkillPassiveTypeEnum.AckNumber_16:
                        trigger = skillIfo.TriggerNumber >= skillproValue;
                        if (trigger)
                        {
                            skillIfo.TriggerNumber = 0;
                        }
                        break;
                }
                if (!trigger)
                {
                    continue;
                }
                
                if (skillid == skillIfo.SkillId)
                {
                    Log.Debug($"SkillPassiveComponent: {skillIfo.SkillId}");
                    continue;
                }

                SkillManagerComponent skillManagerComponent = unit.GetComponent<SkillManagerComponent>();   
                if (skillManagerComponent.IsCanUseSkill(skillIfo.SkillId, false, skillPassiveTypeEnum != SkillPassiveTypeEnum.WillDead_6) != ErrorCode.ERR_Success)
                {
                    continue;
                }

                //int weaponSkill = unit.GetWeaponSkill(skillIfo.SkillId);
                //SkillConfig skillConfig = SkillConfigCategory.Instance.Get(weaponSkill);
                LDSkill ldSkill = LDSkillCategory.Instance.Get(skillIfo.SkillId);
                if (ldSkill.SkillFrontSingTime > 0f)
                {
                    self.BeginSingSkill(skillIfo, targetId);
                }
                else
                {
                    self.ImmediateUseSkill(skillIfo, targetId);
                }
                skillIfo.LastTriggerTime = serverTime;
            }
        }
    }
}

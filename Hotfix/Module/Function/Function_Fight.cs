using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ET
{
    //[MessageHandler(AppType.Gate)]
    public class Function_Fight
    {

        public M2C_UnitNumericListUpdate m2C_UnitNumericListUpdate = new M2C_UnitNumericListUpdate();

        private static readonly object obj = new object();
        //实例化自身
        private static Function_Fight _instance;
        public static Function_Fight GetInstance()
        {
            lock (obj)
            {
                if (_instance == null)
                {
                    _instance = new Function_Fight();
                }
            }
            return _instance;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="attackUnit"></param>
        /// <param name="defendUnit"></param>
        /// <param name="skillHandler"></param>
        /// <param name="hurtMode">0 默认 1持续伤害</param>
        /// <returns></returns>
        public bool Fight(Unit attackUnit, Unit defendUnit, SkillHandler skillHandler, int hurtMode)
        {
            if (defendUnit.IsDisposed)
            {
                return false;
            }

            Skill skillconfig = skillHandler.SkillConf;
            //吟唱进度
            float singingvalue = 1;
            //蓄力技能计算伤害
            if (skillconfig.SkillType == 1 && SkillHelp.havePassiveSkillType(skillconfig.PassiveSkillType, 2))
            {
                singingvalue = skillHandler.SkillInfo.SingValue;
                if (singingvalue < 0.3f)
                {
                    singingvalue = 0.3f;
                }
            }

            float buffDamgePro = 0f;
            float buffHurtValueAdd = 0f;
            ///Buff层数触发技能  buffid 1 技能ID 触发间隔
            if (SkillCategory.Instance.BuffTriggerSkill.ContainsKey(skillconfig.Id))
            {
                KeyValuePairLong4 keyValuePairLong = SkillCategory.Instance.BuffTriggerSkill[skillconfig.Id];
                List<Unit> allDefend = attackUnit.GetParent<UnitComponent>().GetAll();
                for ( int defend = 0; defend < allDefend.Count; defend++  )
                {
                    BuffManagerComponent buffManagerComponent = allDefend[defend].GetComponent<BuffManagerComponent>();
                    if (buffManagerComponent == null)
                    {
                        continue;
                    }
                    int buffNum = buffManagerComponent.GetBuffSourceNumber(attackUnit.Id, (int)keyValuePairLong.KeyId);
                    if (buffNum <= 0)
                    {
                        continue;
                    }
                    if (keyValuePairLong.Value3 == 0)
                    {
                        allDefend[defend].GetComponent<BuffManagerComponent>().BuffRemoveByUnit(0, (int)keyValuePairLong.KeyId);
                    }
                    attackUnit.GetComponent<SkillManagerComponent>().TriggerBuffSkill(keyValuePairLong, allDefend[defend].Id, buffNum).Coroutine();
                }
            }

            ///Buff层数叠加伤害  buffid 2 层数  附加伤害系数
            if (SkillCategory.Instance.BuffAddHurt.ContainsKey(skillconfig.Id))
            {
                KeyValuePairLong4 keyValuePairLong = SkillCategory.Instance.BuffAddHurt[skillconfig.Id];
                int buffId = (int)keyValuePairLong.KeyId;
                int buffNum = defendUnit.GetComponent<BuffManagerComponent>().GetBuffSourceNumber(0, buffId);
                if(buffNum > 0)
                {
                    defendUnit.GetComponent<BuffManagerComponent>().BuffRemoveByUnit(0, (int)keyValuePairLong.KeyId);
                    buffHurtValueAdd = keyValuePairLong.Value2 * 0.001f * buffNum;
                }
            }

            //闪电链增加的伤害
            float chainLightningAddValue = skillHandler.HurtAddPro;

            //设置PK状态
            bool playerPKStatus = false;
            if (attackUnit.Type == UnitType.Player && defendUnit.Type == UnitType.Player)
            {
                playerPKStatus = true;
            }
           
            if (attackUnit.MasterIsPlayer() && defendUnit.Type == UnitType.Player)
            {
                playerPKStatus = true;
            }

            if (attackUnit.Type == UnitType.Player && defendUnit.MasterIsPlayer() )
            {
                playerPKStatus = true;
            }


            //已死亡
            if (defendUnit.GetComponent<NumericComponent>().GetAsInt(NumericType.Now_Dead) == 1)
            {
                return false;
            }
            //无敌对所有都有效
            //if (defendUnit.GetComponent<StateComponent>().StateTypeGet(StateTypeEnum.WuDi) && playerPKStatus == false)
            if (defendUnit.GetComponent<StateComponent>().StateTypeGet(StateTypeEnum.WuDi))
            {
                return false;
            }
            // 悬空buff，不受伤害
            if (defendUnit.GetComponent<StateComponent>().StateTypeGet(StateTypeEnum.Hung))
            {
                return false;
            }
            //对怪无敌，对人不无敌
            if (defendUnit.GetComponent<StateComponent>().StateTypeGet(StateTypeEnum.WuDiMonster) && playerPKStatus == false)
            {
                return false;
            }

            //99002002 角斗场免伤状态
            int sceneType = defendUnit.DomainScene().GetComponent<MapComponent>().MapTypeEnum;
            if (sceneType == MapTypeEnum.Arena && attackUnit.GetComponent<BuffManagerComponent>().GetBuffSourceNumber(0, 99002002) > 0)
            {
                return false;
            }


            if (attackUnit.GetComponent<StateComponent>().StateTypeGet(StateTypeEnum.MiaoSha))
            {
                long hp = defendUnit.GetComponent<NumericComponent>().GetAsLong(NumericType.Numeric_Error) + 1;
                defendUnit.GetComponent<NumericComponent>().ApplyChange(attackUnit, NumericType.Numeric_Error, hp * -1, skillconfig.Id);
                return true;
            }

            int DamgeType = 0;      //伤害类型
            SkillPassiveComponent defendSkillPassiveComponent = defendUnit.GetComponent<SkillPassiveComponent>();
            defendSkillPassiveComponent.OnTrigegerPassiveSkill(SkillPassiveTypeEnum.BeHurt_3, attackUnit.Id);
            defendUnit.GetComponent<BuffManagerComponent>()?.BuffRemoveType(2);

            if (skillHandler.OnlyOncePassiveActionUnitID.Count > 0)
            {
                if (!skillHandler.OnlyOncePassiveActionUnitID.Contains(defendUnit.Id))
                {
                    skillHandler.OnlyOncePassiveActionUnitID.Add(defendUnit.Id);
                    C2M_SkillCmd cmd = new C2M_SkillCmd();

                    cmd.SkillID = (int)skillHandler.OnlyOncePassiveActionUnitID[0];
                    cmd.TargetID = defendUnit.Id;

                    Vector3 direction = defendUnit.Position - attackUnit.Position;
                    float ange = Mathf.Rad2Deg(Mathf.Atan2(direction.x, direction.z));
                    if (direction == Vector3.zero)
                    {
                        cmd.TargetAngle = (int)Quaternion.QuaternionToEuler(attackUnit.Rotation).y;
                    }
                    else
                    {
                        cmd.TargetAngle = Mathf.FloorToInt(ange);
                    }
                    cmd.TargetDistance = Vector3.Distance(defendUnit.Position, attackUnit.Position);
                    attackUnit.GetComponent<SkillManagerComponent>().OnUseSkill(cmd, false);
                }
            }

            if (skillHandler.OnlyHideBuffActionUnitID.Count > 0 && !skillHandler.IsSpecifiedFight(defendUnit))
            {
                SkillBuff skillBuff = SkillBuffCategory.Instance.Get((int)skillHandler.OnlyHideBuffActionUnitID[0]);


                if (!skillHandler.OnlyHideBuffActionUnitID.Contains(defendUnit.Id))
                {
                    if (skillBuff.DamgePro > 0)
                    {
                        buffDamgePro = (float)skillBuff.DamgePro;
                    }

                    skillHandler.OnlyHideBuffActionUnitID.Add(defendUnit.Id);

                    BuffData buffData_2 = new BuffData();
                    buffData_2.SkillId = 67000278;
                    buffData_2.BuffId = int.Parse(skillBuff.buffParameterValue2); //69000046
                    defendUnit.GetComponent<BuffManagerComponent>().BuffFactory(buffData_2, attackUnit, null, true);
                }
            }

            //获取攻击方属性
            NumericComponent numericComponentAttack = attackUnit.GetComponent<NumericComponent>();
            long attack_Hp = numericComponentAttack.GetAsLong(NumericType.Numeric_Error);
            long attack_MaxHp = numericComponentAttack.GetAsLong(NumericType.Numeric_Error);
            long attack_MinAct = numericComponentAttack.GetAsLong(NumericType.Numeric_Error);
            long attack_MaxAct = numericComponentAttack.GetAsLong(NumericType.Numeric_Error);
            long attack_MageAct = numericComponentAttack.GetAsLong(NumericType.Numeric_Error);
            long attack_MinDef = numericComponentAttack.GetAsLong(NumericType.Numeric_Error);
            long attack_MaxDef = numericComponentAttack.GetAsLong(NumericType.Numeric_Error);

            float attackPet_hit = 0;
            float attackPet_cri = 0;

            //当前幸运
            int nowluck = numericComponentAttack.GetAsInt(NumericType.Numeric_Error);
            float luckPro = 0;
            switch (nowluck)
            {
                case 0:
                    luckPro = 0.01f;
                    break;
                case 1:
                    luckPro = 0.02f;
                    break;
                case 2:
                    luckPro = 0.04f;
                    break;
                case 3:
                    luckPro = 0.08f;
                    break;
                case 4:
                    luckPro = 0.12f;
                    break;
                case 5:
                    luckPro = 0.2f;
                    break;
                case 6:
                    luckPro = 0.3f;
                    break;
                case 7:
                    luckPro = 0.4f;
                    break;
                case 8:
                    luckPro = 0.5f;
                    break;
                case 9:
                    luckPro = 1f;
                    break;

                default:
                    luckPro = 1f;
                    break;
            }

            if (RandomHelper.RandFloat01() <= luckPro)
            {
                attack_MinAct = attack_MaxAct;
            }

            //最低攻击之换算
            long minActAttack = (long)((attack_MaxAct * 0.5f) + attack_MaxAct * ((float)attack_MinAct / (float)attack_MaxAct) / 2);
            if (minActAttack > attack_MaxAct)
            {
                minActAttack = attack_MaxAct;
            }

            //获取攻击值
            long attack_Act = (long)RandomHelper.RandomNumberFloat(minActAttack, attack_MaxAct);
            if (attackUnit.Type == UnitType.Player)
            {
                //攻击强度和法术强度
                switch (attackUnit.GetComponent<UserInfoComponent>().UserInfo.Occ)
                {
                    //战士
                    case 1:
                        attack_Act += numericComponentAttack.GetAsLong(NumericType.Numeric_Error);
                        break;
                    //法师
                    case 2:
                        attack_Act += numericComponentAttack.GetAsLong(NumericType.Numeric_Error);
                        break;
                    //猎人
                    case 3:
                        attack_Act += numericComponentAttack.GetAsLong(NumericType.Numeric_Error);
                        break;
                    //唤魔者
                    case 4:
                        attack_Act += numericComponentAttack.GetAsLong(NumericType.Numeric_Error);
                        break;
                }
            }
            //long attack_def = (long)RandomHelper.RandomNumberFloat(attack_MinDef, attack_MaxDef);

            //获取受击方属性
            NumericComponent numericComponentDefend = defendUnit.GetComponent<NumericComponent>();
            //long defend_Hp = numericComponentDefend.GetAsLong(NumericType.Numeric_Error);
            //long defend_MaxHp = numericComponentDefend.GetAsLong(NumericType.Numeric_Error);
            long defend_MinAct = numericComponentDefend.GetAsLong(NumericType.Numeric_Error);
            long defend_MaxAct = numericComponentDefend.GetAsLong(NumericType.Numeric_Error);
            long defend_MinDef = numericComponentDefend.GetAsLong(NumericType.Numeric_Error);
            long defend_MaxDef = numericComponentDefend.GetAsLong(NumericType.Numeric_Error);
            long defend_MinAdf = numericComponentDefend.GetAsLong(NumericType.Numeric_Error);
            long defend_MaxAdf = numericComponentDefend.GetAsLong(NumericType.Numeric_Error);

            //忽视防御
            defend_MinDef = (long)((float)defend_MinDef * (1.0f - numericComponentAttack.GetAsFloat(NumericType.Numeric_Error)) - numericComponentAttack.GetAsLong(NumericType.Numeric_Error));
            defend_MaxDef = (long)((float)defend_MaxDef * (1.0f - numericComponentAttack.GetAsFloat(NumericType.Numeric_Error)) - numericComponentAttack.GetAsLong(NumericType.Numeric_Error));
            defend_MinAdf = (long)((float)defend_MinAdf * (1.0f - numericComponentAttack.GetAsFloat(NumericType.Numeric_Error)) - numericComponentAttack.GetAsLong(NumericType.Numeric_Error));
            defend_MaxAdf = (long)((float)defend_MaxAdf * (1.0f - numericComponentAttack.GetAsFloat(NumericType.Numeric_Error)) - numericComponentAttack.GetAsLong(NumericType.Numeric_Error));

            //限制
            defend_MinDef = defend_MinDef < 0 ? 0 : defend_MinDef;
            defend_MaxDef = defend_MaxDef < 0 ? 0 : defend_MaxDef;
            defend_MinAdf = defend_MinAdf < 0 ? 0 : defend_MinAdf;
            defend_MaxAdf = defend_MaxAdf < 0 ? 0 : defend_MaxAdf;

            long defend_Act = (long)RandomHelper.RandomNumberFloat(defend_MinAct, defend_MaxAct);
            long defend_def = (long)RandomHelper.RandomNumberFloat(defend_MinDef, defend_MaxDef);
            long defend_adf = (long)RandomHelper.RandomNumberFloat(defend_MinAdf, defend_MaxAdf);

            float defendPet_dodge = 0;

            bool ifMonsterBoss_Act = false;
            bool ifMonsterBoss_Def = false;

            //当前是否在宠物副本
            bool petfuben = sceneType == MapTypeEnum.PetDungeon || sceneType == MapTypeEnum.PetTianTi;
            if (sceneType == MapTypeEnum.RunRace)
            {
                Log.Warning($"变身大赛触发技能伤害： sceneType == SceneTypeEnum.RunRace  {skillconfig.Id}");
                return false;
            }

            //计算是否闪避
            int defendUnitLv = 0;
            switch (defendUnit.Type)
            {
                //怪物
                case UnitType.Monster:
                   
                    defendUnit.GetComponent<AIComponent>()?.BeAttacking(attackUnit);
                    MonsterConfig monsterCof = MonsterConfigCategory.Instance.Get(defendUnit.ConfigId);
                    defendUnitLv = monsterCof.Lv;
                    if (monsterCof.MonsterType == (int)MonsterTypeEnum.Boss)
                    {
                        ifMonsterBoss_Act = true;
                    }
                    break;
                //宠物
                case UnitType.Pet:
                    defendUnit.GetComponent<AIComponent>()?.BeAttacking(attackUnit);
                    Pet petCof = PetCategory.Instance.Get(defendUnit.ConfigId);
                    defendUnitLv = petCof.PetLv;
                    defend_def += numericComponentDefend.GetAsLong(NumericType.Numeric_Error);
                    defend_adf += numericComponentDefend.GetAsLong(NumericType.Numeric_Error);
                    defend_def += (int)(defend_def * numericComponentDefend.GetAsFloat(NumericType.Numeric_Error));
                    defend_adf += (int)(defend_adf * numericComponentDefend.GetAsFloat(NumericType.Numeric_Error));
                    defendPet_dodge += numericComponentDefend.GetAsFloat(NumericType.Numeric_Error);
                    break;
                //玩家
                case UnitType.Player:
                    defendUnitLv = defendUnit.GetComponent<UserInfoComponent>().UserInfo.Lv;
                    //defendUnit.GetComponent<AttackRecordComponent>().BeAttackId = attackUnit.Id;
                    //受击增加怒气值
                    if (defendUnit.GetComponent<SkillSetComponent>().IfJuexXingSkill())
                    {
                        numericComponentDefend.ApplyChange(null, NumericType.JueXingAnger, 1, 0);
                    }
                    break;
            }

            int attackUnitLv = 0;
            switch (attackUnit.Type)
            {
                //怪物
                case UnitType.Monster:
                    MonsterConfig monsterCof = MonsterConfigCategory.Instance.Get(attackUnit.ConfigId);
                    attackUnitLv = monsterCof.Lv;
                    if (monsterCof.MonsterType == (int)MonsterTypeEnum.Boss)
                        ifMonsterBoss_Def = true;
                    break;
                //宠物
                case UnitType.Pet:
                    Pet petCof = PetCategory.Instance.Get(attackUnit.ConfigId);
                    attackUnitLv = petCof.PetLv;

                    //增加宠物属性
                    ///从主人身上取
                    attack_MaxAct += numericComponentAttack.GetAsLong(NumericType.Numeric_Error);
                    attack_MageAct += numericComponentAttack.GetAsLong(NumericType.Numeric_Error);
                    attackPet_hit += numericComponentAttack.GetAsFloat(NumericType.Numeric_Error);
                    attackPet_cri += numericComponentAttack.GetAsFloat(NumericType.Numeric_Error);

                    attack_MaxAct += (int)(attack_MaxAct * numericComponentAttack.GetAsFloat(NumericType.Numeric_Error));
                    attack_MageAct += (int)(attack_MageAct * numericComponentAttack.GetAsFloat(NumericType.Numeric_Error));

                    //宠物没有最低攻击
                    attack_MinAct = attack_MaxAct;

                    break;
                //玩家
                case UnitType.Player:
                    attackUnitLv = attackUnit.GetComponent<UserInfoComponent>().UserInfo.Lv;
                    //attackUnit.GetComponent<AttackRecordComponent>().AttackingId = defendUnit.Id;
                    //攻击者增加怒气值
                    if (attackUnit.GetComponent<SkillSetComponent>().IfJuexXingSkill())
                    {
                        numericComponentAttack.ApplyChange(null, NumericType.JueXingAnger, 10, 0);
                    }
                    break;
            }

            //float addHitPro = numericComponentAttack.GetAsFloat(NumericType.Numeric_Error) + LvProChange(numericComponentAttack.GetAsLong(NumericType.Numeric_Error), defendUnitLv);
            //float addDodgePro = numericComponentDefend.GetAsFloat(NumericType.Numeric_Error) + LvProChange(numericComponentDefend.GetAsLong(NumericType.Numeric_Error), attackUnitLv);
            float addHitPro = numericComponentAttack.GetAsFloat(NumericType.Numeric_Error);
            float addDodgePro = numericComponentDefend.GetAsFloat(NumericType.Numeric_Error);

            float addHitLvPro = LvProChange(numericComponentAttack.GetAsLong(NumericType.Numeric_Error), defendUnitLv);
            float addDodgeLvPro = LvProChange(numericComponentDefend.GetAsLong(NumericType.Numeric_Error), attackUnitLv);


            addHitPro += addHitLvPro;
            addDodgePro += addDodgeLvPro;

            //等级差命中
            float HitLvPro = (attackUnitLv - defendUnitLv) * 0.03f;
            if (HitLvPro <= -0.1f)
            {
                HitLvPro = -0.1f;
            }

            if (HitLvPro >= 0.2f)
            {
                HitLvPro = 0.2f;
            }

            //等级差闪避
            float DodgeLvPro = (attackUnitLv - defendUnitLv) * 0.03f;
            if (DodgeLvPro <= 0)
            {
                DodgeLvPro = 0;
            }
            if (DodgeLvPro >= 0.1f)
            {
                DodgeLvPro = 0.1f;
            }

            //初始化命中
            float initHitPro = 0.95f;
            float dodgeSum = addDodgePro + DodgeLvPro + defendPet_dodge;
            float hitAdd = HitLvPro + addHitPro + attackPet_hit - dodgeSum;     //附加部分的命中属性
            float HitPro = initHitPro + hitAdd;


            //pk命中
            if (playerPKStatus)
            {
                HitPro -= numericComponentDefend.GetAsFloat(NumericType.Numeric_Error);
                HitPro += numericComponentAttack.GetAsFloat(NumericType.Numeric_Error);
            }

            //最低命中
            if (HitPro <= 0.6f)
            {
                HitPro = 0.6f;
            }

            //根据双方战力增加命中
            if (attackUnit.Type == UnitType.Player && defendUnit.Type == UnitType.Player)
            {
                HitPro += GetFightValueActProValue(attackUnit.GetComponent<UserInfoComponent>().UserInfo.Combat, defendUnit.GetComponent<UserInfoComponent>().UserInfo.Combat) * 0.66f;
            }

            //百发百中(只有玩家对怪物有效)
            if (attackUnit.Type == UnitType.Player && defendUnit.Type == UnitType.Monster && skillconfig.SkillActType == 0)
            {
                if (attackUnit.GetComponent<SkillSetComponent>().GetBySkillID(68000009) != null)
                {
                    HitPro = 1;
                }
            }

            //闪避概率
            bool ifHit = true;

            if (skillconfig.IfMustAct != 1) {

                if (RandomHelper.RandFloat() >= HitPro)
                {
                    ifHit = false;
                }


                if (skillconfig.SkillActType == 0) {

                    float dodgeNowValue = numericComponentDefend.GetAsFloat(NumericType.Numeric_Error);

                    //玩家闪避最多不超过60%
                    if (defendUnit.Type == UnitType.Player)
                    {
                        if (dodgeNowValue >= 0.5)
                        {
                            dodgeNowValue = 0.5f;
                        }
                    }

                    if (RandomHelper.RandFloat() <= dodgeNowValue)
                    {
                        ifHit = false;
                    }
                    
                }

                //技能闪避
                if (skillconfig.SkillActType == 1)
                {
                    float dodgeNowValue = numericComponentDefend.GetAsFloat(NumericType.Numeric_Error);

                    //玩家命中的20%抵消对应闪避
                    dodgeNowValue = dodgeNowValue - hitAdd * 0.2f;

                    //玩家闪避最多不超过60%
                    if (defendUnit.Type == UnitType.Player)
                    {
                        if (dodgeNowValue >= 0.5)
                        {
                            dodgeNowValue = 0.5f;
                        }
                    }

                    if (RandomHelper.RandFloat() <= dodgeNowValue)
                    {
                        ifHit = false;
                    }
                }

                //物理闪避
                if (skillconfig.DamgeType == 1)
                {
                    float dodgeNowValue = numericComponentDefend.GetAsFloat(NumericType.Numeric_Error);

                    //玩家命中的20%抵消对应闪避
                    dodgeNowValue = dodgeNowValue - hitAdd * 0.2f;

                    //玩家闪避最多不超过60%
                    if (defendUnit.Type == UnitType.Player)
                    {
                        if (dodgeNowValue >= 0.5)
                        {
                            dodgeNowValue = 0.5f;
                        }
                    }

                    if (RandomHelper.RandFloat() <= dodgeNowValue)
                    {
                        ifHit = false;
                    }
                }

                //魔法闪避
                if (skillconfig.DamgeType == 2)
                {
                    float dodgeNowValue = numericComponentDefend.GetAsFloat(NumericType.Numeric_Error);

                    //玩家命中的20%抵消对应闪避
                    dodgeNowValue = dodgeNowValue - hitAdd * 0.2f;

                    //玩家闪避最多不超过60%
                    if (defendUnit.Type == UnitType.Player)
                    {
                        if (dodgeNowValue >= 0.5)
                        {
                            dodgeNowValue = 0.5f;
                        }
                    }

                    if (RandomHelper.RandFloat() <= dodgeNowValue)
                    {
                        ifHit = false;
                    }
                }

            }


            if (ifHit)
            {
                //获取属性
                long actValue = attack_Act;
                //宠物普攻是魔法类型得用魔法值
                if (attackUnit.Type == UnitType.Pet && skillconfig.DamgeType == 2) {
                        actValue = attack_MageAct;
                }
                long defValue = defend_def;
                long adfValue = defend_adf;
                //获取重击等级  判定是否触发重击
                int zhongjiLvValue = numericComponentAttack.GetAsInt(NumericType.Numeric_Error);
                float zhongJiPro = numericComponentAttack.GetAsFloat(NumericType.Numeric_Error) + LvProChange(zhongjiLvValue, attackUnitLv);

                //重击阈值
                if (zhongJiPro > 0.75f) {
                    zhongJiPro = 0.75f;
                }

                if (RandomHelper.RandFloat() <= zhongJiPro)
                {
                    defValue = 0;
                    actValue += numericComponentAttack.GetAsLong(NumericType.Numeric_Error);
                    DamgeType = 3;

                    //重击对于怪物会额外附加一些伤害
                    if (attackUnit.Type == UnitType.Player && defendUnit.Type == UnitType.Monster) {
                        actValue =(long)(actValue * 1.2f);
                    }
                }

                //判定是否无视防御
                float wushiPro = numericComponentAttack.GetAsFloat(NumericType.Numeric_Error);
                if (RandomHelper.RandFloat() <= wushiPro)
                {
                    defValue = 0;
                    adfValue = 0;
                    DamgeType = 3;
                }

                //生命低于30%触发,防御提升X%
                float hptoDef = numericComponentDefend.GetAsFloat(NumericType.Numeric_Error);
                if (hptoDef > 0)
                {
                    float nowDefHpPro = (float)numericComponentDefend.GetAsInt(NumericType.Numeric_Error) / (float)numericComponentDefend.GetAsInt(NumericType.Numeric_Error);
                    if (nowDefHpPro <= 0.3f)
                    {
                        defValue = (long)(defValue * (1 + hptoDef));
                    }
                }

                long nowdef = defValue;

                //伤害类型 物理/魔法
                if (skillconfig.DamgeType == 2)
                {
                    nowdef = adfValue;
                }

                //技能加成
                if (skillconfig.SkillActType == 1)
                {
                    actValue += attack_MageAct;
                }

                //宠物远程攻击用魔法
                if (attackUnit.Type == UnitType.Pet && skillconfig.SkillActType == 1)
                {
                    actValue = attack_MageAct;
                }

                //宠物打怪物无视对方防御 150%攻击伤害
                if (attackUnit.Type == UnitType.Pet && defendUnit.Type == UnitType.Monster)
                {
                    nowdef = 0;
                    actValue = (int)(actValue * 1.5f);
                }

                //宠物打玩家无视目标50%的防御属性,防止不破防 攻击提升150%
                if (attackUnit.Type == UnitType.Pet && defendUnit.Type == UnitType.Player)
                {
                    nowdef = (int)(nowdef * 0.5f);
                    actValue = (int)(actValue * 1.5f);
                    //actValue = (int)(actValue * 1.5f);
                }

                //宠物打宠物计算伤害
                if (attackUnit.Type == UnitType.Pet && defendUnit.Type == UnitType.Pet)
                {
                    int attackPingfen = numericComponentAttack.GetAsInt(NumericType.PetPinFen);
                    int defPingfen = numericComponentDefend.GetAsInt(NumericType.PetPinFen);

                    if (attackPingfen == 0)
                    {
                        attackPingfen = 200;
                    }
                    if (defPingfen == 0)
                    {
                        defPingfen = 200;
                    }
                    actValue = (int)(actValue * (1 + GetFightValueActProValue(attackPingfen, defPingfen)));

                    //判断对方是否有神佑技能
                    bool haveshenyou = defendUnit.GetComponent<AIComponent>().HaveSkillId(80001014) || defendUnit.GetComponent<SkillPassiveComponent>().HaveSkillId(80002014);
                    if (haveshenyou) {
                        //低级破咒
                        if (attackUnit.GetComponent<AIComponent>().HaveSkillId(80001015)) {
                            actValue = (long)((float)actValue * 1.1f);
                        }
                        else if (attackUnit.GetComponent<SkillPassiveComponent>().HaveSkillId(80002015))
                        {
                            //高级破咒
                            actValue = (long)((float)actValue * 1.2f);
                        }
                    }
                    //defendUnit.GetComponent<>
                }

                //计算战斗公式
                long damge = (actValue - nowdef);

                //格挡值抵消
                damge = damge - numericComponentDefend.GetAsLong(NumericType.Numeric_Error);

                //查看对应武器
                float weaponAddAct = 0;
                switch (UnitHelper.GetEquipType(attackUnit))
                {
                    //刀
                    case 1:
                        weaponAddAct = numericComponentAttack.GetAsFloat(NumericType.Numeric_Error);
                        break;
                    //剑
                    case 2:
                        weaponAddAct = numericComponentAttack.GetAsFloat(NumericType.Numeric_Error);
                        break;
                    //法杖
                    case 3:
                        weaponAddAct = numericComponentAttack.GetAsFloat(NumericType.Numeric_Error);
                        break;
                    //魔法书
                    case 4:
                        weaponAddAct = numericComponentAttack.GetAsFloat(NumericType.Numeric_Error);
                        break;
                    //弓箭
                    case 5:
                        weaponAddAct = numericComponentAttack.GetAsFloat(NumericType.Numeric_Error);
                        break;
                }

                if (weaponAddAct >= 1f)
                {
                    weaponAddAct = 1f;
                }

                //武器伤害加成
                if (weaponAddAct > 0)
                {
                    damge = (long)((float)damge * (1f + weaponAddAct));
                }

                //怪物打宠物降低 （如果有需要 后期需要加入判定是不是当前怪物的普通攻击来判断躲避技能）
                if (attackUnit.Type == UnitType.Monster && defendUnit.Type == UnitType.Pet && petfuben == false)
                {
                    //普攻受到10%伤害
                    if (skillconfig.SkillActType == 0)
                    {
                        damge = (int)((float)damge * 0.1f);
                    }
                    
                    //技能受到0%伤害
                    if (skillconfig.SkillActType == 1)
                    {
                        damge = (int)((float)damge * 0.02f);
                        //damge = 0;
                    }
                }

                //怪物打玩家
                if (attackUnit.Type == UnitType.Monster && defendUnit.Type == UnitType.Player)
                {
                    //战士降低受到怪物普攻20%的伤害
                    if (defendUnit.GetComponent<UserInfoComponent>().UserInfo.Occ == 1)
                    {

                        if (skillconfig.SkillActType == 0)
                        {
                            damge = (int)((float)damge * 0.7f);
                        }

                    }

                    if (attackUnit.MasterIsPlayer())
                    {
                        //Log.Console("玩家召唤的怪物！");
                        damge = (int)((float)damge * 0.4f);
                    }

                }

              
                //宠物打宠物只造成50%的伤害
                if (attackUnit.Type == UnitType.Pet && defendUnit.Type == UnitType.Pet)
                {
                    damge = (int)((float)damge * 0.5f);
                    
                    //最低造成10%的伤害
                    int baodiValue = (int)((float)actValue * 0.1f);
                    if (damge < baodiValue) {
                        damge = baodiValue;
                    }
                }


                //玩家打宠物只保留10%的伤害,技能伤害(因为技能大多是百分比的)
                if (attackUnit.Type == UnitType.Player && defendUnit.Type == UnitType.Pet)
                {
                    //技能保留20%
                    if (skillconfig.SkillActType == 1)
                    {
                        damge = (int)((float)damge * 0.2f);
                    }

                    //普攻保留50%
                    if (skillconfig.SkillActType == 0)
                    {
                        //猎人保留40%普攻伤害
                        if (attackUnit.GetComponent<UserInfoComponent>().UserInfo.Occ == 3)
                        {
                            damge = (int)((float)damge * 0.4f);
                        }
                        else {
                            damge = (int)((float)damge * 0.5f);
                        }
                    }
                }

                //技能倍伤
                if (skillconfig.SkillActType == 1)
                {
                    nowdef = adfValue;
                }

                //魔法伤害无法被抵消是固定伤害,技能附带加成
                double skillProAdd = 0;
                if (skillconfig.SkillActType == 1)
                {
                    if (RandomHelper.RandFloat() <= numericComponentAttack.GetAsFloat(NumericType.Numeric_Error))
                    {
                        skillProAdd = 0.5f;
                    }
                }

                //获取技能相关系数
                double actDamge = skillconfig.ActDamge * singingvalue + buffHurtValueAdd + buffDamgePro;
                int actDamgeValue = skillconfig.DamgeValue;
                if (hurtMode == 1)  //持续伤害
                {
                    actDamge = skillconfig.DamgeChiXuPro;
                    actDamgeValue = skillconfig.DamgeChiXuValue;
                }

                //如果目标是怪物就附加怪物伤害
                if (defendUnit.Type == UnitType.Monster && skillconfig.MonsterActDamge != 0 ) {
                    actDamge += skillconfig.MonsterActDamge;
                }
                float defHpPro = (float)numericComponentDefend.GetAsInt(NumericType.Numeric_Error) / (float)numericComponentDefend.GetAsInt(NumericType.Numeric_Error);

                float hp_below_value = 0;
                float adddamage_value = 0f;
                List<float> adddamagebyhp = skillHandler.GetTianfuProAdd_2(SkillAttributeEnum.AddDamageByHpBelow);
                if (adddamagebyhp !=null && adddamagebyhp.Count >= 2)
                {
                    hp_below_value = adddamagebyhp[0];   //血量低于xx值  0.5
                    adddamage_value = adddamagebyhp[1];  //伤害提升xx值  0.5
                }

                damge = (long)(damge * (actDamge + skillHandler.ActTargetTemporaryAddPro + skillHandler.ActTargetAddPro + skillHandler.GetTianfuProAdd((int)SkillAttributeEnum.AddDamageCoefficient) + skillProAdd)) + actDamgeValue;

                float damgePro = 1;
                //伤害加成
                damge = (long)((float)damge * (1 + numericComponentAttack.GetAsFloat(NumericType.Numeric_Error) - numericComponentDefend.GetAsFloat(NumericType.Numeric_Error)));

                if (hp_below_value > 0f && hp_below_value < defHpPro)
                {
                    damge = (long)(damge * (1f + adddamage_value));
                }

                //物理伤害
                if (skillconfig.DamgeType == 1)
                {
                    damgePro = damgePro + numericComponentAttack.GetAsFloat(NumericType.Numeric_Error) - numericComponentDefend.GetAsFloat(NumericType.Numeric_Error);
                    if (ifMonsterBoss_Act && petfuben ==  false)
                    {
                        damgePro += numericComponentAttack.GetAsFloat(NumericType.Numeric_Error);
                        damge += numericComponentAttack.GetAsInt(NumericType.Numeric_Error);
                    }

                    if (ifMonsterBoss_Def)
                    {
                        damgePro -= numericComponentAttack.GetAsFloat(NumericType.Numeric_Error);
                    }

                    //物穿怪物加成
                    if (defendUnit.Type == UnitType.Monster)
                    {
                        damgePro += numericComponentAttack.GetAsFloat(NumericType.Numeric_Error) * 0.5f;
                    }

                    //魔导师分身普攻伤害加成
                    if (attackUnit.Type == UnitType.Monster && attackUnit.ConfigId == 90000001)
                    {
                        damgePro += numericComponentAttack.GetAsFloat(NumericType.Numeric_Error);
                    }
                }

                //技能伤害
                if (skillconfig.DamgeType == 2)
                {

                    damgePro = damgePro + numericComponentAttack.GetAsFloat(NumericType.Numeric_Error) - numericComponentDefend.GetAsFloat(NumericType.Numeric_Error);

                    if (ifMonsterBoss_Act && petfuben == false)
                    {
                        damgePro += numericComponentAttack.GetAsFloat(NumericType.Numeric_Error);
                        damge += numericComponentAttack.GetAsInt(NumericType.Numeric_Error);
                    }

                    if (ifMonsterBoss_Def)
                    {
                        damgePro -= numericComponentAttack.GetAsFloat(NumericType.Numeric_Error);
                    }

                    //魔穿怪物加成
                    if (defendUnit.Type == UnitType.Monster)
                    {
                        damgePro += numericComponentAttack.GetAsFloat(NumericType.Numeric_Error);
                    }
                }



                //是否触发斩杀
               
                if (defHpPro <= 0.3f)
                {
                    damgePro += numericComponentAttack.GetAsFloat(NumericType.Numeric_Error);
                }


                //破风之击如果目标血量低于50%，则提升50%伤害。 天赋增加这个效果 ID 50041
                //技能附加伤害
                if (!CommonHelper.IfNull(skillconfig.SkillDamgeAddValue))
                {
                    string[] skillAddValue = skillconfig.SkillDamgeAddValue.Split(',');
                    if (skillAddValue.Length >= 1)
                    {
                        switch (skillAddValue[0])
                        {
                            //当目标血量低于多少,技能伤害额外提升
                            case "1":
                                if (defHpPro <= float.Parse(skillAddValue[1])) {
                                    damgePro += float.Parse(skillAddValue[2]);
                                }
                                break;

                            //当自身血量低于多少,技能伤害额外提升
                            case "2":

                                float acthpPro = (float)numericComponentAttack.GetAsInt(NumericType.Numeric_Error) / (float)numericComponentAttack.GetAsInt(NumericType.Numeric_Error);
                                if (acthpPro <= float.Parse(skillAddValue[1]))
                                {
                                    damgePro += float.Parse(skillAddValue[2]);
                                }
                                break;

                            //当目标血量高于多少,技能伤害额外提升
                            case "3":
                                if (defHpPro >= float.Parse(skillAddValue[1]))
                                {
                                    damgePro += float.Parse(skillAddValue[2]);
                                }
                                break;
                            //当自身血量高于多少,技能伤害额外提升
                            case "4":
                                acthpPro = (float)numericComponentAttack.GetAsInt(NumericType.Numeric_Error) / (float)numericComponentAttack.GetAsInt(NumericType.Numeric_Error);
                                if (acthpPro >= float.Parse(skillAddValue[1]))
                                {
                                    damgePro += float.Parse(skillAddValue[2]);
                                }
                                break;
                        }
                    }
                }


                //普攻加成
                if (skillconfig.SkillActType == 0)
                {
                    //普攻属性加成
                    damgePro += numericComponentAttack.GetAsFloat(NumericType.Numeric_Error);

                    //血量降低转换普攻伤害
                    float hpDamgePro = numericComponentAttack.GetAsFloat(NumericType.Numeric_Error);
                    if (hpDamgePro > 0)
                    {
                        float acthpPro = (float)numericComponentAttack.GetAsInt(NumericType.Numeric_Error) / (float)numericComponentAttack.GetAsInt(NumericType.Numeric_Error);
                        if (acthpPro < 1 && acthpPro > 0)
                        {
                            if (acthpPro >= 0.6f)
                            {
                                //大于0.5
                                damgePro += (1f - acthpPro) / 4 * hpDamgePro;
                            }
                            else if (acthpPro >= 0.3f)
                            {
                                damgePro += (1f - acthpPro) / 2f * hpDamgePro;
                            }
                            else
                            {
                                damgePro += (1f - acthpPro) / 1.5f * hpDamgePro;
                            }
                        }
                    }
                }

                //血量转换加成  （每10%转化成一定攻击值）
                float hpToDamgeAddPro2 = numericComponentAttack.GetAsFloat(NumericType.Numeric_Error);
                if (hpToDamgeAddPro2 > 0)
                {
                    //血量降低转换普攻伤害
                    float acthpPro = (float)numericComponentAttack.GetAsInt(NumericType.Numeric_Error) / (float)numericComponentAttack.GetAsInt(NumericType.Numeric_Error);
                    int toValue = (int)((1f - acthpPro) * 10f);
                    if (toValue >= 1 && toValue <= 10)
                    {
                        damgePro += hpToDamgeAddPro2 * toValue;
                    }
                }

                //抗性
                switch (skillconfig.DamgeElementType)
                {
                    //光     神圣抗性
                    case 1:
                        damgePro = damgePro - numericComponentDefend.GetAsFloat(NumericType.Numeric_Error);
                        break;
                    //暗     暗影抗性
                    case 2:
                        damgePro = damgePro - numericComponentDefend.GetAsFloat(NumericType.Numeric_Error);
                        break;
                    //火     火焰抗性
                    case 3:
                        damgePro = damgePro - numericComponentDefend.GetAsFloat(NumericType.Numeric_Error);
                        break;
                    //水     冰霜抗性
                    case 4:
                        damgePro = damgePro - numericComponentDefend.GetAsFloat(NumericType.Numeric_Error);
                        break;
                    //电     闪电抗性
                    case 5:
                        damgePro = damgePro - numericComponentDefend.GetAsFloat(NumericType.Numeric_Error);
                        break;
                }

                //种族抗性
                if (ifMonsterBoss_Act)
                {
                    switch (MonsterConfigCategory.Instance.Get(defendUnit.ConfigId).MonsterRace)
                    {
                        //通用
                        case 0:
                            break;
                        //野兽
                        case 1:
                            damgePro = damgePro - numericComponentDefend.GetAsFloat(NumericType.Numeric_Error);
                            break;
                        //人类
                        case 2:
                            damgePro = damgePro - numericComponentDefend.GetAsFloat(NumericType.Numeric_Error);
                            break;
                        //恶魔
                        case 3:
                            damgePro = damgePro - numericComponentDefend.GetAsFloat(NumericType.Numeric_Error);
                            break;
                    }
                }

                //种族伤害
                if (ifMonsterBoss_Def)
                {
                    switch (MonsterConfigCategory.Instance.Get(attackUnit.ConfigId).MonsterRace)
                    {
                        //通用
                        case 0:
                            break;
                        //野兽
                        case 1:
                            damgePro = damgePro + numericComponentAttack.GetAsFloat(NumericType.Numeric_Error);
                            break;
                        //人类
                        case 2:
                            damgePro = damgePro + numericComponentAttack.GetAsFloat(NumericType.Numeric_Error);
                            break;
                        //恶魔
                        case 3:
                            damgePro = damgePro + numericComponentAttack.GetAsFloat(NumericType.Numeric_Error);
                            break;
                    }
                }

                //pk相关
                if (playerPKStatus)
                {
                    //actDamgeValue -= (int)(actDamgeValue * 0.4f);

                    //damgePro -= numericComponentDefend.GetAsFloat(NumericType.Numeric_Error);

                    bool jueXinSkill = false;
                    if (CommonConfig.JueXingSkillIDList.Contains(skillHandler.SkillConf.Id))
                    {
                        jueXinSkill = true;
                    }
                    else
                    {
                        //获取觉醒ID
                        int juexingid = 0;
                        if (attackUnit.Type == UnitType.Player)
                        {
                            int occtwo = attackUnit.GetComponent<UserInfoComponent>().UserInfo.OccTwo;
                            if (occtwo != 0)
                            {
                                Occupation_Transfer occupationConfig = Occupation_TransferCategory.Instance.Get(occtwo);
                                juexingid = occupationConfig.JueXingSkill[7];
                            }
                        }

                        jueXinSkill = juexingid != 0 && juexingid == skillHandler.SkillConf.Id;
                    }

                    //普通攻击降低
                    /*
                    if (skillconfig.SkillActType == 0)
                    {
                        damgePro -= numericComponentDefend.GetAsFloat(NumericType.Numeric_Error);
                    }

                    //技能伤害降低
                    if (skillconfig.SkillActType == 1)
                    {
                        damgePro -= numericComponentDefend.GetAsFloat(NumericType.Numeric_Error);
                    }
                    */
                    //根据双方战力调整系数
                    if (attackUnit.Type == UnitType.Player && defendUnit.Type == UnitType.Player)
                    {
                        damgePro += GetFightValueActProValue(attackUnit.GetComponent<UserInfoComponent>().UserInfo.Combat, defendUnit.GetComponent<UserInfoComponent>().UserInfo.Combat);
                    }

                    //系数类的百分比加减乘放在后面

                    //觉醒技能伤害减半
                    if (jueXinSkill)
                    {
                        damgePro = damgePro / 2;
                    }

                    //玩家之间PK伤害降低,普通攻击降低40%,技能伤害降低20%
                    //普通攻击
                    if (skillconfig.SkillActType == 0 && damgePro > 0)
                    {
                        damgePro = damgePro * 0.25f;
                    }

                    //技能攻击
                    if (skillconfig.SkillActType == 1 && damgePro > 0)
                    {
                        damgePro = damgePro * 0.15f;
                    }

                    //----------生命之盾相关-----------

                    //普通攻击降低
                    if (skillconfig.SkillActType == 0)
                    {
                        float PlayerActDamgeSubPro = numericComponentDefend.GetAsFloat(NumericType.Numeric_Error);
                        if (PlayerActDamgeSubPro >= 0.75f)
                        {
                            PlayerActDamgeSubPro = 0.75f;
                        }

                        //降低受到玩家全部攻击伤害比例
                        damgePro = damgePro * (1 - PlayerActDamgeSubPro);
                    }

                    //技能伤害降低
                    if (skillconfig.SkillActType == 1)
                    {
                        float PlayerSkillDamgeSubPro = numericComponentDefend.GetAsFloat(NumericType.Numeric_Error);
                        if (PlayerSkillDamgeSubPro >= 0.75f)
                        {
                            PlayerSkillDamgeSubPro = 0.75f;
                        }

                        //降低受到玩家全部攻击伤害比例
                        damgePro = damgePro * (1 - PlayerSkillDamgeSubPro);
                    }

                    float PlayerAllDamgeSubPro = numericComponentDefend.GetAsFloat(NumericType.Numeric_Error);
                    if (PlayerAllDamgeSubPro >= 0.75f) {
                        PlayerAllDamgeSubPro = 0.75f;
                    }

                    //降低受到玩家全部攻击伤害比例
                    damgePro = damgePro * (1 - PlayerAllDamgeSubPro);
                }

                damgePro = damgePro < 0 ? 0 : damgePro;
                damge = (int)(damge * damgePro);

                //格挡值抵消
                //damge = damge - numericComponentDefend.GetAsLong(NumericType.Numeric_Error);

                if (damge < 1)
                {
                    damge = 1;
                }

                //真实伤害
                damge += numericComponentAttack.GetAsLong(NumericType.Numeric_Error);

                damge += (long)skillHandler.GetTianfuProAdd((int)SkillAttributeEnum.AddDamageValue);

                //二次限定
                if (damge < 1)
                {
                    damge = 1;
                }
                
                //2293987578036158464  入梦
                if (skillconfig.SkillActType == 1  && GMHelp.DebugPlayerList.ContainsKey(attackUnit.Id))
                {
                    Log.Warning($"玩家({GMHelp.DebugPlayerList[attackUnit.Id]})造成伤害   技能:{skillconfig.Name}  伤害:{damge}");
                }
                if (skillconfig.SkillActType == 1 && GMHelp.DebugPlayerList.ContainsKey(attackUnit.MasterId))
                {
                    Log.Warning($"玩家({GMHelp.DebugPlayerList[attackUnit.MasterId]})宠物伤害  技能:{skillconfig.Name}  伤害:{damge}");
                }

                if (defendUnit.Type == UnitType.Player && GMHelp.DebugPlayerList.ContainsKey(defendUnit.Id))
                {
                    Log.Warning($"玩家({GMHelp.DebugPlayerList[defendUnit.Id]})对手伤害   技能ID:{skillconfig.Id} 技能:{skillconfig.Name}  伤害:{damge}");
                }


                //存储是为万为单位的
                //damge = (damge / 10000 * 10000);
                if (damge > 0)
                {
                    //等级换算最终属性
                    //float addCriPro = numericComponentAttack.GetAsFloat(NumericType.Numeric_Error) + LvProChange(numericComponentAttack.GetAsLong(NumericType.Numeric_Error), defendUnitLv);
                    //float addResPro = numericComponentDefend.GetAsFloat(NumericType.Numeric_Error) + LvProChange(numericComponentDefend.GetAsLong(NumericType.Numeric_Error), attackUnitLv);
                    float addCriPro = numericComponentAttack.GetAsFloat(NumericType.Numeric_Error);
                    float addResPro = numericComponentDefend.GetAsFloat(NumericType.Numeric_Error);

                    float addCriLvPro = LvProChange(numericComponentAttack.GetAsLong(NumericType.Numeric_Error), defendUnitLv);
                    float addResLvPro = LvProChange(numericComponentAttack.GetAsLong(NumericType.Numeric_Error), attackUnitLv);

                    addCriPro += addCriLvPro;
                    addResPro += addResLvPro;

                    float CriPro = addCriPro + attackPet_cri - addResPro;

                    //pk命中
                    if (playerPKStatus)
                    {
                        CriPro -= numericComponentDefend.GetAsFloat(NumericType.Numeric_Error);
                        CriPro += numericComponentAttack.GetAsFloat(NumericType.Numeric_Error);
                    }

                    //根据双方战力调整暴击系数
                    if (attackUnit.Type == UnitType.Player && defendUnit.Type == UnitType.Player)
                    {
                        CriPro += GetFightValueCriAndHitProValue(attackUnit.GetComponent<UserInfoComponent>().UserInfo.Combat, defendUnit.GetComponent<UserInfoComponent>().UserInfo.Combat);
                    }


                    if (CriPro <= 0f)
                    {
                        CriPro = 0;
                    }

                    //判断当前是否时暴击状态
                    if (attackUnit.GetComponent<StateComponent>().StateTypeGet(StateTypeEnum.CriStatus) == true)
                    {
                        CriPro = 1;

                        BuffManagerComponent buffManagerComponent = attackUnit.GetComponent<BuffManagerComponent>();

                        if (buffManagerComponent.GetCritBuffNumber() <= 1)
                        {
                            attackUnit.GetComponent<StateComponent>().StateTypeRemove(StateTypeEnum.CriStatus);
                        }
                        buffManagerComponent.RemoveFirstCritBuff();
                    }

                    //暴击概率..
                    if (RandomHelper.RandFloat() <= CriPro)
                    {
                        DamgeType = 1;
                        float criDamge = 1.7f + numericComponentAttack.GetAsFloat(NumericType.Numeric_Error) + numericComponentDefend.GetAsFloat(NumericType.Numeric_Error);
                        damge = (long)((float)damge * criDamge);



                        //Log.Debug("暴击了!");

                        //闪避触发被动技能
                        attackUnit.GetComponent<SkillPassiveComponent>().OnTrigegerPassiveSkill(SkillPassiveTypeEnum.Critical_4, defendUnit.Id);

                        // 普通攻击暴击触发19
                        if (skillconfig.SkillActType == 0)
                        {
                            attackUnit.GetComponent<SkillPassiveComponent>().OnTrigegerPassiveSkill(SkillPassiveTypeEnum.AckCritical_19, defendUnit.Id);
                        }
                    }


                    //是否触发秒杀
                    if (defHpPro <= 0.2f)
                    {
                        float miaoshaPro = numericComponentAttack.GetAsFloat(NumericType.Numeric_Error);
                        if (RandomHelper.RandFloat01() < miaoshaPro)
                        {
                            damge += numericComponentDefend.GetAsInt(NumericType.Numeric_Error);
                        }
                    }

                    int shield_Hp = numericComponentDefend.GetAsInt(NumericType.Now_Shield_HP);
                    float shield_pro = numericComponentDefend.GetAsFloat(NumericType.Now_Shield_DamgeCostPro);
                    if (shield_Hp > 0)
                    {
                        int dunDamge = (int)((float)damge * shield_pro);
                        damge -= dunDamge;
                        damge = Math.Max(0, damge);
                        numericComponentDefend.ApplyChange(attackUnit, NumericType.Now_Shield_HP, -1 * dunDamge, skillconfig.Id, true, DamgeType);
                    }

                    //吸血处理(普通攻击触发吸血)
                    if (skillconfig.SkillActType == 0)
                    {
                        float hushi = numericComponentAttack.GetAsFloat(NumericType.Numeric_Error);
                        if (hushi > 0f)
                        {
                            int addHp = (int)((float)damge * hushi);
                            numericComponentAttack.ApplyChange(attackUnit, NumericType.Numeric_Error, addHp, 0);
                        }
                    }

                    //普攻和技能吸血
                    float xixueAll = numericComponentAttack.GetAsFloat(NumericType.Numeric_Error);
                    if (xixueAll > 0f)
                    {
                        int addHp = (int)((float)damge * xixueAll);
                        numericComponentAttack.ApplyChange(attackUnit, NumericType.Numeric_Error, addHp, 0);
                    }

                    damge *= -1;
                }
                if (defendUnit.IsDisposed)
                {
                    return false;
                }
                if (defendUnit.Type == UnitType.Monster && ifMonsterBoss_Act)
                {
                    defendUnit.GetComponent<AttackRecordComponent>().BeAttacking(attackUnit, damge);
                }

                //即将死亡
                if (numericComponentDefend.GetAsInt(NumericType.Numeric_Error) + damge <= 0)
                {
                    //判定是否复活
                    if (RandomHelper.RandFloat01() < numericComponentDefend.GetAsFloat(NumericType.Numeric_Error))
                    {
                        //复活存在30%的血量
                        defendUnit.GetComponent<BuffManagerComponent>().UpdateFuHuoStatus();
                        numericComponentDefend.ApplyChange(null, NumericType.Numeric_Error, (int)(numericComponentAttack.GetAsInt(NumericType.Numeric_Error) * 0.3f), 0);
                    }
                    else if (RandomHelper.RandFloat01() < numericComponentDefend.GetAsFloat(NumericType.Numeric_Error))
                    {
                        //神佑存在100%的血量
                        defendUnit.GetComponent<BuffManagerComponent>().UpdateFuHuoStatus();
                        numericComponentDefend.ApplyChange(null, NumericType.Numeric_Error, (int)(numericComponentAttack.GetAsInt(NumericType.Numeric_Error) * 1f), 0);
                    }
                    else
                    {
                        //死亡
                    }
                }
                //普通攻击反弹伤害
                if (numericComponentDefend.GetAsFloat(NumericType.Numeric_Error) > 0 && skillconfig.DamgeType == 1)
                {
                    int fantanValue = (int)((float)damge * numericComponentDefend.GetAsFloat(NumericType.Numeric_Error));
                    numericComponentAttack.ApplyChange(attackUnit, NumericType.Numeric_Error, fantanValue, skillconfig.Id, true, DamgeType);
                }
                if (attackUnit.IsDisposed == false)
                {
                    //设置目标当前
                    numericComponentDefend.ApplyChange(attackUnit, NumericType.Numeric_Error, damge, skillconfig.Id, true, DamgeType);

                    //攻击方反弹即将死亡
                    if (numericComponentAttack.GetAsInt(NumericType.Numeric_Error) <= 0)
                    {
                    }
                }
            }
            else
            {
                //设置伤害为0,用于伤害飘字
                long now_hp = numericComponentDefend.GetAsLong(NumericType.Numeric_Error);
                numericComponentDefend.ApplyValue(attackUnit, NumericType.Numeric_Error, now_hp, 0);

                //闪避触发被动技能
                defendUnit.GetComponent<SkillPassiveComponent>().OnTrigegerPassiveSkill(SkillPassiveTypeEnum.ShanBi_5, attackUnit.Id);
            }
            return ifHit;
        }

        //暴击等级等属性转换成实际暴击率的方法
        public static float LvProChange(long value, int lv)
        {
            float proValue = (float)value / (float)(7500 + lv * 250);
            if (proValue < 0)
            {
                proValue = 0;
            }
            if (proValue > 0.75f)
            {
                proValue = 0.75f;
            }
            return proValue;
        }

        //根据双方战力比调整攻击系数，攻击者打弱势有额外的攻击加成
        public static float GetFightValueActProValue(int actFightValue, int defFightValue)
        {

            float addPro = ((actFightValue / defFightValue) - 1) * 1.5f;

            //范围限制
            if (addPro < 0)
            {
                addPro = 0;
            }

            //addPro = addPro + 0.05f;
            if (addPro > 0.75f)
            {
                addPro = 0.75f;
            }

            return addPro;

        }

        //根据双方战力比调整攻击系数，攻击者打弱势有额外的命中和攻击
        public static float GetFightValueCriAndHitProValue(int actFightValue, int defFightValue)
        {

            float addPro = ((actFightValue / defFightValue) - 1) * 1.5f;

            //范围限制
            if (addPro < 0)
            {
                addPro = 0;
            }

            //addPro = addPro + 0.05f;
            if (addPro > 0.2f)
            {
                addPro = 0.2f;
            }

            return addPro;

        }

        //字典是引用,进来的值会发生改变
        public static void AddUpdateProDicList(int typeID, long typeValue, Dictionary<int, long> dic)
        {
            //缓存属性
            if (dic.ContainsKey(typeID))
            {
                dic[typeID] += typeValue;
            }
            else
            {
                dic[typeID] = typeValue;
            }

        }

        //是否是一级属性
        public static bool ifNumTypeOnePro(int numericType)
        {

            if (numericType < (int)NumericType.Max)
            {
                numericType = numericType * 100;
            }
            int nowValue = (int)numericType / 100;
            if (nowValue == NumericType.Numeric_Error || nowValue == NumericType.Numeric_Error || nowValue == NumericType.Numeric_Error || nowValue == NumericType.Numeric_Error || nowValue == NumericType.Numeric_Error)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        /// <summary>
        /// 大恶魔  ...血量提升30倍,攻击提升200%，移动速度变为10，自身会变成恶魔模型
        /// </summary>
        /// <param name="unit"></param>
        /// <param name="notice"></param>
        public void UnitUpdateProperty_DemonBig(Unit unit, bool notice)
        {
            NumericComponent numericComponent = unit.GetComponent<NumericComponent>();

            numericComponent.Set(NumericType.Numeric_Error, 0, notice);
            numericComponent.Set(NumericType.Numeric_Error, 0, notice);
            numericComponent.Set(NumericType.Numeric_Error, 0, notice);
            numericComponent.Set(NumericType.Numeric_Error, 0, notice);
            numericComponent.Set(NumericType.Numeric_Error, 100000, notice);

            ///可以修改属性乘法 属性附属乘法.     
            //numericComponent.Set(NumericType.Numeric_Error, 0, notice);
        }

        /// <summary>
        /// 小恶魔
        /// </summary>
        /// <param name="unit"></param>
        /// <param name="notice"></param>
        public void UnitUpdateProperty_DemonLittle(Unit unit, bool notice)
        {
            NumericComponent numericComponent = unit.GetComponent<NumericComponent>();

            numericComponent.Set(NumericType.Numeric_Error, 0, notice);
            numericComponent.Set(NumericType.Numeric_Error, 0, notice);
            numericComponent.Set(NumericType.Numeric_Error, 0, notice);
            numericComponent.Set(NumericType.Numeric_Error, 0, notice);
            numericComponent.Set(NumericType.Numeric_Error, 80000, notice);
        }

        /// <summary>
        /// 幽灵
        /// </summary>
        /// <param name="unit"></param>
        /// <param name="notice"></param>
        public void UnitUpdateProperty_DemonGhost(Unit unit, bool notice)
        {
            NumericComponent numericComponent = unit.GetComponent<NumericComponent>();

            numericComponent.Set(NumericType.Numeric_Error, 0, notice);
            numericComponent.Set(NumericType.Numeric_Error, 0, notice);
            numericComponent.Set(NumericType.Numeric_Error, 0, notice);
            numericComponent.Set(NumericType.Numeric_Error, 0, notice);
            numericComponent.Set(NumericType.Numeric_Error, 50000, notice);
        }


        /// <summary>
        /// 奔跑大赛属性
        /// </summary>
        /// <param name="unit"></param>
        /// <param name="notice"></param>
        public void UnitUpdateProperty_RunRace(Unit unit, bool notice)
        {
            NumericComponent numericComponent = unit.GetComponent<NumericComponent>();

            int monsterid = numericComponent.GetAsInt(NumericType.RunRaceTransform);
            MonsterConfig monsterConfig = MonsterConfigCategory.Instance.Get(monsterid);
            numericComponent.Set(NumericType.Numeric_Error, 0, notice);
            numericComponent.Set(NumericType.Numeric_Error, 0, notice);
            numericComponent.Set(NumericType.Numeric_Error, 0, notice);
            numericComponent.Set(NumericType.Numeric_Error, 0, notice);
            numericComponent.Set(NumericType.Numeric_Error, (float)monsterConfig.MoveSpeed, notice);
        }

        /// <summary>
        /// 更新基础的属性
        /// </summary>
        /// <param name="unit"></param>
        public void UnitUpdateProperty_Base(Unit unit, bool notice, bool rank)
        {
            if (unit.SceneType == MapTypeEnum.RunRace)
            {
                return;
            }

            //基础职业属性
            UserInfoComponent UnitInfoComponent = unit.GetComponent<UserInfoComponent>();
            UserInfo userInfo = UnitInfoComponent.UserInfo;
            int roleLv = userInfo.Lv;

            //初始化属性
            NumericComponent numericComponent = unit.GetComponent<NumericComponent>();
            //numericComponent.ResetProperty();

            
            Dictionary<int, long> allprodic = numericComponent.NumericDic;
            foreach (int key in allprodic.Keys)
            {
                //这个范围内的属性为特殊属性不进行重置
                /*if (key >= NumericType.Numeric_Error && key < NumericType.Max)
                {
                    continue;
                }*/

                //buff属性重新计算
                /*int yushu = key % 100;
                //203011, 206511
                if (key == 203011 || key == 206511)  ///暂时先处理这两个
                ///if (yushu == 11 || yushu == 12)
                {
                    long ovalue = allprodic[key];
                    numericComponent.Set(key, 0, false);
                    numericComponent.Set(key, ovalue, false);
                }*/
            }

            //缓存列表
            Dictionary<int, long> UpdateProDicList = new Dictionary<int, long>();

            //属性点
            int PointLiLiang = numericComponent.GetAsInt(NumericType.Point_LiLiang);
            int PointZhiLi = numericComponent.GetAsInt(NumericType.Point_ZhiLi);
            int PointTiZhi = numericComponent.GetAsInt(NumericType.Point_TiZhi);
            int PointNaiLi = numericComponent.GetAsInt(NumericType.Point_NaiLi);
            int PointMinJie = numericComponent.GetAsInt(NumericType.Point_MinJie);
            
            //职业属性
            List<HideProList> occInitAttribute = OccupationCategory.Instance.GetOccInitAttribute(userInfo.Occ);
            //装备属性
            unit.GetComponent<BagComponent>().GetEquipAttribute(occInitAttribute);
            
            for (int pro = 0; pro < occInitAttribute.Count; pro++)
            {
                AddUpdateProDicList(occInitAttribute[pro].HideID, occInitAttribute[pro].HideValue, UpdateProDicList);
            }
            
            //时装
            List<int> fashionids = unit.GetComponent<BagComponent>().FashionActiveIds;
            for (int i = 0; i < fashionids.Count; i++)
            {
                if (!FashionConfigCategory.Instance.Contain(fashionids[i]))
                {
                    continue;
                }

                FashionConfig fashionConfig = FashionConfigCategory.Instance.Get(fashionids[i]);
                if (fashionConfig.PropertyKey == null || fashionConfig.PropertyKey.Length == 0 || fashionConfig.PropertyKey[0] == 0)
                {
                    continue;
                }

                for (int pro = 0; pro < fashionConfig.PropertyKey.Length; pro++ )
                {
                    AddUpdateProDicList(fashionConfig.PropertyKey[pro], fashionConfig.PropertyValue[pro], UpdateProDicList);
                }
            }
            
            //史诗宝石数量
            List<int> ShiShiGemID = new List<int>();

            //生命护盾
            List<PropertyValue> lifeShieldList = unit.GetComponent<SkillSetComponent>().GetShieldProLists();
            for (int i = 0; i < lifeShieldList.Count; i++)
            {
                AddUpdateProDicList(lifeShieldList[i].HideID, lifeShieldList[i].HideValue, UpdateProDicList);
            }

            //称号属性
            List<PropertyValue> titlePros = unit.GetComponent<TitleComponent>().GetTitlePro();
            for (int i = 0; i < titlePros.Count; i++)
            {
                AddUpdateProDicList(titlePros[i].HideID, titlePros[i].HideValue, UpdateProDicList);
            }

            //家园属性
            List<PropertyValue> jiayuanPros = unit.GetComponent<JiaYuanComponent>().GetJianYuanPro();
            for (int i = 0; i < jiayuanPros.Count; i++)
            {
                AddUpdateProDicList(jiayuanPros[i].HideID, jiayuanPros[i].HideValue, UpdateProDicList);
            }

            //技能属性
            List<PropertyValue> skillProList = unit.GetComponent<SkillSetComponent>().GetSkillRoleProLists();
            for (int i = 0; i < skillProList.Count; i++)
            {
                //Log.Info("隐藏:" + skillProList[i].HideID + "skillProList[i].HideValue = " + skillProList[i].HideValue);
                AddUpdateProDicList(skillProList[i].HideID, skillProList[i].HideValue, UpdateProDicList);
            }

            //坐骑属性
            List<PropertyValue> zuoqiPros = unit.GetComponent<UserInfoComponent>().GetZuoQiPro();
            for (int i = 0; i < zuoqiPros.Count; i++)
            {
                AddUpdateProDicList(zuoqiPros[i].HideID, zuoqiPros[i].HideValue, UpdateProDicList);
            }

            //收集属性
            List<PropertyValue> shoujiProList = unit.GetComponent<ShoujiComponent>().GetProList();
            for (int i = 0; i < shoujiProList.Count; i++)
            {
                AddUpdateProDicList(shoujiProList[i].HideID, shoujiProList[i].HideValue, UpdateProDicList);
            }

            //精灵属性
            List<PropertyValue> jinglingProList = unit.GetComponent<ChengJiuComponent>().GetJingLingProLists();
            for (int i = 0; i < jinglingProList.Count; i++)
            {
                AddUpdateProDicList(jinglingProList[i].HideID, jinglingProList[i].HideValue, UpdateProDicList);
            }

            List<PropertyValue> magickaProList = unit.GetComponent<ChengJiuComponent>().GetMagickaProLists();
            for (int i = 0; i < magickaProList.Count; i++)
            {
                AddUpdateProDicList(magickaProList[i].HideID, magickaProList[i].HideValue, UpdateProDicList);
            }

            //神兽羁绊属性
            int shenshouNumber = unit.GetComponent<PetComponent>().GetShenShouNumber();
            List<PropertyValue> shenshoujiban = new List<PropertyValue>();
            foreach ((int petnumber, List<PropertyValue> prolist) in CommonConfig.ShenShouJiBan)
            {
                if (shenshouNumber >= petnumber)
                {
                    shenshoujiban.AddRange(prolist);
                }
            }

            for (int i = 0; i < shenshoujiban.Count; i++)
            {
                AddUpdateProDicList(shenshoujiban[i].HideID, shenshoujiban[i].HideValue, UpdateProDicList);
            }
            
            //家园守护
            /*List<PropertyValue> shouhuPros = unit.GetComponent<PetComponent>().GetPetShouHuPro();
            for (int i = 0; i < shouhuPros.Count; i++)
            {
                AddUpdateProDicList(shouhuPros[i].HideID, shouhuPros[i].HideValue, UpdateProDicList);
            }*/

            //天赋系统
            List<PropertyValue> tianfuProList = unit.GetComponent<SkillSetComponent>().GetTianfuRoleProLists();
            for (int i = 0; i < tianfuProList.Count; i++)
            {
                AddUpdateProDicList(tianfuProList[i].HideID, tianfuProList[i].HideValue, UpdateProDicList);
            }
            
            //--------------------新版属性加点------------------------

            long Power_value_add = 0;
            long Intellect_value_add = 0;
            long Agility_value_add = 0;
            long Stamina_value_add = 0;
            long Constitution_value_add = 0;
            int Power_value = 0;
            int Intellect_value = 0;
            int Agility_value = 0;
            int Stamina_value = 0;
            int Constitution_value = 0;

            //力量加物理穿透
            int wuliChuanTouLv = (PointLiLiang + (int)Power_value + (int)Power_value_add) * 5;
            float adddWuLiChuanTou = LvProChange(wuliChuanTouLv, roleLv);
            AddUpdateProDicList((int)NumericType.PATK_Max, (int)(adddWuLiChuanTou * 10000), UpdateProDicList);

            //智力加魔法穿透
            int mageChuanTouLv = (PointZhiLi + (int)Intellect_value + (int)Intellect_value_add) * 5;
            float adddMageChuanTou = LvProChange(mageChuanTouLv, roleLv);
            AddUpdateProDicList((int)NumericType.PATK_Max, (int)(adddMageChuanTou * 10000), UpdateProDicList);

            //敏捷冷却时间
            int cdTimeLv = (PointMinJie + (int)Agility_value + (int)Agility_value_add) * 2;
            float addMinJie = LvProChange(cdTimeLv, roleLv);
            AddUpdateProDicList((int)NumericType.PATK_Max, (int)(addMinJie * 10000), UpdateProDicList);

            //耐力
            int huixueLv = (PointNaiLi + (int)Stamina_value + (int)Stamina_value_add);
            AddUpdateProDicList((int)NumericType.PATK_Max, huixueLv, UpdateProDicList);

            //体力
            int damgeProCostLv = (PointTiZhi + (int)Constitution_value + (int)Constitution_value_add) * 2;
            float damgeProCost = LvProChange(damgeProCostLv, roleLv);
            AddUpdateProDicList((int)NumericType.PATK_Max, (int)(damgeProCost * 10000), UpdateProDicList);

            //攻击部分

            List<int> keys = new List<int>();

            //更新属性
            foreach (int key in UpdateProDicList.Keys)
            {
                long setValue = numericComponent.GetAsLong(key) + UpdateProDicList[key];

                if (!notice)
                {
                    numericComponent.Update(key, setValue, false);
                    continue;
                }
                if (NumericHelp.BroadcastType.Contains(key))
                {
                    numericComponent.Update(key, setValue, true);
                }
                else
                {
                    numericComponent.Update(key, setValue, false);
                    keys.Add(key);
                }
            }

            if (notice)
            {
                List<int> ks = new List<int>();
                List<long> vs = new List<long>();

                for (int i = 0; i < keys.Count; i++)
                {
                    int nowValue = (int)keys[i] / 100;
                    if (!ks.Contains(nowValue))
                    {
                        ks.Add(nowValue);
                        vs.Add(numericComponent.GetAsLong(nowValue));
                    }
                }

                //通知自己
                m2C_UnitNumericListUpdate.UnitID = unit.Id;
                m2C_UnitNumericListUpdate.Vs = vs;
                m2C_UnitNumericListUpdate.Ks = ks;
                MessageHelper.SendToClient(unit, m2C_UnitNumericListUpdate);
            }

            UpdateCombat(unit, numericComponent,notice);
            
            //排行榜
            if (rank)
            {
                unit.GetComponent<UserInfoComponent>().UpdateRankInfo();
            }
            
        }
        
        public void UpdateCombat(Unit unit, NumericComponent numericComponent, bool notice)
        {
            //战力计算
            long ShiLi_Act = 0;
            float ShiLi_ActPro = 0f;
            long ShiLi_Def = 0;
            float ShiLi_DefPro = 0f;
            long ShiLi_Hp = 0;
            float ShiLi_HpPro = 0f;
            //long proLvAdd = criLv + hitLv + dodgeLv + resLv + skillAddLv;
            long proLvAdd = 0;

            //传承鉴定特殊属性加成
            int chuanchengProAdd = 0;
        
            //攻击部分
            foreach (var Item in NumericHelp.ZhanLi_Act)
            {
                ShiLi_Act += (int)((float)numericComponent.ReturnGetFightNumLong(Item.Key) * Item.Value);
            }

            //隐藏技能算在攻击部分

            foreach (var Item in NumericHelp.ZhanLi_ActPro)
            {
                ShiLi_ActPro += ((float)numericComponent.ReturnGetFightNumfloat(Item.Key) * Item.Value);
            }

            //Console.WriteLine("ShiLi_ActPro = " + ShiLi_ActPro);

            //幸运副本附加
            int luck = numericComponent.GetAsInt(NumericType.Numeric_Error);
            switch (luck)
            {
                case 0:
                    ShiLi_ActPro += 0.01f;
                    break;
                case 1:
                    ShiLi_ActPro += 0.02f;
                    break;
                case 2:
                    ShiLi_ActPro += 0.04f;
                    break;
                case 3:
                    ShiLi_ActPro += 0.08f;
                    break;
                case 4:
                    ShiLi_ActPro += 0.12f;
                    break;
                case 5:
                    ShiLi_ActPro += 0.2f;
                    break;
                case 6:
                    ShiLi_ActPro += 0.3f;
                    break;
                case 7:
                    ShiLi_ActPro += 0.4f;
                    break;
                case 8:
                    ShiLi_ActPro += 0.5f;
                    break;
                case 9:
                    ShiLi_ActPro += 0.9f;
                    break;

                default:
                    ShiLi_ActPro += 1f;
                    break;
            }

            //防御部分
            foreach (var Item in NumericHelp.ZhanLi_Def)
            {
                ShiLi_Def += (int)((float)numericComponent.ReturnGetFightNumLong(Item.Key) * Item.Value);
            }

            foreach (var Item in NumericHelp.ZhanLi_DefPro)
            {
                ShiLi_DefPro += ((float)numericComponent.ReturnGetFightNumfloat(Item.Key) * Item.Value);
            }

            //血量部分
            foreach (var Item in NumericHelp.ZhanLi_Hp)
            {
                ShiLi_Hp += (int)((float)numericComponent.ReturnGetFightNumLong(Item.Key) * Item.Value);
            }

            foreach (var Item in NumericHelp.ZhanLi_HpPro)
            {
                ShiLi_HpPro += ((float)numericComponent.ReturnGetFightNumfloat(Item.Key) * Item.Value);
            }

            //宠物守护附加战力
            int fightNum = 0;
            PetComponent petCom = unit.GetComponent<PetComponent>();
            for (int i = 0; i < 4; i++)
            {
                if (petCom.PetShouHuList.Count < 4)
                {
                    break;
                }

                RolePetInfo rolePetInfoNow = petCom.GetPetInfo(petCom.PetShouHuList[i]);
                if (rolePetInfoNow == null)
                {
                    continue;
                }
                fightNum = fightNum + rolePetInfoNow.PetPingFen;
            }

            int addShouHuFight = (int)fightNum / 10;

            //其他战力附加
            int addZhanLi = numericComponent.GetAsInt(NumericType.Numeric_Error);

            //觉醒战力附加
   
            List<int> juexingSkillList = unit.GetComponent<SkillSetComponent>().GetJueSkillIds(0);
            int addJueXingZhanLi = 0;
            if (juexingSkillList.Count >= 1)
            {
                addJueXingZhanLi = Math.Min(juexingSkillList.Count, 3) * 300;
            }
            if (juexingSkillList.Count >= 4)
            {
                addJueXingZhanLi += (Math.Min(juexingSkillList.Count, 7) - 3) * 400;
            }
            if (juexingSkillList.Count >= 8)
            {
                addJueXingZhanLi += 500;
            }

            addZhanLi += addJueXingZhanLi;
            
            long OneProvalueNaiLi = 0;
            long OneProvalueZhiLi = 0;
            long OneProvalueMinJie = 0;
            long OneProvalueLiLiang =0;
            long OneProvalueTiZhi = 0;
            addZhanLi = (int)((OneProvalueNaiLi + OneProvalueZhiLi + OneProvalueMinJie + OneProvalueLiLiang + OneProvalueTiZhi));   //属性点放大系数

            //技能属性点附加战力
            int skillPointFight = 0;  //剩余属性点

            skillPointFight = skillPointFight * 50;
            if (skillPointFight < 0)
            {
                skillPointFight = 0;
            }
            //理论不会超过此值
            if (skillPointFight >= 5000)
            {
                skillPointFight = 5000;
            }

            //int zhanliValue =(int)(ShiLi_Act * (1 + ShiLi_ActPro) + ShiLi_Def * (1 + ShiLi_DefPro) + (ShiLi_Hp * 0.1f) * (1 + ShiLi_HpPro)) + roleLv * 50 + (int)proLvAdd + addZhanLi + addShouHuFight;
            int zhanliValue = (int)(ShiLi_Act * (1 + ShiLi_ActPro) + ShiLi_Def * (1 + ShiLi_DefPro) + (ShiLi_Hp * 0.1f) * (1 + ShiLi_HpPro)) + 1 * 100 + (int)proLvAdd + addZhanLi + addShouHuFight + chuanchengProAdd + skillPointFight;
            //Console.WriteLine("ShiLi_Act = " + ShiLi_Act + " ShiLi_ActPro = " + ShiLi_ActPro + " ShiLi_Def = " + ShiLi_Def + " ShiLi_DefPro = "+ ShiLi_DefPro + " ShiLi_Hp = " + ShiLi_Hp + " ShiLi_HpPro = " + ShiLi_HpPro + " proLvAdd = " + proLvAdd + " addZhanLi = " + addZhanLi);

            //根据属性点整体放大发
            long oneProSum = 0;
            int addZhanliValue = (int)(zhanliValue * (oneProSum/30000f));
            if (addZhanliValue > 0) {
                zhanliValue = zhanliValue + addZhanliValue;
                //Console.WriteLine("zhanliValue = " + zhanliValue + " addZhanliValue = " + addZhanliValue + "oneProSum = " + oneProSum);
            }

            //更新战力
            unit.GetComponent<UserInfoComponent>().UpdateRoleData(UserDataType.Combat, zhanliValue.ToString(), notice);

            if (zhanliValue < 0 || zhanliValue > 500000)
            {
                Log.Error($"战力异常: {unit.DomainZone()}  {unit.GetComponent<UserInfoComponent>().UserInfo.Name}  {zhanliValue}");
            }

        }
    }


}

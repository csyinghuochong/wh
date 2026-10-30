using Alipay.AopSdk.Core.Domain;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

namespace ET
{
    /// <summary>
    /// SkillEditor function handlers。顺序与 DocEditor界面配置.xml 的 function_list 一致。
    /// </summary>
    public static class SkillEditorFunctions
    {
        /// <summary>
        /// 按 DocEditor界面配置.xml 的 function_list 顺序注册。旧名留在对应新函数后面。
        /// </summary>
        public static void RegisterAll()
        {
            // ------  常用函数  ------
            SkillEditorFunctionRegistry.Register("DEFINE_VARIABLE", DefineVariable);        //定义变量
            SkillEditorFunctionRegistry.Register("SET_VARIABLE_VALUE", SetVariableValue);        //变量赋值
            SkillEditorFunctionRegistry.Register("DEFINE_VARIABLE_RAMDOM_VALUE", DefineVariableRamdomValue);        //定义变量-随机值
            SkillEditorFunctionRegistry.Register("CHANCE_TRIGGER", ChanceTrigger);        //概率触发
            SkillEditorFunctionRegistry.Register("LOGIC_RELATION", LogicRelation);        //逻辑运算
            SkillEditorFunctionRegistry.Register("INTERRUPT_CHANT", InterruptChant);        //打断吟唱
            SkillEditorFunctionRegistry.Register("DESTROY_UNIT", DestroyUnit);        //销毁单位
            SkillEditorFunctionRegistry.Register("BREAK", BreakLoop);        //跳出循环
            SkillEditorFunctionRegistry.Register("RETURN_TRUE", ReturnTrue);        //返回成功值
            // ------  发送消息  ------
            SkillEditorFunctionRegistry.Register("INFORM_EVENT_ATTACK", InformEventAttack);        //通知攻击事件
            SkillEditorFunctionRegistry.Register("INFORM_EVENT_DEFENSE", InformEventDefense);        //通知防御事件
            SkillEditorFunctionRegistry.Register("INFORM_EVENT_HEAL", InformEventHeal);        //通知治疗事件
            SkillEditorFunctionRegistry.Register("INFORM_CLIENT_HIT_SUCCESS", InformClientHitSuccess);        //通知客户端命中
            // ------  伤害相关  ------
            SkillEditorFunctionRegistry.Register("SKILL_DAMAGE_CHECK_PHYSICS", SkillDamageCheckPhysics);        //物理技能判定
            SkillEditorFunctionRegistry.Register("SKILL_DAMAGE_CHECK_MAGIC", SkillDamageCheckMagic);        //法术技能判定
            SkillEditorFunctionRegistry.Register("SKILL_DAMAGE_CHECK_HEAL", SkillDamageCheckHeal);        //治疗技能判定
            SkillEditorFunctionRegistry.Register("CALCULATE_PHYSICS_DAMAGE", CalculatePhysicsDamage);        //计算物理伤害
            SkillEditorFunctionRegistry.Register("CALCULATE_MAGIC_DAMAGE", CalculateMagicDamage);        //计算法术伤害
            SkillEditorFunctionRegistry.Register("CALCULATE_HEAL_DAMAGE", CalculateHealDamage);        //计算治疗量
            // ------  BUFF  ------
            SkillEditorFunctionRegistry.Register("ADD_BUFF", AddBuff);        //添加BUFF
            SkillEditorFunctionRegistry.Register("ADD_BUFF_CONTROL", AddBuffControl);        //旧版添加控制BUFF
            SkillEditorFunctionRegistry.Register("ADD_DEBUFF_NO_CONTROL", AddDebuffNoControl);        //添加负面BUFF
            SkillEditorFunctionRegistry.Register("ADD_DEBUFF_CONTROL", AddDebuffControl);        //添加控制BUFF
            SkillEditorFunctionRegistry.Register("REMOVE_BUFF", RemoveBuff);        //移除BUFF
            SkillEditorFunctionRegistry.Register("REMOVE_BUFF_RANDOM", RemoveBuffRandom);        //移除BUFF-随机
            SkillEditorFunctionRegistry.Register("REMOVE_BUFF_GROUP", RemoveBuffGroup);        //移除BUFF组
            SkillEditorFunctionRegistry.Register("BUFF_DATA_SET", BuffDataSet);        //BUFF-数据设置
            SkillEditorFunctionRegistry.Register("SET_BUFF_DATA", SetBuffDataLegacy);        //旧版 BUFF-数据设置
            SkillEditorFunctionRegistry.Register("BUFF_DATA_GET", BuffDataGet);        //BUFF-数据获取
            SkillEditorFunctionRegistry.Register("GET_BUFF_DATA", GetBuffDataLegacy);        //旧版 BUFF-数据获取
            // ------  获取参数  ------
            SkillEditorFunctionRegistry.Register("GET_UNIT_TYPEID", GetUnitTypeId);        //获取单位类型
            SkillEditorFunctionRegistry.Register("GET_UNIT_LEVEL", GetUnitLevel);        //获取单位等级
            SkillEditorFunctionRegistry.Register("GET_UNIT_ATTRIBUTE", GetUnitAttribute);        //获取单位属性
            SkillEditorFunctionRegistry.Register("GET_HP_PERCENT", GetHpPercent);        //获取HP百分比-小数
            SkillEditorFunctionRegistry.Register("GET_SKILL_LEVEL", GetSkillLevel);        //获取技能等级
            SkillEditorFunctionRegistry.Register("GET_BUFF_STACK", GetBuffLevel);        //获取BUFF层数
            SkillEditorFunctionRegistry.Register("GET_BUFF_LEVEL", GetBuffLevel);        //旧版获取BUFF层数
            SkillEditorFunctionRegistry.Register("GET_RANDOM_TARGET", GetRandomUnit);        //获取随机目标
            SkillEditorFunctionRegistry.Register("GET_RANDOM_UNIT", GetRandomUnit);        //旧版获取随机目标
            SkillEditorFunctionRegistry.Register("GET_POINT_DISTANCE", GetPointDistance);        //获取两点之间距离
            SkillEditorFunctionRegistry.Register("GET_POINT_DIRECTION", GetPointDirection);        //获取两点之间朝向
            // ------  条件检测  ------
            SkillEditorFunctionRegistry.Register("IS_DEAD", IsDead);        //是否死亡
            SkillEditorFunctionRegistry.Register("IS_DYING", IsDying);        //是否濒死
            SkillEditorFunctionRegistry.Register("OWN_BUFF", OwnBuff);        //是否拥有BUFF
            SkillEditorFunctionRegistry.Register("HAS_BUFF", HasBuffLegacy);        //旧版是否拥有BUFF
            // ------  设置属性  ------
            SkillEditorFunctionRegistry.Register("CHANGE_CURRENT_HP", ChangeCurrentHp);        //修改生命值
            SkillEditorFunctionRegistry.Register("CHANGE_UNIT_ATTRIBUTE_ADD", ChangeUnitAttributeAdd);        //修改属性
            SkillEditorFunctionRegistry.Register("CHANGE_GLOBAL_CD", ChangeGlobalCd);        //修改公共CD
            SkillEditorFunctionRegistry.Register("CHANGE_SKILL_CURRENT_CD", ChangeSkillCurrentCd);        //修改技能当前CD
            SkillEditorFunctionRegistry.Register("CHANGE_SKILL_CURRENT_CD_MULTIPLE", ChangeSkillCurrentCdMultiple);        //修改技能当前CD-批量
            SkillEditorFunctionRegistry.Register("SET_UNIT_CHOOSE_STATUS", SetUnitChooseStatus);        //设置无法选中状态
            SkillEditorFunctionRegistry.Register("SET_UNIT_MOCK_TARGET", SetUnitMockTarget);        //设置嘲讽目标
            SkillEditorFunctionRegistry.Register("DEL_UNIT_MOCK_TARGET", DelUnitMockTarget);        //取消嘲讽目标
            // ------  进行位移  ------
            SkillEditorFunctionRegistry.Register("DISPLACEMENT_TO_TARGET", DisplacementToTarget);        //指定目标位移
            SkillEditorFunctionRegistry.Register("DISPLACEMENT_TO_POINT", DisplacementToPoint);        //指定地点位移
            SkillEditorFunctionRegistry.Register("DISPLACEMENT_TO_DIRECTION", DisplacementToDirection);        //指定朝向位移
            SkillEditorFunctionRegistry.Register("SET_UNIT_DIRECTION", SetUnitDirection);        //设置单位朝向
            SkillEditorFunctionRegistry.Register("SET_DIR", SetUnitDirection);        //旧版设置单位朝向
            // ------  创建单位  ------
            SkillEditorFunctionRegistry.Register("CREATE_SUMMON", CreateSummon);        //创建技能体
            SkillEditorFunctionRegistry.Register("UNIT_ADD_SUMMON", CreateSummonLegacy);        //旧版创建技能体
            SkillEditorFunctionRegistry.Register("SET_SUMMON_TARGET", SetSummonTarget);        //设置技能体目标 xx
            SkillEditorFunctionRegistry.Register("SET_UNIT_TARGET", SetSummonTarget);        //旧版设置技能体目标
            // ------  护盾  ------
            // ------  暂时不用  ------
            SkillEditorFunctionRegistry.Register("CHANGE_UNIT_ATTRIBUTE_PERCENT", ChangeUnitAttributePercent);        //修改属性-比例
        }

        // ------  常用函数  ------

        /// <summary>
        /// 定义变量（DEFINE_VARIABLE）
        /// 0 变量名
        /// 1 初始值
        /// </summary>
        private static void DefineVariable(SkillEditorFunctionContext ctx)
        {

            string varName = ctx.ResolveVarName(ctx.GetParamRaw(0));

            if (string.IsNullOrEmpty(varName)) { return; }

            // Keep literal as string (false / 0.5 / 100) for LOGIC_RELATION comparisons.

            ctx.SetVariable(varName, ctx.GetParamRaw(1).Trim());

        }

        /// <summary>
        /// 变量赋值（SET_VARIABLE_VALUE）
        /// 0 变量名
        /// 1 值
        /// </summary>
        private static void SetVariableValue(SkillEditorFunctionContext ctx)
        {

            string varName = ctx.ResolveVarName(ctx.GetParamRaw(0));

            if (string.IsNullOrEmpty(varName)) { return; }

            ctx.SetVariable(varName, ctx.ResolveParam(ctx.GetParamRaw(1)));

        }

        /// <summary>
        /// 定义变量-随机值（DEFINE_VARIABLE_RAMDOM_VALUE）
        /// 0 变量名
        /// 1 最小值（默认 1）
        /// 2 最大值（默认 10000）
        /// </summary>
        private static void DefineVariableRamdomValue(SkillEditorFunctionContext ctx)
        {

            string varName = ctx.ResolveVarName(ctx.GetParamRaw(0));

            if (string.IsNullOrEmpty(varName)) { return; }

            int minVal = ctx.GetParamInt(1, 1);

            int maxVal = ctx.GetParamInt(2, 10000);

            if (minVal > maxVal)
            {
                int tmp = minVal;

                minVal = maxVal;

                maxVal = tmp;

            }

            long randomValue = RandomHelper.RandomNumber(minVal, maxVal);

            ctx.SetVariable(varName, randomValue.ToString(CultureInfo.InvariantCulture));

        }

        /// <summary>
        /// 概率触发（CHANCE_TRIGGER）
        /// 0 概率 0.01%（默认 10000）
        /// </summary>
        private static void ChanceTrigger(SkillEditorFunctionContext ctx)
        {
            int rate = ctx.GetParamInt(0, 10000);

            if (rate < 0) { rate = 0; }

            if (rate > 10000) { rate = 10000; }

            bool hit = rate >= 10000 || RandomHelper.RandomNumber(0, 10000) < rate;

            ctx.LastConditionResult = hit;

            ctx.SetVariable("rs", hit ? 1 : 0);

        }

        /// <summary>
        /// 逻辑运算（LOGIC_RELATION）
        /// 0 参数1
        /// 1 关系
        /// 2 参数2
        /// </summary>
        private static void LogicRelation(SkillEditorFunctionContext ctx)
        {

            string left = ctx.ResolveParam(ctx.GetParamRaw(0));

            string op = ctx.GetParamRaw(1).Trim();

            string right = ctx.ResolveParam(ctx.GetParamRaw(2));

            bool result = CompareLogic(left, right, op);

            ctx.LastConditionResult = result;

            ctx.SetVariable("rs", result ? 1 : 0);
        }

        /// <summary>
        /// 打断吟唱（INTERRUPT_CHANT）
        /// 0 概率 0.01%（默认 10000）
        /// 只打断当前循环目标的吟唱读条（尚未 OnUseSkill）。已进 Skills 正在生效的技能不处理。
        /// </summary>
        private static void InterruptChant(SkillEditorFunctionContext ctx)
        {
            int rate = ctx.GetParamInt(0, 10000);
            if (rate < 0) { rate = 0; }
            if (rate > 10000) { rate = 10000; }

            bool hit = rate >= 10000 || RandomHelper.RandomNumber(0, 10000) < rate;
            ctx.LastConditionResult = hit;
            ctx.SetVariable("rs", hit ? 1 : 0);
            if (!hit)
            {
                return;
            }

            Unit target = ctx.Handler?.TheUnitTarget;
            if (target == null || target.IsDisposed)
            {
                ctx.LastConditionResult = false;
                ctx.SetVariable("rs", 0);
                return;
            }

            if (target.Type == UnitType.Monster)
            {
                // 怪物吟唱在服务器读条（SingSkillCmd），清前摇即可
                target.GetComponent<SkillManagerComponent>()?.InterruptPendingSing();
            }
            else if (target.Type == UnitType.Player)
            {
                // 玩家吟唱在客户端读条，服务器只广播停条
                MessageHelper.Broadcast(target, new M2C_SingingUpdate()
                {
                    UnitId = target.Id,
                    StateType = SingingUpdateKind.Singing,
                    StateValue = "0_0",
                    StateOperateType = 2,
                    StateTime = 0,
                });
            }

            if (Log.IsDebugEnabled)
            {
                Log.Debug($"INTERRUPT_CHANT skill={ctx.SkillId} target={target.Id} rate={rate}");
            }
        }

        /// <summary>
        /// 销毁单位（DESTROY_UNIT）
        /// 0 单位（默认 [自己] caster）
        /// </summary>
        private static void DestroyUnit(SkillEditorFunctionContext ctx)
        {
            Unit unit = ctx.ResolveUnit(ctx.GetParamRaw(0));
            if (unit == null || unit.IsDisposed || unit.Type == UnitType.Player)
            {
                return;
            }

            unit.GetParent<UnitComponent>()?.Remove(unit.Id);
            if (Log.IsDebugEnabled) Log.Debug($"DESTROY_UNIT unit={unit.Id}");
        }

        /// <summary>
        /// 跳出循环（BREAK）
        /// </summary>
        private static void BreakLoop(SkillEditorFunctionContext ctx)
        {
            ctx.SetVariable("__break", 1);
        }

        /// <summary>
        /// 返回成功值（RETURN_TRUE）
        /// </summary>
        private static void ReturnTrue(SkillEditorFunctionContext ctx)
        {

            ctx.LastConditionResult = true;

            ctx.SetVariable("rs", 1);

        }

        // ------  发送消息  ------

        /// <summary>
        /// 通知攻击事件（INFORM_EVENT_ATTACK）
        /// 0 施法者（默认 [自己] caster）
        /// 1 目标（默认 [自己] caster）
        /// 通知目标触发攻击事件。
        /// </summary>
        private static void InformEventAttack(SkillEditorFunctionContext ctx)
        {
            Unit caster = ctx.ResolveUnit(ctx.GetParamRaw(0));
            Unit target = ctx.ResolveUnit(ctx.GetParamRaw(1));
            if (caster == null || target == null)
            {
                return;
            }

            target.GetComponent<SkillPassiveComponent>()?.OnTrigegerPassiveSkill(
                    SkillPassiveTypeEnum.AttackAll, caster.Id, ctx.SkillId);

            if (Log.IsDebugEnabled) Log.Debug($"INFORM_EVENT_ATTACK skill={ctx.SkillId} caster={(caster?.Id ?? 0)} target={(target?.Id ?? 0)}");
        }

        /// <summary>
        /// 通知防御事件（INFORM_EVENT_DEFENSE）
        /// 0 施法者（默认 [自己] caster）
        /// 1 目标（默认 [目标] target）
        /// 通知目标触发防御事件。
        /// </summary>
        private static void InformEventDefense(SkillEditorFunctionContext ctx)
        {
            Unit caster = ctx.ResolveUnit(ctx.GetParamRaw(0));
            Unit target = ctx.ResolveUnit(ctx.GetParamRaw(1));

            //target?.GetComponent<SkillPassiveComponent>()?.OnTrigegerPassiveSkill(SkillPassiveTypeEnum.AttackAll, caster.Id, ctx.SkillId);
            //TriggerPassiveEvent(target, caster, SkillPassiveTypeEnum.None);
            Console.WriteLine($"被动类型未定义!!!");
            if (Log.IsDebugEnabled) Log.Debug($"INFORM_EVENT_DEFENSE skill={ctx.SkillId} caster={(caster?.Id ?? 0)} target={(target?.Id ?? 0)}");
        }

        /// <summary>
        /// 通知治疗事件（INFORM_EVENT_HEAL）
        /// 0 施法者（默认 [自己] caster）
        /// 1 目标（默认 [目标] target）
        /// 飘字用 CALCULATE_HEAL_DAMAGE 写入的 healTotal，没有则用 damageTotal。
        /// </summary>
        private static void InformEventHeal(SkillEditorFunctionContext ctx)
        {
            Unit caster = ctx.ResolveUnit(ctx.GetParamRaw(0));
            Unit target = ctx.ResolveUnit(ctx.GetParamRaw(1));
            if (target == null || target.IsDisposed)
            {
                return;
            }

            long rs = ctx.GetVariable("rs", (long)SkillEditorHitResult.Hit);
            long healTotal = ctx.GetVariable("healTotal", 0);
            if (healTotal <= 0)
            {
                healTotal = ctx.GetVariable("damageTotal", 0);
            }

            //计算治疗量
            //ctx.SetVariable("damageTotal", totalHeal);
            //ctx.SetVariable("healTotal", totalHeal);
            Console.WriteLine($"被动类型未定义!!!");
            Console.WriteLine($"应该需要加上通知客户端命中!!!");

            if (healTotal > 0)
            {
                SendNumbericChangeHelper.InformClientHit(caster, target, rs, healTotal);
            }

            if (Log.IsDebugEnabled)
            {
                Log.Debug($"INFORM_EVENT_HEAL skill={ctx.SkillId} caster={(caster?.Id ?? 0)} target={target.Id} heal={healTotal} rs={rs}");
            }
        }

        /// <summary>
        /// 通知客户端命中（INFORM_CLIENT_HIT_SUCCESS）
        /// 0 施法者（默认 [自己] caster）
        /// 1 目标（默认 [目标] target）
        /// 2 技能ID（默认 skill_id）
        /// 3 技能等级（默认 level）
        /// 按变量 rs 与 damageTotal 通知客户端飘字（命中、闪避、暴击、格挡等）。
        /// </summary>
        private static void InformClientHitSuccess(SkillEditorFunctionContext ctx)
        {
            Unit caster = ctx.ResolveUnit(ctx.GetParamRaw(0));
            Unit target = ctx.ResolveUnit(ctx.GetParamRaw(1));
            int skillId = ctx.GetParamInt(2, ctx.SkillId);
            int level = ctx.GetParamInt(3, ctx.SkillLevel);

            //闪避  68
            //暴击  70
            //抗暴  74
            //重击  80
            //化解  82
            //格挡  84
            //穿心  86
            //抵抗  
            /*
            < param desc = "控制类型" type = "string" >

            < option > [眩晕] EFFECT_CONTROL_STUN </ option >

            < option > [冰冻] EFFECT_CONTROL_FREEZE </ option >

            < option > [石化] EFFECT_CONTROL_PETRIFY </ option >

            < option > [缠绕] EFFECT_CONTROL_ENTANGLE </ option >

            < option > [沉睡] EFFECT_CONTROL_SLEEP </ option >

            < option > [定身] EFFECT_CONTROL_ROOT </ option >

            < option > [沉默] EFFECT_CONTROL_SILENCE </ option >

            < option > [减速] EFFECT_CONTROL_SLOW </ option >

            添加负面buff
            */

            if (target == null || target.IsDisposed)
            {
                return;
            }

            long rs = ctx.GetVariable("rs", 0);
            long totalDamage = ctx.GetVariable("damageTotal", 0);

            SendNumbericChangeHelper.InformClientHit(caster, target, rs, totalDamage);

            //TriggerPassiveEvent(caster, target, SkillPassiveTypeEnum.AllSkill_17, skillId);
            //TriggerPassiveEvent(target, caster, SkillPassiveTypeEnum.AllSkill_17, skillId);
            if (Log.IsDebugEnabled) Log.Debug($"INFORM_CLIENT_HIT_SUCCESS caster={(caster?.Id ?? 0)} target={target.Id} skill={skillId} level={level} rs={rs} damage={totalDamage}");
        }

        // ------  伤害相关  ------

        /// <summary>
        /// 物理技能判定（SKILL_DAMAGE_CHECK_PHYSICS）
        /// 0 施法者（默认 [自己] caster）
        /// 1 目标（默认 [目标] target）
        /// 2 技能ID（默认 skill_id）
        /// 3 技能等级（默认 level）
        /// 4 可否暴击（默认 [是] true）
        /// 5 可否重击（默认 [是] true）
        /// 6 可否闪避（默认 [是] true）
        /// 7 可否格挡（默认 [是] true）
        /// 8 暴击加成 0.01%（默认 0）
        /// 9 重击加成 0.01%（默认 0）
        /// 10 命中加成 0.01%（默认 0）
        /// 与法术判定共用 RunDamageCheck。
        /// </summary>
        private static void SkillDamageCheckPhysics(SkillEditorFunctionContext ctx)
        {
            RunDamageCheck(ctx);
        }

        /// <summary>
        /// 法术技能判定（SKILL_DAMAGE_CHECK_MAGIC）
        /// 0 施法者（默认 [自己] caster）
        /// 1 目标（默认 [目标] target）
        /// 2 技能ID（默认 skill_id）
        /// 3 技能等级（默认 level）
        /// 4 可否暴击（默认 [是] true）
        /// 5 可否重击（默认 [是] true）
        /// 6 可否闪避（默认 [是] true）
        /// 7 可否格挡（默认 [是] true）
        /// 8 暴击加成 0.01%（默认 0）
        /// 9 重击加成 0.01%（默认 0）
        /// 10 命中加成 0.01%（默认 0）
        /// 与物理判定共用 RunDamageCheck。
        /// </summary>
        private static void SkillDamageCheckMagic(SkillEditorFunctionContext ctx)
        {
            RunDamageCheck(ctx);
        }

        /// <summary>
        /// 治疗技能判定（SKILL_DAMAGE_CHECK_HEAL）
        /// 0 施法者（默认 [自己] caster）
        /// 1 目标（默认 [目标] target）
        /// 2 技能ID（默认 skill_id）
        /// 3 技能等级（默认 level）
        /// 4 可否暴击（默认 [是] true）
        /// 5 暴击加成 0.01%（默认 0）
        /// </summary>
        private static void SkillDamageCheckHeal(SkillEditorFunctionContext ctx)

        {

            Unit caster = ctx.ResolveUnit(ctx.GetParamRaw(0));

            Unit target = ctx.ResolveUnit(ctx.GetParamRaw(1));

            int skillId = ctx.GetParamInt(2, ctx.SkillId);

            int level = ctx.GetParamInt(3, ctx.SkillLevel);

            bool canCrit = ctx.GetParamBool(4, true);

            int critRateAdd = ctx.GetParamInt(5, 0);

            if (caster == null || target == null)

            {

                ctx.SetVariable("rs", (long)SkillEditorHitResult.Miss);

                ctx.LastConditionResult = false;

                return;

            }

            long rs = (long)SkillEditorHitResult.Hit;

            if (canCrit && SkillEditorContionHelper.RollCrit(caster, target, critRateAdd))
            {
                rs = (long)SkillEditorHitResult.Crit;
            }

            ctx.SetVariable("rs", rs);

            ctx.LastConditionResult = rs > 0;

            if (Log.IsDebugEnabled) Log.Debug($"SKILL_DAMAGE_CHECK_HEAL skill={skillId} level={level} caster={caster.Id} target={target.Id} rs={rs}");

        }

        /// <summary>
        /// 计算物理伤害（CALCULATE_PHYSICS_DAMAGE）
        /// 0 施法者（默认 [自己] caster）
        /// 1 目标（默认 [目标] target）
        /// 2 技能ID（默认 skill_id）
        /// 3 技能等级（默认 level）
        /// 4 物理威力 1=100%
        /// 5 法术威力 1=100%（默认 0）
        /// 6 物防威力 1=100%（默认 0）
        /// 7 法防威力 1=100%（默认 0）
        /// 8 最小攻击附加 二次项（默认 0）
        /// 9 最小攻击附加 一次项（默认 0）
        /// 10 最小攻击附加 常数项（默认 0）
        /// 11 最大攻击附加 二次项（默认 0）
        /// 12 最大攻击附加 一次项（默认 0）
        /// 13 最大攻击附加 常数项（默认 0）
        /// 14 额外加成属性1（默认 0）
        /// 15 额外加成系数1（默认 0）
        /// 16 额外加成属性2（默认 0）
        /// 17 额外加成系数2（默认 0）
        /// 18 无视防御比例 1=100%（默认 0）
        /// 19 无视最小防御（默认 0）
        /// 20 无视最大防御（默认 0）
        /// 21 暴击伤害加成 1=100%（默认 0）
        /// 22 施法者最大生命比例附伤 1=100%（默认 0）
        /// 23 施法者生命附伤最大值（默认 0）
        /// 24 目标最大生命比例附伤 1=100%（默认 0）
        /// 25 目标生命附伤最大值（默认 0）
        /// 26 生命附伤是否受伤害减免（默认 [是] true）
        /// 27 水系附伤（默认 0）
        /// 28 火系附伤（默认 0）
        /// 29 风系附伤（默认 0）
        /// 30 雷系附伤（默认 0）
        /// 31 毒系附伤（默认 0）
        /// 32 常规伤害吸血 1=100%（默认 0）
        /// 33 生命附伤吸血 1=100%（默认 0）
        /// 34 五系附伤吸血 1=100%（默认 0）
        /// 35 常规伤害分摊数（默认 1）
        /// 36 生命伤害分摊数（默认 1）
        /// 37 五系伤害分摊数（默认 1）
        /// 38 无视护盾（默认 [否] false）
        /// 39 仇恨倍率（默认 1）
        /// </summary>
        private static void CalculatePhysicsDamage(SkillEditorFunctionContext ctx)
        {
            SkillEditorDamageHelper.CalculateDamage(ctx, SkillEditorDamageKind.Physics);
        }

        /// <summary>
        /// 计算法术伤害（CALCULATE_MAGIC_DAMAGE）
        /// 0 施法者（默认 [自己] caster）
        /// 1 目标（默认 [目标] target）
        /// 2 技能ID（默认 skill_id）
        /// 3 技能等级（默认 level）
        /// 4 物理威力 1=100%（默认 0）
        /// 5 法术威力 1=100%
        /// 6 物防威力 1=100%（默认 0）
        /// 7 法防威力 1=100%（默认 0）
        /// 8 最小攻击附加 二次项（默认 0）
        /// 9 最小攻击附加 一次项（默认 0）
        /// 10 最小攻击附加 常数项（默认 0）
        /// 11 最大攻击附加 二次项（默认 0）
        /// 12 最大攻击附加 一次项（默认 0）
        /// 13 最大攻击附加 常数项（默认 0）
        /// 14 额外加成属性1（默认 0）
        /// 15 额外加成系数1（默认 0）
        /// 16 额外加成属性2（默认 0）
        /// 17 额外加成系数2（默认 0）
        /// 18 无视防御比例 1=100%（默认 0）
        /// 19 无视最小防御（默认 0）
        /// 20 无视最大防御（默认 0）
        /// 21 暴击伤害加成 1=100%（默认 0）
        /// 22 施法者最大生命比例附伤 1=100%（默认 0）
        /// 23 施法者生命附伤最大值（默认 0）
        /// 24 目标最大生命比例附伤 1=100%（默认 0）
        /// 25 目标生命附伤最大值（默认 0）
        /// 26 生命附伤是否受伤害减免（默认 [是] true）
        /// 27 水系附伤（默认 0）
        /// 28 火系附伤（默认 0）
        /// 29 风系附伤（默认 0）
        /// 30 雷系附伤（默认 0）
        /// 31 毒系附伤（默认 0）
        /// 32 常规伤害吸血 1=100%（默认 0）
        /// 33 生命附伤吸血 1=100%（默认 0）
        /// 34 五系附伤吸血 1=100%（默认 0）
        /// 35 常规伤害分摊数（默认 1）
        /// 36 生命伤害分摊数（默认 1）
        /// 37 五系伤害分摊数（默认 1）
        /// 38 无视护盾（默认 [否] false）
        /// 39 仇恨倍率（默认 1）
        /// </summary>
        private static void CalculateMagicDamage(SkillEditorFunctionContext ctx)
        {
            SkillEditorDamageHelper.CalculateDamage(ctx, SkillEditorDamageKind.Magic);
        }

        /// <summary>
        /// 计算治疗量（CALCULATE_HEAL_DAMAGE）
        /// 0 施法者（默认 [自己] caster）
        /// 1 目标（默认 [目标] target）
        /// 2 技能ID（默认 skill_id）
        /// 3 技能等级（默认 level）
        /// 4 物理威力 1=100%（默认 0）
        /// 5 法术威力 1=100%（默认 0）
        /// 6 物防威力 1=100%（默认 0）
        /// 7 法防威力 1=100%（默认 0）
        /// 8 最小攻击附加 二次项（默认 0）
        /// 9 最小攻击附加 一次项（默认 0）
        /// 10 最小攻击附加 常数项（默认 0）
        /// 11 最大攻击附加 二次项（默认 0）
        /// 12 最大攻击附加 一次项（默认 0）
        /// 13 最大攻击附加 常数项（默认 0）
        /// 14 额外加成属性1（默认 [念] 4）
        /// 15 额外加成系数1（默认 0）
        /// 16 额外加成属性2（默认 0）
        /// 17 额外加成系数2（默认 0）
        /// 18 暴击治疗加成 1=100%（默认 0）
        /// 19 施法者最大生命比例治疗 1=100%（默认 0）
        /// 20 施法者生命治疗最大值（默认 0）
        /// 21 目标最大生命比例治疗 1=100%（默认 0）
        /// 22 目标生命治疗最大值（默认 0）
        /// 23 常规治疗分摊数（默认 1）
        /// 24 生命治疗分摊数（默认 1）
        /// 25 仇恨倍率（默认 1）
        /// </summary>
        private static void CalculateHealDamage(SkillEditorFunctionContext ctx)
        {
            SkillEditorDamageHelper.CalculateHeal(ctx);
        }

        // ------  BUFF  ------

        /// <summary>
        /// 添加BUFF（ADD_BUFF）
        /// 0 施法者（默认 [自己] caster）
        /// 1 目标（默认 [目标] target）
        /// 2 BUFF_ID
        /// 3 作用间隔（毫秒）
        /// 4 作用次数
        /// 5 无视免疫（默认 [否] false）
        /// 6 BUFF等级（默认 level）
        /// </summary>
        private static void AddBuff(SkillEditorFunctionContext ctx)

        {

            Unit caster = ctx.ResolveUnit(ctx.GetParamRaw(0));

            Unit target = ctx.ResolveUnit(ctx.GetParamRaw(1));

            int buffId = ctx.GetParamInt(2, 0);

            int intervalMs = ctx.GetParamInt(3, 0);

            int tickCount = ctx.GetParamInt(4, 1);

            bool ignoreImmune = ctx.GetParamBool(5, false);

            int buffLevel = ctx.GetParamInt(6, ctx.SkillLevel);

            ApplyBuff(ctx, caster, target, buffId, intervalMs, tickCount, ignoreImmune, buffLevel);

        }

        /// <summary>
        /// 旧版添加控制BUFF（ADD_BUFF_CONTROL）
        /// 0 施法者
        /// 1 目标
        /// 2 BUFF_ID
        /// 3 作用间隔（毫秒）
        /// 4 作用次数
        /// 5 控制类型
        /// 6 无视免疫
        /// 7 BUFF等级
        /// 参数序与 ADD_DEBUFF_CONTROL 不同，保留给旧技能。
        /// </summary>
        private static void AddBuffControl(SkillEditorFunctionContext ctx)

        {

            Unit caster = ctx.ResolveUnit(ctx.GetParamRaw(0));

            Unit target = ctx.ResolveUnit(ctx.GetParamRaw(1));

            int buffId = ctx.GetParamInt(2, 0);

            int intervalMs = ctx.GetParamInt(3, 0);

            int tickCount = ctx.GetParamInt(4, 1);

            bool ignoreImmune = ctx.GetParamBool(6, false);

            int buffLevel = ctx.GetParamInt(7, ctx.SkillLevel);

            ApplyBuff(ctx, caster, target, buffId, intervalMs, tickCount, ignoreImmune, buffLevel);

        }

        /// <summary>
        /// 添加负面BUFF（ADD_DEBUFF_NO_CONTROL）
        /// 0 施法者（默认 [自己] caster）
        /// 1 目标（默认 [目标] target）
        /// 2 基础命中率（默认 10000）
        /// 3 BUFF_ID
        /// 4 作用间隔（毫秒）
        /// 5 作用次数（默认 1）
        /// 6 无视免疫（默认 [否] false）
        /// 7 受属性影响命中率（默认 [是] true）
        /// 8 受属性影响持续时间（默认 [是] true）
        /// 9 BUFF等级（默认 level）
        /// </summary>
        private static void AddDebuffNoControl(SkillEditorFunctionContext ctx)
        {
            AddDebuffInternal(ctx);
        }

        /// <summary>
        /// 添加控制BUFF（ADD_DEBUFF_CONTROL）
        /// 0 施法者（默认 [自己] caster）
        /// 1 目标（默认 [目标] target）
        /// 2 基础命中率（默认 10000）
        /// 3 BUFF_ID
        /// 4 作用间隔（毫秒）
        /// 5 作用次数
        /// 6 无视免疫（默认 [否] false）
        /// 7 受属性影响命中率（默认 [是] true）
        /// 8 受属性影响持续时间（默认 [是] true）
        /// 9 BUFF等级（默认 1）
        /// </summary>
        private static void AddDebuffControl(SkillEditorFunctionContext ctx)
        {
            AddDebuffInternal(ctx);
        }

        /// <summary>
        /// 移除BUFF（REMOVE_BUFF）
        /// 0 施法者（默认 [自己] caster）
        /// 1 目标（默认 [目标] target）
        /// 2 BUFF施法者（默认 [不限] NULL）
        /// 3 BUFF_ID
        /// 4 强行清除（默认 [否] false）
        /// 5 触发消失技能（默认 [是] true）
        /// </summary>
        private static void RemoveBuff(SkillEditorFunctionContext ctx)

        {

            Unit target = ctx.ResolveUnit(ctx.GetParamRaw(1));

            if (target == null) { return; }

            long buffFromUnitId = ResolveBuffSourceUnitId(ctx.GetParamRaw(2));

            int buffId = ctx.GetParamInt(3, 0);

            bool forceRemove = ctx.GetParamBool(4, false);

            bool triggerFadeSkill = ctx.GetParamBool(5, true);

            RemoveBuffById(target, buffFromUnitId, buffId, forceRemove, triggerFadeSkill);

        }

        /// <summary>
        /// 移除BUFF-随机（REMOVE_BUFF_RANDOM）
        /// 0 施法者（默认 [自己] caster）
        /// 1 目标（默认 [目标] target）
        /// 2 BUFF施法者（默认 [不限] NULL）
        /// 3 增益/减益
        /// 4 清除数量
        /// 5 独立清除概率 0.01%（默认 10000）
        /// 6 排除不可清除状态（默认 [是] true）
        /// 7 强行清除（默认 [否] false）
        /// 8 触发消失技能（默认 [是] true）
        /// </summary>
        private static void RemoveBuffRandom(SkillEditorFunctionContext ctx)

        {

            Unit target = ctx.ResolveUnit(ctx.GetParamRaw(1));

            if (target == null) { return; }

            long buffFromUnitId = ResolveBuffSourceUnitId(ctx.GetParamRaw(2));

            string benefitFilter = ctx.ResolveParam(ctx.GetParamRaw(3));

            int removeCount = ctx.GetParamInt(4, 1);

            int clearRate = ctx.GetParamInt(5, 10000);

            bool excludeNonClearable = ctx.GetParamBool(6, true);

            bool forceRemove = ctx.GetParamBool(7, false);

            bool triggerFadeSkill = ctx.GetParamBool(8, true);

            RemoveBuffRandomInternal(target, buffFromUnitId, benefitFilter, removeCount, clearRate, excludeNonClearable, forceRemove, triggerFadeSkill);

        }

        /// <summary>
        /// 移除BUFF组（REMOVE_BUFF_GROUP）
        /// 0 施法者（默认 [自己] caster）
        /// 1 目标（默认 [目标] target）
        /// 2 BUFF施法者（默认 [不限] NULL）
        /// 3 BUFF组
        /// 4 强行清除（默认 [否] false）
        /// 5 触发消失技能（默认 [是] true）
        /// </summary>
        private static void RemoveBuffGroup(SkillEditorFunctionContext ctx)

        {

            Unit target = ctx.ResolveUnit(ctx.GetParamRaw(1));

            if (target == null) { return; }

            long buffFromUnitId = ResolveBuffSourceUnitId(ctx.GetParamRaw(2));

            int buffGroup = ctx.GetParamInt(3, 0);

            bool forceRemove = ctx.GetParamBool(4, false);

            bool triggerFadeSkill = ctx.GetParamBool(5, true);

            RemoveBuffGroupInternal(target, buffFromUnitId, buffGroup, forceRemove, triggerFadeSkill);
        }

        /// <summary>
        /// BUFF-数据设置（BUFF_DATA_SET）
        /// 0 施法者（默认 [自己] caster）
        /// 1 BUFF拥有者（默认 [自己] caster）
        /// 2 BUFF_ID
        /// 3 VALUE1
        /// 4 VALUE2
        /// 5 VALUE3
        /// 6 VALUE4
        /// 7 VALUE5
        /// </summary>
        private static void BuffDataSet(SkillEditorFunctionContext ctx)
        {
            Unit owner = ctx.ResolveUnit(ctx.GetParamRaw(1));
            int buffId = ctx.GetParamInt(2, 0);
            if (owner == null || buffId <= 0)
            {
                return;
            }

            string[] values = ctx.GetOrCreateBuffDataValues(owner, buffId);
            for (int i = 0; i < values.Length; i++)
            {
                values[i] = ctx.ResolveParam(ctx.GetParamRaw(3 + i));
            }
        }

        /// <summary>
        /// 旧版 BUFF-数据设置（SET_BUFF_DATA）
        /// 0 BUFF拥有者
        /// 1 BUFF_ID
        /// 3 起 VALUE1～VALUE5
        /// 参数 2 不读。
        /// </summary>
        private static void SetBuffDataLegacy(SkillEditorFunctionContext ctx)
        {
            Unit owner = ctx.ResolveUnit(ctx.GetParamRaw(0));
            int buffId = ctx.GetParamInt(1, 0);
            if (owner == null || buffId <= 0)
            {
                return;
            }

            string[] values = ctx.GetOrCreateBuffDataValues(owner, buffId);
            for (int i = 0; i < values.Length && 3 + i < ctx.Node.Params.Count; i++)
            {
                values[i] = ctx.ResolveParam(ctx.GetParamRaw(3 + i));
            }
        }

        /// <summary>
        /// BUFF-数据获取（BUFF_DATA_GET）
        /// 0 施法者（默认 [自己] caster）
        /// 1 BUFF拥有者（默认 [自己] caster）
        /// 2 BUFF_ID
        /// 3 VALUE1
        /// 4 VALUE2
        /// 5 VALUE3
        /// 6 VALUE4
        /// 7 VALUE5
        /// </summary>
        private static void BuffDataGet(SkillEditorFunctionContext ctx)
        {
            Unit owner = ctx.ResolveUnit(ctx.GetParamRaw(1));
            int buffId = ctx.GetParamInt(2, 0);
            if (owner == null || buffId <= 0)
            {
                return;
            }

            string[] values = ctx.GetOrCreateBuffDataValues(owner, buffId);
            for (int i = 0; i < values.Length; i++)
            {
                string varName = ctx.ResolveVarName(ctx.GetParamRaw(3 + i));
                if (!string.IsNullOrEmpty(varName))
                {
                    ctx.SetVariable(varName, values[i] ?? string.Empty);
                }
            }
        }

        /// <summary>
        /// 旧版 BUFF-数据获取（GET_BUFF_DATA）
        /// 0 BUFF拥有者
        /// 1 BUFF_ID
        /// 3 起 VALUE1～VALUE5 变量名
        /// 参数 2 不读。
        /// </summary>
        private static void GetBuffDataLegacy(SkillEditorFunctionContext ctx)
        {
            Unit owner = ctx.ResolveUnit(ctx.GetParamRaw(0));
            int buffId = ctx.GetParamInt(1, 0);
            if (owner == null || buffId <= 0)
            {
                return;
            }

            string[] values = ctx.GetOrCreateBuffDataValues(owner, buffId);
            for (int i = 0; i < values.Length; i++)
            {
                string varName = ctx.ResolveVarName(ctx.GetParamRaw(3 + i));
                if (!string.IsNullOrEmpty(varName))
                {
                    ctx.SetVariable(varName, values[i] ?? string.Empty);
                }
            }
        }

        // ------  获取参数  ------

        /// <summary>
        /// 获取单位类型（GET_UNIT_TYPEID）
        /// 0 变量名（默认 nUnitTypeId）
        /// 1 单位
        /// </summary>
        private static void GetUnitTypeId(SkillEditorFunctionContext ctx)
        {
            string varName = ctx.ResolveVarName(ctx.GetParamRaw(0), "nUnitTypeId");
            Unit unit = ctx.ResolveUnit(ctx.GetParamRaw(1));
            ctx.SetVariable(varName, unit == null ? 0 : (int)unit.Type);
        }

        /// <summary>
        /// 获取单位等级（GET_UNIT_LEVEL）
        /// 0 变量名（默认 nUnitLevel）
        /// 1 单位
        /// </summary>
        private static void GetUnitLevel(SkillEditorFunctionContext ctx)
        {
            string varName = ctx.ResolveVarName(ctx.GetParamRaw(0), "nUnitLevel");
            Unit unit = ctx.ResolveUnit(ctx.GetParamRaw(1));
            ctx.SetVariable(varName, ResolveUnitLevel(unit));
        }

        /// <summary>
        /// 获取单位属性（GET_UNIT_ATTRIBUTE）
        /// 0 变量名
        /// 1 单位
        /// 2 属性
        /// </summary>
        private static void GetUnitAttribute(SkillEditorFunctionContext ctx)
        {
            string varName = ctx.ResolveVarName(ctx.GetParamRaw(0));
            Unit unit = ctx.ResolveUnit(ctx.GetParamRaw(1));
            int numericType = ctx.ResolveNumericType(ctx.GetParamRaw(2), 0);
            if (string.IsNullOrEmpty(varName) || unit == null || numericType <= 0)
            {
                return;
            }

            ctx.SetVariable(varName, ctx.GetUnitNumericValue(unit, numericType, 0));
        }

        /// <summary>
        /// 获取HP百分比-小数（GET_HP_PERCENT）
        /// 0 变量名（默认 nHpPct）
        /// 1 单位
        /// </summary>
        private static void GetHpPercent(SkillEditorFunctionContext ctx)
        {
            string varName = ctx.ResolveVarName(ctx.GetParamRaw(0), "nHpPct");
            Unit unit = ctx.ResolveUnit(ctx.GetParamRaw(1));
            if (unit == null)
            {
                ctx.SetVariable(varName, "0");
                return;
            }

            long hpMax = ctx.GetUnitNumericValue(unit, NumericType.HP_Max_10, 0);
            long hpCurrent = ctx.GetUnitNumericValue(unit, NumericType.HP_Current_8, 0);
            double pct = hpMax <= 0 ? 0d : (double)hpCurrent / hpMax;
            ctx.SetVariable(varName, pct.ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// 获取技能等级（GET_SKILL_LEVEL）
        /// 0 变量名（默认 nSkillLevel）
        /// 1 技能拥有者
        /// 2 技能ID
        /// </summary>
        private static void GetSkillLevel(SkillEditorFunctionContext ctx)
        {
            string varName = ctx.ResolveVarName(ctx.GetParamRaw(0), "nSkillLevel");
            Unit owner = ctx.ResolveUnit(ctx.GetParamRaw(1));
            int skillId = ctx.GetParamInt(2, ctx.SkillId);
            int level = skillId == ctx.SkillId ? ctx.SkillLevel : 1;
            SkillSetComponentServer skillSet = owner?.GetComponent<SkillSetComponentServer>();
            SkillPro skillPro = skillSet?.GetBySkillID(skillId);
            if (skillPro != null && skillPro.Level > 0)
            {
                level = skillPro.Level;
            }

            ctx.SetVariable(varName, level);
        }

        /// <summary>
        /// 获取BUFF层数（GET_BUFF_STACK）
        /// 0 变量名（默认 nBuffStack）
        /// 1 BUFF拥有者
        /// 2 BUFF_ID
        /// 同时注册旧名 GET_BUFF_LEVEL。
        /// </summary>
        private static void GetBuffLevel(SkillEditorFunctionContext ctx)
        {
            string varName = ctx.ResolveVarName(ctx.GetParamRaw(0), "nBuffStack");
            Unit owner = ctx.ResolveUnit(ctx.GetParamRaw(1));
            int buffId = ctx.GetParamInt(2, 0);
            int stack = 0;
            if (owner != null && buffId > 0)
            {
                BuffManagerComponent buffMgr = owner.GetComponent<BuffManagerComponent>();
                stack = buffMgr?.GetBuffNumber(buffId) ?? 0;
            }

            ctx.SetVariable(varName, stack);
        }

        /// <summary>
        /// 获取随机目标。对象列表只认 targets，为空直接结束。
        /// 优先级生效取第一个，否则随机。结果只写入变量。
        /// </summary>
        private static void GetRandomUnit(SkillEditorFunctionContext ctx)
        {
            string listName = ctx.ResolveVarName(ctx.GetParamRaw(1), string.Empty);
            if (!string.Equals(listName, "targets", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            List<long> hurtIds = ctx.Handler.HurtIds;
            if (hurtIds.Count == 0)
            {
                return;
            }

            int index = ctx.GetParamBool(2, true) ? 0 : RandomHelper.RandomNumber(0, hurtIds.Count);
            string varName = ctx.ResolveVarName(ctx.GetParamRaw(0), "target");
            ctx.SetVariable(varName, hurtIds[index]);
        }

        /// <summary>
        /// 获取两点之间距离（GET_POINT_DISTANCE）
        /// 0 变量（默认 nDistance）
        /// 1 起点（默认 [自己] caster）
        /// 2 终点（默认 [目标] target）
        /// </summary>
        private static void GetPointDistance(SkillEditorFunctionContext ctx)
        {
            string varName = ctx.ResolveVarName(ctx.GetParamRaw(0), "nDistance");
            float x1 = ctx.ResolvePositionComponent(ctx.GetParamRaw(1), 'x');
            float z1 = ctx.ResolvePositionComponent(ctx.GetParamRaw(1), 'z');
            float x2 = ctx.ResolvePositionComponent(ctx.GetParamRaw(2), 'x');
            float z2 = ctx.ResolvePositionComponent(ctx.GetParamRaw(2), 'z');
            double distance = Math.Sqrt((x2 - x1) * (x2 - x1) + (z2 - z1) * (z2 - z1));
            ctx.SetVariable(varName, ((long)distance).ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// 获取两点之间朝向（GET_POINT_DIRECTION）
        /// 0 变量x（默认 nDir_x）
        /// 1 变量z（默认 nDir_z）
        /// 2 起点x（默认 [自己] caster.x）
        /// 3 起点z（默认 [自己] caster.z）
        /// 4 终点x（默认 [目标] target.x）
        /// 5 终点z（默认 [目标] target.z）
        /// </summary>
        private static void GetPointDirection(SkillEditorFunctionContext ctx)
        {
            string varNameX = ctx.ResolveVarName(ctx.GetParamRaw(0), "nDir_x");
            string varNameZ = ctx.ResolveVarName(ctx.GetParamRaw(1), "nDir_z");
            float x1 = ctx.ResolvePositionComponent(ctx.GetParamRaw(2), 'x');
            float z1 = ctx.ResolvePositionComponent(ctx.GetParamRaw(3), 'z');
            float x2 = ctx.ResolvePositionComponent(ctx.GetParamRaw(4), 'x');
            float z2 = ctx.ResolvePositionComponent(ctx.GetParamRaw(5), 'z');
            float dx = x2 - x1;
            float dz = z2 - z1;
            float len = (float)Math.Sqrt(dx * dx + dz * dz);
            if (len <= 1e-6f)
            {
                ctx.SetVariable(varNameX, "0");
                ctx.SetVariable(varNameZ, "0");
                return;
            }

            ctx.SetVariable(varNameX, (dx / len).ToString(CultureInfo.InvariantCulture));
            ctx.SetVariable(varNameZ, (dz / len).ToString(CultureInfo.InvariantCulture));
        }

        // ------  条件检测  ------

        /// <summary>
        /// 是否死亡（IS_DEAD）
        /// 0 检测对象（默认 [自己] caster）
        /// </summary>
        private static void IsDead(SkillEditorFunctionContext ctx)
        {
            Unit unit = ctx.ResolveUnit(ctx.GetParamRaw(0));
            bool dead = IsUnitDead(unit);
            ctx.LastConditionResult = dead;
            ctx.SetVariable("rs", dead ? 1 : 0);
        }

        /// <summary>
        /// 是否濒死（IS_DYING）
        /// 0 检测对象（默认 [自己] caster）
        /// </summary>
        private static void IsDying(SkillEditorFunctionContext ctx)
        {
            Unit unit = ctx.ResolveUnit(ctx.GetParamRaw(0));
            bool dying = false;
            if (unit != null && !unit.IsDisposed)
            {
                NumericComponent numeric = unit.GetComponent<NumericComponent>();
                if (numeric != null)
                {
                    dying = numeric.GetAsInt(NumericType.Now_Dead) == 0
                        && numeric.GetAsLong(NumericType.HP_Current_8) <= 0;
                }
            }

            ctx.LastConditionResult = dying;
            ctx.SetVariable("rs", dying ? 1 : 0);
        }

        /// <summary>
        /// 是否拥有BUFF（OWN_BUFF）
        /// 0 检测对象（默认 [自己] caster）
        /// 1 BUFF施法者（默认 [不限] NULL）
        /// 2 BUFF_ID
        /// </summary>
        private static void OwnBuff(SkillEditorFunctionContext ctx)
        {
            Unit unit = ctx.ResolveUnit(ctx.GetParamRaw(0));
            long buffFromUnitId = ResolveBuffSourceUnitId(ctx.GetParamRaw(1));
            int buffId = ctx.GetParamInt(2, 0);
            bool owned = EvaluateOwnBuff(unit, buffFromUnitId, buffId);
            ctx.LastConditionResult = owned;
            ctx.SetVariable("rs", owned ? 1 : 0);
        }

        /// <summary>
        /// 旧版是否拥有BUFF（HAS_BUFF）
        /// 0 检测对象
        /// 1 BUFF_ID
        /// 不区分 BUFF 施法者。
        /// </summary>
        private static void HasBuffLegacy(SkillEditorFunctionContext ctx)
        {
            Unit unit = ctx.ResolveUnit(ctx.GetParamRaw(0));
            int buffId = ctx.GetParamInt(1, 0);
            bool owned = EvaluateOwnBuff(unit, 0, buffId);
            ctx.LastConditionResult = owned;
            ctx.SetVariable("rs", owned ? 1 : 0);
        }

        // ------  设置属性  ------

        /// <summary>
        /// 修改生命值（CHANGE_CURRENT_HP）
        /// 0 施法者（默认 [自己] caster）
        /// 1 目标（默认 [目标] target）
        /// 2 值（正加负减）
        /// 3 是否飘字（默认 [是] true）
        /// </summary>
        private static void ChangeCurrentHp(SkillEditorFunctionContext ctx)
        {
            Unit caster = ctx.ResolveUnit(ctx.GetParamRaw(0));
            Unit target = ctx.ResolveUnit(ctx.GetParamRaw(1));
            long delta = SkillEditorFunctionContext.ParseLong(ctx.ResolveParam(ctx.GetParamRaw(2)), 0);
            bool notice = ctx.GetParamBool(3, true);
            if (target == null || delta == 0)
            {
                return;
            }

            NumericComponent numeric = target.GetComponent<NumericComponent>();
            numeric?.ApplyChange(caster, NumericType.HP_Current_8, delta, ctx.SkillId, notice);
        }

        /// <summary>
        /// 修改属性（CHANGE_UNIT_ATTRIBUTE_ADD）
        /// 0 施法者（默认 [自己] caster）
        /// 1 目标（默认 [目标] target）
        /// 2 属性
        /// 3 值（正加负减）
        /// 固定值改属性，配表 ID 直接传（21→100211，27→分项，80→StaticStore）。
        /// </summary>
        private static void ChangeUnitAttributeAdd(SkillEditorFunctionContext ctx)
        {
            Unit caster = ctx.ResolveUnit(ctx.GetParamRaw(0));
            Unit target = ctx.ResolveUnit(ctx.GetParamRaw(1));
            int numericType = ctx.ResolveNumericType(ctx.GetParamRaw(2), 0);
            double deltaDisplay = SkillEditorFunctionContext.ParseDouble(ctx.ResolveParam(ctx.GetParamRaw(3)), 0d);


            if (target == null || numericType <= 0 || Math.Abs(deltaDisplay) < 1e-9)
            {
                return;
            }

            target.GetComponent<NumericComponent>()?.ChangeAttrFixed(caster, numericType, deltaDisplay, ctx.SkillId);

            Console.WriteLine($"ChangeUnitAttributeAdd2  numericValue : {target.GetComponent<NumericComponent>().GetAsInt(NumericType.P_HIT_Fixed_66) }");
        }

        /// <summary>
        /// 修改公共CD（CHANGE_GLOBAL_CD）
        /// 0 对象（默认 [自己] caster）
        /// 1 时间（毫秒）正加负减
        /// </summary>
        private static void ChangeGlobalCd(SkillEditorFunctionContext ctx)
        {
            Unit unit = ctx.ResolveUnit(ctx.GetParamRaw(0));
            int deltaMs = ctx.GetParamInt(1, 0);
            SkillManagerComponent skillMgr = unit?.GetComponent<SkillManagerComponent>();
            if (skillMgr == null)
            {
                return;
            }

            // 原公共 CD 对应组 1
            const int globalGroup = 1;
            skillMgr.GroupCDEndTimes.TryGetValue(globalGroup, out long endTime);
            skillMgr.GroupCDEndTimes[globalGroup] = endTime + deltaMs;
            if (Log.IsDebugEnabled) Log.Debug($"CHANGE_GLOBAL_CD unit={unit.Id} deltaMs={deltaMs} publicCd={skillMgr.GroupCDEndTimes[globalGroup]}");
        }

        /// <summary>
        /// 修改技能当前CD（CHANGE_SKILL_CURRENT_CD）
        /// 0 对象（默认 [自己] caster）
        /// 1 技能ID
        /// 2 时间（毫秒）正加负减
        /// </summary>
        private static void ChangeSkillCurrentCd(SkillEditorFunctionContext ctx)
        {
            Unit unit = ctx.ResolveUnit(ctx.GetParamRaw(0));
            int skillId = ctx.GetParamInt(1, 0);
            int deltaMs = ctx.GetParamInt(2, 0);
            SkillManagerComponent skillMgr = unit?.GetComponent<SkillManagerComponent>();
            if (skillMgr == null || skillId <= 0)
            {
                return;
            }

            if (!skillMgr.SkillCDs.TryGetValue(skillId, out SkillCDItem skillCd))
            {
                skillCd = new SkillCDItem();
                skillMgr.SkillCDs.Add(skillId, skillCd);
            }

            skillCd.CDEndTime += deltaMs;
            skillCd.CDPassive += deltaMs;
            if (Log.IsDebugEnabled) Log.Debug($"CHANGE_SKILL_CURRENT_CD unit={unit.Id} skill={skillId} deltaMs={deltaMs}");
        }

        /// <summary>
        /// 修改技能当前CD-批量（CHANGE_SKILL_CURRENT_CD_MULTIPLE）
        /// 0 对象（默认 [自己] caster）
        /// 1 技能类型
        /// 2 时间（毫秒）正加负减
        /// </summary>
        private static void ChangeSkillCurrentCdMultiple(SkillEditorFunctionContext ctx)
        {
            Unit unit = ctx.ResolveUnit(ctx.GetParamRaw(0));
            string skillTypeFilter = ctx.ResolveParam(ctx.GetParamRaw(1)).Trim();
            int deltaMs = ctx.GetParamInt(2, 0);
            SkillManagerComponent skillMgr = unit?.GetComponent<SkillManagerComponent>();
            if (skillMgr == null)
            {
                return;
            }

            SkillSetComponentServer skillSet = unit.GetComponent<SkillSetComponentServer>();
            int changedCount = 0;
            foreach (KeyValuePair<int, SkillCDItem> pair in skillMgr.SkillCDs.ToList())
            {
                if (!ShouldApplySkillCdFilter(skillSet, pair.Key, skillTypeFilter))
                {
                    continue;
                }

                pair.Value.CDEndTime += deltaMs;
                pair.Value.CDPassive += deltaMs;
                changedCount++;
            }

            if (Log.IsDebugEnabled) Log.Debug($"CHANGE_SKILL_CURRENT_CD_MULTIPLE unit={unit.Id} filter={skillTypeFilter} deltaMs={deltaMs} changed={changedCount}");
        }

        /// <summary>
        /// 设置无法选中状态（SET_UNIT_CHOOSE_STATUS）
        /// 0 单位（默认 [自己] caster）
        /// 1 状态（默认 [无法选中] 1）
        /// </summary>
        private static void SetUnitChooseStatus(SkillEditorFunctionContext ctx)
        {
        }

        /// <summary>
        /// 设置嘲讽目标（SET_UNIT_MOCK_TARGET）
        /// 0 施法者（默认 [BUFF施法者] buff.parent）
        /// 1 被嘲讽者（默认 [自己] caster）
        /// 2 目标（默认 [BUFF施法者] buff.parent）
        /// </summary>
        private static void SetUnitMockTarget(SkillEditorFunctionContext ctx)
        {
            Unit tauntSource = ctx.ResolveUnit(ctx.GetParamRaw(0));
            Unit mockedUnit = ctx.ResolveUnit(ctx.GetParamRaw(1));
            Unit mockTarget = ctx.ResolveUnit(ctx.GetParamRaw(2));
            if (mockedUnit == null || mockTarget == null)
            {
                return;
            }

            AIComponent ai = mockedUnit.GetComponent<AIComponent>();
            ai?.ChangeTarget(mockTarget.Id);
            if (mockedUnit.Type == UnitType.Monster || mockedUnit.Type == UnitType.Pet)
            {
                mockedUnit.GetComponent<StateComponent>()?.StateTypeAdd(StateTypeEnum.ChaoFeng);
            }

            if (Log.IsDebugEnabled) Log.Debug($"SET_UNIT_MOCK_TARGET source={(tauntSource?.Id ?? 0)} mocked={mockedUnit.Id} target={mockTarget.Id}");
        }

        /// <summary>
        /// 取消嘲讽目标（DEL_UNIT_MOCK_TARGET）
        /// 0 被嘲讽者（默认 [自己] caster）
        /// </summary>
        private static void DelUnitMockTarget(SkillEditorFunctionContext ctx)
        {
            Unit mockedUnit = ctx.ResolveUnit(ctx.GetParamRaw(0));
            if (mockedUnit == null)
            {
                return;
            }

            mockedUnit.GetComponent<StateComponent>()?.StateTypeRemove(StateTypeEnum.ChaoFeng);
            mockedUnit.GetComponent<AIComponent>()?.ChangeTarget(0);
        }

        // ------  进行位移  ------

        /// <summary>
        /// 指定目标位移（DISPLACEMENT_TO_TARGET）
        /// 0 位移造成者（默认 [自己] caster）
        /// 1 位移者（默认 [自己] caster）
        /// 2 位移目标（默认 [目标] target）
        /// 3 位移后与目标距离
        /// 4 最大位移距离
        /// 5 移动类型
        /// 6 参数
        /// 7 阻挡
        /// 8 是否被动位移
        /// 9 位移后朝向目标
        /// </summary>
        private static void DisplacementToTarget(SkillEditorFunctionContext ctx)
        {
            DisplacementToTargetAsync(ctx).Coroutine();
        }

        /// <summary>
        /// 指定地点位移（DISPLACEMENT_TO_POINT）
        /// 0 位移造成者（默认 [自己] caster）
        /// 1 位移者（默认 [目标] target）
        /// 2 目标X坐标
        /// 3 目标Z坐标
        /// 4 位移后与目标点距离
        /// 5 最大位移距离
        /// 6 移动类型
        /// 7 参数
        /// 8 阻挡
        /// 9 是否被动位移
        /// 10 位移后朝向目标
        /// </summary>
        private static void DisplacementToPoint(SkillEditorFunctionContext ctx)
        {
            Unit mover = ctx.ResolveUnit(ctx.GetParamRaw(1));
            float x = ResolveScalarParam(ctx, ctx.GetParamRaw(2), 'x', mover?.Position.x ?? 0f);
            float z = ResolveScalarParam(ctx, ctx.GetParamRaw(3), 'z', mover?.Position.z ?? 0f);
            float keepDistance = ctx.GetParamFloat(4, 0f);
            int moveType = ctx.GetParamInt(6, 0);
            if (mover == null)
            {
                return;
            }

            Vector3 dest = new Vector3(x, mover.Position.y, z);
            if (keepDistance > 0f)
            {
                Vector3 dir = mover.Position - dest;
                if (dir.sqrMagnitude > 1e-6f)
                {
                    dest += dir.normalized * keepDistance;
                }
            }

            MoveUnitToPosition(mover, dest, moveType, 100);
        }

        /// <summary>
        /// 指定朝向位移（DISPLACEMENT_TO_DIRECTION）
        /// 0 位移造成者（默认 [自己] caster）
        /// 1 位移者（默认 [目标] target）
        /// 2 朝向X（默认 [自己朝向] caster.dir_x）
        /// 3 朝向Z（默认 [自己朝向] caster.dir_z）
        /// 4 位移距离
        /// 5 移动类型
        /// 6 参数
        /// 7 阻挡
        /// 8 是否被动位移
        /// 9 位移后朝向目标
        /// </summary>
        private static void DisplacementToDirection(SkillEditorFunctionContext ctx)
        {
            Unit mover = ctx.ResolveUnit(ctx.GetParamRaw(1));
            float dirX = ctx.GetParamFloat(2, 0f);
            float dirZ = ctx.GetParamFloat(3, 0f);
            float distance = ctx.GetParamFloat(4, 0f);
            int moveType = ctx.GetParamInt(6, 0);
            if (mover == null || distance <= 0f)
            {
                return;
            }

            Vector3 dir = new Vector3(dirX, 0f, dirZ);
            if (dir.sqrMagnitude <= 1e-6f)
            {
                dir = mover.Rotation * Vector3.forward;
            }
            else
            {
                dir.Normalize();
            }

            MoveUnitToPosition(mover, mover.Position + dir * distance, moveType, 100);
        }

        /// <summary>
        /// 设置单位朝向（SET_UNIT_DIRECTION）
        /// 0 单位（默认 [自己] caster）
        /// 1 dir_x
        /// 2 dir_z
        /// 同时注册旧名 SET_DIR。
        /// </summary>
        private static void SetUnitDirection(SkillEditorFunctionContext ctx)
        {
            Unit unit = ctx.ResolveUnit(ctx.GetParamRaw(0));
            float dirX = ResolveDirectionParam(ctx, ctx.GetParamRaw(1), 'x');
            float dirZ = ResolveDirectionParam(ctx, ctx.GetParamRaw(2), 'z');
            if (unit == null)
            {
                return;
            }

            Vector3 dir = new Vector3(dirX, 0f, dirZ);
            if (dir.sqrMagnitude <= 1e-6f)
            {
                return;
            }

            FaceUnitToward(unit, unit.Position + dir.normalized);
        }

        // ------  创建单位  ------

        /// <summary>
        /// 创建技能体（CREATE_SUMMON）
        /// 0 技能体ID
        /// 1 施法者（默认 [自己] caster）
        /// 2 初始坐标X（默认 [自己] caster.x）
        /// 3 初始坐标Z（默认 [自己] caster.z）
        /// 4 初始朝向X（默认 [自己] caster.dir_x）
        /// 5 初始朝向Z（默认 [自己] caster.dir_z）
        /// 6 运动类型（默认 [静止] 0）
        /// 7 追踪目标（默认 [目标] target）
        /// 8 碰到阻挡删除（默认 [否] false）
        /// 9 最大持续时间 ms
        /// 10 作用间隔 ms
        /// 11 作用次数
        /// 12 创建技能ID（默认 Skill_1）
        /// 13 创建技能LV（默认 level）
        /// 14 作用技能ID（默认 Skill_2）
        /// 15 作用技能LV（默认 level）
        /// 16 追踪技能ID（默认 Skill_3）
        /// 17 追踪技能LV（默认 level）
        /// 18 消亡技能ID（默认 skill_4）
        /// 19 消亡技能LV（默认 level）
        /// 20 消亡-次数（默认 [是] true）
        /// 21 消亡-追踪消亡（默认 [是] true）
        /// 22 消亡-施法者死亡（默认 [否] false）
        /// 23 消亡-目标死亡（默认 [否] false）
        /// 读 LDSummon。旧入口 UNIT_ADD_SUMMON 走 CreateSummonLegacy。
        /// </summary>
        private static void CreateSummon(SkillEditorFunctionContext ctx)
        {
            CreateSummonInternal(ctx, legacyFormat: false);
        }

        /// <summary>
        /// 旧版创建技能体（UNIT_ADD_SUMMON）
        /// 参数序与 CREATE_SUMMON 不同，由 ParseLegacySummonRuntime 解析。
        /// </summary>
        private static void CreateSummonLegacy(SkillEditorFunctionContext ctx)
        {
            CreateSummonInternal(ctx, legacyFormat: true);
        }

        // 创建单位 xx：配置 name 为空，暂不接入。

        /// <summary>
        /// 设置技能体目标 xx（SET_SUMMON_TARGET）
        /// 界面未配参数。当前实现：0 技能体（空则读变量 createSummon），1 目标，2 是否锁定（默认 true）。同时注册旧名 SET_UNIT_TARGET。
        /// </summary>
        private static void SetSummonTarget(SkillEditorFunctionContext ctx)
        {
            Unit summon = ctx.ResolveUnit(ctx.GetParamRaw(0));
            if (summon == null)
            {
                summon = ctx.ResolveUnit("createSummon");
            }

            Unit target = ctx.ResolveUnit(ctx.GetParamRaw(1));
            bool lockTarget = ctx.GetParamBool(2, true);
            if (target == null)
            {
                return;
            }

            SkillEntityComponent skillEntity = summon?.GetComponent<SkillEntityComponent>();
            skillEntity?.SetTrackTarget(target, lockTarget);

            if (ctx.Handler != null)
            {
                ctx.Handler.TheUnitTarget = target;
            }

            if (Log.IsDebugEnabled) Log.Debug($"SET_SUMMON_TARGET skill={ctx.SkillId} summon={(summon?.Id ?? 0)} target={target.Id} lock={lockTarget}");
        }

        // ------  护盾  ------

        // 护盾初始化 xx：配置 name 为空，暂不接入。

        // 护盾效果清除 xx：配置 name 为空，暂不接入。

        // ------  暂时不用  ------

        /// <summary>
        /// 修改属性-比例（CHANGE_UNIT_ATTRIBUTE_PERCENT）
        /// 0 施法者（默认 [自己] caster）
        /// 1 目标（默认 [目标] target）
        /// 2 属性
        /// 3 值（正加负减）
        /// 百分比改属性，percent=10 表示 +10%（21→100212，80→100802）。
        /// </summary>
        private static void ChangeUnitAttributePercent(SkillEditorFunctionContext ctx)
        {
            Unit caster = ctx.ResolveUnit(ctx.GetParamRaw(0));
            Unit target = ctx.ResolveUnit(ctx.GetParamRaw(1));
            int numericType = ctx.ResolveNumericType(ctx.GetParamRaw(2), 0);
            double percent = SkillEditorFunctionContext.ParseDouble(ctx.ResolveParam(ctx.GetParamRaw(3)), 0d);
            if (target == null || numericType <= 0 || Math.Abs(percent) < 1e-9)
            {
                return;
            }

            target.GetComponent<NumericComponent>()?.ChangeAttrPercent(caster, numericType, (float)(percent / 100d), ctx.SkillId);
        }

        // ------  内部辅助  ------

        /// <summary>
        /// 添加负面 BUFF 与控制 BUFF 的共用实现。
        /// </summary>
        private static void AddDebuffInternal(SkillEditorFunctionContext ctx)
        {
            Unit caster = ctx.ResolveUnit(ctx.GetParamRaw(0));
            Unit target = ctx.ResolveUnit(ctx.GetParamRaw(1));
            int baseHitRate = ctx.GetParamInt(2, 10000);
            int buffId = ctx.GetParamInt(3, 0);
            int intervalMs = ctx.GetParamInt(4, 0);
            int tickCount = ctx.GetParamInt(5, 1);
            bool ignoreImmune = ctx.GetParamBool(7, false);
            bool useAttrHitRate = ctx.GetParamBool(8, true);
            bool useAttrDuration = ctx.GetParamBool(9, true);
            int buffLevel = ctx.GetParamInt(10, ctx.SkillLevel);

            if (target == null || buffId <= 0)
            {
                ctx.SetVariable("rs", 0);
                ctx.LastConditionResult = false;
                return;
            }

            if (!TryRollDebuffHit(caster, target, baseHitRate, useAttrHitRate))
            {
                ctx.SetVariable("rs", 0);
                ctx.LastConditionResult = false;
                return;
            }

            if (useAttrDuration && intervalMs > 0 && tickCount > 0)
            {
                int extraTicks = ResolveDebuffDurationBonus(caster, target);
                if (extraTicks != 0)
                {
                    tickCount = Math.Max(1, tickCount + extraTicks);
                }
            }

            if (!ApplyBuff(
                ctx,
                caster,
                target,
                buffId,
                intervalMs,
                tickCount,
                ignoreImmune,
                buffLevel))
            {
                ctx.SetVariable("rs", 0);
                ctx.LastConditionResult = false;
                return;
            }
            ctx.SetVariable("rs", 1);
            ctx.LastConditionResult = true;
        }

        /// <summary>
        /// 按基础命中率掷是否命中；勾选受属性影响时，用双方等级差修正命中率。
        /// </summary>
        private static bool TryRollDebuffHit(Unit caster, Unit target, int baseHitRate, bool useAttrHitRate)
        {
            int hitRate = baseHitRate;
            if (hitRate >= 10000)
            {
                return true;
            }

            if (hitRate <= 0)
            {
                return false;
            }

            if (useAttrHitRate && caster != null && target != null)
            {
                int casterLevel = ResolveUnitLevel(caster);
                int targetLevel = ResolveUnitLevel(target);
                if (casterLevel > targetLevel)
                {
                    hitRate += (casterLevel - targetLevel) * 100;
                }
                else if (targetLevel > casterLevel)
                {
                    hitRate -= (targetLevel - casterLevel) * 100;
                }
            }

            if (hitRate >= 10000)
            {
                return true;
            }

            if (hitRate <= 0)
            {
                return false;
            }

            return RandomHelper.RandomNumber(0, 10000) < hitRate;
        }

        /// <summary>
        /// 受属性影响持续时间时，按施法者与目标等级差折成额外作用次数。
        /// </summary>
        private static int ResolveDebuffDurationBonus(Unit caster, Unit target)
        {
            if (caster == null || target == null)
            {
                return 0;
            }

            int levelDelta = ResolveUnitLevel(caster) - ResolveUnitLevel(target);
            return levelDelta / 5;
        }

        /// <summary>
        /// 物理、法术技能判定共用：闪避、格挡、暴击、重击，结果写入 rs。未命中时直接通知客户端飘字。
        /// </summary>
        private static void RunDamageCheck(SkillEditorFunctionContext ctx)
        {
            Unit caster = ctx.ResolveUnit(ctx.GetParamRaw(0));

            Unit target = ctx.ResolveUnit(ctx.GetParamRaw(1));

            int skillId = ctx.GetParamInt(2, ctx.SkillId);

            int level = ctx.GetParamInt(3, ctx.SkillLevel);

            bool canCrit = ctx.GetParamBool(4, true);

            bool canHeavy = ctx.GetParamBool(5, true);

            bool canDodge = ctx.GetParamBool(6, true);

            int critRateAdd = ctx.GetParamInt(8, 0);

            int heavyRateAdd = ctx.GetParamInt(9, 0);

            int hitRateAdd = ctx.GetParamInt(10, 0);

            if (caster == null || target == null)
            {
                ctx.SetVariable("rs", (long)SkillEditorHitResult.Miss);

                ctx.LastConditionResult = false;

                return;
            }

            long rs = SkillEditorContionHelper.EvaluateDirectHit(
                ctx, caster, target, skillId,
                canCrit, canHeavy, canDodge,
                critRateAdd, heavyRateAdd, hitRateAdd,
                level, 0f, 0f, true);

            ctx.SetVariable("rs", rs);
            bool hitOk = rs == (long)SkillEditorHitResult.Hit
                || rs == (long)SkillEditorHitResult.Heavy
                || rs == (long)SkillEditorHitResult.Crit;
            ctx.LastConditionResult = hitOk;
            if (!hitOk)
            {
                //hitOk 只认三种结果：普通命中、重击、暴击。这三种为真，后面的 IF 结果才会走「算伤害」和 INFORM_CLIENT_HIT_SUCCESS，由那个节点把伤害量发给客户端。
                //为假时，rs 是 Miss、Dodge 或 Immune。IF 结果整段不执行，INFORM_CLIENT_HIT_SUCCESS 不会跑。不在判定里发一次，客户端就看不到「闪避」或「Miss」。所以这里用伤害量 0，只把 rs 送出去，头顶按 HitType 飘字。
                SendNumbericChangeHelper.InformClientHit(caster, target, rs, 0);
            }
        }

        /// <summary>
        /// LOGIC_RELATION 的比较与逻辑运算。
        /// </summary>
        private static bool CompareLogic(string left, string right, string op)

        {

            if (string.IsNullOrEmpty(op)) { return false; }

            switch (op)

            {

                case "&&":

                    return SkillEditorFunctionContext.ParseBool(left) && SkillEditorFunctionContext.ParseBool(right);

                case "||":

                    return SkillEditorFunctionContext.ParseBool(left) || SkillEditorFunctionContext.ParseBool(right);

                case "&":

                    return SkillEditorFunctionContext.ParseLong(left, 0) != 0 && SkillEditorFunctionContext.ParseLong(right, 0) != 0;

                case "|":

                    return SkillEditorFunctionContext.ParseLong(left, 0) != 0 || SkillEditorFunctionContext.ParseLong(right, 0) != 0;

                case "==":

                    if (TryParseNumeric(left, out double ldEq) && TryParseNumeric(right, out double rdEq))

                    {

                        return Math.Abs(ldEq - rdEq) < 1e-9;

                    }

                    return SkillEditorFunctionContext.ParseBool(left) == SkillEditorFunctionContext.ParseBool(right)

                        || string.Equals(left?.Trim(), right?.Trim(), StringComparison.OrdinalIgnoreCase);

                case "~=":

                    if (TryParseNumeric(left, out double ldNe) && TryParseNumeric(right, out double rdNe))

                    {

                        return Math.Abs(ldNe - rdNe) >= 1e-9;

                    }

                    return SkillEditorFunctionContext.ParseBool(left) != SkillEditorFunctionContext.ParseBool(right)

                        && !string.Equals(left?.Trim(), right?.Trim(), StringComparison.OrdinalIgnoreCase);

                case ">":

                    return SkillEditorFunctionContext.ParseDouble(left) > SkillEditorFunctionContext.ParseDouble(right);

                case "<":

                    return SkillEditorFunctionContext.ParseDouble(left) < SkillEditorFunctionContext.ParseDouble(right);

                case ">=":

                    return SkillEditorFunctionContext.ParseDouble(left) >= SkillEditorFunctionContext.ParseDouble(right);

                case "<=":

                    return SkillEditorFunctionContext.ParseDouble(left) <= SkillEditorFunctionContext.ParseDouble(right);

                default:

                    Log.Warning($"LOGIC_RELATION unknown op: {op}");

                    return false;

            }

        }

        /// <summary>
        /// 把参数解析成数值，供等于、不等于比较使用。
        /// </summary>
        private static bool TryParseNumeric(string raw, out double value)

        {

            value = 0;

            if (string.IsNullOrWhiteSpace(raw)) { return false; }

            raw = raw.Trim();

            if (double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out value)) { return true; }

            if (double.TryParse(raw, NumberStyles.Float, CultureInfo.CurrentCulture, out value)) { return true; }

            if (long.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out long lv)) { value = lv; return true; }

            if (bool.TryParse(raw, out bool bv)) { value = bv ? 1 : 0; return true; }

            return false;

        }

        /// <summary>
        /// 解析 BUFF 施法者。空或 NULL 表示不限，返回 0。
        /// </summary>
        private static long ResolveBuffSourceUnitId(string raw)

        {

            string token = raw?.Trim() ?? string.Empty;

            if (string.IsNullOrEmpty(token) || token.Equals("NULL", StringComparison.OrdinalIgnoreCase))

            {

                return 0;

            }

            if (long.TryParse(token, out long id)) { return id; }

            return 0;

        }

        /// <summary>
        /// 按 BUFF_ID 从目标身上移除。
        /// </summary>
        private static void RemoveBuffById(Unit target, long buffFromUnitId, int buffId, bool forceRemove, bool triggerFadeSkill)

        {

            BuffManagerComponent buffMgr = target.GetComponent<BuffManagerComponent>();

            if (buffMgr == null || buffId <= 0) { return; }

            buffMgr.BuffRemoveByUnit(buffFromUnitId, buffId);

            if (Log.IsDebugEnabled) Log.Debug($"REMOVE_BUFF target={target.Id} buffId={buffId} from={buffFromUnitId} force={forceRemove} fade={triggerFadeSkill}");

        }

        /// <summary>
        /// 按增益/减益筛选后随机移除若干 BUFF。
        /// </summary>
        private static void RemoveBuffRandomInternal(Unit target, long buffFromUnitId, string benefitFilter, int removeCount, int clearRate, bool excludeNonClearable, bool forceRemove, bool triggerFadeSkill)

        {

            BuffManagerComponent buffMgr = target.GetComponent<BuffManagerComponent>();

            if (buffMgr == null || removeCount <= 0) { return; }

            HashSet<int> allowedBenefits = ParseBenefitFilter(benefitFilter);

            List<int> candidates = new List<int>();

            for (int i = 0; i < buffMgr.m_Buffs.Count; i++)

            {

                Buff bh = buffMgr.m_Buffs[i];

                if (bh?.MBuff == null) { continue; }

                if (buffFromUnitId != 0 && bh.TheUnitFrom?.Id != buffFromUnitId) { continue; }

                //if (allowedBenefits.Count > 0 && !allowedBenefits.Contains(bh.MBuff.BuffBenefitType)) { continue; }

                if (excludeNonClearable && !IsBuffClearable(bh.MBuff)) { continue; }

                candidates.Add(bh.MBuff.Id);

            }

            for (int n = 0; n < removeCount && candidates.Count > 0; n++)

            {

                if (clearRate < 10000 && RandomHelper.RandomNumber(0, 10000) >= clearRate)
                {
                    continue;
                }

                int idx = RandomHelper.RandomNumber(0, candidates.Count);

                int buffId = candidates[idx];

                candidates.RemoveAt(idx);

                RemoveBuffById(target, buffFromUnitId, buffId, forceRemove, triggerFadeSkill);

            }

        }

        /// <summary>
        /// 按 BUFF 组移除。
        /// </summary>
        private static void RemoveBuffGroupInternal(Unit target, long buffFromUnitId, int buffGroup, bool forceRemove, bool triggerFadeSkill)

        {

            BuffManagerComponent buffMgr = target.GetComponent<BuffManagerComponent>();

            if (buffMgr == null || buffGroup <= 0) { return; }

            for (int i = buffMgr.m_Buffs.Count - 1; i >= 0; i--)

            {

                Buff bh = buffMgr.m_Buffs[i];

                if (bh?.MBuff == null) { continue; }

                if (buffFromUnitId != 0 && bh.TheUnitFrom?.Id != buffFromUnitId) { continue; }

                //if (bh.MBuff.Remove == null || !bh.MBuff.Remove.Contains(buffGroup)) { continue; }

                buffMgr.OnRemoveBuffItem(bh);

                buffMgr.m_Buffs.RemoveAt(i);

            }

            if (Log.IsDebugEnabled) Log.Debug($"REMOVE_BUFF_GROUP target={target.Id} group={buffGroup} force={forceRemove} fade={triggerFadeSkill}");

        }

        /// <summary>
        /// 解析增益/减益过滤，多项用 | 分隔。
        /// </summary>
        private static HashSet<int> ParseBenefitFilter(string raw)

        {

            HashSet<int> set = new HashSet<int>();

            if (string.IsNullOrWhiteSpace(raw)) { return set; }

            foreach (string part in raw.Split('|'))

            {

                if (int.TryParse(part.Trim(), out int v)) { set.Add(v); }

            }

            return set;

        }

        /// <summary>
        /// 挂上 BUFF。处理免疫、间隔与作用次数。
        /// </summary>
        private static bool ApplyBuff(

            SkillEditorFunctionContext ctx,

            Unit caster,

            Unit target,

            int buffId,

            int intervalMs,

            int tickCount,

            bool ignoreImmune,

            int buffLevel)

        {

            if (target == null || buffId <= 0) { return false; }

            BuffManagerComponent buffMgr = target.GetComponent<BuffManagerComponent>();

            if (buffMgr == null) { return false; }

            if (!ignoreImmune && buffMgr.IsSkillImmune(ctx.SkillId)) { return false; }

            BuffData buffData = new BuffData
            {
                SkillId = ctx.SkillId,
                BuffId = buffId,
                UnitIdFrom = caster?.Id ?? 0,
            };
            if (intervalMs > 0 && tickCount > 0)
            {
                buffData.BuffEndTime = TimeHelper.ServerNow() + intervalMs * tickCount;
            }

            if (!buffMgr.BuffFactory(buffData, caster, ctx.Handler, true, ignoreImmune, intervalMs))
            {
                return false;
            }
            if (Log.IsDebugEnabled) Log.Debug($"ApplyBuff target={target.Id} buffId={buffId} interval={intervalMs} ticks={tickCount} level={buffLevel}");
            return true;
        }

        /// <summary>
        /// 取单位等级：玩家读角色等级，怪物读配置，宠物与精灵读主人等级。
        /// </summary>
        private static int ResolveUnitLevel(Unit unit)
        {
            if (unit == null || unit.IsDisposed)
            {
                return 0;
            }

            if (unit.Type == UnitType.Player)
            {
                return unit.GetComponent<RoleInfoComponentServer>()?.RoleInfo?.Lv ?? 0;
            }

            if (unit.Type == UnitType.Pet || unit.Type == UnitType.Elf)
            {
                Unit master = unit.GetParent<UnitComponent>()?.Get(unit.MasterId);
                if (master != null && !master.IsDisposed && master.Type == UnitType.Player)
                {
                    return master.GetComponent<RoleInfoComponentServer>()?.RoleInfo?.Lv ?? 1;
                }
            }

            if (LDMonsterCategory.Instance.Contain(unit.ConfigId))
            {
                return LDMonsterCategory.Instance.Get(unit.ConfigId).Lv;
            }

            return 1;
        }

        /// <summary>
        /// 判断单位是否拥有指定 BUFF，可限定来源单位。
        /// </summary>
        private static bool EvaluateOwnBuff(Unit unit, long buffFromUnitId, int buffId)
        {
            if (unit == null || buffId <= 0)
            {
                return false;
            }

            BuffManagerComponent buffMgr = unit.GetComponent<BuffManagerComponent>();
            if (buffMgr == null)
            {
                return false;
            }

            return buffFromUnitId == 0
                ? buffMgr.HaveBuff(buffId)
                : buffMgr.GetBuffSourceNumber(buffFromUnitId, buffId) > 0;
        }

        /// <summary>
        /// 批量改 CD 时过滤技能。occupation 只改职业技能。
        /// </summary>
        private static bool ShouldApplySkillCdFilter(SkillSetComponentServer skillSet, int skillId, string skillTypeFilter)
        {
            if (string.IsNullOrEmpty(skillTypeFilter)
                || skillTypeFilter.Equals("occupation", StringComparison.OrdinalIgnoreCase)
                || skillTypeFilter.Contains("职业"))
            {
                if (skillSet == null)
                {
                    return true;
                }

                SkillPro skillPro = skillSet.GetBySkillID(skillId);
                return skillPro == null || skillPro.SkillSource == SkillSourceEnum.Occupation;
            }

            return true;
        }

        /// <summary>
        /// 指定目标位移的异步移动。
        /// </summary>
        private static async ETTask DisplacementToTargetAsync(SkillEditorFunctionContext ctx)
        {
            Unit mover = ctx.ResolveUnit(ctx.GetParamRaw(1));
            Unit destUnit = ctx.ResolveUnit(ctx.GetParamRaw(2));
            float keepDistance = ctx.GetParamFloat(3, 0f);
            int moveType = ctx.GetParamInt(5, 0);
            float moveParam1 = ctx.GetParamFloat(6, 0f);
            bool faceTarget = ctx.GetParamBool(9, false);
            if (mover == null || destUnit == null)
            {
                return;
            }

            Vector3 dest = destUnit.Position;
            Vector3 dir = (mover.Position - destUnit.Position);
            if (dir.sqrMagnitude > 1e-6f)
            {
                dest = destUnit.Position + dir.normalized * keepDistance;
            }

            int speedRate = 100;
            if (moveType == 1 && moveParam1 > 0f)
            {
                float nowSpeed = mover.GetSpeedNow();
                if (nowSpeed > 1e-6f)
                {
                    speedRate = (int)(100f * moveParam1 / nowSpeed);
                }
            }

            int moveFlags = PathMoveFlags.NoRunAnim;
            long faceTargetId = 0;
            if (faceTarget)
            {
                moveFlags |= PathMoveFlags.FaceTargetOnArrive;
                faceTargetId = destUnit.Id;
            }

            await MoveUnitToPositionAsync(mover, dest, moveType, speedRate, moveFlags, faceTargetId);
        }

        /// <summary>
        /// 创建技能体。legacyFormat 为 true 时走旧参数。
        /// </summary>
        private static void CreateSummonInternal(SkillEditorFunctionContext ctx, bool legacyFormat)
        {
            int summonId = ctx.GetParamInt(0, 0);
            Unit caster = ctx.ResolveUnit(ctx.GetParamRaw(1));
            if (summonId <= 0 || caster == null || caster.IsDisposed || ctx.Handler == null)
            {
                return;
            }

            if (!LDSummonCategory.Instance.Contain(summonId))
            {
                Log.Error($"CREATE_SUMMON 找不到 LDSummon 配置: {summonId} skill={ctx.SkillId}");
                return;
            }

            LDSummon summonConfig = LDSummonCategory.Instance.Get(summonId);
            Scene scene = caster.DomainScene();
            if (scene?.GetComponent<UnitComponent>() == null)
            {
                return;
            }

            float x = ResolveScalarParam(ctx, ctx.GetParamRaw(2), 'x', caster.Position.x);
            float z = ResolveScalarParam(ctx, ctx.GetParamRaw(3), 'z', caster.Position.z);
            float dirX = ResolveDirectionParam(ctx, ctx.GetParamRaw(4), 'x');
            float dirZ = ResolveDirectionParam(ctx, ctx.GetParamRaw(5), 'z');

            SummonRuntimeData runtime = legacyFormat
                ? ParseLegacySummonRuntime(ctx, summonConfig)
                : ParseCreateSummonRuntime(ctx, summonConfig);

            Vector3 position = new Vector3(x, caster.Position.y, z);
            Vector3 forward = new Vector3(dirX, 0f, dirZ);
            if (forward.sqrMagnitude <= 1e-6f)
            {
                forward = caster.Rotation * Vector3.forward;
            }

            Quaternion rotation = Quaternion.LookRotation(forward.normalized, Vector3.up);
            Unit summonUnit = UnitFactory.CreateSkillEntity(scene, caster.Id, summonId, position, rotation);
            if (summonUnit == null)
            {
                return;
            }

            runtime.SummonId = summonId;
            if (runtime.TrackTargetId <= 0)
            {
                runtime.TrackTargetId = ctx.Handler.TheUnitTarget?.Id ?? 0;
            }

            SkillEntityComponent skillEntity = summonUnit.AddComponent<SkillEntityComponent>();
            skillEntity.Init(ctx.Handler, caster.Id, summonConfig, runtime);
            summonUnit.AddComponent<AOIEntity, int, Vector3>(9 * 1000, summonUnit.Position);
            skillEntity.FireCreateSkill();

            ctx.SetVariable("createSummon", summonUnit.Id.ToString(CultureInfo.InvariantCulture));
            if (Log.IsDebugEnabled)
            {
                Log.Debug($"CREATE_SUMMON skill={ctx.SkillId} summonId={summonId} unit={summonUnit.Id} create={runtime.CreateSkillId} action={runtime.ActionSkillId} track={runtime.TrackSkillId} destroy={runtime.DestroySkillId} move={runtime.MoveType}");
            }
        }

        /// <summary>
        /// 解析创建技能体的新参数。
        /// </summary>
        private static SummonRuntimeData ParseCreateSummonRuntime(SkillEditorFunctionContext ctx, LDSummon summonConfig)
        {
            // DocEditor「创建技能体」：0ID 1施法者 2x 3z 4dirX 5dirZ 6运动类型 7追踪目标 8碰到阻挡删除
            // 9最大持续时间ms 10作用间隔ms 11作用次数
            // 12创建技能 13创建LV 14作用技能 15作用LV 16追踪技能 17追踪LV 18消亡技能 19消亡LV
            // 20消亡-次数 21消亡-追踪消亡 22消亡-施法者死亡 23消亡-目标死亡
            SummonRuntimeData runtime = new SummonRuntimeData
            {
                MoveType = ctx.GetParamInt(6, 0),
                DeleteOnBlock = ctx.GetParamBool(8, false),
                MaxDurationMs = ctx.GetParamInt(9, 0),
                IntervalMs = ctx.GetParamInt(10, 0),
                MaxActionCount = ctx.GetParamInt(11, 0),
                CreateSkillId = ResolveSummonSkillId(ctx, 12, summonConfig, summonConfig.Skill_1),
                CreateSkillLevel = ctx.GetParamInt(13, ctx.SkillLevel),
                ActionSkillId = ResolveSummonSkillId(ctx, 14, summonConfig, summonConfig.Skill_2),
                ActionSkillLevel = ctx.GetParamInt(15, ctx.SkillLevel),
                TrackSkillId = ResolveSummonSkillId(ctx, 16, summonConfig, summonConfig.Skill_3),
                TrackSkillLevel = ctx.GetParamInt(17, ctx.SkillLevel),
                DestroySkillId = ResolveSummonSkillId(ctx, 18, summonConfig, summonConfig.Skill_4),
                DestroySkillLevel = ctx.GetParamInt(19, ctx.SkillLevel),
                DestroyOnCount = ctx.GetParamBool(20, true),
                DeleteOnTrackReach = ctx.GetParamBool(21, true),
                DestroyOnMasterDead = ctx.GetParamBool(22, false),
                DestroyOnTargetDead = ctx.GetParamBool(23, false),
            };

            Unit trackTarget = ctx.ResolveUnit(ctx.GetParamRaw(7));
            runtime.TrackTargetId = trackTarget?.Id ?? 0;
            return runtime;
        }

        /// <summary>
        /// 解析旧版 UNIT_ADD_SUMMON 参数。
        /// </summary>
        private static SummonRuntimeData ParseLegacySummonRuntime(SkillEditorFunctionContext ctx, LDSummon summonConfig)
        {
            float durationSec = ctx.GetParamFloat(9, 0f);
            float intervalSec = ctx.GetParamFloat(10, 0f);
            SummonRuntimeData runtime = new SummonRuntimeData
            {
                ActionType = ctx.GetParamInt(6, 0),
                MoveType = ctx.GetParamInt(7, 0),
                DestroyMode = NormalizeLegacyDestroyMode(ctx.GetParamInt(8, 101)),
                MaxDurationMs = durationSec > 0f ? (long)(durationSec * 1000f) : 0,
                IntervalMs = intervalSec > 0f ? (long)(intervalSec * 1000f) : 0,
                MaxActionCount = ctx.GetParamInt(11, 0),
                ActionSkillId = ResolveSummonSkillId(ctx, 12, summonConfig, summonConfig.Skill_1),
                ActionSkillLevel = ctx.GetParamInt(13, ctx.SkillLevel),
                TriggerOnCreate = ctx.GetParamBool(14, false),
                DeleteOnTrackReach = ctx.GetParamBool(15, false),
                DeleteOnBlock = ctx.GetParamBool(16, false),
                DestroySkillId = ResolveSummonSkillId(ctx, 17, summonConfig, summonConfig.Skill_2),
                DestroySkillLevel = ctx.GetParamInt(13, ctx.SkillLevel),
            };

            runtime.TrackTargetId = ctx.Handler?.TheUnitTarget?.Id ?? 0;
            runtime.DestroyOnCount = runtime.DestroyMode == SkillEntityDestroyMode.OnActionCount_1
                || runtime.DestroyMode == SkillEntityDestroyMode.OnActionCountOrMasterDead_11;
            runtime.DestroyOnMasterDead = runtime.DestroyMode == SkillEntityDestroyMode.OnMasterDead_10
                || runtime.DestroyMode == SkillEntityDestroyMode.OnActionCountOrMasterDead_11;
            return runtime;
        }

        /// <summary>
        /// 把旧消亡模式 100、101 转成现行枚举。
        /// </summary>
        private static int NormalizeLegacyDestroyMode(int destroyMode)
        {
            switch (destroyMode)
            {
                case 100:
                    return SkillEntityDestroyMode.OnMasterDead_10;
                case 101:
                    return SkillEntityDestroyMode.OnActionCountOrMasterDead_11;
                default:
                    return destroyMode;
            }
        }

        /// <summary>
        /// 解析技能体上的技能：Skill_1 到 Skill_4、当前技能，或直接填数字 ID。
        /// </summary>
        private static int ResolveSummonSkillId(
            SkillEditorFunctionContext ctx,
            int paramIndex,
            LDSummon summonConfig,
            int fallbackSkillId)
        {
            string raw = ctx.GetParamRaw(paramIndex);
            string skillCol = ctx.GetParamSkillIdColumn(paramIndex);
            string token = ctx.ResolveParam(string.IsNullOrEmpty(skillCol) ? raw : skillCol).Trim().ToLowerInvariant();

            if (string.IsNullOrEmpty(token) || token == "0")
            {
                return fallbackSkillId;
            }

            switch (token)
            {
                case "skill_1":
                case "skill1":
                    return summonConfig.Skill_1;
                case "skill_2":
                case "skill2":
                    return summonConfig.Skill_2;
                case "skill_3":
                case "skill3":
                    return summonConfig.Skill_3;
                case "skill_4":
                case "skill4":
                    return summonConfig.Skill_4;
                case "skillid":
                case "skill_id":
                    return ctx.SkillId;
            }

            return ctx.GetParamInt(paramIndex, fallbackSkillId);
        }

        /// <summary>
        /// 解析朝向。支持单位朝向 token，也支持直接填数。
        /// </summary>
        private static float ResolveDirectionParam(SkillEditorFunctionContext ctx, string raw, char defaultAxis)
        {
            string token = raw?.Trim() ?? string.Empty;
            if (token.Length == 0)
            {
                return 0f;
            }

            if (token.Contains(".dir_", StringComparison.Ordinal))
            {
                return ctx.ResolveDirectionComponent(raw, defaultAxis);
            }

            string resolved = ctx.ResolveParam(raw);
            return float.TryParse(resolved, NumberStyles.Float, CultureInfo.InvariantCulture, out float value) ? value : 0f;
        }

        /// <summary>
        /// 解析坐标。支持单位坐标 token，也支持直接填数。
        /// </summary>
        private static float ResolveScalarParam(SkillEditorFunctionContext ctx, string raw, char axis, float defaultValue)
        {
            string token = raw?.Trim() ?? string.Empty;
            if (token.Length == 0)
            {
                return defaultValue;
            }

            if (token.Contains(".x", StringComparison.Ordinal) || token.Contains(".z", StringComparison.Ordinal))
            {
                return ctx.ResolvePositionComponent(raw, axis);
            }

            string resolved = ctx.ResolveParam(raw);
            if (float.TryParse(resolved, NumberStyles.Float, CultureInfo.InvariantCulture, out float value))
            {
                return value;
            }

            return defaultValue;
        }

        /// <summary>
        /// 按移动类型把单位移到目标点。
        /// </summary>
        private static void MoveUnitToPosition(Unit unit, Vector3 dest, int moveType, int speedRate)
        {
            MoveUnitToPositionAsync(unit, dest, moveType, speedRate).Coroutine();
        }

        /// <summary>
        /// 传送或寻路。到达后可按标记朝向目标。
        /// </summary>
        private static async ETTask MoveUnitToPositionAsync(Unit unit, Vector3 dest, int moveType, int speedRate, int moveFlags = 0, long faceTargetId = 0)
        {
            if (unit == null || unit.IsDisposed)
            {
                return;
            }

            MapComponent map = unit.DomainScene()?.GetComponent<MapComponent>();
            Vector3 finalPos = map == null ? dest : map.GetCanChongJiPath(unit, unit.Position, dest);
            if (moveType == 0)
            {
                unit.Position = finalPos;
                if ((moveFlags & PathMoveFlags.FaceTargetOnArrive) != 0 && faceTargetId > 0)
                {
                    Unit faceTarget = unit.GetParent<UnitComponent>()?.Get(faceTargetId);
                    if (faceTarget != null && !faceTarget.IsDisposed)
                    {
                        FaceUnitToward(unit, faceTarget.Position);
                    }
                }

                unit.Stop(MoveStopTypeEnum.MoveStopTypeFu2);
                return;
            }

            await unit.FindPathMoveToAsync(finalPos, null, false, speedRate, moveFlags, faceTargetId);
        }

        /// <summary>
        /// 让单位朝向某个世界坐标。
        /// </summary>
        private static void FaceUnitToward(Unit unit, Vector3 worldPoint)
        {
            Vector3 dir = worldPoint - unit.Position;
            dir.y = 0f;
            if (dir.sqrMagnitude <= 1e-6f)
            {
                return;
            }

            unit.Rotation = Quaternion.LookRotation(dir.normalized, Vector3.up);
        }

        /// <summary>
        /// 触发单位被动技能。
        /// </summary>
        private static void TriggerPassiveEvent(Unit owner, Unit target, int passiveType, int skillId = 0)
        {
            if (owner == null || owner.IsDisposed)
            {
                return;
            }

            owner.GetComponent<SkillPassiveComponent>()?.OnTrigegerPassiveSkill(
                passiveType, target?.Id ?? 0, skillId > 0 ? skillId : 0);
        }

        /// <summary>
        /// 单位已释放、处于死亡状态，或当前生命小于等于 0。
        /// </summary>
        private static bool IsUnitDead(Unit unit)
        {
            if (unit == null || unit.IsDisposed)
            {
                return true;
            }

            NumericComponent numeric = unit.GetComponent<NumericComponent>();
            if (numeric == null)
            {
                return false;
            }

            return numeric.GetAsInt(NumericType.Now_Dead) == 1
                || numeric.GetAsLong(NumericType.HP_Current_8) <= 0;
        }

        /// <summary>
        /// 该 BUFF 配置是否允许被清除。
        /// </summary>
        private static bool IsBuffClearable(LDSkill_Battle_Buff buffConfig)
        {
           // if (buffConfig?.Remove == null || buffConfig.Remove.Length == 0)
            {
                return false;
            }

            //return !(buffConfig.Remove.Length == 1 && buffConfig.Remove[0] == 0);
        }

    }
}

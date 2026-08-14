using Alipay.AopSdk.Core.Domain;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

namespace ET
{
    /// <summary>
    /// SkillEditor function handlers. Regenerate registry from: tools/generate_skill_editor_functions.ps1
    /// </summary>
    public static class SkillEditorFunctions
    {
        public static void RegisterAll()
        {
            SkillEditorFunctionRegistry.Register("DEFINE_VARIABLE", DefineVariable);        //定义变量
            SkillEditorFunctionRegistry.Register("SET_VARIABLE_VALUE", SetVariableValue);        //变量赋值
            SkillEditorFunctionRegistry.Register("DEFINE_VARIABLE_RAMDOM_VALUE", DefineVariableRamdomValue);        //定义变量-随机值
            SkillEditorFunctionRegistry.Register("CHANCE_TRIGGER", ChanceTrigger);        //概率触发
            SkillEditorFunctionRegistry.Register("LOGIC_RELATION", LogicRelation);        //逻辑运算
            SkillEditorFunctionRegistry.Register("BREAK", BreakLoop);        //跳出循环
            SkillEditorFunctionRegistry.Register("RETURN_TRUE", ReturnTrue);        //返回成功值
            SkillEditorFunctionRegistry.Register("DESTROY_UNIT", DestroyUnit);        //销毁单位
            SkillEditorFunctionRegistry.Register("INFORM_EVENT_ATTACK", InformEventAttack);        //通知攻击事件
            SkillEditorFunctionRegistry.Register("INFORM_EVENT_DEFENSE", InformEventDefense);        //通知防御事件
            SkillEditorFunctionRegistry.Register("INFORM_EVENT_HEAL", InformEventHeal);        //通知治疗事件
            SkillEditorFunctionRegistry.Register("INFORM_CLIENT_HIT_SUCCESS", InformClientHitSuccess);        //通知客户端命中
            SkillEditorFunctionRegistry.Register("SKILL_DAMAGE_CHECK_PHYSICS", SkillDamageCheckPhysics);        //物理技能判定
            SkillEditorFunctionRegistry.Register("SKILL_DAMAGE_CHECK_MAGIC", SkillDamageCheckMagic);        //法术技能判定
            SkillEditorFunctionRegistry.Register("SKILL_DAMAGE_CHECK_HEAL", SkillDamageCheckHeal);        //治疗技能判定
            SkillEditorFunctionRegistry.Register("CALCULATE_PHYSICS_DAMAGE", CalculatePhysicsDamage);        //计算物理伤害
            SkillEditorFunctionRegistry.Register("CALCULATE_MAGIC_DAMAGE", CalculateMagicDamage);        //计算法术伤害
            SkillEditorFunctionRegistry.Register("CALCULATE_HEAL_DAMAGE", CalculateHealDamage);        //计算治疗量
            SkillEditorFunctionRegistry.Register("ADD_BUFF", AddBuff);        //添加BUFF
            SkillEditorFunctionRegistry.Register("ADD_DEBUFF_NO_CONTROL", AddDebuffNoControl);        //添加负面BUFF
            SkillEditorFunctionRegistry.Register("ADD_DEBUFF_CONTROL", AddDebuffControl);        //添加控制BUFF
            SkillEditorFunctionRegistry.Register("ADD_BUFF_CONTROL", AddBuffControl);        //旧版添加控制BUFF
            SkillEditorFunctionRegistry.Register("REMOVE_BUFF", RemoveBuff);        //移除BUFF
            SkillEditorFunctionRegistry.Register("REMOVE_BUFF_RANDOM", RemoveBuffRandom);        //移除BUFF-随机
            SkillEditorFunctionRegistry.Register("REMOVE_BUFF_GROUP", RemoveBuffGroup);        //移除BUFF组
            SkillEditorFunctionRegistry.Register("BUFF_DATA_SET", BuffDataSet);        //BUFF-数据设置
            SkillEditorFunctionRegistry.Register("BUFF_DATA_GET", BuffDataGet);        //BUFF-数据获取
            SkillEditorFunctionRegistry.Register("SET_BUFF_DATA", SetBuffDataLegacy);        //旧版 BUFF-数据设置
            SkillEditorFunctionRegistry.Register("GET_BUFF_DATA", GetBuffDataLegacy);        //旧版 BUFF-数据获取
            SkillEditorFunctionRegistry.Register("GET_UNIT_TYPEID", GetUnitTypeId);        //获取对象类型
            SkillEditorFunctionRegistry.Register("GET_UNIT_LEVEL", GetUnitLevel);        //获取单位等级
            SkillEditorFunctionRegistry.Register("GET_UNIT_ATTRIBUTE", GetUnitAttribute);        //获取单位属性
            SkillEditorFunctionRegistry.Register("GET_HP_PERCENT", GetHpPercent);        //获取HP百分比
            SkillEditorFunctionRegistry.Register("GET_SKILL_LEVEL", GetSkillLevel);        //获取技能等级
            SkillEditorFunctionRegistry.Register("GET_BUFF_LEVEL", GetBuffLevel);        //旧版获取BUFF层数
            SkillEditorFunctionRegistry.Register("GET_BUFF_STACK", GetBuffLevel);        //获取BUFF层数
            SkillEditorFunctionRegistry.Register("GET_RANDOM_UNIT", GetRandomUnit);        //旧版获取随机目标
            SkillEditorFunctionRegistry.Register("GET_RANDOM_TARGET", GetRandomUnit);        //获取随机目标
            SkillEditorFunctionRegistry.Register("GET_POINT_DISTANCE", GetPointDistance);        //获取两点之间距离
            SkillEditorFunctionRegistry.Register("GET_POINT_DIRECTION", GetPointDirection);        //获取两点之间朝向
            SkillEditorFunctionRegistry.Register("IS_DEAD", IsDead);        //是否死亡
            SkillEditorFunctionRegistry.Register("IS_DYING", IsDying);        //是否濒死
            SkillEditorFunctionRegistry.Register("OWN_BUFF", OwnBuff);        //是否拥有BUFF
            SkillEditorFunctionRegistry.Register("HAS_BUFF", HasBuffLegacy);        //旧版是否拥有BUFF
            SkillEditorFunctionRegistry.Register("CHANGE_GLOBAL_CD", ChangeGlobalCd);        //修改公共CD
            SkillEditorFunctionRegistry.Register("CHANGE_CURRENT_HP", ChangeCurrentHp);        //修改生命值
            SkillEditorFunctionRegistry.Register("CHANGE_UNIT_ATTRIBUTE_ADD", ChangeUnitAttributeAdd);        //修改属性-固定值
            SkillEditorFunctionRegistry.Register("CHANGE_UNIT_ATTRIBUTE_PERCENT", ChangeUnitAttributePercent);        //修改属性-比例值
            SkillEditorFunctionRegistry.Register("CHANGE_SKILL_CURRENT_CD", ChangeSkillCurrentCd);        //修改技能当前CD
            SkillEditorFunctionRegistry.Register("CHANGE_SKILL_CURRENT_CD_MULTIPLE", ChangeSkillCurrentCdMultiple);        //修改技能当前CD-批量
            SkillEditorFunctionRegistry.Register("SET_UNIT_CHOOSE_STATUS", SetUnitChooseStatus);        //设置无法选中状态
            SkillEditorFunctionRegistry.Register("SET_UNIT_MOCK_TARGET", SetUnitMockTarget);        //设置嘲讽目标
            SkillEditorFunctionRegistry.Register("DEL_UNIT_MOCK_TARGET", DelUnitMockTarget);        //取消嘲讽目标
            SkillEditorFunctionRegistry.Register("DISPLACEMENT_TO_TARGET", DisplacementToTarget);        //指定目标位移
            SkillEditorFunctionRegistry.Register("DISPLACEMENT_TO_DIRECTION", DisplacementToDirection);        //指定朝向位移
            SkillEditorFunctionRegistry.Register("DISPLACEMENT_TO_POINT", DisplacementToPoint);        //指定地点位移
            SkillEditorFunctionRegistry.Register("SET_UNIT_DIRECTION", SetUnitDirection);        //设置单位朝向
            SkillEditorFunctionRegistry.Register("SET_DIR", SetUnitDirection);        //旧版设置单位朝向
            SkillEditorFunctionRegistry.Register("CREATE_SUMMON", CreateSummon);        //创建技能体（新参数，读 LDSummon）
            SkillEditorFunctionRegistry.Register("UNIT_ADD_SUMMON", CreateSummonLegacy);        //旧版创建技能体
            SkillEditorFunctionRegistry.Register("SET_SUMMON_TARGET", SetSummonTarget);        //设置技能体目标
            SkillEditorFunctionRegistry.Register("SET_UNIT_TARGET", SetSummonTarget);        //旧版设置技能体目标
        }

        private static void DefineVariable(SkillEditorFunctionContext ctx)
        {

            string varName = ctx.ResolveVarName(ctx.GetParamRaw(0));

            if (string.IsNullOrEmpty(varName)) { return; }

            // Keep literal as string (false / 0.5 / 100) for LOGIC_RELATION comparisons.

            ctx.SetVariable(varName, ctx.GetParamRaw(1).Trim());

        }

        private static void SetVariableValue(SkillEditorFunctionContext ctx)
        {

            string varName = ctx.ResolveVarName(ctx.GetParamRaw(0));

            if (string.IsNullOrEmpty(varName)) { return; }

            ctx.SetVariable(varName, ctx.ResolveParam(ctx.GetParamRaw(1)));

        }


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


        private static void ChanceTrigger(SkillEditorFunctionContext ctx)
        {
            int rate = ctx.GetParamInt(0, 10000);

            if (rate < 0) { rate = 0; }

            if (rate > 10000) { rate = 10000; }

            bool hit = rate >= 10000 || RandomHelper.RandomNumber(0, 10000) < rate;

            ctx.LastConditionResult = hit;

            ctx.SetVariable("rs", hit ? 1 : 0);

        }


        private static void LogicRelation(SkillEditorFunctionContext ctx)
        {

            string left = ctx.ResolveParam(ctx.GetParamRaw(0));

            string op = ctx.GetParamRaw(1).Trim();

            string right = ctx.ResolveParam(ctx.GetParamRaw(2));

            bool result = CompareLogic(left, right, op);

            ctx.LastConditionResult = result;

            ctx.SetVariable("rs", result ? 1 : 0);
        }


        private static void BreakLoop(SkillEditorFunctionContext ctx)
        {
            ctx.SetVariable("__break", 1);
        }


        private static void ReturnTrue(SkillEditorFunctionContext ctx)
        {

            ctx.LastConditionResult = true;

            ctx.SetVariable("rs", 1);

        }


        /// <summary>
        /// 通知目标触发攻击事件  。   所有的操作基本都是对目标的
        /// </summary>
        /// <param name="ctx"></param>
        private static void InformEventAttack(SkillEditorFunctionContext ctx)
        {
            Unit caster = ctx.ResolveUnit(ctx.GetParamRaw(0));
            Unit target = ctx.ResolveUnit(ctx.GetParamRaw(1));

            target?.GetComponent<SkillPassiveComponent>()?.OnTrigegerPassiveSkill(
                    SkillPassiveTypeEnum.AttackAll, caster.Id, ctx.SkillId);

            if (Log.IsDebugEnabled) Log.Debug($"INFORM_EVENT_ATTACK skill={ctx.SkillId} caster={(caster?.Id ?? 0)} target={(target?.Id ?? 0)}");
        }


        /// <summary>
        /// 通知目标触发触发防御事件  。   所有的操作基本都是对目标的
        /// </summary>
        /// <param name="ctx"></param>
        private static void InformEventDefense(SkillEditorFunctionContext ctx)
        {
            Unit caster = ctx.ResolveUnit(ctx.GetParamRaw(0));
            Unit target = ctx.ResolveUnit(ctx.GetParamRaw(1));

            //target?.GetComponent<SkillPassiveComponent>()?.OnTrigegerPassiveSkill(SkillPassiveTypeEnum.AttackAll, caster.Id, ctx.SkillId);
            //TriggerPassiveEvent(target, caster, SkillPassiveTypeEnum.None);
            if (Log.IsDebugEnabled) Log.Debug($"INFORM_EVENT_DEFENSE skill={ctx.SkillId} caster={(caster?.Id ?? 0)} target={(target?.Id ?? 0)}");
        }

        /// <summary>
        /// 通知治疗事件：瓢字用 CALCULATE_HEAL_DAMAGE 写入的 healTotal/damageTotal。
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
        /// 通知客户端命中效果。
        /// 有以下很多类型、、。
        /// 比方这个闪避。  
        /// </summary>
        /// <param name="ctx"></param>
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


        private static void SkillDamageCheckPhysics(SkillEditorFunctionContext ctx)
        {
            RunDamageCheck(ctx, canBlockAsImmune: true);
        }


        private static void SkillDamageCheckMagic(SkillEditorFunctionContext ctx)

        {

            RunDamageCheck(ctx, canBlockAsImmune: true);

        }



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

            if (canCrit && RandomHelper.RandomNumber(0, 10000) < 500 + critRateAdd)

            {

                rs = (long)SkillEditorHitResult.Crit;

            }

            ctx.SetVariable("rs", rs);

            ctx.LastConditionResult = rs > 0;

            if (Log.IsDebugEnabled) Log.Debug($"SKILL_DAMAGE_CHECK_HEAL skill={skillId} level={level} caster={caster.Id} target={target.Id} rs={rs}");

        }



        private static void AddBuff(SkillEditorFunctionContext ctx)

        {

            Unit caster = ctx.ResolveUnit(ctx.GetParamRaw(0));

            Unit target = ctx.ResolveUnit(ctx.GetParamRaw(1));

            int buffId = ctx.GetParamInt(2, 0);

            int intervalMs = ctx.GetParamInt(3, 0);

            int tickCount = ctx.GetParamInt(4, 1);

            bool ignoreImmune = ctx.GetParamBool(5, false);

            int buffLevel = ctx.GetParamInt(6, ctx.SkillLevel);

            ApplyBuff(ctx, caster, target, buffId, intervalMs, tickCount, ignoreImmune, buffLevel, null);

        }



        private static void AddBuffControl(SkillEditorFunctionContext ctx)

        {

            Unit caster = ctx.ResolveUnit(ctx.GetParamRaw(0));

            Unit target = ctx.ResolveUnit(ctx.GetParamRaw(1));

            int buffId = ctx.GetParamInt(2, 0);

            int intervalMs = ctx.GetParamInt(3, 0);

            int tickCount = ctx.GetParamInt(4, 1);

            string controlType = ctx.ResolveParam(ctx.GetParamRaw(5));

            bool ignoreImmune = ctx.GetParamBool(6, false);

            int buffLevel = ctx.GetParamInt(7, ctx.SkillLevel);

            ApplyBuff(ctx, caster, target, buffId, intervalMs, tickCount, ignoreImmune, buffLevel, controlType);

        }



        private static void AddDebuffNoControl(SkillEditorFunctionContext ctx)
        {
            AddDebuffInternal(ctx, isControl: false);
        }

        private static void AddDebuffControl(SkillEditorFunctionContext ctx)
        {
            AddDebuffInternal(ctx, isControl: true);
        }

        private static void AddDebuffInternal(SkillEditorFunctionContext ctx, bool isControl)
        {
            Unit caster = ctx.ResolveUnit(ctx.GetParamRaw(0));
            Unit target = ctx.ResolveUnit(ctx.GetParamRaw(1));
            int baseHitRate = ctx.GetParamInt(2, 10000);
            int buffId = ctx.GetParamInt(3, 0);
            int intervalMs = ctx.GetParamInt(4, 0);
            int tickCount = ctx.GetParamInt(5, 1);
            string effectType = ctx.ResolveParam(ctx.GetParamRaw(6));
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

            ApplyBuff(
                ctx,
                caster,
                target,
                buffId,
                intervalMs,
                tickCount,
                ignoreImmune,
                buffLevel,
                isControl ? effectType : null);
            ctx.SetVariable("rs", 1);
            ctx.LastConditionResult = true;
        }

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

        private static int ResolveDebuffDurationBonus(Unit caster, Unit target)
        {
            if (caster == null || target == null)
            {
                return 0;
            }

            int levelDelta = ResolveUnitLevel(caster) - ResolveUnitLevel(target);
            return levelDelta / 5;
        }



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
        /// 
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="canBlockAsImmune"></param>
        private static void RunDamageCheck(SkillEditorFunctionContext ctx, bool canBlockAsImmune)
        {

            Unit caster = ctx.ResolveUnit(ctx.GetParamRaw(0));

            Unit target = ctx.ResolveUnit(ctx.GetParamRaw(1));

            int skillId = ctx.GetParamInt(2, ctx.SkillId);

            int level = ctx.GetParamInt(3, ctx.SkillLevel);

            bool canCrit = ctx.GetParamBool(4, true);

            bool canHeavy = ctx.GetParamBool(5, true);

            bool canDodge = ctx.GetParamBool(6, true);

            bool canBlock = ctx.GetParamBool(7, true);

            int critRateAdd = ctx.GetParamInt(8, 0);

            int heavyRateAdd = ctx.GetParamInt(9, 0);

            int hitRateAdd = ctx.GetParamInt(10, 0);

            if (caster == null || target == null)
            {
                ctx.SetVariable("rs", (long)SkillEditorHitResult.Miss);

                ctx.LastConditionResult = false;

                return;
            }

            bool canImmune = canBlockAsImmune && canBlock;

            long rs = SkillEditorContionHelper.EvaluateDirectHit(

                ctx, caster, target, skillId,

                canCrit, canImmune, canDodge,

                critRateAdd + (canHeavy ? heavyRateAdd : 0),

                hitRateAdd, level, 0f, 0f, true);

            // Miss=0 Hit=1 Immune=2 Dodge=3 Crit=11
            ctx.SetVariable("rs", rs);

            // 仅写判定结果；瓢字由技能树 INFORM_CLIENT_HIT_SUCCESS 负责，判定节点不通知客户端
            bool hitOk = rs == (long)SkillEditorHitResult.Hit || rs >= (long)SkillEditorHitResult.Crit;
            ctx.LastConditionResult = hitOk;
        }


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



        private static void RemoveBuffById(Unit target, long buffFromUnitId, int buffId, bool forceRemove, bool triggerFadeSkill)

        {

            BuffManagerComponent buffMgr = target.GetComponent<BuffManagerComponent>();

            if (buffMgr == null || buffId <= 0) { return; }

            buffMgr.BuffRemoveByUnit(buffFromUnitId, buffId);

            if (Log.IsDebugEnabled) Log.Debug($"REMOVE_BUFF target={target.Id} buffId={buffId} from={buffFromUnitId} force={forceRemove} fade={triggerFadeSkill}");

        }



        private static void RemoveBuffRandomInternal(Unit target, long buffFromUnitId, string benefitFilter, int removeCount, int clearRate, bool excludeNonClearable, bool forceRemove, bool triggerFadeSkill)

        {

            BuffManagerComponent buffMgr = target.GetComponent<BuffManagerComponent>();

            if (buffMgr == null || removeCount <= 0) { return; }

            HashSet<int> allowedBenefits = ParseBenefitFilter(benefitFilter);

            List<int> candidates = new List<int>();

            for (int i = 0; i < buffMgr.m_Buffs.Count; i++)

            {

                BuffHandler bh = buffMgr.m_Buffs[i];

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

                int idx = RandomHelper.RandomNumber(0, candidates.Count - 1);

                int buffId = candidates[idx];

                candidates.RemoveAt(idx);

                RemoveBuffById(target, buffFromUnitId, buffId, forceRemove, triggerFadeSkill);

            }

        }



        private static void RemoveBuffGroupInternal(Unit target, long buffFromUnitId, int buffGroup, bool forceRemove, bool triggerFadeSkill)

        {

            BuffManagerComponent buffMgr = target.GetComponent<BuffManagerComponent>();

            if (buffMgr == null || buffGroup <= 0) { return; }

            for (int i = buffMgr.m_Buffs.Count - 1; i >= 0; i--)

            {

                BuffHandler bh = buffMgr.m_Buffs[i];

                if (bh?.MBuff == null) { continue; }

                if (buffFromUnitId != 0 && bh.TheUnitFrom?.Id != buffFromUnitId) { continue; }

                //if (bh.MBuff.Remove == null || !bh.MBuff.Remove.Contains(buffGroup)) { continue; }

                buffMgr.OnRemoveBuffItem(bh);

                buffMgr.m_Buffs.RemoveAt(i);

            }

            if (Log.IsDebugEnabled) Log.Debug($"REMOVE_BUFF_GROUP target={target.Id} group={buffGroup} force={forceRemove} fade={triggerFadeSkill}");

        }



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



        private static void ApplyBuff(

            SkillEditorFunctionContext ctx,

            Unit caster,

            Unit target,

            int buffId,

            int intervalMs,

            int tickCount,

            bool ignoreImmune,

            int buffLevel,

            string controlType)

        {

            if (target == null || buffId <= 0) { return; }

            BuffManagerComponent buffMgr = target.GetComponent<BuffManagerComponent>();

            if (buffMgr == null) { return; }

            if (!ignoreImmune && buffMgr.IsSkillImmune(ctx.SkillId)) { return; }

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

            buffMgr.BuffFactory(buffData, caster, ctx.Handler);
            if (Log.IsDebugEnabled) Log.Debug($"ApplyBuff target={target.Id} buffId={buffId} interval={intervalMs} ticks={tickCount} level={buffLevel} control={controlType ?? string.Empty}");
        }

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

        private static void CalculatePhysicsDamage(SkillEditorFunctionContext ctx)
        {
            SkillEditorDamageHelper.CalculateDamage(ctx, SkillEditorDamageKind.Physics);
        }

        private static void CalculateMagicDamage(SkillEditorFunctionContext ctx)
        {
            SkillEditorDamageHelper.CalculateDamage(ctx, SkillEditorDamageKind.Magic);
        }

        private static void CalculateHealDamage(SkillEditorFunctionContext ctx)
        {
            SkillEditorDamageHelper.CalculateHeal(ctx);
        }

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

        private static void GetUnitTypeId(SkillEditorFunctionContext ctx)
        {
            string varName = ctx.ResolveVarName(ctx.GetParamRaw(0), "nUnitTypeId");
            Unit unit = ctx.ResolveUnit(ctx.GetParamRaw(1));
            ctx.SetVariable(varName, unit == null ? 0 : (int)unit.Type);
        }

        private static void GetUnitLevel(SkillEditorFunctionContext ctx)
        {
            string varName = ctx.ResolveVarName(ctx.GetParamRaw(0), "nUnitLevel");
            Unit unit = ctx.ResolveUnit(ctx.GetParamRaw(1));
            ctx.SetVariable(varName, ResolveUnitLevel(unit));
        }

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

            if (unit.Type == UnitType.Pet || unit.Type == UnitType.JingLing)
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

        private static void GetRandomUnit(SkillEditorFunctionContext ctx)
        {
            bool usePriority = ctx.GetParamBool(2, true);
            List<long> targetIds = CollectTargetIds(ctx, usePriority, ctx.GetParamRaw(1));
            if (targetIds.Count == 0)
            {
                return;
            }

            int index = RandomHelper.RandomNumber(0, targetIds.Count - 1);
            UnitComponent unitComponent = ctx.Handler?.TheUnitFrom?.GetParent<UnitComponent>();
            Unit randomTarget = unitComponent?.Get(targetIds[index]);
            if (randomTarget == null)
            {
                return;
            }

            ctx.Handler.TheUnitTarget = randomTarget;
            string varName = ctx.ResolveVarName(ctx.GetParamRaw(0), "target");
            ctx.SetVariable(varName, randomTarget.Id.ToString(CultureInfo.InvariantCulture));
        }

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

        private static void IsDead(SkillEditorFunctionContext ctx)
        {
            Unit unit = ctx.ResolveUnit(ctx.GetParamRaw(0));
            bool dead = IsUnitDead(unit);
            ctx.LastConditionResult = dead;
            ctx.SetVariable("rs", dead ? 1 : 0);
        }

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

        private static void OwnBuff(SkillEditorFunctionContext ctx)
        {
            Unit unit = ctx.ResolveUnit(ctx.GetParamRaw(0));
            long buffFromUnitId = ResolveBuffSourceUnitId(ctx.GetParamRaw(1));
            int buffId = ctx.GetParamInt(2, 0);
            bool owned = EvaluateOwnBuff(unit, buffFromUnitId, buffId);
            ctx.LastConditionResult = owned;
            ctx.SetVariable("rs", owned ? 1 : 0);
        }

        private static void HasBuffLegacy(SkillEditorFunctionContext ctx)
        {
            Unit unit = ctx.ResolveUnit(ctx.GetParamRaw(0));
            int buffId = ctx.GetParamInt(1, 0);
            bool owned = EvaluateOwnBuff(unit, 0, buffId);
            ctx.LastConditionResult = owned;
            ctx.SetVariable("rs", owned ? 1 : 0);
        }

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

        private static void ChangeGlobalCd(SkillEditorFunctionContext ctx)
        {
            Unit unit = ctx.ResolveUnit(ctx.GetParamRaw(0));
            int deltaMs = ctx.GetParamInt(1, 0);
            SkillManagerComponent skillMgr = unit?.GetComponent<SkillManagerComponent>();
            if (skillMgr == null)
            {
                return;
            }

            skillMgr.SkillPublicCDTime += deltaMs;
            if (Log.IsDebugEnabled) Log.Debug($"CHANGE_GLOBAL_CD unit={unit.Id} deltaMs={deltaMs} publicCd={skillMgr.SkillPublicCDTime}");
        }

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

        private static void SetUnitChooseStatus(SkillEditorFunctionContext ctx)
        {
            Unit unit = ctx.ResolveUnit(ctx.GetParamRaw(0));
            int status = ctx.GetParamInt(1, 0);
            StateComponent stateComponent = unit?.GetComponent<StateComponent>();
            if (stateComponent == null)
            {
                return;
            }

            if (status == 0)
            {
                stateComponent.StateTypeRemove(StateTypeEnum.Stealth);
                stateComponent.StateTypeRemove(StateTypeEnum.Hide);
            }
            else
            {
                stateComponent.StateTypeAdd(StateTypeEnum.Stealth);
            }
        }

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


        /// <summary>技能：固定值改属性，配表 ID 直接传（21→100211，27→分项，80→StaticStore）。</summary>
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
        }

        /// <summary>技能：百分比改属性，percent=10 表示 +10%（21→100212，80→100802）。</summary>
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

        private static void DisplacementToTarget(SkillEditorFunctionContext ctx)
        {
            DisplacementToTargetAsync(ctx).Coroutine();
        }

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


        /// <summary>
        /// 创建技能体
        /// </summary>
        /// <param name="ctx"></param>
        private static void CreateSummon(SkillEditorFunctionContext ctx)
        {
            CreateSummonInternal(ctx, legacyFormat: false);
        }

        private static void CreateSummonLegacy(SkillEditorFunctionContext ctx)
        {
            CreateSummonInternal(ctx, legacyFormat: true);
        }

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

            // 碰撞作用技能固定用表 Skill_1
            if (summonConfig.Skill_1 > 0)
            {
                runtime.ActionSkillId = summonConfig.Skill_1;
            }

            SkillEntityComponent skillEntity = summonUnit.AddComponent<SkillEntityComponent>();
            skillEntity.Init(ctx.Handler, caster.Id, summonConfig, runtime);
            summonUnit.AddComponent<AOIEntity, int, Vector3>(9 * 1000, summonUnit.Position);

            ctx.SetVariable("createSummon", summonUnit.Id.ToString(CultureInfo.InvariantCulture));
            Log.Info($"CREATE_SUMMON skill={ctx.SkillId} summonId={summonId} unit={summonUnit.Id} skill_1={runtime.ActionSkillId} move={runtime.MoveType} track={runtime.TrackTargetId}");
        }

        private static SummonRuntimeData ParseCreateSummonRuntime(SkillEditorFunctionContext ctx, LDSummon summonConfig)
        {
            // 与 DocEditor「创建技能体」参数顺序一致（从 0 起）：
            // 0技能体ID 1施法者 2x 3z 4dirX 5dirZ 6作用类型 7运动类型 8追踪目标
            // 9碰到阻挡删除 10最大持续时间ms 11作用间隔ms 12作用次数 13创建时触发
            // 14作用技能 15作用等级 16消亡-次数 17消亡-施法者死亡 18消亡-目标死亡 19消亡技能 20消亡等级
            SummonRuntimeData runtime = new SummonRuntimeData
            {
                ActionType = ctx.GetParamInt(6, 0),
                MoveType = ctx.GetParamInt(7, 0),
                DeleteOnBlock = ctx.GetParamBool(9, false),
                MaxDurationMs = ctx.GetParamInt(10, 0),
                IntervalMs = ctx.GetParamInt(11, 0),
                MaxActionCount = ctx.GetParamInt(12, 0),
                TriggerOnCreate = ctx.GetParamBool(13, false),
                ActionSkillId = ResolveSummonSkillId(ctx, 14, summonConfig, summonConfig.Skill_1),
                ActionSkillLevel = ctx.GetParamInt(15, ctx.SkillLevel),
                DestroySkillId = ResolveSummonSkillId(ctx, 19, summonConfig, summonConfig.Skill_2),
                DestroySkillLevel = ctx.GetParamInt(20, ctx.SkillLevel),
            };

            bool destroyOnCount = ctx.GetParamBool(16, true);
            bool destroyOnCasterDead = ctx.GetParamBool(17, false);
            if (destroyOnCount && destroyOnCasterDead)
            {
                runtime.DestroyMode = SkillEntityDestroyMode.OnActionCountOrMasterDead_11;
            }
            else if (destroyOnCasterDead)
            {
                runtime.DestroyMode = SkillEntityDestroyMode.OnMasterDead_10;
            }
            else if (destroyOnCount)
            {
                runtime.DestroyMode = SkillEntityDestroyMode.OnActionCount_1;
            }
            else
            {
                runtime.DestroyMode = SkillEntityDestroyMode.None_0;
            }

            Unit trackTarget = ctx.ResolveUnit(ctx.GetParamRaw(8));
            runtime.TrackTargetId = trackTarget?.Id ?? 0;
            if (runtime.ActionSkillId <= 0)
            {
                runtime.ActionSkillId = summonConfig.Skill_1;
            }

            return runtime;
        }

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
            return runtime;
        }

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
                case "skillid":
                case "skill_id":
                    return ctx.SkillId;
            }

            return ctx.GetParamInt(paramIndex, fallbackSkillId);
        }

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

        private static void MoveUnitToPosition(Unit unit, Vector3 dest, int moveType, int speedRate)
        {
            MoveUnitToPositionAsync(unit, dest, moveType, speedRate).Coroutine();
        }

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

                unit.Stop(-2);
                return;
            }

            await unit.FindPathMoveToAsync(finalPos, null, false, speedRate, moveFlags, faceTargetId);
        }

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

        private static void TriggerPassiveEvent(Unit owner, Unit target, int passiveType, int skillId = 0)
        {
            if (owner == null || owner.IsDisposed)
            {
                return;
            }

            owner.GetComponent<SkillPassiveComponent>()?.OnTrigegerPassiveSkill(
                passiveType, target?.Id ?? 0, skillId > 0 ? skillId : 0);
        }

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

        private static bool IsBuffClearable(LDSkill_Battle_Buff buffConfig)
        {
           // if (buffConfig?.Remove == null || buffConfig.Remove.Length == 0)
            {
                return false;
            }

            //return !(buffConfig.Remove.Length == 1 && buffConfig.Remove[0] == 0);
        }

        private static List<long> CollectTargetIds(SkillEditorFunctionContext ctx, bool usePriority, string targetsParam = null)
        {
            List<long> targetIds = ParseTargetIdList(ctx, targetsParam);
            if (targetIds.Count > 0)
            {
                return targetIds;
            }

            if (ctx.Handler?.HurtIds != null && ctx.Handler.HurtIds.Count > 0)
            {
                targetIds.AddRange(ctx.Handler.HurtIds);
                return targetIds;
            }

            long targetId = ctx.Handler?.SkillInfo?.TargetID ?? 0;
            if (targetId > 0)
            {
                targetIds.Add(targetId);
            }

            return targetIds;
        }

        private static List<long> ParseTargetIdList(SkillEditorFunctionContext ctx, string raw)
        {
            List<long> targetIds = new List<long>();
            if (string.IsNullOrWhiteSpace(raw))
            {
                return targetIds;
            }

            string resolved = ctx.ResolveParam(raw).Trim();
            string token = ExtractListToken(raw);
            if (resolved.Equals("targets", StringComparison.OrdinalIgnoreCase)
                || token.Equals("targets", StringComparison.OrdinalIgnoreCase))
            {
                if (ctx.Handler?.HurtIds != null && ctx.Handler.HurtIds.Count > 0)
                {
                    targetIds.AddRange(ctx.Handler.HurtIds);
                }

                return targetIds;
            }

            if (ctx.Variables.TryGetValue(token, out string variableValue))
            {
                AppendParsedIds(variableValue, targetIds);
                if (targetIds.Count > 0)
                {
                    return targetIds;
                }
            }

            AppendParsedIds(resolved, targetIds);
            return targetIds;
        }

        private static string ExtractListToken(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                return string.Empty;
            }

            string trimmed = raw.Trim();
            int space = trimmed.LastIndexOf(' ');
            if (space >= 0 && space < trimmed.Length - 1)
            {
                return trimmed.Substring(space + 1).Trim();
            }

            return trimmed;
        }

        private static void AppendParsedIds(string raw, List<long> targetIds)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                return;
            }

            string[] parts = raw.Split(new[] { ',', '|', ';' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < parts.Length; i++)
            {
                if (long.TryParse(parts[i].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out long id)
                    && id > 0
                    && !targetIds.Contains(id))
                {
                    targetIds.Add(id);
                }
            }
        }
    }
}

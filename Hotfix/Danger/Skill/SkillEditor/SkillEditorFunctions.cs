using System;
using System.Collections.Generic;

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
            SkillEditorFunctionRegistry.Register("INFORM_EVENT_ATTACK", InformEventAttack);        //通知攻击事件
            SkillEditorFunctionRegistry.Register("INFORM_EVENT_DEFENSE", InformEventDefense);        //通知防御事件
            SkillEditorFunctionRegistry.Register("INFORM_CLIENT_HIT_SUCCESS", InformClientHitSuccess);        //通知客户端命中
            SkillEditorFunctionRegistry.Register("SKILL_DAMAGE_CHECK_PHYSICS", SkillDamageCheckPhysics);        //物理技能判定
            SkillEditorFunctionRegistry.Register("SKILL_DAMAGE_CHECK_MAGIC", SkillDamageCheckMagic);        //法术技能判定
            SkillEditorFunctionRegistry.Register("SKILL_DAMAGE_CHECK_HEAL", SkillDamageCheckHeal);        //治疗技能判定
            SkillEditorFunctionRegistry.Register("ADD_BUFF", AddBuff);        //添加BUFF
            SkillEditorFunctionRegistry.Register("ADD_BUFF_CONTROL", AddBuffControl);        //添加控制BUFF
            SkillEditorFunctionRegistry.Register("REMOVE_BUFF", RemoveBuff);        //移除BUFF
            SkillEditorFunctionRegistry.Register("REMOVE_BUFF_RANDOM", RemoveBuffRandom);        //移除BUFF-随机
            SkillEditorFunctionRegistry.Register("REMOVE_BUFF_GROUP", RemoveBuffGroup);        //移除BUFF组
        }

        private static void DefineVariable(SkillEditorFunctionContext ctx)

        {

            string varName = ctx.ResolveVarName(ctx.GetParamRaw(0));

            if (string.IsNullOrEmpty(varName)) { return; }

            long initValue = ParseLong(ctx.ResolveParam(ctx.GetParamRaw(1)), 0);

            ctx.SetVariable(varName, initValue);

        }



        private static void SetVariableValue(SkillEditorFunctionContext ctx)

        {

            string varName = ctx.ResolveVarName(ctx.GetParamRaw(0));

            if (string.IsNullOrEmpty(varName)) { return; }

            long value = ParseLong(ctx.ResolveParam(ctx.GetParamRaw(1)), 0);

            ctx.SetVariable(varName, value);

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

            ctx.SetVariable(varName, randomValue);

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

            string op = ctx.ResolveParam(ctx.GetParamRaw(1)).Trim();

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



        private static void InformEventAttack(SkillEditorFunctionContext ctx)

        {

            Unit caster = ctx.ResolveUnit(ctx.GetParamRaw(0));

            Unit target = ctx.ResolveUnit(ctx.GetParamRaw(1));

            Log.Debug($"INFORM_EVENT_ATTACK skill={ctx.SkillId} caster={(caster?.Id ?? 0)} target={(target?.Id ?? 0)}");

        }



        private static void InformEventDefense(SkillEditorFunctionContext ctx)

        {

            Unit caster = ctx.ResolveUnit(ctx.GetParamRaw(0));

            Unit target = ctx.ResolveUnit(ctx.GetParamRaw(1));

            Log.Debug($"INFORM_EVENT_DEFENSE skill={ctx.SkillId} caster={(caster?.Id ?? 0)} target={(target?.Id ?? 0)}");

        }



        private static void InformClientHitSuccess(SkillEditorFunctionContext ctx)

        {

            Unit caster = ctx.ResolveUnit(ctx.GetParamRaw(0));

            Unit target = ctx.ResolveUnit(ctx.GetParamRaw(1));

            int skillId = ctx.GetParamInt(2, ctx.SkillId);

            int level = ctx.GetParamInt(3, ctx.SkillLevel);

            Log.Debug($"INFORM_CLIENT_HIT_SUCCESS caster={(caster?.Id ?? 0)} target={(target?.Id ?? 0)} skill={skillId} level={level}");

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

                ctx.SetVariable("rs", SkillEditorHitResult.Miss);

                ctx.LastConditionResult = false;

                return;

            }

            long rs = SkillEditorHitResult.Hit;

            if (canCrit && RandomHelper.RandomNumber(0, 10000) < 500 + critRateAdd)

            {

                rs = 11;

            }

            ctx.SetVariable("rs", rs);

            ctx.LastConditionResult = rs > 0;

            Log.Debug($"SKILL_DAMAGE_CHECK_HEAL skill={skillId} level={level} caster={caster.Id} target={target.Id} rs={rs}");

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

            bool excludeNonClearable = ctx.GetParamBool(5, true);

            bool forceRemove = ctx.GetParamBool(6, false);

            bool triggerFadeSkill = ctx.GetParamBool(7, true);

            RemoveBuffRandomInternal(target, buffFromUnitId, benefitFilter, removeCount, excludeNonClearable, forceRemove, triggerFadeSkill);

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

                ctx.SetVariable("rs", SkillEditorHitResult.Miss);

                ctx.LastConditionResult = false;

                return;

            }

            bool canImmune = canBlockAsImmune && canBlock;

            long rs = SkillEditorContionHelper.EvaluateDirectHit(

                ctx, caster, target, skillId,

                canCrit, canImmune, canDodge,

                critRateAdd + (canHeavy ? heavyRateAdd : 0),

                hitRateAdd, level, 0f, 0f, true);

            ctx.SetVariable("rs", rs);

            ctx.LastConditionResult = rs > 0;

        }



        private static bool CompareLogic(string left, string right, string op)

        {

            if (string.IsNullOrEmpty(op)) { return false; }

            switch (op)

            {

                case "&&":

                    return ParseBool(left) && ParseBool(right);

                case "||":

                    return ParseBool(left) || ParseBool(right);

                case "&":

                    return (ParseLong(left, 0) != 0) && (ParseLong(right, 0) != 0);

                case "|":

                    return (ParseLong(left, 0) != 0) || (ParseLong(right, 0) != 0);

                case "==":

                    if (long.TryParse(left, out long lv) && long.TryParse(right, out long rv))

                    {

                        return lv == rv;

                    }

                    return ParseBool(left) == ParseBool(right)

                        || string.Equals(left?.Trim(), right?.Trim(), StringComparison.OrdinalIgnoreCase);

                case "~=":

                    if (long.TryParse(left, out long lv2) && long.TryParse(right, out long rv2))

                    {

                        return lv2 != rv2;

                    }

                    return ParseBool(left) != ParseBool(right)

                        && !string.Equals(left?.Trim(), right?.Trim(), StringComparison.OrdinalIgnoreCase);

                case ">":

                    return ParseDouble(left) > ParseDouble(right);

                case "<":

                    return ParseDouble(left) < ParseDouble(right);

                case ">=":

                    return ParseDouble(left) >= ParseDouble(right);

                case "<=":

                    return ParseDouble(left) <= ParseDouble(right);

                default:

                    Log.Warning($"LOGIC_RELATION unknown op: {op}");

                    return false;

            }

        }



        private static bool ParseBool(string raw)

        {

            if (bool.TryParse(raw, out bool b)) { return b; }

            return ParseLong(raw, 0) != 0;

        }



        private static long ParseLong(string raw, long defaultValue)

        {

            if (long.TryParse(raw, out long value)) { return value; }

            if (int.TryParse(raw, out int intValue)) { return intValue; }

            if (bool.TryParse(raw, out bool boolValue)) { return boolValue ? 1 : 0; }

            return defaultValue;

        }



        private static double ParseDouble(string raw)

        {

            if (double.TryParse(raw, out double value)) { return value; }

            return ParseLong(raw, 0);

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

            Log.Debug($"REMOVE_BUFF target={target.Id} buffId={buffId} from={buffFromUnitId} force={forceRemove} fade={triggerFadeSkill}");

        }



        private static void RemoveBuffRandomInternal(Unit target, long buffFromUnitId, string benefitFilter, int removeCount, bool excludeNonClearable, bool forceRemove, bool triggerFadeSkill)

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

                if (allowedBenefits.Count > 0 && !allowedBenefits.Contains(bh.MBuff.BuffBenefitType)) { continue; }

                candidates.Add(bh.MBuff.Id);

            }

            for (int n = 0; n < removeCount && candidates.Count > 0; n++)

            {

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

                if (bh.MBuff.Remove == null || !bh.MBuff.Remove.Contains(buffGroup)) { continue; }

                buffMgr.OnRemoveBuffItem(bh);

                buffMgr.m_Buffs.RemoveAt(i);

            }

            Log.Debug($"REMOVE_BUFF_GROUP target={target.Id} group={buffGroup} force={forceRemove} fade={triggerFadeSkill}");

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

            Log.Debug($"ApplyBuff target={target.Id} buffId={buffId} interval={intervalMs} ticks={tickCount} level={buffLevel} control={controlType ?? string.Empty}");

        }
    }
}

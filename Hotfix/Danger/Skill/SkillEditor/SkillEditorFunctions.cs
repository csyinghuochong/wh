using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

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
            SkillEditorFunctionRegistry.Register("INFORM_CLIENT_HIT_SUCCESS", InformClientHitSuccess);        //通知客户端命中
            SkillEditorFunctionRegistry.Register("SKILL_DAMAGE_CHECK_PHYSICS", SkillDamageCheckPhysics);        //物理技能判定
            SkillEditorFunctionRegistry.Register("SKILL_DAMAGE_CHECK_MAGIC", SkillDamageCheckMagic);        //法术技能判定
            SkillEditorFunctionRegistry.Register("SKILL_DAMAGE_CHECK_HEAL", SkillDamageCheckHeal);        //治疗技能判定
            SkillEditorFunctionRegistry.Register("CALCULATE_PHYSICS_DAMAGE", CalculatePhysicsDamage);        //计算物理伤害
            SkillEditorFunctionRegistry.Register("ADD_BUFF", AddBuff);        //添加BUFF
            SkillEditorFunctionRegistry.Register("ADD_BUFF_CONTROL", AddBuffControl);        //添加控制BUFF
            SkillEditorFunctionRegistry.Register("REMOVE_BUFF", RemoveBuff);        //移除BUFF
            SkillEditorFunctionRegistry.Register("REMOVE_BUFF_RANDOM", RemoveBuffRandom);        //移除BUFF-随机
            SkillEditorFunctionRegistry.Register("REMOVE_BUFF_GROUP", RemoveBuffGroup);        //移除BUFF组
            SkillEditorFunctionRegistry.Register("BUFF_DATA_SET", BuffDataSet);        //BUFF-数据设置
            SkillEditorFunctionRegistry.Register("BUFF_DATA_GET", BuffDataGet);        //BUFF-数据获取
            SkillEditorFunctionRegistry.Register("SET_BUFF_DATA", SetBuffDataLegacy);        //旧版 BUFF-数据设置
            SkillEditorFunctionRegistry.Register("GET_BUFF_DATA", GetBuffDataLegacy);        //旧版 BUFF-数据获取
            SkillEditorFunctionRegistry.Register("GET_UNIT_TYPEID", GetUnitTypeId);        //获取对象类型
            SkillEditorFunctionRegistry.Register("GET_SKILL_LEVEL", GetSkillLevel);        //获取技能等级
            SkillEditorFunctionRegistry.Register("GET_BUFF_LEVEL", GetBuffLevel);        //获取BUFF层数
            SkillEditorFunctionRegistry.Register("GET_RANDOM_UNIT", GetRandomUnit);        //获取随机目标
            SkillEditorFunctionRegistry.Register("GET_POINT_DISTANCE", GetPointDistance);        //获取两点之间距离
            SkillEditorFunctionRegistry.Register("GET_POINT_DIRECTION", GetPointDirection);        //获取两点之间朝向
            SkillEditorFunctionRegistry.Register("IS_DEAD", IsDead);        //是否死亡
            SkillEditorFunctionRegistry.Register("IS_DYING", IsDying);        //是否濒死
            SkillEditorFunctionRegistry.Register("OWN_BUFF", OwnBuff);        //是否拥有BUFF
            SkillEditorFunctionRegistry.Register("CHANGE_GLOBAL_CD", ChangeGlobalCd);        //修改公共CD
            SkillEditorFunctionRegistry.Register("CHANGE_SKILL_CURRENT_CD", ChangeSkillCurrentCd);        //修改技能当前CD
            SkillEditorFunctionRegistry.Register("CHANGE_SKILL_CURRENT_CD_MULTIPLE", ChangeSkillCurrentCdMultiple);        //修改技能当前CD-批量
            SkillEditorFunctionRegistry.Register("SET_UNIT_CHOOSE_STATUS", SetUnitChooseStatus);        //设置无法选中状态
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

        private static void InformEventAttack(SkillEditorFunctionContext ctx)
        {
            Unit caster = ctx.ResolveUnit(ctx.GetParamRaw(0));
            Unit target = ctx.ResolveUnit(ctx.GetParamRaw(1));
            TriggerPassiveEvent(caster, target, SkillPassiveTypeEnum.AttackAll);
            if (target != null && caster != null)
            {
                target.GetComponent<SkillPassiveComponent>()?.OnTrigegerPassiveSkill(
                    SkillPassiveTypeEnum.AttackAll, caster.Id, ctx.SkillId);
            }

            Log.Debug($"INFORM_EVENT_ATTACK skill={ctx.SkillId} caster={(caster?.Id ?? 0)} target={(target?.Id ?? 0)}");
        }

        private static void InformEventDefense(SkillEditorFunctionContext ctx)
        {
            Unit caster = ctx.ResolveUnit(ctx.GetParamRaw(0));
            Unit target = ctx.ResolveUnit(ctx.GetParamRaw(1));

            //TriggerPassiveEvent(target, caster, SkillPassiveTypeEnum.None);
            Log.Debug($"INFORM_EVENT_DEFENSE skill={ctx.SkillId} caster={(caster?.Id ?? 0)} target={(target?.Id ?? 0)}");
        }

        private static void InformClientHitSuccess(SkillEditorFunctionContext ctx)
        {
            Unit caster = ctx.ResolveUnit(ctx.GetParamRaw(0));
            Unit target = ctx.ResolveUnit(ctx.GetParamRaw(1));
            int skillId = ctx.GetParamInt(2, ctx.SkillId);
            int level = ctx.GetParamInt(3, ctx.SkillLevel);

            //TriggerPassiveEvent(caster, target, SkillPassiveTypeEnum.AllSkill_17, skillId);
            //TriggerPassiveEvent(target, caster, SkillPassiveTypeEnum.AllSkill_17, skillId);
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

                //if (bh.MBuff.Remove == null || !bh.MBuff.Remove.Contains(buffGroup)) { continue; }
                if (bh.MBuff.Remove == null ) { continue; }

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
            Log.Debug($"ApplyBuff target={target.Id} buffId={buffId} interval={intervalMs} ticks={tickCount} level={buffLevel} control={controlType ?? string.Empty}");
        }

        private static void DestroyUnit(SkillEditorFunctionContext ctx)
        {
            Unit unit = ctx.ResolveUnit(ctx.GetParamRaw(0));
            if (unit == null || unit.IsDisposed || unit.Type == UnitType.Player)
            {
                return;
            }

            unit.GetParent<UnitComponent>()?.Remove(unit.Id);
            Log.Debug($"DESTROY_UNIT unit={unit.Id}");
        }

        private static void CalculatePhysicsDamage(SkillEditorFunctionContext ctx)
        {
            Unit caster = ctx.ResolveUnit(ctx.GetParamRaw(0));
            Unit target = ctx.ResolveUnit(ctx.GetParamRaw(1));
            int skillId = ctx.GetParamInt(2, ctx.SkillId);
            int level = ctx.GetParamInt(3, ctx.SkillLevel);
            if (caster == null || target == null || caster.IsDisposed || target.IsDisposed)
            {
                return;
            }

            long minAtk = ctx.ResolveNumericAttribute(caster, ctx.GetParamRaw(4), 0);
            long maxAtk = ctx.ResolveNumericAttribute(caster, ctx.GetParamRaw(5), minAtk);
            if (minAtk > maxAtk)
            {
                long tmp = minAtk;
                minAtk = maxAtk;
                maxAtk = tmp;
            }

            float power = ctx.GetParamFloat(6, 1f);
            long atk = minAtk == maxAtk
                ? minAtk
                : RandomHelper.RandomNumber((int)minAtk, (int)maxAtk);
            long damage = (long)(atk * power);
            long rs = ctx.GetVariable("rs", 1);
            if (rs > 1)
            {
                damage = (long)(damage * 1.5f);
            }

            damage = RandomHelper.RandomNumber(1,3);
            
            NumericComponent defendNumeric = target.GetComponent<NumericComponent>();
            if (defendNumeric != null)
            {
                defendNumeric.ApplyChange(caster, NumericType.HP_Current, -damage, skillId);
            }

            Log.Debug($"CALCULATE_PHYSICS_DAMAGE skill={skillId} level={level} caster={caster.Id} target={target.Id} damage={damage}");
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
            Unit owner = ctx.ResolveUnit(ctx.GetParamRaw(2));
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
            Unit owner = ctx.ResolveUnit(ctx.GetParamRaw(2));
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

        private static void GetSkillLevel(SkillEditorFunctionContext ctx)
        {
            string varName = ctx.ResolveVarName(ctx.GetParamRaw(0), "nSkillLevel");
            int skillId = ctx.GetParamInt(2, ctx.SkillId);
            int level = skillId == ctx.SkillId ? ctx.SkillLevel : 1;
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
            List<long> targetIds = CollectTargetIds(ctx, usePriority);
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
                        && numeric.GetAsLong(NumericType.HP_Current) <= 0;
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
            bool owned = false;
            if (unit != null && buffId > 0)
            {
                BuffManagerComponent buffMgr = unit.GetComponent<BuffManagerComponent>();
                owned = buffFromUnitId == 0
                    ? buffMgr != null && buffMgr.HaveBuff(buffId)
                    : buffMgr != null && buffMgr.GetBuffSourceNumber(buffFromUnitId, buffId) > 0;
            }

            ctx.LastConditionResult = owned;
            ctx.SetVariable("rs", owned ? 1 : 0);
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
            Log.Debug($"CHANGE_GLOBAL_CD unit={unit.Id} deltaMs={deltaMs} publicCd={skillMgr.SkillPublicCDTime}");
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
            Log.Debug($"CHANGE_SKILL_CURRENT_CD unit={unit.Id} skill={skillId} deltaMs={deltaMs}");
        }

        private static void ChangeSkillCurrentCdMultiple(SkillEditorFunctionContext ctx)
        {
            Unit unit = ctx.ResolveUnit(ctx.GetParamRaw(0));
            int deltaMs = ctx.GetParamInt(2, 0);
            SkillManagerComponent skillMgr = unit?.GetComponent<SkillManagerComponent>();
            if (skillMgr == null)
            {
                return;
            }

            foreach (SkillCDItem skillCd in skillMgr.SkillCDs.Values)
            {
                skillCd.CDEndTime += deltaMs;
                skillCd.CDPassive += deltaMs;
            }

            Log.Debug($"CHANGE_SKILL_CURRENT_CD_MULTIPLE unit={unit.Id} deltaMs={deltaMs} count={skillMgr.SkillCDs.Count}");
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
                || numeric.GetAsLong(NumericType.HP_Current) <= 0;
        }

        private static List<long> CollectTargetIds(SkillEditorFunctionContext ctx, bool usePriority)
        {
            List<long> targetIds = new List<long>();
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
    }
}

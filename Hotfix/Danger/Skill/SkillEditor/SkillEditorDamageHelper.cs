using System;
using System.Globalization;

namespace ET
{
    internal enum SkillEditorDamageKind
    {
        Physics,
        Magic,
    }

    /// <summary>
    /// Damage/heal calculation for CALCULATE_*_DAMAGE skill-tree functions.
    /// Parameter layout matches DocEditor界面配置.xml (CALCULATE_PHYSICS_DAMAGE / CALCULATE_MAGIC_DAMAGE).
    /// </summary>
    internal static class SkillEditorDamageHelper
    {
        private const double DefaultCritMultiplier = 1.5d;

        public static void CalculateDamage(SkillEditorFunctionContext ctx, SkillEditorDamageKind kind)
        {
            Unit caster = ctx.ResolveUnit(ctx.GetParamRaw(0));
            Unit target = ctx.ResolveUnit(ctx.GetParamRaw(1));
            int skillId = ctx.GetParamInt(2, ctx.SkillId);
            int level = ctx.GetParamInt(3, ctx.SkillLevel);
            if (caster == null || target == null || caster.IsDisposed || target.IsDisposed)
            {
                return;
            }

            long rs = ctx.GetVariable("rs", SkillEditorHitResult.Hit);
            if (rs == SkillEditorHitResult.Miss
                || rs == SkillEditorHitResult.Immune
                || rs == SkillEditorHitResult.Dodge)
            {
                return;
            }

            NumericComponent casterNumeric = caster.GetComponent<NumericComponent>();
            NumericComponent targetNumeric = target.GetComponent<NumericComponent>();
            if (casterNumeric == null || targetNumeric == null)
            {
                return;
            }

            
            double physicalPower = GetParamDouble(ctx, 4, 0d);//物理威力
            double magicPower = GetParamDouble(ctx, 5, 0d);  //法术威力
            double pdefPower = GetParamDouble(ctx, 6, 0d);  //物防威力
            double mdefPower = GetParamDouble(ctx, 7, 0d);  //法防威力

            ApplyDefaultPower(kind, ref physicalPower, ref magicPower);

            long minPatk = casterNumeric.GetAsLong(NumericType.PATK_Min_21);  //最小物攻
            long maxPatk = casterNumeric.GetAsLong(NumericType.PATK_Max_22);  //最大物攻
            long minMatk = casterNumeric.GetAsLong(NumericType.MATK_Min_31);
            long maxMatk = casterNumeric.GetAsLong(NumericType.MATK_Max_32);

            long minBonus = (long)EvalLevelGrowth(
                level,
                GetParamDouble(ctx, 8, 0d),
                GetParamDouble(ctx, 9, 0d),
                GetParamDouble(ctx, 10, 0d));
            long maxBonus = (long)EvalLevelGrowth(
                level,
                GetParamDouble(ctx, 11, 0d),
                GetParamDouble(ctx, 12, 0d),
                GetParamDouble(ctx, 13, 0d));

            long extraBonus = ResolveExtraBonus(ctx, caster, 14, 15) + ResolveExtraBonus(ctx, caster, 16, 17);
            minPatk += minBonus + extraBonus;
            maxPatk += maxBonus + extraBonus;
            if (minPatk > maxPatk)
            {
                long tmp = minPatk;
                minPatk = maxPatk;
                maxPatk = tmp;
            }

            long rolledPatk = RollLong(minPatk, maxPatk);
            long rolledMatk = RollLong(minMatk, maxMatk);
            double attack = rolledPatk * physicalPower + rolledMatk * magicPower;

            if (Math.Abs(pdefPower) > 1e-9)
            {
                attack += RollLong(
                    targetNumeric.GetAsLong(NumericType.PDEF_Min_41),
                    targetNumeric.GetAsLong(NumericType.PDEF_Max_42)) * pdefPower;
            }

            if (Math.Abs(mdefPower) > 1e-9)
            {
                attack += RollLong(
                    targetNumeric.GetAsLong(NumericType.MDEF_Min_51),
                    targetNumeric.GetAsLong(NumericType.MDEF_Max_52)) * mdefPower;
            }

            double ignoreDefRatio = Clamp01(GetParamDouble(ctx, 18, 0d));
            long ignoreMinDef = (long)GetParamDouble(ctx, 19, 0d);
            long ignoreMaxDef = (long)GetParamDouble(ctx, 20, 0d);
            long defense;
            if (kind == SkillEditorDamageKind.Magic)
            {
                defense = RollDefense(
                    targetNumeric.GetAsLong(NumericType.MDEF_Min_51),
                    targetNumeric.GetAsLong(NumericType.MDEF_Max_52),
                    ignoreDefRatio,
                    ignoreMinDef,
                    ignoreMaxDef);
            }
            else
            {
                defense = RollDefense(
                    targetNumeric.GetAsLong(NumericType.PDEF_Min_41),
                    targetNumeric.GetAsLong(NumericType.PDEF_Max_42),
                    ignoreDefRatio,
                    ignoreMinDef,
                    ignoreMaxDef);
            }

            long normalDamage = Math.Max(1L, (long)(attack - defense));

            double critBonus = GetParamDouble(ctx, 21, 0d);
            if (rs > SkillEditorHitResult.Hit)
            {
                double critMultiplier = DefaultCritMultiplier
                    + critBonus
                    + casterNumeric.GetAsLong(NumericType.P_CRI_DMG_PerMyriad_72) / 10000d;
                normalDamage = Math.Max(1L, (long)(normalDamage * critMultiplier));
            }

            int normalSplit = Math.Max(1, ctx.GetParamInt(35, 1));
            normalDamage = Math.Max(1L, normalDamage / normalSplit);

            long elementalDamage = ResolveElementalDamage(ctx, 36);
            long hpDamage = ResolveHpDamage(
                ctx,
                casterNumeric,
                targetNumeric,
                defense,
                GetParamBool(ctx, 31, true));

            long totalDamage = normalDamage + elementalDamage + hpDamage;
            if (totalDamage <= 0)
            {
                return;
            }

            bool ignoreShield = GetParamBool(ctx, 38, false);
            int damageType = rs > SkillEditorHitResult.Hit ? 1 : 0;
            ApplyDamage(caster, target, targetNumeric, totalDamage, skillId, ignoreShield, damageType);

            ApplyLifeSteal(caster, casterNumeric, normalDamage, GetParamDouble(ctx, 32, 0d), skillId);
            ApplyLifeSteal(caster, casterNumeric, elementalDamage, GetParamDouble(ctx, 33, 0d), skillId);
            ApplyLifeSteal(caster, casterNumeric, hpDamage, GetParamDouble(ctx, 34, 0d), skillId);

            double hateMultiplier = GetParamDouble(ctx, 39, 1d);
            if (hateMultiplier > 0d)
            {
                Log.Debug(
                    $"SkillEditor hate skill={skillId} caster={caster.Id} target={target.Id} damage={totalDamage} multiplier={hateMultiplier.ToString(CultureInfo.InvariantCulture)}");
            }

            Log.Debug(
                $"CALCULATE_{(kind == SkillEditorDamageKind.Physics ? "PHYSICS" : "MAGIC")}_DAMAGE skill={skillId} level={level} caster={caster.Id} target={target.Id} rs={rs} normal={normalDamage} elemental={elementalDamage} hp={hpDamage} total={totalDamage}");
        }

        public static void CalculateHeal(SkillEditorFunctionContext ctx)
        {
            Unit caster = ctx.ResolveUnit(ctx.GetParamRaw(0));
            Unit target = ctx.ResolveUnit(ctx.GetParamRaw(1));
            int skillId = ctx.GetParamInt(2, ctx.SkillId);
            int level = ctx.GetParamInt(3, ctx.SkillLevel);
            if (caster == null || target == null || caster.IsDisposed || target.IsDisposed)
            {
                return;
            }

            NumericComponent targetNumeric = target.GetComponent<NumericComponent>();
            if (targetNumeric == null)
            {
                return;
            }

            long minValue = ctx.ResolveNumericAttribute(caster, ctx.GetParamRaw(4), NumericType.PATK_Min_21);
            long maxValue = ctx.ResolveNumericAttribute(caster, ctx.GetParamRaw(5), minValue);
            if (minValue > maxValue)
            {
                long tmp = minValue;
                minValue = maxValue;
                maxValue = tmp;
            }

            double power = GetParamDouble(ctx, 6, 1d);
            if (power <= 0d)
            {
                power = 1d;
            }

            long amount = Math.Max(1L, (long)(RollLong(minValue, maxValue) * power));
            targetNumeric.ApplyChange(caster, NumericType.HP_Current_8, amount, skillId);
            Log.Debug($"CALCULATE_HEAL_DAMAGE skill={skillId} level={level} caster={caster.Id} target={target.Id} heal={amount}");
        }

        private static void ApplyDefaultPower(SkillEditorDamageKind kind, ref double physicalPower, ref double magicPower)
        {
            if (kind == SkillEditorDamageKind.Physics)
            {
                if (physicalPower <= 0d && magicPower <= 0d)
                {
                    physicalPower = 1d;
                }
            }
            else if (magicPower <= 0d && physicalPower <= 0d)
            {
                magicPower = 1d;
            }
        }

        private static long ResolveExtraBonus(SkillEditorFunctionContext ctx, Unit unit, int attrIndex, int coefIndex)
        {
            int numericType = ctx.ResolveNumericType(ctx.GetParamRaw(attrIndex), 0);
            if (numericType <= 0)
            {
                return 0;
            }

            double coef = GetParamDouble(ctx, coefIndex, 0d);
            if (Math.Abs(coef) < 1e-9)
            {
                return 0;
            }

            return (long)(ctx.GetUnitNumericValue(unit, numericType, 0) * coef);
        }

        private static long ResolveElementalDamage(SkillEditorFunctionContext ctx, int splitParamIndex)
        {
            double total = GetParamDouble(ctx, 22, 0d)
                + GetParamDouble(ctx, 23, 0d)
                + GetParamDouble(ctx, 24, 0d)
                + GetParamDouble(ctx, 25, 0d)
                + GetParamDouble(ctx, 26, 0d);
            if (total <= 0d)
            {
                return 0;
            }

            int split = Math.Max(1, ctx.GetParamInt(splitParamIndex, 1));
            return Math.Max(0L, (long)(total / split));
        }

        private static long ResolveHpDamage(
            SkillEditorFunctionContext ctx,
            NumericComponent casterNumeric,
            NumericComponent targetNumeric,
            long defense,
            bool useReduction)
        {
            long casterHpMax = casterNumeric.GetAsLong(NumericType.HP_Max_10);
            long targetHpMax = targetNumeric.GetAsLong(NumericType.HP_Max_10);

            long casterPart = CapPercentDamage(
                casterHpMax,
                GetParamDouble(ctx, 27, 0d),
                (long)GetParamDouble(ctx, 28, 0d));
            long targetPart = CapPercentDamage(
                targetHpMax,
                GetParamDouble(ctx, 29, 0d),
                (long)GetParamDouble(ctx, 30, 0d));

            long hpDamage = casterPart + targetPart;
            if (hpDamage <= 0)
            {
                return 0;
            }

            if (useReduction && defense > 0)
            {
                hpDamage = Math.Max(0L, hpDamage - defense);
            }

            int split = Math.Max(1, ctx.GetParamInt(37, 1));
            return Math.Max(0L, hpDamage / split);
        }

        private static long CapPercentDamage(long hpMax, double ratio, long cap)
        {
            if (hpMax <= 0 || ratio <= 0d)
            {
                return 0;
            }

            long damage = (long)(hpMax * ratio);
            if (cap > 0 && damage > cap)
            {
                damage = cap;
            }

            return Math.Max(0L, damage);
        }

        private static void ApplyDamage(
            Unit caster,
            Unit target,
            NumericComponent targetNumeric,
            long totalDamage,
            int skillId,
            bool ignoreShield,
            int damageType)
        {
            long damageToHp = totalDamage;
            if (!ignoreShield)
            {
                long shieldHp = targetNumeric.GetAsLong(NumericType.Now_Shield_HP);
                if (shieldHp > 0)
                {
                    long absorbed = Math.Min(shieldHp, damageToHp);
                    targetNumeric.ApplyChange(caster, NumericType.Now_Shield_HP, -absorbed, skillId, true, damageType);
                    damageToHp -= absorbed;
                }
            }

            if (damageToHp <= 0)
            {
                return;
            }

            targetNumeric.ApplyChange(caster, NumericType.HP_Current_8, -damageToHp, skillId, true, damageType);
        }

        private static void ApplyLifeSteal(Unit caster, NumericComponent casterNumeric, long damage, double ratio, int skillId)
        {
            if (caster == null || casterNumeric == null || damage <= 0 || ratio <= 0d)
            {
                return;
            }

            long heal = (long)(damage * ratio);
            if (heal <= 0)
            {
                return;
            }

            casterNumeric.ApplyChange(caster, NumericType.HP_Current_8, heal, skillId);
        }

        private static long RollDefense(long minDef, long maxDef, double ignoreRatio, long ignoreMin, long ignoreMax)
        {
            long adjustedMin = (long)Math.Max(0d, minDef * (1d - ignoreRatio) - ignoreMin);
            long adjustedMax = (long)Math.Max(0d, maxDef * (1d - ignoreRatio) - ignoreMax);
            if (adjustedMin > adjustedMax)
            {
                long tmp = adjustedMin;
                adjustedMin = adjustedMax;
                adjustedMax = tmp;
            }

            return RollLong(adjustedMin, adjustedMax);
        }

        private static long RollLong(long minValue, long maxValue)
        {
            if (minValue >= maxValue)
            {
                return minValue;
            }

            return RandomHelper.RandomNumber((int)minValue, (int)maxValue);
        }

        private static double EvalLevelGrowth(int level, double quad, double linear, double constant)
        {
            double lv = Math.Max(0, level - 1);
            return quad * lv * lv + linear * lv + constant;
        }

        private static double GetParamDouble(SkillEditorFunctionContext ctx, int index, double defaultValue)
        {
            return SkillEditorFunctionContext.ParseDouble(ctx.ResolveParam(ctx.GetParamRaw(index)), defaultValue);
        }

        private static bool GetParamBool(SkillEditorFunctionContext ctx, int index, bool defaultValue)
        {
            return ctx.GetParamBool(index, defaultValue);
        }

        private static double Clamp01(double value)
        {
            if (value < 0d)
            {
                return 0d;
            }

            if (value > 1d)
            {
                return 1d;
            }

            return value;
        }
    }
}

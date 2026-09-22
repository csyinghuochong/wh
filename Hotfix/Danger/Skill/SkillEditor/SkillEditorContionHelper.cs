namespace ET
{
    /// <summary>
    /// 技能伤害判定。命中/暴击走自身 66/70（职业/怪/宠初始 950、50，/1000）。技能加成 /10000。重击默认 0.05。
    /// </summary>
    public static class SkillEditorContionHelper
    {
        private const double DefaultCritRate = 0.05d;

        public static long EvaluateDirectHit(
            SkillEditorFunctionContext ctx,
            Unit caster,
            Unit target,
            int skillId,
            bool canCrit,
            bool canHeavy,
            bool canDodge,
            int critRateAdd,
            int heavyRateAdd,
            int hitRateAdd,
            int skillLevel,
            float hateInit,
            float hateGrowth,
            bool sendHitMsg)
        {
            if (caster == null || target == null || caster.IsDisposed || target.IsDisposed)
            {
                return (long)SkillEditorHitResult.Miss;
            }

            if (!caster.IsCanAttackUnit(target, false, false))
            {
                return (long)SkillEditorHitResult.Miss;
            }

            NumericComponent casterNumeric = caster.GetComponent<NumericComponent>();
            NumericComponent targetNumeric = target.GetComponent<NumericComponent>();
            int x = RandomHelper.RandomNumber(0, 10001);

            if (canDodge)
            {
                double hitRate = hitRateAdd / 10000d
                    + Attr(casterNumeric, NumericType.P_HIT_Fixed_66) / 1000d
                    - Attr(targetNumeric, NumericType.P_DODGE_Fixed_68) / 1000d;
                if (x > hitRate * 10000d)
                {
                    return (long)SkillEditorHitResult.Dodge;
                }
            }

            long result = (long)SkillEditorHitResult.Hit;
            if (canCrit)
            {
                double critRate = critRateAdd / 10000d
                    + Attr(casterNumeric, NumericType.P_CRI_Fixed_70) / 1000d
                    - Attr(targetNumeric, NumericType.P_CRI_RES_Fixed_74) / 1000d;
                if (x <= critRate * 10000d)
                {
                    result = (long)SkillEditorHitResult.Crit;
                }
            }

            if (result == (long)SkillEditorHitResult.Hit && canHeavy)
            {
                double heavyRate = DefaultCritRate
                    + heavyRateAdd / 10000d
                    + Attr(casterNumeric, NumericType.SMASH_Fixed_80) / 1000d
                    - Attr(targetNumeric, NumericType.SMASH_RES_Fixed_82) / 1000d;
                if (x <= heavyRate * 10000d)
                {
                    result = (long)SkillEditorHitResult.Heavy;
                }
            }

            if (result == (long)SkillEditorHitResult.Crit)
            {
                target.GetComponent<SkillManagerComponent>()?.InterruptSkillsBeforeTime1();
            }

            ApplyHate(caster, target, hateInit, hateGrowth, skillLevel);
            if (sendHitMsg && Log.IsDebugEnabled)
            {
                Log.Debug($"SkillEditor hit skill={skillId} caster={caster.Id} target={target.Id} x={x} rs={result}");
            }

            return result;
        }

        public static bool RollCrit(Unit caster, Unit target, int critRateAdd)
        {
            NumericComponent casterNumeric = caster?.GetComponent<NumericComponent>();
            NumericComponent targetNumeric = target?.GetComponent<NumericComponent>();
            double critRate = critRateAdd / 10000d
                + Attr(casterNumeric, NumericType.P_CRI_Fixed_70) / 1000d
                - Attr(targetNumeric, NumericType.P_CRI_RES_Fixed_74) / 1000d;
            int x = RandomHelper.RandomNumber(0, 10001);
            return x <= critRate * 10000d;
        }

        private static long Attr(NumericComponent numeric, int attrId)
        {
            return numeric?.GetAsLong(attrId) ?? 0;
        }

        private static void ApplyHate(Unit caster, Unit target, float hateInit, float hateGrowth, int skillLevel)
        {
            if (hateInit <= 0f && hateGrowth <= 0f)
            {
                return;
            }

            float hate = hateInit + hateGrowth * (skillLevel - 1);
            if (hate <= 0f || !Log.IsDebugEnabled)
            {
                return;
            }

            Log.Debug($"SkillEditor hate caster={caster.Id} target={target.Id} value={hate}");
        }
    }
}

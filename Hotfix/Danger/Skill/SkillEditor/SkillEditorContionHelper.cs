namespace ET
{
    /// <summary>
    /// Hit result codes written to rs (aligned with legacy battle_skill_utility.*DirectRes).
    /// </summary>
    public static class SkillEditorHitResult
    {
        public const long Miss = 0;
        public const long Hit = 1;
        public const long Immune = 2;
        public const long Dodge = 3;
    }

    /// <summary>
    /// Direct hit / condition evaluation for function.contion_1 (phyDirectRes-like).
    /// </summary>
    public static class SkillEditorContionHelper
    {
        public static long EvaluateDirectHit(
          SkillEditorFunctionContext ctx,
          Unit caster,
          Unit target,
          int skillId,
          bool canCrit,
          bool canImmune,
          bool canDodge,
          int critRateAdd,
          int hitRateAdd,
          int skillLevel,
          float hateInit,
          float hateGrowth,
          bool sendHitMsg)
        {
            if (caster == null || target == null || caster.IsDisposed || target.IsDisposed)
            {
                return SkillEditorHitResult.Miss;
            }

            if (!caster.IsCanAttackUnit(target, false, false))
            {
                return SkillEditorHitResult.Miss;
            }


            float hitRate = 10000 + hitRateAdd;
            if (hitRate < 0)
            {
                hitRate = 0;
            }

            if (hitRate < 10000f && RandomHelper.RandFloat01() > hitRate / 10000f)
            {
                return SkillEditorHitResult.Miss;
            }

            if (canDodge && RollDodge(caster, target))
            {
                return SkillEditorHitResult.Dodge;
            }

            long result = SkillEditorHitResult.Hit;
            if (canCrit && RollCrit(caster, target, critRateAdd))
            {
                result = 11; // legacy: crit hit flag (>1 means crit in many trees)
            }

            ApplyHate(caster, target, hateInit, hateGrowth, skillLevel);

            if (sendHitMsg)
            {
                Log.Debug($"SkillEditor hit skill={skillId} caster={caster.Id} target={target.Id} rs={result}");
            }

            return result;
        }

        private static bool RollDodge(Unit caster, Unit target)
        {
            NumericComponent casterNumeric = caster?.GetComponent<NumericComponent>();
            NumericComponent targetNumeric = target?.GetComponent<NumericComponent>();
            if (targetNumeric == null)
            {
                return false;
            }

            // 闪避率 = 基础 + 受击方闪避(68) - 攻击方命中(66)，万分率
            long dodgeRate = 500
                + NumericConvert.GetRatePoints(targetNumeric, NumericType.P_DODGE_Fixed_68)
                - NumericConvert.GetRatePoints(casterNumeric, NumericType.P_HIT_Fixed_66);
            if (dodgeRate <= 0)
            {
                return false;
            }

            return RandomHelper.RandomNumber(0, 10000) < dodgeRate;
        }

        private static bool RollCrit(Unit caster, Unit target, int critRateAdd)
        {
            NumericComponent casterNumeric = caster?.GetComponent<NumericComponent>();
            NumericComponent targetNumeric = target?.GetComponent<NumericComponent>();
            // 暴击率 = 基础 + 攻击方暴击(70) - 受击方抗暴(74) + 技能附加，万分率
            long critRate = 500
                + critRateAdd
                + NumericConvert.GetRatePoints(casterNumeric, NumericType.P_CRI_Fixed_70)
                - NumericConvert.GetRatePoints(targetNumeric, NumericType.P_CRI_RES_Fixed_74);
            if (critRate <= 0)
            {
                return false;
            }

            return RandomHelper.RandomNumber(0, 10000) < critRate;
        }

        private static void ApplyHate(Unit caster, Unit target, float hateInit, float hateGrowth, int skillLevel)
        {
            if (hateInit <= 0f && hateGrowth <= 0f)
            {
                return;
            }

            float hate = hateInit + hateGrowth * (skillLevel - 1);
            if (hate <= 0f)
            {
                return;
            }

            // TODO: integrate with monster hate/threat component
            Log.Debug($"SkillEditor hate caster={caster.Id} target={target.Id} value={hate}");
        }
    }
}

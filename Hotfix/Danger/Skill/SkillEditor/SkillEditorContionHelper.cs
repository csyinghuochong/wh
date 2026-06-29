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
            return 1;
        }


        public static long EvaluateDirectHit_Old(
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

            BuffManagerComponent buffMgr = target.GetComponent<BuffManagerComponent>();
            if (canImmune && buffMgr != null && buffMgr.IsSkillImmune(skillId))
            {
                return SkillEditorHitResult.Immune;
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
            // TODO: read dodge rate from NumericComponent when attribute ids are wired
            return false;
        }

        private static bool RollCrit(Unit caster, Unit target, int critRateAdd)
        {
            float critRate = 500 + critRateAdd;
            if (critRate <= 0)
            {
                return false;
            }

            return RandomHelper.RandFloat01() < critRate / 10000f;
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

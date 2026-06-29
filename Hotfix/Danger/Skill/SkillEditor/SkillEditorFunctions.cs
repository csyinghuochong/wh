using System;

namespace ET
{
    /// <summary>
    /// Hand-written helper functions referenced from TreeSave function nodes.
    /// </summary>
    public static class SkillEditorFunctions
    {
        public static void RegisterAll()
        {
            SkillEditorFunctionRegistry.Register("function.test_1", Test1);
            SkillEditorFunctionRegistry.Register("function.contion_1", Contion1);
        }

        private static void Test1(SkillEditorFunctionContext ctx)
        {
            string p0 = ctx.Node.Params.Count > 0 ? ctx.ResolveParam(ctx.Node.Params[0]) : "0";
            string p1 = ctx.Node.Params.Count > 1 ? ctx.ResolveParam(ctx.Node.Params[1]) : "0";
            string p2 = ctx.Node.Params.Count > 2 ? ctx.ResolveParam(ctx.Node.Params[2]) : "0";

            Log.Debug($"SkillEditor function.test_1 skill={ctx.SkillId} params=({p0},{p1},{p2}) desc={ctx.Node.Desc}");
            Console.WriteLine($"Test1: {p0} {p1}  {p2}");
        }

        /// <summary>
        /// function.contion_1 ¡ª direct hit condition (legacy: battle_skill_utility.phyDirectRes).
        /// Params:
        /// 0 rs, 1 caster, 2 target, 3 skillid, 4 sType,
        /// 5 canCrit, 6 canImmune, 7 canDodge, 8 critRateAdd, 9 hitRateAdd,
        /// 10 level, 11 hateInit(91), 12 hateGrowth(92), 13 sendHitMsg
        /// </summary>
        private static void Contion1(SkillEditorFunctionContext ctx)
        {
            string rsVar = ctx.ResolveVarName(ctx.GetParamRaw(0), "rs");
            Unit caster = ctx.ResolveUnit(ctx.GetParamRaw(1));
            Unit target = ctx.ResolveUnit(ctx.GetParamRaw(2));
            int skillId = ctx.GetParamInt(3, ctx.SkillId);
            string sTypeVar = ctx.ResolveVarName(ctx.GetParamRaw(4), "sType");
            bool canCrit = ctx.GetParamBool(5, true);
            bool canImmune = ctx.GetParamBool(6, true);
            bool canDodge = ctx.GetParamBool(7, true);
            int critRateAdd = ctx.GetParamInt(8, 0);
            int hitRateAdd = ctx.GetParamInt(9, 0);
            int skillLevel = ctx.GetParamInt(10, ctx.SkillLevel);
            float hateInit = ResolveTableFloat(ctx, 11, 0f);
            float hateGrowth = ResolveTableFloat(ctx, 12, 0f);
            bool sendHitMsg = ctx.GetParamBool(13, true);

            ctx.SetVariable(sTypeVar, ParseEffectType(ctx.GetParamRaw(4)));

            long rs = SkillEditorContionHelper.EvaluateDirectHit(
                ctx,
                caster,
                target,
                skillId,
                canCrit,
                canImmune,
                canDodge,
                critRateAdd,
                hitRateAdd,
                skillLevel,
                hateInit,
                hateGrowth,
                sendHitMsg);

            ctx.SetVariable(rsVar, rs);
            ctx.LastConditionResult = rs == SkillEditorHitResult.Hit || rs > SkillEditorHitResult.Hit;

            Log.Debug(
                $"SkillEditor function.contion_1 skill={skillId} rs={rsVar}:{rs} caster={(caster?.Id ?? 0)} target={(target?.Id ?? 0)} sType={sTypeVar} level={skillLevel}");
        }

        private static float ResolveTableFloat(SkillEditorFunctionContext ctx, int paramIndex, float defaultValue)
        {
            string raw = ctx.GetParamRaw(paramIndex);
            if (!string.IsNullOrWhiteSpace(raw))
            {
                if (float.TryParse(ctx.ResolveParam(raw), out float explicitValue))
                {
                    return explicitValue;
                }
            }

            string column = ctx.GetParamSkillIdColumn(paramIndex);
            if (string.IsNullOrEmpty(column))
            {
                return defaultValue;
            }

            // skillID column is filled from skill logic sheet in editor; runtime fallback uses param text only for now
            return defaultValue;
        }

        private static long ParseEffectType(string raw)
        {
            string token = raw?.Trim() ?? string.Empty;
            if (token.Equals("sType", StringComparison.OrdinalIgnoreCase))
            {
                return 0;
            }

            if (token.Contains("SKILL_EFFECT_PHY", StringComparison.OrdinalIgnoreCase))
            {
                return 1;
            }

            if (token.Contains("SKILL_EFFECT_MAGIC", StringComparison.OrdinalIgnoreCase))
            {
                return 2;
            }

            if (token.Contains("SKILL_EFFECT_ASSIST", StringComparison.OrdinalIgnoreCase))
            {
                return 3;
            }

            return 0;
        }
    }
}

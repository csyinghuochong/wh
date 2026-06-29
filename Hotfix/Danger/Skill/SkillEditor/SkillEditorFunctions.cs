using System;

namespace ET
{
    /// <summary>
    /// Auto-generated from SkillEditor_v1/bin/Debug/DocEditor界面配置.xml function_list.
    /// Regenerate: tools/generate_skill_editor_functions.ps1
    /// </summary>
    public static class SkillEditorFunctions
    {
        public static void RegisterAll()
        {
            SkillEditorFunctionRegistry.Register("DEFINE_VARIABLE", DefineVariable);
            SkillEditorFunctionRegistry.Register("SET_VARIABLE_VALUE", SetVariableValue);
            SkillEditorFunctionRegistry.Register("DEFINE_VARIABLE_RAMDOM_VALUE", DefineVariableRamdomValue);
            SkillEditorFunctionRegistry.Register("CHANCE_TRIGGER", ChanceTrigger);
            SkillEditorFunctionRegistry.Register("BREAK", BreakLoop);
            SkillEditorFunctionRegistry.Register("RETURN_TRUE", ReturnTrue);
            SkillEditorFunctionRegistry.Register("INFORM_CLIENT_HIT_SUCCESS", InformClientHitSuccess);
            SkillEditorFunctionRegistry.Register("ADD_BUFF", AddBuff);
            SkillEditorFunctionRegistry.Register("ADD_BUFF_CONTROL", AddBuffControl);
        }

        /// <summary>
        /// 定义变量 (DEFINE_VARIABLE)
        /// Params: 变量名, 初始值
        /// </summary>
        private static void DefineVariable(SkillEditorFunctionContext ctx)
        {
            Log.Debug($"SkillEditor DEFINE_VARIABLE skill={ctx.SkillId} desc={ctx.Node.Desc}");

            string varName = ctx.ResolveVarName(ctx.GetParamRaw(0));
            if (string.IsNullOrEmpty(varName)) { return; }
            long initValue = ParseLong(ctx.ResolveParam(ctx.GetParamRaw(1)), 0);
            ctx.SetVariable(varName, initValue);
        }

        /// <summary>
        /// 变量赋值 (SET_VARIABLE_VALUE)
        /// Params: 变量名, 值
        /// </summary>
        private static void SetVariableValue(SkillEditorFunctionContext ctx)
        {
            Log.Debug($"SkillEditor SET_VARIABLE_VALUE skill={ctx.SkillId} desc={ctx.Node.Desc}");

            string varName = ctx.ResolveVarName(ctx.GetParamRaw(0));
            if (string.IsNullOrEmpty(varName)) { return; }
            long value = ParseLong(ctx.ResolveParam(ctx.GetParamRaw(1)), 0);
            ctx.SetVariable(varName, value);
        }

        /// <summary>
        /// 定义变量-随机值 (DEFINE_VARIABLE_RAMDOM_VALUE)
        /// Params: 变量名, 随机值
        /// </summary>
        private static void DefineVariableRamdomValue(SkillEditorFunctionContext ctx)
        {
            Log.Debug($"SkillEditor DEFINE_VARIABLE_RAMDOM_VALUE skill={ctx.SkillId} desc={ctx.Node.Desc}");

            string varName = ctx.ResolveVarName(ctx.GetParamRaw(0));
            if (string.IsNullOrEmpty(varName)) { return; }
            int maxExclusive = ctx.GetParamInt(1, 10000);
            if (maxExclusive <= 0) { maxExclusive = 10000; }
            long randomValue = RandomHelper.RandomNumber(0, maxExclusive);
            ctx.SetVariable(varName, randomValue);
        }

        /// <summary>
        /// 概率触发 (CHANCE_TRIGGER)
        /// Params: 概率 0.01%
        /// </summary>
        private static void ChanceTrigger(SkillEditorFunctionContext ctx)
        {
            Log.Debug($"SkillEditor CHANCE_TRIGGER skill={ctx.SkillId} desc={ctx.Node.Desc}");

            int rate = ctx.GetParamInt(0, 10000);
            if (rate < 0) { rate = 0; }
            if (rate > 10000) { rate = 10000; }
            bool hit = rate >= 10000 || RandomHelper.RandomNumber(0, 10000) < rate;
            ctx.LastConditionResult = hit;
            ctx.SetVariable("rs", hit ? 1 : 0);
        }

        /// <summary>
        /// 跳出循环 (BREAK)
        /// Params: (none)
        /// </summary>
        private static void BreakLoop(SkillEditorFunctionContext ctx)
        {
            Log.Debug($"SkillEditor BREAK skill={ctx.SkillId} desc={ctx.Node.Desc}");

            ctx.SetVariable("__break", 1);
        }

        /// <summary>
        /// 返回成功值 (RETURN_TRUE)
        /// Params: (none)
        /// </summary>
        private static void ReturnTrue(SkillEditorFunctionContext ctx)
        {
            Log.Debug($"SkillEditor RETURN_TRUE skill={ctx.SkillId} desc={ctx.Node.Desc}");

            ctx.LastConditionResult = true;
            ctx.SetVariable("rs", 1);
        }

        /// <summary>
        /// 通知客户端命中 (INFORM_CLIENT_HIT_SUCCESS)
        /// Params: 施法者, 目标, 技能ID, 技能等级
        /// </summary>
        private static void InformClientHitSuccess(SkillEditorFunctionContext ctx)
        {
            Log.Debug($"SkillEditor INFORM_CLIENT_HIT_SUCCESS skill={ctx.SkillId} desc={ctx.Node.Desc}");

            Unit caster = ctx.ResolveUnit(ctx.GetParamRaw(0));
            Unit target = ctx.ResolveUnit(ctx.GetParamRaw(1));
            int skillId = ctx.GetParamInt(2, ctx.SkillId);
            int level = ctx.GetParamInt(3, ctx.SkillLevel);
            Log.Debug($"INFORM_CLIENT_HIT_SUCCESS caster={(caster?.Id ?? 0)} target={(target?.Id ?? 0)} skill={skillId} level={level}");
        }

        /// <summary>
        /// 添加BUFF (ADD_BUFF)
        /// Params: 施法者, 目标, BUFF_ID, 作用间隔-ms, 作用次数, 无视免疫, BUFF等级
        /// </summary>
        private static void AddBuff(SkillEditorFunctionContext ctx)
        {
            Log.Debug($"SkillEditor ADD_BUFF skill={ctx.SkillId} desc={ctx.Node.Desc}");

            Unit caster = ctx.ResolveUnit(ctx.GetParamRaw(0));
            Unit target = ctx.ResolveUnit(ctx.GetParamRaw(1));
            int buffId = ctx.GetParamInt(2, 0);
            int intervalMs = ctx.GetParamInt(3, 0);
            int tickCount = ctx.GetParamInt(4, 1);
            bool ignoreImmune = ctx.GetParamBool(5, false);
            int buffLevel = ctx.GetParamInt(6, ctx.SkillLevel);
            ApplyBuff(ctx, caster, target, buffId, intervalMs, tickCount, ignoreImmune, buffLevel, null);
        }

        /// <summary>
        /// 添加控制BUFF (ADD_BUFF_CONTROL)
        /// Params: 施法者, 目标, BUFF_ID, 作用间隔-ms, 作用次数, 控制类型, 无视免疫, BUFF等级
        /// </summary>
        private static void AddBuffControl(SkillEditorFunctionContext ctx)
        {
            Log.Debug($"SkillEditor ADD_BUFF_CONTROL skill={ctx.SkillId} desc={ctx.Node.Desc}");

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

        private static long ParseLong(string raw, long defaultValue)
        {
            if (long.TryParse(raw, out long value)) { return value; }
            if (int.TryParse(raw, out int intValue)) { return intValue; }
            return defaultValue;
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

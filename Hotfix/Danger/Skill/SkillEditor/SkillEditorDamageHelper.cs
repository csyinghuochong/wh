using OfficeOpenXml.FormulaParsing.Excel.Functions.Database;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace ET
{
    /// <summary>伤害种类：物理 / 法术（决定默认威力与读取哪套防御）。</summary>
    internal enum SkillEditorDamageKind
    {
        Physics,
        Magic,
    }

    /// <summary>
    /// CALCULATE_PHYSICS_DAMAGE / CALCULATE_MAGIC_DAMAGE / CALCULATE_HEAL_DAMAGE 实现。
    /// 公式来源：
    ///   - Document/属性计算/属性公式.txt（威力/分摊/附伤/生命附伤）
    ///   - Document/属性计算/属性作用.txt（破防 x、生效防御 y、减伤比例）
    /// </summary>
    internal static class SkillEditorDamageHelper
    {
        /// <summary>暴击默认倍率 1.5（文档外，技能编辑器参数 21 可再加成）。</summary>
        private const double DefaultCritMultiplier = 1.5d;

        /// <summary>属性作用：y/135 中的除数。</summary>
        private const double DefenseMitigationDivisor = 135d;

        /// <summary>属性作用：减伤公式中 45×攻击方等级 的系数。</summary>
        private const double DefenseMitigationLevelFactor = 45d;

        /// <summary>五系附伤：技能参数索引 + 攻击方附伤属性 ID + 防御方抵抗属性 ID。</summary>
        private readonly struct ElementalDamageConfig
        {
            public ElementalDamageConfig(int skillParamIndex, int attackerAttrId, int defenderResistAttrId)
            {
                SkillParamIndex = skillParamIndex;
                AttackerAttrId = attackerAttrId;
                DefenderResistAttrId = defenderResistAttrId;
            }

            /// <summary>技能编辑器参数 22~26（水/火/风/雷/毒附伤）；无技能参数的系为 -1。</summary>
            public int SkillParamIndex { get; }

            /// <summary>攻击方 Numeric 附伤属性（Attribute 130~139，如 132 水系附伤）。</summary>
            public int AttackerAttrId { get; }

            /// <summary>防御方 Numeric 减免属性（Attribute 140~149，如 142 水伤减免）。</summary>
            public int DefenderResistAttrId { get; }
        }

        // Attribute 表：130~139 系附伤，140~149 系伤减免。
        // 技能参数 22~26 仅有水/火/风/雷/毒；金木土光暗无装备附伤（SkillParamIndex=-1）。
        private static readonly ElementalDamageConfig[] ElementalConfigs =
        {
            new ElementalDamageConfig(-1, NumericType.Attr_130, NumericType.Attr_140), // 金
            new ElementalDamageConfig(-1, NumericType.Attr_131, NumericType.Attr_141), // 木
            new ElementalDamageConfig(22, NumericType.Attr_132, NumericType.Attr_142), // 水
            new ElementalDamageConfig(23, NumericType.Attr_133, NumericType.Attr_143), // 火
            new ElementalDamageConfig(-1, NumericType.Attr_134, NumericType.Attr_144), // 土
            new ElementalDamageConfig(24, NumericType.Attr_135, NumericType.Attr_145), // 风
            new ElementalDamageConfig(25, NumericType.Attr_136, NumericType.Attr_146), // 雷
            new ElementalDamageConfig(26, NumericType.Attr_137, NumericType.Attr_147), // 毒
            new ElementalDamageConfig(-1, NumericType.Attr_138, NumericType.Attr_148), // 光
            new ElementalDamageConfig(-1, NumericType.Attr_139, NumericType.Attr_149), // 暗
        };

        /// <summary>
        /// 职业 Id → (对职业伤害属性, 减免该职业伤害属性)。与 LDOccupation / LDAttribute 表对齐。
        /// </summary>
        private static readonly Dictionary<int, (int BonusAttrId, int ResistAttrId)> OccupationDamageAttrs =
            new Dictionary<int, (int, int)>
            {
                { 10, (NumericType.Attr_150, NumericType.Attr_160) }, // 镇岳
                { 11, (NumericType.Attr_151, NumericType.Attr_161) }, // 云狩
                { 12, (NumericType.Attr_152, NumericType.Attr_162) }, // 影煞
                { 15, (NumericType.Attr_155, NumericType.Attr_165) }, // 玄灵
                { 16, (NumericType.Attr_156, NumericType.Attr_166) }, // 惊尘
                { 17, (NumericType.Attr_157, NumericType.Attr_167) }, // 清汐
            };

        /// <summary>计算物理/法术伤害主入口。</summary>
        public static void CalculateDamage(SkillEditorFunctionContext ctx, SkillEditorDamageKind kind)
        {
            // 参数 0~3：施法者、目标、技能 ID、技能等级
            Unit caster = ctx.ResolveUnit(ctx.GetParamRaw(0));
            Unit target = ctx.ResolveUnit(ctx.GetParamRaw(1));
            int skillId = ctx.GetParamInt(2, ctx.SkillId);
            int level = ctx.GetParamInt(3, ctx.SkillLevel);
            if (caster == null || target == null || caster.IsDisposed || target.IsDisposed)
            {
                return;
            }

            // 未命中 / 免疫 / 闪避时不计算伤害
            long rs = ctx.GetVariable("rs", (long)SkillEditorHitResult.Hit);
            if (rs == (long)SkillEditorHitResult.Miss
                || rs == (long)SkillEditorHitResult.Immune
                || rs == (long)SkillEditorHitResult.Dodge)
            {
                return;
            }

            NumericComponent casterNumeric = caster.GetComponent<NumericComponent>();
            NumericComponent targetNumeric = target.GetComponent<NumericComponent>();
            if (casterNumeric == null || targetNumeric == null)
            {
                return;
            }

            // 参数 4~7：物理/法术/物防/法防威力（文档：物理威力=0.2 等，1=100%）
            double physicalPower = GetParamDouble(ctx, 4, 0d);
            double magicPower = GetParamDouble(ctx, 5, 0d);
            double pdefPower = GetParamDouble(ctx, 6, 0d);
            double mdefPower = GetParamDouble(ctx, 7, 0d);
            // 物理伤害若未配威力，默认物理威力=1
            ApplyDefaultPower(kind, ref physicalPower, ref magicPower);

            // 【文档第一段】威力部分 + 二次项 + 额外加成 → 442~669
            ResolveBaseDamageRange(
                ctx,
                caster,
                target,
                casterNumeric,
                targetNumeric,
                level,
                physicalPower,
                magicPower,
                pdefPower,
                mdefPower,
                out double normalMin,
                out double normalMax);

            // 【文档】常规分摊数=2 → 442/2 ~ 669/2 = 221 ~ 334.5
            int normalSplit = Math.Max(1, ctx.GetParamInt(35, 1));
            normalMin /= normalSplit;
            normalMax /= normalSplit;
            // 【文档】如果最大值 < 最小值，则最大值 = 最小值
            NormalizeRange(ref normalMin, ref normalMax);

            // 【属性公式】在 [221, 334.5] 间随机得到分摊后攻击值（尚未扣防）
            double rawNormalDamage = RollDouble(normalMin, normalMax);

            // 【属性作用】破防 x → 生效防御 y → 减伤比例 → 常规伤害 × (1-减伤比例)
            ResolveDefenseMitigation(
                ctx,
                caster,
                casterNumeric,
                targetNumeric,
                kind,
                out double effectiveDefenseY,
                out double damageReductionRatio);
            double mitigatedNormalDamage = ApplyDamageReduction(rawNormalDamage, damageReductionRatio);
            long normalDamage = FloorPositiveDamage(mitigatedNormalDamage);

            // 暴击（技能参数 21 + 攻击方爆伤属性 72）
            double critBonus = GetParamDouble(ctx, 21, 0d);
            if (rs >= (long)SkillEditorHitResult.Crit)
            {
                double critMultiplier = DefaultCritMultiplier
                    + critBonus
                    + NumericConvert.GetRatioBonus(casterNumeric, NumericType.P_CRI_DMG_Fixed_72);
                normalDamage = FloorPositiveDamage(normalDamage * critMultiplier);
            }

            // 【文档第二段】五系附伤，分摊后逐系 max(0, 攻击附伤+技能附伤/分摊-抵抗)，求和=185
            long elementalDamage = ResolveElementalDamage(ctx, caster, target, 36);

            // 【文档第三段】生命比例附伤；参数 31=true 时同样套用属性作用的减伤比例
            long hpDamage = ResolveHpDamage(
                ctx,
                casterNumeric,
                targetNumeric,
                damageReductionRatio,
                GetParamBool(ctx, 31, true));

            // 总伤害 = 常规 + 五系 + 生命
            long totalDamage = normalDamage + elementalDamage + hpDamage;
            // 职业克制：攻击方对目标职业加成 − 目标对攻击方职业减免（千分比）
            totalDamage = ApplyOccupationDamageModifier(caster, target, casterNumeric, targetNumeric, totalDamage);
            if (totalDamage <= 0)
            {
                return;
            }

            // 参数 38：是否无视护盾；先扣护盾再扣 HP_Current_8
            bool ignoreShield = GetParamBool(ctx, 38, false);
            int damageType = rs >= (long)SkillEditorHitResult.Crit ? 1 : 0;
            ctx.SetVariable("damageTotal", totalDamage);
            ApplyDamage(caster, target, targetNumeric, totalDamage, skillId, ignoreShield, damageType);

            // 参数 32~34：三类伤害的吸血比例
            ApplyLifeSteal(caster, casterNumeric, normalDamage, GetParamDouble(ctx, 32, 0d), skillId);
            ApplyLifeSteal(caster, casterNumeric, elementalDamage, GetParamDouble(ctx, 33, 0d), skillId);
            ApplyLifeSteal(caster, casterNumeric, hpDamage, GetParamDouble(ctx, 34, 0d), skillId);

            // 参数 39：仇恨倍率（供仇恨系统读取）
            double hateMultiplier = GetParamDouble(ctx, 39, 1d);
            if (hateMultiplier > 0d && Log.IsDebugEnabled)
            {
                Log.Debug(
                    $"SkillEditor hate skill={skillId} caster={caster.Id} target={target.Id} damage={totalDamage} multiplier={hateMultiplier.ToString(CultureInfo.InvariantCulture)}");
            }

            if (Log.IsDebugEnabled)
            {
                Log.Debug(
                    $"CALCULATE_{(kind == SkillEditorDamageKind.Physics ? "PHYSICS" : "MAGIC")}_DAMAGE skill={skillId} level={level} caster={caster.Id} target={target.Id} rs={rs} y={effectiveDefenseY.ToString(CultureInfo.InvariantCulture)} reduction={damageReductionRatio.ToString(CultureInfo.InvariantCulture)} normal={normalDamage} elemental={elementalDamage} hp={hpDamage} total={totalDamage}");
            }
        }

        /// <summary>
        /// CALCULATE_HEAL_DAMAGE。
        /// 参数表来源：SkillEditor_v1/bin/Debug/DocEditor界面配置.xml → function name=CALCULATE_HEAL_DAMAGE 的 param_format（共 26 项，下标 0 起）。
        /// 常规段复用 ResolveBaseDamageRange（与 CALCULATE_PHYSICS/MAGIC_DAMAGE 参数 4~17 同布局）；
        /// 生命比例段对齐伤害侧 ResolveHpDamage 的 min(HP_Max×比例,上限) 写法（伤害用参数 27~30/37，治疗用 19~22/24）。
        /// 治疗不做破防/减伤。威力全 0 时可靠 8~13 常数项出固定治疗（如初级生命药剂常数 1000）。
        /// </summary>
        public static void CalculateHeal(SkillEditorFunctionContext ctx)
        {
            // —— 参数 0~3：施法者 / 目标 / 技能ID / 技能等级（界面配置.xml）——
            Unit caster = ctx.ResolveUnit(ctx.GetParamRaw(0));
            Unit target = ctx.ResolveUnit(ctx.GetParamRaw(1));
            int skillId = ctx.GetParamInt(2, ctx.SkillId);
            int level = ctx.GetParamInt(3, ctx.SkillLevel);
            if (caster == null || target == null || caster.IsDisposed || target.IsDisposed)
            {
                return;
            }

            // rs 来自 SKILL_DAMAGE_CHECK_HEAL；未命中等不结算治疗
            long rs = ctx.GetVariable("rs", (long)SkillEditorHitResult.Hit);
            if (rs == (long)SkillEditorHitResult.Miss
                || rs == (long)SkillEditorHitResult.Immune
                || rs == (long)SkillEditorHitResult.Dodge)
            {
                return;
            }

            NumericComponent casterNumeric = caster.GetComponent<NumericComponent>();
            NumericComponent targetNumeric = target.GetComponent<NumericComponent>();
            if (casterNumeric == null || targetNumeric == null)
            {
                return;
            }

            // —— 参数 4~7：物理/法术/物防/法防威力（界面 desc「xxx威力 1=100%」）——
            // 不调用 ApplyDefaultPower：治疗允许全 0，仅靠等级附加常数项（药剂）
            double physicalPower = GetParamDouble(ctx, 4, 0d);
            double magicPower = GetParamDouble(ctx, 5, 0d);
            double pdefPower = GetParamDouble(ctx, 6, 0d);
            double mdefPower = GetParamDouble(ctx, 7, 0d);

            // —— 参数 8~17：最小/最大攻击附加二次·一次·常数 + 额外加成属性/系数 ——
            // 来源：与伤害函数同一套 ResolveBaseDamageRange（见本文件 CalculateDamage / 属性公式注释）
            ResolveBaseDamageRange(
                ctx,
                caster,
                target,
                casterNumeric,
                targetNumeric,
                level,
                physicalPower,
                magicPower,
                pdefPower,
                mdefPower,
                out double normalMin,
                out double normalMax);

            //用两个 out 吐出区间： 还没随机、没分摊、没破防。
            //baseMin：常规伤害/治疗下限
            //baseMax：常规伤害 / 治疗上限

            // —— 参数 23：常规治疗分摊数（界面「常规治疗分摊数」，默认 1）——
            // 对照伤害 CalculateDamage 参数 35「常规分摊数」：区间先除分摊再随机
            int normalSplit = Math.Max(1, ctx.GetParamInt(23, 1));
            normalMin /= normalSplit;
            normalMax /= normalSplit;
            NormalizeRange(ref normalMin, ref normalMax);

            long normalHeal = FloorPositiveDamage(RollDouble(normalMin, normalMax));

            // —— 参数 18：暴击治疗加成 1=100%（界面「暴击治疗加成」）——
            // 对照伤害参数 21 暴击加成；治疗侧无爆伤属性 72，仅用默认 1.5 + 本参数
            double critHealBonus = GetParamDouble(ctx, 18, 0d);
            if (rs >= (long)SkillEditorHitResult.Crit)
            {
                double critMultiplier = DefaultCritMultiplier + critHealBonus;
                normalHeal = FloorPositiveDamage(normalHeal * critMultiplier);
            }

            // ========== 生命比例治疗段 ==========
            // 参数来源：DocEditor界面配置.xml
            //   19 施法者最大生命比例治疗 1=100%
            //   20 施法者生命治疗最大值
            //   21 目标最大生命比例治疗 1=100%
            //   22 目标生命治疗最大值
            //   24 生命治疗分摊数（默认 1）
            // 算法来源：本文件 ResolveHpDamage（伤害参数 27/28 施法者、29/30 目标、37 生命分摊）
            //   单端 = CapPercentDamage(HP_Max, 比例, 上限) = min(HP_Max×比例, 上限>0?上限:不封顶)
            //   施法者部分再 / 生命分摊数；目标部分不分摊（与伤害文档一致）
            // 治疗差异：不做参数 31 那种「是否套减伤」；治疗直接加血
            int lifeSplit = Math.Max(1, ctx.GetParamInt(24, 1));
            double casterHpPart = CapPercentDamage(
                casterNumeric.GetAsFloat(NumericType.HP_Max_10),
                GetParamDouble(ctx, 19, 0d),
                GetParamDouble(ctx, 20, 0d)) / lifeSplit;
            double targetHpPart = CapPercentDamage(
                targetNumeric.GetAsFloat(NumericType.HP_Max_10),
                GetParamDouble(ctx, 21, 0d),
                GetParamDouble(ctx, 22, 0d));
            long hpHeal = FloorPositiveDamage(casterHpPart + targetHpPart);

            // 总量 = 常规治疗 + 生命比例治疗（对照伤害：常规 + 五系 + 生命；治疗无五系）
            long totalHeal = normalHeal + hpHeal;
            if (totalHeal <= 0)
            {
                ctx.SetVariable("damageTotal", 0);
                ctx.SetVariable("healTotal", 0);
                return;
            }

            targetNumeric.ApplyChange(caster, NumericType.HP_Current_8, totalHeal, skillId);
            // damageTotal：兼容 INFORM_CLIENT_HIT_SUCCESS；healTotal：给 INFORM_EVENT_HEAL
            ctx.SetVariable("damageTotal", totalHeal);
            ctx.SetVariable("healTotal", totalHeal);

            // —— 参数 25：仇恨倍率（界面「仇恨倍率」，默认 1；当前仅日志，对照伤害参数 39）——
            double hateMultiplier = GetParamDouble(ctx, 25, 1d);
            if (hateMultiplier > 0d && Log.IsDebugEnabled)
            {
                Log.Debug(
                    $"SkillEditor heal hate skill={skillId} caster={caster.Id} target={target.Id} heal={totalHeal} multiplier={hateMultiplier.ToString(CultureInfo.InvariantCulture)}");
            }

            if (Log.IsDebugEnabled)
            {
                Log.Debug(
                    $"CALCULATE_HEAL_DAMAGE skill={skillId} level={level} caster={caster.Id} target={target.Id} rs={rs} normal={normalHeal} hp={hpHeal} total={totalHeal}");
            }
        }

        //怎么算出来的（三部分相加）
        //威力段
        //物攻/法攻/物防/法防 × 对应威力 → powerMin / powerMax

        //等级附加（参数 8~13）
        //a* level² + b* level + c → minGrowth / maxGrowth
        //额外加成（参数 14~17）
        //属性×系数（Min/Max 同一份）→ extraBonus
        /// <summary>
        /// 【文档】三部分相加得到常规伤害区间：
        /// 威力部分(152~289) + 二次项(90~180) + 额外加成(200~200) = 442~669
        /// </summary>
        private static void ResolveBaseDamageRange(
            SkillEditorFunctionContext ctx,
            Unit caster,
            Unit target,
            NumericComponent casterNumeric,
            NumericComponent targetNumeric,
            int level,
            double physicalPower,
            double magicPower,
            double pdefPower,
            double mdefPower,
            out double baseMin,
            out double baseMax)
        {
            // --- 读取攻击/防御区间（文档：物攻100~200，法攻200~400，物防50~70，法防90~140）---
            double minPatk = casterNumeric.GetAsFloat(NumericType.PATK_Min_21); // 最小物理攻击
            double maxPatk = casterNumeric.GetAsFloat(NumericType.PATK_Max_22); // 最大物理攻击
            double minMatk = casterNumeric.GetAsFloat(NumericType.MATK_Min_31); // 最小法术攻击
            double maxMatk = casterNumeric.GetAsFloat(NumericType.MATK_Max_32); // 最大法术攻击
            double minPdef = targetNumeric.GetAsFloat(NumericType.PDEF_Min_41); // 受击方最小物防（物防威力≠0时参与）
            double maxPdef = targetNumeric.GetAsFloat(NumericType.PDEF_Max_42);
            double minMdef = targetNumeric.GetAsFloat(NumericType.MDEF_Min_51); // 受击方最小法防（法防威力≠0时参与）
            double maxMdef = targetNumeric.GetAsFloat(NumericType.MDEF_Max_52);

            // --- 【威力部分】各区间分别乘威力后相加 ---
            // 文档示例 Min：100*0.2 + 200*0.5 + 50*0.1 + 90*0.3 = 20+100+5+27 = 152
            double powerMin = minPatk * physicalPower
                + minMatk * magicPower
                + minPdef * pdefPower
                + minMdef * mdefPower;
            // 文档示例 Max：200*0.2 + 400*0.5 + 70*0.1 + 140*0.3 = 40+200+7+42 = 289
            double powerMax = maxPatk * physicalPower
                + maxMatk * magicPower
                + maxPdef * pdefPower
                + maxMdef * mdefPower;
            NormalizeRange(ref powerMin, ref powerMax);

            // --- 【二次项 / 等级成长】a*level² + b*level + c ---
            // 文档：level=10，最小 0.5*10*10+2*10+20=90；最大 1*10*10+4*10+40=180
            // 参数 8~10：最小攻击附加 二次/一次/常数项；11~13：最大攻击附加
            double minGrowth = EvalLevelGrowth(
                level,
                GetParamDouble(ctx, 8, 0d),
                GetParamDouble(ctx, 9, 0d),
                GetParamDouble(ctx, 10, 0d));
            double maxGrowth = EvalLevelGrowth(
                level,
                GetParamDouble(ctx, 11, 0d),
                GetParamDouble(ctx, 12, 0d),
                GetParamDouble(ctx, 13, 0d));
            NormalizeRange(ref minGrowth, ref maxGrowth);

            // --- 【额外加成】属性1*系数1 + 属性2*系数2 ---
            // 文档：速度500*0.2 + 力量1000*0.1 = 200（Min=Max=200）
            // 参数 14/15、16/17：额外加成属性与系数
            double extraBonus = ResolveExtraBonus(ctx, caster, 14, 15) + ResolveExtraBonus(ctx, caster, 16, 17);

            // --- 【三部分相加】152~289 + 90~180 + 200 = 442~669 ---
            baseMin = powerMin + minGrowth + extraBonus;
            baseMax = powerMax + maxGrowth + extraBonus;
            NormalizeRange(ref baseMin, ref baseMax);
        }

        /// <summary>
        /// 【文档第二段】五系附伤。
        /// 单系：max(0, 攻击方附伤 + 技能附伤/五系分摊数 - 防御方抵抗)
        /// 示例：风 max(0, 20+400/2-100)=120；合计 185。
        /// </summary>
        private static long ResolveElementalDamage(
            SkillEditorFunctionContext ctx,
            Unit caster,
            Unit target,
            int splitParamIndex)
        {
            // 参数 36：五系分摊数（文档示例=2）
            int split = Math.Max(1, ctx.GetParamInt(splitParamIndex, 1));
            double total = 0d;
            foreach (ElementalDamageConfig config in ElementalConfigs)
            {
                // 攻击方该系附伤（Attribute 130~139）
                double attackerValue = ctx.GetUnitNumericDisplayValue(caster, config.AttackerAttrId, 0d);
                // 技能该系附伤：仅水/火/风/雷/毒有参数 22~26
                double skillValue = config.SkillParamIndex >= 0
                    ? GetParamDouble(ctx, config.SkillParamIndex, 0d)
                    : 0d;
                // 防御方该系减免（Attribute 140~149）
                double resistValue = ctx.GetUnitNumericDisplayValue(target, config.DefenderResistAttrId, 0d);
                // max(0, 攻击附伤 + 技能附伤/分摊 - 减免)
                total += Math.Max(0d, attackerValue + skillValue / split - resistValue);
            }

            if (total <= 0d)
            {
                return 0;
            }

            // 五系合计向下取整（文档示例 total=185）
            return (long)Math.Floor(total);
        }

        /// <summary>
        /// 【文档第三段】最大生命比例附伤。
        /// 施法者部分=min(HP_Max*比例,上限)/生命分摊数；目标部分=min(HP_Max*比例,上限)。
        /// 参数 31=true 时，再乘 (1-减伤比例)（属性作用文档）。
        /// </summary>
        private static long ResolveHpDamage(
            SkillEditorFunctionContext ctx,
            NumericComponent casterNumeric,
            NumericComponent targetNumeric,
            double damageReductionRatio,
            bool useReduction)
        {
            // 施法者/目标最大生命（文档：10000 / 5000）
            double casterHpMax = casterNumeric.GetAsFloat(NumericType.HP_Max_10);
            double targetHpMax = targetNumeric.GetAsFloat(NumericType.HP_Max_10);
            // 参数 37：生命分摊数（文档=2，仅除施法者部分）
            int lifeSplit = Math.Max(1, ctx.GetParamInt(37, 1));

            // 施法者部分 = min(10000*0.2, 1000) / 2 = 500（参数 27 比例、28 上限）
            double casterPart = CapPercentDamage(
                casterHpMax,
                GetParamDouble(ctx, 27, 0d),
                GetParamDouble(ctx, 28, 0d)) / lifeSplit;
            // 目标部分 = min(5000*0.3, 2000) = 1500（参数 29 比例、30 上限，不再除以分摊数）
            double targetPart = CapPercentDamage(
                targetHpMax,
                GetParamDouble(ctx, 29, 0d),
                GetParamDouble(ctx, 30, 0d));

            // 最大生命附伤 = 施法者部分 + 目标部分
            double hpDamage = casterPart + targetPart;
            if (hpDamage <= 0d)
            {
                return 0;
            }

            // 【属性作用 / 属性公式】是否套用减伤比例，由 skill 参数 31 传入
            if (useReduction)
            {
                hpDamage = ApplyDamageReduction(hpDamage, damageReductionRatio);
            }

            if (hpDamage <= 0d)
            {
                return 0;
            }

            return (long)Math.Floor(hpDamage);
        }

        /// <summary>
        /// 【属性作用】常规段防御减免：破防 x → 生效防御 y → 减伤比例。
        /// </summary>
        private static void ResolveDefenseMitigation(
            SkillEditorFunctionContext ctx,
            Unit caster,
            NumericComponent casterNumeric,
            NumericComponent targetNumeric,
            SkillEditorDamageKind kind,
            out double effectiveDefenseY,
            out double damageReductionRatio)
        {
            // x = max(0, 攻击方 62|63 + 无视防御比例(18) - 受击方 64|65)
            double breakRatioX = ResolvePenetrationBreakRatio(ctx, casterNumeric, targetNumeric, kind);
            // y = max(0, (1-x)*受击方防御 - 攻击方 60|61 - 无视最小~最大防御(19~20))，在区间内随机
            effectiveDefenseY = ResolveEffectiveDefenseY(ctx, casterNumeric, targetNumeric, kind, breakRatioX);
            // 减伤比例 = 1 / (1 + y/135 + 45×攻击方等级)
            int attackerLevel = ResolveUnitLevel(caster);
            damageReductionRatio = ResolveDamageReductionRatio(effectiveDefenseY, attackerLevel);
        }

        /// <summary>
        /// 【属性作用】破防比例 x = max(0, 攻击方穿透 + 技能无视防御比例 - 受击方穿透抵抗)。
        /// 物理：62 vs 64；法术：63 vs 65。
        /// </summary>
        private static double ResolvePenetrationBreakRatio(
            SkillEditorFunctionContext ctx,
            NumericComponent casterNumeric,
            NumericComponent targetNumeric,
            SkillEditorDamageKind kind)
        {
            double attackerPenRatio;
            double defenderPenResist;
            if (kind == SkillEditorDamageKind.Magic)
            {
                attackerPenRatio = casterNumeric.GetAsFloat(NumericType.M_PEN_PerMyriad_63);
                defenderPenResist = targetNumeric.GetAsFloat(NumericType.M_PEN_RES_PerMyriad_65);
            }
            else
            {
                attackerPenRatio = casterNumeric.GetAsFloat(NumericType.P_PEN_PerMyriad_62);
                defenderPenResist = targetNumeric.GetAsFloat(NumericType.P_PEN_RES_PerMyriad_64);
            }

            // 参数 18：无视防御比例（1=100%），与穿透比例相加参与破防
            double skillIgnoreDefRatio = GetParamDouble(ctx, 18, 0d);
            return Math.Max(0d, attackerPenRatio + skillIgnoreDefRatio - defenderPenResist);
        }

        /// <summary>
        /// 【属性作用】生效防御 y：受击方防御区间经破防与固定穿透、技能无视固定防御后随机取值。
        /// y = max(0, (1-x)×Def - 攻击方 60|61 - 无视最小~最大防御)。
        /// </summary>
        private static double ResolveEffectiveDefenseY(
            SkillEditorFunctionContext ctx,
            NumericComponent casterNumeric,
            NumericComponent targetNumeric,
            SkillEditorDamageKind kind,
            double breakRatioX)
        {
            double minDef;
            double maxDef;
            double penFixed;
            if (kind == SkillEditorDamageKind.Magic)
            {
                minDef = targetNumeric.GetAsFloat(NumericType.MDEF_Min_51);
                maxDef = targetNumeric.GetAsFloat(NumericType.MDEF_Max_52);
                penFixed = casterNumeric.GetAsFloat(NumericType.M_PEN_Fixed_61);
            }
            else
            {
                minDef = targetNumeric.GetAsFloat(NumericType.PDEF_Min_41);
                maxDef = targetNumeric.GetAsFloat(NumericType.PDEF_Max_42);
                penFixed = casterNumeric.GetAsFloat(NumericType.P_PEN_Fixed_60);
            }

            // 参数 19/20：无视最小/最大防御（分别对应受击方防御区间两端）
            double ignoreMinDef = GetParamDouble(ctx, 19, 0d);
            double ignoreMaxDef = GetParamDouble(ctx, 20, 0d);
            double yMin = Math.Max(0d, (1d - breakRatioX) * minDef - penFixed - ignoreMinDef);
            double yMax = Math.Max(0d, (1d - breakRatioX) * maxDef - penFixed - ignoreMaxDef);
            NormalizeRange(ref yMin, ref yMax);
            return RollDouble(yMin, yMax);
        }

        /// <summary>【属性作用】减伤比例 = 1 / (1 + y/135 + 45×攻击方等级)。</summary>
        private static double ResolveDamageReductionRatio(double effectiveDefenseY, int attackerLevel)
        {
            double level = Math.Max(0, attackerLevel);
            double denominator = 1d + effectiveDefenseY / DefenseMitigationDivisor + DefenseMitigationLevelFactor * level;
            if (denominator <= 0d)
            {
                return 0d;
            }

            return Clamp01(1d / denominator);
        }

        /// <summary>【属性作用】最终伤害 = 攻击 × (1 - 减伤比例)。</summary>
        private static double ApplyDamageReduction(double rawDamage, double damageReductionRatio)
        {
            if (rawDamage <= 0d)
            {
                return 0d;
            }

            return Math.Max(0d, rawDamage * (1d - Clamp01(damageReductionRatio)));
        }

        /// <summary>
        /// 职业克制（Attribute 150~167，千分比）。
        /// 攻击方读「对目标职业伤害」；目标读「减免攻击方职业伤害」。
        /// 最终：damage × max(0, 1 + 加成 − 减免)。
        /// </summary>
        private static long ApplyOccupationDamageModifier(
            Unit caster,
            Unit target,
            NumericComponent casterNumeric,
            NumericComponent targetNumeric,
            long totalDamage)
        {
            if (totalDamage <= 0)
            {
                return totalDamage;
            }

            int targetOcc = ResolveUnitOccupation(target);
            int casterOcc = ResolveUnitOccupation(caster);
            double bonus = 0d;
            double resist = 0d;

            if (OccupationDamageAttrs.TryGetValue(targetOcc, out var vsTarget) && vsTarget.BonusAttrId > 0)
            {
                bonus = NumericConvert.GetRatioBonus(casterNumeric, vsTarget.BonusAttrId);
            }

            if (OccupationDamageAttrs.TryGetValue(casterOcc, out var vsCaster) && vsCaster.ResistAttrId > 0)
            {
                resist = NumericConvert.GetRatioBonus(targetNumeric, vsCaster.ResistAttrId);
            }

            if (bonus == 0d && resist == 0d)
            {
                return totalDamage;
            }

            double factor = 1d + bonus - resist;
            if (factor <= 0d)
            {
                return 0;
            }

            return FloorPositiveDamage(totalDamage * factor);
        }

        /// <summary>单位职业：玩家取 RoleInfo.Occ；宠物取主人；怪物取 LDMonster.Occupation。</summary>
        private static int ResolveUnitOccupation(Unit unit)
        {
            if (unit == null || unit.IsDisposed)
            {
                return 0;
            }

            if (unit.Type == UnitType.Player)
            {
                return unit.GetComponent<RoleInfoComponentServer>()?.RoleInfo?.Occ ?? 0;
            }

            if (unit.Type == UnitType.Pet || unit.Type == UnitType.JingLing)
            {
                Unit master = unit.GetParent<UnitComponent>()?.Get(unit.MasterId);
                if (master != null && !master.IsDisposed && master.Type == UnitType.Player)
                {
                    return master.GetComponent<RoleInfoComponentServer>()?.RoleInfo?.Occ ?? 0;
                }
            }

            if (LDMonsterCategory.Instance.Contain(unit.ConfigId))
            {
                return LDMonsterCategory.Instance.Get(unit.ConfigId).Occupation;
            }

            return 0;
        }

        /// <summary>攻击方等级：玩家/宠物取主人等级，怪物取配置等级。</summary>
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

        /// <summary>物理/法术伤害未配置任何威力时，给默认主威力=1。</summary>
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

        /// <summary>
        /// 单条额外加成：属性显示值 × 系数。
        /// 文档：速度500×0.2 或 力量1000×0.1。
        /// </summary>
        private static double ResolveExtraBonus(SkillEditorFunctionContext ctx, Unit unit, int attrIndex, int coefIndex)
        {
            int numericType = ctx.ResolveNumericType(ctx.GetParamRaw(attrIndex), 0);
            if (numericType <= 0)
            {
                return 0d;
            }

            double coef = GetParamDouble(ctx, coefIndex, 0d);
            if (Math.Abs(coef) < 1e-9)
            {
                return 0d;
            }

            return ctx.GetUnitNumericDisplayValue(unit, numericType, 0d) * coef;
        }

        /// <summary>
        /// min(HP_Max × 比例, 上限)，浮点结果。
        /// 文档：min(10000×0.2, 1000)=1000；min(5000×0.3, 2000)=1500。
        /// </summary>
        private static double CapPercentDamage(double hpMax, double ratio, double cap)
        {
            if (hpMax <= 0d || ratio <= 0d)
            {
                return 0d;
            }

            double damage = hpMax * ratio;
            if (cap > 0d && damage > cap)
            {
                damage = cap;
            }

            return Math.Max(0d, damage);
        }

        /// <summary>扣护盾后扣当前 HP（HP_Current_8 为显示刻度整数）。</summary>
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
                //long shieldHp = targetNumeric.GetStoredValue(NumericType.Now_Shield_HP);
                //if (shieldHp > 0)
                //{
                //    long absorbed = Math.Min(shieldHp, damageToHp);
                //    targetNumeric.ApplyChange(caster, NumericType.Now_Shield_HP, -absorbed, skillId, true, damageType);
                //    damageToHp -= absorbed;
                //}
            }

            if (damageToHp <= 0)
            {
                return;
            }

            targetNumeric.ApplyChange(caster, NumericType.HP_Current_8, -damageToHp, skillId, true, damageType);
        }

        /// <summary>按伤害 × 吸血比例回复施法者 HP，向下取整。</summary>
        private static void ApplyLifeSteal(Unit caster, NumericComponent casterNumeric, long damage, double ratio, int skillId)
        {
            if (caster == null || casterNumeric == null || damage <= 0 || ratio <= 0d)
            {
                return;
            }

            long heal = (long)Math.Floor(damage * ratio);
            if (heal <= 0)
            {
                return;
            }

            casterNumeric.ApplyChange(caster, NumericType.HP_Current_8, heal, skillId);
        }

        /// <summary>在 [min, max] 闭区间内均匀随机（浮点）。</summary>
        private static double RollDouble(double minValue, double maxValue)
        {
            if (minValue >= maxValue)
            {
                return minValue;
            }

            double range = maxValue - minValue;
            return minValue + RandomHelper.RandFloat01() * range;
        }

        /// <summary>
        /// 【文档】等级成长二次多项式：a×level² + b×level + c。
        /// level 为技能等级本身（文档 level=10 → 0.5×10×10+2×10+20=90）。
        /// </summary>
        private static double EvalLevelGrowth(int level, double quad, double linear, double constant)
        {
            double lv = Math.Max(0, level);
            return quad * lv * lv + linear * lv + constant;
        }

        /// <summary>【文档】若 max &lt; min，则交换，保证 max = min。</summary>
        private static void NormalizeRange(ref double minValue, ref double maxValue)
        {
            if (minValue > maxValue)
            {
                double tmp = minValue;
                minValue = maxValue;
                maxValue = tmp;
            }
        }

        /// <summary>【文档】向下取整；正伤害至少为 1。</summary>
        private static long FloorPositiveDamage(double value)
        {
            if (value <= 0d)
            {
                return 0;
            }

            return Math.Max(1L, (long)Math.Floor(value));
        }

        /// <summary>读取技能编辑器第 index 个参数并解析为 double。</summary>
        private static double GetParamDouble(SkillEditorFunctionContext ctx, int index, double defaultValue)
        {
            return SkillEditorFunctionContext.ParseDouble(ctx.ResolveParam(ctx.GetParamRaw(index)), defaultValue);
        }

        /// <summary>读取技能编辑器布尔参数。</summary>
        private static bool GetParamBool(SkillEditorFunctionContext ctx, int index, bool defaultValue)
        {
            return ctx.GetParamBool(index, defaultValue);
        }

        /// <summary>比例限制在 [0, 1]（如无视防御比例）。</summary>
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

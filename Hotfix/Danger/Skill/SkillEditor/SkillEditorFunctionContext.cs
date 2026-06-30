using System;
using System.Collections.Generic;
using System.Globalization;

namespace ET
{
    /// <summary>
    /// Runtime context passed to SkillEditor helper functions.
    /// Variables are stored as strings (like v0/Lua) so bool/int/double literals all work in LOGIC_RELATION.
    /// </summary>
    public class SkillEditorFunctionContext
    {

        public Skill_TreeEditor Handler;
        public SkillEditorSkillLogic Logic;
        public SkillEditorTreeNode Node;

        /// <summary>Tree variables (rs, hasTarget, _hpPct, ...). Values kept as string.</summary>
        public readonly Dictionary<string, string> Variables = new Dictionary<string, string>(StringComparer.Ordinal);

        /// <summary>Per-unit buff custom values: key = "{unitId}_{buffId}", length 5 for VALUE1..5.</summary>
        public readonly Dictionary<string, string[]> BuffCustomData = new Dictionary<string, string[]>(StringComparer.Ordinal);

        /// <summary>Set by condition functions; used when IF node has no operators.</summary>
        public bool LastConditionResult;

        public int SkillId => this.Handler?.LdSkillConf?.Id ?? 0;
        public int SkillLevel => 1;

        public string GetParamRaw(int index)
        {
            if (this.Node?.Params == null || index < 0 || index >= this.Node.Params.Count)
            {
                return string.Empty;
            }

            return this.Node.Params[index] ?? string.Empty;
        }

        public string GetParamSkillIdColumn(int index)
        {
            if (this.Node?.ParamSkillIds == null || index < 0 || index >= this.Node.ParamSkillIds.Count)
            {
                return string.Empty;
            }

            return this.Node.ParamSkillIds[index] ?? string.Empty;
        }

        public string ResolveVarName(string raw, string fallback = "")
        {
            string token = ExtractToken(raw);
            return string.IsNullOrEmpty(token) ? fallback : token;
        }

        public string ResolveParam(string raw)
        {
            if (string.IsNullOrEmpty(raw))
            {
                return raw;
            }

            string token = ExtractToken(raw);
            if (string.IsNullOrEmpty(token))
            {
                return raw;
            }

            if (this.Variables.TryGetValue(token, out string value))
            {
                return value;
            }

            switch (token.ToLowerInvariant())
            {
                case "skillid":
                case "skill_id":
                    return this.SkillId.ToString();
                case "level":
                    return this.SkillLevel.ToString();
                case "rs":
                    return this.GetVariableString("rs", "0");
                case "stype":
                    return this.Variables.TryGetValue("sType", out string sType) ? sType : raw;
                default:
                    return raw;
            }
        }

        public Unit ResolveUnit(string raw)
        {
            return ResolveUnitByToken(ExtractUnitToken(raw));
        }

        public float ResolvePositionComponent(string raw, char axis)
        {
            string unitToken = ExtractUnitToken(raw);
            int dot = unitToken.LastIndexOf('.');
            if (dot >= 0)
            {
                axis = unitToken[dot + 1];
                unitToken = unitToken.Substring(0, dot);
            }

            Unit unit = ResolveUnitByToken(unitToken);
            if (unit == null)
            {
                return 0f;
            }

            return axis == 'z' ? unit.Position.z : unit.Position.x;
        }

        public long ResolveNumericAttribute(Unit unit, string raw, long defaultValue = 0)
        {
            if (unit == null)
            {
                return defaultValue;
            }

            NumericComponent numeric = unit.GetComponent<NumericComponent>();
            if (numeric == null)
            {
                return defaultValue;
            }

            string token = raw?.Trim() ?? string.Empty;
            if (token.Contains("最小物理攻击"))
            {
                return numeric.GetAsLong(NumericType.PATK_Min);
            }

            if (token.Contains("最大物理攻击"))
            {
                return numeric.GetAsLong(NumericType.PATK_Max);
            }

            if (token.Contains("最小法术攻击"))
            {
                return numeric.GetAsLong(NumericType.MATK_Min);
            }

            if (token.Contains("最大法术攻击"))
            {
                return numeric.GetAsLong(NumericType.MATK_Max);
            }

            return ParseLong(this.ResolveParam(raw), defaultValue);
        }

        public string GetBuffDataKey(Unit owner, int buffId)
        {
            return owner == null ? $"0_{buffId}" : $"{owner.Id}_{buffId}";
        }

        public string[] GetOrCreateBuffDataValues(Unit owner, int buffId)
        {
            string key = GetBuffDataKey(owner, buffId);
            if (!this.BuffCustomData.TryGetValue(key, out string[] values))
            {
                values = new string[5];
                this.BuffCustomData[key] = values;
            }

            return values;
        }

        private Unit ResolveUnitByToken(string token)
        {
            token = token?.Trim().ToLowerInvariant() ?? string.Empty;
            switch (token)
            {
                case "caster":
                    return this.Handler?.TheUnitFrom;
                case "target":
                    return this.Handler?.TheUnitTarget;
                case "buffcaster":
                case "buff.parent":
                    return this.Handler?.TheUnitFrom;
                case "caster.parent":
                    return this.Handler?.TheUnitFrom?.Parent as Unit;
                case "targets":
                    return this.Handler?.TheUnitTarget;
                default:
                    if (this.Variables.ContainsKey(token))
                    {
                        return this.Handler?.TheUnitFrom;
                    }

                    return null;
            }
        }

        private static string ExtractUnitToken(string raw)
        {
            string token = ExtractToken(raw);
            int dot = token.LastIndexOf('.');
            if (dot > 0 && (token.EndsWith(".x", StringComparison.Ordinal) || token.EndsWith(".z", StringComparison.Ordinal)))
            {
                return token;
            }

            return token;
        }

        public bool GetParamBool(int index, bool defaultValue = true)
        {
            return ParseBool(this.ResolveParam(this.GetParamRaw(index)), defaultValue);
        }

        public int GetParamInt(int index, int defaultValue = 0)
        {
            string raw = this.ResolveParam(this.GetParamRaw(index));
            if (int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value))
            {
                return value;
            }

            if (double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out double dValue))
            {
                return (int)dValue;
            }

            return defaultValue;
        }

        public float GetParamFloat(int index, float defaultValue = 0f)
        {
            string raw = this.ResolveParam(this.GetParamRaw(index));
            if (float.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out float value))
            {
                return value;
            }

            return defaultValue;
        }

        public void SetVariable(string varName, string value)
        {
            if (string.IsNullOrEmpty(varName))
            {
                return;
            }

            this.Variables[varName] = value ?? string.Empty;
        }

        public void SetVariable(string varName, long value)
        {
            this.SetVariable(varName, value.ToString(CultureInfo.InvariantCulture));
        }

        public string GetVariableString(string varName, string defaultValue = "")
        {
            return this.Variables.TryGetValue(varName, out string value) ? value : defaultValue;
        }

        public long GetVariable(string varName, long defaultValue = 0)
        {
            return ParseLong(this.GetVariableString(varName), defaultValue);
        }

        public double GetVariableDouble(string varName, double defaultValue = 0d)
        {
            return ParseDouble(this.GetVariableString(varName), defaultValue);
        }

        public static bool ParseBool(string raw, bool defaultValue = false)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                return defaultValue;
            }

            if (bool.TryParse(raw.Trim(), out bool b))
            {
                return b;
            }

            return ParseLong(raw, 0) != 0;
        }

        public static long ParseLong(string raw, long defaultValue)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                return defaultValue;
            }

            raw = raw.Trim();
            if (long.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out long value))
            {
                return value;
            }

            if (int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out int intValue))
            {
                return intValue;
            }

            if (bool.TryParse(raw, out bool boolValue))
            {
                return boolValue ? 1 : 0;
            }

            if (double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out double dValue))
            {
                return (long)dValue;
            }

            return defaultValue;
        }

        public static double ParseDouble(string raw, double defaultValue = 0d)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                return defaultValue;
            }

            raw = raw.Trim();
            if (double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out double value))
            {
                return value;
            }

            if (double.TryParse(raw, NumberStyles.Float, CultureInfo.CurrentCulture, out value))
            {
                return value;
            }

            if (bool.TryParse(raw, out bool boolValue))
            {
                return boolValue ? 1d : 0d;
            }

            return ParseLong(raw, (long)defaultValue);
        }

        private static string ExtractToken(string raw)
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
    }
}

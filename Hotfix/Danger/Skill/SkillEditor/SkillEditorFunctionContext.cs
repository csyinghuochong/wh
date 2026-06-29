using System;
using System.Collections.Generic;

namespace ET
{
    /// <summary>
    /// Runtime context passed to SkillEditor helper functions.
    /// </summary>
    public class SkillEditorFunctionContext
    {
        public SkillHandler Handler;
        public SkillEditorSkillLogic Logic;
        public SkillEditorTreeNode Node;

        /// <summary>Tree variables (rs, sType, buffCaster, ...).</summary>
        public readonly Dictionary<string, long> Variables = new Dictionary<string, long>(StringComparer.Ordinal);

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

            if (this.Variables.TryGetValue(token, out long value))
            {
                return value.ToString();
            }

            switch (token.ToLowerInvariant())
            {
                case "skillid":
                    return this.SkillId.ToString();
                case "level":
                    return this.SkillLevel.ToString();
                case "rs":
                    return this.Variables.TryGetValue("rs", out long rs) ? rs.ToString() : "0";
                case "stype":
                    return this.Variables.TryGetValue("sType", out long sType) ? sType.ToString() : raw;
                default:
                    return raw;
            }
        }

        public Unit ResolveUnit(string raw)
        {
            string token = ExtractToken(raw).ToLowerInvariant();
            switch (token)
            {
                case "caster":
                    return this.Handler?.TheUnitFrom;
                case "target":
                    return this.Handler?.TheUnitTarget;
                case "buffcaster":
                    // TODO: bind buff caster when executing from buff context
                    return this.Handler?.TheUnitFrom;
                case "caster.parent":
                    return this.Handler?.TheUnitFrom?.Parent as Unit;
                default:
                    if (this.Variables.TryGetValue(token, out _))
                    {
                        return this.Handler?.TheUnitFrom;
                    }
                    return null;
            }
        }

        public bool GetParamBool(int index, bool defaultValue = true)
        {
            string raw = this.ResolveParam(this.GetParamRaw(index));
            if (string.IsNullOrWhiteSpace(raw))
            {
                return defaultValue;
            }

            if (bool.TryParse(raw, out bool value))
            {
                return value;
            }

            if (int.TryParse(raw, out int intValue))
            {
                return intValue != 0;
            }

            return defaultValue;
        }

        public int GetParamInt(int index, int defaultValue = 0)
        {
            string raw = this.ResolveParam(this.GetParamRaw(index));
            if (int.TryParse(raw, out int value))
            {
                return value;
            }

            return defaultValue;
        }

        public float GetParamFloat(int index, float defaultValue = 0f)
        {
            string raw = this.ResolveParam(this.GetParamRaw(index));
            if (float.TryParse(raw, out float value))
            {
                return value;
            }

            return defaultValue;
        }

        public void SetVariable(string varName, long value)
        {
            if (string.IsNullOrEmpty(varName))
            {
                return;
            }

            this.Variables[varName] = value;
        }

        public long GetVariable(string varName, long defaultValue = 0)
        {
            return this.Variables.TryGetValue(varName, out long value) ? value : defaultValue;
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

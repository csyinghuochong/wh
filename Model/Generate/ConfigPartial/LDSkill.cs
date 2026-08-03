using System;
using System.Collections.Generic;
using System.Text;

namespace ET
{
    public struct LDSkillAttributeLimit
    {
        public int CompareType;
        public int NumericType;
        public long Value;
    }


    public partial class LDSkill
    {
        /// <summary>表分隔：ASCII ~ / 全角～ / 波浪号 / 旧 _ `</summary>
        static readonly string[] TildeSeparators = { "\u007E", "\uFF5E", "\u301C", "_", "`" };

        public List<KeyValuePairInt> ReplaceSkillList = new List<KeyValuePairInt>();
        public List<LDSkillAttributeLimit> SelfAttributeLimits = new List<LDSkillAttributeLimit>();

        public void ParseRuntimeData()
        {
            this.ReplaceSkillList.Clear();
            this.SelfAttributeLimits.Clear();

            if (!string.IsNullOrEmpty(this.Replace_Skill) && this.Replace_Skill != "0")
            {
                string[] replaceItems = this.Replace_Skill.Split('|');
                for (int i = 0; i < replaceItems.Length; i++)
                {
                    string[] parts = replaceItems[i].Split(TildeSeparators, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length != 2)
                    {
                        continue;
                    }
                    if (!int.TryParse(parts[0], out int buffId) || !int.TryParse(parts[1], out int newSkillId))
                    {
                        continue;
                    }
                    KeyValuePairInt pair = new KeyValuePairInt();
                    pair.KeyId = buffId;
                    pair.Value = newSkillId;
                    this.ReplaceSkillList.Add(pair);
                }
            }

            if (!string.IsNullOrEmpty(this.Self_Attribute_Limit) && this.Self_Attribute_Limit != "0")
            {
                string[] limitItems = this.Self_Attribute_Limit.Split('|');
                for (int i = 0; i < limitItems.Length; i++)
                {
                    // 表格式：4~14~1（比较类型~属性ID~值），多条用 |
                    if (!TryParseAttributeLimit(limitItems[i], out int compareType, out int numericType, out long value))
                    {
                        Log.Warning($"LDSkill[{this.Id}] Self_Attribute_Limit 解析失败: [{limitItems[i]}] codes={ToCharCodes(limitItems[i])}");
                        continue;
                    }

                    this.SelfAttributeLimits.Add(new LDSkillAttributeLimit
                    {
                        CompareType = compareType,
                        NumericType = numericType,
                        Value = value,
                    });
                }
            }
        }

        static bool TryParseAttributeLimit(string raw, out int compareType, out int numericType, out long value)
        {
            compareType = 0;
            numericType = 0;
            value = 0;
            if (string.IsNullOrEmpty(raw))
            {
                return false;
            }

            string[] parts = raw.Split(TildeSeparators, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 3
                && int.TryParse(parts[0], out compareType)
                && int.TryParse(parts[1], out numericType)
                && long.TryParse(parts[2], out value))
            {
                return true;
            }

            return TryReadThreeNumbers(raw, out compareType, out numericType, out value);
        }

        static bool TryReadThreeNumbers(string raw, out int a, out int b, out long c)
        {
            a = 0;
            b = 0;
            c = 0;
            int index = 0;
            if (!TryReadNumber(raw, ref index, out long n1))
            {
                return false;
            }

            SkipNonNumber(raw, ref index);
            if (!TryReadNumber(raw, ref index, out long n2))
            {
                return false;
            }

            SkipNonNumber(raw, ref index);
            if (!TryReadNumber(raw, ref index, out long n3))
            {
                return false;
            }

            a = (int)n1;
            b = (int)n2;
            c = n3;
            return true;
        }

        static bool TryReadNumber(string raw, ref int index, out long number)
        {
            number = 0;
            while (index < raw.Length && char.IsWhiteSpace(raw[index]))
            {
                index++;
            }

            if (index >= raw.Length)
            {
                return false;
            }

            int start = index;
            if (raw[index] == '-' || raw[index] == '+')
            {
                index++;
            }

            int digitStart = index;
            while (index < raw.Length && char.IsDigit(raw[index]))
            {
                index++;
            }

            if (index == digitStart)
            {
                return false;
            }

            return long.TryParse(raw.Substring(start, index - start), out number);
        }

        static void SkipNonNumber(string raw, ref int index)
        {
            while (index < raw.Length)
            {
                char ch = raw[index];
                if (ch == '-' || ch == '+' || char.IsDigit(ch))
                {
                    break;
                }

                index++;
            }
        }

        static string ToCharCodes(string raw)
        {
            if (string.IsNullOrEmpty(raw))
            {
                return string.Empty;
            }

            StringBuilder sb = new StringBuilder(raw.Length * 6);
            for (int i = 0; i < raw.Length; i++)
            {
                if (i > 0)
                {
                    sb.Append(' ');
                }

                sb.Append("U+").Append(((int)raw[i]).ToString("X4"));
            }

            return sb.ToString();
        }
    }

    public partial class LDSkillCategory
    {
        public override void AfterEndInit()
        {
            foreach (LDSkill skillconfig in this.GetAll().Values)
            {
                skillconfig.ParseRuntimeData();
            }
        }
    }
}

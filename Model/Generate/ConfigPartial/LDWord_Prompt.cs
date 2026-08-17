using System;
using System.Collections.Generic;

namespace ET
{
    public partial class LDWord_PromptCategory
    {
        private static Dictionary<string, int> Word_PromptByKey = new Dictionary<string, int>(StringComparer.Ordinal);

        public override void AfterEndInit()
        {
            Word_PromptByKey.Clear();
            foreach (LDWord_Prompt item in this.GetAll().Values)
            {
                if (string.IsNullOrEmpty(item.Key))
                {
                    continue;
                }

                if (Word_PromptByKey.ContainsKey(item.Key))
                {
                    Log.Error($"LDWord_Prompt Key 重复: {item.Key}, id={item.Id}");
                    continue;
                }

                Word_PromptByKey.Add(item.Key, item.Id);
            }
        }

        /// <summary>通过 Key 取 Word_Prompt.Id；找不到返回 0。</summary>
        public int GetWord_PromptByKey(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return 0;
            }

            Word_PromptByKey.TryGetValue(key, out int id);
            return id;
        }

        /// <summary>通过 WordPromptKey 取 Word_Prompt.Id，用作错误码 / 飘字 Id。</summary>
        public int GetWordId(string key)
        {
            int id = this.GetWord_PromptByKey(key);
            if (id <= 0)
            {
                throw new Exception($"LDWord_Prompt 找不到 Key: {key}");
            }

            return id;
        }

        public bool TryGet(int id, out LDWord_Prompt word)
        {
            return this.dict.TryGetValue(id, out word) && word != null;
        }
    }

    public partial class LDWord_Prompt
    {
        /// <summary>按语言取提示文案；lang=0 中文，其它英文。EN 未配时回退 CN。</summary>
        public string GetShowText(int lang = 0)
        {
            if (lang != 0 && !string.IsNullOrEmpty(this.CN))
            {
                return this.CN;
            }

            return this.CN ?? string.Empty;
        }
    }
}

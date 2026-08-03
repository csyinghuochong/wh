using System.Collections.Generic;

namespace ET
{
    public partial class LDMailCategory
    {
        private static Dictionary<string, int> MailByKey = new Dictionary<string, int>();

        public override void AfterEndInit()
        {
            MailByKey.Clear();
            foreach (LDMail item in this.GetAll().Values)
            {
                if (string.IsNullOrEmpty(item.Key))
                {
                    continue;
                }

                if (MailByKey.ContainsKey(item.Key))
                {
                    Log.Error($"LDMail Key 重复: {item.Key}, id={item.Id}");
                    continue;
                }

                MailByKey.Add(item.Key, item.Id);
            }
        }

        /// <summary>通过 Key 取邮件配置 Id；找不到返回 0。</summary>
        public int GetMailByKey(string key)
        {
            MailByKey.TryGetValue(key, out int id);
            return id;
        }
    }
}

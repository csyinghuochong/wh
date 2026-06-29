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

        public int SkillId => this.Handler?.LdSkillConf?.Id ?? 0;
        public int SkillLevel => 1;

        /// <summary>
        /// Resolve editor param placeholders (e.g. skillid, level, rs).
        /// </summary>
        public string ResolveParam(string raw)
        {
            if (string.IsNullOrEmpty(raw))
            {
                return raw;
            }

            string trimmed = raw.Trim();
            if (trimmed.EndsWith("skillid", System.StringComparison.OrdinalIgnoreCase))
            {
                return this.SkillId.ToString();
            }

            if (trimmed.EndsWith("level", System.StringComparison.OrdinalIgnoreCase))
            {
                return this.SkillLevel.ToString();
            }

            if (trimmed.EndsWith("rs", System.StringComparison.OrdinalIgnoreCase))
            {
                return "0";
            }

            return raw;
        }
    }
}

using System.Collections.Generic;

namespace ET
{
    /// <summary>
    /// Runtime registry of skill logic trees parsed from Config/SkillEditor/TreeSave.xml.
    /// Populated at server startup by SkillEditorTreeLoader (Hotfix).
    /// </summary>
    public static class SkillEditorTreeRegistry
    {
        public static readonly Dictionary<int, SkillEditorSkillLogic> SkillTrees = new Dictionary<int, SkillEditorSkillLogic>();

        public static bool IsLoaded { get; private set; }

        public static void Reset()
        {
            SkillTrees.Clear();
            IsLoaded = false;
        }

        public static void SetLoaded(bool loaded)
        {
            IsLoaded = loaded;
        }

        public static bool TryGetTree(int skillId, out SkillEditorSkillLogic logic)
        {
            return SkillTrees.TryGetValue(skillId, out logic);
        }

        public static bool HasTree(int skillId)
        {
            return SkillTrees.ContainsKey(skillId);
        }
    }
}

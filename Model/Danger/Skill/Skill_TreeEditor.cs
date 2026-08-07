using System.Collections.Generic;
using UnityEngine;

namespace ET
{
    /// <summary>
    /// Skill handler that runs logic from SkillEditor TreeSave.xml.
    /// </summary>
    public class Skill_TreeEditor : Entity
    {
        public bool treeLogicExecuted;

        public List<long> HurtIds = new List<long>();

        public SkillState SkillState;

        public LDSkill LdSkillConf;

        public long SkillBeginTime;
        public long SkillEndTime;

        /// <summary>下次可执行树逻辑/跳伤的时间戳（毫秒）。</summary>
        public long SkillExcuteHurtTime;

        /// <summary>引导跳伤间隔（毫秒）；&lt;=0 表示非周期引导。</summary>
        public long GuideIntervalMs;

        public Vector3 ActionPosition;

        public Unit TheUnitFrom;

        public Unit TheUnitTarget;

        public Shape ICheckShape;

        public SkillInfo SkillInfo;
    }
}

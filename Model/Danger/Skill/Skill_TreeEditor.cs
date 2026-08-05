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

    
        public long SkillExcuteHurtTime;


        public Vector3 ActionPosition;



        public Unit TheUnitFrom;

        public Unit TheUnitTarget;

        public Shape ICheckShape;

        public SkillInfo SkillInfo;

     

    }
}

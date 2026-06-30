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

        //1 正在执行   2完成使命
        public SkillState SkillState;

        public LDSkill LdSkillConf;


        public long SkillBeginTime;
        public long SkillEndTime;

        /// <summary>
        /// 记录是否触发过技能伤害
        /// </summary>
        public long SkillExcuteHurtTime;
        public long SkillFirstHurtTime;

        /// <summary>
        /// 持续伤害
        /// </summary>
        public long DamgeChiXuLastTime;

        public int SkillExcuteNum;

        public Vector3 NowPosition;             //当前技能的坐标点
        public Vector3 TargetPosition;

        /// <summary>
        /// 伤害增加系数
        /// </summary>
        public float HurtAddPro = 0f;

        /// <summary>
        /// 来自哪个Unit
        /// </summary>
        public Unit TheUnitFrom;

        public Unit TheUnitTarget;

        public List<Shape> ICheckShape;

        public SkillInfo SkillInfo;

     

    }
}

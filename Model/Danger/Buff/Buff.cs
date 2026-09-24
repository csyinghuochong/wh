
using System.Collections.Generic;
using UnityEngine;

namespace ET
{

    public  class Buff : Entity, IAwake
    {
        /// <summary>
        /// Buff当前状态
        /// </summary>
        public BuffState BuffState;

        /// <summary>
        /// 最多持续到什么时候
        /// </summary>
        public long BuffEndTime;

        /// <summary>
        /// Buff数据
        /// </summary>
        public BuffData BuffData;

        public LDSkill_Battle_Buff MBuff;

        /// <summary>
        /// 来自哪个Unit
        /// </summary>
        public Unit TheUnitFrom;

        /// <summary>寄居 Unit：Buff → BuffManager → Unit。不存字段。</summary>
        public Unit TheUnitBelongto => this.GetParent<BuffManagerComponent>()?.GetParent<Unit>();

        public bool IsTrigger;
        public long BeginTime;
        public long PassTime;

        public Vector3 TargetPosition;

        public long InterValTime;
        public long InterValTimeBegin;

        /// <summary>OnUpdate 判定到期后为 true，OnFinished 发 Skill_TimeEnd。</summary>
        public bool IsTimeEnd;

        /// <summary>被驱散/顶掉/技能移除。切场景、到期、死亡不加。到期另看 IsTimeEnd，同样放消失技能。</summary>
        public bool IsInterrupt;
    }
}

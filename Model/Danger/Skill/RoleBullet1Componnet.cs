using System.Collections.Generic;

namespace ET
{
    public class RoleBullet1Componnet :  Entity, IAwake, IDestroy
    {
        public long PassTime;
        public long BuffEndTime;
        public long BeginTime;
        public long DelayTime;
        public float DamageRange;
        public long Masterid;
        public Skill_TreeEditor SkillHandler;

        public BuffState BuffState;
        public long Timer;
        public long DamgeChiXuLastTime;
    }

}

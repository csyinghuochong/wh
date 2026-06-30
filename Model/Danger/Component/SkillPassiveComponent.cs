using System.Collections.Generic;

namespace ET
{
    public class SkillPassiveInfo
    {
        public int SkillId;
        public  int SkillPassiveTypeEnum;

        public SkillPassiveInfo(int skillId, int skillPassiveTypeEnum)
        {
            this.SkillId = skillId;
            this.SkillPassiveTypeEnum = skillPassiveTypeEnum;
        }

        public void Reset()
        {

        }
    }

    public class SkillPassiveComponent : Entity, IAwake, IDestroy, ITransfer
    {
        public long Timer;
        public int UnitType;
        public long SingTimer;
        public int HuixueTimeNum;               //回血触发计时器,几秒触发
        public List<SkillPassiveInfo> SkillPassiveInfos = new List<SkillPassiveInfo>();
        public C2M_SkillCmd C2M_SkillCmd = new C2M_SkillCmd();
        public StateComponent StateComponent;
        public NumericComponent NumericComponent;

        public SkillPassiveInfo SingSkillIfo;
        public long SingTargetId = 0;

        //public long LastAckGaiLv_1Time = 0;
    }
}

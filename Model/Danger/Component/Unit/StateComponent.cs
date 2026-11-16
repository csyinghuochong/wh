
namespace ET
{
    public class StateComponent : Entity, IAwake, ITransfer, IDeserialize
    {
        //当前携带状态标志
        public long CurrentStateType;

        public long RigidityEndTime;

        // 本次硬直对应的实际技能，用于读取 Limit_Move / Limit_Rotate。
        public int RigiditySkillId;
    }

}

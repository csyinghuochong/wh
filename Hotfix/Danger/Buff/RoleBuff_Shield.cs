using UnityEngine;

namespace ET
{
    public class RoleBuff_Shield : BuffHandler
    {
        public override void OnInit(BuffData buffData, Unit theUnitFrom, Unit theUnitBelongto, Skill_TreeEditor skillHandler = null)
        {
            this.OnBaseBuffInit(buffData,  theUnitFrom,  theUnitBelongto);

            NumericComponent numericComponent = this.TheUnitBelongto.GetComponent<NumericComponent>();
            int maxHp = numericComponent.GetAsInt(NumericType.HP_Max);
            //1百分比 2固定伤害
            int totalValue = 0;
            if (this.MBuff.buffParameterType == 1)
            {
                numericComponent.ApplyValue(NumericType.Now_Shield_HP, (int)this.MBuff.buffParameterValue * theUnitFrom.GetComponent<NumericComponent>().GetAsLong(NumericType.Numeric_Error), true);
                totalValue = (int)(maxHp * 1f * this.MBuff.buffParameterValue);
            }
            else
            {
                totalValue = (int)this.MBuff.buffParameterValue;
            }
            numericComponent.ApplyValue(NumericType.Now_Shield_HP, totalValue, true);
            numericComponent.ApplyValue(NumericType.Now_Shield_MaxHP, totalValue, true);
            numericComponent.Set(NumericType.Now_Shield_DamgeCostPro, this.MBuff.DamgePro, false);
        }

        public override void OnUpdate()
        {
            NumericComponent numericComponent = this.TheUnitBelongto.GetComponent<NumericComponent>();

            if (numericComponent.GetAsLong(NumericType.Now_Shield_HP) <= 0)
            {
                this.BuffState = BuffState.Finished;
                int skillId = 0;
                if (!string.IsNullOrEmpty(this.MBuff.buffParameterValue2))
                {
                    skillId = int.Parse(this.MBuff.buffParameterValue2);
                }
                if (skillId > 0)
                {
                    C2M_SkillCmd cmd = new C2M_SkillCmd();
                    cmd.SkillID = skillId;
                    cmd.TargetID =0;
                    cmd.TargetAngle = (int)Quaternion.QuaternionToEuler(this.TheUnitBelongto.Rotation).y;
                    cmd.TargetDistance = 0;
                    this.TheUnitBelongto.GetComponent<SkillManagerComponent>().OnUseSkill(cmd, true);
                }
                return;
            }
            if (TimeHelper.ServerNow() > this.BuffEndTime)
            {
                this.BuffState = BuffState.Finished;
            }
        }

        public override void OnFinished()
        {
            NumericComponent numericComponent = this.TheUnitBelongto.GetComponent<NumericComponent>();
            numericComponent.ApplyValue(NumericType.Now_Shield_HP, 0);
        }
    }
}

namespace ET
{

#if SERVER
    [ObjectSystem]
    public class StateComponentDeserializeSystem : DeserializeSystem<StateComponent>
    {
        public override void Deserialize(StateComponent self)
        {
            self.CurrentStateType = StateTypeEnum.None;
            self.RigidityEndTime = 0;
        }
    }
#endif

    [ObjectSystem]
    public class StateComponentAwakeSystem : AwakeSystem<StateComponent>
    {
        public override void Awake(StateComponent self)
        {
            self.Awake();
        }
    }

    public static class StateComponentSystem
    {
        public static void Awake(this StateComponent self)
        {
            self.CurrentStateType = StateTypeEnum.None;
            self.RigidityEndTime = 0;
        }

        public static void SetRigidityEndTime(this StateComponent self, long addTime)
        {
            self.RigidityEndTime =  addTime;
        }

        public static bool IsRigidity(this StateComponent self)
        {
            return  TimeHelper.ClientNow() <  self.RigidityEndTime;
        }

        public static int CanUseSkill(this StateComponent self, LDSkill_Battle ldSkill, bool checkDead)
        {
           
            bool stunSkill = ldSkill != null && ldSkill.Use_Stun == 1 && self.StateTypeGet(StateTypeEnum.HunMi);
            if (self.StateTypeGet(StateTypeEnum.HunMi) && (ldSkill == null || ldSkill.Use_Stun != 1))
            {
                return ErrorCode.ERR_CanNotUseSkill_Dizziness;
            }
            if (self.StateTypeGet(StateTypeEnum.MaBi))
            {
                return ErrorCode.ERR_CanNotUseSkill_Dizziness;
            }
            if (self.StateTypeGet(StateTypeEnum.PassiveMove) )
            {
                return ErrorCode.ERR_CanNotUseSkill_JiTui;
            }
            if (self.StateTypeGet(StateTypeEnum.Sleep))
            {
                return ErrorCode.ERR_CanNotUseSkill_Sleep;
            }

            if (self.StateTypeGet(StateTypeEnum.ForbidCast) && (ldSkill == null || ldSkill.Use_Silence != 1) && !stunSkill)
            {
                return ErrorCode.ERR_CanNotUseSkill_Silence;
            }
            if (self.StateTypeGet(StateTypeEnum.Silence) && (ldSkill == null || ldSkill.Use_Silence != 1) && !stunSkill)
            {
                return ErrorCode.ERR_CanNotUseSkill_Silence;
            }

            Unit unit = self.GetParent<Unit>();
            if (checkDead && unit.GetComponent<NumericComponent>().GetAsInt(NumericType.Now_Dead) == 1)
            {
                return ErrorCode.ERR_CanNotSkillDead;
            }
            if (unit.Type == UnitType.Monster && unit.IsSinging())
            {
                return ErrorCode.ERR_CanNotMove_Singing;
            }
            return ErrorCode.ERR_Success;
        }

        public static int ServerCanMove(this StateComponent self)
        {
            int canMove = self.CanMove();
            if (canMove == ErrorCode.ERR_Success)
            {
                return canMove;
            }
            if (self.StateTypeGet(StateTypeEnum.PassiveMove))
            {
                return ErrorCode.ERR_Success;
            }
            return canMove; 
        }

        public static int CanMove(this StateComponent self)
        {
            if (self.StateTypeGet(StateTypeEnum.PassiveMove) || 
               self.StateTypeGet(StateTypeEnum.ForbidMove))
            {
                return ErrorCode.ERR_CanNotMove_1;
            }
            if (self.IsRigidity())
            {
                return ErrorCode.ERR_CanNotMove_Rigidity;
            }
            if (self.StateTypeGet(StateTypeEnum.HunMi))
            {
                return ErrorCode.ERR_CanNotMove_Dizziness;
            }
            if (self.StateTypeGet(StateTypeEnum.MaBi))
            {
                return ErrorCode.ERR_CanNotMove_Dizziness;
            }
            if (self.StateTypeGet(StateTypeEnum.Sleep))
            {
                return ErrorCode.ERR_CanNotMove_Sleep;
            }
            if (self.StateTypeGet(StateTypeEnum.Fear))
            {
                return ErrorCode.ERR_CanNotMove_Fear;
            }
            Unit unit = self.GetParent<Unit>();
            if (unit.Type == UnitType.Monster && unit.IsSinging())
            {
                return ErrorCode.ERR_CanNotMove_Singing;
            }

            NumericComponent numericComponent = unit.GetComponent<NumericComponent>();
            if (unit.GetSpeedNow() <= 0)
            {
                return ErrorCode.ERR_CanNotMove_Speed;
            }
            if (numericComponent.GetAsInt(NumericType.Now_Dead) == 1)
            {
                return ErrorCode.ERR_CanNotMove_Dead;
            }
            return ErrorCode.ERR_Success;
        }

        /// <summary>
        /// 增加某个状态
        /// </summary>
        /// <param name="nowStateType"></param>
        public static void StateTypeAdd(this StateComponent self, long nowStateType)
        {
            Unit unit = self.GetParent<Unit>();
            self.CurrentStateType = self.CurrentStateType | nowStateType;

            //眩晕状态停止当前移动(服务器代码)
            if ( ErrorCode.ERR_Success!=self.CanMove())
            {
                unit.Stop(0);        //停止当前移动
            }

            unit.GetComponent<SkillManagerComponent>()?.InterruptSing(0, true);
            unit.GetComponent<SkillPassiveComponent>().StateTypeAdd(nowStateType);
        }

        /// <summary>
        /// 移除某个状态。Buff 加的状态不广播，客户端/服务器各自处理。吟唱走 C2M/M2C_SingingUpdate。
        /// </summary>
        /// <param name="nowStateType"></param>
        public static void StateTypeRemove(this StateComponent self, long nowStateType)
        {
            self.CurrentStateType = self.CurrentStateType & ~nowStateType;
        }

        /// <summary>
        /// 获取某个状态是否存在
        /// </summary>
        /// <param name="nowStateType"></param>
        public static bool StateTypeGet(this StateComponent self, long nowStateType)
        {
            return (self.CurrentStateType & nowStateType) > 0;
        }
    }
}

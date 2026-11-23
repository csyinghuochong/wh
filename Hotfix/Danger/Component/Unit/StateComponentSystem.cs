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
            self.RigiditySkillId = 0;
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
            self.RigiditySkillId = 0;
        }

        /// <summary>记录硬直结束的本地时间戳（毫秒）和实际技能 ID；只记录状态，动作及操作权限由调用方检查。</summary>
        public static void SetRigidityEndTime(this StateComponent self, long addTime, int skillId = 0)
        {
            self.RigidityEndTime = addTime;
            self.RigiditySkillId = skillId;
        }

        /// <summary>本地时间尚未到达硬直结束时间则为 true；到达结束时刻即解除，无需另设计时器清零。</summary>
        public static bool IsRigidity(this StateComponent self)
        {
            return  TimeHelper.ClientNow() <  self.RigidityEndTime;
        }

        /// <summary>
        /// 转向与移动独立：只在硬直内读取 Limit_Rotate，不读取 Limit_Move；未指定硬直技能时也禁止转向。
        /// 硬直结束后不再受该技能的 Limit_Rotate 限制，但眩晕、麻痹、睡眠、被动位移及死亡仍禁止转向。
        /// </summary>
        public static bool CanRotate(this StateComponent self)
        {
            if (self.IsRigidity() && (self.RigiditySkillId == 0
                    || LDSkill_BattleCategory.Instance.Get(self.RigiditySkillId).Limit_Rotate == 1))
            {
                return false;
            }
            return !self.StateTypeGet(StateTypeEnum.HunMi | StateTypeEnum.MaBi | StateTypeEnum.Sleep | StateTypeEnum.PassiveMove)
                    && self.GetParent<Unit>().GetComponent<NumericComponent>().GetAsInt(NumericType.Now_Dead) == 0;
        }

        public static int CanUseSkill(this StateComponent self, LDSkill_Battle ldSkill, bool checkDead)
        {
           
            // Time_2 内不能再次主动施法；即使 Limit_Move/Limit_Rotate 都为 0，也不能用新技能替换当前动作。
            if (self.IsRigidity())
            {
                return ErrorCode.ERR_CanNotUseSkill_Rigidity;
            }

            bool stunSkill = ldSkill != null && ldSkill.Use_Stun == 1 && self.StateTypeGet(StateTypeEnum.HunMi);
            if (self.StateTypeGet(StateTypeEnum.HunMi) && (ldSkill == null || ldSkill.Use_Stun != 1))
            {
                return LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_Operation_Fail);
            }
            if (self.StateTypeGet(StateTypeEnum.MaBi))
            {
                return LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_Operation_Fail);
            }
            if (self.StateTypeGet(StateTypeEnum.PassiveMove) )
            {
                return LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_Operation_Fail);
            }
            if (self.StateTypeGet(StateTypeEnum.Sleep))
            {
                return LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_Operation_Fail);
            }

            if (self.StateTypeGet(StateTypeEnum.ForbidCast) && (ldSkill == null || ldSkill.Use_Silence != 1) && !stunSkill)
            {
                return LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_Operation_Fail);
            }
            if (self.StateTypeGet(StateTypeEnum.Silence) && (ldSkill == null || ldSkill.Use_Silence != 1) && !stunSkill)
            {
                return LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_Operation_Fail);
            }

            Unit unit = self.GetParent<Unit>();
            if (checkDead && unit.GetComponent<NumericComponent>().GetAsInt(NumericType.Now_Dead) == 1)
            {
                return LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_Operation_Fail);
            }
            if (unit.Type == UnitType.Monster && unit.IsSinging())
            {
                return LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_Operation_Fail);
            }
            return ErrorCode.ERR_Success;
        }

        public static int ServerCanMove(this StateComponent self, bool isSkillMove = false)
        {
            int canMove = self.CanMove(isSkillMove);
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

        // 技能自身位移只跳过硬直的移动限制，保留其它控制、速度和死亡校验。
        public static int CanMove(this StateComponent self, bool isSkillMove = false)
        {
            if (self.StateTypeGet(StateTypeEnum.PassiveMove) || 
               self.StateTypeGet(StateTypeEnum.ForbidMove))
            {
                return LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_Operation_Fail);
            }
            // 只在硬直内应用 Limit_Move；Limit_Move=0 可移动，不代表可以转向（另查 Limit_Rotate）。
            // RigiditySkillId=0 表示未指定技能的硬直，仍禁止移动。
            // 技能脚本自身位移（如 1004 冲锋）跳过此项，避免被自身硬直拦住。
            if (!isSkillMove && self.IsRigidity() && (self.RigiditySkillId == 0
                    || LDSkill_BattleCategory.Instance.Get(self.RigiditySkillId).Limit_Move == 1))
            {
                return ErrorCode.ERR_CanNotMove_Rigidity;
            }
            if (self.StateTypeGet(StateTypeEnum.HunMi))
            {
                return LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_Operation_Fail);
            }
            if (self.StateTypeGet(StateTypeEnum.MaBi))
            {
                return LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_Operation_Fail);
            }
            if (self.StateTypeGet(StateTypeEnum.Sleep))
            {
                return LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_Operation_Fail);
            }
            if (self.StateTypeGet(StateTypeEnum.Fear))
            {
                return LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_Operation_Fail);
            }
            Unit unit = self.GetParent<Unit>();
            if (unit.Type == UnitType.Monster && unit.IsSinging())
            {
                return LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_Operation_Fail);
            }

            NumericComponent numericComponent = unit.GetComponent<NumericComponent>();
            if (unit.GetSpeedNow() <= 0)
            {
                return LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_Operation_Fail);
            }
            if (numericComponent.GetAsInt(NumericType.Now_Dead) == 1)
            {
                return LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_Operation_Fail);
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

            SkillManagerComponent skillManager = unit.GetComponent<SkillManagerComponent>();
            skillManager?.InterruptPendingSing();
            if (StateTypeEnum.IsHardControl(nowStateType))
            {
                skillManager?.InterruptSkillsBeforeTime1();
            }

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

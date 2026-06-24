using ET;

namespace ET
{

    //闪现2(怪物技能)
    public class Skill_ShanXian_2 : SkillHandler
    {

        public override void OnInit(SkillInfo skillId, Unit theUnitFrom)
        {
            this.BaseOnInit(skillId, theUnitFrom);
        }

        public override void OnExecute()
        {
            //先跳过去再触发伤害
            this.SyncPostion();
            this.InitSelfBuff();

            this.OnUpdate();
        }

        public void SyncPostion()
        {
            if (this.TheUnitFrom.GetComponent<StateComponent>().CanMove() == ErrorCode.ERR_Success)
            {
                TheUnitFrom.Position = this.TargetPosition;
                TheUnitFrom.Stop(-3);
            }
            else
            {
                Log.Debug($"FsmStateEnum.ShanXian被定[S] {TargetPosition}");
            }
        }

        public override void OnUpdate()
        {
            long serverNow = TimeHelper.ServerNow();
            //根据技能效果延迟触发伤害
            if (serverNow < this.SkillExcuteHurtTime)
            {
                return;
            }

            this.BaseOnUpdate();
           /* if (this.LdSkillConf.GameObjectParameter == "1" && serverNow > this.SkillExcuteHurtTime)
            {
               
            }*/
            this.SyncPostion();
            this.SetSkillState(SkillState.Finished);
        }

        public override void OnFinished()
        {
            this.Clear();
        }
    }
}

using System;
using UnityEngine;

namespace ET
{
    public class RoleBuff_JiFei : BuffHandler
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="buffData"></param>
        /// <param name="theUnitFrom">buff持有者</param>
        /// <param name="theUnitBelongto">施法者</param>
        /// <param name="skillHandler"></param>
        public override void OnInit(BuffData buffData, Unit theUnitFrom, Unit theUnitBelongto, Skill_TreeEditor skillHandler = null)
        {
            this.OnBaseBuffInit(buffData, theUnitFrom, theUnitBelongto);
            //float speed = -1f;// (float)this.MBuff.buffParameterValue;
            float distance = -1f;// (this.MBuff.buffParameterType * speed) * 0.001f;
            Vector3 dir = (theUnitBelongto.Position - theUnitFrom.Position).normalized;
            Vector3 vector3 = theUnitBelongto.Position + dir * distance;
            StateComponent stateComponent = theUnitBelongto.GetComponent<StateComponent>();
            stateComponent.StateTypeAdd(StateTypeEnum.JiTui);
            this.BeginTime = TimeHelper.ServerNow();
            this.StartPosition = theUnitBelongto.Position;
            this.TargetPosition = vector3;
            this.TheUnitBelongto.Position = this.TargetPosition;
        }

        public override void OnUpdate()
        {
            if (TimeHelper.ServerNow() >= this.BuffEndTime)
            {
                this.BuffState = BuffState.Finished;
                this.TheUnitBelongto.Position = this.StartPosition;
                Log.Console($"stop: {this.TheUnitBelongto.Position.x} {this.TheUnitBelongto.Position.z}");
                this.TheUnitBelongto.Stop(-2);
            }
        }

        public override void OnFinished()
        {
            StateComponent stateComponent = this.TheUnitBelongto.GetComponent<StateComponent>();
            stateComponent.StateTypeRemove(StateTypeEnum.JiTui);
        }

    }
}

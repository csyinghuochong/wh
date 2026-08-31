using System;
using UnityEngine;

namespace ET
{

    /// <summary>
    /// 需要寻路
    /// </summary>
    public class RoleBuff_JiTui : BuffHandler
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
            int buff_time = -1;// this.MBuff.BuffTime;
            float oldSpeed = theUnitFrom.GetSpeedNow();
            float newSpeed = 0f;// (float)this.MBuff.buffParameterValue;
            float distance = (buff_time * newSpeed) * 0.001f;
            Vector3 dir = (theUnitBelongto.Position - theUnitFrom.Position).normalized;
            if (theUnitBelongto.Id == theUnitFrom.Id)
            {
                dir = theUnitBelongto.Rotation * Vector3.back;
            }
            Vector3 vector3 = theUnitBelongto.Position + dir * distance;
            this.BeginTime = TimeHelper.ServerNow();
            this.StartPosition = theUnitBelongto.Position;
            MapComponent map = theUnitBelongto.DomainScene().GetComponent<MapComponent>();
            this.TargetPosition = map.GetCanChongJiPath(theUnitBelongto,theUnitBelongto.Position, vector3);

            theUnitBelongto.Stop(-2);
            StateComponent stateComponent = theUnitBelongto.GetComponent<StateComponent>();
            stateComponent.StateTypeAdd(StateTypeEnum.PassiveMove);
            theUnitBelongto.FindPathMoveToAsync(this.TargetPosition, null, false, Math.Max(100, (int)(newSpeed * 100f / oldSpeed))).Coroutine();
        }

        public override void OnUpdate()
        {
            if (TimeHelper.ServerNow() >= this.BuffEndTime)
            {
                this.BuffState = BuffState.Finished;
            }
        }

        public override void OnFinished()
        {
            StateComponent stateComponent = this.TheUnitBelongto.GetComponent<StateComponent>();
            stateComponent.StateTypeRemove(StateTypeEnum.PassiveMove);
        }

    }
}

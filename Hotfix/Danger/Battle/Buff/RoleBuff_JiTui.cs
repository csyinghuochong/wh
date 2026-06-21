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
        public override void OnInit(BuffData buffData, Unit theUnitFrom, Unit theUnitBelongto, SkillHandler skillHandler = null)
        {
            this.OnBaseBuffInit(buffData, theUnitFrom, theUnitBelongto);
            int buff_time = this.MBuff.BuffTime;
            float oldSpeed = theUnitFrom.GetSpeedNow();
            float newSpeed = (float)this.MBuff.buffParameterValue;
            float distance = (buff_time * newSpeed) * 0.001f;
            Vector3 dir = (theUnitBelongto.Position - theUnitFrom.Position).normalized;
            if (theUnitBelongto.Id == theUnitFrom.Id)
            {
                dir = theUnitBelongto.Rotation * Vector3.back;
            }
            Vector3 vector3 = theUnitBelongto.Position + dir * distance;
            this.BeginTime = TimeHelper.ServerNow();
            this.StartPosition = theUnitBelongto.Position;
            this.TargetPosition = theUnitBelongto.DomainScene().GetComponent<MapComponent>().GetCanChongJiPath(theUnitBelongto,theUnitBelongto.Position, vector3);

            theUnitBelongto.Stop(-2);
            theUnitBelongto.GetComponent<StateComponent>().StateTypeAdd(StateTypeEnum.JiTui);
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
            this.TheUnitBelongto.GetComponent<StateComponent>().StateTypeRemove(StateTypeEnum.JiTui);
        }

    }
}

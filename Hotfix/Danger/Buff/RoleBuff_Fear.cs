using System;
using UnityEngine;

namespace ET
{

    /// <summary>
    /// 恐惧BUFF. 随机移动
    /// </summary>
    public class RoleBuff_Fear : BuffHandler
    {
        public override void OnInit(BuffData buffData, Unit theUnitFrom, Unit theUnitBelongto, Skill_TreeEditor skillHandler = null)
        {
            this.OnBaseBuffInit(buffData, theUnitFrom, theUnitBelongto);
            this.TheUnitBelongto.GetComponent<StateComponent>().StateTypeAdd(StateTypeEnum.Fear);
            
            this.TargetPosition.x = this.TheUnitBelongto.Position.x + RandomHelper.RandomNumberFloat(-10, 10);
            this.TargetPosition.y = this.TheUnitBelongto.Position.y;
            this.TargetPosition.z = this.TheUnitBelongto.Position.z + RandomHelper.RandomNumberFloat(-10, 10);
            this.TargetPosition = this.TheUnitBelongto.DomainScene().GetComponent<MapComponent>()
                    .GetCanChongJiPath(theUnitBelongto, this.TheUnitBelongto.Position, TargetPosition);
            this.TheUnitBelongto.FindPathMoveToAsync(this.TargetPosition).Coroutine();
        }

        public override void OnUpdate()
        {
            long serverNow = TimeHelper.ServerNow();
            if (serverNow > this.BuffEndTime)
            {
                this.BuffState = BuffState.Finished;
            }

            Unit unit = this.TheUnitBelongto;
            if (Vector3.Distance(this.TargetPosition, unit.Position) < 0.5f)
            {
                this.TargetPosition.x = unit.Position.x + RandomHelper.RandomNumberFloat(-8, 8);
                this.TargetPosition.y = unit.Position.y;
                this.TargetPosition.z = unit.Position.z + RandomHelper.RandomNumberFloat(-8, 8);
                MapComponent map = unit.DomainScene().GetComponent<MapComponent>();
                this.TargetPosition = map.GetCanReachPath(unit, unit.Position, TargetPosition);
                unit.FindPathMoveToAsync(this.TargetPosition).Coroutine();
            }

        }

        public override void OnFinished()
        {
            this.TheUnitBelongto.GetComponent<StateComponent>().StateTypeRemove(StateTypeEnum.Fear);
        }
    }
}

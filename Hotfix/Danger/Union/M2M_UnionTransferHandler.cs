using System;

namespace ET
{

    [ActorMessageHandler]
    public class M2M_UnionTransferHandler : AMActorLocationHandler<Unit, M2M_UnionTransferMessage>
    {
        protected override async ETTask Run(Unit unit, M2M_UnionTransferMessage message)
        {
            NumericComponent numericComponent = unit.GetComponent<NumericComponent>();
            numericComponent.ApplyValue( NumericType.UnionLeader, message.UnionLeader, true );
            // 在线接任同样立即保存会长标记，避免下线前缓存仍保留旧职位。
            int homeZone = UnitZoneHelper.GetHomeZone(unit);
            await DBHelper.SaveComponent(homeZone, unit.Id, numericComponent);
            await DBHelper.SaveComponentCache(homeZone, unit.Id, numericComponent);
        }
    }
}

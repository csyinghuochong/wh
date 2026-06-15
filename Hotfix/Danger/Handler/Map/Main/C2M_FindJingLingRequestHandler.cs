using System;
using System.Collections.Generic;

namespace ET
{

    [ActorMessageHandler]
    public class C2M_FindJingLingRequestHandler : AMActorLocationRpcHandler<Unit, C2M_FindJingLingRequest, M2C_FindJingLingResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_FindJingLingRequest request, M2C_FindJingLingResponse response, Action reply)
        {
            int jinglingid = 0;
            List<Unit> units = UnitHelper.GetUnitList( unit.DomainScene(), UnitType.Monster );
            for (int i = 0; i < units.Count; i++)
            {
                LDMonster ldMonster = LDMonsterCategory.Instance.Get(units[i].ConfigId);

                if (ldMonster.MonsterSonType == 57 || ldMonster.MonsterSonType == 58 || ldMonster.MonsterSonType == 59)
                {
                    jinglingid = ldMonster.Id;
                    break;
                }
            }

            response.MonsterID = jinglingid;    
            reply();
            await ETTask.CompletedTask;
        }
    }
}

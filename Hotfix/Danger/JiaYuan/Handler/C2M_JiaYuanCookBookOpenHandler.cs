using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ET
{

    [ActorMessageHandler]
    public class C2M_JiaYuanCookBookOpenHandler : AMActorLocationRpcHandler<Unit, C2M_JiaYuanCookBookOpen, M2C_JiaYuanCookBookOpen>
    {
        protected override async ETTask Run(Unit unit, C2M_JiaYuanCookBookOpen request, M2C_JiaYuanCookBookOpen response, Action reply)
        {
            LDItem ldItemCof = LDItemCategory.Instance.Get(request.LearnMakeId);
            long needzijin = JiaYuanHelper.GetCookBookCost(ldItemCof.UseLv_Min);
            JiaYuanComponentServer jiaYuanComponentServer = unit.GetComponent<JiaYuanComponentServer>();

            if (jiaYuanComponentServer.JiaYuanFund < needzijin)
            {
                response.Error = ErrorCode.ERR_HouBiNotEnough;
                reply();
                return;
            }

            if (jiaYuanComponentServer.LearnMakeIds_7.Contains(request.LearnMakeId))
            {
                response.Error = ErrorCode.ERR_AlreadyLearn;
                reply();
                return;
            }

            jiaYuanComponentServer.LearnMakeIds_7.Add(request.LearnMakeId);
            jiaYuanComponentServer.AddJiaYuanFund(needzijin * -1);
            DBHelper.SaveComponentCache(UnitZoneHelper.GetHomeZone(unit), unit.Id, jiaYuanComponentServer).Coroutine();

            response.LearnMakeIds = jiaYuanComponentServer.LearnMakeIds_7;
            reply();
            await ETTask.CompletedTask;
        }
    }
}

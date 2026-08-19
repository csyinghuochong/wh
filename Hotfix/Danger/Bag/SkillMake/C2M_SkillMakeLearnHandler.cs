using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ET
{

    [ActorMessageHandler]
    public class C2M_SkillMakeLearnHandler : AMActorLocationRpcHandler<Unit, C2M_SkillMakeLearnRequest, M2C_SkillMakeLearnResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_SkillMakeLearnRequest request, M2C_SkillMakeLearnResponse response, Action reply)
        {
            RoleInfo roleInfo = unit.GetComponent<RoleInfoComponentServer>().RoleInfo;
            if (request.OperatateType == 1)
            {
                if (roleInfo.MakeTypeList.Contains(request.SkillMakeType))
                {
                    return;
                }
                roleInfo.MakeTypeList.Add(request.SkillMakeType);
            }
            if (request.OperatateType == 2)
            {
                roleInfo.MakeTypeList.Remove(request.SkillMakeType);
            }
            reply();
            await ETTask.CompletedTask;
        }
    }
}

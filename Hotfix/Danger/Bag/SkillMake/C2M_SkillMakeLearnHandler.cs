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
            RoleInfoComponentServer roleInfoComponent = unit.GetComponent<RoleInfoComponentServer>();
            RoleInfo roleInfo = roleInfoComponent.RoleInfo;
            if (request.OperatateType == 1)
            {
                if (roleInfo.MakeTypeList.Contains(request.SkillMakeType))
                {
                    reply();
                    return;
                }
                roleInfo.MakeTypeList.Add(request.SkillMakeType);
                if (roleInfoComponent.AutoLearnMakeByLevel())
                {
                    MessageHelper.SendToClient(unit, new M2C_UpdateRoleInfoMessage { RoleInfo = roleInfo });
                }
                DBHelper.SaveComponentCache(UnitZoneHelper.GetHomeZone(unit), unit.Id, roleInfoComponent).Coroutine();
            }
            if (request.OperatateType == 2)
            {
                roleInfo.MakeTypeList.Remove(request.SkillMakeType);
                roleInfoComponent.RemoveMakeIdsByType(request.SkillMakeType);
                MessageHelper.SendToClient(unit, new M2C_UpdateRoleInfoMessage { RoleInfo = roleInfo });
                DBHelper.SaveComponentCache(UnitZoneHelper.GetHomeZone(unit), unit.Id, roleInfoComponent).Coroutine();
            }
            reply();
            await ETTask.CompletedTask;
        }
    }
}

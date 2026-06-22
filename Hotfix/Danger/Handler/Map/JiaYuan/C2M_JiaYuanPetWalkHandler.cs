using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ET
{

    [ActorMessageHandler]
    public class C2M_JiaYuanPetWalkHandler : AMActorLocationRpcHandler<Unit, C2M_JiaYuanPetWalkRequest, M2C_JiaYuanPetWalkResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_JiaYuanPetWalkRequest request, M2C_JiaYuanPetWalkResponse response, Action reply)
        {
            RolePetInfo rolePetInfo = unit.GetComponent<PetComponent>().GetPetInfo(request.PetId);
            if (rolePetInfo == null )
            {
                response.Error = ErrorCode.ERR_Pet_NoExist;
                reply();
                return;
            }
            if (rolePetInfo.PetStatus == 1)
            {
                response.Error = ErrorCode.ERR_Pet_Hint_3;
                response.Message = "出战宠物";
                reply();
                return;
            }

            RoleInfoComponent roleInfoComponent = unit.GetComponent<RoleInfoComponent>();

            if (request.Position == 1 &&  roleInfoComponent.RoleInfo.Lv < roleInfoComponent.RoleInfo.JiaYuanLv)
            {
                response.Error = ErrorCode.ERR_JiaYuanLevel;
                reply();
                return;
            }
            if (request.Position == 2 && roleInfoComponent.RoleInfo.Lv < roleInfoComponent.RoleInfo.JiaYuanLv)
            {
                response.Error = ErrorCode.ERR_JiaYuanLevel;
                reply();
                return;
            }

            JiaYuanPet jiaYuanPet = unit.GetComponent<JiaYuanComponent>().GetJiaYuanPet(request.PetId);
          
            unit.GetComponent<PetComponent>().OnPetWalk(request.PetId, request.PetStatus);
            unit.GetComponent<JiaYuanComponent>().OnJiaYuanPetWalk(rolePetInfo, request.PetStatus, request.Position);
            UnitComponent unitComponent = unit.GetParent<UnitComponent>();
            if (request.PetStatus == 2)
            {
                if (unitComponent.Get(request.PetId) == null)
                {
                    UnitFactory.CreateJiaYuanPet(unit.DomainScene(), unit.Id, unit.GetComponent<JiaYuanComponent>().GetJiaYuanPet(request.PetId) );
                }
            }
            if (request.PetStatus == 0)
            {
                if (unitComponent.Get(request.PetId) != null)
                {
                    unitComponent.Remove(request.PetId);
                }
                if (jiaYuanPet != null)
                {
                    unit.GetComponent<PetComponent>().PetAddExp(rolePetInfo, (int)jiaYuanPet.CurExp);
                }
            }
            DBHelper.SaveComponentCache(unit.DomainZone(), unit.Id, unit.GetComponent<JiaYuanComponent>()).Coroutine();
            response.JiaYuanPetList = unit.GetComponent<JiaYuanComponent>().JiaYuanPetList_2;
            reply();
            await ETTask.CompletedTask;
        }
    }
}

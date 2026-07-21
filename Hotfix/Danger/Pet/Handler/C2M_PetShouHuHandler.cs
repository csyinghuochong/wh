using System;
using System.Collections.Generic;

namespace ET
{

    [ActorMessageHandler]
    public class C2M_PetShouHuHandler : AMActorLocationRpcHandler<Unit, C2M_PetShouHuRequest, M2C_PetShouHuResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_PetShouHuRequest request, M2C_PetShouHuResponse response, Action reply)
        {
            PetComponentServer petComponentServer = unit.GetComponent<PetComponentServer>();
            RolePetInfo rolePetInfo = petComponentServer.GetPetInfo(request.PetInfoId);
            if (rolePetInfo == null || rolePetInfo.ShouHuPos == 0)
            {
                reply();
                return;
            }

            List<long> shouhulist = petComponentServer.PetShouHuList;
            if (PetHelper.IsShenShou(rolePetInfo.ConfigId))
            {
                shouhulist[request.Position] = request.PetInfoId;
            }
            else
            {
                shouhulist[rolePetInfo.ShouHuPos - 1] = request.PetInfoId;
            }


            response.PetShouHuList = shouhulist;
            Function_Fight.UnitUpdateProperty_Base(  unit, true, true);
            reply();
            await ETTask.CompletedTask;
        }
    }
}

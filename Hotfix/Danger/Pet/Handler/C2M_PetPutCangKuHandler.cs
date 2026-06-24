using System;


namespace ET
{
    [ActorMessageHandler]
    public class C2M_PetPutCangKuHandler : AMActorLocationRpcHandler<Unit, C2M_PetPutCangKu, M2C_PetPutCangKu>
    {
        protected override async ETTask Run(Unit unit, C2M_PetPutCangKu request, M2C_PetPutCangKu response, Action reply)
        {
            PetComponentServer petComponentServer = unit.GetComponent<PetComponentServer>();
            RolePetInfo petinfo = petComponentServer.GetPetInfo(request.PetInfoId);
            if (petinfo == null)
            {
                response.Error = ErrorCode.ERR_Pet_NoExist;
                reply();
                return;
            }
            if (request.PetStatus == 3)
            {
                int cangkupetNumber = PetHelper.GetCangKuPetNum(petComponentServer.RolePetInfos);
                if (cangkupetNumber >= petComponentServer.PetCangKuOpen.Count)
                {
                    response.Error = ErrorCode.ERR_CangKu_NotOpen;
                    reply();
                    return;
                }
            }

            petinfo.PetStatus= request.PetStatus;
            reply();
            await ETTask.CompletedTask;
        }
    }
}

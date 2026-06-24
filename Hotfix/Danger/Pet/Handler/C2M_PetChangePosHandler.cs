using System;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_PetChangePosHandler: AMActorLocationRpcHandler<Unit, C2M_PetChangePosRequest, M2C_PetChangePosResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_PetChangePosRequest request, M2C_PetChangePosResponse response, Action reply)
        {
            PetComponentServer petComponentServer = unit.GetComponent<PetComponentServer>();

            if (request.Index1 < 0 || request.Index1 >= petComponentServer.RolePetInfos.Count)
            {
                Log.Error($"C2M_PetChangePosRequest 1");
                response.Error = ErrorCode.ERR_ModifyData;
                reply();
                return;
            }

            if (request.Index2 < 0 || request.Index2 >= petComponentServer.RolePetInfos.Count)
            {
                Log.Error($"C2M_PetChangePosRequest 2");
                response.Error = ErrorCode.ERR_ModifyData;
                reply();
                return;
            }

            if (request.Index1 == request.Index2)
            {
                Log.Error($"C2M_PetChangePosRequest 3");
                response.Error = ErrorCode.ERR_ModifyData;
                reply();
                return;
            }

            (petComponentServer.RolePetInfos[request.Index1], petComponentServer.RolePetInfos[request.Index2]) =
                    (petComponentServer.RolePetInfos[request.Index2], petComponentServer.RolePetInfos[request.Index1]);
            reply();

            await ETTask.CompletedTask;
        }
    }
}
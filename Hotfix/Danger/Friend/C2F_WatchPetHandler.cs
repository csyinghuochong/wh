using System;


namespace ET
{

    [ActorMessageHandler]
    public class C2F_WatchPetHandler : AMActorRpcHandler<Scene, C2F_WatchPetRequest, F2C_WatchPetResponse>
    {
        protected override async ETTask Run(Scene scene, C2F_WatchPetRequest request, F2C_WatchPetResponse response, Action reply)
        {
            int homeZone = UnitZoneHelper.GetHomeZone(request.UnitID);
            PetComponentServer petComponentServer = await DBHelper.GetComponent<PetComponentServer>(homeZone, request.UnitID);
            if (petComponentServer == null)
            {
                response.Error = ErrorCode.ERR_Error;
                reply();
                return;
            }

            BagComponentServer bagComponentsServer = await DBHelper.GetComponent<BagComponentServer>(homeZone, request.UnitID);
            if (bagComponentsServer == null)
            {
                response.Error = ErrorCode.ERR_Error;
                reply();
                return;
            }
            response.PetInfos = petComponentServer.GetPetInfo( request.PetId );
            
            NumericComponent numericComponent = await DBHelper.GetComponent<NumericComponent>(homeZone, request.UnitID);
            if (numericComponent == null)
            {
                response.Error = ErrorCode.ERR_Error;
                reply();
                return;
            }
            foreach ((int key, long value) in numericComponent.NumericDic)
            {
                if (key >= (int)NumericType.Max)
                {
                    continue;
                }
                response.Ks.Add(key);
                response.Vs.Add(value);
            }
            
            reply();
            await ETTask.CompletedTask;
        }
    }
}

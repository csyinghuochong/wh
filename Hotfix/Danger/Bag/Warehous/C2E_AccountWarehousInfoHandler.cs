using System;

namespace ET
{

    [ActorMessageHandler]
    public class C2E_AccountWarehousInfoHandler : AMActorRpcHandler<Scene, C2E_AccountWarehousInfoRequest, E2C_AccountWarehousInfoResponse>
    {
        protected override async ETTask Run(Scene scene, C2E_AccountWarehousInfoRequest request, E2C_AccountWarehousInfoResponse response, Action reply)
        {
            using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.LoginAccount, request.AccInfoID))
            {
                DBAccountBagInfo dBAccountBagWarehouse = await DBHelper.GetComponent<DBAccountBagInfo>(scene.DomainZone(), request.AccInfoID);

                if (dBAccountBagWarehouse != null)
                {
                    response.BagInfos = dBAccountBagWarehouse.BagInfoList;
                }

                reply();
            }
            await ETTask.CompletedTask;
        }
    }
}

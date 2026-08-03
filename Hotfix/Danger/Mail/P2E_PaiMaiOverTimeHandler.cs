using System;
using System.Collections.Generic;

namespace ET
{
    [ActorMessageHandler]
    public class P2E_PaiMaiOverTimeHandler : AMActorRpcHandler<Scene, P2E_PaiMaiOverTimeRequest, E2P_PaiMaiOverTimeResponse>
    {
        protected override async ETTask Run(Scene scene, P2E_PaiMaiOverTimeRequest request, E2P_PaiMaiOverTimeResponse response, Action reply)
        {
            long dbCacheId = DBHelper.GetDbCacheId(scene.DomainZone());
          
            DBMailInfo dBMainInfo = await DBHelper.GetComponent<DBMailInfo>(scene.DomainZone(), request.PaiMaiItemInfo.UserId);
            if (dBMainInfo == null)
            {
                Log.Debug($"DBMailInfo==null {request.PaiMaiItemInfo.UserId}");
                reply();
                return;
            }

            long mailid = IdGenerater.Instance.GenerateId();
            //dBMainInfo.MailInfoList.Add(new MailInfo() { MailId = mailid, Context = "拍卖下架", Title = "拍卖下架", ItemList = new List<BagInfo>() { request.PaiMaiItemInfo.BagInfo } });

            await DBHelper.SaveComponent(scene.DomainZone(),  request.PaiMaiItemInfo.UserId, dBMainInfo);
            reply();
        }
    }
}

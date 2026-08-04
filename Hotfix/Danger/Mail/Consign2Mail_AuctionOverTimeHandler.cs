using System;
using System.Collections.Generic;

namespace ET
{
    [ActorMessageHandler]
    public class Consign2Mail_AuctionOverTimeHandler : AMActorRpcHandler<Scene, Consign2Mail_AuctionOverTimeRequest, Mail2Consign_AuctionOverTimeResponse>
    {
        protected override async ETTask Run(Scene scene, Consign2Mail_AuctionOverTimeRequest request, Mail2Consign_AuctionOverTimeResponse response, Action reply)
        {
            long dbCacheId = DBHelper.GetDbCacheId(scene.DomainZone());
          
            DBMailInfo dBMainInfo = await DBHelper.GetComponent<DBMailInfo>(scene.DomainZone(), request.ConsignItemInfo.UserId);
            if (dBMainInfo == null)
            {
                Log.Debug($"DBMailInfo==null {request.ConsignItemInfo.UserId}");
                reply();
                return;
            }

            long mailid = IdGenerater.Instance.GenerateId();
            //dBMainInfo.MailInfoList.Add(new MailInfo() { MailId = mailid, Context = "拍卖下架", Title = "拍卖下架", ItemList = new List<BagInfo>() { request.ConsignItemInfo.BagInfo } });

            //await DBHelper.SaveComponent(scene.DomainZone(),  request.ConsignItemInfo.UserId, dBMainInfo);
            reply();
        }
    }
}

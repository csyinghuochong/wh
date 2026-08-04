using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ET
{
    [ActorMessageHandler]
    public class C2Consign_AuctionRecordHandler : AMActorRpcHandler<Scene, C2Consign_AuctionRecordRequest, Consign2C_AuctionRecordResponse>
    {
        protected override async ETTask Run(Scene scene, C2Consign_AuctionRecordRequest request, Consign2C_AuctionRecordResponse response, Action reply)
        {
            response.RecordList = scene.GetComponent<ConsignSceneComponent>().AuctionRecords;

            reply();
            await ETTask.CompletedTask;
        }
    }
}

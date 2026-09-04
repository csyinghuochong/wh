using System;
using System.Collections.Generic;

namespace ET
{
    [ActorMessageHandler]
    public class C2U_UnionRaceInfoHandler : AMActorRpcHandler<Scene, C2U_UnionRaceInfoRequest, U2C_UnionRaceInfoResponse>
    {
        protected override async ETTask Run(Scene scene, C2U_UnionRaceInfoRequest request, U2C_UnionRaceInfoResponse response, Action reply)
        {
            UnionSceneComponent unionSceneComponent = scene.GetComponent<UnionSceneComponent>();
            await unionSceneComponent.LoadAllUnionInfos();

            response.TotalDonation = unionSceneComponent.GetBaseJiangJin() + (int)(unionSceneComponent.DBUnionManager.TotalDonation);

            for (int i = 0; i < unionSceneComponent.DBUnionManager.SignupUnions.Count; i++)
            {
                DBUnionInfo dBUnionInfo = await unionSceneComponent.GetDBUnionInfo(unionSceneComponent.DBUnionManager.SignupUnions[i]);
                if (dBUnionInfo?.UnionInfo == null || dBUnionInfo.UnionInfo.LeaderId == 0)
                {
                    continue;
                }

                response.UnionInfoList.Add(dBUnionInfo.ToUnionListItem());
            }

            reply();
            await ETTask.CompletedTask;
        }
    }
}

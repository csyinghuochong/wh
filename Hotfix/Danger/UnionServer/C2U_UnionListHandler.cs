using System;
using System.Collections.Generic;

namespace ET
{
    [ActorMessageHandler]
    public class C2U_UnionListHandler : AMActorRpcHandler<Scene, C2U_UnionListRequest, U2C_UnionListResponse>
    {
        protected override async ETTask Run(Scene scene, C2U_UnionListRequest request, U2C_UnionListResponse response, Action reply)
        {
            UnionSceneComponent unionScene = scene.GetComponent<UnionSceneComponent>();
            await unionScene.LoadAllUnionInfos();

            List<UnionListItem> unionList = response.UnionList;
            unionList.Clear();
            foreach (DBUnionInfo dBUnionInfo in unionScene.DBUnionInfos.Values)
            {
                if (dBUnionInfo == null || dBUnionInfo.UnionInfo == null || dBUnionInfo.UnionInfo.LeaderId == 0)
                {
                    continue;
                }

                unionList.Add(dBUnionInfo.ToUnionListItem());
            }

            reply();
        }
    }
}

using System;
using System.Collections.Generic;

namespace ET
{
    /// <summary>
    /// 查找装备所在拍卖行那一页
    /// </summary>
    [ActorMessageHandler]
    public class C2Consign_FindHandler: AMActorRpcHandler<Scene, C2Consign_FindRequest, Consign2C_FindResponse>
    {
        protected override async ETTask Run(Scene scene, C2Consign_FindRequest request, Consign2C_FindResponse response, Action reply)
        {
            ConsignSceneComponent paiMaiComponent = scene.GetComponent<ConsignSceneComponent>();
            ConsignItemInfo paiMaiItemInfo = paiMaiComponent.FindShangJiaItem(request.BelongId, request.ConsignItemInfoId, out List<ConsignItemInfo> belongList);
            if (paiMaiItemInfo == null || belongList == null)
            {
                response.Page = 0;
                reply();
                return;
            }

            int pagenum = int.Parse(LDGlobalValueCategory.Instance.Get(104).Value);
            for (int i = 0; i < belongList.Count; i++)
            {
                if (belongList[i].Id == paiMaiItemInfo.Id)
                {
                    response.Page = i / pagenum + 1;
                    reply();
                    return;
                }
            }

            response.Page = 0;
            reply();
            await ETTask.CompletedTask;
        }
    }
}

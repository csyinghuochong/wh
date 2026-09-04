using System;
using System.Collections.Generic;

namespace ET
{

    [ActorMessageHandler]
    public class M2U_UnionCreateHandler : AMActorRpcHandler<Scene, M2U_UnionCreateRequest, U2M_UnionCreateResponse>
    {
        protected override async ETTask Run(Scene scene, M2U_UnionCreateRequest request, U2M_UnionCreateResponse response, Action reply)
        {
            if (request.UnionName.Length > 10 || !StringHelper.IsSpecialChar(request.UnionName))
            {
                response.Error = ErrorCode.ERR_Union_NameValied;
                reply();
                return;
            }

            UnionSceneComponent unionSceneComponent = scene.GetComponent<UnionSceneComponent>();
            await unionSceneComponent.LoadAllUnionInfos();
            foreach (DBUnionInfo exist in unionSceneComponent.DBUnionInfos.Values)
            {
                if (exist?.UnionInfo != null && exist.UnionInfo.UnionName == request.UnionName)
                {
                    response.Error = ErrorCode.ERR_Union_Same_Name;
                    reply();
                    return;
                }
            }

            long unionId = IdGenerater.Instance.GenerateId();
            DBUnionInfo unionInfo = new DBUnionInfo();
            unionInfo.Id = unionId;
            unionInfo.UnionInfo.Level = 1;
            unionInfo.UnionInfo.UnionId = unionId;
            unionInfo.UnionInfo.LeaderId = request.UserID;       
            unionInfo.UnionInfo.UnionName = request.UnionName;
            unionInfo.UnionInfo.UnionPurpose = request.UnionPurpose;
            unionInfo.UnionInfo.UnionBanner = request.UnionBanner;
            unionInfo.UnionInfo.UnionPattern = request.UnionPattern;

            RoleInfoComponentServer roleInfoComponentServer = await DBHelper.GetComponent<RoleInfoComponentServer>(scene.DomainZone(), request.UserID);
            unionInfo.UnionInfo.LeaderName = roleInfoComponentServer.RoleInfo.Name;
            unionInfo.UnionInfo.UnionPlayerList.Add(new UnionPlayerInfo()
            {
                 PlayerLevel = roleInfoComponentServer.RoleInfo.Lv,
                 PlayerName = roleInfoComponentServer.RoleInfo.Name,
                 UserID = request.UserID,
            });
            await DBHelper.SaveComponent(scene.DomainZone(), unionId, unionInfo);
            unionSceneComponent.DBUnionInfos[unionId] = unionInfo;
            response.UnionId = unionId;
            reply();
            await ETTask.CompletedTask;
        }
    }
}

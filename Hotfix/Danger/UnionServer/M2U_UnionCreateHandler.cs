using System;
using System.Collections.Generic;

namespace ET
{

    [ActorMessageHandler]
    public class M2U_UnionCreateHandler : AMActorRpcHandler<Scene, M2U_UnionCreateRequest, U2M_UnionCreateResponse>
    {
        protected override async ETTask Run(Scene scene, M2U_UnionCreateRequest request, U2M_UnionCreateResponse response, Action reply)
        {
            Log.Warning($"M2U_UnionCreateRequest:{request.UserID}");
            if (request.UnionName.Length > 7 || !StringHelper.IsSpecialChar(request.UnionName))
            {
                response.Error = ErrorCode.ERR_Union_Same_Name;
                reply();
                return;
            }
            List<DBUnionInfo> result = await Game.Scene.GetComponent<DBComponent>().Query<DBUnionInfo>(scene.DomainZone(), _unionifo => _unionifo.UnionInfo.UnionName == request.UnionName);
            if (result.Count > 0)
            {
                response.Error = ErrorCode.ERR_Union_Same_Name;
                reply();
                return;
            }

            long dbCacheId = DBHelper.GetDbCacheId(scene.DomainZone());
            long unionId = IdGenerater.Instance.GenerateId();
            UnionSceneComponent unionSceneComponent = scene.GetComponent<UnionSceneComponent>();
            DBUnionInfo unionInfo = unionSceneComponent.AddChildWithId<DBUnionInfo>(unionId);
            unionInfo.UnionInfo.Level = 1;
            unionInfo.UnionInfo.UnionId = unionId;
            unionInfo.UnionInfo.LeaderId = request.UserID;       
            unionInfo.UnionInfo.UnionName = request.UnionName;
            unionInfo.UnionInfo.UnionPurpose = request.UnionPurpose;

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

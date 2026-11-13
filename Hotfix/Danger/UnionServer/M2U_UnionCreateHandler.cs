using System;
using System.Collections.Generic;

namespace ET
{

    [ActorMessageHandler]
    public class M2U_UnionCreateHandler : AMActorRpcHandler<Scene, M2U_UnionCreateRequest, U2M_UnionCreateResponse>
    {
        protected override async ETTask Run(Scene scene, M2U_UnionCreateRequest request, U2M_UnionCreateResponse response, Action reply)
        {
            using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.UnionCreate, scene.DomainZone()))
            {
                await Create(scene, request, response, reply);
            }
        }

        private static async ETTask Create(Scene scene, M2U_UnionCreateRequest request, U2M_UnionCreateResponse response, Action reply)
        {
            if (!UnionHelper.IsValidUnionName(request.UnionName)
                || !UnionHelper.IsValidUnionText(request.UnionPurpose, GlobalValueKey.Global_Union_Declaration_Character_Length)
                || !UnionHelper.IsValidUnionText(request.UnionNotice, GlobalValueKey.Global_Union_Notic_Character_Length))
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
            unionInfo.UnionInfo.UnionNo = unionSceneComponent.AllocUnionNo();
            unionInfo.UnionInfo.UnionNotice = request.UnionNotice ?? string.Empty;
            unionInfo.UnionInfo.UnionPurpose = request.UnionPurpose;
            unionInfo.UnionInfo.UnionBanner = request.UnionBanner;
            unionInfo.UnionInfo.UnionPattern = request.UnionPattern;

            // 仓库初始使用默认密码。
            unionInfo.WarehousePassword = UnionWarehouseHelper.DefaultWarehousePassword;
            unionInfo.UnionInfo.HasWarehousePassword = UnionWarehouseHelper.GetWarehousePasswordState(unionInfo.WarehousePassword);

            RoleInfoComponentServer roleInfoComponentServer = await DBHelper.GetComponent<RoleInfoComponentServer>(scene.DomainZone(), request.UserID);
            unionInfo.UnionInfo.LeaderName = roleInfoComponentServer.RoleInfo.Name;
            unionInfo.UnionInfo.UnionPlayerList.Add(UnionHelper.CreateUnionPlayerSnapshot(roleInfoComponentServer.RoleInfo, request.UserID, TimeHelper.ServerNow(), UnionPosition.Leader));
            await DBHelper.SaveComponent(scene.DomainZone(), unionId, unionInfo);
            unionSceneComponent.DBUnionInfos[unionId] = unionInfo;
            response.UnionId = unionId;
            reply();
            await ETTask.CompletedTask;
        }
    }
}

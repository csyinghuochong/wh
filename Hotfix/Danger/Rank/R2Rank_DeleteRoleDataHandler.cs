using System;


namespace ET
{

    [ActorMessageHandler]
    public  class R2Rank_DeleteRoleDataHandler : AMActorRpcHandler<Scene, R2Rank_DeleteRoleData, Rank2R_DeleteRoleData>
    {

        protected override async ETTask Run(Scene scene, R2Rank_DeleteRoleData request, Rank2R_DeleteRoleData response, Action reply)
        {
            RankSceneComponent rankScene = scene.GetComponent<RankSceneComponent>();
            rankScene.OnDeleteRole(rankScene.DBRankInfo.rankingInfos, request.DeleUserID);
            rankScene.OnDeleteRole(rankScene.DBRankInfo.rankingCamp1, request.DeleUserID);
            rankScene.OnDeleteRole(rankScene.DBRankInfo.rankingCamp2, request.DeleUserID);

            rankScene.OnDeleteRole(rankScene.DBRankInfo.rankingPets, request.DeleUserID);

            reply();
            await ETTask.CompletedTask;
        }
    }
}

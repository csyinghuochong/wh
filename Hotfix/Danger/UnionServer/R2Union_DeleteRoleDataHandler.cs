using System;
using System.Collections.Generic;

namespace ET
{
    [ActorMessageHandler]
    public class R2Union_DeleteRoleDataHandler : AMActorRpcHandler<Scene, R2Union_DeleteRoleData, Union2R_DeleteRoleData>
    {
        protected override async ETTask Run(Scene scene, R2Union_DeleteRoleData request, Union2R_DeleteRoleData response, Action reply)
        {
            if (request.UnitId <= 0)
            {
                response.Error = LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_Invalid_Data);
                reply();
                return;
            }

            UnionSceneComponent unionScene = scene.GetComponent<UnionSceneComponent>();
            await unionScene.LoadAllUnionInfos();
            if (!unionScene.AllUnionsLoaded)
            {
                response.Error = LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_Operation_Fail);
                reply();
                return;
            }

            // 删角不依赖玩家缓存中的公会 ID；逐个公会加锁清理，避免遗漏旧申请或残留的成员记录。
            List<long> unionIds = new List<long>(unionScene.DBUnionInfos.Keys);
            foreach (long unionId in unionIds)
            {
                using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.UnionOperate, unionId))
                {
                    DBUnionInfo dbUnionInfo = await unionScene.GetDBUnionInfo(unionId);
                    if (dbUnionInfo == null)
                    {
                        continue;
                    }

                    UnionInfo unionInfo = dbUnionInfo.UnionInfo;
                    bool removedApply = unionInfo.ApplyList.Remove(request.UnitId);
                    int removedMembers = unionInfo.UnionPlayerList.RemoveAll(player => player.UserID == request.UnitId);
                    bool deletedLeader = unionInfo.LeaderId == request.UnitId;
                    if (!removedApply && removedMembers == 0 && !deletedLeader)
                    {
                        continue;
                    }

                    // 原会长移除后，第一位剩余成员接任；无剩余成员时沿用 LeaderId=0 的解散标记。
                    long newLeaderId = 0;
                    if (deletedLeader)
                    {
                        unionInfo.LeaderId = 0;
                        unionInfo.LeaderName = string.Empty;
                        if (unionInfo.UnionPlayerList.Count > 0)
                        {
                            UnionPlayerInfo newLeader = unionInfo.UnionPlayerList[0];
                            newLeaderId = newLeader.UserID;
                            unionInfo.LeaderId = newLeaderId;
                            unionInfo.LeaderName = newLeader.PlayerName;
                            newLeader.Position = UnionPosition.Leader;
                        }
                    }

                    await DBHelper.SaveComponent(scene.DomainZone(), unionId, dbUnionInfo);
                    if (newLeaderId != 0)
                    {
                        await ServerMessageHelper.NoticeUnionLeader(scene.DomainZone(), newLeaderId, 1);
                    }
                }
            }

            reply();
        }
    }
}

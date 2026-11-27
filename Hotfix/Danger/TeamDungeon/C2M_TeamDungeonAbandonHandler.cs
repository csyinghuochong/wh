using System;

namespace ET
{

	//队长关闭副本
    [ActorMessageHandler]
    public class C2M_TeamDungeonAbandonHandler : AMActorLocationRpcHandler<Unit, C2M_TeamDungeonAbandonRequest, M2C_TeamDungeonAbandonResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_TeamDungeonAbandonRequest request, M2C_TeamDungeonAbandonResponse response, Action reply)
        {
            if (request.FubenInstanceId <= 0)
            {
                response.Error = LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_Invalid_Data);
                reply();
                return;
            }

            // 完整登录会重置角色 TeamId；队长身份及实例归属统一由队伍服按当前队伍校验。
            T2M_TeamDungeonAbandonResponse result = (T2M_TeamDungeonAbandonResponse)await MessageHelper.CallActor(
                DBHelper.GetTeamServerId(unit), new M2T_TeamDungeonAbandonRequest
                {
                    UserId = unit.Id,
                    FubenInstanceId = request.FubenInstanceId,
                });
            response.Error = result.Error;
            reply();
        }
    }
}

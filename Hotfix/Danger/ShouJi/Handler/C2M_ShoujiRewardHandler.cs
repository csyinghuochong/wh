using System;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_ShoujiRewardHandler : AMActorLocationRpcHandler<Unit, C2M_ShoujiRewardRequest, M2C_ShoujiRewardResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_ShoujiRewardRequest request, M2C_ShoujiRewardResponse response, Action reply)
        {
            ShoujiComponentServer shoujiComponentServer = unit.GetComponent<ShoujiComponentServer>();
            ShouJiChapterInfo shouJiChapterInfo = shoujiComponentServer.GetShouJiChapterInfo(request.ChapterId);
            
            shouJiChapterInfo.RewardInfo |= (1 << request.RewardIndex);
            reply();
            await ETTask.CompletedTask;
        }

    }
}

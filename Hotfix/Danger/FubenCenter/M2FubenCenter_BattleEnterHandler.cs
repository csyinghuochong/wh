using System;

namespace ET
{
    [ActorMessageHandler]
    public class M2FubenCenter_BattleEnterHandler : AMActorRpcHandler<Scene, M2FubenCenter_BattleEnterRequest, FubenCenter2M_BattleEnterResponse>
    {
        protected override async ETTask Run(Scene scene, M2FubenCenter_BattleEnterRequest request, FubenCenter2M_BattleEnterResponse response, Action reply)
        {
            using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.Battle, scene.DomainZone()))
            {
                FubenCenterComponent fubenCenter = scene.GetComponent<FubenCenterComponent>();
                KeyValuePairInt keyValuePairInt = fubenCenter.GetBattleInstanceId(request.UserID, request.SceneId);
                if (keyValuePairInt == null)
                {
                    keyValuePairInt = await fubenCenter.GenerateBattleInstanceId(request.UserID, request.SceneId);
                }
                if (keyValuePairInt != null)
                {
                    response.FubenInstanceId = keyValuePairInt.Value;
                    response.Camp = keyValuePairInt.KeyId;
                }
            }

            reply();
            await ETTask.CompletedTask;
        }
    }
}

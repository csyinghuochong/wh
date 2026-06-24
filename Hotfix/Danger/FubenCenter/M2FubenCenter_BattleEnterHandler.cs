using System;

namespace ET
{
    [ActorMessageHandler]
    public class M2FubenCenter_BattleEnterHandler : AMActorRpcHandler<Scene, M2FubenCenter_BattleEnterRequest, FubenCenter2M_BattleEnterResponse>
    {
        protected override async ETTask Run(Scene scene, M2FubenCenter_BattleEnterRequest request, FubenCenter2M_BattleEnterResponse response, Action reply)
        {

            KeyValuePairInt keyValuePairInt  = scene.GetComponent<BattleSceneComponent>().GetBattleInstanceId(request.UserID, request.SceneId);
            if (keyValuePairInt != null)
            {
                response.FubenInstanceId = keyValuePairInt.Value;
                response.Camp = keyValuePairInt.KeyId;
                reply();
            }
            else
            {
                using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.Battle, scene.DomainZone()))
                {
                    
                    ///随机选择一个 fubenwork 创建副本。 
                    keyValuePairInt = await scene.GetComponent<BattleSceneComponent>().GenerateBattleInstanceId(request.UserID, request.SceneId);
                    if (keyValuePairInt != null)
                    {
                        response.FubenInstanceId = keyValuePairInt.Value;
                        response.Camp = keyValuePairInt.KeyId;
                    }
                }
                reply();
            }
           
            await ETTask.CompletedTask;
        }
    }
}

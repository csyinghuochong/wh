using System;

namespace ET
{
    [ActorMessageHandler]
    public class C2M_FubenTimesResetHandler : AMActorLocationRpcHandler<Unit, C2M_FubenTimesResetRequest, M2C_FubenTimesResetResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_FubenTimesResetRequest request, M2C_FubenTimesResetResponse response, Action reply)
        {
            if (request.SceneType != MapTypeEnum.PetTianTi)
            {
                response.Error = ErrorCode.ERR_NetWorkError;
                reply();
                return;
            }

            NumericComponent numericComponent = unit.GetComponent<NumericComponent>();
            if (numericComponent.GetAsInt(NumericType.FubenTimesReset) >= 3)
            {
                response.Error = ErrorCode.ERR_RequestRepeatedly;
                reply();
                return;
            }

            BagComponentServer bagComponentServer = unit.GetComponent<BagComponentServer>();
            if (!bagComponentServer.OnCostItemData($"3;200", ItemLocType.ItemLocBag, ItemGetWay.FubenGetReward  ))
            {
                response.Error = ErrorCode.ERR_DiamondNotEnoughError;
                reply();
                return;
            }
            int sceneId = BattleHelper.GetSceneIdByType(request.SceneType);
            numericComponent.ApplyChange(null, NumericType.FubenTimesReset, 1, 0);
            RoleInfoComponentServer roleInfoComponentServer = unit.GetComponent<RoleInfoComponentServer>();
            roleInfoComponentServer.ClearFubenTimes(sceneId);

            reply();
            await ETTask.CompletedTask;
        }
    }
}

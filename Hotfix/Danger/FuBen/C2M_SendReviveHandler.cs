using System;
using UnityEngine;
using System.Collections.Generic;

namespace ET
{

    [ActorMessageHandler]
    public class C2M_SendReviveHandler : AMActorLocationRpcHandler<Unit, C2M_SendReviveRequest, M2C_SendReviveResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_SendReviveRequest request, M2C_SendReviveResponse response, Action reply)
        {
            MapComponent mapComponent = unit.DomainScene().GetComponent<MapComponent>();

            if (request.Revive)
            {
                string reviveCost = LDGlobalValueCategory.Instance.Get(5).Value;
                bool success = unit.GetComponent<BagComponentServer>().OnCostItemData(reviveCost, ItemLocType.ItemLocBag, ItemGetWay.FubenGetReward  );
                if (!success)
                {
                    response.Error = ErrorCode.ERR_ItemNotEnoughError;
                    reply();
                    return;
                }

                unit.SetBornPosition(unit.Position, true);
                unit.GetComponent<UnitLifeComponent>()?.OnRevive();
            }
            else
            {
                if (mapComponent.MapTypeEnum == MapTypeEnum.TeamDungeon)
                {
                    TeamDungeonComponent teamDungeonComponent = unit.DomainScene().GetComponent<TeamDungeonComponent>();
                    unit.SetBornPosition(teamDungeonComponent.BossDeadPosition, true);
                }
                else
                {
                    LDScene ldScene = LDSceneCategory.Instance.Get(mapComponent.SceneId);

                    if (unit.GetBattleCamp() == CampEnum.CampPlayer_1)
                    {
                        unit.SetBornPosition(ldScene.GetBornPos(), false);
                    }
                    else
                    {
                        unit.SetBornPosition(ldScene.GetBorn2Pos(), false);
                    }
                }

                unit.GetComponent<UnitLifeComponent>()?.OnRevive();
            }

            reply();
            await ETTask.CompletedTask;
        }
    }
}

using System;

namespace ET
{

    [ActorMessageHandler]
    public class C2M_TowerFightBeginHandler : AMActorLocationRpcHandler<Unit, C2M_TowerFightBeginRequest, M2C_TowerFightBeginResponse>
    {
        protected override async ETTask Run(Unit unit, C2M_TowerFightBeginRequest request, M2C_TowerFightBeginResponse response, Action reply)
        {
            if (request.SceneType == 0)
            {
                int scenetype = unit.DomainScene().GetComponent<MapComponent>().MapTypeEnum;
                request.SceneType = scenetype;
                Log.Error($"C2M_TowerFightBeginRequest11 request.SceneType=null  {request.SceneType}");
            }

            switch (request.SceneType)
            {
                case MapTypeEnum.TowerDungeon:
                    unit.DomainScene().GetComponent<TowerComponent>()?.BeginTower();
                    break;
                default:
                  
                    break;
            }
          
            reply();
            await ETTask.CompletedTask;
        }
    }
}

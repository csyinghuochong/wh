using System;

namespace ET
{
    [ActorMessageHandler]
    public class M2FubenWork_ExitHandler : AMActorRpcHandler<Scene, M2FubenWork_ExitRequest, FubenWork2M_ExitResponse>
    {

        private async ETTask CloseBattleFubenScene(Scene fubenscene, M2FubenWork_ExitRequest request)
        {
            //Console.WriteLine($"M2LocalDungeon_Exit:  {fubenscene.Name}  {request.Camp1Player.Count}  {request.Camp2Player.Count}   {fubenscene.DomainZone()} ");
            fubenscene.GetComponent<BattleDungeonComponent>().OnBattleOver(request.Camp1Player, request.Camp2Player);
            await fubenscene.GetComponent<BattleDungeonComponent>().KickOutPlayer();
            await TimerComponent.Instance.WaitAsync(60000 + RandomHelper.RandomNumber(0, 1000));
            TransferHelper.NoticeFubenCenter(fubenscene, 2).Coroutine();
            fubenscene.Dispose();
        }

        protected override async ETTask Run(Scene scene, M2FubenWork_ExitRequest request, FubenWork2M_ExitResponse response, Action reply)
        {
            switch (request.SceneType)
            {
                case MapTypeEnum.Battle:
                    Scene fubenscene = Game.Scene.Get(request.FubenId);
                    CloseBattleFubenScene(fubenscene, request).Coroutine();
                    break;
            }

            reply();
            await ETTask.CompletedTask;
        }
    }
}

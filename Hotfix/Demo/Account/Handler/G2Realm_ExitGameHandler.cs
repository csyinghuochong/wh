using System;

namespace ET
{
    public class G2Realm_ExitGameHandler : AMActorRpcHandler<Scene, G2Realm_ExitGame, Realm2G_ExitGame>
    {

        /// <summary>
        /// KickPlayer
        /// </summary>
        /// <param name="scene"></param>
        /// <param name="request"></param>
        /// <param name="response"></param>
        /// <param name="reply"></param>
        /// <returns></returns>
        protected override async ETTask Run(Scene scene, G2Realm_ExitGame request, Realm2G_ExitGame response, Action reply)
        {
            if (MongoHelper.KeepSession)
            {
                return;
            }
            //scene.GetComponent<TokenComponent>().Remove(request.AccountId);
            //Log.Console($"G2A_ExitGame: {request.AccountId}");
            Game.EventSystem.Publish(new EventType.RemoveAccountSessions() { DomainScene = scene, AccountId = request.AccountId });

            reply();
            await ETTask.CompletedTask;
        }
    }
}

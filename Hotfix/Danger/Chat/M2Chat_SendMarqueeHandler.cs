using System;
using System.Collections.Generic;

namespace ET
{
    [ActorMessageHandler]
    public class M2Chat_SendMarqueeHandler : AMActorRpcHandler<Scene, M2Chat_SendMarquee, Chat2M_SendMarqueeResponse>
    {
        protected override async ETTask Run(Scene scene, M2Chat_SendMarquee request, Chat2M_SendMarqueeResponse response, Action reply)
        {
            if (scene.SceneType != SceneType.Chat)
            {
                reply();
                return;
            }

            ChatSceneComponent chatScene = scene.GetComponent<ChatSceneComponent>();
            MarqueeInfo marqueeInfo = new MarqueeInfo()
            {
                MarqueeId = request.MarqueeId,
                StartTime = TimeHelper.ServerNow(),
            };
            if (request.ParamList != null)
            {
                marqueeInfo.ParamList.AddRange(request.ParamList);
            }

            if (request.ExpireTime > marqueeInfo.StartTime)
            {
                await chatScene.AddImportantMarquee(marqueeInfo, request.ExpireTime);
            }

            M2C_MarqueeMessage message = new M2C_MarqueeMessage()
            {
                MarqueeInfo = marqueeInfo,
            };
            foreach (ChatInfoUnit otherUnit in chatScene.ChatInfoUnitsDict.Values)
            {
                MessageHelper.SendActor(otherUnit.GateSessionActorId, message);
            }

            reply();
        }
    }
}

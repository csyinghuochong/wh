using System;
using System.Collections.Generic;

namespace ET
{

    [ActorMessageHandler]
    public class C2Chat_SendJinYanHandler : AMActorRpcHandler<ChatInfoUnit, C2C_ChatJinYanRequest, C2C_ChatJinYanResponse>
    {

        protected override async ETTask Run(ChatInfoUnit chatInfoUnit, C2C_ChatJinYanRequest request, C2C_ChatJinYanResponse response, Action reply)
        {

            ChatSceneComponent chatInfoUnitsComponent = chatInfoUnit.DomainScene().GetComponent<ChatSceneComponent>();

            if (!chatInfoUnitsComponent.BeReportedNumber.TryGetValue(request.JinYanId, out BeReportedInfo bePortedNumber))
            {
                bePortedNumber = new BeReportedInfo();
                chatInfoUnitsComponent.BeReportedNumber.Add(request.JinYanId, bePortedNumber);
            }

            HashSet<long> reportedSet = new HashSet<long>(bePortedNumber.ReportedList);
            if (reportedSet.Contains(request.UnitId))
            {
                reply();
                return;
            }
            if (bePortedNumber.ReportedList.Count >= 5)
            {
                reply();
                return;
            }

            bePortedNumber.ReportedList.Add(request.UnitId);
            if (bePortedNumber.ReportedList.Count == 5)
            {
                for (int i = chatInfoUnitsComponent.WordChatInfos.Count - 1; i >= 0; i--)
                {
                    if (chatInfoUnitsComponent.WordChatInfos[i].UserId == request.JinYanId)
                    {
                        chatInfoUnitsComponent.WordChatInfos.RemoveAt(i);   
                    }
                }

                bePortedNumber.JinYanTime = TimeHelper.ServerNow() + TimeHelper.OneDay;
            }
            Log.Warning($"{chatInfoUnit.DomainZone()}   {request.JinYanId}  {request.JinYanPlayer} 被举报");
            reply();
            await ETTask.CompletedTask;
        }
    }
}

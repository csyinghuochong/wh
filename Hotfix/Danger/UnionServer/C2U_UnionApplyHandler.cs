using System;

namespace ET
{
    [ActorMessageHandler]
    public class C2U_UnionApplyHandler : AMActorRpcHandler<Scene, C2U_UnionApplyRequest, U2C_UnionApplyResponse>
    {
        protected override async ETTask Run(Scene scene, C2U_UnionApplyRequest request, U2C_UnionApplyResponse response, Action reply)
        {
            DBUnionInfo dBUnionInfo =await scene.GetComponent<UnionSceneComponent>().GetDBUnionInfo(request.UnionId);
            if (!dBUnionInfo.UnionInfo.ApplyList.Contains(request.UserId))
            {
                dBUnionInfo.UnionInfo.ApplyList.Add(request.UserId);
            }

            long gateServerId = DBHelper.GetGateServerId(scene.DomainZone());
            G2T_GateUnitInfoResponse g2M_UpdateUnitResponse = (G2T_GateUnitInfoResponse)await ActorMessageSenderComponent.Instance.Call
                  (gateServerId, new T2G_GateUnitInfoRequest()
                  {
                      UserID = dBUnionInfo.UnionInfo.LeaderId
                  });
            if (g2M_UpdateUnitResponse.PlayerState == (int)PlayerState.Game && g2M_UpdateUnitResponse.SessionInstanceId > 0)
            {
                M2C_UnionApplyResult m2C_HorseNoticeInfo = new M2C_UnionApplyResult();
                MessageHelper.SendActor(g2M_UpdateUnitResponse.SessionInstanceId, m2C_HorseNoticeInfo);
            }
            DBHelper.SaveComponent(scene.DomainZone(), request.UnionId, dBUnionInfo).Coroutine();
            reply();
        }
    }
}

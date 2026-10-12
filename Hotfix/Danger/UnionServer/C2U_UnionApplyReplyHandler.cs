using System;

namespace ET
{

    [ActorMessageHandler]
    public class C2U_UnionApplyReplyHandler : AMActorRpcHandler<Scene, C2U_UnionApplyReplyRequest, U2C_UnionApplyReplyResponse>
    {

        protected override async ETTask Run(Scene scene, C2U_UnionApplyReplyRequest request, U2C_UnionApplyReplyResponse response, Action reply)
        {
            using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.UnionOperate, request.UnionId)) 
            {
                UnionSceneComponent unionSceneComponent = scene.GetComponent<UnionSceneComponent>();
                DBUnionInfo dBUnionInfo = await unionSceneComponent.GetDBUnionInfo(request.UnionId);
                if (dBUnionInfo == null)
                {
                    response.Error = ErrorCode.ERR_Union_Not_Exist;
                    reply();
                    return;
                }

                UnionPlayerInfo operatorInfo = UnionHelper.GetUnionPlayerInfo(dBUnionInfo.UnionInfo.UnionPlayerList, request.ActorId);
                if (operatorInfo == null || operatorInfo.Position == UnionPosition.Member)
                {
                    response.Error = ErrorCode.ERR_Union_NoLimits;
                    reply();
                    return;
                }
                if (request.ReplyCode == 1 && !dBUnionInfo.UnionInfo.ApplyList.Contains(request.UserId))
                {
                    response.Error = ErrorCode.ERR_ModifyData;
                    reply();
                    return;
                }

                response.Error = await unionSceneComponent.OnJoinUinon(request.UnionId, request.UserId, request.ReplyCode);
            }
            reply();
        }
    }
}

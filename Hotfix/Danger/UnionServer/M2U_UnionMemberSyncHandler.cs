using System;

namespace ET
{
    [ActorMessageHandler]
    public class M2U_UnionMemberSyncHandler : AMActorRpcHandler<Scene, M2U_UnionMemberSyncRequest, U2M_UnionMemberSyncResponse>
    {
        protected override async ETTask Run(Scene scene, M2U_UnionMemberSyncRequest request, U2M_UnionMemberSyncResponse response, Action reply)
        {
            using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.UnionOperate, request.UnionId))
            {
                DBUnionInfo dBUnionInfo = await scene.GetComponent<UnionSceneComponent>().GetDBUnionInfo(request.UnionId);
                if (dBUnionInfo == null || dBUnionInfo.UnionInfo == null || dBUnionInfo.UnionInfo.UnionPlayerList == null)
                {
                    reply();
                    return;
                }

                UnionPlayerInfo member = UnionHelper.GetUnionPlayerInfo(dBUnionInfo.UnionInfo.UnionPlayerList, request.UnitId);
                if (member == null || !ApplyMember(member, request))
                {
                    reply();
                    return;
                }

                DBHelper.SaveComponent(scene.DomainZone(), request.UnionId, dBUnionInfo).Coroutine();
            }

            reply();
            await ETTask.CompletedTask;
        }

        private static bool ApplyMember(UnionPlayerInfo member, M2U_UnionMemberSyncRequest request)
        {
            if (member.PlayerName == request.PlayerName
                && member.PlayerLevel == request.PlayerLevel
                && member.Combat == request.Combat
                && member.Occ == request.Occ
                && member.OccTwo == request.OccTwo
                && member.RoleHeadId == request.RoleHeadId
                && member.RoleHeadFrame == request.RoleHeadFrame)
            {
                return false;
            }

            member.PlayerName = request.PlayerName;
            member.PlayerLevel = request.PlayerLevel;
            member.Combat = request.Combat;
            member.Occ = request.Occ;
            member.OccTwo = request.OccTwo;
            member.RoleHeadId = request.RoleHeadId;
            member.RoleHeadFrame = request.RoleHeadFrame;
            return true;
        }
    }
}

using System;
using System.Collections.Generic;

namespace ET
{

    [ActorMessageHandler]
    public class C2U_UnionApplyListHandler : AMActorRpcHandler<Scene, C2U_UnionApplyListRequest, U2C_UnionApplyListResponse>
    {

        protected override async ETTask Run(Scene scene, C2U_UnionApplyListRequest request, U2C_UnionApplyListResponse response, Action reply)
        {
            DBUnionInfo dBUnionInfo =await scene.GetComponent<UnionSceneComponent>().GetDBUnionInfo(request.UnionId);

            List<UnionPlayerInfo> unionPlayers = new List<UnionPlayerInfo>();
            for(int i = dBUnionInfo.UnionInfo.ApplyList.Count - 1; i >= 0; i--)
            {
                long applicantId = dBUnionInfo.UnionInfo.ApplyList[i];
                int homeZone = UnitZoneHelper.GetHomeZone(applicantId);
                //判断玩家是否已经有家族了
                NumericComponent numericComponent_0 = await DBHelper.GetComponent<NumericComponent>(homeZone, applicantId);
                if (numericComponent_0 == null ||  numericComponent_0.GetAsLong(NumericType.UnionId_0) > 0)
                {
                    dBUnionInfo.UnionInfo.ApplyList.RemoveAt(i);
                    continue;
                }

                RoleInfoComponentServer roleInfoComponentServer = await DBHelper.GetComponent<RoleInfoComponentServer>(homeZone, applicantId);
                if (roleInfoComponentServer == null)
                {
                    continue;
                }
                unionPlayers.Add( new UnionPlayerInfo() 
                {  
                    PlayerLevel = roleInfoComponentServer.RoleInfo.Lv,
                    PlayerName = roleInfoComponentServer.RoleInfo.Name,
                    Combat  = roleInfoComponentServer.RoleInfo.Combat,
                    UserID = roleInfoComponentServer.RoleInfo.UserId,
                    Occ = roleInfoComponentServer.RoleInfo.Occ,
                    OccTwo = roleInfoComponentServer.RoleInfo.OccTwo, 
                } );
            }

            response.UnionPlayerList = unionPlayers;
            reply();
        }
    }
}

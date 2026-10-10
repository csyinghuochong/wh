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

            List<UnionApplyItem> applyList = new List<UnionApplyItem>();
            for(int i = dBUnionInfo.UnionInfo.ApplyList.Count - 1; i >= 0; i--)
            {
                long applicantId = dBUnionInfo.UnionInfo.ApplyList[i];
                //判断玩家是否已经有家族了
                NumericComponent numericComponent_0 = await DBHelper.GetPlayerComponent<NumericComponent>(applicantId);
                if (numericComponent_0 == null ||  numericComponent_0.GetAsLong(NumericType.UnionId_0) > 0)
                {
                    dBUnionInfo.UnionInfo.ApplyList.RemoveAt(i);
                    continue;
                }

                RoleInfoComponentServer roleInfoComponentServer = await DBHelper.GetPlayerComponent<RoleInfoComponentServer>(applicantId);
                if (roleInfoComponentServer == null)
                {
                    continue;
                }

                RoleInfo roleInfo = roleInfoComponentServer.RoleInfo;
                applyList.Add(new UnionApplyItem()
                {
                    PlayerName = roleInfo.Name,
                    PlayerLevel = roleInfo.Lv,
                    Combat = roleInfo.Combat,
                    HeadId = roleInfo.HeadIconId,
                    Occ = roleInfo.Occ,
                    UserID = roleInfo.UserId,
                    OccTwo = roleInfo.OccTwo,
                });
            }

            response.ApplyList = applyList;
            reply();
        }
    }
}

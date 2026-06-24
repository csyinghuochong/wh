using System;

namespace ET
{
    [ActorMessageHandler]
    public class C2U_UnionRecordHandler : AMActorRpcHandler<Scene, C2U_UnionRecordRequest, U2C_UnionRecordResponse>
    {
        protected override async ETTask Run(Scene scene, C2U_UnionRecordRequest request, U2C_UnionRecordResponse response, Action reply)
        {
            DBUnionInfo dBUnionInfo = await scene.GetComponent<UnionSceneComponent>().GetDBUnionInfo( request.UnionId );
            if (dBUnionInfo == null)
            {
                reply();
                return;
            }

            for (int i = dBUnionInfo.UnionInfo.DonationRecords.Count - 1; i >=0; i--)
            {
                DonationRecord donationRecord = dBUnionInfo.UnionInfo.DonationRecords[i];
                RoleInfoComponent roleInfoComponent = await DBHelper.GetComponent<RoleInfoComponent>(scene.DomainZone(), donationRecord.UnitId);
                if (roleInfoComponent == null)
                {
                    dBUnionInfo.UnionInfo.UnionPlayerList.RemoveAt(i);
                    continue;
                }
                donationRecord.Name = roleInfoComponent.RoleInfo.Name;
                donationRecord.Occ = roleInfoComponent.RoleInfo.Occ;    
            }
            response.DonationRecords = dBUnionInfo.UnionInfo.DonationRecords;
            reply();
            await ETTask.CompletedTask;
        }
    }
}

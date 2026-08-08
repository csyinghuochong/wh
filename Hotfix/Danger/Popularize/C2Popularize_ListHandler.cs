using System;
using System.Collections.Generic;

namespace ET
{

    [ActorMessageHandler]
    public class C2Popularize_ListHandler : AMActorRpcHandler<Scene, C2Popularize_ListRequest, Popularize2C_ListResponse>
    {
        protected override async ETTask Run(Scene scene, C2Popularize_ListRequest request, Popularize2C_ListResponse response, Action reply)
        {
            Log.Warning($"C2Popularize_ListRequest:{request.ActorId}");
            using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.Popularize, scene.DomainZone()))
            {
                DBPopularizeInfo dBPopularizeInfo = await DBHelper.GetComponent<DBPopularizeInfo>(scene.DomainZone(), request.ActorId);
                bool created = false;
                if (dBPopularizeInfo == null)
                {
                    if (scene.GetChild<DBPopularizeInfo>(request.ActorId) != null)
                    {
                        reply();
                        return;
                    }

                    dBPopularizeInfo = scene.AddChildWithId<DBPopularizeInfo>(request.ActorId);
                    created = true;

                    int newzone = scene.DomainZone();
                    List<DBPopularizeInfo> dBPopularizeInfoList = await Game.Scene.GetComponent<DBComponent>().Query<DBPopularizeInfo>(newzone, d => d.Id > 0);
                    int xuindex = dBPopularizeInfoList.Count + 1;

                    //推广码生成规则
                    dBPopularizeInfo.PopularizeCode = newzone * 1000000 + xuindex;

                    await DBHelper.SaveComponent(scene.DomainZone(), request.ActorId, dBPopularizeInfo);
                }
             
                for (int i = 0; i < dBPopularizeInfo.MyPopularizeList.Count; i++)
                {
                    long unitid = dBPopularizeInfo.MyPopularizeList[i].UnitId;
                    int oldZone = UnitIdStruct.GetUnitZone(unitid);
                    int newZone = CommonHelper.GetNewServerId(ServerHelper.GetServerList(), oldZone);
                    if (newZone < 5)
                    {
                        continue;
                    }

                    RoleInfoComponentServer roleInfoComponentServer = await DBHelper.GetComponentCache<RoleInfoComponentServer>(newZone, unitid);
                    if (roleInfoComponentServer == null)
                    {
                        continue;
                    }

                    dBPopularizeInfo.MyPopularizeList[i].Nmae = roleInfoComponentServer.RoleInfo.Name;
                    dBPopularizeInfo.MyPopularizeList[i].Level = roleInfoComponentServer.RoleInfo.Lv;
                    dBPopularizeInfo.MyPopularizeList[i].Occ = roleInfoComponentServer.RoleInfo.Occ;
                    dBPopularizeInfo.MyPopularizeList[i].OccTwo = roleInfoComponentServer.RoleInfo.OccTwo;
                }

                response.PopularizeCode = dBPopularizeInfo.PopularizeCode;
                response.BePopularizeId = dBPopularizeInfo.BePopularizeId;
                response.MyPopularizeList = dBPopularizeInfo.MyPopularizeList;

                if (created)
                {
                    dBPopularizeInfo.Dispose();
                }
            }

            reply();
            await ETTask.CompletedTask;
        }
    }
}

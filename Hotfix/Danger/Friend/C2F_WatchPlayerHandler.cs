using System;
using System.Collections.Generic;

namespace ET
{

    [ActorMessageHandler]
    public class C2F_WatchPlayerHandler : AMActorRpcHandler<Scene, C2F_WatchPlayerRequest, F2C_WatchPlayerResponse>
    {
        protected override async ETTask Run(Scene scene, C2F_WatchPlayerRequest request, F2C_WatchPlayerResponse response, Action reply)
        {
            // 可跨区查看：按被查看玩家 UnitId 归属服取 DBCache，勿用 Friend 场景 DomainZone
            long dbCacheId = DBHelper.GetUnitCacheConfig(request.UserId);
            D2G_GetComponent d2GGetUnit_1 = (D2G_GetComponent)await ActorMessageSenderComponent.Instance.Call(dbCacheId, new G2D_GetComponent() { UnitId = request.UserId, Component = DBHelper.RoleInfoComponent });
            RoleInfoComponentServer userinfo = d2GGetUnit_1.Component as RoleInfoComponentServer;
            if (userinfo == null)
            {
                response.Error = ErrorCode.ERR_Error;
                reply();
                return;
            }
            //根据类型返回不同的值
            switch (request.WatchType) 
            {
                //全部
                case 0:
                    D2G_GetComponent d2GGetUnit_2 = (D2G_GetComponent)await ActorMessageSenderComponent.Instance.Call(dbCacheId, new G2D_GetComponent() { UnitId = request.UserId, Component = DBHelper.BagComponentServer });
                    response.Lv = userinfo.RoleInfo.Lv;
                    response.Name = userinfo.RoleInfo.Name;
                    BagComponentServer bagComponentsServer = d2GGetUnit_2.Component as BagComponentServer;
                    if (bagComponentsServer == null)
                    {
                        response.Error = ErrorCode.ERR_Error;
                        reply();
                        return;
                    }

                    response.EquipList = bagComponentsServer.EquipList;
                    response.PetHeXinList = bagComponentsServer.PetHeXinList;
                    response.Occ = userinfo.RoleInfo.Occ;
                    D2G_GetComponent d2GGetUnit_3 = (D2G_GetComponent)await ActorMessageSenderComponent.Instance.Call(dbCacheId, new G2D_GetComponent() { UnitId = request.UserId, Component = DBHelper.PetComponent });
                    PetComponentServer petComponentServer = d2GGetUnit_3.Component as PetComponentServer;
                    List<RolePetInfo> rolePetInfos = petComponentServer.RolePetInfos;
                    List<RolePetInfo> rolePetInfosResponse = new List<RolePetInfo>();
                    for (int pet = rolePetInfos.Count - 1; pet >= 0; pet-- )
                    {
                        if (rolePetInfos[pet].PetStatus < 2)
                        {
                            rolePetInfosResponse.Add(rolePetInfos[pet]);
                        }
                    }

                    response.RolePetInfos = rolePetInfosResponse;
                    response.PetSkinList = petComponentServer.PetSkinList;

                    D2G_GetComponent d2GGetUnit_4 = (D2G_GetComponent)await ActorMessageSenderComponent.Instance.Call(dbCacheId, new G2D_GetComponent() { UnitId = request.UserId, Component = DBHelper.NumericComponent });
                    NumericComponent numericComponent = d2GGetUnit_4.Component as NumericComponent;
                    foreach ((int key, long value) in numericComponent.NumericDic)
                    {
                        if (key >= (int)NumericType.Max)
                        {
                            continue;
                        }
                        response.Ks.Add(key);
                        response.Vs.Add(value);
                    }

                    response.FashionIds = bagComponentsServer.FashionEquipList;
                    break;
                //只返回名字
                case 1:
                    response.Name = userinfo.RoleInfo.Name;
                    break;
                case 2:
                    long teamServerId = DBHelper.GetTeamServerId(UnitZoneHelper.GetHomeZone(request.UserId));
                    T2C_GetTeamInfoResponse g_SendChatRequest1 = (T2C_GetTeamInfoResponse)await ActorMessageSenderComponent.Instance.Call
                        (teamServerId, new C2T_GetTeamInfoRequest() { UserID = request.UserId });

                    response.TeamId = g_SendChatRequest1.TeamInfo != null ? g_SendChatRequest1.TeamInfo.TeamId : 0;
                    break;
                default:
                    break;
            }
            reply();
        }
    }
}

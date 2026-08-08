using System;
using System.Collections.Generic;

namespace ET
{

    [ActorMessageHandler]
    public class C2F_WatchPlayerHandler : AMActorRpcHandler<Scene, C2F_WatchPlayerRequest, F2C_WatchPlayerResponse>
    {
        protected override async ETTask Run(Scene scene, C2F_WatchPlayerRequest request, F2C_WatchPlayerResponse response, Action reply)
        {
            int homeZone = UnitZoneHelper.GetHomeZone(request.UserId);
            RoleInfoComponentServer userinfo = await DBHelper.GetComponent<RoleInfoComponentServer>(homeZone, request.UserId);
            if (userinfo == null)
            {
                response.Error = ErrorCode.ERR_Error;
                reply();
                return;
            }
            RoleInfo roleInfo = userinfo.RoleInfo;
            //根据类型返回不同的值
            switch (request.WatchType) 
            {
                //全部
                case 0:
                    response.Lv = roleInfo.Lv;
                    response.Name = roleInfo.Name;
                    BagComponentServer bagComponentsServer = await DBHelper.GetComponent<BagComponentServer>(homeZone, request.UserId);
                    if (bagComponentsServer == null)
                    {
                        response.Error = ErrorCode.ERR_Error;
                        reply();
                        return;
                    }

                    response.EquipList = bagComponentsServer.EquipList;
                    response.Occ = roleInfo.Occ;
                    PetComponentServer petComponentServer = await DBHelper.GetComponent<PetComponentServer>(homeZone, request.UserId);
                    if (petComponentServer == null)
                    {
                        response.Error = ErrorCode.ERR_Error;
                        reply();
                        return;
                    }
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

                    NumericComponent numericComponent = await DBHelper.GetComponent<NumericComponent>(homeZone, request.UserId);
                    if (numericComponent == null)
                    {
                        response.Error = ErrorCode.ERR_Error;
                        reply();
                        return;
                    }
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
                    response.Name = roleInfo.Name;
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

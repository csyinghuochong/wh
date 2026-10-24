using System;
using System.Collections.Generic;

namespace ET
{

    [ActorMessageHandler]
    public class C2F_WatchPlayerHandler : AMActorRpcHandler<Scene, C2F_WatchPlayerRequest, F2C_WatchPlayerResponse>
    {
        protected override async ETTask Run(Scene scene, C2F_WatchPlayerRequest request, F2C_WatchPlayerResponse response, Action reply)
        {
            int homeZone = FriendHelper.PrepareHomeZone(request.UserId);
            Console.WriteLine($"request.UserId:  {homeZone}");
            if (homeZone == 0)
            {
                response.Error = ErrorCode.ERR_Error;
                reply();
                return;
            }
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
                    response.Occ = roleInfo.Occ;
                    response.RoleHeadId = roleInfo.RoleHeadId;
                    response.RoleHeadFrame = roleInfo.RoleHeadFrame;
                    response.UnionName = string.IsNullOrWhiteSpace(roleInfo.UnionName) ? string.Empty : roleInfo.UnionName;
                    response.Sex = roleInfo.Sex;
                    BagComponentServer bagComponentsServer = await DBHelper.GetComponent<BagComponentServer>(homeZone, request.UserId);
                    if (bagComponentsServer == null)
                    {
                        response.Error = ErrorCode.ERR_Error;
                        reply();
                        return;
                    }

                    NumericComponent numericComponent = await DBHelper.GetComponent<NumericComponent>(homeZone, request.UserId);
                    if (numericComponent == null)
                    {
                        response.Error = ErrorCode.ERR_Error;
                        reply();
                        return;
                    }
                    response.FashionIds = bagComponentsServer.FashionEquipList;
                    response.MainHandId = bagComponentsServer.GetWuqiItemId();
                    response.OffHandId = bagComponentsServer.GetFuShouItemId();
                    response.TeamId = numericComponent.GetAsLong(NumericType.TeamId);
                    break;
                //只返回名字
                case 1:
                    response.Name = roleInfo.Name;
                    break;
                default:
                    break;
            }
            reply();
        }
    }
}

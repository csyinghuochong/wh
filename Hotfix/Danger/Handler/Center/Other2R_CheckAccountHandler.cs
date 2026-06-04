using System.Collections.Generic;
using System;

namespace ET
{

    [ActorMessageHandler]
    public class Other2R_CheckAccountHandler : AMActorRpcHandler<Scene, Other2R_CheckAccount, R2Other_CheckAccount>
    {
        protected override async ETTask Run(Scene scene, Other2R_CheckAccount request, R2Other_CheckAccount response, Action reply)
        {
            Log.Warning(($"A2Center_CheckAccount:{request.AccountName}"));
            List<DBCenterAccountInfo> centerAccountInfoList = await Game.Scene.GetComponent<DBComponent>().Query<DBCenterAccountInfo>(scene.DomainZone(), d => d.Account == request.AccountName && d.Password == request.Password); 
            
            DBCenterAccountInfo dBCenterAccountInfo = centerAccountInfoList != null && centerAccountInfoList.Count > 0 ? centerAccountInfoList[0] : null;
            response.PlayerInfo = dBCenterAccountInfo != null ? dBCenterAccountInfo.PlayerInfo : null;
            response.AccountId = dBCenterAccountInfo != null ? dBCenterAccountInfo.Id : 0;
            
            if (response.PlayerInfo != null)
            {
                for (int i = 0; i < response.PlayerInfo.RechargeInfos.Count; i++)
                {
                    response.PlayerInfo.RechargeInfos[i].OrderInfo = string.Empty;
                }
            }
            
        
            response.IsHoliday = scene.GetComponent<CenterServerComponent>().IsHoliday;
            response.StopServer = scene.GetComponent<CenterServerComponent>().StopServer;
            response.Message = dBCenterAccountInfo!=null? dBCenterAccountInfo.AccountType.ToString():string.Empty;
            
            if (dBCenterAccountInfo != null)
            {
                response.TodayCreateRole = ComHelp.GetTodayCreateRoleNumber(dBCenterAccountInfo.RoleList);
                response.CreateTime = dBCenterAccountInfo.CreateTime;
            }
            else
            {
                response.TodayCreateRole = 0;
                response.CreateTime = 0;    
            }

            reply();
            await ETTask.CompletedTask;
        }
    }
}
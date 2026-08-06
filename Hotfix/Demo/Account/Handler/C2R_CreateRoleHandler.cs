using System;
using System.Collections.Generic;
using System.Linq;

namespace ET
{
    [MessageHandler]
	public class C2R_CreateRoleHandler : AMRpcHandler<C2R_CreateRoleRequest, R2C_CreateRoleResponse>
	{
		protected override async ETTask Run(Session session, C2R_CreateRoleRequest request, R2C_CreateRoleResponse response, Action reply)
		{
			try
			{
				//判断名字是否符合要求
				if (string.IsNullOrEmpty(request.CreateName))
				{
                    response.Error = ErrorCode.ERR_CreateRoleName;
                    response.Message = "角色名字过短!";
                    reply();
                    return;
                }
                if (request.CreateName.Contains(" "))
                {
					Log.Error($"C2A_CreateRoleHandler.1");
                    response.Error = ErrorCode.ERR_ModifyData;
                    reply();
                    return;
                }
                request.CreateName = request.CreateName.Trim();
                if (request.CreateName.Length >= 8)
				{
					response.Error = ErrorCode.ERR_CreateRoleName;
					response.Message = "角色名字过长!";
					reply();
					return;
				}
				if (session.DomainZone() == 0)
				{
					Log.Error("session.DomainZone() == 0");
					response.Error = ErrorCode.ERR_Error;
					reply();
					return;
				}

				using (session.AddComponent<SessionLockingComponent>())
				{
					using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.LoginAccount, request.AccountId.GetHashCode()))
					{
						List<RoleInfoComponentServer> result = await Game.Scene.GetComponent<DBComponent>().Query<RoleInfoComponentServer>(request.ServerId, _account => _account.UserName == request.CreateName);
						if (result.Count > 0)
						{
							response.Error = ErrorCode.ERR_RoleNameRepeat;
							reply();
							return;
						}

                        List<DBCenterAccountInfo> centerAccountList = await Game.Scene.GetComponent<DBComponent>().Query<DBCenterAccountInfo>(CommonConfig.CenterZoneId, d => d.Id == request.AccountId);
                        if (centerAccountList == null || centerAccountList.Count == 0)
                        {
                            response.Error = ErrorCode.ERR_NotFindAccount;
                            reply();
                            return;
                        }

						if (!ServerHelper.IsBanHaoZone(session.DomainZone())
                            && !GMHelp.GmAccount.Contains(centerAccountList[0].Account)
                            && CommonHelper.GetTodayCreateRoleNumber(centerAccountList[0].RoleList) >= 8)
						{
                            response.Error = ErrorCode.ERR_CreateRole_Limit;
                            reply();
                            return;
                        }

						long accountCrateTime = centerAccountList[0].CreateTime;
						long serverNowTime = TimeHelper.ServerNow();
						long serverOpenTime = ServerHelper.GetOpenServerTime(false, request.ServerId);
						if (!ServerHelper.IsBanHaoZone(session.DomainZone()) 
							&& !ServerHelper.IsGoogleServer(session.DomainZone())
                            && !GMHelp.GmAccount.Contains(centerAccountList[0].Account)
							&& !GMHelp.TestNewOccAccount.Contains(centerAccountList[0].Account))
						{
                           /* if (!centerAccountList[0].Password.Equals(CommonConfig.RobotPassWord) && accountCrateTime > 0 && (accountCrateTime - serverOpenTime >= TimeHelper.OneDay * 14))
                            {
                                response.Error = ErrorCode.ERR_CreateRole_Limit_2;
                                reply();
                                return;
                            }*/
                        }
                     
						if (!LDOccupationCategory.Instance.Contain(request.CreateOcc))
						{
                            Log.Error($"C2A_CreateRoleHandler.3");
                            response.Error = ErrorCode.ERR_ModifyData;
                            reply();
                            return;
                        }
						
						int robotId = 0;
						if(centerAccountList[0].Password == CommonConfig.RobotPassWord)
						{
							robotId = int.Parse(centerAccountList[0].Account.Split('_')[0]);
						}
						
						CreateRoleInfo createRoleInfo = new CreateRoleInfo();
						createRoleInfo.UserID = IdGenerater.Instance.GenerateUnitId(request.ServerId);
						createRoleInfo.PlayerLv = 1;
						createRoleInfo.PlayerOcc = request.CreateOcc;
						createRoleInfo.PlayerName = request.CreateName;
						createRoleInfo.ServerId = request.ServerId;
						createRoleInfo.RobotId = robotId;
						createRoleInfo.Sex = request.Sex;
                        centerAccountList[0].RoleList.Add(createRoleInfo);
                        Game.Scene.GetComponent<DBComponent>().Save<DBCenterAccountInfo>(CommonConfig.CenterZoneId, centerAccountList[0]).Coroutine();
                        
                        //返回角色信息
                        //CreateRoleInfo roleList = Function_Role.GetInstance().GetRoleListInfo(roleInfo,  userId);
						response.createRoleInfo = createRoleInfo;
						response.TodayCreateRole = CommonHelper.GetTodayCreateRoleNumber(centerAccountList[0].RoleList);
                        reply();
					}
				}
			}
			catch (Exception ex)
			{ 
				Log.Info(ex.ToString());
			}
			
		}
	}
}
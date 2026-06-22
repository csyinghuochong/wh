using System;
using System.Collections.Generic;
using System.Linq;

namespace ET
{
    [MessageHandler]
	public class C2R_CreateRoleHandler : AMRpcHandler<C2R_CreateRoleData, R2C_CreateRoleData>
	{
		protected override async ETTask Run(Session session, C2R_CreateRoleData request, R2C_CreateRoleData response, Action reply)
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
						List<RoleInfoComponent> result = await Game.Scene.GetComponent<DBComponent>().Query<RoleInfoComponent>(request.ServerId, _account => _account.UserName == request.CreateName);
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

						if (!CommonHelper.IsBanHaoZone(session.DomainZone())
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
						if (!CommonHelper.IsBanHaoZone(session.DomainZone()) 
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
						
                        long userId = IdGenerater.Instance.GenerateUnitId(request.ServerId);
                        /*RoleInfoComponent roleInfoComponent = session.AddChildWithId<RoleInfoComponent>(userId);
						roleInfoComponent.Account = centerAccountList[0].Account;
                        roleInfoComponent.Password = centerAccountList[0].Password;
						roleInfoComponent.CreateAccountTime = centerAccountList[0].CreateTime;
                        RoleInfo roleInfo = roleInfoComponent.RoleInfo;
						roleInfo.Sp = 1;
						roleInfo.UserId = userId;
						roleInfo.BaoShiDu = 100;
						roleInfo.JiaYuanLv = 10001;
						roleInfo.JiaYuanFund = 10000;
						roleInfo.AccInfoID = centerAccountList[0].Id;
						roleInfo.Name = request.CreateName;
						roleInfo.ServerMailIdCur = -1;
                        roleInfo.PiLao = int.Parse(GlobalValueConfigCategory.Instance.Get(10).Value);        //初始化疲劳
						roleInfo.Vitality = int.Parse(GlobalValueConfigCategory.Instance.Get(10).Value);
						roleInfo.MakeList.AddRange(ComHelp.StringArrToIntList(GlobalValueConfigCategory.Instance.Get(18).Value.Split(';')));
						roleInfo.CreateTime = TimeHelper.ServerNow();

                        if (centerAccountList[0].Password == ComHelp.RobotPassWord)
						{
							int robotId = int.Parse(centerAccountList[0].Account.Split('_')[0]);
							RobotConfig robotConfig = RobotConfigCategory.Instance.Get(robotId);
							roleInfo.Level = robotConfig.Behaviour == 1 ?  RandomHelper.RandomNumber(10, 19) : robotConfig.Level;
							roleInfo.Occ = robotConfig.Behaviour == 1 ?  RandomHelper.RandomNumber(1, 3) : robotConfig.Occ;
                            roleInfo.Gold = 100000;
                            roleInfo.RobotId = robotId;
                            //roleInfo.OccTwo = robotConfig.OccTwo;
                        }
						else
						{
							roleInfo.Level = 1;
							roleInfo.Gold = 0;
                            roleInfo.SeasonLevel = 1;
                            roleInfo.Occ = request.CreateOcc;
						}*/

						//long dbCacheId = DBHelper.GetDbCacheId(request.ServerId);
                        //D2M_SaveComponent d2GSave = (D2M_SaveComponent)await ActorMessageSenderComponent.Instance.Call(dbCacheId, new M2D_SaveComponent() { UnitId = userId, EntityByte = MongoHelper.ToBson(RoleInfoComponent), ComponentType = DBHelper.RoleInfoComponent });
						//roleInfoComponent.Dispose();
						//创建角色组件
						//await DBHelper.AddDataComponent<NumericComponent>(request.ServerId, userId, DBHelper.NumericComponent);
						//await DBHelper.AddDataComponent<DBFriendInfo>(request.ServerId, userId, DBHelper.DBFriendInfo);
						//await DBHelper.AddDataComponent<DBMailInfo>(request.ServerId, userId, DBHelper.DBMailInfo);

						int robotId = 0;
						if(centerAccountList[0].Password == CommonConfig.RobotPassWord)
						{
							robotId = int.Parse(centerAccountList[0].Account.Split('_')[0]);
						}
						
						//存储账号信息
						CreateRoleInfo createRoleInfo = new CreateRoleInfo();
						createRoleInfo.UserID = IdGenerater.Instance.GenerateId();
						createRoleInfo.PlayerLv = 1;
						createRoleInfo.PlayerOcc = request.CreateOcc;
						createRoleInfo.PlayerName = request.CreateName;
						createRoleInfo.ServerId = request.ServerId;
						createRoleInfo.RobotId = robotId;
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
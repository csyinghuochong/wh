using System;
using System.Collections.Generic;

namespace ET
{
    public class C2R_LoginAccountHandler : AMRpcHandler<C2R_LoginAccount, R2C_LoginAccount>
    {
        
        protected override async ETTask Run(Session session, C2R_LoginAccount request, R2C_LoginAccount response, Action reply)
        {
            try
            {
                request.AccountName = request.AccountName.Trim().ToLower();
                request.Password = request.Password.Trim().ToLower();
                
                if (session.DomainScene().SceneType != SceneType.Realm)
                {
                    Log.Error($"LoginTest C2A_LoginAccount请求的Scene错误，当前Scene为：{session.DomainScene().SceneType}");
                    session.Dispose();
                    return;
                }
                session.RemoveComponent<SessionAcceptTimeoutComponent>();
               
                if (session.GetComponent<SessionLockingComponent>() != null)
                {
                    response.Error = ErrorCode.ERR_RequestRepeatedly;
                    reply();
                    session.Disconnect().Coroutine();
                    return;
                }

                if (string.IsNullOrEmpty(request.AccountName) || string.IsNullOrEmpty(request.Password))
                {
                    response.Error = ErrorCode.ERR_LoginInfoIsNull;
                    reply();
                    session.Disconnect().Coroutine();
                    return;
                }
                if (request.AccountName.Contains("请选择一种登录方式") 
                    || request.AccountName.Contains("一键登陆"))
                {
                    response.Error = ErrorCode.ERR_LoginInfoIsNull;
                    response.Message = "请联系qq136087482处理";
                    reply();
                    session.Disconnect().Coroutine();
                    return;
                }

                if (session.DomainScene().GetComponent<PlayerInfoListComponent>().IsArchiveing(request.AccountName, 0))
                {
                    response.Error = ErrorCode.ERR_Archiveing;
                    session.Disconnect().Coroutine();
                    reply();
                    return;
                }
                
                if (session.RemoteAddress.ToString().Contains("42.177.217.71"))
                {
                    response.Error = ErrorCode.ERR_LoginInfoIsNull;
                    reply();
                    session.Disconnect().Coroutine();
                    return;
                }

                if (request.Password == "3" || request.Password == "4")
                {
                    if (request.AccountName.Length < 3)
                    {
                        response.Error = ErrorCode.ERR_LoginInfoIsNull;
                        reply();
                        session.Disconnect().Coroutine();
                        return;
                    }
                    string head = request.AccountName.Substring(0, 3);
                    if (GMHelp.IllegalPhone.Contains(head))
                    {
                        response.Error = ErrorCode.ERR_IllegalPhoneError;
                        reply();
                        session.Disconnect().Coroutine();
                        return;
                    }
                }
                
                long sessionId  = session.InstanceId;
                
                using (session.AddComponent<SessionLockingComponent>())
                {
                    using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.LoginAccount, request.AccountName.Trim().GetHashCode()))
                    {
                        List<DBCenterAccountInfo> centerAccountInfoList = await Game.Scene.GetComponent<DBComponent>().Query<DBCenterAccountInfo>(CommonConfig.CenterZoneId, d => d.Account.Equals(request.AccountName));
                        DBCenterAccountInfo dbcenterAccountInfo = null;
                        if (centerAccountInfoList != null && centerAccountInfoList.Count > 0)
                        {
                            dbcenterAccountInfo = centerAccountInfoList[0];
                        }
                        if (dbcenterAccountInfo != null)
                        {
                            for (int i = dbcenterAccountInfo.RoleList.Count- 1; i >= 0; i--)
                            {
                                if (!LDOccupationCategory.Instance.Contain(dbcenterAccountInfo.RoleList[i].PlayerOcc))
                                {
                                    dbcenterAccountInfo.RoleList.RemoveAt(i);   
                                }
                            }

                            await Game.Scene.GetComponent<DBComponent>().Save(CommonConfig.CenterZoneId, dbcenterAccountInfo);
                        }

                        
                        //没有则注册
                        if (dbcenterAccountInfo == null)
                        {
                            /*response.NewRegister = 1;
                            reply();
                            session.Disconnect().Coroutine();
                            return;*/
                            await RegisterAccountHelper.RegisterAccount(session.DomainScene(), request.AccountName, request.Password,
                                request.LoginType, request.DeviceID);
                            
                            centerAccountInfoList = await Game.Scene.GetComponent<DBComponent>().Query<DBCenterAccountInfo>(session.DomainZone(), d => d.Account == request.AccountName && d.Password == request.Password);
                            dbcenterAccountInfo = centerAccountInfoList[0];
                        }
                       
                        if (sessionId != session.InstanceId || session.IsDisposed || session.DomainZone() == 0)
                        {
                            Log.Console($"session.IsDisposed.loginaccounthandler: {request.AccountName}");
                            response.Error = ErrorCode.ERR_LoginInfoIsNull;
                            reply();
                            session.Disconnect().Coroutine();
                            return;
                        }
                        
                        //if (dbcenterAccountInfo.DeviceID != request.DeviceID)
                        //{
                        //    Log.Console($"ErrorCode.ERR_LoginInfoExpire: {request.AccountName}");
                        //    response.Error = ErrorCode.ERR_LoginInfoExpire;
                        //    reply();
                        //    session.Disconnect().Coroutine();
                        //    return;
                        //}
                        
                        CenterServerComponent centerServerComponent = session.DomainScene().GetComponent<CenterServerComponent>();
                        bool IsHoliday = centerServerComponent.IsHoliday;
                        bool StopServer = centerServerComponent.StopServer;
                        
                        PlayerInfo centerPlayerInfo = dbcenterAccountInfo.PlayerInfo;
                        if (centerPlayerInfo.RealName == 0)
                        {
                            response.Error = ErrorCode.ERR_NotRealName;
                            response.AccountId = dbcenterAccountInfo.Id;
                            reply();
                            session.Disconnect().Coroutine();
                            return;
                        }
                        if (session.IsDisposed || session.DomainZone() == 0)
                        {
                            Log.Console($"session.IsDisposed.loginaccounthandler2: {request.AccountName}");
                            response.Error = ErrorCode.ERR_LoginInfoIsNull;
                            reply();
                            session.Disconnect().Coroutine();
                            return;
                        }
                        //if (!account.Password.Equals(request.Password))
                        //{
                        //    response.Error = ErrorCode.ERR_AccountOrPasswordError;
                        //    reply();
                        //    session.Disconnect().Coroutine();
                        //    account?.Dispose();
                        //    return;
                        //}
                        //防沉迷相关
                        string idCardNo = centerPlayerInfo.IdCardNo;
                        int canLogin = CanLogin(idCardNo, IsHoliday, request.LoginType);
                        if (canLogin != ErrorCode.ERR_Success && !ServerHelper.IsGoogleServer(session.DomainZone()))
                        {
                            response.Error = canLogin;
                            reply();
                            session.Disconnect().Coroutine();
                            return;
                        }

                        TokenComponent tokenComponent = session.DomainScene().GetComponent<TokenComponent>();
                        string queueToken = tokenComponent.Get(dbcenterAccountInfo.Id);

                     
                        //long onlineNumber = 10000;
                        //int maxNumber = GlobalValueConfigCategory.Instance.OnLineLimit;
                        //Log.Console($" {session.DomainZone()} ---  onlineNumber:{onlineNumber}");
                        //排队功能
                        
                        //请求登录中心服查询有没有同账号玩家登录[uwa]
                        //StartSceneConfig startSceneConfig = StartSceneConfigCategory.Instance.GetBySceneName(session.DomainZone(), "LoginCenter");
                        //long loginCenterInstanceId = startSceneConfig.InstanceId;
                        long loginCenterInstanceId = StartSceneConfigCategory.Instance.LoginCenterConfig.InstanceId;//踢掉进入gate的玩家
                        var loginAccountResponse = (L2A_LoginAccountResponse)await ActorMessageSenderComponent.Instance.Call(loginCenterInstanceId, new A2L_LoginAccountRequest() { AccountId = dbcenterAccountInfo.Id, Relink = request.Relink });

                        if (session.IsDisposed)
                        { 
                            return;
                        }

                        if (loginAccountResponse.Error != ErrorCode.ERR_Success)
                        {
                            response.Error = loginAccountResponse.Error;

                            reply();
                            session?.Disconnect().Coroutine();
                            dbcenterAccountInfo?.Dispose();
                            return;
                        }
                        //AccountSessionsComponent.Remove 需要在适当的时候移除
                        AccountSessionsComponent accountSessionsComponent = session.DomainScene().GetComponent<AccountSessionsComponent>();
                        long accountSessionInstanceId = accountSessionsComponent.Get(dbcenterAccountInfo.Id);
                        Session otherSession = Game.EventSystem.Get(accountSessionInstanceId) as Session;
                        if (otherSession != null)
                        {
                            Log.Debug($"LoginTest C2A_LoginAccount.ERR_OtherAccountLogin1 account.Id: {dbcenterAccountInfo.Id}");
                            otherSession?.Send(new A2C_Disconnect() { Error = ErrorCode.ERR_OtherAccountLogin });                 //踢accout服的玩家下线
                            otherSession?.Disconnect().Coroutine();
                        }
                      
                        accountSessionsComponent.Add(dbcenterAccountInfo.Id, session.InstanceId);
                        session.AddComponent<AccountCheckOutTimeComponent, long>(dbcenterAccountInfo.Id);   //自己在账号服只能停留600秒

                        string Token = TimeHelper.ServerNow().ToString() + RandomHelper.RandomNumber(int.MinValue, int.MaxValue).ToString();
                        tokenComponent.Remove(dbcenterAccountInfo.Id);    //Token也是保留十分钟
                        tokenComponent.Add(dbcenterAccountInfo.Id, Token);

                        response.RoleLists.Clear();
                        for (int i = 0; i < dbcenterAccountInfo.RoleList.Count; i++)
                        {
                            CreateRoleInfo createRoleInfo = dbcenterAccountInfo.RoleList[i];
                            if (createRoleInfo.ServerId != request.ServerId
                                || createRoleInfo.State == (int)RoleInfoState.Freeze)
                            {
                                continue;
                            }
                            if (!LDOccupationCategory.Instance.Contain(createRoleInfo.PlayerOcc))
                            {
                                continue;
                            }

                            CreateRoleInfo roleList = CloneHelper.ShallowClone(createRoleInfo);

                            RoleInfoComponentServer roleInfoComponentServer = await DBHelper.GetComponent<RoleInfoComponentServer>(createRoleInfo.ServerId,createRoleInfo.UserID);
                            if (roleInfoComponentServer == null)
                            {
                                response.RoleLists.Add(roleList);
                                continue;
                            }

                            NumericComponent numericComponent =await DBHelper.GetComponent<NumericComponent>(createRoleInfo.ServerId,createRoleInfo.UserID);
                            BagComponentServer bagComponentServer =await DBHelper.GetComponent<BagComponentServer>(createRoleInfo.ServerId,createRoleInfo.UserID);
                            if (numericComponent == null)
                            {
                                response.RoleLists.Add(roleList);
                                continue;
                            }
                            
                            roleList.PlayerLv = roleInfoComponentServer.RoleInfo.Lv;
                            roleList.WeaponId = numericComponent.GetAsInt(NumericType.Now_Weapon);
  
                            roleList.FashionIds = bagComponentServer.FashionEquipList;
                            
                            response.RoleLists.Add(roleList);
                        }
                      
                        response.RelinkRecord = CommonConfig.RelinkRecordUsers.Contains(request.AccountName) ? 1 : 0;
                        response.TodayCreateRole = dbcenterAccountInfo.TodayCreateRole();
                        response.TaprepRequest = dbcenterAccountInfo.TaprepRequest;
                        response.PlayerInfo = centerPlayerInfo;
                        response.AccountId = dbcenterAccountInfo.Id;
                        response.ClintFindPath = 1;
                        response.Token = Token;
                       
                        for (int r = 0; r < response.PlayerInfo.RechargeInfos.Count; r++) 
                        {
                            response.PlayerInfo.RechargeInfos[r].OrderInfo = String.Empty;
                        }
                        dbcenterAccountInfo?.Dispose();
                        reply();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
        }

        public int CanLogin(string identityCard, bool isHoliday, int thirdlogin)
        {
            if (thirdlogin == LoginTypeEnum.Google
                || thirdlogin == LoginTypeEnum.TikTok
                || thirdlogin == LoginTypeEnum.TikTokGuanFu)
            {
                return ErrorCode.ERR_Success; 
            }

            int age = IDCardHelper.GetBirthdayAgeSex(identityCard);
            if (age >= 18)
            {
                return ErrorCode.ERR_Success;
            }
            /*if (age < 12)
            {
                return ErrorCode.ERR_FangChengMi_Tip6;
            }*/
            DateTime dateTime = TimeHelper.DateTimeNow();
            if (isHoliday)
            {
                if (dateTime.Hour == 20)
                {
                    return ErrorCode.ERR_Success;           //允许登录
                }
                else
                {
                    return ErrorCode.ERR_FangChengMi_Tip7;
                }
            }
            else
            {
                return ErrorCode.ERR_FangChengMi_Tip7;
            }
        }

    }
}
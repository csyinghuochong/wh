using MongoDB.Bson.Serialization;
using System;
using System.Collections.Generic;


namespace ET
{

    [MessageHandler]
    public class C2R_TikTokVerifyUserHandler : AMRpcHandler<C2R_TikTokVerifyUser, R2C_TikTokVerifyUser>
    {
        protected override async ETTask Run(Session session, C2R_TikTokVerifyUser request, R2C_TikTokVerifyUser response, Action reply)
        {

            if (TikTokHelper.UseOldLogin)
            {
                long serverNow = TimeHelper.ServerNow() / 1000;
                Dictionary<string, string> paramslist = new Dictionary<string, string>();
                paramslist.Add("access_token", request.access_token);
                paramslist.Add("app_id", TikTokHelper.AppID.ToString());
                paramslist.Add("ts", serverNow.ToString());
                string sign = TikTokHelper.getSign(paramslist);
                paramslist.Add("sign", sign);

                string result = HttpHelper.OnWebRequestPost_TikTokLogin("https://usdk.dailygn.com/gsdk/usdk/account/verify_user", paramslist);

                if (ComHelp.IsInnerNet())
                {
                    result = "{\"code\":0,\"data\":{\"age_type\":100,\"log_id\":\"20231121162107BEDB3B3662AD2265532E\",\"sdk_open_id\":\"7303474616922905310\"},\"log_id\":\"20231121162107BEDB3B3662AD2265532E\",\"message\":\"success\"}";
                }

                TikTokCode tikTokCode = BsonSerializer.Deserialize<TikTokCode>(result);
                if (tikTokCode.code != 0 || tikTokCode.data == null)
                {
                    response.Error = tikTokCode.code;
                    response.sdk_open_id = string.Empty;
                    reply();
                    return;
                }
                else
                {
                    if (tikTokCode.data.age_type <= 0)
                    {
                        response.Error = ErrorCode.ERR_NotRealName;
                        reply();
                        return;
                    }

                    long accountZone = DBHelper.GetRealmCenter();
                    R2Other_CheckAccount centerAccount = (R2Other_CheckAccount)await ActorMessageSenderComponent.Instance.Call(accountZone, new Other2R_CheckAccount()
                    {
                        AccountName = tikTokCode.data.sdk_open_id,
                        Password = LoginTypeEnum.TikTok.ToString(),
                        LoginType = LoginTypeEnum.TikTok,
                    });

                    //没有则注册
                    if (centerAccount.PlayerInfo == null)
                    {
                        R2Other_RegisterAccount saveAccount = (R2Other_RegisterAccount)await ActorMessageSenderComponent.Instance.Call(accountZone, new Other2R_RegisterAccount()
                        {
                            AccountName = tikTokCode.data.sdk_open_id,
                            Password = LoginTypeEnum.TikTok.ToString(),
                            LoginType = LoginTypeEnum.TikTok,
                            age_type = tikTokCode.data.age_type,
                        });
                    }

                    response.sdk_open_id = tikTokCode.data.sdk_open_id;
                    response.age_type = tikTokCode.data.age_type;
                }
            }
            else
            {
                if (string.IsNullOrEmpty(request.sdk_open_id))
                {
                    response.Error = ErrorCode.ERR_LoginInfoIsNull;
                    reply();
                    return;
                }
                long accountZone = DBHelper.GetRealmCenter();
                R2Other_CheckAccount centerAccount = (R2Other_CheckAccount)await ActorMessageSenderComponent.Instance.Call(accountZone, new Other2R_CheckAccount()
                {
                    AccountName = request.sdk_open_id,
                    Password = LoginTypeEnum.TikTok.ToString(),
                    LoginType = LoginTypeEnum.TikTok,
                });

                //没有则注册
                if (centerAccount.PlayerInfo == null)
                {
                    R2Other_RegisterAccount saveAccount = (R2Other_RegisterAccount)await ActorMessageSenderComponent.Instance.Call(accountZone, new Other2R_RegisterAccount()
                    {
                        AccountName = request.sdk_open_id,
                        Password = LoginTypeEnum.TikTok.ToString(),
                        LoginType = LoginTypeEnum.TikTok,
                        age_type = request.age_type,
                    });
                }

                response.sdk_open_id = request.sdk_open_id;
                response.age_type = request.age_type;

            }
            //Log.Console($"C2A_TikTokVerifyUser sign: {sign}    result: {result}");
            //Log.Warning($"C2A_TikTokVerifyUser sign: {sign}    result: {result}");
            reply();
            await ETTask.CompletedTask;
        }
    }
}
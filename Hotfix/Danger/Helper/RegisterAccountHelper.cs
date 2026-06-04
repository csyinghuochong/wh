using System.Collections.Generic;

namespace ET
{
    public static class RegisterAccountHelper
    {
        public static async ETTask<long> RegisterAccount(Scene scene, string accountName, string password, int loginType, string deviceId)
        {
            accountName  = accountName.Trim().ToLower();
            password  = password.Trim().ToLower();
             using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.Register, accountName.GetHashCode()))
            {
            
                List<DBCenterAccountInfo> result = await Game.Scene.GetComponent<DBComponent>().Query<DBCenterAccountInfo>(scene.DomainZone(), _account => _account.Account == accountName);

                //如果查询数据不为空,表示当前账号已经被注册
                if (result.Count > 0)
                {
                    return result[0].Id;
                }

                //创建一条数据库信息,创建账号信息
                DBCenterAccountInfo newAccount = scene.AddChild<DBCenterAccountInfo>();
                newAccount.Account = accountName;
                newAccount.Password = password;
                newAccount.PlayerInfo = new PlayerInfo();
                newAccount.CreateTime = TimeHelper.ServerNow();
                newAccount.DeviceID = deviceId;

                //抖音账户 和 v20 直接实名

                if (loginType == LoginTypeEnum.Google
                    || loginType == LoginTypeEnum.QuDao)
                {
                    newAccount.PlayerInfo.Name = "loginType_" + loginType;
                    newAccount.PlayerInfo.RealName = 1;
                    newAccount.PlayerInfo.IdCardNo =  "429001199012282996";
                }
                else
                {
                    newAccount.PlayerInfo.Name ="loginType_" + loginType;
                    newAccount.PlayerInfo.RealName = 1;
                    newAccount.PlayerInfo.IdCardNo = "429001199012282996";
                }
                
                
                if (password == ComHelp.RobotPassWord)
                {
                    newAccount.PlayerInfo.RealName = 1;
                    newAccount.PlayerInfo.Name = accountName;
                    newAccount.PlayerInfo.IdCardNo = "429001198010232399";
                }
                
                //if (request.LoginType == LoginTypeEnum.TikTokGuanFu)
                //{
                //    newAccount.PlayerInfo.Name = "loginType_" + request.LoginType;
                //    newAccount.PlayerInfo.RealName = 1;
                //    newAccount.PlayerInfo.IdCardNo = string.Empty;
                //    //理论上不会到这 加个打印
                //    Console.WriteLine($"request.LoginType == LoginTypeEnum.TikTokGaunFu");
                //}

                //Log.Warning($"注册三方账号: {MongoHelper.ToJson(newAccount)}");
                await Game.Scene.GetComponent<DBComponent>().Save(scene.DomainZone(), newAccount);
                newAccount.Dispose();
                return newAccount.Id;
            }
        }
    }
}
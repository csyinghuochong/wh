using Alipay.AopSdk.Core.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace ET
{

    [Timer(TimerType.AccountCenterTimer)]
    public class AccountCenterTimer : ATimer<CenterServerComponent>
    {
        public override void Run(CenterServerComponent self)
        {
            try
            {
                self.OnCheck();
                self.SaveDB().Coroutine();
            }
            catch (Exception e)
            {
                Log.Error($"move timer error: {self.Id}\n{e}");
            }
        }
    }
    
    [ObjectSystem]
    public class AccountCenterComponentDestroy : DestroySystem<CenterServerComponent>
    {
        public override void Destroy(CenterServerComponent self)
        {
            TimerComponent.Instance.Remove(ref self.Timer);
        }
    }
    
    [ObjectSystem]
    public class CenterSceneComponentSystemAwakeSystem : AwakeSystem<CenterServerComponent>
    {
        public override void Awake(CenterServerComponent self)
        {
            self.StopServer = false;///  !ComHelp.IsInnerNet();
            self.CheckHoliday().Coroutine();
            
            self.UpdateServerInfo().Coroutine();
            self.InitDBRankInfo().Coroutine();
            self.UpdateTianQi();
            self.Timer = TimerComponent.Instance.NewRepeatedTimer(TimeHelper.Second, TimerType.AccountCenterTimer, self);
        }
    }

    public static class CenterServerComponentSystem
    {
        
        public static async ETTask CheckHoliday(this CenterServerComponent self)
        {
            DateTime dateTime = TimeHelper.DateTimeNow();
            self.IsHoliday = await HttpHelper.IsHolidayByDate(dateTime);
        }
        
        public static async ETTask UpdateServerInfo(this CenterServerComponent self)
        {
            // 临时读写用 new：Children 按 Id，会与 DBCenterSerialInfo 同 zone Id 冲突
            DBCenterServerInfo dBServerInfo = null;
            List<DBCenterServerInfo> result = await Game.Scene.GetComponent<DBComponent>().Query<DBCenterServerInfo>(self.DomainZone(), d => d.Id == self.DomainZone());
            if (result.Count == 0)
            {
                dBServerInfo = new DBCenterServerInfo() { Id = self.DomainZone() };
            }
            else
            {
                dBServerInfo = result[0];
            }

            await Game.Scene.GetComponent<DBComponent>().Save(self.DomainZone(), dBServerInfo);
            dBServerInfo.Dispose();
        }


        public static async ETTask UpdateWeeklyIndex(this CenterServerComponent self, System.DateTime dateTime)
        {
            List<DBCenterServerInfo> result = await Game.Scene.GetComponent<DBComponent>().Query<DBCenterServerInfo>(self.DomainZone(), d => d.Id == self.DomainZone());
            if (result.Count == 0)
            {
                return;
            }
            DBCenterServerInfo dBServerInfo = result[0];
            await Game.Scene.GetComponent<DBComponent>().Save(self.DomainZone(), dBServerInfo);
        }

        
          public static (int, int) GetSerialKeyId(this CenterServerComponent self, string serial)
        {
            DBCenterSerialInfo dBCenterSerialInfo = self.DBCenterSerialInfo;
            for (int i = 0; i < dBCenterSerialInfo.SerialList.Count; i++)
            {
                if (dBCenterSerialInfo.SerialList[i].Value != serial)
                {
                    continue;
                }

                return (dBCenterSerialInfo.SerialList[i].KeyId, int.Parse(dBCenterSerialInfo.SerialList[i].Value2));
            }
            return (0, 0);
        }

        public static int GetSerialReward(this CenterServerComponent self, string serial)
        {
            DBCenterSerialInfo dBCenterSerialInfo = self.DBCenterSerialInfo;
            for (int i = dBCenterSerialInfo.SerialList.Count - 1; i >= 0; i--)
            {
                if (dBCenterSerialInfo.SerialList[i].Value != serial)
                {
                    continue;
                }
                if (dBCenterSerialInfo.SerialList[i].Value2 == "1")
                {
                    return ErrorCode.ERR_AlreadyReceived;
                }

                dBCenterSerialInfo.SerialList[i].Value2 = "1";
                return ErrorCode.ERR_Success;
            }
            return ErrorCode.ERR_SerialNoExist;
        }

        public static async ETTask InitDBRankInfo(this CenterServerComponent self)
        {
            List<DBCenterSerialInfo> d2GGetUnit = await Game.Scene.GetComponent<DBComponent>().Query<DBCenterSerialInfo>(self.DomainZone(), _account => _account.Id == self.DomainZone());
            if (d2GGetUnit.Count == 0)
            {
                self.DBCenterSerialInfo = self.AddChildWithId<DBCenterSerialInfo>((long)self.DomainZone());
            }
            else
            {
                self.AddChild(d2GGetUnit[0]);
                self.DBCenterSerialInfo = d2GGetUnit[0];
            }

            self.SaveDB().Coroutine();
        }

        public static void UpdateTianQi(this CenterServerComponent self)
        {
            int[] rand = { 95, 4, 1 };
            int index = RandomHelper.RandomByWeight(rand);
            switch (index)
            {
                case 0:
                    self.TianQiValue = 0;
                    break;
                case 1:
                    self.TianQiValue = 1;
                    break;
                case 2:
                    self.TianQiValue = 2;
                    break;
            }
        }

        public static int GenerateSecureFourDigitNumber()
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                byte[] buffer = new byte[4];
                rng.GetBytes(buffer);
                int randomNumber = Math.Abs(BitConverter.ToInt32(buffer, 0));
                return randomNumber % 9000 + 1000;
            }
        }

        public static string GenerateVerification(this CenterServerComponent self, string phone)
        {
            if (self.PhoneVerification.ContainsKey(phone))
            {
                KeyValuePair<long, string> keyValuePair = self.PhoneVerification[phone];

                if (TimeHelper.ServerNow() - keyValuePair.Key < TimeHelper.Minute * 10)
                {
                    return keyValuePair.Value;
                }
                else
                {
                    self.PhoneVerification.Remove(phone);
                }
            }
            int secureNumber = GenerateSecureFourDigitNumber();
            self.PhoneVerification.Add(phone, new KeyValuePair<long, string>(TimeHelper.ServerNow(), secureNumber.ToString()));
            return secureNumber.ToString();
        }

        public static bool CheckVerification(this CenterServerComponent self, string phone, string code)
        {
            if (self.PhoneVerification.ContainsKey(phone))
            {
                KeyValuePair<long, string> keyValuePair = self.PhoneVerification[phone];

                if (TimeHelper.ServerNow() - keyValuePair.Key > TimeHelper.Minute * 10)
                {
                    return false;
                }

                return keyValuePair.Value.Equals(code);
            }

            return false;
        }

        public static void CheckSerials(this CenterServerComponent self)
        {
            Log.Warning("移除第七/八批序列号");
            DBCenterSerialInfo dBCenterSerialInfo = self.DBCenterSerialInfo;
            for (int i = dBCenterSerialInfo.SerialList.Count - 1; i >= 0; i--)
            {
                if (dBCenterSerialInfo.SerialList[i].KeyId == 7
                    || dBCenterSerialInfo.SerialList[i].KeyId == 8)
                {
                    dBCenterSerialInfo.SerialList.RemoveAt(i);  
                }
            }
            dBCenterSerialInfo.SerialIndex = 5;
        }

        public static void GenerateSerials(this CenterServerComponent self, int sindex)
        {
            DBCenterSerialInfo dBCenterSerialInfo = self.DBCenterSerialInfo;
            for (int i = dBCenterSerialInfo.SerialList.Count - 1; i >= 0; i--)
            {
                if (dBCenterSerialInfo.SerialList[i].KeyId == sindex)
                {
                    Log.Warning("生成序列号: 重复");
                    Console.WriteLine("生成序列号: 重复");
                    return;
                }
            }

            Console.WriteLine($"生成第{sindex}序列号: start");
            Log.Warning($"生成第{sindex}序列号: start");
            string codelist = string.Empty;
            self.DBCenterSerialInfo.SerialIndex = sindex;
            SerialHelper serialHelper = new SerialHelper();
            serialHelper.rep = sindex * 1000;  //累加.每次生成1000
            for (int i = 0; i < 1000; i++)
            {
                string code = serialHelper.GenerateCheckCode(6);
                dBCenterSerialInfo.SerialList.Add(new IntStringPair() { KeyId = sindex, Value = code, Value2 = "0" });
                codelist += code;
                codelist += "\r\n";
            }
            LogHelper.PaiMaiInfo(codelist);
            Log.Warning($"生成第{sindex}序列号: end");
            Console.WriteLine($"生成第{sindex}序列号: end");
        }

        public static void OnCheck(this CenterServerComponent self)
        {
            if (self.DBCenterSerialInfo == null)
            {
                return;
            }


            DateTime dateTime = TimeHelper.DateTimeNow();
            int hour = dateTime.Hour;
            if (self.DBCenterSerialInfo.LastHour == hour)
            {
                return;
              
            }
            self.DBCenterSerialInfo.LastHour = hour;

            self.CheckHoliday().Coroutine();


            if (hour == 21)
            {
                Console.WriteLine("savedb 0");
                Game.EventSystem.Publish(new EventType.GMCommonRequest() { Context = "savedb 0" });
            }

            if (hour == 3)
            {
                self.UpdateWeeklyIndex(TimeHelper.DateTimeNow()).Coroutine();
            }
            if (hour == -1)
            {
                self.UpdateWeeklyIndex(TimeInfo.Instance.ToDateTime(1767542401000)).Coroutine();
            }

            LogHelper.CheckLogSize();

            //self.TeamUpdateHandler().Coroutine();
        }

        public static async ETTask SaveDB(this CenterServerComponent self)
        {
            self.CheckIndex++;
            if (self.CheckIndex >=300)
            {
                self.CheckIndex = 0;
                await Game.Scene.GetComponent<DBComponent>().Save<DBCenterSerialInfo>(self.DomainZone(), self.DBCenterSerialInfo);
            }

   
            self.TianQITime++;
            if (self.TianQITime >= 12)
            {
                self.TianQITime = 0;
                self.UpdateTianQi();
            }
        }
    }
}
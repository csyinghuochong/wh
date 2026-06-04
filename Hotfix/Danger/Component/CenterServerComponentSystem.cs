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
            self.Timer = TimerComponent.Instance.NewRepeatedTimer(TimeHelper.Minute * 5 + self.DomainZone() * 800, TimerType.AccountCenterTimer, self);
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
            DBCenterServerInfo dBServerInfo = null;
            List<DBCenterServerInfo> result = await Game.Scene.GetComponent<DBComponent>().Query<DBCenterServerInfo>(self.DomainZone(), d => d.Id == self.DomainZone());
            if (result.Count == 0)
            {
                dBServerInfo = new DBCenterServerInfo();
                dBServerInfo.Id = self.DomainZone();
            }
            else
            {
                dBServerInfo = result[0];
            }

            if (dBServerInfo.V1ActivityList.Count == 0)
            {
                dBServerInfo.V1ActivityList = ActivityConfigHelper.RandomGenerateActivityList(0);
            }
            await Game.Scene.GetComponent<DBComponent>().Save(self.DomainZone(), dBServerInfo);

            await self.BroadcastActivityList(dBServerInfo);
        }

        public static async ETTask BroadcastActivityList(this CenterServerComponent self, DBCenterServerInfo dBServerInfo)
        {
            await TimerComponent.Instance.WaitAsync(TimeHelper.Second);

            Console.WriteLine($"BroadcastActivityList.WeeklyIndex:{dBServerInfo.WeeklyIndex}");

            List<StartProcessConfig> listprogress = StartProcessConfigCategory.Instance.GetAll().Values.ToList();
            for (int i = 0; i < listprogress.Count; i++)
            {
                List<StartSceneConfig> processScenes = StartSceneConfigCategory.Instance.GetByProcess(listprogress[i].Id);
                if (processScenes.Count == 0 || listprogress[i].Id == ComHelp.RobotProgress)  //机器人进程
                {
                    continue;
                }

                StartSceneConfig startSceneConfig = processScenes[0];
                long mapInstanceId = StartSceneConfigCategory.Instance.GetBySceneName(startSceneConfig.Zone, startSceneConfig.Name).InstanceId;
                A2R_Broadcast createUnit = (A2R_Broadcast)await ActorMessageSenderComponent.Instance.Call(
                    mapInstanceId, new R2A_Broadcast() { LoadType = 3, V1ActivityList = dBServerInfo.V1ActivityList });
            }
        }

        public static async ETTask UpdateWeeklyIndex(this CenterServerComponent self, System.DateTime dateTime)
        {
            List<DBCenterServerInfo> result = await Game.Scene.GetComponent<DBComponent>().Query<DBCenterServerInfo>(self.DomainZone(), d => d.Id == self.DomainZone());
            if (result.Count == 0)
            {
                return;
            }
            DBCenterServerInfo dBServerInfo = result[0];

            //每周刷新一次
            if (dateTime.DayOfWeek == System.DayOfWeek.Monday || dBServerInfo.V1ActivityList.Count == 0)
            {
                Console.WriteLine($"RandomGenerateActivityList.WeeklyIndex++:{dBServerInfo.WeeklyIndex}");
                dBServerInfo.WeeklyIndex++;
                if (dBServerInfo.WeeklyIndex >= 4)
                {
                    dBServerInfo.WeeklyIndex = 0;
                }

                dBServerInfo.V1ActivityList = ActivityConfigHelper.RandomGenerateActivityList(dBServerInfo.WeeklyIndex);
            }

            await Game.Scene.GetComponent<DBComponent>().Save(self.DomainZone(), dBServerInfo);
            await self.BroadcastActivityList(dBServerInfo);
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
                self.DBCenterSerialInfo = new DBCenterSerialInfo();
                self.DBCenterSerialInfo.Id = self.DomainZone();
            }
            else
            {
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
                dBCenterSerialInfo.SerialList.Add(new KeyValuePair() { KeyId = sindex, Value = code, Value2 = "0" });
                codelist += code;
                codelist += "\r\n";
            }
            LogHelper.PaiMaiInfo(codelist);
            Log.Warning($"生成第{sindex}序列号: end");
            Console.WriteLine($"生成第{sindex}序列号: end");
        }

        public static async ETTask SaveDB(this CenterServerComponent self)
        {
            await Game.Scene.GetComponent<DBComponent>().Save<DBCenterSerialInfo>(self.DomainZone(), self.DBCenterSerialInfo);

            self.TianQITime++;
            if (self.TianQITime >= 12)
            {
                self.TianQITime = 0;
                self.UpdateTianQi();


                List<int> zones = ServerMessageHelper.GetAllZone();
                for (int i = 0; i < zones.Count; i++)
                {
                    long chatServerId = StartSceneConfigCategory.Instance.GetBySceneName(zones[i], "Chat").InstanceId;
                    A2A_ServerMessageRResponse g_SendChatRequest = (A2A_ServerMessageRResponse)await ActorMessageSenderComponent.Instance.Call
                        (chatServerId, new A2A_ServerMessageRequest()
                        {
                            MessageType = NoticeType.TianQiChange,
                            MessageValue = self.TianQiValue.ToString(),
                        });

                    await TimerComponent.Instance.WaitAsync(10000);
                }
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;

namespace ET
{

    [ObjectSystem]
    public class DataCollationComponentAwake : AwakeSystem<DataCollationComponent>
    {
        public override void Awake(DataCollationComponent self)
        {
            self.CreateRoleTime = TimeHelper.DateTimeNow().ToString();
        }
    }


    public static class DataCollationComponentSystem
    {

        public static void Check(this DataCollationComponent self)
        {
            self.TotalOnLine++;

            Unit unit = self.GetParent<Unit>();
            self.TodayOnLine = unit.GetComponent<RoleInfoComponentServer>().TodayOnLine;
        }

        public static void OnXiLian(this DataCollationComponent self, int times)
        {
            self.XiLianTimes += times;

            if (times > 1)
            {
                self.DiamondXiLianTimes += times;
            }
        }

        public static void OnSceondHurt(this DataCollationComponent self, long hurtValue)
        {
            self.SceondHurt = hurtValue;
        }

        public static void OnChouKa(this DataCollationComponent self, int choukaType)
        {
            self.ChouKaTimes += choukaType;
        }

        public static void OnPetChouKa(this DataCollationComponent self, int choukaType)
        {
            self.PetChouKaTimes += choukaType;
        }

        public static void OnPetDuiHuan(this DataCollationComponent self)
        {
            self.PetDuiHuanTimes += 1;
        }

        public static void UpdateRoleMoneySub(this DataCollationComponent self, int Type, int getWay, long value)
        {
            if (value > 0)
            {
                return;
            }
            value *= -1;
            if (Type == UserDataType.Gold)
            {
                self.OnAddCostList(self.GoldCostList, getWay, value);
            }
            if (Type == UserDataType.Diamond)
            {
                self.OnAddCostList(self.DiamondCostList, getWay, value);
            }
        }

        public static void UpdateRoleMoneyAdd(this DataCollationComponent self, int Type, int getWay, long value)
        {
            if (value < 0)
            {
                Log.Warning($"UpdateRoleMoneyAdd<0 : {Type}  {value}");
                return;
            }
            if (Type == UserDataType.Gold)
            {
                self.OnAddCostList(self.GoldGetList, getWay, value);
            }
            if (Type == UserDataType.Diamond)
            {
                self.OnAddCostList(self.DiamondGetList, getWay, value);
            }
        }

        public static void OnDailyReset(this DataCollationComponent self, bool notice)
        {
            self.PaiMaiCostGoldToday = 0;
        }


        public static void UpdateBuySelfPlayerList(this DataCollationComponent self, long addgold, long unitid, long baginfoid, bool notice)
        {
            if (unitid == 0)
            {
                return;
            }
    
            self.PaiMaiGold += addgold;

            if (baginfoid > 0)
            {
                if (string.IsNullOrEmpty(self.SoldBagInfoID))
                {
                    self.SoldBagInfoID = baginfoid.ToString();
                }
                else
                {
                    self.SoldBagInfoID += $"&{baginfoid}";
                }
            }

            if (string.IsNullOrEmpty(self.BuySelfPlayer))
            {
                self.BuySelfPlayer = $"{unitid}&{addgold}";
            }
            else
            {
                self.BuySelfPlayer += $"_{unitid}&{addgold}";
            }
        }

        public static List<KeyValuePairLong> GetBuySelfPlayer(this DataCollationComponent self)
        {
            return null;
        }

        public static long GetCostByType(this DataCollationComponent self, int getWay)
        {
            if (string.IsNullOrEmpty(self.GoldCost))
            { 
                return 0; 
            }

            string[] costlist = self.GoldCost.Split('_');
            for (int i = 0; i < costlist.Length; i++)
            {
                string[] costinfo = costlist[i].Split(ConfigData.DataCollationSpit);
                if (costinfo.Length < 3)
                {
                    continue;
                }

                if (int.Parse(costinfo[0]) == getWay)
                {
                    long value = long.Parse(costinfo[2]);
                    return value;
                }
            }
            return 0;
        }


        public static long GetGoldByType(this DataCollationComponent self, int getWay)
        {
            if (string.IsNullOrEmpty(self.GoldGet))
            {
                return 0;
            }

            string[] costlist = self.GoldGet.Split('_');
            for (int i = 0; i < costlist.Length; i++)
            {
                string[] costinfo = costlist[i].Split(ConfigData.DataCollationSpit);
                if (costinfo.Length < 3)
                {
                    continue;
                }

                if (int.Parse(costinfo[0]) == getWay)
                {
                    long value = long.Parse(costinfo[2]);
                    return value;
                }
            }
            return 0;
        }

        public static void OnAddCostList(this DataCollationComponent self, List<KeyValuePairInt> pairInts, int getWay, long value)
        {
            bool have = false;
            for (int i = 0; i < pairInts.Count; i++)
            {
                if (pairInts[i].KeyId == getWay)
                {
                    have = true;
                    pairInts[i].Value += value;
                }
            }
            if (!have)
            {
                pairInts.Add(new KeyValuePairInt() { KeyId = getWay, Value = value });
            }
        }

        public static void SetAllCostList(this DataCollationComponent self, List<KeyValuePairInt> pairInts, string costValue)
        {
            if (string.IsNullOrEmpty(costValue))
            {
                return;
            }
            string[] costlist = costValue.Split('_');
            for (int i = 0; i < costlist.Length; i++)
            {
                string[] costinfo = costlist[i].Split(ConfigData.DataCollationSpit);
                if (costinfo.Length < 3)
                {
                    continue;
                }

                int getWay = int.Parse(costinfo[0]);    
                long value = long.Parse(costinfo[2]);
                self.OnAddCostList(pairInts, getWay, value);
            }

            pairInts.Sort((x, y) => x.KeyId.CompareTo(y.KeyId));
        }

        public static long GetGoldGetTotal(this DataCollationComponent self)
        {
            if (string.IsNullOrEmpty(self.GoldGet))
            {
                return 0;
            }
            long value = 0;
            string[] costlist = self.GoldGet.Split('_');
            for (int i = 0; i < costlist.Length; i++)
            {
                string[] costinfo = costlist[i].Split(ConfigData.DataCollationSpit);
                if (costinfo.Length < 3)
                {
                    continue;
                }

                value += long.Parse(costinfo[2]);

            }
            return value;
        }

        public static long GetGoldCostTotal(this DataCollationComponent self)
        {
            if (string.IsNullOrEmpty(self.GoldCost))
            {
                return 0;
            }
            long value = 0;
            string[] costlist = self.GoldCost.Split('_');
            for (int i = 0; i < costlist.Length; i++)
            {
                string[] costinfo = costlist[i].Split(ConfigData.DataCollationSpit);
                if (costinfo.Length < 3)
                {
                    continue;
                }

                int getWay = int.Parse(costinfo[0]);
                value += long.Parse(costinfo[2]);

            }
            return value;
        }

        public static string CostListToString(this DataCollationComponent self, List<KeyValuePairInt> pairInts)
        {
            string str = string.Empty;
            for (int i = 0; i < pairInts.Count; i++)
            {
            }
            return str;
        }

        public static void CorrectData(this DataCollationComponent self)
        { 
            if (!string.IsNullOrEmpty(self.GoldCost))
            {
                self.GoldCost = self.GoldCost.Replace(',', ConfigData.DataCollationSpit);
            }
            if (!string.IsNullOrEmpty(self.GoldGet))
            {
                self.GoldGet = self.GoldGet.Replace(',', ConfigData.DataCollationSpit);
            }
            if (!string.IsNullOrEmpty(self.DiamondCost))
            {
                self.DiamondCost = self.DiamondCost.Replace(',', ConfigData.DataCollationSpit);
            }
            if (!string.IsNullOrEmpty(self.DiamondGet))
            {
                self.DiamondGet = self.DiamondGet.Replace(',', ConfigData.DataCollationSpit);
            }
        }

        public static void UpdatePlatName(this DataCollationComponent self, int platform, int paltformtwo, int simulator, int  root, string deviceId, string unityversion, int bigversion, string deviceName, string oaid)
        {
            self.Simulator = simulator;
            self.IsRoot = root;
            self.DeviceID = deviceId;
            self.UnityVersion = unityversion;
            self.BigVersion = bigversion;
            self.OAID = oaid;   
            if (!string.IsNullOrEmpty(deviceName))
            {
                deviceName = deviceName.Replace(';', '&');
            }
            self.DeviceName = deviceName;

            string platformName = PlatformHelper.GetPlatformName(platform, paltformtwo);
            if (!string.IsNullOrEmpty(self.Platform) && !self.Platform.Contains('_'))
            {
                self.Platform = string.Empty;
            }
            if (!string.IsNullOrEmpty(self.Platform) && self.Platform.Contains(platformName))
            {
                return;
            }
            self.Platform += $"{platformName}: {TimeInfo.Instance.ToDateTime(TimeHelper.ServerNow()).ToString()}_";
        }

        public static void UpdateRegionCode(this DataCollationComponent self, int systemLanguage, string systemRegionCode, string byIPRegionCode, int downloadtype)
        {
            self.CurSystemLanguage = systemLanguage;
            self.CurSystemRegionCode = systemRegionCode;
            self.ByIPRegionCode = byIPRegionCode;
            self.DownloadType = downloadtype;   
        }


        public static string GetDeviceID(this DataCollationComponent self)
        { 
            string device = string.Empty;

            if (self.Simulator == 1)
            {
                device = "模拟器_";
            }
            else
            {
                device = "真机_";
            }

            if (self.IsRoot == 1)
            {
                device += "Root_";
            }

            device += self.DeviceID;
            return device;
        }

        public static void UpdateData(this DataCollationComponent self)
        {
            self.SetAllCostList(self.GoldCostList, self.GoldCost);
            self.GoldCost = self.CostListToString(self.GoldCostList);
            self.GoldCostList.Clear();

            self.SetAllCostList(self.GoldGetList, self.GoldGet);
            self.GoldGet = self.CostListToString(self.GoldGetList);
            self.GoldGetList.Clear();

            self.SetAllCostList(self.DiamondGetList, self.DiamondGet);
            self.DiamondGet = self.CostListToString(self.DiamondGetList);
            self.DiamondGetList.Clear();

            self.SetAllCostList(self.DiamondCostList, self.DiamondCost);
            self.DiamondCost = self.CostListToString(self.DiamondCostList);
            self.DiamondCostList.Clear();
        }

        public static void OnOffLine(this DataCollationComponent self, string lastgametime)
        {
            Unit unit = self.GetParent<Unit>();

            RoleInfoComponentServer roleInfoComponentServer = unit.GetComponent<RoleInfoComponentServer>();
            NumericComponent numericComponent = unit.GetComponent<NumericComponent>();
            PetComponentServer petComponentServer = unit.GetComponent<PetComponentServer>();  
            BagComponentServer bagComponentServer = unit.GetComponent<BagComponentServer>();  

            self.Name = roleInfoComponentServer.RoleInfo.Name;
            self.Level = roleInfoComponentServer.RoleInfo.Lv;
            self.Account = roleInfoComponentServer.Account;
            self.Password = roleInfoComponentServer.Password;
            self.Robot = unit.IsRobot() ?  1 : 0;

            self.CreateAccountTime = roleInfoComponentServer.RoleInfo.CreateTime;
            self.CreateAccountTimeStr = TimeInfo.Instance.ToDateTime(self.CreateAccountTime).ToString();

            self.OccId = roleInfoComponentServer.RoleInfo.Occ;

            self.Combat = roleInfoComponentServer.RoleInfo.Combat;

            self.Gold = roleInfoComponentServer.RoleInfo.Gold;    

            self.Diamond = roleInfoComponentServer.RoleInfo.Diamond;

            self.Recharge = numericComponent.GetAsLong( NumericType.RechargeNumber );

            self.TodayOnLine = roleInfoComponentServer.TodayOnLine;

            self.LastLoginTime = lastgametime;

            self.MainTask = unit.GetComponent<TaskComponentServer>().GetMainTaskId();   

            self.PetPingfen = petComponentServer.GetPingfenList();

            self.UnionName = roleInfoComponentServer.RoleInfo.UnionName;

            self.JiaYuanLv = unit.GetComponent<JiaYuanComponentServer>()?.JiaYuanLv ?? 1;

            self.JiaYuanFund = unit.GetComponent<JiaYuanComponentServer>()?.JiaYuanFund ?? 0;

            self.PetFubenId = petComponentServer.GetPassMaxFubenId();

            self.UpdateData();
        }
    }
}

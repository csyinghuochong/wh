using NLog.Fluent;
using SharpCompress.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ET
{

    //合区
    public static class MergeZoneHelper
    {


        public static async ETTask QueryTodayAccount()
        {
            var startZoneConfig = StartZoneConfigCategory.Instance.Get(CommonConfig.CenterZoneId);
            Game.Scene.GetComponent<DBComponent>().InitDatabase(startZoneConfig);

            long serverNow = TimeHelper.ServerNow();
            int todayNumber = CommonHelper.GetDayByTime(serverNow);  
            string tipinfo = string.Empty;  
            List<DBCenterAccountInfo> dBAccountInfos_new = await Game.Scene.GetComponent<DBComponent>().Query<DBCenterAccountInfo>(CommonConfig.CenterZoneId, d => d.Id > 0);
            foreach (var entity in dBAccountInfos_new)
            {
                if (entity.CreateTime == 0)
                {
                    continue;
                }

                int accountDay = CommonHelper.GetDayByTime(entity.CreateTime);
                if (todayNumber!= accountDay)
                {
                    continue;
                }

                if(entity.Password!="3" && entity.Password != "4")
                {
                    continue;

                }

                string head = entity.Account.Substring(0, 3);
                if (head == "170" || head == "171" || head == "162" || head == "165" || head == "167" || head == "192")
                {
                    tipinfo += $"{entity.Account} \n";
                }
            }

            LogHelper.PaiMaiInfo(tipinfo);
        }


        public static async ETTask QueryTaptapAccount()
        {
            var startZoneConfig = StartZoneConfigCategory.Instance.Get(CommonConfig.CenterZoneId);
            Game.Scene.GetComponent<DBComponent>().InitDatabase(startZoneConfig);

            long serverNow = TimeHelper.ServerNow();
            int todayNumber = CommonHelper.GetDayByTime(serverNow);
            string tipinfo = string.Empty;
            List<DBCenterAccountInfo> dBAccountInfos_new = await Game.Scene.GetComponent<DBComponent>().Query<DBCenterAccountInfo>(CommonConfig.CenterZoneId, d => d.Id > 0);
          
            Dictionary<int, long> DayCreateNumber = new Dictionary<int, long>();    
            
            foreach (var entity in dBAccountInfos_new)
            {
                if (entity.CreateTime == 0)
                {
                    continue;
                }

                int accountDay = CommonHelper.GetDayByTime(entity.CreateTime);
               
                if (entity.Password != "6" )
                {
                    continue;

                }

                if(!DayCreateNumber.ContainsKey(accountDay))
                {
                    DayCreateNumber.Add(accountDay, 0);
                }

                DayCreateNumber[accountDay]++;
            }

            var sortedDictionary = DayCreateNumber.OrderBy(kvp => kvp.Key).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            foreach (var item in sortedDictionary)
            {
                tipinfo += $"{item.Key}    {item.Value} \n";
            }

            LogHelper.PaiMaiInfo(tipinfo);
        }
        

        private static readonly Random Rand = new Random();

        /// <summary>
        /// 在 [startTimestamp, endTimestamp] 范围内随机生成一个毫秒级时间戳（本地时间体系）。
        /// 充值时段偏好：晚上 18:00~23:59 高概率，凌晨 0:00~5:59 极低概率，白天 6:00~17:59 较低概率。
        /// </summary>
        /// <param name="startTimestamp">起始毫秒时间戳（由 ClientNow 或 ServerNow 体系产生）</param>
        /// <param name="endTimestamp">结束毫秒时间戳</param>
        /// <returns>一个符合权重分布的毫秒时间戳</returns>
        public static long GenerateRechargeTimestamp(long startTimestamp, long endTimestamp)
        {
            if (startTimestamp > endTimestamp)
                throw new ArgumentException("起始时间不能晚于结束时间");

            // 转为本地 DateTime（已考虑东八区等 TimeZone 设置）
            DateTime startDt = TimeInfo.Instance.ToDateTime(startTimestamp);
            DateTime endDt   = TimeInfo.Instance.ToDateTime(endTimestamp);

            // 整个时间范围的毫秒跨度
            double totalMs = (endDt - startDt).TotalMilliseconds;

            const int maxAttempts = 10000;
            for (int i = 0; i < maxAttempts; i++)
            {
                // 1. 在范围内均匀随机选一个时间点（本地 DateTime）
                double offsetMs = Rand.NextDouble() * totalMs;
                DateTime candidate = startDt.AddMilliseconds(offsetMs);

                // 2. 根据本地小时决定接受概率
                int hour = candidate.Hour;
                double acceptProb;
                if (hour >= 18 && hour <= 23)
                    acceptProb = 1.0;        // 晚上高峰，必定接受
                else if (hour >= 0 && hour <= 5)
                    acceptProb = 0.05;       // 凌晨，极低概率
                else
                    acceptProb = 0.2;        // 白天，较低概率

                if (Rand.NextDouble() < acceptProb)
                {
                    // 3. 用 Transition 转回毫秒时间戳，与项目保持一致
                    return TimeInfo.Instance.Transition(candidate);
                }
            }

            // 极端情况（如整个范围都在凌晨且 maxAttempts 耗尽）退回均匀随机结果
            double fallbackOffset = Rand.NextDouble() * totalMs;
            DateTime fallback = startDt.AddMilliseconds(fallbackOffset);
            return TimeInfo.Instance.Transition(fallback);
        }
    

        // 方便传入 DateTime 的重载
        public static long GenerateRechargeTimestamp(DateTime start, DateTime end)
        {
            long startTs = new DateTimeOffset(start.ToUniversalTime()).ToUnixTimeSeconds();
            long endTs   = new DateTimeOffset(end.ToUniversalTime()).ToUnixTimeSeconds();
            return GenerateRechargeTimestamp(startTs, endTs);
        }

        // 直接返回 DateTime（ET 中常用）
        public static DateTime GenerateRechargeDateTime(long startTimestamp, long endTimestamp)
        {
            long ts = GenerateRechargeTimestamp(startTimestamp, endTimestamp);
            return DateTimeOffset.FromUnixTimeSeconds(ts).UtcDateTime;
        }
     
        /// <summary>
        /// 严格按精确概率版本（调整后总和100%）
        /// </summary>
        private static int GetRandomRechargeAmountPrecise()
        {
            // 精确权重，总和1000
            // 数据来源：基于手游付费玩家分布调研（2024-2025）
            // 参考：台湾市场44%玩家每月付费300-1000台币(约67-223元)[citation:2]
            //       72.1%玩家每月付费不超过1000元，其中300-500元占23.5%，500-1000元占22.5%[citation:5]
            //       日本市场89.5%玩家为低热层(月付费<1万日元/约484元)[citation:6]
    
            int random = RandomHelper.RandomNumber(0, 1000);
    
            // 底层玩家（约65-70%）- 首充/月卡/小额礼包
            if (random < 650) return 6;      // 65.0% - 对应每月300元以下群体[citation:5]
            if (random < 750) return 30;     // 10.0% - 月卡/战令基础档
    
            // 中层玩家（约15-20%）- 性价比礼包
            if (random < 830) return 50;     // 8.0%  - 对应每月300-500元群体[citation:5]
            if (random < 900) return 98;     // 7.0%  - 战令进阶/特权卡
    
            // 中高层玩家（约5-8%）- 进阶消费
            if (random < 940) return 198;    // 4.0%  - 对应每月500-1000元群体[citation:5]
            if (random < 965) return 298;    // 2.5%  - 中R进阶档
    
            // 高价值玩家（约3-5%）- 核心付费
            if (random < 982) return 488;    // 1.7%  - 对应每月1000元以上群体[citation:6]
            return 648;                      // 1.8%  - 大R核心档（日本市场核心层约3%）[citation:6]
        }

           
        /// <summary>
        /// 严格按精确概率版本（调整后总和100%）
        /// </summary>
        private static int GetRandomRechargeAmountPreciseByLevel(int level)
        {
            // 精确权重，总和1000
            // 数据来源：基于手游付费玩家分布调研（2024-2025）
            // 参考：台湾市场44%玩家每月付费300-1000台币(约67-223元)[citation:2]
            //       72.1%玩家每月付费不超过1000元，其中300-500元占23.5%，500-1000元占22.5%[citation:5]
            //       日本市场89.5%玩家为低热层(月付费<1万日元/约484元)[citation:6]
            
            int random = 0;
            if (level <= 10)
            {
                random = RandomHelper.RandomNumber(0, 830);
            }
            else if(level <= 40)
            {
                random = RandomHelper.RandomNumber(0, 900);
            }
            else
            {
                random = RandomHelper.RandomNumber(0, 1000);
            }

            // 底层玩家（约65-70%）- 首充/月卡/小额礼包
            if (random < 650) return 6;      // 65.0% - 对应每月300元以下群体[citation:5]
            if (random < 750) return 30;     // 10.0% - 月卡/战令基础档
    
            // 中层玩家（约15-20%）- 性价比礼包
            if (random < 830) return 50;     // 8.0%  - 对应每月300-500元群体[citation:5]
            if (random < 900) return 98;     // 7.0%  - 战令进阶/特权卡
    
            // 中高层玩家（约5-8%）- 进阶消费
            if (random < 940) return 198;    // 4.0%  - 对应每月500-1000元群体[citation:5]
            if (random < 965) return 298;    // 2.5%  - 中R进阶档
    
            // 高价值玩家（约3-5%）- 核心付费
            if (random < 982) return 488;    // 1.7%  - 对应每月1000元以上群体[citation:6]
            return 648;                      // 1.8%  - 大R核心档（日本市场核心层约3%）[citation:6]
        }
        
        
        public static async ETTask QueryRecharge_2()
        {

            ListComponent<int> mergezones = new ListComponent<int>() { 81, CommonConfig.CenterZoneId };
            for (int i = 0; i < mergezones.Count; i++)
            {
                var startZoneConfig = StartZoneConfigCategory.Instance.Get(mergezones[i]);
                Game.Scene.GetComponent<DBComponent>().InitDatabase(startZoneConfig);
            }


            List<DBCenterAccountInfo> dBAccountInfos_new = await Game.Scene.GetComponent<DBComponent>().Query<DBCenterAccountInfo>(CommonConfig.CenterZoneId, d => d.Id > 0);
            foreach (var entity in dBAccountInfos_new)
            {
                long sigleRecharge = 0;

                for (int i = 0; i < entity.PlayerInfo.RechargeInfos.Count; i++)
                {
                    //一月份
                    if (entity.PlayerInfo.RechargeInfos[i].UserId == 2283301304216387584
                        || entity.PlayerInfo.RechargeInfos[i].UserId == 2291096446520328192)
                    {
                        Log.Warning($"sigleRecharge > 50000: {entity.Account}");
                    }
                }

                if (sigleRecharge > 30000)
                {
                    Log.Warning($"sigleRecharge > 50000: {sigleRecharge}");
                }
            }
        }

        public static async ETTask QueryCard(string card)
        {
            var startZoneConfig = StartZoneConfigCategory.Instance.Get(CommonConfig.CenterZoneId);
            Game.Scene.GetComponent<DBComponent>().InitDatabase(startZoneConfig);
            List<DBCenterAccountInfo> dBAccountInfos_new = await Game.Scene.GetComponent<DBComponent>().Query<DBCenterAccountInfo>(CommonConfig.CenterZoneId, d => d.Id > 0);
            foreach (var entity in dBAccountInfos_new)
            {
                if (entity.PlayerInfo != null && entity.PlayerInfo.IdCardNo ==card)
                {
                    Log.Console(entity.Account);
                }
            }
        }

        public static async ETTask QueryOrderInfo(string dingdan)
        {
            var startZoneConfig = StartZoneConfigCategory.Instance.Get(CommonConfig.CenterZoneId);
            Game.Scene.GetComponent<DBComponent>().InitDatabase(startZoneConfig);
            List<DBCenterAccountInfo> dBAccountInfos_new = await Game.Scene.GetComponent<DBComponent>().Query<DBCenterAccountInfo>(CommonConfig.CenterZoneId, d => d.Id > 0);
            foreach (var entity in dBAccountInfos_new)
            {
                if (entity.PlayerInfo == null)
                {
                    continue;
                }
                List<RechargeInfo> rechargeInfos = entity.PlayerInfo.RechargeInfos;
                for (int i = 0; i < rechargeInfos.Count; i++)
                {
                    if (string.IsNullOrEmpty(rechargeInfos[i].OrderInfo))
                    {
                        continue;
                    }
                    if (rechargeInfos[i].OrderInfo.Equals(dingdan))
                    {
                        Console.WriteLine($"{entity.Account}");
                        Log.Warning($"{dingdan}   {entity.Account}");
                    }
                }
            }
        }

        public static async ETTask QueryGongzuoshi(int zone)
        {
            ListComponent<int> mergezones = new ListComponent<int>() { zone };
            for (int i = 0; i < mergezones.Count; i++)
            {
                var startZoneConfig = StartZoneConfigCategory.Instance.Get(mergezones[i]);
                Game.Scene.GetComponent<DBComponent>().InitDatabase(startZoneConfig);
            }


            Dictionary<string, List<long>> accountGold = new Dictionary<string, List<long>>();
            await ETTask.CompletedTask;
        }


        //查询被那个id购买过的记录
        public static async ETTask QueryGongzuoshi_2(int zone, long buyId)
        {
            ListComponent<int> mergezones = new ListComponent<int>() { zone };
            for (int i = 0; i < mergezones.Count; i++)
            {
                var startZoneConfig = StartZoneConfigCategory.Instance.Get(mergezones[i]);
                Game.Scene.GetComponent<DBComponent>().InitDatabase(startZoneConfig);
            }


            List<DataCollationComponent> dataCollationComponents = await Game.Scene.GetComponent<DBComponent>().Query<DataCollationComponent>(zone, d => d.Id > 0);
            for (int i = 0; i < dataCollationComponents.Count; i++)
            {
            }

        }

        public static async ETTask QueryGold(int zone)
        {
            ListComponent<int> mergezones = new ListComponent<int>() { zone };
            for (int i = 0; i < mergezones.Count; i++)
            {
                var startZoneConfig = StartZoneConfigCategory.Instance.Get(mergezones[i]);
                Game.Scene.GetComponent<DBComponent>().InitDatabase(startZoneConfig);
            }

            Dictionary<long, RoleInfoComponentServer> UserinfoComponetDict = new Dictionary<long, RoleInfoComponentServer>();
            List<RoleInfoComponentServer> RoleInfoComponents = await Game.Scene.GetComponent<DBComponent>().Query<RoleInfoComponentServer>(zone, d => d.Id > 0);
            foreach (var entity in RoleInfoComponents)
            {
                UserinfoComponetDict.Add(entity.Id, entity as RoleInfoComponentServer);
                if ((entity.RoleInfo.Gold > 1000000 || entity.RoleInfo.Diamond > 10000) && entity.RoleInfo.RobotId == 0)
                {
                   // Log.Warning($"Gold:{entity.RoleInfo.Gold}  Diamond:{entity.RoleInfo.Diamond}  ID:{entity.Id}  Account:{entity.Account} Name: {entity.RoleInfo.Name}  Level:{entity.RoleInfo.Level} ");
                }

                if (entity.RemoteAddress != null && entity.RemoteAddress.Contains("39.153.233.46"))
                {
                    //Log.Warning($"Gold:{entity.Id} ");
                }
               
                if (entity.RoleInfo.Name.Contains("南宫") || entity.RoleInfo.Name.Contains("世家"))
                {
                    //Log.Warning($"南宫:   {entity.Id}  {entity.RoleInfo.Level}\t  {entity.RoleInfo.Name}\t   {entity.RoleInfo.Combat}");
                }

                if (entity.RoleInfo.Combat < 0 || entity.RoleInfo.Combat > 10000000)
                {
                    //Log.Warning($"Combat < 0:   {entity.Id}  {entity.RoleInfo.Level}\t  {entity.RoleInfo.Name}\t   {entity.DeviceName}");
                }

                if (entity.RoleInfo.Occ == 3 && (entity.RoleInfo.Lv >= 22 ))
                {
                    Log.Warning($"Occ == 3:   {entity.Id}  \t{entity.RoleInfo.Lv}  \t{entity.RoleInfo.Name}   \t{entity.RoleInfo.Combat}");
                }
            }

            Dictionary<long, NumericComponent> NumericComponentDict = new Dictionary<long, NumericComponent>();
            List<NumericComponent> NumericComponents = await Game.Scene.GetComponent<DBComponent>().Query<NumericComponent>(zone, d => d.Id > 0);
            foreach (var entity in NumericComponents)
            {
                NumericComponentDict.Add(entity.Id, entity as NumericComponent);
            }

            List<PetComponentServer> petComponents = await Game.Scene.GetComponent<DBComponent>().Query<PetComponentServer>(zone, d => d.Id > 0);
            foreach (var entity in petComponents)
            {
                string shenshou = string.Empty;
                for (int pet = 0; pet < entity.RolePetInfos.Count; pet++)
                {
                    if (entity.RolePetInfos[pet].ConfigId == 2000001)
                    {
                        shenshou += "2000001 ";
                    }
                    if (entity.RolePetInfos[pet].ConfigId == 2000002)
                    {
                        shenshou += "2000002 ";
                    }
                    if (entity.RolePetInfos[pet].ConfigId == 2000003)
                    {
                        shenshou += "2000003 ";
                    }
                }

                if (string.IsNullOrEmpty(shenshou))
                {
                    continue;
                }

                RoleInfoComponentServer userInfo = UserinfoComponetDict[entity.Id];
                string servername = ServerHelper.GetGetServerItem(false, zone).ServerName;

                string userName = userInfo.RoleInfo.Name;
                int userlv = userInfo.RoleInfo.Lv;
                long recharget = NumericComponentDict[entity.Id].GetAsLong(NumericType.RechargeNumber);
                long diamond = userInfo.RoleInfo.Diamond;

                Log.Warning($"{servername} 玩家:{userName}  等级: {userlv}  充值额度:{recharget}  当前钻石{diamond}  拥有神兽:{shenshou}");
            }
        }

        public static async ETTask QueryAccount(int newzone, long userid)
        {
            ListComponent<int> mergezones = new ListComponent<int>() { newzone };
            for (int i = 0; i < mergezones.Count; i++)
            {
                var startZoneConfig = StartZoneConfigCategory.Instance.Get(mergezones[i]);
                Game.Scene.GetComponent<DBComponent>().InitDatabase(startZoneConfig);
            }

            await ETTask.CompletedTask;
        }

        public static async ETTask MergeZoneUnion(int oldzone, int newzone)
        {
            ListComponent<int> mergezones = new ListComponent<int>() { oldzone, newzone };
            for (int i = 0; i < mergezones.Count; i++)
            {
                var startZoneConfig = StartZoneConfigCategory.Instance.Get(mergezones[i]);
                Game.Scene.GetComponent<DBComponent>().InitDatabase(startZoneConfig);
            }

            List<DBUnionInfo> dBUnionInfo_new = await Game.Scene.GetComponent<DBComponent>().Query<DBUnionInfo>(oldzone, d => d.Id > 0);
            foreach (var entity in dBUnionInfo_new)
            {
                Log.Console($"合并家族: {newzone} {entity.Id}");
                await Game.Scene.GetComponent<DBComponent>().Save(newzone, entity);
            }

            List<DBUnionManager> DBUnionManager_new = await Game.Scene.GetComponent<DBComponent>().Query<DBUnionManager>(newzone, d => d.Id == newzone);
            List<DBUnionManager> DBUnionManager_old = await Game.Scene.GetComponent<DBComponent>().Query<DBUnionManager>(newzone, d => d.Id == oldzone);
            if (DBUnionManager_new.Count >= 0)
            {

            }
            Log.Console($"合并家族完成！:");
        }

        //Parameters=31_30   31区合并到30区
        public static async ETTask MergeZone(int oldzone, int newzone)
        {
            ListComponent<int> mergezones = new ListComponent<int>() { oldzone, newzone };
            for (int i = 0; i < mergezones.Count; i++)
            {
                var startZoneConfig = StartZoneConfigCategory.Instance.Get(mergezones[i]);
                Game.Scene.GetComponent<DBComponent>().InitDatabase(startZoneConfig);
            }

            //同时满足以下规则,数据将被清理
            //1.未充值
            //2.角色20级以内
            //3.10天内未登陆游戏
            long serverNow = TimeHelper.ServerNow();
            List<long> invalidPlayers = new List<long>();

            ///记录玩家等级
            ///Parameters=31_30   31区合并到30区   oldzone合并到newzone
            Dictionary<long, int> userLevel = new Dictionary<long, int>();
            List<RoleInfoComponentServer> oldRoleInfoComponents_0 = await Game.Scene.GetComponent<DBComponent>().Query<RoleInfoComponentServer>(oldzone, d => d.Id > 0);

            int validLv = 20;
            if (oldRoleInfoComponents_0.Count > 40000)
            {
                validLv = 25;
            }

            foreach (var oldentity in oldRoleInfoComponents_0)
            {
                if (!userLevel.ContainsKey(oldentity.Id))
                {
                    userLevel.Add(oldentity.Id, oldentity.RoleInfo.Lv);
                }

                if (oldentity.RoleInfo.RobotId > 0)
                {
                    invalidPlayers.Add(oldentity.Id);
                    continue;
                }

                if (oldentity.RoleInfo.Lv >= validLv)
                {
                    continue;
                }
                if (serverNow - oldentity.LastLoginTime < TimeHelper.OneDay * 10)
                {
                    continue;
                }
                List<NumericComponent> numericComponentlist = await Game.Scene.GetComponent<DBComponent>().Query<NumericComponent>(oldzone, d => d.Id == oldentity.Id);
                if (numericComponentlist == null || numericComponentlist.Count == 0)
                {
                    continue;
                }
                if (numericComponentlist[0].GetAsLong(NumericType.RechargeNumber) > 0)
                {
                    continue;
                }
                
                invalidPlayers.Add( oldentity.Id );
                //Log.Console($"移除玩家： {oldentity.RoleInfo.Name}  {oldentity.RoleInfo.Level}   {numericComponentlist[0].GetAsLong(NumericType.RechargeNumber)}  {TimeInfo.Instance.ToDateTime(numericComponentlist[0].GetAsLong(NumericType.LastGameTime)).ToString()}");
                //Log.Warning($"移除玩家： {oldentity.RoleInfo.Name}  {oldentity.RoleInfo.Level}   {numericComponentlist[0].GetAsLong(NumericType.RechargeNumber)}  {TimeInfo.Instance.ToDateTime(numericComponentlist[0].GetAsLong(NumericType.LastGameTime)).ToString()}");
            }
            Log.Console($"不参与合区的玩家数量 {invalidPlayers.Count}");

            //ActivityComponentServer
            List<ActivityComponentServer> activityComponents = await Game.Scene.GetComponent<DBComponent>().Query<ActivityComponentServer>(oldzone, d => d.Id > 0);
            long dbcount = 0;
            int onecount = 1000;
            foreach (var entity in activityComponents)
            {
                if (invalidPlayers.Contains(entity.Id))
                {
                    continue;
                }

                dbcount++;
                if (dbcount % onecount == 0)
                {
                    await TimerComponent.Instance.WaitFrameAsync();
                }
                await Game.Scene.GetComponent<DBComponent>().Save(newzone, entity);
            }
            Log.Console("ActivityComponentServer Complelte");

            //BagComponentServer
            dbcount = 0;
            List<BagComponentServer> bagComponents = await Game.Scene.GetComponent<DBComponent>().Query<BagComponentServer>(oldzone, d => d.Id > 0);
            foreach (var entity in bagComponents)
            {
                if (invalidPlayers.Contains(entity.Id))
                {
                    continue;
                }
                dbcount++;
                if (dbcount % onecount == 0)
                {
                    await TimerComponent.Instance.WaitFrameAsync();
                }
                await Game.Scene.GetComponent<DBComponent>().Save(newzone, entity);
            }
            await TimerComponent.Instance.WaitFrameAsync();
            Log.Console("BagComponentServer Complelte");
            //ChengJiuComponen
            dbcount = 0;
            List<ChengJiuComponentServer> chengJiuComponents = await Game.Scene.GetComponent<DBComponent>().Query<ChengJiuComponentServer>(oldzone, d => d.Id > 0);
            foreach (var entity in chengJiuComponents)
            {
                if (invalidPlayers.Contains(entity.Id))
                {
                    continue;
                }
                dbcount++;
                if (dbcount % onecount == 0)
                {
                    await TimerComponent.Instance.WaitFrameAsync();
                }
                await Game.Scene.GetComponent<DBComponent>().Save(newzone, entity);
            }
            Log.Console("ChengJiuComponent Complelte");
            //DBAccountInfo.  问清楚规则 不能全部合并
            dbcount = 0;
            /*List<DBAccountBagInfo> dBAccountInfos_old = await Game.Scene.GetComponent<DBComponent>().Query<DBAccountBagInfo>(oldzone, d => d.Id > 0);
            List<DBAccountBagInfo> dBAccountInfos_new = await Game.Scene.GetComponent<DBComponent>().Query<DBAccountBagInfo>(newzone, d => d.Id > 0);
            foreach (var entity in dBAccountInfos_old)
            {
              
                dbcount++;
                if (dbcount % onecount == 0)
                {
                    await TimerComponent.Instance.WaitFrameAsync();
                }

                List<DBAccountBagInfo> dBAccountInfos = await Game.Scene.GetComponent<DBComponent>().Query<DBAccountBagInfo>(newzone, d => d.Id == entity.Id);
                if (dBAccountInfos.Count > 0)
                {
                    if (entity.BagInfoList.Count > 0 && dBAccountInfos[0].HaveItemById(entity.BagInfoList[0].BagInfoID) < 0)
                    {
                        dBAccountInfos[0].BagInfoList.AddRange(entity.BagInfoList);
                        await Game.Scene.GetComponent<DBComponent>().Save(newzone, dBAccountInfos[0]);
                    }
                }
                else
                {
                    await Game.Scene.GetComponent<DBComponent>().Save(newzone, entity);
                }
            }*/
            Log.Console("DBAccountBagInfo Complelte");

            //DBDayActivityInfo  活动相关也要特殊处理
            List<DBDayActivityInfo> dBDayActivityInfos_old = await Game.Scene.GetComponent<DBComponent>().Query<DBDayActivityInfo>(oldzone, d => d.Id > 0);
            List<DBDayActivityInfo> dBDayActivityInfos_new = await Game.Scene.GetComponent<DBComponent>().Query<DBDayActivityInfo>(newzone, d => d.Id > 0);
            foreach (var newentity in dBDayActivityInfos_new)
            {
                if (newentity.Id != newzone)
                {
                    continue;
                }

                newentity.AddGuessPlayerList(dBDayActivityInfos_old[0].GuessPlayerList);
                newentity.AddGuessRewardList(dBDayActivityInfos_old[0].GuessRewardList);
                await Game.Scene.GetComponent<DBComponent>().Save(newzone, newentity);
            }

            //DBFriendInfo
            dbcount = 0;
            List<DBFriendInfo> dBFriendInfos = await Game.Scene.GetComponent<DBComponent>().Query<DBFriendInfo>(oldzone, d => d.Id > 0);
            foreach (var entity in dBFriendInfos)
            {
                if (invalidPlayers.Contains(entity.Id))
                {
                    continue;
                }

                dbcount++;
                if (dbcount % onecount == 0)
                {
                    await TimerComponent.Instance.WaitFrameAsync();
                }
                await Game.Scene.GetComponent<DBComponent>().Save(newzone, entity);
            }
            Log.Console("DBFriendInfo Complelte");

            //DBMailInfo 邮件
            dbcount = 0;
            List<DBMailInfo> dBMailInfos = await Game.Scene.GetComponent<DBComponent>().Query<DBMailInfo>(oldzone, d => d.Id > 0);
            foreach (var entity in dBMailInfos)
            {
                if (invalidPlayers.Contains(entity.Id))
                {
                    continue;
                }

                dbcount++;
                if (dbcount % onecount == 0)
                {
                    await TimerComponent.Instance.WaitFrameAsync();
                }
                int lv = 0;
                userLevel.TryGetValue(entity.Id, out lv);

                List<BagInfo> rewardlist = CommonConfig.GetHeQuReward(lv);
                if (rewardlist!=null && rewardlist.Count > 0)
                {
                    Log.Error("MailInfo mailInfo = new MailInfo");
                    //MailInfo mailInfo = new MailInfo();
                    //mailInfo.Status = 0;
                    //mailInfo.Context = "合区补偿";
                    //mailInfo.Title = "合区补偿";
                    //mailInfo.MailId = IdGenerater.Instance.GenerateId();
                    //mailInfo.ItemList.AddRange(rewardlist);
                    //entity.MailInfoList.Add(mailInfo);
                }

                await Game.Scene.GetComponent<DBComponent>().Save(newzone, entity);
            }

            Log.Console("DBMailInfo Complelte");

            //DBPaiMainInfo 拍卖，也合并过来，要着重测试
            //List<DBPaiMainInfo> dBPaiMainInfos_old = await Game.Scene.GetComponent<DBComponent>().Query<DBPaiMainInfo>(oldzone, d => d.Id > 0);
            List<DBConsignInfo> dBPaiMainInfos_new = await Game.Scene.GetComponent<DBComponent>().Query<DBConsignInfo>(newzone, d => d.Id > 0);
            List<long> paimaishangjiaIds = new List<long>() 
            {
                ConsignHelper.GetPaiMaiId(1),
                ConsignHelper.GetPaiMaiId(2),
                ConsignHelper.GetPaiMaiId(3),
                ConsignHelper.GetPaiMaiId(4),
            };
            foreach (var entityNew in dBPaiMainInfos_new)
            {
                if (!paimaishangjiaIds.Contains( entityNew.Id) )
                {
                    continue;
                }
                bool have = false;
                List<DBConsignInfo> dBPaiMainInfos_old = await Game.Scene.GetComponent<DBComponent>().Query<DBConsignInfo>(oldzone, d => d.Id == entityNew.Id);
                if (dBPaiMainInfos_old == null || dBPaiMainInfos_old.Count == 0)
                {
                    continue;
                }
                List<ConsignItemInfo> oldlist_0 = dBPaiMainInfos_old[0].PaiMaiItemInfos;
                if (oldlist_0.Count > 0)
                {
                    for (int i = 0; i < entityNew.PaiMaiItemInfos.Count; i++)
                    {
                        if (entityNew.PaiMaiItemInfos[i].Id == oldlist_0[0].Id)
                        {
                            have = true;
                            break;
                        }
                    }
                }
                if (!have)
                {
                    entityNew.PaiMaiItemInfos.AddRange(oldlist_0);
                }

                await Game.Scene.GetComponent<DBComponent>().Save(newzone, entityNew);
            }

            dbcount = 0;
            List<DBPopularizeInfo> dBPopularizeInfos = await Game.Scene.GetComponent<DBComponent>().Query<DBPopularizeInfo>(oldzone, d => d.Id > 0);
            foreach (var entity in dBPopularizeInfos)
            {
                if (invalidPlayers.Contains(entity.Id))
                {
                    continue;
                }

                dbcount++;
                if (dbcount % onecount == 0)
                {
                    await TimerComponent.Instance.WaitFrameAsync();
                }
                await Game.Scene.GetComponent<DBComponent>().Save(newzone, entity);
            }
            Log.Console("DBPopularizeInfo Complelte");

            //DBRankInfo 排行榜  。 
            List<DBRankInfo> dBRankInfos_old = await Game.Scene.GetComponent<DBComponent>().Query<DBRankInfo>(oldzone, d => d.Id == (long)oldzone);
            List<DBRankInfo> dBRankInfos_new = await Game.Scene.GetComponent<DBComponent>().Query<DBRankInfo>(newzone, d => d.Id == (long)newzone);
            if(dBRankInfos_old.Count > 0 && dBRankInfos_new.Count > 0)
            {
                DBRankInfo entity = dBRankInfos_new[0];

                List<RankingInfo> rankingInfos_new = entity.rankingInfos;
                List<RankingInfo> rankingInfos_old = dBRankInfos_old[0].rankingInfos;

                bool havemerge = false; 
                for (int i = 0; i < rankingInfos_new.Count; i++)
                {
                    for (int j = 0; j < rankingInfos_old.Count; j++ )
                    {
                        if (rankingInfos_new[i].UserId == rankingInfos_old[j].UserId)
                        {
                            havemerge = true;
                            break;
                        }
                    }
                }
                if (havemerge)
                {
                    Log.Console($"排行榜已合并！");
                }
                else
                {
                    rankingInfos_new.AddRange(rankingInfos_old);
                    rankingInfos_new.Sort(delegate (RankingInfo a, RankingInfo b)
                    {
                        return (int)b.Combat - (int)a.Combat;
                    });
                    int maxnumber = Math.Min(500, rankingInfos_new.Count);
                    entity.rankingInfos = rankingInfos_new.GetRange(0, maxnumber);

                    List<KeyValuePairLong> rankingTrial_new = entity.rankingTrial;
                    List<KeyValuePairLong> rankingTrial_old = dBRankInfos_old[0].rankingTrial;
                    rankingTrial_new.AddRange(rankingTrial_old);
                    rankingTrial_new.Sort(delegate (KeyValuePairLong a, KeyValuePairLong b)
                    {
                        if (b.Value2 == a.Value2)
                        {
                            return (int)b.Value2 - (int)a.Value2;
                        }
                        else
                        {
                            return (int)b.Value - (int)a.Value;
                        }
                    });
                    maxnumber = Math.Min(rankingTrial_new.Count, 100);
                    entity.rankingTrial = rankingTrial_new.GetRange(0, maxnumber);

                    List<KeyValuePairLong> rankSeasonTower_new = entity.rankSeasonTower;
                    List<KeyValuePairLong> rankSeasonTower_old = dBRankInfos_old[0].rankSeasonTower;
                    rankSeasonTower_new.AddRange(rankSeasonTower_old);
                    rankSeasonTower_new.Sort(delegate (KeyValuePairLong a, KeyValuePairLong b)
                    {
                        if (b.Value2 == a.Value2)
                        {
                            return (int)a.Value - (int)b.Value;
                        }
                        else
                        {
                            return (int)b.Value2 - (int)a.Value2;
                        }
                    });
                    maxnumber = Math.Min(rankSeasonTower_new.Count, 100);
                    entity.rankSeasonTower = rankSeasonTower_new.GetRange(0, maxnumber);

                    //阵营相关的都要重置
                    await Game.Scene.GetComponent<DBComponent>().Save(newzone, entity);
                }
            }

            //DBServerInfo   服务器的一些公用内容
            List<DBServerInfo> dBServerInfos_old = await Game.Scene.GetComponent<DBComponent>().Query<DBServerInfo>(oldzone, d => d.Id > 0);
            List<DBServerInfo> dBServerInfos_new = await Game.Scene.GetComponent<DBComponent>().Query<DBServerInfo>(newzone, d => d.Id > 0);
            foreach (var entity in dBServerInfos_new)
            {
                if (entity.Id != newzone)
                {
                    continue;
                }
                await Game.Scene.GetComponent<DBComponent>().Save(newzone, entity);
            }

            ///全服邮件(不需要处理)
            ///DBServerMailInfo

            List<DBUnionInfo> dBUnionInfo_old = await Game.Scene.GetComponent<DBComponent>().Query<DBUnionInfo>(oldzone, d => d.Id > 0);
            foreach (var entity in dBUnionInfo_old)
            {
                await Game.Scene.GetComponent<DBComponent>().Save(newzone, entity);
            }
            Log.Console($"DBUnionInfo Complelte");

            //合并捐献总金额
            List<DBUnionManager> dBUnionManager_old = await Game.Scene.GetComponent<DBComponent>().Query<DBUnionManager>(oldzone, d => d.Id == (long)oldzone);
            List<DBUnionManager> dBUnionManager_new = await Game.Scene.GetComponent<DBComponent>().Query<DBUnionManager>(newzone, d => d.Id == (long)newzone);
            if (dBUnionManager_old.Count > 0 && dBUnionManager_new.Count > 0)
            {
                DBUnionManager oldentity = dBUnionManager_old[0];
                DBUnionManager newentity = dBUnionManager_new[0];

                Log.Console($"合并家族捐献资金: {oldentity.TotalDonation} {newentity.TotalDonation}");
                newentity.TotalDonation += oldentity.TotalDonation;

                if(oldentity.SignupUnions.Count > 0 && !newentity.SignupUnions.Contains(oldentity.SignupUnions[0]))
                {
                    Log.Console($"合并家族战报名列表: {oldentity.SignupUnions[0]}");
                    newentity.SignupUnions.AddRange(oldentity.SignupUnions);

                    List<RankingInfo> rankingDonation_old = oldentity.rankingDonation;
                    List<RankingInfo> rankingDonation_new = newentity.rankingDonation;

                    rankingDonation_new.AddRange(rankingDonation_old);
                    rankingDonation_new.Sort(delegate (RankingInfo a, RankingInfo b)
                    {
                        return (int)b.Combat - (int)a.Combat;
                    });
                    int number = Math.Min(rankingDonation_new.Count, 20);
                    rankingDonation_new = rankingDonation_new.GetRange(0, number);
                    newentity.rankingDonation = rankingDonation_new;
                }

                await Game.Scene.GetComponent<DBComponent>().Save(newzone, newentity);
            }
            dbcount = 0;

            try
            {
                List<DataCollationComponent> datacollationComponents = await Game.Scene.GetComponent<DBComponent>().Query<DataCollationComponent>(oldzone, d => d.Id > 0);
                foreach (var entity in datacollationComponents)
                {
                    if (invalidPlayers.Contains(entity.Id))
                    {
                        continue;
                    }

                    dbcount++;
                    if (dbcount % onecount == 0)
                    {
                        await TimerComponent.Instance.WaitFrameAsync();
                    }
                    await Game.Scene.GetComponent<DBComponent>().Save(newzone, entity);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
            Log.Console("DataCollationComponent Complelte");

            //EnergyComponent 正能量组件
            
            dbcount = 0;
            List<JiaYuanComponentServer> jiayuanComponents = await Game.Scene.GetComponent<DBComponent>().Query<JiaYuanComponentServer>(oldzone, d => d.Id > 0);
            foreach (var entity in jiayuanComponents)
            {
                if (invalidPlayers.Contains(entity.Id))
                {
                    continue;
                }

                dbcount++;
                if (dbcount % onecount == 0)
                {
                    await TimerComponent.Instance.WaitFrameAsync();
                }
                await Game.Scene.GetComponent<DBComponent>().Save(newzone, entity);
            }
            Log.Console("JiaYuanComponent Complelte");

            //NumericComponent  数值组件
            dbcount = 0;
            List<NumericComponent> numericComponents = await Game.Scene.GetComponent<DBComponent>().Query<NumericComponent>(oldzone, d => d.Id > 0);
            foreach (var entity in numericComponents)
            {
                if (invalidPlayers.Contains(entity.Id))
                {
                    continue;
                }
                dbcount++;
                if (dbcount % onecount == 0)
                {
                    await TimerComponent.Instance.WaitFrameAsync();
                }
                await Game.Scene.GetComponent<DBComponent>().Save(newzone, entity);
            }
            Log.Console("NumericComponent Complelte");

            //PetComponent  宠物组件
            dbcount = 0;
            List<PetComponentServer> petComponents = await Game.Scene.GetComponent<DBComponent>().Query<PetComponentServer>(oldzone, d => d.Id > 0);
            foreach (var entity in petComponents)
            {
                if (invalidPlayers.Contains(entity.Id))
                {
                    continue;
                }
                dbcount++;
                if (dbcount % onecount == 0)
                {
                    await TimerComponent.Instance.WaitFrameAsync();
                }
                await Game.Scene.GetComponent<DBComponent>().Save(newzone, entity);
            }
            Log.Console("PetComponent Complelte");

            //RechargeComponent  充值记录组件
            dbcount = 0;
            List<RechargeComponentServer> rechargeComponents = await Game.Scene.GetComponent<DBComponent>().Query<RechargeComponentServer>(oldzone, d => d.Id > 0);
            foreach (var entity in rechargeComponents)
            {
                if (invalidPlayers.Contains(entity.Id))
                {
                    continue;
                }
                dbcount++;
                if (dbcount % onecount == 0)
                {
                    await TimerComponent.Instance.WaitFrameAsync();
                }
                await Game.Scene.GetComponent<DBComponent>().Save(newzone, entity);
            }
            Log.Console("DBPopularizeInfo Complelte");
            //ReddotComponent  红点组件
            dbcount = 0;
            List<ReddotComponentServer> reddotComponents = await Game.Scene.GetComponent<DBComponent>().Query<ReddotComponentServer>(oldzone, d => d.Id > 0);
            foreach (var entity in reddotComponents)
            {
                if (invalidPlayers.Contains(entity.Id))
                {
                    continue;
                }
                dbcount++;
                if (dbcount % onecount == 0)
                {
                    await TimerComponent.Instance.WaitFrameAsync();
                }
                await Game.Scene.GetComponent<DBComponent>().Save(newzone, entity);
            }
            Log.Console("ReddotComponent Complelte");


            //ShoujiComponent  收集大厅
            dbcount = 0;
            List<ShoujiComponentServer> shoujiComponents = await Game.Scene.GetComponent<DBComponent>().Query<ShoujiComponentServer>(oldzone, d => d.Id > 0);
            foreach (var entity in shoujiComponents)
            {
                if (invalidPlayers.Contains(entity.Id))
                {
                    continue;
                }
                dbcount++;
                if (dbcount % onecount == 0)
                {
                    await TimerComponent.Instance.WaitFrameAsync();
                }
                await Game.Scene.GetComponent<DBComponent>().Save(newzone, entity);
            }
            Log.Console("ShoujiComponent Complelte");

            //SkillSetComponent  技能
            dbcount = 0;
            List<SkillSetComponentServer> skillSetComponents = await Game.Scene.GetComponent<DBComponent>().Query<SkillSetComponentServer>(oldzone, d => d.Id > 0);
            foreach (var entity in skillSetComponents)
            {
                if (invalidPlayers.Contains(entity.Id))
                {
                    continue;
                }
                dbcount++;
                if (dbcount % onecount == 0)
                {
                    await TimerComponent.Instance.WaitFrameAsync();
                }
                await Game.Scene.GetComponent<DBComponent>().Save(newzone, entity);
            }
            Log.Console("SkillSetComponent Complelte");

            //TaskComponent  renw组件
            dbcount = 0;
            List<TaskComponentServer> taskComponents = await Game.Scene.GetComponent<DBComponent>().Query<TaskComponentServer>(oldzone, d => d.Id > 0);
            foreach (var entity in taskComponents)
            {
                if (invalidPlayers.Contains(entity.Id))
                {
                    continue;
                }
                dbcount++;
                if (dbcount % onecount == 0)
                {
                    await TimerComponent.Instance.WaitFrameAsync();
                }
                await Game.Scene.GetComponent<DBComponent>().Save(newzone, entity);
            }
            Log.Console("TaskComponent Complelte");

            dbcount = 0;
            List<TitleComponentServer> titleComponents = await Game.Scene.GetComponent<DBComponent>().Query<TitleComponentServer>(oldzone, d => d.Id > 0);
            foreach (var entity in titleComponents)
            {
                if (invalidPlayers.Contains(entity.Id))
                {
                    continue;
                }
                dbcount++;
                if (dbcount % onecount == 0)
                {
                    await TimerComponent.Instance.WaitFrameAsync();
                }
                await Game.Scene.GetComponent<DBComponent>().Save(newzone, entity);
            }
            Log.Console("TitleComponent Complelte");

            //RoleInfoComponent  玩家信息
            dbcount = 0;
            Dictionary<string, RoleInfoComponentServer> newuserinfoList = new Dictionary<string, RoleInfoComponentServer>();
            //先初始化新的玩家列表
            List<RoleInfoComponentServer> newRoleInfoComponents = await Game.Scene.GetComponent<DBComponent>().Query<RoleInfoComponentServer>(newzone, d => d.Id > 0);
            foreach (var entity in newRoleInfoComponents)
            {
                if (entity.RoleInfo == null || string.IsNullOrEmpty(entity.RoleInfo.Name))
                {
                    Log.Debug("entity.RoleInfo == null:  " + entity.Id);
                    continue;
                }

                if (!newuserinfoList.ContainsKey(entity.RoleInfo.Name))
                {
                    newuserinfoList.Add(entity.RoleInfo.Name, entity);
                }
            }
            Log.Console("newuserinfoList Complelte");

            int maxServerId = 0;
            List<DBServerMailInfo> dBServerMailInfos = await Game.Scene.GetComponent<DBComponent>().Query<DBServerMailInfo>(newzone, d => d.Id == newzone);
            if (dBServerMailInfos.Count > 0)
            {
                foreach ((int id, ServerMailItem ServerItem) in dBServerMailInfos[0].ServerMailList)
                {
                    if (id >= maxServerId)
                    {
                        maxServerId = id;
                    }
                }
            }
            Log.Console($"maxServerId {maxServerId}");

            List<RoleInfoComponentServer> oldRoleInfoComponents = await Game.Scene.GetComponent<DBComponent>().Query<RoleInfoComponentServer>(oldzone, d => d.Id > 0);
            foreach (var oldentity in oldRoleInfoComponents)
            {
                if (invalidPlayers.Contains(oldentity.Id))
                {
                    continue;
                }

                dbcount++;
                if (dbcount % onecount == 0)
                {
                    Log.Console("合区补偿改名卡");
                    await TimerComponent.Instance.WaitFrameAsync();
                }

                if (oldentity.RoleInfo == null || string.IsNullOrEmpty(oldentity.RoleInfo.Name))
                {
                    continue;
                }
                if (newuserinfoList.ContainsKey(oldentity.RoleInfo.Name))
                {
                    //合服账号名称规则，A：流星 25级 B 流星 30级 则B流星 名字沿用，A自动发放一个改名卡 （规则 等级高 > 战力高 > id在前）
                    long renameId = 0;
                    RoleInfoComponentServer newentity = newuserinfoList[oldentity.RoleInfo.Name];
                    if (oldentity.RoleInfo.Lv > newentity.RoleInfo.Lv)
                    {
                        renameId = newentity.Id;
                        newentity.RoleInfo.Name += oldzone.ToString();
                        await Game.Scene.GetComponent<DBComponent>().Save(newzone, newentity);
                    }
                    else
                    {
                        renameId = oldentity.Id;
                        oldentity.RoleInfo.Name += oldzone.ToString();
                    }

                    List<DBMailInfo> renamedBMailInfos = await Game.Scene.GetComponent<DBComponent>().Query<DBMailInfo>(oldzone, d => d.Id == renameId);
                    if (renamedBMailInfos.Count > 0)
                    {
                        Log.Error("MailInfo mailInfo = new MailInfo");
                        //MailInfo mailInfo = new MailInfo();
                        //mailInfo.Status = 0;
                        //mailInfo.Context = "合区补偿改名卡";
                        //mailInfo.Title = "合区补偿";
                        //mailInfo.MailId = IdGenerater.Instance.GenerateId();
                        //BagInfo reward = new BagInfo();
                        //reward.ItemID = 10010036;
                        //reward.ItemNum = 1;
                        //reward.GetWay = $"{ItemGetWay.System}_{TimeHelper.ServerNow()}";
                        //mailInfo.ItemList.Add(reward);
                        //renamedBMailInfos[0].MailInfoList.Add(mailInfo);

                        await Game.Scene.GetComponent<DBComponent>().Save(newzone, renamedBMailInfos[0]);
                    }
                }

                if (maxServerId > 0 && maxServerId > oldentity.RoleInfo.ServerMailIdCur)
                {
                    oldentity.RoleInfo.ServerMailIdCur = maxServerId;
                }
                await Game.Scene.GetComponent<DBComponent>().Save(newzone, oldentity);
            }

            Log.Console("MergeZone Complelte");
        }
    }
}

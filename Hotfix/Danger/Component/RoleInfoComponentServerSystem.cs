using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ET
{

    [Timer(TimerType.ShouLieUpLoadTimer)]
    public class ShouLieUpLoadTimer : ATimer<RoleInfoComponent>
    {
        public override void Run(RoleInfoComponent self)
        {
            try
            {
                self.UpdateShowLie().Coroutine();
            }
            catch (Exception e)
            {
                Log.Error($"move timer error: {self.Id}\n{e}");
            }
        }
    }


    [ObjectSystem]
    public class RoleInfoComponentAwake : AwakeSystem<RoleInfoComponent>
    {
        public override void Awake(RoleInfoComponent self)
        {

        }
    }

    [ObjectSystem]
    public class RoleInfoComponentDestroy : DestroySystem<RoleInfoComponent>
    {
        public override void Destroy(RoleInfoComponent self)
        {
            TimerComponent.Instance.Remove(ref self.ShouLieUpLoadTimer);
        }
    }

    public static class RoleInfoComponentServerSystem
    {

        public static void OnInit(this RoleInfoComponent self,string account, long userId, long accountId, CreateRoleInfo createRoleInfo)
        {
            self.Account = account;
            self.CreateAccountTime = createRoleInfo.CreateTime;
            RoleInfo roleInfo = self.RoleInfo;
            roleInfo.Sp = 1;
            roleInfo.UserId = userId;
            roleInfo.JiaYuanLv = 1;
            roleInfo.BaoShiDu = 100;
            roleInfo.JiaYuanFund = 10000;
            roleInfo.AccInfoID =accountId;
            roleInfo.Name = createRoleInfo.PlayerName;
            roleInfo.ServerMailIdCur = -1;
            roleInfo.PiLao = 120;     //初始化疲劳
            roleInfo.Vitality = 120;
            //roleInfo.MakeList.AddRange(CommonHelper.StringArrToIntList(LDGlobalValueCategory.Instance.Get(18).Value.Split(';')));
            roleInfo.CreateTime = TimeHelper.ServerNow();

            if (createRoleInfo.RobotId > 0)
            {
                int robotId = int.Parse(account.Split('_')[0]);
                RobotConfig robotConfig = RobotConfigCategory.Instance.Get(robotId);
                roleInfo.Lv = robotConfig.Behaviour == 1 ?  RandomHelper.RandomNumber(10, 19) : robotConfig.Level;
                roleInfo.Occ = robotConfig.Behaviour == 1 ?  RandomHelper.RandomNumber(1, 3) : robotConfig.Occ;
                roleInfo.Gold = 100000;
                roleInfo.RobotId = robotId;
                //roleInfo.OccTwo = robotConfig.OccTwo;
            }
            else
            {
                roleInfo.Lv = 1;
                roleInfo.Gold = 0;
                roleInfo.SeasonLevel = 1;
                roleInfo.Occ = createRoleInfo.PlayerOcc;
            }
        }
        
        public static void Check(this RoleInfoComponent self)
        {
            self.TodayOnLine++;
            self.LingDiOnLine++;

            //领地和家园都是一小时刷新一次经验
            if (self.LingDiOnLine > 60)
            {
                self.LingDiOnLine = 0;
                //self.OnRongyuChanChu(1, true);
                self.OnJiaYuanExp(1f);
            }

            if (self.UpdateRankTime > 0 )
            {
                self.UpdateRankTime = 0;
                self.UploadCombat().Coroutine();
            }
        }

        public static void OnJiaYuanExp(this RoleInfoComponent self, float hour)
        {
            
            if ( !LDHomeCategory.Instance.Contain(self.RoleInfo.JiaYuanLv + 1) )
            {
                
                return;
            }
            
            LDHome ldHome = LDHomeCategory.Instance.Get(self.RoleInfo.JiaYuanLv);
            //self.RoleInfo.JiaYuanExp += jiaYuanConfig.JiaYuanAddExp;
            //int addexp = Mathf.FloorToInt(hour * ldHome.JiaYuanAddExp);
            //self.UpdateRoleMoneyAdd(UserDataType.JiaYuanExp, $"{addexp}", true, ItemGetWay.JiaYuanExchange);
        }

        public static void OnRongyuChanChu(this RoleInfoComponent self, int coefficient, bool notice)
        {
            if (coefficient == 0)
            {
                return;
            }
            Unit unit = self.GetParent<Unit>();
            int lingdiLv = unit.GetComponent<NumericComponent>().GetAsInt(NumericType.Ling_DiLv);
          //  LingDiConfig lingDiConfig = LingDiConfigCategory.Instance.Get(lingdiLv);

            //unit.GetComponent<RoleInfoComponent>().UpdateRoleData(UserDataType.Exp, (coefficient *lingDiConfig.HoureExp).ToString(), notice).Coroutine();
           // self.UpdateRoleData(UserDataType.FangRong, (coefficient * lingDiConfig.HoureExp).ToString(), notice);
           // self.UpdateRoleData(UserDataType.RongYu, (coefficient * lingDiConfig.HoureHonor).ToString(), notice);
        }

        public static void OpenAll(this RoleInfoComponent self)
        {
            self.RoleInfo.FubenPassList.Clear();

            /*Dictionary<int, ChapterConfig> keyValuePairs = ChapterConfigCategory.Instance.GetAll();
            foreach (var item in keyValuePairs)
            {
                self.RoleInfo.FubenPassList.Add(new FubenPassInfo()
                {
                    FubenId = item.Key,
                    Difficulty = (int)FubenDifficulty.DiYu
                });
            }*/
        }


        public static int GetTiLiTimes(this RoleInfoComponent self, int hour_1, int hour_2)
        {
            int index_1 = self.GetTiLiIndex(hour_1);
            int index_2 = self.GetTiLiIndex(hour_2);
            if (index_1 > index_2)
            {
                return 0;
            }
            return index_2 - index_1;
        }

        /// <summary>
        ///  
        /// </summary>
        /// <param name="self"></param>
        /// <param name="hour_1"></param>
        /// <param name="hour_2"></param>  0 6 12 20
        /// <returns></returns>
        public static List<int> GetTiLiIndexsNew(this RoleInfoComponent self, int hour_1, int hour_2)
        {
            List<int> indexs = new  List<int>();    
            if (hour_1 >= hour_2)
            {

                return indexs;
            }
            if (hour_1 < 6 && hour_2 >= 6)
            {
                indexs.Add(6);
            }
            if (hour_1 < 12 && hour_2 >= 12)
            {
                indexs.Add(12);
            }
            if (hour_1 < 20 && hour_2 >= 20)
            {
                indexs.Add(20);
            }

            return indexs;
        }

        public static int GetTiliRecover(this RoleInfoComponent self, List<int> indexids)
        {
            int totalTili = 0;
            int totalindex = indexids.Count;
            if (totalindex >= 1 && indexids.Contains(6))
            {
                totalTili += 50;
                totalindex--;
            }
            if (totalindex >= 1 && indexids.Contains(20))
            {
                totalTili += 50;
                totalindex--;
            }
            if (totalindex >= 1)
            {
                totalTili = totalTili + totalindex * 30;
                totalindex = 0;
            }
            return totalTili;
        }

        public static int GetTiLiIndex(this RoleInfoComponent self, int hour_1)
        {
            if (hour_1 < 6)
            {
                return 1;
            }
            if (hour_1 < 12)
            {
                return 2;
            }
            if (hour_1 < 20)
            {
                return 3;
            }
            if (hour_1 < 24)
            {
                return 4;
            }
            return 5;
        }

        public static void CheckData(this RoleInfoComponent self)
        {
            if (!LDHomeCategory.Instance.Contain(self.RoleInfo.JiaYuanLv))
            {
                self.RoleInfo.JiaYuanLv = 1;
            }
            if (self.RoleInfo.SeasonLevel == 0)
            {
                self.RoleInfo.SeasonLevel = 1;
            }
            if (self.RoleInfo.CreateTime == 0)
            {
                self.RoleInfo.CreateTime = TimeHelper.ServerNow();
            }
            if (self.RoleInfo.Lv < 20 && self.RoleInfo.BaoShiDu < 100)
            {
                self.RoleInfo.BaoShiDu = 100;
            }

            int maxTowerId = 0;
            if (self.RoleInfo.TowerRewardIds.Count > 0)
            {
                maxTowerId = self.RoleInfo.TowerRewardIds[self.RoleInfo.TowerRewardIds.Count - 1];
            }
            NumericComponent numericComponent = self.GetParent<Unit>().GetComponent<NumericComponent>();

            for (int  i =  self.RoleInfo.HorseIds.Count - 1; i >= 0; i--)
            {
                if ( !LDMountCategory.Instance.Contain( self.RoleInfo.HorseIds[i]))
                {
                    self.RoleInfo.HorseIds.RemoveAt(i);
                }
            }

            if (self.RoleInfo.RobotId > 0 &&    self.RoleInfo.HorseIds.Count == 0)
            {
                List<LDMount> mounts = LDMountCategory.Instance.GetAll().Values.ToList();
                int intdexxx =  RandomHelper.RandomNumber(0, mounts.Count);
                int randomid = mounts[intdexxx].Id;
                self.OnHorseActive(randomid, true);
                numericComponent.Set(NumericType.HorseFightID, randomid, false);
                numericComponent.Set(NumericType.HorseRide, randomid, false);
            }
            
            UpgrageLevelHelper.CheckInitPoint(self.GetParent<Unit>(),self.RoleInfo.Lv);
            
            PetComponent petComponent = self.GetParent<Unit>().GetComponent<PetComponent>();
            if (self.RoleInfo.RobotId > 0 &&   petComponent.RolePetInfos.Count == 0)
            {
                List<int> petids = LDPetCategory.Instance.GetAll().Keys.ToList();
                int randomindex = RandomHelper.RandomNumber(0, petids.Count);
                
                petComponent.OnGmAddPet(petids[randomindex]);
                petComponent.RolePetInfos[0].PetStatus = 1;
                petComponent.FightPetId = petComponent.RolePetInfos[0].Id;
            }

            if (numericComponent.GetAsInt(NumericType.TrialDungeonId) < maxTowerId)
            {
                numericComponent.Set(NumericType.TrialDungeonId, maxTowerId, false);
            }

            DataCollationComponent dataCollationComponent = self.GetParent<Unit>().GetComponent<DataCollationComponent>();
            int recharge = numericComponent.GetAsInt(NumericType.RechargeNumber);
            if (recharge!=0 && dataCollationComponent.ChouKaTimes > (recharge * 2) && dataCollationComponent.ChouKaTimes > 100)
            {
                Log.Warning($"抽卡次数异常:{self.DomainZone()} {self.RoleInfo.Name}   充值:{numericComponent.GetAsInt(NumericType.RechargeNumber)}  抽卡:{dataCollationComponent.ChouKaTimes}");
            }

            // 烟雨楼Id: 2466222808943362373   烟雨楼 寸断De法殇 ID: 2466171477355986944
            if (self.RoleInfo.UserId == 2466171477355986944)
            {
                //self.RoleInfo.UnionName = "烟雨楼";
                //self.GetParent<Unit>().GetComponent<NumericComponent>().ApplyValue(NumericType.UnionLeader, 0, false);
                //self.GetParent<Unit>().GetComponent<NumericComponent>().ApplyValue(NumericType.UnionId_0, 2466222808943362373, false);
            }

            if (self.RoleInfo.UnionKeJiList.Count < UnionKeJiConfigCategory.Instance.UnionQiangHuaList.Count)
            {
                int curNumber = self.RoleInfo.UnionKeJiList.Count;
                int maxNumber = UnionKeJiConfigCategory.Instance.UnionQiangHuaList.Count;
                for (int keji = curNumber; keji < maxNumber; keji++)
                {
                    self.RoleInfo.UnionKeJiList.Add(UnionKeJiConfigCategory.Instance.GetFristId( keji ) );
                }
            }
            if (!LDHomeCategory.Instance.Contain(self.RoleInfo.JiaYuanLv +1) && self.RoleInfo.JiaYuanExp > 0)
            {
                self.RoleInfo.JiaYuanExp = 0;
                Console.WriteLine($"清空家园经验: {self.Id}  {self.RoleInfo.JiaYuanLv}  {self.RoleInfo.JiaYuanExp}");
            }

        }

        private static bool IsZhuBoLevel16(this RoleInfoComponent self)
        {
            if (!CommonHelper.IsZhuBoZone(self.DomainZone()))
            {
                return false;

            }

            return self.Id == 2648795239413776384 || self.Id == 2641338471813283840;
        }

        public static void OnOffLine(this RoleInfoComponent self)
        {
            //self.LastLoginTime = TimeHelper.ServerNow();
        }

        public static void OnLogin(this RoleInfoComponent self, string remoteIp)
        {
            self.CheckData();
            self.RemoteAddress = remoteIp;
            
            self.LastLoginTime = TimeHelper.ServerNow();
            self.UserName = self.RoleInfo.Name;
            self.ShouLieSendTime = 0;
        }

        /// <summary>
        /// 体力
        /// </summary>
        /// <param name="self"></param>
        /// <param name="skillNumber"></param>
        /// <returns></returns>
        public static int GetAddPiLao(this RoleInfoComponent self, int skillNumber)
        {
            return 0;
        }

        /// <summary>
        /// 0 6 12 20点各刷新30点体力
        /// </summary>
        /// <param name="self"></param>
        /// <param name="notice"></param>
        public static void OnHourUpdate(this RoleInfoComponent self, int hour, bool notice)
        {
            if (self.LastLoginTime > 0)
            {
                DateTime lastdateTime = TimeInfo.Instance.ToDateTime(self.LastLoginTime);
                DateTime nowdateTime = TimeInfo.Instance.ToDateTime(TimeHelper.ServerNow());
                if ( lastdateTime.Hour == nowdateTime.Hour)
                {
                    if (self.Id == 2341487098982367232)
                    {
                        Console.WriteLine($"刀：lastdateTime.Hour == nowdateTime.Hour  {hour}");
                    }
                    return;
                }
            }

            if (hour == 0 )
            {
                self.RecoverPiLao(30 + self.GetAddPiLao(self.RoleInfo.MakeList.Count), notice);
            }
            if (hour == 12)
            {
                self.RecoverPiLao(30, notice);
            }

            if (hour == 6 ||  hour == 20)
            {
                self.RecoverPiLao(50, notice);
            }

            self.GetParent<Unit>().GetComponent<JiaYuanComponentServer>().OnHourUpdate(hour, notice);
            LogHelper.CheckZuoBi(self.GetParent<Unit>());
            //LogHelper.CheckBlackRoom(self.GetParent<Unit>());
        }

        public static void RecoverPiLao(this RoleInfoComponent self, int addValue, bool notice)
        {
            Unit unit = self.GetParent<Unit>();
            long recoverPiLao = self.GetParent<Unit>().GetMaxPiLao() - self.RoleInfo.PiLao;
            recoverPiLao = Math.Min(recoverPiLao, addValue);

            Log.Warning($"[增加体力] {unit.DomainZone()}    {unit.Id}    {recoverPiLao}");
            self.UpdateRoleData(UserDataType.PiLao, recoverPiLao.ToString(), notice);
            self.LastLoginTime = TimeHelper.ServerNow();
        }



        public static void OnZeroClockUpdate(this RoleInfoComponent self, bool notice)
        {
            Unit unit = self.GetParent<Unit>();
            NumericComponent numericComponent = unit.GetComponent<NumericComponent>();
            int skillNumber = 1 + numericComponent.GetAsInt(NumericType.MakeType_2) > 0 ? 1 : 0;
            int updatevalue = unit.GetMaxHuoLi(skillNumber) - self.RoleInfo.Vitality;
            self.UpdateRoleData(UserDataType.Vitality, updatevalue.ToString(), notice);
            //updatevalue = ComHelp.GetMaxBaoShiDu() - self.RoleInfo.BaoShiDu;
            //self.UpdateRoleData(UserDataType.BaoShiDu, updatevalue.ToString(), notice);
            numericComponent.ApplyValue(NumericType.ZeroClock, 1, notice);
            self.ClearDayData();
            self.LastLoginTime = TimeHelper.ServerNow();
            self.TodayOnLine = 0;
            self.ShouLieKill = 0;
        }

        public static RoleInfo GetUserInfo(this RoleInfoComponent self)
        {
            return self.RoleInfo;
        }

        public static void OnShowLieKill(this RoleInfoComponent self)
        {
            self.ShouLieKill++;

            if (self.ShouLieUpLoadTimer == 0)
            {
                self.ShouLieUpLoadTimer = TimerComponent.Instance.NewOnceTimer(TimeHelper.ServerNow() + 5 * TimeHelper.Second, TimerType.ShouLieUpLoadTimer, self);
            }
            else
            {
                self.UpdateShowLie().Coroutine();
            }
        }

        public static async ETTask UpdateShowLie(this RoleInfoComponent self)
        {
            Unit unit = self.GetParent<Unit>();
            if (!ActivityHelper.IsShowLieOpen() || unit.IsRobot())
            {
                return;
            }
            self.ShouLieSendTime = TimeHelper.ServerNow();
            TimerComponent.Instance.Remove(ref self.ShouLieUpLoadTimer);
            RankShouLieInfo rankPetInfo = new RankShouLieInfo();
            RoleInfoComponent roleInfoComponent = unit.GetComponent<RoleInfoComponent>();
            rankPetInfo.UnitID = roleInfoComponent.RoleInfo.UserId;
            rankPetInfo.PlayerName = roleInfoComponent.RoleInfo.Name;
            rankPetInfo.Occ = roleInfoComponent.RoleInfo.Occ;
            rankPetInfo.KillNumber = self.ShouLieKill;
            long mapInstanceId = DBHelper.GetRankServerId(self.DomainZone());
            R2M_RankShowLieResponse Response = (R2M_RankShowLieResponse)await ActorMessageSenderComponent.Instance.Call
                     (mapInstanceId, new M2R_RankShowLieRequest()
                     {
                         RankingInfo = rankPetInfo
                     });
        }


        /// <summary>
        /// 杀怪经验
        /// </summary>
        /// <param name="self"></param>
        /// <param name="beKill"></param>
        public static void OnKillUnit(this RoleInfoComponent self, Unit beKill, int sceneType, int sceneId)
        {
            Unit main = self.GetParent<Unit>();
            if (beKill.Type != UnitType.Monster)
            {
                return;
            }

            bool showlieopen = ActivityHelper.IsShowLieOpen();
            LDMonster ldMonster = LDMonsterCategory.Instance.Get(beKill.ConfigId);
            if (showlieopen && ( ldMonster.Lv >= 60 || Mathf.Abs(self.RoleInfo.Lv - ldMonster.Lv) <= 9) )
            {
                self.OnShowLieKill();
           
            }

            if (SeasonHelper.GetOpenSeason(self.RoleInfo.Lv)!=null && beKill.IsBoss() && ldMonster.Lv >= 40)
            {
                int seasonExp = RandomHelper.RandomNumber(1, 6);
                self.UpdateRoleData(UserDataType.SeasonExp, seasonExp.ToString());
            }

            NumericComponent numericComponent = main.GetComponent<NumericComponent>();
            numericComponent.ApplyChange(null, NumericType.KillMonsterNumber, 1, 0);

            int tiliKillNumber = numericComponent.GetAsInt(NumericType.TiLiKillNumber);
            if (sceneType == MapTypeEnum.LocalDungeon && !showlieopen && self.RoleInfo.PiLao > 0)
            {
                if (tiliKillNumber >= 4)
                {
                    numericComponent.ApplyValue(NumericType.TiLiKillNumber, 0, false);

                    numericComponent.ApplyChange(null, NumericType.CostTiLi, 1, 0);
                    if ( CommonHelper.IsZhuBoZone(self.DomainZone()) && self.RoleInfo.PiLao < 2)
                    {
                        self.UpdateRoleData(UserDataType.PiLao, "100", true);
                    }
                    else
                    {
                        self.UpdateRoleData(UserDataType.PiLao, "-1", true);
                    }
                }
                else
                {
                    numericComponent.ApplyChange(null, NumericType.TiLiKillNumber,  1, 0);
                }
            }

            bool drop = true;
            if (SceneConfigHelper.IsSingleFuben(sceneType))
            {
                drop = self.RoleInfo.PiLao > 0 || beKill.IsBoss() || showlieopen;
            }
            if (drop)
            {
                LDMonster mCof = LDMonsterCategory.Instance.Get(beKill.ConfigId);
                float expcoefficient = 1f;
                if (sceneType == MapTypeEnum.LocalDungeon && beKill.IsBoss())
                {
                    int killNumber = main.GetComponent<RoleInfoComponent>().GetMonsterKillNumber(mCof.Id);
                    int chpaterid = -1;////LDSceneCategory.Instance.GetChapterByDungeon(sceneId);
                    BossDevelopment bossDevelopment = CommonConfig.GetBossDevelopmentByKill(chpaterid, killNumber);
                    expcoefficient *= bossDevelopment.ExpAdd;
                }

                float expAdd = (numericComponent.GetAsFloat(NumericType.Numeric_Error) - 1f);
                expAdd = Math.Clamp(expAdd, 0f, 1f);    
                
                float now_GoldAdd_Pro  = (numericComponent.GetAsFloat(NumericType.Numeric_Error));
                now_GoldAdd_Pro = Math.Clamp(now_GoldAdd_Pro, 0f, 1f);
                
                expcoefficient += expAdd;
                expcoefficient+= now_GoldAdd_Pro;
                
                if ((sceneType == MapTypeEnum.LocalDungeon && self.RoleInfo.PiLao > 0)
                  || sceneType != MapTypeEnum.LocalDungeon)
                {
                    if (numericComponent.GetAsInt(NumericType.JueXingExp) < 5000)
                    {
                        numericComponent.ApplyChange(null, NumericType.JueXingExp, 1, 0);
                    }
                }

                int addexp = (int)(expcoefficient * 0);
                self.UpdateRoleData(UserDataType.Exp, addexp.ToString());
            }

            // 纪录击败的Boss
            if (beKill.IsBoss() && CommonConfig.DefeatedBossIds.ContainsKey(beKill.ConfigId))
            {
                if (!self.RoleInfo.DefeatedBossIds.Contains(beKill.ConfigId))
                {
                    self.RoleInfo.DefeatedBossIds.Add(beKill.ConfigId);
                }
            }
        }

        public static void UpdateRoleDataBroadcast(this RoleInfoComponent self, int Type, string value)
        {
            Unit unit = self.GetParent<Unit>();
            M2C_RoleDataBroadcast m2C_BroadcastRoleData = self.m2C_RoleDataBroadcast;
            m2C_BroadcastRoleData.UnitId = unit.Id;
            m2C_BroadcastRoleData.UpdateType = (int)Type;
            m2C_BroadcastRoleData.UpdateTypeValue = value;
            MessageHelper.Broadcast(unit, m2C_BroadcastRoleData);
        }

        public static int GetMysteryBuy(this RoleInfoComponent self, int mysteryId)
        {
            for (int i = 0; i < self.RoleInfo.MysteryItems.Count; i++)
            {
                if (self.RoleInfo.MysteryItems[i].KeyId == mysteryId)
                {
                    return (int)self.RoleInfo.MysteryItems[i].Value;
                }
            }
            return 0;
        }

        public static void OnMysteryBuy(this RoleInfoComponent self, int mysteryId)
        {
            for (int i = 0; i < self.RoleInfo.MysteryItems.Count; i++)
            {
                if (self.RoleInfo.MysteryItems[i].KeyId == mysteryId)
                {
                    self.RoleInfo.MysteryItems[i].Value += 1;
                    return;
                }
            }
            self.RoleInfo.MysteryItems.Add(new KeyValuePairInt() { KeyId = mysteryId, Value = 1 });
        }

        public static int GetStoreBuy(this RoleInfoComponent self, int mysteryId)
        {
            for (int i = 0; i < self.RoleInfo.BuyStoreItems.Count; i++)
            {
                if (self.RoleInfo.BuyStoreItems[i].KeyId == mysteryId)
                {
                    return (int)self.RoleInfo.BuyStoreItems[i].Value;
                }
            }
            return 0;
        }

        public static void OnStoreBuy(this RoleInfoComponent self, int mysteryId)
        {
            for (int i = 0; i < self.RoleInfo.BuyStoreItems.Count; i++)
            {
                if (self.RoleInfo.BuyStoreItems[i].KeyId == mysteryId)
                {
                    self.RoleInfo.BuyStoreItems[i].Value += 1;
                    return;
                }
            }
            self.RoleInfo.BuyStoreItems.Add(new KeyValuePairInt() { KeyId = mysteryId, Value = 1 });
        }

        //加金币
        public static void UpdateRoleMoneyAdd(this RoleInfoComponent self, int Type, string value, bool notice, int getWay, string paramsifo = "")
        {
            Unit unit = self.GetParent<Unit>();
            long gold = long.Parse(value);
            if (gold < 0)
            {
                Log.Warning($"增加货币出错:{Type}  {unit.Id} {getWay} {self.RoleInfo.Name}  {value}", true);
            }
            else
            {
                if (getWay != ItemGetWay.PickItem || gold > 1000)
                {
                    LogHelper.LogWarning($"增加货币:{Type} {unit.Id} {getWay} {self.RoleInfo.Name}  {value}", true);
                }
            }
            if (gold > 100000 || gold < -100000)
            {
                Log.Warning($"增加货币[大额]:{Type} {unit.Id} {getWay} {self.RoleInfo.Name} {value}  {paramsifo}", true);
            }
            else if (gold > 1000000 || gold < -1000000)
            {
                Log.Warning($"增加货币[超额]:{Type} {unit.Id} {getWay} {self.RoleInfo.Name} {value}", true);
            }

            if (gold > 0 && getWay == ItemGetWay.PaiMaiSell)
            {
                unit.GetComponent<ChengJiuComponentServer>().TriggerEvent(ChengJiuTargetEnum.PaiMaiGetGoldNumber_217, 0, (int)gold);
            }

            if (Type == UserDataType.Diamond)
            {
                self.RoleInfo.DiamondGetWay.Add(getWay);
                if (self.RoleInfo.DiamondGetWay.Count > 200)
                {
                    self.RoleInfo.DiamondGetWay.RemoveAt(0);    
                }
            }

            if (Type == UserDataType.Gold)
            {
                self.RoleInfo.GoldGetWay.Add(getWay);
                if (self.RoleInfo.GoldGetWay.Count > 200)
                {
                    self.RoleInfo.GoldGetWay.RemoveAt(0);
                }
            }

            if (Type == UserDataType.Exp)
            {
                self.RoleInfo.ExpGetWay.Add(getWay);
                if (self.RoleInfo.ExpGetWay.Count > 200)
                {
                    self.RoleInfo.ExpGetWay.RemoveAt(0);
                }
            }

            if (Type == UserDataType.Diamond)
            {
                Log.Warning($"增加钻石: {Type} {unit.Id} {getWay} {self.RoleInfo.Name} {value}");
            }
            if (Type == UserDataType.WeiJingGold)
            {
                Log.Warning($"增加兑换币: {Type} {unit.Id} {getWay} {self.RoleInfo.Name} {value}");
            }
    
            if (Type == UserDataType.UnionExp || Type == UserDataType.UnionGold)
            {
                self.SendUnionOperate(getWay, Type, gold).Coroutine();
            }
            unit.GetComponent<DataCollationComponent>().UpdateRoleMoneyAdd(Type, getWay, gold);
            self.UpdateRoleData(Type, value, notice);
        }

        //扣金币
        public static void UpdateRoleMoneySub(this RoleInfoComponent self, int Type, string value, bool notice = true, int getWay = ItemGetWay.System, string paramsifo = "")
        {
            Unit unit = self.GetParent<Unit>();
            long gold = long.Parse(value);
            if (gold > 0)
            {
                LogHelper.LogWarning($"扣除货币出错:{Type} {unit.Id} {getWay} {self.RoleInfo.Name}  {value}", true);
            }
            else
            {
                LogHelper.LogWarning($"扣除货币:{Type} {unit.Id} {getWay} {self.RoleInfo.Name} {value}", true);
            }
            if (gold > 100000 || gold < -100000)
            {
                Log.Warning($"扣除货币[大额]:{Type} {unit.Id} {getWay} {self.RoleInfo.Name} {value}");
            }
            if (Type == UserDataType.Diamond)
            {
                Log.Warning($"扣除钻石: {Type} {unit.Id} {getWay} {self.RoleInfo.Name} {value}");
            }
            if (Type == UserDataType.WeiJingGold)
            {
                Log.Warning($"扣除兑换币: {Type} {unit.Id} {getWay} {self.RoleInfo.Name} {value}");
            }

            unit.GetComponent<DataCollationComponent>().UpdateRoleMoneySub(Type, getWay, gold);
            self.UpdateRoleData(Type, value, notice);
        }

        public static async ETTask SendUnionOperate(this RoleInfoComponent self, int getWay, int dataType,  long dataValue)
        {
            Unit unit = self.GetParent<Unit>();
            long unionid = unit.GetUnionId();
            if (unionid == 0)
            {
                return;
            }
            string playerName = self.RoleInfo.Name;
            long serverod = DBHelper.GetUnionServerId(self.DomainZone() );
            U2M_UnionOperationResponse responseUnionEnter = (U2M_UnionOperationResponse)await ActorMessageSenderComponent.Instance.Call(
                            serverod, new M2U_UnionOperationRequest() { OperateType = 1, UnionId = unionid, Par = $"{playerName}_{getWay}_{dataType}_{dataValue}" });
        }

        public static async ETTask BroadcastLevel(this RoleInfoComponent self, int level)
        {
            Unit unit = self.GetParent<Unit>();
            long chatServerId = StartSceneConfigCategory.Instance.GetBySceneName(unit.DomainZone(), Enum.GetName(SceneType.Chat)).InstanceId;
            Chat2M_UpdateLevel chat2G_EnterChat = (Chat2M_UpdateLevel)await MessageHelper.CallActor(chatServerId, new M2Chat_UpdateLevel()
            {
                UnitId = unit.Id,
                Level = level,
            });

            if (level == 70)
            {
                MailInfo mailInfo = new MailInfo();
                mailInfo.Status = 0;
                mailInfo.Context = "完成所有进阶之路后，可以将等级上限突破至75级！";
                mailInfo.Title = "进阶之路系列任务";
                mailInfo.MailId = IdGenerater.Instance.GenerateId();
                MailHelp.SendUserMail(self.DomainZone(), self.Id, mailInfo).Coroutine();
            }
        }

        //需要通知客户端
        public static void UpdateRoleData(this RoleInfoComponent self, int Type, string value, bool notice = true)
        {
            Unit unit = self.GetParent<Unit>();
            string saveValue = "";
            long longValue = 0;
            switch (Type)
            {
                //case UserDataType.UnionExp:
                //    int addexp = int.Parse(value);
                //    self.SendUnionOperate(1, addexp).Coroutine();
                //    return;
                //case UserDataType.UnionGold:
                //    self.SendUnionOperate(5, int.Parse(value)).Coroutine();
                //    return;
                case UserDataType.JiaYuanExp:
                    self.RoleInfo.JiaYuanExp += int.Parse(value);
                    saveValue = self.RoleInfo.JiaYuanExp.ToString();
                    break;
                case UserDataType.JiaYuanFund:
                    self.RoleInfo.JiaYuanFund += int.Parse(value);
                    saveValue = self.RoleInfo.JiaYuanFund.ToString();
                    break;
                case UserDataType.UnionContri:
                    self.RoleInfo.UnionZiJin += int.Parse(value);
                    saveValue = self.RoleInfo.UnionZiJin.ToString();
                    break;
                case UserDataType.SeasonCoin:
                    self.RoleInfo.SeasonCoin += int.Parse(value);
                    saveValue = self.RoleInfo.SeasonCoin.ToString();
                    break;
                case UserDataType.SeasonExp:
                    self.RoleInfo.SeasonExp += int.Parse(value);
                    SeasonLevelConfig seasonLevelConfig = SeasonLevelConfigCategory.Instance.Get(self.RoleInfo.SeasonLevel);
                    if (self.RoleInfo.SeasonExp >= seasonLevelConfig.UpExp && SeasonLevelConfigCategory.Instance.Contain(self.RoleInfo.SeasonLevel + 1))
                    {
                        self.RoleInfo.SeasonExp -= seasonLevelConfig.UpExp;
                        self.UpdateRoleData( UserDataType.SeasonLevel, "1" );
                    }
                    saveValue = self.RoleInfo.SeasonExp.ToString();
                    longValue = self.RoleInfo.SeasonExp;
                    break;
                case UserDataType.SeasonLevel:
                    self.RoleInfo.SeasonLevel += int.Parse(value);
                    saveValue = self.RoleInfo.SeasonLevel.ToString();
                    break;
                case UserDataType.JiaYuanLv:
                    self.RoleInfo.JiaYuanLv += int.Parse(value);
                    saveValue = self.RoleInfo.JiaYuanLv.ToString();
                    unit.GetComponent<TaskComponent>().TriggerTaskEvent(TastConditionType.JiaYuanLevel_22, 0, self.RoleInfo.JiaYuanLv - 10000);
                    unit.GetComponent<ChengJiuComponentServer>().TriggerEvent(ChengJiuTargetEnum.JiaYuanLevel_404, 0, self.RoleInfo.JiaYuanLv - 10000);
                    break;
                case UserDataType.FangRong:
                    LingDiHelp.OnAddLingDiExp(unit, int.Parse(value), notice);
                    break;
                //名字应该在改名的协议处理
                case UserDataType.Name:
                    self.RoleInfo.Name = value;
                    saveValue = self.RoleInfo.Name;
                    break;
                case UserDataType.Exp:
                    if (self.IsZhuBoLevel16())
                    {
                        return;
                    }

                    self.Role_AddExp(long.Parse(value), notice);
                    //saveValue = self.RoleInfo.Exp.ToString();
                    longValue = self.RoleInfo.Exp;
                    saveValue = value;
                    break;
                case UserDataType.Level:
                    if (self.IsZhuBoLevel16())
                    {
                        return;
                    }

                    int addLevel = int.Parse(value);
                    int oldLevel = self.RoleInfo.Lv;
                    self.RoleInfo.Lv += addLevel;
                    saveValue = self.RoleInfo.Lv.ToString();
                    unit.OnUpgrageLevel(self.RoleInfo.Lv, oldLevel);
                    Function_Fight.UnitUpdateProperty_Base(unit, true,true );
                    self.BroadcastLevel(self.RoleInfo.Lv).Coroutine();
                    break;
                case UserDataType.Sp:
                    self.RoleInfo.Sp += int.Parse(value);
                    saveValue = self.RoleInfo.Sp.ToString();
                    break;
                case UserDataType.Gold:
                    self.RoleInfo.Gold += long.Parse(value);
                    saveValue = self.RoleInfo.Gold.ToString();
                    unit.GetComponent<ChengJiuComponentServer>().OnGetGold(int.Parse(value));
                    unit.GetComponent<TaskComponent>().OnCostCoin(int.Parse(value));
                    break;
                case UserDataType.WeiJingGold:
                    self.RoleInfo.WeiJingGold += long.Parse(value);
                    saveValue = self.RoleInfo.WeiJingGold.ToString();
                    break;
                case UserDataType.RongYu:
                    self.RoleInfo.RongYu += long.Parse(value);
                    saveValue = self.RoleInfo.RongYu.ToString();
                    break;
                case UserDataType.Diamond:
                    long addDiamond = long.Parse(value);
                    self.RoleInfo.Diamond += addDiamond;
                    self.RoleInfo.Diamond = Math.Max(self.RoleInfo.Diamond, 0);
                    saveValue = self.RoleInfo.Diamond.ToString();
                    if (addDiamond < 0)
                    {
                        unit.GetComponent<ChengJiuComponentServer>().OnCostDiamond(addDiamond);
                        unit.GetComponent<NumericComponent>().ApplyChange(null, NumericType.V1DayCostDiamond, addDiamond * - 1, 0);
                        long costdiamond = unit.GetComponent<NumericComponent>().GetAsLong(NumericType.V1DayCostDiamond);

                        //累计消耗钻石转换为积分
                        self.UpdateRoleData( UserDataType.V1TotalPoints,  (addDiamond * -0.01f).ToString());
                    }
                    break;
                case UserDataType.V1TotalPoints:
                    self.RoleInfo.V1TotalPoints += float.Parse(value);
                    self.RoleInfo.V1TotalPoints = Math.Max(self.RoleInfo.V1TotalPoints, 0);
                    saveValue = self.RoleInfo.V1TotalPoints.ToString();
                    break;
                case UserDataType.Occ:
                    break;
                case UserDataType.InvestMent:
                    unit.GetComponent<NumericComponent>().ApplyChange(null, NumericType.InvestMent, long.Parse(value), 0);
                    unit.GetComponent<NumericComponent>().ApplyChange(null, NumericType.InvestTotal, long.Parse(value), 0);
                    break;
                case UserDataType.JueXingExp:
                    unit.GetComponent<NumericComponent>().ApplyChange(null, NumericType.JueXingExp, long.Parse(value), 0);
                    break;
                case UserDataType.MaoXianExp:
                    unit.GetComponent<NumericComponent>().ApplyChange(null, NumericType.MaoXianExp, long.Parse(value), 0);
                    break;
                case UserDataType.Recharge:
                    RechargeHelp.SendDiamondToUnit(unit, int.Parse(value), "道具", 0);
                    break;
                case UserDataType.PiLao:
                    if (value == "0")
                    {
                        return;
                    }

                    int maxValue = 100;///unit.IsYueKaStates() ? int.Parse(LDGlobalValueCategory.Instance.Get(26).Value) : int.Parse(LDGlobalValueCategory.Instance.Get(10).Value);
                    long newValue = long.Parse(value) + self.RoleInfo.PiLao;
                    newValue = Math.Min(Math.Max(0, newValue), maxValue);
                    self.RoleInfo.PiLao = newValue;
                    saveValue = self.RoleInfo.PiLao.ToString();
                    break;
                case UserDataType.BaoShiDu:
                    long addValue = long.Parse(value);
                    newValue = self.RoleInfo.BaoShiDu + (int)addValue;
                    newValue = Math.Min(Math.Max(0, newValue), CommonHelper.GetMaxBaoShiDu());
                    self.RoleInfo.BaoShiDu = (int)newValue;
                    saveValue = self.RoleInfo.BaoShiDu.ToString();
                    unit.GetComponent<BuffManagerComponent>()?.InitBaoShiBuff();
                    break;
                case UserDataType.HuoYue:
                    break;
                case UserDataType.DungeonTimes:
                    unit.GetComponent<NumericComponent>().ApplyValue(NumericType.TeamDungeonTimes, unit.GetTeamDungeonTimes() - 1);
                    unit.GetComponent<NumericComponent>().ApplyValue(NumericType.TeamDungeonXieZhu, unit.GetTeamDungeonXieZhu() - 1);
                    self.RoleInfo.DayFubenTimes.Clear();
                    break;
                case UserDataType.UnionName:
                    self.RoleInfo.UnionName = value;
                    saveValue = self.RoleInfo.UnionName;
                    break;
                case UserDataType.DemonName:
                    self.RoleInfo.DemonName = value;
                    saveValue = self.RoleInfo.DemonName;
                    break;
                case UserDataType.StallName:
                    self.RoleInfo.StallName = value;
                    saveValue = self.RoleInfo.StallName;
                    break;
                case UserDataType.Combat:
                    self.RoleInfo.Combat = int.Parse(value);
                    saveValue = self.RoleInfo.Combat.ToString();
                    unit.GetComponent<ChengJiuComponentServer>().TriggerEvent(ChengJiuTargetEnum.CombatToValue_211, 0, self.RoleInfo.Combat);
                    unit.GetComponent<TaskComponent>().TriggerTaskEvent(TastConditionType.CombatToValue_133, 0, self.RoleInfo.Combat);
                    break;
                case UserDataType.Vitality:
                    NumericComponent numericComponent = unit.GetComponent<NumericComponent>();
                    int skillNumber = 1 + numericComponent.GetAsInt(NumericType.MakeType_2) > 0 ? 1 : 0;
                    maxValue = unit.GetMaxHuoLi(skillNumber);
                    addValue = long.Parse(value);
                    newValue = self.RoleInfo.Vitality + (int)addValue;
                    newValue = Math.Min(Math.Max(0, newValue), maxValue);
                    self.RoleInfo.Vitality = (int)newValue;
                    saveValue = self.RoleInfo.Vitality.ToString();
                    break;
                case UserDataType.BuffSkill:
                    longValue = long.Parse(value);
                    break;
                default:
                    saveValue = value;
                    break;
            }

            //发送更新值
            if (notice)
            {
                M2C_RoleDataUpdate m2C_RoleDataUpdate1 = self.m2C_RoleDataUpdate;
                m2C_RoleDataUpdate1.UpdateType = (int)Type;
                m2C_RoleDataUpdate1.UpdateTypeValue = saveValue;
                m2C_RoleDataUpdate1.UpdateValueLong = longValue;
                MessageHelper.SendToClient(self.GetParent<Unit>(), m2C_RoleDataUpdate1);
            }
        }

        public static async ETTask UploadCombat(this RoleInfoComponent self)
        {
            Unit unit = self.GetParent<Unit>();
            if (unit.IsRobot())
            {
                return;
            }

            NumericComponent numericComponent = unit.GetComponent<NumericComponent>();
            long mapInstanceId = StartSceneConfigCategory.Instance.GetBySceneName(self.DomainZone(), Enum.GetName(SceneType.Rank)).InstanceId;
            RankingInfo rankPetInfo = new RankingInfo();
            RoleInfoComponent roleInfoComponent = unit.GetComponent<RoleInfoComponent>();
            rankPetInfo.UserId = roleInfoComponent.RoleInfo.UserId;
            rankPetInfo.PlayerName = roleInfoComponent.RoleInfo.Name;
            rankPetInfo.PlayerLv = roleInfoComponent.RoleInfo.Lv;
            rankPetInfo.Combat = roleInfoComponent.RoleInfo.Combat;
            rankPetInfo.Occ = roleInfoComponent.RoleInfo.Occ;
            R2M_RankUpdateResponse Response = (R2M_RankUpdateResponse)await ActorMessageSenderComponent.Instance.Call
                     (mapInstanceId, new M2R_RankUpdateRequest()
                     {
                         CampId = numericComponent.GetAsInt(NumericType.AcvitiyCamp),
                         RankingInfo = rankPetInfo
                     });
            if (unit.IsDisposed)
            {
                return;
            }
            numericComponent.ApplyValue(NumericType.CombatRankID, Response.RankId);
            numericComponent.ApplyValue(NumericType.OccCombatRankID, Response.OccRankId);
            numericComponent.ApplyValue(NumericType.PetTianTiRankID, Response.PetRankId);
            numericComponent.ApplyValue(NumericType.SoloRankId, Response.SoloRankId);
        }

        public static void  UpdateRankInfo(this RoleInfoComponent self)
        {
            Unit unit = self.GetParent<Unit>();
            if (unit.IsRobot())
            {
                return;
            }
            self.UpdateRankTime = TimeHelper.ServerNow();
        }

        //增加经验
        public static void Role_AddExp(this RoleInfoComponent self, long addValue, bool notice)
        {
            Scene scene = self.DomainScene();
            ServerInfo serverInfo = ConfigData.ServerInfoList[scene.DomainZone()];
            if (serverInfo == null)
            {
                Log.Warning($"ServerInfo==null: {scene.GetComponent<MapComponent>().MapTypeEnum} {self.Id}");
                return;
            }
        
            float expAdd = CommonHelper.GetExpAdd(self.RoleInfo.Lv, serverInfo);

            LDExp xiulianconf1 = LDExpCategory.Instance.Get(self.RoleInfo.Lv);
            long upNeedExp = xiulianconf1.Exp_Role;

            TaskComponent taskComponent = self.GetParent<Unit>().GetComponent<TaskComponent>();

            //等级达到上限,则无法获得经验. 经验最多200%
            int maxlevel = self.GetMaxLevel(taskComponent.RoleComoleteTaskList);
            if (addValue > 0 &&self.RoleInfo.Lv >= maxlevel)
            {
                long maxExp = upNeedExp * 2;

                if (self.RoleInfo.Exp > maxExp && maxlevel == 70)
                {
                    self.UpdateRoleData(UserDataType.Message, "当前经验超过200%无法获得新的经验！请完成进阶之路任务开启新的等级，或前往主城经验老头处用多余的经验兑换奖励喔！");
                    return;
                }
                if (self.RoleInfo.Exp > maxExp && maxlevel == 75)
                {
                    self.UpdateRoleData(UserDataType.Message, "当前经验超过200%，请前往主城经验老头处用多余的经验兑换奖励喔！");
                    return;
                }

                if (self.RoleInfo.Exp > maxExp) 
                {
                    self.UpdateRoleData(UserDataType.Message, "当前经验超过200%，请前往主城经验老头处用多余的经验兑换奖励喔！");
                    return;
                }
            }

            self.RoleInfo.Exp = self.RoleInfo.Exp + (int)(addValue * (1.0f + expAdd));

            if (self.RoleInfo.Lv >= maxlevel)
            {
                return;
            }

            //判定是否升级
            if (self.RoleInfo.Lv >= serverInfo.WorldLv)
            {
                return;
            }

            if (self.RoleInfo.Exp >= upNeedExp)
            {
                self.RoleInfo.Exp -= upNeedExp;
                self.UpdateRoleData(UserDataType.Level, "1", notice);
            }
        }

        public static int GetRandomMonsterId(this RoleInfoComponent self)
        {
            List<KeyValuePairInt> dayMonster = self.RoleInfo.DayMonsters;
            List<DayMonsters> dayMonsterConfig = LDGlobalValueCategory.Instance.DayMonsterList;

            for (int i = 0; i < dayMonsterConfig.Count; i++)
            {
                if (RandomHelper.RandFloat01() > dayMonsterConfig[i].GaiLv)
                {
                    continue;
                }

                KeyValuePairInt keyValuePairInt = null;
                for (int d = 0; d < dayMonster.Count; d++)
                {
                    if (dayMonster[d].KeyId != dayMonsterConfig[i].MonsterId)
                    {
                        continue;
                    }
                    keyValuePairInt = dayMonster[d];
                }
                if (keyValuePairInt == null)
                {
                    keyValuePairInt = new KeyValuePairInt() { KeyId = dayMonsterConfig[i].MonsterId, Value = 0 };
                    dayMonster.Add(keyValuePairInt);
                }
                if (keyValuePairInt.Value < dayMonsterConfig[i].TotalNumber)
                {
                    keyValuePairInt.Value++;
                    return dayMonsterConfig[i].MonsterId;
                }
            }

            return 0;
        }

        public static int GetRandomJingLingId(this RoleInfoComponent self)
        {
            List<DayJingLing> dayMonsterConfig = LDGlobalValueCategory.Instance.DayJingLingList;
            List<int> dayMonster = self.RoleInfo.DayJingLing;
            for(int i = 0; i < dayMonsterConfig.Count; i++)
            {
                if (RandomHelper.RandFloat01() > dayMonsterConfig[i].GaiLv)
                {
                    continue;
                }
                if (dayMonster.Count <= i)
                {
                    for (int d = dayMonster.Count; d < i+1; d++)
                    {
                        dayMonster.Add(0);
                    }
                }
                if (dayMonster[i] >= dayMonsterConfig[i].TotalNumber)
                {
                    continue;
                }

                dayMonster[i]++;
                int randomIndex = RandomHelper.RandomByWeight(dayMonsterConfig[i].Weights);
                return dayMonsterConfig[i].MonsterId[randomIndex];
            }

            return 0;
        }

        public static void OnMakeItem(this RoleInfoComponent self, int makeId)
        {
            EquipMakeConfig equipMakeConfig = EquipMakeConfigCategory.Instance.Get(makeId);
            List<KeyValuePairInt> makeList = self.RoleInfo.MakeIdList;

            bool have = false;
            long endTime = TimeHelper.ServerNow() + equipMakeConfig.MakeTime * 1000;
            for (int i = 0; i < makeList.Count; i++)
            {
                if (makeList[i].KeyId == makeId)
                {
                    makeList[i].Value = endTime;
                    have = true;
                }
            }
            if (!have)
            {
                self.RoleInfo.MakeIdList.Add(new KeyValuePairInt() { KeyId = makeId, Value = endTime });
            }
        }

        public static void OnAddChests(this RoleInfoComponent self, int fubenId, int monsterId)
        {
            bool have = false;
            List<KeyValuePair> chestList = self.RoleInfo.OpenChestList;
            for (int i = 0; i < chestList.Count; i++)
            {
                if (chestList[i].KeyId == fubenId)
                {
                    chestList[i].Value += ($"_{monsterId}");
                    have = true;
                }
            }
            if (!have)
            {
                self.RoleInfo.OpenChestList.Add(new KeyValuePair() { KeyId = fubenId, Value = monsterId.ToString() });
            }
        }

        public static bool IsCheskOpen(this RoleInfoComponent self, int fubenId, int monsterId)
        {
            List<KeyValuePair> chestList = self.RoleInfo.OpenChestList;
            for (int i = 0; i < chestList.Count; i++)
            {
                if (chestList[i].KeyId == fubenId)
                {
                    return chestList[i].Value.Contains(monsterId.ToString());
                }
            }
            return false;
        }

        public static int OnGetFirstWinSelf(this RoleInfoComponent self, int firstwinid, int difficulty)
        {
            KeyValuePair keyValuePair1 = null;
            for (int i = 0; i < self.RoleInfo.FirstWinSelf.Count; i++)
            {
                if (self.RoleInfo.FirstWinSelf[i].KeyId != firstwinid)
                {
                    continue;
                }
                keyValuePair1 = self.RoleInfo.FirstWinSelf[i];
                break;
            }
            if (keyValuePair1 == null)
            {
                return ErrorCode.ERR_NetWorkError;
            }
            if (keyValuePair1.Value2.Contains(difficulty.ToString()))
            {
                return ErrorCode.ERR_AlreadyReceived;
            }
            if (string.IsNullOrEmpty(keyValuePair1.Value2))
            {
                keyValuePair1.Value2 = difficulty.ToString();
            }
            else
            {
                keyValuePair1.Value2 += $"_{difficulty}";
            }
            return ErrorCode.ERR_Success;
        }

        public static void OnAddFirstWinSelf(this RoleInfoComponent self, Unit boss, int difficulty)
        {
            if (difficulty == 0)
            {
                difficulty = 1;
            }
            int firstwinid = FirstWinHelper.GetFirstWinID(boss.ConfigId, difficulty);
            if (firstwinid == 0)
            {
                return;
            }

            bool have = false;
            for (int i = 0; i < self.RoleInfo.FirstWinSelf.Count; i++)
            {
                KeyValuePair keyValuePair = self.RoleInfo.FirstWinSelf[i];
                if (keyValuePair.KeyId != firstwinid)
                {
                    continue;
                }
                //keyValuePair.Value  击杀难度
                //keyValuePair.Value2 领取难度
                if (keyValuePair.Value.Contains(difficulty.ToString()))
                {
                    return;
                }

                keyValuePair.Value += $"_{difficulty}";
                have = true;
                break;
            }
            if (!have)
            {
                self.RoleInfo.FirstWinSelf.Add( new KeyValuePair() {  KeyId = firstwinid, Value = difficulty.ToString(), Value2 = "" } );
            }

            M2C_FirstWinSelfUpdateMessage m2C_FirstWinSelfUpdateMessage = new M2C_FirstWinSelfUpdateMessage() { FirstWinInfos = self.RoleInfo.FirstWinSelf  };
            MessageHelper.SendToClient( self.GetParent<Unit>(), m2C_FirstWinSelfUpdateMessage);
        }

        public static void OnCleanBossCD(this RoleInfoComponent self)
        {
            for (int i = 0; i < self.RoleInfo.MonsterRevives.Count; i++)
            {
                self.RoleInfo.MonsterRevives[i].Value = "0";
            }
        }

        public static void OnAddRevive(this RoleInfoComponent self, int monsterId, long reviveTime)
        {
            bool have = false;  
            for (int i = 0; i < self.RoleInfo.MonsterRevives.Count; i++)
            {
                KeyValuePair keyValuePair = self.RoleInfo.MonsterRevives[i];
                if (keyValuePair.KeyId != monsterId)
                {
                    continue;
                }
                if (string.IsNullOrEmpty(keyValuePair.Value2))
                {
                    keyValuePair.Value2 = "1";
                }

                keyValuePair.Value = reviveTime.ToString();
                keyValuePair.Value2 = (int.Parse(keyValuePair.Value2) + 1).ToString();  
                have = true;
                break;
            }
            if (!have)
            {
                self.RoleInfo.MonsterRevives.Add(new KeyValuePair() { KeyId = monsterId, Value = reviveTime.ToString(), Value2 = "1" });
            }

            M2C_UpdateUserInfoMessage m2C_UpdateUserInfo = new M2C_UpdateUserInfoMessage();
            m2C_UpdateUserInfo.RoleInfo = self.RoleInfo;
            MessageHelper.SendToClient( self.GetParent<Unit>(), m2C_UpdateUserInfo );
        }

        public static string GetGameSettingValue(this RoleInfoComponent self, GameSettingEnum gameSettingEnum)
        {
            for (int i = 0; i < self.RoleInfo.GameSettingInfos.Count; i++)
            {
                if (self.RoleInfo.GameSettingInfos[i].KeyId == (int)gameSettingEnum)
                    return self.RoleInfo.GameSettingInfos[i].Value;
            }
            switch (gameSettingEnum)
            {
                case GameSettingEnum.Music:
                    return "1";
                case GameSettingEnum.Sound:
                    return "0";
                // 0 固定 1移动
                case GameSettingEnum.YanGan:
                    return "0";
                case GameSettingEnum.FenBianlLv:
                    return "1";
                default:
                    return "0";
            }
        }

        public static void OnFubenSettlement(this RoleInfoComponent self, int levelid, int difficulty)
        {
            FubenPassInfo fubenPassInfo = null;
            for (int i = 0; i < self.RoleInfo.FubenPassList.Count; i++)
            {
                if (self.RoleInfo.FubenPassList[i].FubenId == levelid)
                {
                    fubenPassInfo = self.RoleInfo.FubenPassList[i];
                }
            }
            if (fubenPassInfo == null)
            {
                fubenPassInfo = new FubenPassInfo();
                fubenPassInfo.FubenId = levelid;
                self.RoleInfo.FubenPassList.Add(fubenPassInfo);
            }
            fubenPassInfo.Difficulty = (difficulty > fubenPassInfo.Difficulty) ? difficulty : fubenPassInfo.Difficulty;
        }

        public static bool IsLevelPassed(this RoleInfoComponent self, int levelid)
        {
            for (int i = 0; i < self.RoleInfo.FubenPassList.Count; i++)
            {
                if (self.RoleInfo.FubenPassList[i].FubenId == levelid)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool IsChapterOpen(this RoleInfoComponent self, int chapterid)
        {
            if (chapterid == 1)
            {
                return true;
            }
            /*if (!ChapterSectionConfigCategory.Instance.Contain(chapterid))
            {
                return false;
            }

            ChapterSectionConfig chapterSectionConfig = ChapterSectionConfigCategory.Instance.Get(chapterid - 1);
            int[] RandomArea = chapterSectionConfig.RandomArea;

            for (int i = 0; i < RandomArea.Length; i++)
            {
                if (!self.IsLevelPassed(RandomArea[i]))
                {
                    return false;
                }
            }*/
            return true;
        }

        public static int GetCrateDay(this RoleInfoComponent self)
        {
            return ServerHelper.DateDiff_Time(TimeHelper.ServerNow(), self.RoleInfo.CreateTime);
        }

        /// <summary>
        /// 高级是1  中级是2
        /// </summary>
        /// <param name="self"></param>
        /// <param name="level"></param>
        public static void OnGmGaoJi(this RoleInfoComponent self, int level)
        {
            int lv = level == 1 ? 70 - self.RoleInfo.Lv : 40 - self.RoleInfo.Lv;
            self.UpdateRoleData(UserDataType.Level, lv.ToString());

            self.RoleInfo.HorseIds.Clear();
            Dictionary<int, LDMount> allzuoqi = LDMountCategory.Instance.GetAll();
            foreach (( int zuoqiid, LDMount zuoQiShowConfig ) in allzuoqi)
            {
                self.RoleInfo.HorseIds.Add(zuoqiid);
            }
            self.GetParent<Unit>().GetComponent<NumericComponent>().ApplyValue(NumericType.HorseRide, self.RoleInfo.HorseIds[0]);
            self.GetParent<Unit>().GetComponent<NumericComponent>().ApplyValue(NumericType.HorseFightID, self.RoleInfo.HorseIds[0]);

            LDHome maxjiayuan = null;
            Dictionary<int, LDHome> allJiayuan = LDHomeCategory.Instance.GetAll();
            foreach ((int jiayualv, LDHome jiaYuanConfig) in allJiayuan)
            {
                maxjiayuan = jiaYuanConfig;
            }
            self.RoleInfo.JiaYuanLv = maxjiayuan.Id;


            SeasonLevelConfig maxseason = null;
            Dictionary<int, SeasonLevelConfig> allseason = SeasonLevelConfigCategory.Instance.GetAll(); 
            foreach ((int seasonid, SeasonLevelConfig seasonLevelConfig) in allseason )
            {
                maxseason = seasonLevelConfig;
            }
            self.RoleInfo.SeasonLevel = maxseason.Id;
        }

        public static void ClearDayData(this RoleInfoComponent self)
        {
            self.RoleInfo.DayFubenTimes.Clear();
            self.RoleInfo.ChouKaRewardIds.Clear();
            self.RoleInfo.MysteryItems.Clear();
            self.RoleInfo.DayItemUse.Clear();
            self.RoleInfo.DayMonsters.Clear();
            self.RoleInfo.DayJingLing.Clear();
            self.RoleInfo.PetExploreRewardIds.Clear();  
            self.RoleInfo.PetHeXinExploreRewardIds.Clear();
            self.RoleInfo.ItemXiLianNumRewardIds.Clear();
        }

    }

}

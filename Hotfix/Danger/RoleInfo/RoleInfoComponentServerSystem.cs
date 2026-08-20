using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ET
{

    //[Timer(TimerType.ShouLieUpLoadTimer)]
    //public class ShouLieUpLoadTimer : ATimer<RoleInfoComponentServer>
    //{
    //    public override void Run(RoleInfoComponentServer self)
    //    {
    //        try
    //        {
    //            self.UpdateShowLie().Coroutine();
    //        }
    //        catch (Exception e)
    //        {
    //            Log.Error($"move timer error: {self.Id}\n{e}");
    //        }
    //    }
    //}

    [ObjectSystem]
    public class RoleInfoComponentAwake : AwakeSystem<RoleInfoComponentServer>
    {
        public override void Awake(RoleInfoComponentServer self)
        {

        }
    }

    [ObjectSystem]
    public class RoleInfoComponentDestroy : DestroySystem<RoleInfoComponentServer>
    {
        public override void Destroy(RoleInfoComponentServer self)
        {
        }
    }

    public static class RoleInfoComponentServerSystem
    {

        public static void OnInit(this RoleInfoComponentServer self,string account, long userId, long accountId, CreateRoleInfo createRoleInfo)
        {
            self.Account = account;

            RoleInfo roleInfo = self.RoleInfo;
            roleInfo.Sp = 1;
            roleInfo.UserId = userId;
            roleInfo.AccInfoID =accountId;
            roleInfo.Name = createRoleInfo.PlayerName;
            roleInfo.ServerMailIdCur = -1;
            roleInfo.TiLi = 120;     //初始化疲劳
            //roleInfo.MakeList.AddRange(CommonHelper.StringArrToIntList(LDGlobalValueCategory.Instance.Get(18).Value.Split(';')));
            roleInfo.CreateTime = TimeHelper.ServerNow();
            roleInfo.Occ = createRoleInfo.PlayerOcc;
            roleInfo.CreateTime = createRoleInfo.CreateTime;

            if (createRoleInfo.RobotId > 0)
            {
                int robotId = int.Parse(account.Split('_')[0]);
                LDRobot ldRobot = LDRobotCategory.Instance.Get(robotId);
                //roleInfo.Lv = ldRobot.Behaviour == 1 ?  RandomHelper.RandomNumber(10, 19) : ldRobot.Level;
                //roleInfo.Occ = ldRobot.Behaviour == 1 ?  RandomHelper.RandomNumber(1, 3) : ldRobot.Occ;
                roleInfo.Gold = 100000;
                roleInfo.RobotId = robotId;
                //roleInfo.OccTwo = robotConfig.OccTwo;
            }
            else
            {
                roleInfo.Lv = 1;
                roleInfo.Gold = 0;
                //roleInfo.SeasonLevel = 1;
            }
        }
        
        public static void Check(this RoleInfoComponentServer self)
        {
            self.TodayOnLine++;
           
            if (self.UpdateCombatTime > 0 )
            {
                self.UpdateCombatTime = 0;
                self.UploadCombat().Coroutine();
            }
        }
        public static void OnRongyuChanChu(this RoleInfoComponentServer self, int coefficient, bool notice)
        {
            if (coefficient == 0)
            {
                return;
            }
          //  LingDiConfig lingDiConfig = LingDiConfigCategory.Instance.Get(lingdiLv);

            //unit.GetComponent<RoleInfoComponentServer>().UpdateRoleData(UserDataType.Exp, (coefficient *lingDiConfig.HoureExp).ToString(), notice).Coroutine();
           // self.UpdateRoleData(UserDataType.FangRong, (coefficient * lingDiConfig.HoureExp).ToString(), notice);
           // self.UpdateRoleData(UserDataType.RongYu, (coefficient * lingDiConfig.HoureHonor).ToString(), notice);
        }

        public static void OpenAll(this RoleInfoComponentServer self)
        {

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


        public static int GetTiLiTimes(this RoleInfoComponentServer self, int hour_1, int hour_2)
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
        public static List<int> GetTiLiIndexsNew(this RoleInfoComponentServer self, int hour_1, int hour_2)
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

        public static int GetTowerId(RoleInfo roleInfo, int sceneType)
        {
            if (roleInfo?.TowerIds == null)
            {
                return 0;
            }
            for (int i = 0; i < roleInfo.TowerIds.Count; i++)
            {
                if (roleInfo.TowerIds[i].KeyId == sceneType)
                {
                    return (int)roleInfo.TowerIds[i].Value;
                }
            }
            return 0;
        }

        public static void ApplyTowerId(RoleInfo roleInfo, string sceneTypeAndTowerId)
        {
            if (roleInfo == null || string.IsNullOrEmpty(sceneTypeAndTowerId))
            {
                return;
            }
            string[] parts = sceneTypeAndTowerId.Split(';');
            if (parts.Length < 2)
            {
                return;
            }
            ApplyTowerId(roleInfo, int.Parse(parts[0]), int.Parse(parts[1]));
        }

        public static void ApplyTowerId(RoleInfo roleInfo, int sceneType, int towerId)
        {
            if (roleInfo == null)
            {
                return;
            }
            if (roleInfo.TowerIds == null)
            {
                roleInfo.TowerIds = new List<KeyValuePairInt>();
            }
            for (int i = 0; i < roleInfo.TowerIds.Count; i++)
            {
                if (roleInfo.TowerIds[i].KeyId == sceneType)
                {
                    roleInfo.TowerIds[i].Value = towerId;
                    return;
                }
            }
            roleInfo.TowerIds.Add(new KeyValuePairInt() { KeyId = sceneType, Value = towerId });
        }

        public static int GetTiliRecover(this RoleInfoComponentServer self, List<int> indexids)
        {
            int totalTili = 0;
            int totalindex = indexids.Count;
            HashSet<int> indexIdSet = new HashSet<int>(indexids);
            if (totalindex >= 1 && indexIdSet.Contains(6))
            {
                totalTili += 50;
                totalindex--;
            }
            if (totalindex >= 1 && indexIdSet.Contains(20))
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

        public static int GetTiLiIndex(this RoleInfoComponentServer self, int hour_1)
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

        public static void CheckData(this RoleInfoComponentServer self)
        {
            Unit unit = self.GetParent<Unit>();

            for (int  i =  self.RoleInfo.HorseIds.Count - 1; i >= 0; i--)
            {
                if ( !LDMountCategory.Instance.Contain( self.RoleInfo.HorseIds[i]))
                {
                    self.RoleInfo.HorseIds.RemoveAt(i);
                }
            }

            RoleAddPointHelper.EnsureLevel1InitPoints(unit, self.RoleInfo.Lv);

            DataCollationComponent dataCollationComponent = unit.GetComponent<DataCollationComponent>();
            int recharge = (int)unit.GetTotalRechargeNum();
            if (recharge!=0 && dataCollationComponent.ChouKaTimes > (recharge * 2) && dataCollationComponent.ChouKaTimes > 100)
            {
                Log.Warning($"抽卡次数异常:{self.DomainZone()} {self.RoleInfo.Name}   充值:{recharge}  抽卡:{dataCollationComponent.ChouKaTimes}");
            }
        }

        public static void OnOffLine(this RoleInfoComponentServer self)
        {
            //self.LastLoginTime = TimeHelper.ServerNow();
        }

        public static void OnLogin(this RoleInfoComponentServer self, string remoteIp)
        {
            self.CheckData();
            self.RemoteAddress = remoteIp;
            self.UserName = self.RoleInfo.Name;
        }

        /// <summary>
        /// 体力
        /// </summary>
        /// <param name="self"></param>
        /// <param name="skillNumber"></param>
        /// <returns></returns>
        public static int GetAddPiLao(this RoleInfoComponentServer self, int skillNumber)
        {
            return 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="self"></param>
        /// <param name="notice"></param>
        public static void OnHourUpdate(this RoleInfoComponentServer self, int hour, bool notice)
        {
          
        }

        public static void RecoverPiLao(this RoleInfoComponentServer self, int addValue, bool notice)
        {
            //Unit unit = self.GetParent<Unit>();
            //long recoverPiLao = unit.GetMaxPiLao() - self.RoleInfo.PiLao;
            //recoverPiLao = Math.Min(recoverPiLao, addValue);

            //Log.Warning($"[增加体力] {unit.DomainZone()}    {unit.Id}    {recoverPiLao}");
            ////self.UpdateRoleData(UserDataType.PiLao, recoverPiLao.ToString(), notice);
            //self.LastLoginTime = TimeHelper.ServerNow();
        }


        public static void OnDailyReset(this RoleInfoComponentServer self, bool notice)
        {
            Unit unit = self.GetParent<Unit>();

            self.LastLoginTime = TimeHelper.ServerNow();
            self.TodayOnLine = 0;
        }

        public static RoleInfo GetUserInfo(this RoleInfoComponentServer self)
        {
            return self.RoleInfo;
        }

        public static void OnShowLieKill(this RoleInfoComponentServer self)
        {
           
            //if (self.ShouLieUpLoadTimer == 0)
            //{
            //    self.ShouLieUpLoadTimer = TimerComponent.Instance.NewOnceTimer(TimeHelper.ServerNow() + 5 * TimeHelper.Second, TimerType.ShouLieUpLoadTimer, self);
            //}
            //else
            //{
            //    self.UpdateShowLie().Coroutine();
            //}
        }

        public static async ETTask UpdateShowLie(this RoleInfoComponentServer self)
        {
            Unit unit = self.GetParent<Unit>();
            if (!ConfigData.ShowLieOpen || unit.IsRobot())
            {
                return;
            }
            RankShouLieInfo rankPetInfo = new RankShouLieInfo();
            rankPetInfo.UnitID = self.RoleInfo.UserId;
            rankPetInfo.PlayerName = self.RoleInfo.Name;
            rankPetInfo.Occ = self.RoleInfo.Occ;
            rankPetInfo.KillNumber = 0;/// self.ShouLieKill;
            long mapInstanceId = DBHelper.GetRankServerId(unit);
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
        public static void OnKillUnit(this RoleInfoComponentServer self, Unit beKill, int sceneType, int sceneId)
        {
            Unit main = self.GetParent<Unit>();
            if (beKill.Type != UnitType.Monster)
            {
                return;
            }

            bool showlieopen = ConfigData.ShowLieOpen;
            LDMonster ldMonster = LDMonsterCategory.Instance.Get(beKill.ConfigId);
            if (showlieopen && ( ldMonster.Lv >= 60 || Mathf.Abs(self.RoleInfo.Lv - ldMonster.Lv) <= 9) )
            {
                self.OnShowLieKill();
           
            }

            if (SeasonHelper.GetOpenSeason(self.RoleInfo.Lv)!=null && beKill.IsBoss() && ldMonster.Lv >= 40)
            {
                int seasonExp = RandomHelper.RandomNumber(1, 6);
            }

            NumericComponent numericComponent = main.GetComponent<NumericComponent>();

            RoleDailyDataComponentServer dailyData = main.GetComponent<RoleDailyDataComponentServer>();
            int tiliKillNumber = dailyData?.GetTiLiKillNumber() ?? 0;
            if (sceneType == MapTypeEnum.LocalDungeon && !showlieopen && self.RoleInfo.TiLi > 0)
            {
                if (tiliKillNumber >= 4)
                {
                    dailyData?.SetTiLiKillNumber(0, false);
                    //if ( CommonHelper.IsZhuBoZone(UnitZoneHelper.GetHomeZone(main)) && self.RoleInfo.PiLao < 2)
                    //{
                    //    self.UpdateRoleData(UserDataType.PiLao, "100", true);
                    //}
                    //else
                    //{
                    //    self.UpdateRoleData(UserDataType.PiLao, "-1", true);
                    //}
                }
                else
                {
                    dailyData?.AddTiLiKillNumber();
                }
            }

            bool drop = true;
            if (SceneConfigHelper.IsSingleFuben(sceneType))
            {
                drop = self.RoleInfo.TiLi > 0 || beKill.IsBoss() || showlieopen;
            }
            if (drop)
            {
                float expcoefficient = 1f;
                if (sceneType == MapTypeEnum.LocalDungeon && beKill.IsBoss())
                {
                    int killNumber = self.GetMonsterKillNumber(ldMonster.Id);
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
              
                int addexp = (int)(expcoefficient * 0);
                self.UpdateRoleData(UserDataType.Exp, addexp.ToString());
            }

      
        }

        public static void UpdateRoleDataBroadcast(this RoleInfoComponentServer self, int Type, string value)
        {
            Unit unit = self.GetParent<Unit>();
            M2C_RoleDataBroadcast m2C_BroadcastRoleData = self.m2C_RoleDataBroadcast;
            m2C_BroadcastRoleData.UnitId = unit.Id;
            m2C_BroadcastRoleData.UpdateType = (int)Type;
            m2C_BroadcastRoleData.UpdateTypeValue = value;
            MessageHelper.Broadcast(unit, m2C_BroadcastRoleData);
        }

        private static int FindKeyValuePairIndex(List<KeyValuePairInt> list, int keyId)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].KeyId == keyId)
                {
                    return i;
                }
            }
            return -1;
        }

        public static int GetMysteryBuy(this RoleInfoComponentServer self, int mysteryId)
        {
            return self.GetParent<Unit>()?.GetComponent<RoleDailyDataComponentServer>()?.GetMysteryBuy(mysteryId) ?? 0;
        }

        public static void OnMysteryBuy(this RoleInfoComponentServer self, int mysteryId)
        {
            self.GetParent<Unit>()?.GetComponent<RoleDailyDataComponentServer>()?.OnMysteryBuy(mysteryId);
        }

        public static int GetStoreBuy(this RoleInfoComponentServer self, int mysteryId)
        {
            return self.GetParent<Unit>()?.GetComponent<RoleDailyDataComponentServer>()?.GetBuyStorePeriod(mysteryId) ?? 0;
        }

        public static void OnShopBuy(this RoleInfoComponentServer self, int mysteryId)
        {
            self.OnShopBuy(mysteryId, 1);
        }

        public static void OnShopBuy(this RoleInfoComponentServer self, int mysteryId, int buyNumber)
        {
            self.GetParent<Unit>()?.GetComponent<RoleDailyDataComponentServer>()
                    ?.AddShopBuy(mysteryId, buyNumber, true, false);
        }

        public static async ETTask SendUnionOperate(this RoleInfoComponentServer self, int getWay, int dataType,  long dataValue)
        {
            Unit unit = self.GetParent<Unit>();
            long unionid = unit.GetUnionId();
            if (unionid == 0)
            {
                return;
            }
            string playerName = self.RoleInfo.Name;
            long serverod = DBHelper.GetUnionServerId(unit);
            U2M_UnionOperationResponse responseUnionEnter = (U2M_UnionOperationResponse)await ActorMessageSenderComponent.Instance.Call(
                            serverod, new M2U_UnionOperationRequest() { OperateType = 1, UnionId = unionid, Par = $"{playerName}_{getWay}_{dataType}_{dataValue}" });
        }

        public static async ETTask BroadcastLevel(this RoleInfoComponentServer self, int level)
        {
            Unit unit = self.GetParent<Unit>();
            long chatServerId = DBHelper.GetChatServerId(unit);
            Chat2M_UpdateLevel chat2G_EnterChat = (Chat2M_UpdateLevel)await MessageHelper.CallActor(chatServerId, new M2Chat_UpdateLevel()
            {
                UnitId = unit.Id,
                Level = level,
            });
        }

        //需要通知客户端
        public static void UpdateRoleData(this RoleInfoComponentServer self, int Type, string value, bool notice = true, int getWay = ItemGetWay.System, string paramsifo = "")
        {
            Unit unit = self.GetParent<Unit>();
            if (Type == UserDataType.Gold
                || Type == UserDataType.BindGold
                || Type == UserDataType.Diamond
                || Type == UserDataType.BindDiamond)
            {
                long gold = long.Parse(value);
                if (gold >= 0)
                {
                    AntiCheatAuditHelper.LogMoneyAdd(unit, Type, gold, getWay, self.RoleInfo.Name, paramsifo);
                    PlayerEconomyHelper.NotifyAfterMoneyAdd(unit, gold, getWay);
                    PlayerEconomyHelper.RecordMoneyGetWay(self.RoleInfo, Type, getWay);
                    unit.GetComponent<DataCollationComponent>().UpdateRoleMoneyAdd(Type, getWay, gold);
                }
                else
                {
                    AntiCheatAuditHelper.LogMoneySub(unit, Type, gold, getWay, self.RoleInfo.Name);
                    unit.GetComponent<DataCollationComponent>().UpdateRoleMoneySub(Type, getWay, gold);
                }
            }

            NumericComponent numericComponent = null;
            string saveValue = "";
            long longValue = 0;
            switch (Type)
            {
                
                case UserDataType.JiaYuanExp:
                    unit.GetComponent<JiaYuanComponentServer>()?.AddJiaYuanExp(int.Parse(value));
                    return;
                case UserDataType.JiaYuanFund:
                    unit.GetComponent<JiaYuanComponentServer>()?.AddJiaYuanFund(long.Parse(value));
                    return;
                case UserDataType.UnionContri:
                    self.RoleInfo.UnionZiJin += int.Parse(value);
                    saveValue = self.RoleInfo.UnionZiJin.ToString();
                    break;
                case UserDataType.DailyActive:
                case UserDataType.WeeklyActive:
                    unit.GetComponent<RoleDailyDataComponentServer>()?.AddActivePoint(Type, int.Parse(value), notice);
                    return;
              
                case UserDataType.JiaYuanLv:
                    unit.GetComponent<JiaYuanComponentServer>()?.AddJiaYuanLv(int.Parse(value));
                    return;
                
                //名字应该在改名的协议处理
                case UserDataType.Name:
                    self.RoleInfo.Name = value;
                    saveValue = self.RoleInfo.Name;
                    break;
                case UserDataType.Exp:
                  
                    self.Role_AddExp(long.Parse(value), notice);
                    //saveValue = self.RoleInfo.Exp.ToString();
                    longValue = self.RoleInfo.Exp;
                    saveValue = value;
                    break;
                case UserDataType.Level:
                    int addLevel = int.Parse(value);
                    int oldLevel = self.RoleInfo.Lv;
                    self.RoleInfo.Lv += addLevel;
                    saveValue = self.RoleInfo.Lv.ToString();
                    RoleAddPointHelper.AddPointsForLevelRange(unit, oldLevel, self.RoleInfo.Lv);
                    numericComponent ??= unit.GetComponent<NumericComponent>();
                    PlayerEconomyHelper.NotifyRoleDataProgression(unit, Type, self.RoleInfo);
                    Function_Fight.UnitUpdateProperty_Base(unit, true, true);
                    // 升级后按新上限回满（ResetProperty 保留 HP_Current，但 Max 可能变大）
                    numericComponent.Set(NumericType.HP_Current_8, numericComponent.GetAsLong(NumericType.HP_Max_10), true);
                    self.UpdateRankInfo();
                    self.BroadcastLevel(self.RoleInfo.Lv).Coroutine();
                    break;
                case UserDataType.Sp:
                    self.RoleInfo.Sp += int.Parse(value);
                    saveValue = self.RoleInfo.Sp.ToString();
                    break;
                case UserDataType.Gold:
                    long goldChange = long.Parse(value);
                    self.RoleInfo.Gold += goldChange;
                    saveValue = self.RoleInfo.Gold.ToString();
                    PlayerEconomyHelper.NotifyRoleDataProgression(unit, Type, self.RoleInfo, goldChange);
                    break;
                case UserDataType.BindGold:
                    self.RoleInfo.BindGold += long.Parse(value);
                    saveValue = self.RoleInfo.BindGold.ToString();
                    break;
                case UserDataType.Diamond:
                    long addDiamond = long.Parse(value);
                    self.RoleInfo.Diamond += addDiamond;
                    self.RoleInfo.Diamond = Math.Max(self.RoleInfo.Diamond, 0);
                    saveValue = self.RoleInfo.Diamond.ToString();
                    if (addDiamond < 0)
                    {
                        //累计消耗钻石转换为积分
                    }
                    break;
                case UserDataType.BindDiamond:
                    addDiamond = long.Parse(value);
                    self.RoleInfo.BindDiamond += addDiamond;
                    self.RoleInfo.BindDiamond = Math.Max(self.RoleInfo.BindDiamond, 0);
                    saveValue = self.RoleInfo.BindDiamond.ToString();
                    break;
                case UserDataType.Occ:
                    break;
                //case UserDataType.PiLao:
                //    if (value == "0")
                //    {
                //        return;
                //    }

                //    int maxValue = 100;///unit.IsYueKaStates() ? int.Parse(LDGlobalValueCategory.Instance.Get(26).Value) : int.Parse(LDGlobalValueCategory.Instance.Get(10).Value);
                //    long newValue = long.Parse(value) + self.RoleInfo.PiLao;
                //    newValue = Math.Min(Math.Max(0, newValue), maxValue);
                //    self.RoleInfo.PiLao = newValue;
                //    saveValue = self.RoleInfo.PiLao.ToString();
                //    break;
              
                case UserDataType.UnionName:
                    self.RoleInfo.UnionName = value;
                    saveValue = self.RoleInfo.UnionName;
                    break;
                case UserDataType.TowerId:
                    ApplyTowerId(self.RoleInfo, value);
                    saveValue = value;
                    break;
         
                case UserDataType.Combat:
                    self.RoleInfo.Combat = int.Parse(value);
                    saveValue = self.RoleInfo.Combat.ToString();
                    PlayerEconomyHelper.NotifyRoleDataProgression(unit, Type, self.RoleInfo);
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
                MessageHelper.SendToClient(unit, m2C_RoleDataUpdate1);
            }
        }

        public static async ETTask UploadCombat(this RoleInfoComponentServer self)
        {
            Unit unit = self.GetParent<Unit>();
            if (unit.IsRobot())
            {
                return;
            }

            NumericComponent numericComponent = unit.GetComponent<NumericComponent>();
            long mapInstanceId = DBHelper.GetRankServerId(unit);
            RankingInfo rankPetInfo = new RankingInfo();
            rankPetInfo.UserId = self.RoleInfo.UserId;
            rankPetInfo.PlayerName = self.RoleInfo.Name;
            rankPetInfo.PlayerLv = self.RoleInfo.Lv;
            rankPetInfo.Combat = self.RoleInfo.Combat;
            rankPetInfo.Occ = self.RoleInfo.Occ;

            R2M_RankUpdateResponse Response = (R2M_RankUpdateResponse)await ActorMessageSenderComponent.Instance.Call
                     (mapInstanceId, new M2R_RankUpdateRequest()
                     {
                         CampId = 0,
                         RankingInfo = rankPetInfo
                     });
            if (unit.IsDisposed)
            {
                return;
            }
            numericComponent.ApplyValue(NumericType.CombatRankID, Response.RankId);
            numericComponent.ApplyValue(NumericType.OccCombatRankID, Response.OccRankId);
            numericComponent.ApplyValue(NumericType.SoloRankId, Response.SoloRankId);

            // 同步上报战区排行（不影响本服名次）
            self.UploadWarCombat(0, rankPetInfo).Coroutine();
        }

        /// <summary>上报战区战力榜；展示名带服前缀</summary>
        public static async ETTask UploadWarCombat(this RoleInfoComponentServer self, int campId, RankingInfo homeRankInfo)
        {
            Unit unit = self.GetParent<Unit>();
            if (unit == null || unit.IsDisposed || unit.IsRobot())
            {
                return;
            }

            long warRankServerId = DBHelper.GetWarRankServerId(unit);
            if (warRankServerId == 0)
            {
                return;
            }

            RankingInfo warRankInfo = new RankingInfo();
            warRankInfo.UserId = homeRankInfo.UserId;
            warRankInfo.PlayerLv = homeRankInfo.PlayerLv;
            warRankInfo.Combat = homeRankInfo.Combat;
            warRankInfo.Occ = homeRankInfo.Occ;
            int homeZone = UnitZoneHelper.GetHomeZone(unit);
            ServerItem serverItem = ServerHelper.GetGetServerItem(CommonHelper.IsInnerNet(), homeZone);
            string serverName = serverItem != null ? serverItem.ServerName : homeZone.ToString();
            warRankInfo.PlayerName = $"[{serverName}]{homeRankInfo.PlayerName}";

            await ActorMessageSenderComponent.Instance.Call(warRankServerId, new M2R_RankUpdateRequest()
            {
                CampId = campId,
                RankingInfo = warRankInfo
            });
        }

        public static void  UpdateRankInfo(this RoleInfoComponentServer self)
        {
            Unit unit = self.GetParent<Unit>();
            if (unit.IsRobot())
            {
                return;
            }
            self.UpdateCombatTime = TimeHelper.ServerNow();
        }

        //增加经验
        public static void Role_AddExp(this RoleInfoComponentServer self, long addValue, bool notice)
        {
            Scene scene = self.DomainScene();
            Unit unit = self.GetParent<Unit>();
            int homeZone = UnitZoneHelper.GetHomeZone(unit);
            if (!ConfigData.ServerInfoList.TryGetValue(homeZone, out ServerInfo serverInfo) || serverInfo == null)
            {
                Log.Warning($"ServerInfo==null: home={homeZone} map={scene.GetComponent<MapComponent>()?.MapTypeEnum} {self.Id}");
                return;
            }
        
            float expAdd = CommonHelper.GetExpAdd(self.RoleInfo.Lv, serverInfo);

            LDExp_Lv xiulianconf1 = LDExp_LvCategory.Instance.Get(self.RoleInfo.Lv);
            long upNeedExp = xiulianconf1.Exp_Role;

            TaskComponentServer taskComponentServer = unit.GetComponent<TaskComponentServer>();

            //等级达到上限,则无法获得经验. 经验最多200%
            int maxlevel = self.GetMaxLevel(taskComponentServer.RoleComoleteTaskList);
            if (addValue > 0 &&self.RoleInfo.Lv >= maxlevel)
            {
                long maxExp = upNeedExp * 2;
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

        public static void OnMakeItem(this RoleInfoComponentServer self, int makeId)
        {
            //LDMake equipMakeConfig = LDMakeCategory.Instance.Get(makeId);
            //List<KeyValuePairInt> makeList = self.RoleInfo.MakeIdList;

            //bool have = false;
            //long endTime = 0;// TimeHelper.ServerNow() + equipMakeConfig.MakeTime * 1000;
            //for (int i = 0; i < makeList.Count; i++)
            //{
            //    if (makeList[i].KeyId == makeId)
            //    {
            //        makeList[i].Value = endTime;
            //        have = true;
            //    }
            //}
            //if (!have)
            //{
            //    self.RoleInfo.MakeIdList.Add(new KeyValuePairInt() { KeyId = makeId, Value = endTime });
            //}
        }

        /// <summary>
        /// 领悟配方。ItemType=98，ItemTypeParam1=LDSkill_Make.Id。已学会则忽略。
        /// </summary>
        public static void LearnRecipe(this RoleInfoComponentServer self, int makeId)
        {
            RoleInfo roleInfo = self.RoleInfo;
            if (roleInfo.MakeIdList == null)
            {
                roleInfo.MakeIdList = new List<int>();
            }
            if (roleInfo.MakeIdList.Contains(makeId))
            {
                return;
            }

            roleInfo.MakeIdList.Add(makeId);
            Unit unit = self.GetParent<Unit>();
            MessageHelper.SendToClient(unit, new M2C_UpdateUserInfoMessage { RoleInfo = roleInfo });
            DBHelper.SaveComponentCache(UnitZoneHelper.GetHomeZone(unit), unit.Id, self).Coroutine();
        }

        public static void OnAddChests(this RoleInfoComponentServer self, int fubenId, int monsterId)
        {
            // OpenChestList 已从 RoleInfo 移除
        }

        public static bool IsCheskOpen(this RoleInfoComponentServer self, int fubenId, int monsterId)
        {
            return false;
        }

        public static int GetRandomMonsterId(this RoleInfoComponentServer self)
        {
            return 0;
        }

        public static int GetRandomJingLingId(this RoleInfoComponentServer self)
        {
            return 0;
        }

        public static int GetTotalUseTimes(this RoleInfoComponentServer self, int itemId)
        {
            return 0;
        }

        public static void OnTotalUseTimes(this RoleInfoComponentServer self, int itemId)
        {
        }

        public static int OnGetFirstWinSelf(this RoleInfoComponentServer self, int firstwinid, int difficulty)
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
            HashSet<string> receivedDifficulties = new HashSet<string>(keyValuePair1.Value2.Split('_'));
            if (receivedDifficulties.Contains(difficulty.ToString()))
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

        public static void OnAddFirstWinSelf(this RoleInfoComponentServer self, Unit boss, int difficulty)
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

        public static void OnCleanBossCD(this RoleInfoComponentServer self)
        {
            // MonsterRevives 已从 RoleInfo 移除
        }

        public static void OnAddRevive(this RoleInfoComponentServer self, int monsterId, long reviveTime)
        {
            // MonsterRevives 已从 RoleInfo 移除
        }

        public static string GetGameSettingValue(this RoleInfoComponentServer self, GameSettingEnum gameSettingEnum)
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

        public static void OnResetSeason(this RoleInfoComponentServer self, bool notice)
        {
            //self.RoleInfo.SeasonLevel = 1;
            //self.RoleInfo.SeasonExp = 0;
            //self.RoleInfo.SeasonCoin = 0;
            //self.RoleInfo.OpenJingHeIds.Clear();
        }


        public static int GetMonsterKillNumber(this RoleInfoComponentServer self, int monsterId)
        {
            return 0;
        }

        public static long GetReviveTime(this RoleInfoComponentServer self, int monsterId)
        {
            return 0;
        }
       
        public static long GetSceneFubenTimes(this RoleInfoComponentServer self, int sceneId)
        {
            return self.GetParent<Unit>()?.GetComponent<RoleDailyDataComponentServer>()?.GetSceneFubenTimes(sceneId) ?? 0;
        }

        public static void AddSceneFubenTimes(this RoleInfoComponentServer self, int sceneId)
        {
            self.GetParent<Unit>()?.GetComponent<RoleDailyDataComponentServer>()?.AddSceneFubenTimes(sceneId);
        }

        public static void ClearFubenTimes(this RoleInfoComponentServer self, int sceneId)
        {
            self.GetParent<Unit>()?.GetComponent<RoleDailyDataComponentServer>()?.ClearFubenTimes(sceneId);
        }

        public static int GetMaxLevel(this RoleInfoComponentServer self, List<int> compeltetask)
        {
            if (compeltetask.Contains(30080019))
            {
                return LDGlobalValueCategory.Instance.MaxLevel;
            }
            else
            {
                return 70;
            }
        }

        public static void AddFubenTimes(this RoleInfoComponentServer self, int sceneId, int times)
        {
            self.GetParent<Unit>()?.GetComponent<RoleDailyDataComponentServer>()?.AddFubenTimes(sceneId, times);
        }

        
        public static bool IsChapterOpen(this RoleInfoComponentServer self, int chapterid)
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

        public static int GetCrateDay(this RoleInfoComponentServer self)
        {
            return ServerHelper.DateDiff_Time(TimeHelper.ServerNow(), self.RoleInfo.CreateTime);
        }

        /// <summary>
        /// 高级是1  中级是2
        /// </summary>
        /// <param name="self"></param>
        /// <param name="level"></param>
        public static void OnGmGaoJi(this RoleInfoComponentServer self, int level)
        {
            int lv = level == 1 ? 70 - self.RoleInfo.Lv : 40 - self.RoleInfo.Lv;
            self.UpdateRoleData(UserDataType.Level, lv.ToString());

            self.RoleInfo.HorseIds.Clear();
            Dictionary<int, LDMount> allzuoqi = LDMountCategory.Instance.GetAll();
            foreach (( int zuoqiid, LDMount zuoQiShowConfig ) in allzuoqi)
            {
                self.RoleInfo.HorseIds.Add(zuoqiid);
            }
            NumericComponent numeric = self.GetParent<Unit>().GetComponent<NumericComponent>();
            numeric.ApplyValue(NumericType.HorseRide, self.RoleInfo.HorseIds[0]);
            numeric.ApplyValue(NumericType.HorseFightID, self.RoleInfo.HorseIds[0]);

            LDHome maxjiayuan = null;
            Dictionary<int, LDHome> allJiayuan = LDHomeCategory.Instance.GetAll();
            foreach ((int jiayualv, LDHome jiaYuanConfig) in allJiayuan)
            {
                maxjiayuan = jiaYuanConfig;
            }
            self.GetParent<Unit>().GetComponent<JiaYuanComponentServer>().JiaYuanLv = maxjiayuan.Id;

            /*
            SeasonLevelConfig maxseason = null;
            Dictionary<int, SeasonLevelConfig> allseason = SeasonLevelConfigCategory.Instance.GetAll(); 
            foreach ((int seasonid, SeasonLevelConfig seasonLevelConfig) in allseason )
            {
                maxseason = seasonLevelConfig;
            }
          
            self.RoleInfo.SeasonLevel = maxseason.Id;
        */
        }

        public static void ClearDayData(this RoleInfoComponentServer self)
        {
            self.GetParent<Unit>()?.GetComponent<RoleDailyDataComponentServer>()?.ClearDayLists(RoleDailyClearType.Day);
        }

    }

}

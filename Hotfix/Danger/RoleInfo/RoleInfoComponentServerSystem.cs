using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ET
{

 

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

            LDOccupation lDOccupation = LDOccupationCategory.Instance.Get(createRoleInfo.PlayerOcc);

            RoleInfo roleInfo = self.RoleInfo;
            RoleCurrencyHelper.Set(roleInfo, UserDataType.Sp, 1);
            roleInfo.UserId = userId;
            roleInfo.AccInfoID =accountId;
            roleInfo.Name = createRoleInfo.PlayerName;
            roleInfo.ServerMailIdCur = -1;
            RoleCurrencyHelper.Set(roleInfo, UserDataType.TiLi, 120);     //初始化体力
            RoleCurrencyHelper.Set(roleInfo, UserDataType.HuoLi, 120);     //初始化活力
            //roleInfo.MakeList.AddRange(CommonHelper.StringArrToIntList(LDGlobalValueCategory.Instance.Get(18).Value.Split(';')));
            roleInfo.CreateTime = TimeHelper.ServerNow();
            roleInfo.Occ = createRoleInfo.PlayerOcc;
            roleInfo.Sex = createRoleInfo.Sex;
            roleInfo.CreateTime = createRoleInfo.CreateTime;
            roleInfo.RoleHeadId = createRoleInfo.Sex == SexEnum.Man ? lDOccupation.RoleHead_Man : lDOccupation.RoleHead_Woman;
            roleInfo.RoleHeadFrame = 1;
            roleInfo.AddRoleHead(roleInfo.RoleHeadId);
            roleInfo.AddRoleHeadFrame(roleInfo.RoleHeadFrame);

            if (createRoleInfo.RobotId > 0)
            {
                int robotId = int.Parse(account.Split('_')[0]);
                LDRobot ldRobot = LDRobotCategory.Instance.Get(robotId);
                //roleInfo.Lv = ldRobot.Behaviour == 1 ?  RandomHelper.RandomNumber(10, 19) : ldRobot.Level;
                //roleInfo.Occ = ldRobot.Behaviour == 1 ?  RandomHelper.RandomNumber(1, 3) : ldRobot.Occ;
                RoleCurrencyHelper.Set(roleInfo, UserDataType.Gold, 100000);
                roleInfo.RobotId = robotId;
                //roleInfo.OccTwo = robotConfig.OccTwo;
            }
            else
            {
                roleInfo.Lv = 1;
                RoleCurrencyHelper.Set(roleInfo, UserDataType.Gold, 0);
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
                roleInfo.TowerIds = new List<IntLongPair>();
            }
            for (int i = 0; i < roleInfo.TowerIds.Count; i++)
            {
                if (roleInfo.TowerIds[i].KeyId == sceneType)
                {
                    roleInfo.TowerIds[i].Value = towerId;
                    return;
                }
            }
            roleInfo.TowerIds.Add(new IntLongPair() { KeyId = sceneType, Value = towerId });
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

        public const int MakeExpMax = 2000000000;
        public const int MakeTypeCount = 5;

        public static void CheckData(this RoleInfoComponentServer self)
        {
            RoleInfo roleInfo = self.RoleInfo;
            bool makeChanged = false;
            EnsureMakeProgress(roleInfo);
            for (int type = 1; type <= MakeTypeCount; type++)
            {
                if (roleInfo.MakeLevelList[type] <= 0)
                {
                    roleInfo.MakeLevelList[type] = 1;
                    makeChanged = true;
                }

                if (roleInfo.MakeExpList[type] > MakeExpMax)
                {
                    roleInfo.MakeExpList[type] = MakeExpMax;
                    makeChanged = true;
                }
                else if (roleInfo.MakeExpList[type] < 0)
                {
                    roleInfo.MakeExpList[type] = 0;
                    makeChanged = true;
                }
            }

            if (self.AutoLearnMakeByLevel())
            {
                makeChanged = true;
            }

            Unit unit = self.GetParent<Unit>();
            if (makeChanged)
            {
                DBHelper.SaveComponentCache(UnitZoneHelper.GetHomeZone(unit), unit.Id, self).Coroutine();
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
            self.LastOfflineTime = TimeHelper.ServerNow();
        }

        public static void OnLogin(this RoleInfoComponentServer self, string remoteIp)
        {
            self.CheckData();
            self.RemoteAddress = remoteIp;
            self.UserName = self.RoleInfo.Name;
        }

        /// <summary>名字、等级、战力、职业、转职、头像、头像框变更时写入公会成员快照。登录不推。</summary>
        public static void SyncUnionMember(this RoleInfoComponentServer self)
        {
            Unit unit = self.GetParent<Unit>();
            if (unit == null)
            {
                return;
            }

            long unionId = unit.GetUnionId();
            if (unionId == 0)
            {
                return;
            }

            RoleInfo roleInfo = self.RoleInfo;
            ActorMessageSenderComponent.Instance.Call(
                DBHelper.GetUnionServerId(unit),
                new M2U_UnionMemberSyncRequest()
                {
                    UnionId = unionId,
                    UnitId = unit.Id,
                    PlayerName = roleInfo.Name,
                    PlayerLevel = roleInfo.Lv,
                    Combat = roleInfo.Combat,
                    Occ = roleInfo.Occ,
                    OccTwo = roleInfo.OccTwo,
                    RoleHeadId = roleInfo.RoleHeadId,
                    RoleHeadFrame = roleInfo.RoleHeadFrame,
                }).Coroutine();
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
            
            NumericComponent numericComponent = main.GetComponent<NumericComponent>();

            RoleDailyDataComponentServer dailyData = main.GetComponent<RoleDailyDataComponentServer>();

            bool drop = true;
            if (SceneConfigHelper.IsSingleFuben(sceneType))
            {
                drop = RoleCurrencyHelper.Get(self.RoleInfo, UserDataType.TiLi) > 0 || beKill.IsBoss() || showlieopen;
            }
            if (drop)
            {
                float expcoefficient = 1f;
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

        private static int FindKeyValuePairIndex(List<IntLongPair> list, int keyId)
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

        /// <summary>
        /// 公会名只作为玩家身上的显示缓存（真名在 UnionInfo）。改档案并推 AOI，调用方只走这一次。
        /// </summary>
        public static void SetUnionName(this RoleInfoComponentServer self, string unionName, bool notice = true)
        {
            if (unionName == null)
            {
                unionName = string.Empty;
            }

            Unit unit = self.GetParent<Unit>();
            if (unit == null)
            {
                self.RoleInfo.UnionName = unionName;
                return;
            }

            self.UpdateRoleData(UserDataType.UnionName, unionName, notice);
            if (notice)
            {
                self.UpdateRoleDataBroadcast(UserDataType.UnionName, unionName);
            }
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
                
                case UserDataType.DailyActive:
                case UserDataType.WeeklyActive:
                    ActivePointHelper.Add(unit, Type, int.Parse(value), notice);
                    return;
              
                case UserDataType.HomeLv:
                    unit.GetComponent<HomeComponentServer>()?.AddHomeLv(int.Parse(value));
                    return;
                
                //名字应该在改名的协议处理
                case UserDataType.Name:
                    self.RoleInfo.Name = value;
                    saveValue = self.RoleInfo.Name;
                    break;
                case UserDataType.Exp:
                  
                    self.Role_AddExp(long.Parse(value), notice);
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
                    numericComponent.Set(NumericType.HP_Current_8, numericComponent.GetAsLong(NumericType.HP_Max_10), true);
                    self.UpdateRankInfo();
                    self.BroadcastLevel(self.RoleInfo.Lv).Coroutine();
                    break;
                case UserDataType.Occ:
                    break;
                case UserDataType.UnionName:
                    self.RoleInfo.UnionName = value;
                    saveValue = self.RoleInfo.UnionName;
                    self.UpdateRankInfo();
                    break;
                case UserDataType.TowerId:
                    ApplyTowerId(self.RoleInfo, value);
                    saveValue = value;
                    break;
         
                case UserDataType.Combat:
                    int newCombat = int.Parse(value);
                    int oldCombat = self.RoleInfo.Combat;
                    int oldMax = self.MaxCombat;
                    self.RoleInfo.Combat = newCombat;
                    if (newCombat > self.MaxCombat)
                    {
                        self.MaxCombat = newCombat;
                    }

                    saveValue = self.RoleInfo.Combat.ToString();
                    // 老档只有当前战力、没有历史最高：先补档，不把存量算成 122。
                    int maxDelta = oldMax <= 0 && oldCombat > 0 ? 0 : self.MaxCombat - oldMax;
                    PlayerEconomyHelper.NotifyRoleDataProgression(unit, Type, self.RoleInfo, maxDelta);
                    break;

                default:
                    if (RoleCurrencyHelper.IsItemCurrency(Type))
                    {
                        long delta = long.Parse(value);
                        long next = RoleCurrencyHelper.Get(self.RoleInfo, Type) + delta;
                        if (Type == UserDataType.TiLi || Type == UserDataType.HuoLi)
                        {
                            next = System.Math.Min(System.Math.Max(0, next), 120);
                        }
                        else if (next < 0)
                        {
                            next = 0;
                        }

                        saveValue = RoleCurrencyHelper.Set(self.RoleInfo, Type, next).ToString();
                        PlayerEconomyHelper.NotifyRoleDataProgression(unit, Type, self.RoleInfo, delta);
                    }
                    else
                    {
                        saveValue = value;
                    }
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

            if (Type == UserDataType.Name || Type == UserDataType.Level || Type == UserDataType.Combat)
            {
                self.SyncUnionMember();
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
            rankPetInfo.UnionName = self.RoleInfo.UnionName ?? string.Empty;

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
            warRankInfo.UnionName = homeRankInfo.UnionName ?? string.Empty;
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

            float expAdd = RoleLevelHelper.GetExpAdd(self.RoleInfo.Lv, self.RoleInfo.Exp, serverInfo.WorldLv);

            LDExp_Lv xiulianconf1 = LDExp_LvCategory.Instance.Get(self.RoleInfo.Lv);
            long upNeedExp = xiulianconf1.Exp_Role;

            TaskComponentServer taskComponentServer = unit.GetComponent<TaskComponentServer>();

            //等级达到上限, 不升级 但是可以继续获得经验
            int maxlevel = CommonHelper.GetPlayerMaxLevel(); //读表

            self.RoleInfo.Exp = self.RoleInfo.Exp + (int)(addValue * (1.0f + expAdd));

            if (self.RoleInfo.Lv >= maxlevel)
            {
                return;
            }

            if (upNeedExp < 0)
            {
                return;
            }

            // 到达自动等级后不再自动升级，等手动升级协议。
            if (RoleLevelHelper.NeedManualLevelUp(self.RoleInfo.Lv))
            {
                return;
            }

            if (self.RoleInfo.Exp >= upNeedExp)
            {
                self.RoleInfo.Exp -= upNeedExp;
                self.UpdateRoleData(UserDataType.Level, "1", notice);
            }
        }

        /// <summary>
        /// 给指定制造类型加经验。该类型等级不超过角色等级；经验继续累计，上限 20 亿。
        /// Exp_Skill_Make 为当前级升下一级所需经验，小于等于 0 表示不能再升级。
        /// </summary>
        public static void AddMakeExp(this RoleInfoComponentServer self, int makeType, int addExp)
        {
            if (makeType <= 0 || makeType > MakeTypeCount)
            {
                return;
            }

            RoleInfo roleInfo = self.RoleInfo;
            EnsureMakeProgress(roleInfo);
            int oldLevel = roleInfo.MakeLevelList[makeType];
            if (roleInfo.MakeLevelList[makeType] == 0)
            {
                roleInfo.MakeLevelList[makeType] = 1;
            }

            int oldExp = roleInfo.MakeExpList[makeType];
            long exp = roleInfo.MakeExpList[makeType];
            if (addExp > 0)
            {
                exp += addExp;
            }

            if (exp > MakeExpMax)
            {
                exp = MakeExpMax;
            }

            if (exp < 0)
            {
                exp = 0;
            }

            roleInfo.MakeExpList[makeType] = (int)exp;
            int roleLv = roleInfo.Lv;
            while (roleInfo.MakeLevelList[makeType] < roleLv)
            {
                if (!LDExp_LvCategory.Instance.Contain(roleInfo.MakeLevelList[makeType]))
                {
                    break;
                }

                int need = LDExp_LvCategory.Instance.Get(roleInfo.MakeLevelList[makeType]).Exp_Skill_Make;
                if (need <= 0 || roleInfo.MakeExpList[makeType] < need)
                {
                    break;
                }

                roleInfo.MakeExpList[makeType] -= need;
                roleInfo.MakeLevelList[makeType]++;
            }

            bool learned = false;
            if (roleInfo.MakeLevelList[makeType] > oldLevel)
            {
                learned = self.AutoLearnMakeByLevel();
            }

            if (!learned && roleInfo.MakeLevelList[makeType] == oldLevel && roleInfo.MakeExpList[makeType] == oldExp)
            {
                return;
            }

            Unit unit = self.GetParent<Unit>();
            if (learned)
            {
                MessageHelper.SendToClient(unit, new M2C_UpdateRoleInfoMessage { RoleInfo = roleInfo });
            }

            DBHelper.SaveComponentCache(UnitZoneHelper.GetHomeZone(unit), unit.Id, self).Coroutine();
        }

        /// <summary>
        /// 配方所属类型的制造等级达到 Skill_Make.Lv_AutoLearn，且已学会该类型时，自动加入 MakeIdList。
        /// Lv_AutoLearn 小于等于 0 不自动学。
        /// </summary>
        public static bool AutoLearnMakeByLevel(this RoleInfoComponentServer self)
        {
            RoleInfo roleInfo = self.RoleInfo;
            List<int> makeTypeList = roleInfo.MakeTypeList;
            if (makeTypeList == null || makeTypeList.Count == 0)
            {
                return false;
            }

            EnsureMakeProgress(roleInfo);
            if (roleInfo.MakeIdList == null)
            {
                roleInfo.MakeIdList = new List<int>();
            }

            bool learned = false;
            foreach (LDSkill_Make config in LDSkill_MakeCategory.Instance.GetAll().Values)
            {
                if (config.Lv_AutoLearn <= 0)
                {
                    continue;
                }

                if (!LDSkill_Make_GroupCategory.Instance.Contain(config.Group))
                {
                    continue;
                }

                int makeType = LDSkill_Make_GroupCategory.Instance.Get(config.Group).Type;
                if (makeType <= 0 || makeType > MakeTypeCount)
                {
                    continue;
                }

                int typeLevel = roleInfo.MakeLevelList[makeType];
                if (typeLevel <= 0)
                {
                    typeLevel = 1;
                }

                if (config.Lv_AutoLearn > typeLevel)
                {
                    continue;
                }

                if (!makeTypeList.Contains(makeType))
                {
                    continue;
                }

                if (roleInfo.MakeIdList.Contains(config.Id))
                {
                    continue;
                }

                roleInfo.MakeIdList.Add(config.Id);
                learned = true;
            }

            return learned;
        }

        static void EnsureMakeProgress(RoleInfo roleInfo)
        {
            if (roleInfo.MakeLevelList == null)
            {
                roleInfo.MakeLevelList = new List<int>();
            }

            if (roleInfo.MakeExpList == null)
            {
                roleInfo.MakeExpList = new List<int>();
            }

            while (roleInfo.MakeLevelList.Count <= MakeTypeCount)
            {
                roleInfo.MakeLevelList.Add(0);
            }

            while (roleInfo.MakeExpList.Count <= MakeTypeCount)
            {
                roleInfo.MakeExpList.Add(0);
            }
        }

        public static int GetSkillMakeType(LDSkill_Make cfg)
        {
            if (cfg == null || !LDSkill_Make_GroupCategory.Instance.Contain(cfg.Group))
            {
                return 0;
            }

            return LDSkill_Make_GroupCategory.Instance.Get(cfg.Group).Type;
        }

        public static void OnMakeItem(this RoleInfoComponentServer self, int makeId)
        {
            //LDMake equipMakeConfig = LDMakeCategory.Instance.Get(makeId);
            //List<IntLongPair> makeList = self.RoleInfo.MakeIdList;

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
            //    self.RoleInfo.MakeIdList.Add(new IntLongPair() { KeyId = makeId, Value = endTime });
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
            MessageHelper.SendToClient(unit, new M2C_UpdateRoleInfoMessage { RoleInfo = roleInfo });
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


        public static int GetMonsterKillNumber(this RoleInfoComponentServer self, int monsterId)
        {
            return 0;
        }

        public static long GetReviveTime(this RoleInfoComponentServer self, int monsterId)
        {
            return 0;
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

            LDHome maxHomeLv = null;
            Dictionary<int, LDHome> allHome = LDHomeCategory.Instance.GetAll();
            foreach ((int jiayualv, LDHome ldHomeConfig) in allHome)
            {
                maxHomeLv = ldHomeConfig;
            }
            self.GetParent<Unit>().GetComponent<HomeComponentServer>().HomeLv = maxHomeLv.Id;

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

    }

}

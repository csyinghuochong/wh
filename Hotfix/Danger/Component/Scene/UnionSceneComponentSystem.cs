using System;
using System.Collections.Generic;
using UnityEngine;

namespace ET
{
    [Timer(TimerType.UnionTimer)]
    public class UnionTimer : ATimer<UnionSceneComponent>
    {
        public override void Run(UnionSceneComponent self)
        {
            try
            {
                self.SaveDB();
            }
            catch (Exception e)
            {
                Log.Error($"move timer error: {self.Id}\n{e}");
            }
        }
    }

    [ObjectSystem]
    public class UnionSceneComponentAwakeSystem : AwakeSystem<UnionSceneComponent>
    {
        public override void Awake(UnionSceneComponent self)
        {
            self.OnAwake().Coroutine();
        }
    }

    [ObjectSystem]
    public class UnionSceneComponentDestroySystem : DestroySystem<UnionSceneComponent>
    {
        public override void Destroy(UnionSceneComponent self)
        {
            TimerComponent.Instance.Remove(ref self.Timer);
        }
    }

    public static class UnionSceneComponentSystem
    {

        public static async ETTask OnAwake(this UnionSceneComponent self)
        {
            await self.InitServerInfo();

            self.Timer = TimerComponent.Instance.NewRepeatedTimer(TimeHelper.Minute * 30 + RandomHelper.RandomNumber(1000, 10000), TimerType.UnionTimer, self);
        }

        public static int GetDonationRank(this UnionSceneComponent self, long usrerId)
        {
            for (int i = 0; i < self.DBUnionManager.rankingDonation.Count; i++)
            {
                if (self.DBUnionManager.rankingDonation[i].UserId == usrerId)
                {
                    return i + 1;
                }
            }
            return 0;
        }

        public static async ETTask InitServerInfo(this UnionSceneComponent self)
        {
            DBUnionManager dBServerInfo = await DBHelper.GetComponent<DBUnionManager>(self.DomainZone(), self.DomainZone());
            if (dBServerInfo == null)
            {
                dBServerInfo = new DBUnionManager();
                dBServerInfo.Id = self.DomainZone();
            }
            self.DBUnionManager = dBServerInfo;
            await self.LoadAllUnionInfos();
        }

        /// <summary>启动全量载入本区公会；列表/重复调用走同一趟，已在内存的跳过。</summary>
        public static async ETTask LoadAllUnionInfos(this UnionSceneComponent self)
        {
            if (self.AllUnionsLoaded)
            {
                return;
            }

            if (self.LoadAllUnionsTask != null)
            {
                await self.LoadAllUnionsTask;
                return;
            }

            ETTask tcs = ETTask.Create(true);
            self.LoadAllUnionsTask = tcs;
            try
            {
                List<DBUnionInfo> result = await Game.Scene.GetComponent<DBComponent>().Query<DBUnionInfo>(self.DomainZone(), d => d.UnionInfo != null);
                if (result != null)
                {
                    for (int i = 0; i < result.Count; i++)
                    {
                        self.CacheUnionInfo(result[i]);
                    }
                }

                self.AllUnionsLoaded = true;
                self.LoadAllUnionsTask = null;
                tcs.SetResult();
                Log.Info($"LoadAllUnionInfos zone:{self.DomainZone()} count:{self.DBUnionInfos.Count}");
            }
            catch (Exception e)
            {
                self.LoadAllUnionsTask = null;
                tcs.SetResult();
                Log.Error($"LoadAllUnionInfos zone:{self.DomainZone()} {e}");
            }
        }

        private static void CacheUnionInfo(this UnionSceneComponent self, DBUnionInfo unionInfo)
        {
            if (unionInfo == null)
            {
                return;
            }

            long unionId = unionInfo.Id;
            if (unionId == 0 && unionInfo.UnionInfo != null)
            {
                unionId = unionInfo.UnionInfo.UnionId;
            }
            if (unionId == 0)
            {
                return;
            }

            if (self.DBUnionInfos.TryGetValue(unionId, out DBUnionInfo cached) && cached != null)
            {
                return;
            }

            if (unionInfo.Id == 0)
            {
                unionInfo.Id = unionId;
            }

            self.DBUnionInfos[unionId] = unionInfo;
        }

        public static UnionListItem ToUnionListItem(this DBUnionInfo dBUnionInfo)
        {
            UnionInfo unionInfo = dBUnionInfo.UnionInfo;
            int unionLevel = Math.Max(unionInfo.Level, 1);
            UnionListItem unionListItem = new UnionListItem();
            unionListItem.UnionName = unionInfo.UnionName;
            unionListItem.PlayerNumber = unionInfo.UnionPlayerList?.Count ?? 0;
            unionListItem.UnionId = unionInfo.UnionId;
            // GenerateId 的高位记录自 2020-01-01 UTC 起的秒数，历史公会无需迁移即可还原创建时间。
            unionListItem.CreateTime = dBUnionInfo.CreateTime > 0
                    ? dBUnionInfo.CreateTime
                    : 1577836800000L + new IdStruct(unionInfo.UnionId).Time * 1000L;
            unionListItem.LevelLimit = unionInfo.LevelLimit;
            unionListItem.UnionLevel = unionLevel;
            unionListItem.UnionLeader = unionInfo.LeaderName;
            unionListItem.UnionBanner = unionInfo.UnionBanner;
            unionListItem.UnionPattern = unionInfo.UnionPattern;
            unionListItem.UnionPurpose = unionInfo.UnionPurpose;
            unionListItem.LeaderId = unionInfo.LeaderId;
            unionListItem.UnionNo = unionInfo.UnionNo;
            unionListItem.CombatLimit = unionInfo.CombatLimit;
            unionListItem.PlayerLimit = GetPlayerLimit(unionLevel);
            UnionPlayerInfo leader = null;
            List<UnionPlayerInfo> players = unionInfo.UnionPlayerList;
            if (players != null)
            {
                for (int i = 0; i < players.Count; i++)
                {
                    UnionPlayerInfo player = players[i];
                    if (player != null && player.UserID == unionInfo.LeaderId)
                    {
                        leader = player;
                        break;
                    }
                }
            }

            if (leader != null)
            {
                unionListItem.Occ = leader.Occ;
                unionListItem.RoleHeadId = leader.RoleHeadId;
                unionListItem.RoleHeadFrame = leader.RoleHeadFrame;
            }

            FillViceLeaders(unionListItem, unionInfo);
            return unionListItem;
        }

        /// <summary>名称包含或编号包含。keyword 空表示全部。</summary>
        public static bool MatchUnionKeyword(UnionInfo unionInfo, string keyword)
        {
            if (string.IsNullOrEmpty(keyword))
            {
                return true;
            }

            if (unionInfo.UnionName != null && unionInfo.UnionName.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }

            return unionInfo.UnionNo.ToString().IndexOf(keyword, StringComparison.Ordinal) >= 0;
        }

        public static void CollectUnionList(this UnionSceneComponent self, List<UnionListItem> unionList, string keyword)
        {
            unionList.Clear();
            string key = keyword?.Trim() ?? string.Empty;
            foreach (DBUnionInfo dBUnionInfo in self.DBUnionInfos.Values)
            {
                UnionInfo unionInfo = dBUnionInfo?.UnionInfo;
                if (unionInfo == null || unionInfo.LeaderId == 0)
                {
                    continue;
                }

                if (!MatchUnionKeyword(unionInfo, key))
                {
                    continue;
                }

                unionList.Add(dBUnionInfo.ToUnionListItem());
            }
        }

        private static int GetPlayerLimit(int unionLevel)
        {
            if (LDUnionCategory.Instance == null || !LDUnionCategory.Instance.Contain(unionLevel))
            {
                return 0;
            }

            return LDUnionCategory.Instance.Get(unionLevel).Limit_Player;
        }

        private const int ViceLeaderShowCount = 3;

        private static void FillViceLeaders(UnionListItem unionListItem, UnionInfo unionInfo)
        {
            List<UnionPlayerInfo> players = unionInfo.UnionPlayerList;
            if (players == null)
            {
                return;
            }

            int added = 0;
            for (int i = 0; i < players.Count && added < ViceLeaderShowCount; i++)
            {
                UnionPlayerInfo player = players[i];
                if (player == null || player.Position != UnionPosition.ViceLeader || player.UserID == 0)
                {
                    continue;
                }

                unionListItem.ViceLeaderList.Add(new UnionPlayerInfo()
                {
                    PlayerName = player.PlayerName,
                    PlayerLevel = player.PlayerLevel,
                    Position = player.Position,
                    UserID = player.UserID,
                    Combat = player.Combat,
                    Occ = player.Occ,
                    OccTwo = player.OccTwo,
                    RoleHeadId = player.RoleHeadId,
                    RoleHeadFrame = player.RoleHeadFrame,
                });
                added++;
            }
        }

        /// <summary>新公会编号：1001 + 当前已有工会数量。已占用则顺延。</summary>
        public static int AllocUnionNo(this UnionSceneComponent self)
        {
            int count = 0;
            foreach (DBUnionInfo info in self.DBUnionInfos.Values)
            {
                if (info?.UnionInfo == null || info.UnionInfo.LeaderId == 0)
                {
                    continue;
                }

                count++;
            }

            int unionNo = 1001 + count;
            while (self.ContainsUnionNo(unionNo))
            {
                unionNo++;
            }

            return unionNo;
        }

        private static bool ContainsUnionNo(this UnionSceneComponent self, int unionNo)
        {
            foreach (DBUnionInfo info in self.DBUnionInfos.Values)
            {
                if (info?.UnionInfo != null && info.UnionInfo.UnionNo == unionNo)
                {
                    return true;
                }
            }

            return false;
        }

        public static async ETTask<DBUnionInfo> GetDBUnionInfo(this UnionSceneComponent self, long unionId)
        {
            if (unionId == 0)
            {
                return null;
            }

            if (self.DBUnionInfos.TryGetValue(unionId, out DBUnionInfo cached) && cached != null)
            {
                return cached;
            }

            if (self.AllUnionsLoaded)
            {
                return null;
            }

            DBUnionInfo unionInfo = await DBHelper.GetComponent<DBUnionInfo>(self.DomainZone(), unionId);
            if (unionInfo == null)
            {
                return null;
            }

            self.CacheUnionInfo(unionInfo);
            if (self.DBUnionInfos.TryGetValue(unionId, out cached) && cached != null)
            {
                return cached;
            }

            return unionInfo;
        }

        /// <summary>
        /// 批准与邀请共用入会入口；在线由 Unit 实时校验，仅确认离线后才修改缓存与数据库。
        /// </summary>
        /// <param name="self"></param>
        /// <param name="unionid"></param>
        /// <param name="unitid"></param>
        /// <param name="replyCode">0拒绝  1同意</param>
        /// <returns></returns>
        public static async ETTask<int> OnJoinUinon(this UnionSceneComponent self, long unionid, long unitid, int replyCode)
        {
            if (unionid <= 0 || unitid <= 0 || unionid == unitid || (replyCode != 0 && replyCode != 1))
            {
                return LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_Invalid_Data);
            }

            DBUnionInfo dBUnionInfo = await self.GetDBUnionInfo(unionid);
            if (dBUnionInfo == null)
            {
                return LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_Operation_Fail);
            }

            if (replyCode == 0)
            {
                dBUnionInfo.UnionInfo.ApplyList.Remove(unitid);
                await DBHelper.SaveComponent(self.DomainZone(), unionid, dBUnionInfo);
                return ErrorCode.ERR_Success;
            }

            // 公会锁由批准/邀请入口持有，容量检查与新增成员在同一次操作中完成。
            LDUnion ldUnionCof = LDUnionCategory.Instance.Get(dBUnionInfo.UnionInfo.Level);
            if (dBUnionInfo.UnionInfo.UnionPlayerList.Count >= ldUnionCof.Limit_Player)
            {
                return LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_No_Volume);
            }

            bool exist = false;
            for (int i = 0; i < dBUnionInfo.UnionInfo.UnionPlayerList.Count; i++)
            {
                if (dBUnionInfo.UnionInfo.UnionPlayerList[i].UserID == unitid)
                {
                    exist = true;
                }
            }
            if (!exist)
            {
                // 在线角色先检查当前公会并存档，拒绝入会不能转为离线写入。
                U2M_UnionApplyRequest r2M_RechargeRequest = new U2M_UnionApplyRequest() { UnionId = unionid, UnionName = dBUnionInfo.UnionInfo.UnionName };
                M2U_UnionApplyResponse m2G_RechargeResponse = (M2U_UnionApplyResponse)await ActorLocationSenderComponent.Instance.Call(unitid, r2M_RechargeRequest);
                RoleInfoComponentServer joinRoleInfo = null;
                if (m2G_RechargeResponse.Error != ErrorCode.ERR_Success
                    && m2G_RechargeResponse.Error != ErrorCore.ERR_NotFoundActor)
                {
                    if (m2G_RechargeResponse.Error == LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Union_Already_Joined))
                    {
                        dBUnionInfo.UnionInfo.ApplyList.Remove(unitid);
                        await DBHelper.SaveComponent(self.DomainZone(), unionid, dBUnionInfo);
                    }
                    return m2G_RechargeResponse.Error;
                }

                if (m2G_RechargeResponse.Error == ErrorCore.ERR_NotFoundActor)
                {
                    // 不同公会可能同时批准同一离线玩家；用玩家 ID 串行校验和写入。
                    // 玩家 ID 与公会 ID 是不同实体的全局 ID，可复用 UnionOperate 锁域。
                    using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.UnionOperate, unitid))
                    {
                        NumericComponent numericComponent = await DBHelper.GetPlayerComponent<NumericComponent>(unitid);
                        joinRoleInfo = await DBHelper.GetPlayerComponent<RoleInfoComponentServer>(unitid);
                        if (numericComponent == null || joinRoleInfo == null)
                        {
                            return LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Common_Invalid_Data);
                        }
                        if (numericComponent.GetAsLong(NumericType.UnionId_0) != 0)
                        {
                            dBUnionInfo.UnionInfo.ApplyList.Remove(unitid);
                            await DBHelper.SaveComponent(self.DomainZone(), unionid, dBUnionInfo);
                            return LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Union_Already_Joined);
                        }

                        numericComponent.ApplyValue(NumericType.UnionId_0, unionid, false);
                        numericComponent.ApplyValue(NumericType.UnionLeader, 0, false);
                        await DBHelper.SavePlayerComponent(unitid, numericComponent);
                        joinRoleInfo.SetUnionName(dBUnionInfo.UnionInfo.UnionName, false);
                        await DBHelper.SavePlayerComponent(unitid, joinRoleInfo);
                    }
                }

                if (joinRoleInfo == null)
                {
                    joinRoleInfo = await DBHelper.GetPlayerComponent<RoleInfoComponentServer>(unitid);
                }
                dBUnionInfo.UnionInfo.UnionPlayerList.Add(UnionHelper.CreateUnionPlayerSnapshot(joinRoleInfo?.RoleInfo, unitid, TimeHelper.ServerNow()));
            }

            dBUnionInfo.UnionInfo.ApplyList.Remove(unitid);
            await DBHelper.SaveComponent(self.DomainZone(), unionid, dBUnionInfo);
            return ErrorCode.ERR_Success;
        }

        public static void OnDailyReset(this UnionSceneComponent self)
        {
            Console.WriteLine($"RankSceneComponent OnDailyReset");
            self.DBUnionManager.rankingDonation.Clear();

        }

        public static async ETTask CheckWinUnion(this UnionSceneComponent self, Scene fubnescene, int minite)
        {
            Vector3 initPosi = new Vector3(-68.62f, 0f, -3.05f);
            Dictionary<long, int> map = new Dictionary<long, int>();


            List<Unit> units = UnitHelper.GetAliveUnitList(fubnescene, UnitType.Player);
            for (int i = 0; i < units.Count; i++)
            {
                if (Vector3.Distance(initPosi, units[i].Position) > 20)
                {
                    continue;
                }
                long unionId = units[i].GetUnionId();

                if (!map.TryGetValue(unionId, out int unionCount))
                {
                    unionCount = 0;
                }
                map[unionId] = unionCount + 1;
            }

            long winunionid = 0;
            int playernumber = 0;
            string unionplayerNumber = $"{self.DomainZone()} 占领区人数: ";
            foreach ((long unioid, int number) in map)
            {
                if (number > playernumber)
                {
                    winunionid = unioid;
                    playernumber = number;
                }

                unionplayerNumber = unionplayerNumber +  $"{unioid}:{playernumber}  ";
            }

          
            self.WinUnionId = winunionid;

            if (minite == 0)
            {
                Log.Warning(unionplayerNumber);
                Log.Warning($"胜利家族:  {self.DomainZone()} {winunionid}");
            }

            DBUnionInfo dBUnionInfo = await self.GetDBUnionInfo(winunionid);
            if (dBUnionInfo != null)
            {
                string messageContent = $"恭喜 <color=#{CommonHelper.QualityReturnColor(2)}>{dBUnionInfo.UnionInfo.UnionName}</color>家族占领了家族争霸赛地图!";
                string messageContentEn = $"Congratulations <color=#{CommonHelper.QualityReturnColor(2)}>{dBUnionInfo.UnionInfo.UnionName}</color>Family Take over Family Fight Map!";
                //ServerMessageHelper.SendBroadMessage(self.DomainZone(), NoticeType.Notice, messageContent, messageContentEn);
            }
        }

        public static void OnJoinUnionRace(this UnionSceneComponent self, long unionid, long unitid)
        {
            List<long> unitids = null;
            self.UnionRaceUnits.TryGetValue(unionid, out unitids);
            if (unitids == null)
            {
                self.UnionRaceUnits.Add(unionid, new List<long>());
            }

            if (!self.UnionRaceUnits[unionid].Contains(unitid))
            {
                self.UnionRaceUnits[unionid].Add(unitid);
            }
        }

        public static async ETTask OnCheckWinUnion(this UnionSceneComponent self)
        {
            int minite = (int)((FunctionHelp.GetCloseTime(1044) - FunctionHelp.GetOpenTime(1044)) / 60);
            /////进程9
            //Log.Console($"家族争霸赛开始！！:{self.DomainZone()}  {minite}");
            Log.Debug($"家族争霸赛开始！！:{self.DomainZone()}  {minite}");
            for (int i = minite - 1; i >= 0; i--)
            {
                await TimerComponent.Instance.WaitAsync(60 * 1000);
                Log.Debug($"家族争霸赛检测！！: {self.DomainZone()}  {i}");

                Scene fubnescene = self.GetChild<Scene>(self.UnionRaceSceneId);
                if (fubnescene == null)
                {
                    break;
                }
                self.CheckWinUnion(fubnescene, i).Coroutine();
            }

            self.OnUnionRaceOver().Coroutine();
        }

        public static async ETTask OnUnionRaceOver(this UnionSceneComponent self)
        {
            long serverTime = TimeHelper.ServerNow();
           
            Log.Console($"家族争霸赛结束！！:{self.DomainZone()}");
            Log.Warning($"家族争霸赛结束！！: {self.DomainZone()}");
            int allwinunits = 0;
            int allfailunits = 0;
            foreach ((long unionid, List<long> unitids) in self.UnionRaceUnits)
            {
                for (int i = 0; i < unitids.Count; i++)
                {
                    if (unionid == self.WinUnionId)
                    {
                        allwinunits++;
                    }
                    else
                    {
                        allfailunits++;
                    }    
                }
            }

            long allJiangjin =(long) (0.8f * (self.DBUnionManager.TotalDonation + self.GetBaseJiangJin()));
            allwinunits = Math.Max(allwinunits, 10);
            allfailunits = Math.Max(allfailunits, 10);

            int winJingJin = (int)(allJiangjin * 0.6f / allwinunits);
            int failJiangJin = (int)(allJiangjin * 0.4f / allfailunits);

            Log.Console("家族战发放奖励");
            Log.Warning($"allwinunits: {allwinunits}   allfailunits: {allfailunits}  winJingJin: {winJingJin} failJiangJin:{failJiangJin} winunionid: {self.WinUnionId} allJiangjin:{allJiangjin}");

            
            long mailServerId = DBHelper.GetMailServerId(self.DomainZone());
            foreach (( long unionid, List<long> unitids ) in self.UnionRaceUnits )
            {
                for (int i = 0; i < unitids.Count; i++)
                {
                    MailInfo mailInfo = new MailInfo();
                    mailInfo.Status = 0;
                    //mailInfo.Title = "家族争霸赛奖励";
                    //mailInfo.MailId = IdGenerater.Instance.GenerateId();
                  
                    //if (unionid == self.WinUnionId)
                    //{
                    //    mailInfo.Context = "发送家族争霸赛胜利奖励";
                    //    Log.Warning($"发送奖励胜利！！: {self.DomainZone()} {unitids[i]}");
                    //    mailInfo.ItemList.Add(new BagInfo() { ItemID = 1, ItemNum = winJingJin,  GetWay = $"{ItemGetWay.UnionRace}_{serverTime}" });
                    //}
                    //else
                    //{
                    //    mailInfo.Context = "发送家族争霸赛失败奖励";
                    //    Log.Warning($"发送奖励失败！！: {self.DomainZone()} {unitids[i]}");
                    //    mailInfo.ItemList.Add(new BagInfo() { ItemID = 1, ItemNum = failJiangJin, GetWay = $"{ItemGetWay.UnionRace}_{serverTime}" });
                    //}

                    //MailHelp.SendUserMail(UnitZoneHelper.GetHomeZone(unitids[i]), unitids[i], mailInfo).Coroutine();
              
                }
            }

            await TimerComponent.Instance.WaitAsync(1000);

            Scene fubnescene = self.GetChild<Scene>(self.UnionRaceSceneId);
            if (fubnescene != null)
            {
                List<Unit> units = UnitHelper.GetUnitList(fubnescene, UnitType.Player);
                M2C_UnionRaceInfoResult m2C_Battle = new M2C_UnionRaceInfoResult();
                m2C_Battle.SceneType = MapTypeEnum.UnionRace;
                for (int i = 0; i < units.Count; i++)
                {
                    MessageHelper.SendToClient(units[i], m2C_Battle);
                }
            }

            //通知家族争霸赛地图开始踢人
            await TimerComponent.Instance.WaitAsync(TimeHelper.Minute);
            fubnescene = self.GetChild<Scene>(self.UnionRaceSceneId);
            if (fubnescene != null)
            {
                TransferArgs args = new TransferArgs(sceneType: MapTypeEnum.MainCityScene, sceneId: 0);
                List<Unit> units = UnitHelper.GetUnitList(fubnescene, UnitType.Player);
                for (int i = 0; i < units.Count; i++)
                {
                    TransferHelper.TransferUnit(units[i], args).Coroutine();
                }

                await TimerComponent.Instance.WaitAsync(RandomHelper.RandomNumber(1000, 2000));
                TransferHelper.NoticeFubenCenter(fubnescene, 2).Coroutine();
                fubnescene.Dispose();
            }
          
            self.DBUnionManager.SignupUnions.Clear();
            self.DBUnionManager.LastWeakDonation = self.DBUnionManager.TotalDonation;
            self.DBUnionManager.TotalDonation = (long)((self.GetBaseJiangJin() + self.DBUnionManager.TotalDonation )  * 0.2f);
            self.DBUnionManager.WinUnionId = self.WinUnionId;
            self.DBUnionManager.UnionRaceTime ++;
            self.UnionRaceUnits.Clear();
            self.SaveDB();
        }

        public static long GetBaseJiangJin(this UnionSceneComponent self)
        {
            if (self.DBUnionManager.UnionRaceTime == 0)
            {
                return 10000000;
            }
            if (self.DBUnionManager.UnionRaceTime == 1)
            {
                return 5000000;
            }
            return 3000000;
        }

        public static async ETTask OnUnionRaceBegin(this UnionSceneComponent self)
        {
            self.GenerateUnionRace();
            self.OnCheckWinUnion().Coroutine();
            await TimerComponent.Instance.WaitAsync(RandomHelper.RandomNumber(0, 1000));

            ///////
            //long chatServerId = DBHelper.GetChatServerId(self.DomainZone());
            //A2A_ServerMessageRResponse g_SendChatRequest = (A2A_ServerMessageRResponse)await ActorMessageSenderComponent.Instance.Call
            //    (chatServerId, new A2A_ServerMessageRequest()
            //    {
            //        MessageType = NoticeType.UnionRace,
            //    });
            List<UnionPlayerInfo> playerlist = new List<UnionPlayerInfo>();
            for (int i = 0; i < self.DBUnionManager.SignupUnions.Count; i++)
            {
                DBUnionInfo dBUnionInfo = await self.GetDBUnionInfo(self.DBUnionManager.SignupUnions[i]);
                if (dBUnionInfo == null)
                {
                    continue;
                }
                playerlist.AddRange(dBUnionInfo.UnionInfo.UnionPlayerList);
            }
            int zone = self.DomainZone();
        }

        /// <summary>
        /// 家族boss击杀
        /// </summary>
        /// <param name="self"></param>
        /// <param name="scene"></param>
        /// <param name="defend"></param>
        public static void OnKillEvent(this UnionSceneComponent self, Scene scene,Unit defend)
        {
            
            Log.Error("UnionSceneComponent.OnKillEvent:  {defend.ConfigId}");
            /*
            LDScene ldScene = LDSceneCategory.Instance.Get(2000009);
            if (defend.Type != UnitType.Monster || defend.ConfigId != ldScene.BossId)
            {
                return;
            }
            */

            long serverTime = TimeHelper.ServerNow();
            List<Unit> players = UnitHelper.GetUnitList(scene, UnitType.Player);
        }

        public static void GenerateUnionRace(this UnionSceneComponent self)
        {
            long fubenid = IdGenerater.Instance.GenerateId();
            long fubenInstanceId = IdGenerater.Instance.GenerateInstanceId();

            LDScene ldScene = LDSceneCategory.Instance.Get(2000008);
            Scene fubnescene = SceneFactory.Create(self, fubenid, fubenInstanceId, self.DomainZone(), "UnionRace" + ldScene.Id.ToString(), SceneType.Map);
            MapComponent mapComponent = fubnescene.GetComponent<MapComponent>();
            mapComponent.SetMapInfo(ldScene.Scene_Type, ldScene.Id, 0);
            mapComponent.NavMeshId = ldScene.GetNavMeshId();
            Game.Scene.GetComponent<RecastPathComponent>().Update(mapComponent.NavMeshId);
            fubnescene.AddComponent<YeWaiRefreshComponent>().SceneId = 2000008;
           
            //FubenHelp.CreateMonsterList(fubnescene, ldScene.CreateMonsterPosi);
            
            TransferHelper.NoticeFubenCenter(fubnescene, 1).Coroutine();
            self.UnionRaceSceneId = fubenid;
            self.UnionRaceSceneInstanceId = fubenInstanceId;    
        }

        public static  void SaveDB(this UnionSceneComponent self)
        {
            DBHelper.SaveComponent(self.DomainZone(), self.DomainZone(), self.DBUnionManager).Coroutine();
        }
    }
}

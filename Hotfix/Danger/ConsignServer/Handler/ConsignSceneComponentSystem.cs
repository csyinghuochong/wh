using System;
using System.Collections.Generic;

namespace ET
{

    [Timer(TimerType.ConsignSceneTimer)]
    public class PaiMaiTimer : ATimer<ConsignSceneComponent>
    {
        public override void Run(ConsignSceneComponent self)
        {
            try
            {
                self.SaveDB(1).Coroutine();
            }
            catch (Exception e)
            {
                Log.Error($"move timer error: {self.Id}\n{e}");
            }
        }
    }

    [Timer(TimerType.AuctionOverTimer)]
    public class AuctionOverTimer : ATimer<ConsignSceneComponent>
    {
        public override void Run(ConsignSceneComponent self)
        {
            try
            {
                self.OnAuctionOver().Coroutine();
            }
            catch (Exception e)
            {
                Log.Error($"move timer error: {self.Id}\n{e}");
            }
        }
    }


    public class ConsignSceneComponentAwake : AwakeSystem<ConsignSceneComponent>
    {
        public override void Awake(ConsignSceneComponent self)
        {
            self.InitDBData().Coroutine();
        }
    }

    [ObjectSystem]
    public class ConsignSceneComponentDestroy : DestroySystem<ConsignSceneComponent>
    {

        public override void Destroy(ConsignSceneComponent self)
        {
            TimerComponent.Instance.Remove(ref self.Timer);
            TimerComponent.Instance.Remove(ref self.AuctionOverTimer);
        }
    }


    public static class ConsignSceneComponentSystem
    {

        public static  void OnAuctionBegin(this ConsignSceneComponent self, long overlefttime)
        {
            self.AuctionRecords.Clear();
            //初始化拍卖价格
            self.AuctionPrice = 1000000;
            self.AuctionStart = self.AuctionPrice;
            self.AuctioUnitId = 0;
            self.AuctionPlayer = String.Empty;

            int openDay = DBHelper.GetOpenServerDay(self.DomainZone());
            int[] dayAuctionItems = { 0, 14060005, 15207003, 15306003, 15302007, 15406003, 15407003, 15506003 };
            if (openDay >= 1 && openDay <= 7)
            {
                self.AuctionItem = dayAuctionItems[openDay];
                self.AuctionItemNum = 1;
            }
            else if (openDay > 7)
            {
                int[] weights = new int[] { 10, 10, 10, 10, 10, 10, 10, 20, 5 };
                string[] weightsItem = new string[] { "10000143,10", "10000141,1", "10000152,3", "10000150,1", "10000165,1", "10010053,1", "10010040,1", "10045108,1", "10010094,1" };
                int id = RandomHelper.RandomByWeight(weights);

                string[] weightItemParts = weightsItem[id].Split(',');
                self.AuctionItem = int.Parse(weightItemParts[0]);
                self.AuctionItemNum = int.Parse(weightItemParts[1]);
            }

            //拍卖会开始
            ServerMessageHelper.SendServerMessage(DBHelper.GetChatServerId(self.DomainZone()), NoticeType.PaiMaiAuction,
            $"{self.AuctionItem}_{self.AuctionItemNum}_{self.AuctionPrice}_{self.AuctionPlayer}_1").Coroutine();

            Log.Warning($"拍卖会开始:  {self.DomainZone()}  {self.AuctioUnitId} {self.AuctionPlayer}");
            Log.Warning($"拍卖会开始结束时间:  {overlefttime}");
            self.AuctionStatus = TimeHelper.ServerNow() + overlefttime;
            TimerComponent.Instance.Remove(ref self.AuctionOverTimer);
            self.AuctionOverTimer = TimerComponent.Instance.NewOnceTimer(self.AuctionStatus, TimerType.AuctionOverTimer, self );
        }

        public static void ExtendOverTime(this ConsignSceneComponent self)
        {
            if (self.AuctionOverTimer <= 0)
            {
                return;
            }

            DateTime dateTime = TimeInfo.Instance.ToDateTime( TimeHelper.ServerNow() );
            int curTime = dateTime.Hour * 60 + dateTime.Minute;
            int maxTime = 23 * 60 + 58;
            long serverNow = TimeHelper.ServerNow();

            if (curTime <= maxTime &&  self.AuctionStatus - serverNow < TimeHelper.Minute)
            {
                //Console.WriteLine($"有人加价 延迟时间！   {self.DomainZone()}");
                self.AuctionStatus = serverNow + TimeHelper.Minute;
                TimerComponent.Instance.Remove(ref self.AuctionOverTimer);
                self.AuctionOverTimer = TimerComponent.Instance.NewOnceTimer(self.AuctionStatus, TimerType.AuctionOverTimer, self);
            }
            else
            { 
                //Console.WriteLine($"有人加价！   {self.DomainZone()}");
            }
        }

        public static async ETTask OnAuctionOver(this ConsignSceneComponent self)
        {
            long serverNow = TimeHelper.ServerNow();
            Log.Debug($"拍卖结束: {self.DomainZone()} {TimeInfo.Instance.ToDateTime(serverNow)}");

            if (self.AuctioUnitId != 0)
            {
                int auctionHomeZone = UnitZoneHelper.GetHomeZone(self.AuctioUnitId);
                string auctionGetWay = $"{ItemGetWay.Auction}_{serverNow}";

                // 先按在线 Unit 扣款；找不到人 / 扣款失败再走库结算（与原离线分支合并）
                Consign2M_AuctionOverRequest p2M_PaiMaiAuctionOverRequest = new Consign2M_AuctionOverRequest()
                {
                    Price = self.AuctionPrice,
                    ItemID = self.AuctionItem,
                    ItemNumber = self.AuctionItemNum,
                };
                M2Consign_AuctionOverResponse m2G_RechargeResponse = (M2Consign_AuctionOverResponse)await ActorLocationSenderComponent.Instance.Call(
                    self.AuctioUnitId, p2M_PaiMaiAuctionOverRequest);

                if (m2G_RechargeResponse.Error == ErrorCode.ERR_Success)
                {
                    Log.Error("MailInfo mailInfo = new MailInfo");
                    Log.Warning($"OnAuctionOver[在线]:  {self.DomainZone()}  {self.AuctioUnitId}  {self.AuctionPlayer}");
                    //MailInfo mailInfo = new MailInfo();
                    //mailInfo.Status = 0;
                    //mailInfo.Context = "竞拍道具";
                    //mailInfo.Title = "竞拍道具";
                    //mailInfo.MailId = IdGenerater.Instance.GenerateId();
                    //mailInfo.ItemList.Add(new BagInfo() { ItemID = self.AuctionItem, ItemNum = self.AuctionItemNum, GetWay = auctionGetWay });
                    //await MailHelp.SendUserMail(auctionHomeZone, self.AuctioUnitId, mailInfo);
                }
                else
                {
                    Log.Warning($"OnAuctionOver[离线/失败]:  {self.DomainZone()}  {self.AuctioUnitId}  {self.AuctionPlayer}  Error={m2G_RechargeResponse.Error}");
                    RoleInfoComponentServer roleInfoComponentServer = await DBHelper.GetComponentCache<RoleInfoComponentServer>(auctionHomeZone, self.AuctioUnitId);
                    if (roleInfoComponentServer != null && roleInfoComponentServer.RoleInfo.Gold >= self.AuctionPrice)
                    {
                        roleInfoComponentServer.RoleInfo.Gold -= self.AuctionPrice;
                        DBHelper.SaveComponentCache(auctionHomeZone, self.AuctioUnitId, roleInfoComponentServer).Coroutine();
                        Log.Error("MailInfo mailInfo = new MailInfo");
                        //MailInfo mailInfo = new MailInfo();
                        //mailInfo.Status = 0;
                        //mailInfo.Context = "竞拍道具";
                        //mailInfo.Title = "竞拍道具";
                        //mailInfo.MailId = IdGenerater.Instance.GenerateId();
                        //mailInfo.ItemList.Add(new BagInfo() { ItemID = self.AuctionItem, ItemNum = self.AuctionItemNum, GetWay = auctionGetWay });
                        //await MailHelp.SendUserMail(auctionHomeZone, self.AuctioUnitId, mailInfo);
                    }
                    else
                    {
                        // 流拍则不退还保证金
                        if (self.AuctionJoinList.Contains(self.AuctioUnitId))
                        {
                            self.AuctionJoinList.Remove(self.AuctioUnitId);
                        }

                        Log.Error("MailInfo mailInfo = new MailInfo");
                        MailInfo mailInfo = new MailInfo();
                        mailInfo.Status = 0;
                        //mailInfo.Context = "竞拍失败";
                        //mailInfo.Title = $"金币小于{self.AuctionPrice},竞拍失败";
                        mailInfo.MailId = IdGenerater.Instance.GenerateId();
                        await MailHelp.SendUserMail(auctionHomeZone, self.AuctioUnitId, mailInfo);
                    }
                }
            }

            //退还保证金
            int returnggold = (int)( self.AuctionStart * 0.1f);
            for (int i = 0; i < self.AuctionJoinList.Count; i++)
            {
                Log.Error("MailInfo mailInfo = new MailInfo");
                long joinPlayerId = self.AuctionJoinList[i];
                MailInfo mailInfo = new MailInfo();
                mailInfo.Status = 0;
                //mailInfo.Context = "退还保证金";
                //mailInfo.Title = "退还保证金";
                mailInfo.MailId = IdGenerater.Instance.GenerateId();
                mailInfo.ItemList.Add(new BagInfo() { ItemID = 1, ItemNum = returnggold, GetWay = $"{ItemGetWay.Auction}_{serverNow}" });

                await MailHelp.SendUserMail(UnitZoneHelper.GetHomeZone(joinPlayerId), joinPlayerId, mailInfo);
            }

            //其他玩家退还保证金
            self.AuctionJoinList.Clear();
            self.AuctionStatus = -1;

            //拍卖会结束
            ServerMessageHelper.SendServerMessage(DBHelper.GetChatServerId(self.DomainZone()), NoticeType.PaiMaiAuction,
            $"{self.AuctionItem}_{self.AuctionItemNum}_{self.AuctionPrice}_{self.AuctionPlayer}_2").Coroutine();

            Log.Warning($"拍卖会结束:  {self.DomainZone()} {self.AuctionPlayer}  {self.AuctionPrice} {self.AuctionItem}:{self.AuctionItemNum}");
        }

        public static async ETTask BeginAuctionTimer(this ConsignSceneComponent self)
        {
            self.AuctionStatus = 0;
            self.AuctionRecords.Clear();
            DateTime dateTime = TimeHelper.DateTimeNow();
            long curTime = (dateTime.Hour * 60 + dateTime.Minute) * 60 + dateTime.Second;
            long openTime = FunctionHelp.GetOpenTime(1040);
            long closeTime = FunctionHelp.GetCloseTime(1040);

            if (curTime < openTime)
            {
                await TimerComponent.Instance.WaitAsync((openTime - curTime) * TimeHelper.Second);
                dateTime = TimeHelper.DateTimeNow();
                curTime = (dateTime.Hour * 60 + dateTime.Minute) * 60 + dateTime.Second;
                self.OnAuctionBegin((closeTime - curTime) * 1000);
            }
            else if (curTime >= openTime && curTime <= closeTime)
            {
                self.OnAuctionBegin((closeTime - curTime) * 1000);
            }
            else
            {

            }
        }


        /// <summary>
        /// 拍卖商店
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static async ETTask InitPaiMainShop(this ConsignSceneComponent self, int itemType, List<ConsignShopItemInfo> oldPaiMaiShop)
        {
            int zone = self.DomainZone();
            long unitid = ConsignHelper.GetPaiMaiId(itemType);
            long dbCacheId = DBHelper.GetDbCacheId(zone);
            
            List<DBConsignInfo> paimaiList = await Game.Scene.GetComponent<DBComponent>().Query<DBConsignInfo>(self.DomainZone(), d => d.Id == unitid);
            if (zone == 66)
            {
                Log.Console("zone == 66");
            }
            if (paimaiList == null || paimaiList.Count == 0)
            {
                //初始拍卖行商店
                DBConsignInfo dBPaiMainInfo = new DBConsignInfo();
                dBPaiMainInfo.Id = unitid;
                self.dBPaiMainInfo_Shop = dBPaiMainInfo;
                //存储拍卖行商店
                //D2M_SaveComponent d2GSave = (D2M_SaveComponent)await ActorMessageSenderComponent.Instance.Call(dbCacheId, new M2D_SaveComponent() { UnitId = unitid, EntityByte = MongoHelper.ToBson(dBPaiMainInfo), ComponentType = DBHelper.DBPaiMainInfo });
                await Game.Scene.GetComponent<DBComponent>().Save<DBConsignInfo>(self.DomainZone(), dBPaiMainInfo);
            }
            else
            {
                self.dBPaiMainInfo_Shop = paimaiList[0];
            }

            //更新快捷购买列表
            self.UpdatePaiMaiShopItemList();
        }

        public static async ETTask InitPaiMainStall(this ConsignSceneComponent self, int itemType, List<ConsignItemInfo> oldPaiMaiStall)
        {
            int zone = self.DomainZone();
            long unitid = ConsignHelper.GetPaiMaiId(itemType);
            long dbCacheId = DBHelper.GetDbCacheId(zone);

            List<DBConsignInfo> paimaiList = await Game.Scene.GetComponent<DBComponent>().Query<DBConsignInfo>(self.DomainZone(), d => d.Id == unitid);
            if (zone == 66)
            {
                Log.Console("zone == 66");
            }

            if (paimaiList == null || paimaiList.Count == 0)
            {
                //初始摆摊数据
                DBConsignInfo dBPaiMainInfo = new DBConsignInfo();
                dBPaiMainInfo.Id = unitid;
                self.dBPaiMainInfo_Stall = dBPaiMainInfo;
                self.dBPaiMainInfo_Stall.PaiMaiItemInfos = oldPaiMaiStall;
                //存储摆摊数据
                //D2M_SaveComponent d2GSave = (D2M_SaveComponent)await ActorMessageSenderComponent.Instance.Call(dbCacheId, new M2D_SaveComponent() { UnitId = unitid, EntityByte = MongoHelper.ToBson(dBPaiMainInfo), ComponentType = DBHelper.DBPaiMainInfo });
                await Game.Scene.GetComponent<DBComponent>().Save<DBConsignInfo>(self.DomainZone(), dBPaiMainInfo);
            }
            else
            {
                self.dBPaiMainInfo_Stall = paimaiList[0];
            }
        }

        public static List<ConsignItemInfo> GetItemListByUser(this ConsignSceneComponent self, long useriD, List<ConsignItemInfo> oldPaiMaiAl)
        {
            List<ConsignItemInfo> paiMaiType = new List<ConsignItemInfo>();

            for (int i = 0; i < oldPaiMaiAl.Count; i++)
            {
                ConsignItemInfo item = oldPaiMaiAl[i];
                if (useriD != 9 && item.UserId == useriD)
                {
                    paiMaiType.Add(item);
                }
            }

            return paiMaiType;
        }

        public static List<ConsignItemInfo> GetItemListByType(this ConsignSceneComponent self, int itemType, List<ConsignItemInfo> oldPaiMaiAl)
        {
            List<ConsignItemInfo> paiMaiType = new List<ConsignItemInfo>();

            for (int i = 0;  i < oldPaiMaiAl.Count; i++)
            {
                ConsignItemInfo item = oldPaiMaiAl[i];
                LDItem ldItem = LDItemCategory.Instance.Get(item.BagInfo.ItemID);
                if (ldItem.ItemType == itemType)
                {
                    paiMaiType.Add(item);
                }
            }

            return paiMaiType;
        }

        public static void UpdatePaiMaiDBByType(this ConsignSceneComponent self, int itemType, DBConsignInfo dBPaiMainInfo_Type)
        {
            switch (itemType)
            {
                case 1:
                    self.dBPaiMainInfo_Consume = dBPaiMainInfo_Type;
                    break;
                case 2:
                    self.dBPaiMainInfo_Material = dBPaiMainInfo_Type;
                    break;
                case 3:
                    self.dBPaiMainInfo_Equipment = dBPaiMainInfo_Type;
                    break;
                case 4:
                    self.dBPaiMainInfo_Gemstone = dBPaiMainInfo_Type;
                    break;
                default:
                    Log.Error($"InitPaiMainShangJia: {itemType}");
                    break;
            }

        }

        public static DBConsignInfo GetPaiMaiDBByType(this ConsignSceneComponent self, int itemType)
        {
            DBConsignInfo dBPaiMainInfo_Type = null;
            switch (itemType)
            {
                case 1:
                    dBPaiMainInfo_Type = self.dBPaiMainInfo_Consume;
                    break;
                case 2:
                    dBPaiMainInfo_Type = self.dBPaiMainInfo_Material;
                    break;
                case 3:
                    dBPaiMainInfo_Type = self.dBPaiMainInfo_Equipment;
                    break;
                case 4:
                    dBPaiMainInfo_Type = self.dBPaiMainInfo_Gemstone;
                    break;
                default:
                    Log.Warning($"InitPaiMainShangJia: {itemType}");
                    break;
            }

            return dBPaiMainInfo_Type;  
        }

        public static async ETTask InitPaiMaiShangJia(this ConsignSceneComponent self, int itemType, List<ConsignItemInfo> oldPaiMaiAll)
        {
            int zone = self.DomainZone();
            long unitid = ConsignHelper.GetPaiMaiId(itemType);
            long dbCacheId = DBHelper.GetDbCacheId(zone);

            List<DBConsignInfo> paimaiList = await Game.Scene.GetComponent<DBComponent>().Query<DBConsignInfo>(self.DomainZone(), d => d.Id == unitid);
            if (zone == 66)
            {
                Log.Console("zone == 66");
            }

            if (paimaiList == null || paimaiList.Count == 0)
            {
                //初始摆摊数据
                DBConsignInfo dBPaiMainInfo = new DBConsignInfo();
                dBPaiMainInfo.Id = unitid;
                dBPaiMainInfo.PaiMaiItemInfos = self.GetItemListByType(itemType, oldPaiMaiAll);
                self.UpdatePaiMaiDBByType(itemType, dBPaiMainInfo);
                //存储摆摊数据
                //D2M_SaveComponent d2GSave = (D2M_SaveComponent)await ActorMessageSenderComponent.Instance.Call(dbCacheId, new M2D_SaveComponent() { UnitId = unitid, EntityByte = MongoHelper.ToBson(dBPaiMainInfo), ComponentType = DBHelper.DBPaiMainInfo });
                await Game.Scene.GetComponent<DBComponent>().Save<DBConsignInfo>(self.DomainZone(), dBPaiMainInfo);
            }
            else
            {
                self.UpdatePaiMaiDBByType(itemType, paimaiList[0]);
            }
        }

        public static async ETTask InitDBData(this ConsignSceneComponent self)
        {
            int zone = self.DomainZone();
            long dbCacheId = DBHelper.GetDbCacheId(zone);
            await TimerComponent.Instance.WaitAsync(RandomHelper.RandomNumber(5000, 10000));

            List<ConsignShopItemInfo> oldPaiMaiShop = new List<ConsignShopItemInfo>();
            List<ConsignItemInfo> oldPaiMaiAll = new List<ConsignItemInfo>();
            List<ConsignItemInfo> oldPaiMaiStall = new List<ConsignItemInfo>();

            List<DBConsignInfo> paimaiList = await Game.Scene.GetComponent<DBComponent>().Query<DBConsignInfo>(self.DomainZone(), d => d.Id == zone);


            await self.InitPaiMaiShangJia(1, oldPaiMaiAll);
            await self.InitPaiMaiShangJia(2, oldPaiMaiAll);
            await self.InitPaiMaiShangJia(3, oldPaiMaiAll);
            await self.InitPaiMaiShangJia(4, oldPaiMaiAll);

            await self.InitPaiMainShop(11, oldPaiMaiShop);
            await self.InitPaiMainStall(12, oldPaiMaiStall);

            self.Timer = TimerComponent.Instance.NewRepeatedTimer(TimeHelper.Minute * 30 + RandomHelper.RandomNumber(1000, 10000), TimerType.ConsignSceneTimer, self);
            self.OnZeroClockUpdate();
        }

        //更新快捷购买列表
        public static void UpdatePaiMaiShopItemList(this ConsignSceneComponent self)
        {
            //self.dBPaiMainInfo_Shop.PaiMaiShopItemInfos = PaiMaiHelper.Instance.InitPaiMaiShopItemList(self.dBPaiMainInfo_Shop.PaiMaiShopItemInfos);
        }

        //零点刷新
        public static void OnZeroClockUpdate(this ConsignSceneComponent self)
        {
            //更新价格
            self.UpdatePaiMaiShopItemPrice();

            self.UpdateShangJiaItems();

            self.BeginAuctionTimer().Coroutine();
        }

        //每天更新道具物品价格
        public static void UpdatePaiMaiShopItemPrice(this ConsignSceneComponent self)
        {
            int curzone = ServerHelper.GetOldServerId(self.DomainZone());
            int openserverDay = DBHelper.GetOpenServerDay(curzone);
            Log.Info($"curzone = {curzone} openserverDay = {openserverDay} PaiMaiScene开服天数 {self.DomainZone()} {openserverDay}");
            if (openserverDay == 0)
            {
                return;
            }
            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="self"></param>
        /// <param name="deleteType">0删角 1回档</param>
        /// <param name="userId"></param>
        public static void OnDeleteRole(this ConsignSceneComponent self, int deleteType, long userId)
        {
            if (userId <= 0)
            {
                return;
            }
            //Console.WriteLine($"OnDeleteRole.PaiMai :  {self.DomainZone()} {userId}");
            self.OnDeleteRole_ByType(userId, self.dBPaiMainInfo_Consume);
            self.OnDeleteRole_ByType(userId, self.dBPaiMainInfo_Material);
            self.OnDeleteRole_ByType(userId, self.dBPaiMainInfo_Gemstone);
            self.OnDeleteRole_ByType(userId, self.dBPaiMainInfo_Equipment);
        }

        public static void OnDeleteRole_ByType(this ConsignSceneComponent self, long userId, DBConsignInfo dBPaiMainInfo)
        {
            List<ConsignItemInfo> paimaiItems = dBPaiMainInfo.PaiMaiItemInfos;

            for (int i = paimaiItems.Count - 1; i >= 0; i--)
            {
                ConsignItemInfo paiMaiItem = paimaiItems[i];
                if (paiMaiItem.UserId != userId)
                {
                    continue;
                }

                dBPaiMainInfo.PaiMaiItemInfos.RemoveAt(i);
            }
        }

        //遍历上架道具
        public static void UpdateShangJiaItems(this ConsignSceneComponent self)
        {
            self.UpdateShangJiaItems_ByType(self.dBPaiMainInfo_Consume );
            self.UpdateShangJiaItems_ByType(self.dBPaiMainInfo_Material);
            self.UpdateShangJiaItems_ByType(self.dBPaiMainInfo_Equipment);
            self.UpdateShangJiaItems_ByType(self.dBPaiMainInfo_Gemstone);
        }


        public static void UpdateShangJiaItems_ByType(this ConsignSceneComponent self, DBConsignInfo dBPaiMainInfo)
        {
            List<ConsignItemInfo> paimaiItems = dBPaiMainInfo.PaiMaiItemInfos;

            for (int i = paimaiItems.Count - 1; i >= 0; i--)
            {
                ConsignItemInfo paiMaiItem = paimaiItems[i];

                //int price = 0;
                int itemId = paiMaiItem.BagInfo.ItemID;
            }
        }



        //根据道具ID获取对应快捷购买的列表
        public static void PaiMaiShopInfoAddBuyNum(this ConsignSceneComponent self, long needItemID, int buyNum)
        {
            foreach (ConsignShopItemInfo info in self.dBPaiMainInfo_Shop.PaiMaiShopItemInfos)
            {
                if (info.Id == needItemID)
                {
                    info.BuyNum += buyNum;
                }
            }
        }

        public static async ETTask SaveDB(this ConsignSceneComponent self, int random)
        {
            //if (random == 1)
            //{
            //    if (RandomHelper.RandomNumber(1,3) != 1)
            //    {
            //        return;
            //    }
            //}

            int zone = self.DomainZone();
            await self.CheckOverTime(self.dBPaiMainInfo_Consume);
            await self.CheckOverTime(self.dBPaiMainInfo_Material);
            await self.CheckOverTime(self.dBPaiMainInfo_Equipment);
            await self.CheckOverTime(self.dBPaiMainInfo_Gemstone);
            await self.CheckOverTime(self.dBPaiMainInfo_Stall);

            await self.SavePaiMaiData(ConsignHelper.GetPaiMaiId(1), self.dBPaiMainInfo_Consume);
            await TimerComponent.Instance.WaitAsync(RandomHelper.RandomNumber(1000,5000));
            await self.SavePaiMaiData(ConsignHelper.GetPaiMaiId(2), self.dBPaiMainInfo_Material);
            await TimerComponent.Instance.WaitAsync(RandomHelper.RandomNumber(1000, 5000));
            await self.SavePaiMaiData(ConsignHelper.GetPaiMaiId(3), self.dBPaiMainInfo_Equipment);
            await TimerComponent.Instance.WaitAsync(RandomHelper.RandomNumber(1000, 5000));
            await self.SavePaiMaiData(ConsignHelper.GetPaiMaiId(4), self.dBPaiMainInfo_Gemstone);

            await self.SavePaiMaiData(ConsignHelper.GetPaiMaiId(11), self.dBPaiMainInfo_Shop);
            await self.SavePaiMaiData(ConsignHelper.GetPaiMaiId(12), self.dBPaiMainInfo_Stall);
        }

        public static async ETTask SavePaiMaiData(this ConsignSceneComponent self, long unitId, DBConsignInfo dBPaiMainInfo)
        {
            //Log.Warning($"PaiMaiSceneComponent.SaveDB:  zone:{self.DomainZone()}  id:{unitId}  {dBPaiMainInfo.PaiMaiItemInfos.Count}");
            
            //long dbCacheId = DBHelper.GetDbCacheId(self.DomainZone());
            //D2M_SaveComponent d2GSave = (D2M_SaveComponent)await ActorMessageSenderComponent.Instance.Call(dbCacheId, new M2D_SaveComponent() { UnitId = unitId, EntityByte = MongoHelper.ToBson(dBPaiMainInfo), ComponentType = DBHelper.DBPaiMainInfo });
            await Game.Scene.GetComponent<DBComponent>().Save<DBConsignInfo>(self.DomainZone(), dBPaiMainInfo);
        }

        public static async ETTask CheckOverTime(this ConsignSceneComponent self, DBConsignInfo dBPaiMainInfo)
        {

            await ETTask.CompletedTask;
            //检测超时的道具
            long currentTime = TimeHelper.ServerNow();

            HashSet<long> removeIdSet = new HashSet<long>();
            for (int i = 1600; i < dBPaiMainInfo.PaiMaiItemInfos.Count; i++ )
            {
                removeIdSet.Add(dBPaiMainInfo.PaiMaiItemInfos[i].Id);
            }

            for (int i = dBPaiMainInfo.PaiMaiItemInfos.Count - 1; i >= 0; i--)
            {
                ConsignItemInfo paiMaiItemInfo = dBPaiMainInfo.PaiMaiItemInfos[i];
                if (currentTime - paiMaiItemInfo.SellTime >= TimeHelper.OneDay || removeIdSet.Contains(paiMaiItemInfo.Id))
                {
                    long emaiId = StartSceneConfigCategory.Instance.GetBySceneName(self.DomainZone(), Enum.GetName(SceneType.Mail)).InstanceId;
                    //Mail2Consign_PaiMaiOverTimeResponse g_SendChatRequest = (E2P_PaiMaiOverTimeResponse)await ActorMessageSenderComponent.Instance.Call
                    //    (emaiId, new Consign2Mail_AuctionOverTimeRequest()
                    //    {
                    //        PaiMaiItemInfo = paiMaiItemInfo
                    //    });
                    //dBPaiMainInfo.PaiMaiItemInfos.RemoveAt(i);
                }
            }
        }

    }
}

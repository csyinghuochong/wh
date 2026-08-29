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
        }
    }


    public static class ConsignSceneComponentSystem
    {


        public static List<ConsignItemInfo> GetItemListByUser(this ConsignSceneComponent self, long useriD, List<ConsignItemInfo> oldPaiMaiAl)
        {
            List<ConsignItemInfo> paiMaiType = new List<ConsignItemInfo>();

            for (int i = 0; i < oldPaiMaiAl.Count; i++)
            {
                ConsignItemInfo item = oldPaiMaiAl[i];
                if (item.UserId == useriD)
                {
                    paiMaiType.Add(item);
                }
            }

            return paiMaiType;
        }

        public static void UpdatePaiMaiDBByBelongId(this ConsignSceneComponent self, int belongId, DBConsignInfo dBPaiMainInfo_Type)
        {
            if (dBPaiMainInfo_Type == null)
            {
                Log.Error($"UpdatePaiMaiDBByBelongId null: {belongId}");
                return;
            }

            self.ShangJiaByBelongId[belongId] = dBPaiMainInfo_Type;
        }

        public static DBConsignInfo GetPaiMaiDBByBelongId(this ConsignSceneComponent self, int belongId)
        {
            self.ShangJiaByBelongId.TryGetValue(belongId, out DBConsignInfo db);
            return db;
        }

        public static DBConsignInfo GetOrCreatePaiMaiDBByBelongId(this ConsignSceneComponent self, int belongId)
        {
            DBConsignInfo db = self.GetPaiMaiDBByBelongId(belongId);
            if (db != null)
            {
                return db;
            }

            db = self.AddChildWithId<DBConsignInfo>(belongId);
            self.UpdatePaiMaiDBByBelongId(belongId, db);
            return db;
        }

        /// <summary>汇总玩家在所有上架分类中的寄售物</summary>
        public static List<ConsignItemInfo> GetUserShangJiaItems(this ConsignSceneComponent self, long userId)
        {
            List<ConsignItemInfo> result = new List<ConsignItemInfo>();
            foreach (DBConsignInfo db in self.ShangJiaByBelongId.Values)
            {
                if (db?.PaiMaiItemInfos == null)
                {
                    continue;
                }

                result.AddRange(self.GetItemListByUser(userId, db.PaiMaiItemInfos));
            }

            return result;
        }

        public static void FillListPage(this ConsignSceneComponent self, List<ConsignItemInfo> paimaiListShow, int page, Consign2C_ListResponse response)
        {
            if (paimaiListShow == null)
            {
                paimaiListShow = new List<ConsignItemInfo>();
            }

            //每个belongid 每次请求 30个为一页
            int pagenum = 30;

            int maxpage = paimaiListShow.Count / pagenum;
            int extra = (paimaiListShow.Count % pagenum) > 0 ? 1 : 0;
            maxpage += extra;

            int startindex = (page - 1) * pagenum;
            if (startindex >= paimaiListShow.Count)
            {
                startindex = paimaiListShow.Count - 1;
            }

            if (startindex < 0)
            {
                startindex = 0;
            }

            if (page >= maxpage)
            {
                if (page == maxpage)
                {
                    int getnumber = Math.Max(paimaiListShow.Count - startindex, 0);
                    response.ConsignItemInfo = paimaiListShow.GetRange(startindex, getnumber);
                    response.Message = "1";
                    response.NextPage = maxpage;
                }
                else
                {
                    if (paimaiListShow.Count > 0)
                    {
                        response.Error = ErrorCode.ERR_PaiMaiBuyMaxPage;
                    }
                }
            }
            else
            {
                int getnumber = Math.Min(paimaiListShow.Count - startindex, pagenum);
                response.ConsignItemInfo = paimaiListShow.GetRange(startindex, getnumber);
                response.Message = "0";
                response.NextPage = maxpage;
            }
        }

        public static ConsignItemInfo RemoveShangJiaItem(this ConsignSceneComponent self, int belongId, long consignItemInfoId)
        {
            if (belongId > 0)
            {
                DBConsignInfo db = self.GetPaiMaiDBByBelongId(belongId);
                return self.RemoveShangJiaItemInDb(db, consignItemInfoId);
            }

            foreach (DBConsignInfo db in self.ShangJiaByBelongId.Values)
            {
                ConsignItemInfo removed = self.RemoveShangJiaItemInDb(db, consignItemInfoId);
                if (removed != null)
                {
                    return removed;
                }
            }

            return null;
        }

        public static ConsignItemInfo RemoveShangJiaItemInDb(this ConsignSceneComponent self, DBConsignInfo db, long consignItemInfoId)
        {
            if (db?.PaiMaiItemInfos == null)
            {
                return null;
            }

            for (int i = db.PaiMaiItemInfos.Count - 1; i >= 0; i--)
            {
                if (db.PaiMaiItemInfos[i].Id == consignItemInfoId)
                {
                    ConsignItemInfo item = db.PaiMaiItemInfos[i];
                    db.PaiMaiItemInfos.RemoveAt(i);
                    return item;
                }
            }

            return null;
        }

        public static ConsignItemInfo FindShangJiaItem(this ConsignSceneComponent self, int belongId, long consignItemInfoId, out List<ConsignItemInfo> belongList)
        {
            belongList = null;
            if (belongId > 0)
            {
                DBConsignInfo db = self.GetPaiMaiDBByBelongId(belongId);
                belongList = db?.PaiMaiItemInfos;
                return self.FindShangJiaItemInList(belongList, consignItemInfoId);
            }

            foreach (DBConsignInfo db in self.ShangJiaByBelongId.Values)
            {
                ConsignItemInfo item = self.FindShangJiaItemInList(db?.PaiMaiItemInfos, consignItemInfoId);
                if (item != null)
                {
                    belongList = db.PaiMaiItemInfos;
                    return item;
                }
            }

            return null;
        }

        public static ConsignItemInfo FindShangJiaItemInList(this ConsignSceneComponent self, List<ConsignItemInfo> list, long consignItemInfoId)
        {
            if (list == null)
            {
                return null;
            }

            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Id == consignItemInfoId)
                {
                    return list[i];
                }
            }

            return null;
        }

        public static async ETTask InitPaiMaiShangJia(this ConsignSceneComponent self, int belongId)
        {
            long unitid = belongId;
            List<DBConsignInfo> paimaiList = await Game.Scene.GetComponent<DBComponent>().Query<DBConsignInfo>(self.DomainZone(), d => d.Id == unitid);
            if (paimaiList == null || paimaiList.Count == 0)
            {
                DBConsignInfo dBPaiMainInfo = self.AddChildWithId<DBConsignInfo>(unitid);
                self.UpdatePaiMaiDBByBelongId(belongId, dBPaiMainInfo);
                await Game.Scene.GetComponent<DBComponent>().Save<DBConsignInfo>(self.DomainZone(), dBPaiMainInfo);
            }
            else
            {
                self.AddChild(paimaiList[0]);
                self.UpdatePaiMaiDBByBelongId(belongId, paimaiList[0]);
            }
        }

        public static async ETTask InitDBData(this ConsignSceneComponent self)
        {
            await TimerComponent.Instance.WaitAsync(RandomHelper.RandomNumber(1000, 10000));

            int[] belongIds = ConsignHelper.GetShangJiaBelongIds();
            for (int i = 0; i < belongIds.Length; i++)
            {
                await self.InitPaiMaiShangJia(belongIds[i]);
            }

            await self.CheckAllOverTime();

            await self.InitWantBuyData();

            self.Timer = TimerComponent.Instance.NewRepeatedTimer(TimeHelper.Minute * 30 + RandomHelper.RandomNumber(1000, 10000), TimerType.ConsignSceneTimer, self);
        }

        //零点刷新
        public static void OnZeroClockUpdate(this ConsignSceneComponent self)
        {
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

            foreach (DBConsignInfo db in self.ShangJiaByBelongId.Values)
            {
                self.OnDeleteRole_ByType(userId, db);
            }

            self.OnDeleteRoleWantBuy(userId);
        }

        public static void OnDeleteRole_ByType(this ConsignSceneComponent self, long userId, DBConsignInfo dBPaiMainInfo)
        {
            if (dBPaiMainInfo?.PaiMaiItemInfos == null)
            {
                return;
            }

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

 

        public static async ETTask SaveDB(this ConsignSceneComponent self, int random)
        {
            //if (random == 1)
            //{
            //    if (RandomHelper.RandomNumber(1,3) != 1)
            //    {
            //        return;
            //    }
            //}

            foreach (KeyValuePair<int, DBConsignInfo> kv in self.ShangJiaByBelongId)
            {
                await self.CheckOverTime(kv.Value);
                await self.SavePaiMaiData(kv.Value);
                await TimerComponent.Instance.WaitAsync(RandomHelper.RandomNumber(1000, 5000));
            }

            foreach (KeyValuePair<long, DBConsignWantBuy> kv in self.WantBuyByItemKey)
            {
                await self.SaveWantBuyData(kv.Value);
                await TimerComponent.Instance.WaitAsync(RandomHelper.RandomNumber(1000, 3000));
            }
        }

        public static async ETTask SavePaiMaiData(this ConsignSceneComponent self, DBConsignInfo dBPaiMainInfo)
        {
            await Game.Scene.GetComponent<DBComponent>().Save<DBConsignInfo>(self.DomainZone(), dBPaiMainInfo);
        }

        public static async ETTask CheckAllOverTime(this ConsignSceneComponent self)
        {
            foreach (DBConsignInfo db in self.ShangJiaByBelongId.Values)
            {
                await self.CheckOverTime(db);
            }
        }

        public static async ETTask CheckOverTime(this ConsignSceneComponent self, DBConsignInfo dBPaiMainInfo)
        {
            if (dBPaiMainInfo?.PaiMaiItemInfos == null)
            {
                return;
            }

            for (int i = dBPaiMainInfo.PaiMaiItemInfos.Count - 1; i >= 0; i--)
            {
                ConsignItemInfo paiMaiItemInfo = dBPaiMainInfo.PaiMaiItemInfos[i];
                if (!ConsignHelper.IsConsignExpired(paiMaiItemInfo))
                {
                    continue;
                }

                dBPaiMainInfo.PaiMaiItemInfos.RemoveAt(i);
                await MailHelp.SendConsignOverTimeMail(paiMaiItemInfo);
            }
        }

        public static long GetWantBuyDbId(this ConsignSceneComponent self, int itemType, int itemId)
        {
            return ConsignHelper.GetWantBuyKey(itemType, itemId);
        }

        public static DBConsignWantBuy GetWantBuyDB(this ConsignSceneComponent self, int itemType, int itemId)
        {
            self.WantBuyByItemKey.TryGetValue(self.GetWantBuyDbId(itemType, itemId), out DBConsignWantBuy db);
            return db;
        }

        public static DBConsignWantBuy GetOrCreateWantBuyDB(this ConsignSceneComponent self, int itemType, int itemId)
        {
            long dbId = self.GetWantBuyDbId(itemType, itemId);
            if (self.WantBuyByItemKey.TryGetValue(dbId, out DBConsignWantBuy db) && db != null)
            {
                return db;
            }

            db = self.AddChildWithId<DBConsignWantBuy>(dbId);
            self.WantBuyByItemKey[dbId] = db;
            return db;
        }

        public static async ETTask InitWantBuyData(this ConsignSceneComponent self)
        {
            List<DBConsignWantBuy> list = await Game.Scene.GetComponent<DBComponent>().Query<DBConsignWantBuy>(self.DomainZone(), d => d.Id > 0);
            if (list == null)
            {
                return;
            }

            for (int i = 0; i < list.Count; i++)
            {
                DBConsignWantBuy db = list[i];
                if (db == null)
                {
                    continue;
                }

                self.AddChild(db);
                self.WantBuyByItemKey[db.Id] = db;
            }
        }

        public static async ETTask SaveWantBuyData(this ConsignSceneComponent self, DBConsignWantBuy db)
        {
            if (db == null)
            {
                return;
            }

            await Game.Scene.GetComponent<DBComponent>().Save<DBConsignWantBuy>(self.DomainZone(), db);
        }

        public static List<ConsignWantBuyInfo> GetWantBuyList(this ConsignSceneComponent self, int itemType, int itemId)
        {
            DBConsignWantBuy db = self.GetWantBuyDB(itemType, itemId);
            if (db?.WantBuyInfos == null)
            {
                return new List<ConsignWantBuyInfo>();
            }

            return new List<ConsignWantBuyInfo>(db.WantBuyInfos);
        }

        public static List<ConsignItemInfo> GetShangJiaItemsByItem(this ConsignSceneComponent self, int itemType, int itemId)
        {
            List<ConsignItemInfo> result = new List<ConsignItemInfo>();
            foreach (DBConsignInfo db in self.ShangJiaByBelongId.Values)
            {
                if (db?.PaiMaiItemInfos == null)
                {
                    continue;
                }

                for (int i = 0; i < db.PaiMaiItemInfos.Count; i++)
                {
                    ConsignItemInfo item = db.PaiMaiItemInfos[i];
                    if (item?.BagInfo == null)
                    {
                        continue;
                    }

                    if (item.BagInfo.ItemID != itemId)
                    {
                        continue;
                    }

                    if (itemType > 0 && item.BagInfo.ItemType != itemType)
                    {
                        continue;
                    }

                    result.Add(item);
                }
            }

            return result;
        }

        public static ConsignWantBuyInfo FindWantBuy(this ConsignSceneComponent self, int itemType, int itemId, long wantBuyId)
        {
            List<ConsignWantBuyInfo> list = self.GetWantBuyList(itemType, itemId);
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Id == wantBuyId)
                {
                    return list[i];
                }
            }

            return null;
        }

        public static ConsignWantBuyInfo DealWantBuy(this ConsignSceneComponent self, int itemType, int itemId, long wantBuyId, int sellNum, long sellerUserId)
        {
            DBConsignWantBuy db = self.GetWantBuyDB(itemType, itemId);
            if (db?.WantBuyInfos == null)
            {
                return null;
            }

            for (int i = 0; i < db.WantBuyInfos.Count; i++)
            {
                ConsignWantBuyInfo info = db.WantBuyInfos[i];
                if (info.Id != wantBuyId)
                {
                    continue;
                }

                if (info.UserId == sellerUserId)
                {
                    return null;
                }

                if (sellNum <= 0 || sellNum > info.ItemNum)
                {
                    return null;
                }

                info.ItemNum -= sellNum;
                if (info.ItemNum <= 0)
                {
                    db.WantBuyInfos.RemoveAt(i);
                }

                return info;
            }

            return null;
        }

        public static void OnDeleteRoleWantBuy(this ConsignSceneComponent self, long userId)
        {
            if (userId <= 0)
            {
                return;
            }

            foreach (DBConsignWantBuy db in self.WantBuyByItemKey.Values)
            {
                if (db?.WantBuyInfos == null)
                {
                    continue;
                }

                for (int i = db.WantBuyInfos.Count - 1; i >= 0; i--)
                {
                    ConsignWantBuyInfo info = db.WantBuyInfos[i];
                    if (info.UserId != userId)
                    {
                        continue;
                    }

                    db.WantBuyInfos.RemoveAt(i);
                    MailHelp.SendWantBuyGoldMail(info.UserId, (long)info.Price * info.ItemNum).Coroutine();
                }
            }
        }

    }
}

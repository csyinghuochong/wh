using System.Collections.Generic;

namespace ET
{
    public class ConsignSceneComponent : Entity, IAwake, IDestroy
    {
        public long Timer;
        public long AuctionOverTimer;

        /// <summary>
        /// 0没开始 1当前时间小于该值表示开始 2当前时间大于等于该值表示结束  -1也表示结束
        /// </summary>
        public long AuctionStatus;

        public int AuctionItem;
        public long AuctionPrice;
        public long AuctionStart;
        public long AuctioUnitId;
        public int AuctionItemNum;
        public string AuctionPlayer;
        public List<long> AuctionJoinList = new List<long>();
        public List<AuctionRecord> AuctionRecords = new List<AuctionRecord>();

        /// <summary>
        /// 拍卖上架：key = ItemType（1消耗 2材料 3装备 4宝石…），DB Id = 1000 + ItemType，类型各存各的文档。
        /// </summary>
        public Dictionary<int, DBConsignInfo> ShangJiaByType = new Dictionary<int, DBConsignInfo>();

        /// <summary>拍卖商店 id = 1011</summary>
        public DBConsignInfo dBPaiMainInfo_Shop;
    }
}

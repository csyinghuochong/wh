using System.Collections.Generic;

namespace ET
{
    public class ConsignSceneComponent : Entity, IAwake, IDestroy
    {
        public long Timer;

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
        /// 上架分桶：key / DB Id = 道具表、装备表上的具体 belongid（166020 等大分类的下一级）。
        /// </summary>
        public Dictionary<int, DBConsignInfo> ShangJiaByBelongId = new Dictionary<int, DBConsignInfo>();

        /// <summary>
        /// 求购分桶：key / DB Id = GetWantBuyKey(ItemType, ItemId)
        /// </summary>
        public Dictionary<long, DBConsignWantBuy> WantBuyByItemKey = new Dictionary<long, DBConsignWantBuy>();
    }
}

using System.Collections.Generic;

namespace ET
{
    /// <summary>
    /// 一批道具的入包参数。平时只用构造函数传来源；指定落点、制作人、制作时间再改字段。
    /// </summary>
    public struct AddItemArg
    {
        public string GetWay;
        public bool Notice;
        public ItemLocType Loc;
        public string MakePlayer;
        public long MakeTime;
        /// <summary>可选结果接收列表；保留入包实例信息的副本，ItemNum只累计本次入该格的数量，不含原有数量。</summary>
        public List<BagInfo> ResultItems;

        public AddItemArg(string getWay, bool notice = true)
        {
            GetWay = getWay;
            Notice = notice;
            Loc = ItemLocType.ItemLocMax;
            MakePlayer = string.Empty;
            MakeTime = 0;
            ResultItems = null;
        }
    }
}

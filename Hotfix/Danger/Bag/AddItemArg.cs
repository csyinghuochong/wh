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

        public AddItemArg(string getWay, bool notice = true)
        {
            GetWay = getWay;
            Notice = notice;
            Loc = ItemLocType.ItemLocMax;
            MakePlayer = string.Empty;
            MakeTime = 0;
        }
    }
}

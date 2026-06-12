using System.Collections.Generic;


namespace ET
{

    public struct DayMonsters
    {
        public int MonsterId;
        public float GaiLv;
        public int TotalNumber;
    }

    public struct DayJingLing
    {
        public List<int> MonsterId;
        public List<int> Weights;
        public float GaiLv;
        public int TotalNumber;
    }

    public partial class LDGlobalValueCategory
    {

        public int JianDingFuQulity = 0;

        public int FangunSkillId = 0;

        public int BagInitCapacity = 0;
        public int BagMaxCapacity = 0;

        public int HourseInitCapacity = 0;
        public int HourseMaxCapacity = 0;

        public int GemStoreInitCapacity = 0;
        public int GemStoreMaxCapacity = 0;

        public int OnLineLimit = 0;

        public int AccountBagMax = 0;

        public int MaxLevel = 0;

        public List<DayMonsters> DayMonsterList = new List<DayMonsters>();

        public List<DayJingLing> DayJingLingList = new List<DayJingLing>();

        public Dictionary<int, int> ZhuaPuItem = new Dictionary<int, int>();

        public int TempValue = 0;

        public override void AfterEndInit()
        {
            DayMonsterList.Clear();
         

            /*string[] dayrefresh = this.Get(79).Value.Split('@');
            for (int i = 0; i < dayrefresh.Length; i++)
            {
                string[] itemInfo = dayrefresh[i].Split(';');
                if (itemInfo.Length < 3)
                {
                    Log.Error($"itemInfo.Length < 3: {dayrefresh[i]}");
                    continue;
                }

                if (!int.TryParse(itemInfo[0], out int monsterId))
                {
                    Log.Error($"int.TryParse error: {itemInfo[0]}");
                    continue;
                }

                if (!float.TryParse(itemInfo[1], out float gaiLv))
                {
                    Log.Error($"float.TryParse error: {itemInfo[1]}");
                    continue;
                }

                if (!int.TryParse(itemInfo[2], out int total))
                {
                    Log.Error($"int.TryParse error: {itemInfo[2]}");
                    continue;
                }

                DayMonsterList.Add(new DayMonsters()
                {
                    MonsterId = monsterId,
                    GaiLv = gaiLv,
                    TotalNumber = total
                });
            }

            string[] jinglingfresh = this.Get(80).Value.Split('@');
            for (int i = 0; i < jinglingfresh.Length; i++)
            {
                string[] itemInfo = jinglingfresh[i].Split(';');
                if (itemInfo.Length < 3)
                {
                    Log.Error($"itemInfo.Length < 3: {jinglingfresh[i]}");
                    continue;
                }

                if (!float.TryParse(itemInfo[0], out float gaiLv))
                {
                    Log.Error($"float.TryParse error: {itemInfo[0]}");
                    continue;
                }

                if (!int.TryParse(itemInfo[1], out int total))
                {
                    Log.Error($"int.TryParse error: {itemInfo[1]}");
                    continue;
                }

                DayJingLing dayJingLing = new DayJingLing();
                dayJingLing.MonsterId = new List<int>();
                dayJingLing.Weights = new List<int>();

                string[] monsterIist = itemInfo[2].Split('&');
                for (int m = 0; m < monsterIist.Length; m++)
                {
                    string[] monsterid = monsterIist[m].Split(',');
                    if (monsterid.Length < 2)
                    {
                        Log.Error($"monsterid.Length < 2: {monsterIist[m]}");
                        continue;
                    }

                    if (!int.TryParse(monsterid[0], out int weight))
                    {
                        Log.Error($"int.TryParse error: {monsterid[0]}");
                        continue;
                    }

                    if (!int.TryParse(monsterid[1], out int monsterId))
                    {
                        Log.Error($"int.TryParse error: {monsterid[1]}");
                        continue;
                    }

                    dayJingLing.Weights.Add(weight);
                    dayJingLing.MonsterId.Add(monsterId);
                }

                dayJingLing.GaiLv = gaiLv;
                dayJingLing.TotalNumber = total;    
                DayJingLingList.Add(dayJingLing);
            }

            string[] zhuabuItems = this.Get(82).Value.Split('@');
            for (int i = 0; i < zhuabuItems.Length; i++)
            {
                string[] zhubuids = zhuabuItems[i].Split(';');
                if (zhubuids.Length < 2)
                {
                    Log.Error($"zhubuids.Length < 2: {zhuabuItems[i]}");
                    continue;
                }

                if (!int.TryParse(zhubuids[0], out int itemId))
                {
                    Log.Error($"int.TryParse error: {zhubuids[0]}");
                    continue;
                }

                if (!int.TryParse(zhubuids[1], out int itemNum))
                {
                    Log.Error($"int.TryParse error: {zhubuids[1]}");
                    continue;
                }

                ZhuaPuItem.Add(itemId, itemNum);
            }*/
        }


    }
}

using System.Collections.Generic;

namespace ET
{
    public static class RoleInfoComponentSystemEx
    {

        public static List<int> GetMakeListByType(this RoleInfoComponent self, int makeType)
        {
            List<int> makeIds =  new List<int> { };
            if (makeType == 0)
            { 
                return makeIds;
            }
            for(int i = 0; i < self.RoleInfo.MakeList.Count; i++)
            {
                EquipMakeConfig equipMakeConfig = EquipMakeConfigCategory.Instance.Get(self.RoleInfo.MakeList[i]);
                if (equipMakeConfig.ProficiencyType == makeType)
                {
                    makeIds.Add(self.RoleInfo.MakeList[i]);
                }
            }
            return makeIds; 
        }

        public static void OnResetSeason(this RoleInfoComponent self, bool notice)
        {
            self.RoleInfo.SeasonLevel = 1;
            self.RoleInfo.SeasonExp = 0;
            self.RoleInfo.SeasonCoin = 0;
            self.RoleInfo.OpenJingHeIds.Clear();
        }

        public static void ClearMakeListByType(this RoleInfoComponent self, int makeType)
        {
            if (makeType == 0)
            {
                return;
            }
            for (int i = self.RoleInfo.MakeList.Count - 1; i >= 0; i--)
            {
                int makeId = self.RoleInfo.MakeList[i];
                if (makeId == 0)
                {
                    self.RoleInfo.MakeList.RemoveAt(i);
                    continue;
                }

                EquipMakeConfig equipMakeConfig = EquipMakeConfigCategory.Instance.Get(makeId);
                if (equipMakeConfig.ProficiencyType == makeType)
                {
                    self.RoleInfo.MakeList.RemoveAt(i); 
                }
            }
        }

        public static int GetMonsterKillNumber(this RoleInfoComponent self, int monsterId)
        {
            for (int i = 0; i < self.RoleInfo.MonsterRevives.Count; i++)
            {
                KeyValuePair keyValuePair = self.RoleInfo.MonsterRevives[i];
                if (keyValuePair.KeyId != monsterId)
                {
                    continue;
                }
                if (!string.IsNullOrEmpty(keyValuePair.Value2))
                {
                    return int.Parse(keyValuePair.Value2);
                }
                else
                {
                    return 1;
                }
            }
            return 0;
        }

        public static long GetReviveTime(this RoleInfoComponent self, int monsterId)
        {
            for (int i = 0; i < self.RoleInfo.MonsterRevives.Count; i++)
            {
                if (self.RoleInfo.MonsterRevives[i].KeyId == monsterId)
                {
                    return long.Parse(self.RoleInfo.MonsterRevives[i].Value);
                }
            }
            return 0;
        }
       
        public static long GetSceneFubenTimes(this RoleInfoComponent self, int sceneId)
        {
            for (int i = 0; i < self.RoleInfo.DayFubenTimes.Count; i++)
            {
                if (self.RoleInfo.DayFubenTimes[i].KeyId == sceneId)
                {
                    return self.RoleInfo.DayFubenTimes[i].Value;
                }
            }
            return 0;
        }
       
        public static int GetDayItemUse(this RoleInfoComponent self, int mysteryId)
        {
            for (int i = 0; i < self.RoleInfo.DayItemUse.Count; i++)
            {
                if (self.RoleInfo.DayItemUse[i].KeyId == mysteryId)
                {
                    return (int)self.RoleInfo.DayItemUse[i].Value;
                }
            }
            return 0;
        }


        public static void OnDayItemUse(this RoleInfoComponent self, int itemId)
        {
            for (int i = 0; i < self.RoleInfo.DayItemUse.Count; i++)
            {
                if (self.RoleInfo.DayItemUse[i].KeyId == itemId)
                {
                    self.RoleInfo.DayItemUse[i].Value += 1;
                    return;
                }
            }
            self.RoleInfo.DayItemUse.Add(new KeyValuePairInt() { KeyId = itemId, Value = 1 });
        }

        public static int GetTotalUseTimes(this RoleInfoComponent self, int mysteryId)
        {
            for (int i = 0; i < self.RoleInfo.TotalUseTimes.Count; i++)
            {
                if (self.RoleInfo.TotalUseTimes[i].KeyId == mysteryId)
                {
                    return (int)self.RoleInfo.TotalUseTimes[i].Value;
                }
            }
            return 0;
        }

        public static void OnTotalUseTimes(this RoleInfoComponent self, int itemId, int useNumber = 1)
        {
            for (int i = 0; i < self.RoleInfo.TotalUseTimes.Count; i++)
            {
                if (self.RoleInfo.TotalUseTimes[i].KeyId == itemId)
                {
                    self.RoleInfo.TotalUseTimes[i].Value += useNumber;
                    return;
                }
            }
            self.RoleInfo.TotalUseTimes.Add(new KeyValuePairInt() { KeyId = itemId, Value = useNumber });
        }

        public static void AddSceneFubenTimes(this RoleInfoComponent self, int sceneId)
        {
            for (int i = 0; i < self.RoleInfo.DayFubenTimes.Count; i++)
            {
                if (self.RoleInfo.DayFubenTimes[i].KeyId == sceneId)
                {
                    self.RoleInfo.DayFubenTimes[i].Value++;
                    return;
                }
            }
            self.RoleInfo.DayFubenTimes.Add(new KeyValuePairInt() { KeyId = sceneId, Value = 1 });
        }

        public static void ClearFubenTimes(this RoleInfoComponent self, int sceneId)
        {
            for (int i = 0; i < self.RoleInfo.DayFubenTimes.Count; i++)
            {
                if (self.RoleInfo.DayFubenTimes[i].KeyId == sceneId)
                {
                    self.RoleInfo.DayFubenTimes[i].Value = 0;
                    break;
                }
            }
        }

        public static int GetMaxLevel(this RoleInfoComponent self, List<int> compeltetask)
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

        public static void AddFubenTimes(this RoleInfoComponent self, int sceneId, int times)
        {
            for (int i = 0; i < self.RoleInfo.DayFubenTimes.Count; i++)
            {
                if (self.RoleInfo.DayFubenTimes[i].KeyId == sceneId)
                {
                    long curTimes = self.RoleInfo.DayFubenTimes[i].Value -= times;
                    if (curTimes < 0)
                    {
                        curTimes = 0;
                    }
                    self.RoleInfo.DayFubenTimes[i].Value = curTimes;
                    break;
                }
            }
        }

    }



}
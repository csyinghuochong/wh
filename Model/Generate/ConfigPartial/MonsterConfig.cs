using MongoDB.Driver.Linq;
using System.Collections.Generic;

namespace ET
{
    public partial class MonsterConfigCategory
    {

        public Dictionary<int, List<KeyValuePair<int, int>>> OpenDayMonsters = new Dictionary<int, List<KeyValuePair<int, int>>>();

        //public List<int> NoSkillMonsterList = new List<int>();

        public override void AfterEndInit()
        {
            foreach (MonsterConfig monsterConfig in this.GetAll().Values)
            {
                //1;70001001@3;70001003
                if (string.IsNullOrEmpty(monsterConfig.OpenDayMonster))
                {
                    continue;
                }

                string[] opendaylist = monsterConfig.OpenDayMonster.Split('@');
                for (int i = 0; i < opendaylist.Length; i++)
                { 
                    string[] monsterinfo  = opendaylist[i].Split(";");
                    if(monsterinfo.Length < 2)
                    {
                        continue;
                    }

                    if (!OpenDayMonsters.ContainsKey(monsterConfig.Id))
                    {
                        OpenDayMonsters.Add(monsterConfig.Id, new List<KeyValuePair<int, int>>());
                    }
                    if (!int.TryParse(monsterinfo[0], out int openDay))
                    {
                        Log.Error($"int.TryParse error: {monsterinfo[0]} monsterId:{monsterConfig.Id}");
                        continue;
                    }

                    if (!int.TryParse(monsterinfo[1], out int monster))
                    {
                        Log.Error($"int.TryParse error: {monsterinfo[1]} monsterId:{monsterConfig.Id}");
                        continue;
                    }

                    OpenDayMonsters[monsterConfig.Id].Add(new KeyValuePair<int, int>(openDay, monster));
                }

                //if(monsterConfig.ActSkillID != 70000001)
                //{
                //    continue;
                //}
                //int[] skillids = monsterConfig.SkillID;
                //if (skillids != null)
                //{
                //    for (int i = 0; i < skillids.Length; i++)
                //    {
                //        if (skillids[i] != 0)
                //        {
                //            continue;
                //        }
                //    }
                //}

                //NoSkillMonsterList.Add(monsterConfig.Id);
            }
        }

        public int GetNewMonsterId(int openDay, int monsterId)
        {
            List<KeyValuePair<int, int>> monsterList = null;
            OpenDayMonsters.TryGetValue( monsterId, out monsterList );
            if (monsterList == null)
            {
                return monsterId;
            }

            for (int i = monsterList.Count - 1; i >= 0; i--)
            { 
                if(openDay >= monsterList[i].Key)
                {
                    return monsterList[i].Value;
                }
            }
            return monsterId;
        }

    }
}

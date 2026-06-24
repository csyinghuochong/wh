using System;
using System.Collections.Generic;
using UnityEngine;

namespace ET
{
    /// <summary>
    /// 自身释放一个指定技能后，如果自身有一个或多个特定的召唤物，其召唤物会跟随角色本身一起释放角色当前技能
    /// </summary>
    public class Skill_Com_Summon_6: SkillHandler
    {
        public override void OnInit(SkillInfo skillId, Unit theUnitFrom)
        {
            this.BaseOnInit(skillId, theUnitFrom);
        }

        public override void OnExecute()
        {
            this.InitSelfBuff();

            Unit theUnitFrom = this.TheUnitFrom;

            if (theUnitFrom.Type == UnitType.Player)
            {
                // 召唤物释放相同技能
                // '90000102,90000103(如果填0是所有)
                // 召唤ID,召唤ID
                /* string[] summonParList = null;// this.LdSkillConf.GameObjectParameter.Split(';');
                 List<int> monsterIds = new List<int>();
                 bool allMonster = false;
                 try
                 {
                     foreach (string s in summonParList)
                     {
                         if (s == "0")
                         {
                             allMonster = true;
                             break;
                         }

                         monsterIds.Add(int.Parse(s));
                     }
                 }
                 catch (Exception ex)
                 {
                     Log.Error("Skill_Com_Summon_6:Error:  ", this.LdSkillConf.Id);
                     Log.Error(ex.ToString());
                     return;
                 }*/

                List<Unit> all = theUnitFrom.GetParent<UnitComponent>().GetAll();
                
            }

            this.OnUpdate();
        }

        public override void OnUpdate()
        {
            this.BaseOnUpdate();
            this.CheckChiXuHurt();
        }

        public override void OnFinished()
        {
            this.Clear();
        }
    }
}
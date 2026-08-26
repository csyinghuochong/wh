using System;
using System.Collections.Generic;

namespace ET
{

    [ObjectSystem]
    public class ChengJiuComponentAwakeSystem : AwakeSystem<ChengJiuComponentServer>
    {
        public override void Awake(ChengJiuComponentServer self)
        {
            self.RandomDrop = 0;
            self.ChengJiuEventCoalesceAdd?.Clear();
            self.ChengJiuEventCoalesceSet?.Clear();
            Unit unit = self.GetParent<Unit>();
            RoleInfo roleInfo = unit.GetComponent<RoleInfoComponentServer>().RoleInfo;
        
        }
    }

    public static class ChengJiuComponentServerSystem
    {

        public static List<AttributeItem> GetJingLingProLists(this ChengJiuComponentServer self)
        {
            List<AttributeItem> proList = new List<AttributeItem>();
           
            for (int i = 0; i < self.JingLingList.Count; i++)
            {
                LDElf jinglingCof = LDElfCategory.Instance.Get(self.JingLingList[i]);
                //NumericHelp.GetProList(jinglingCof.AddProperty, proList);
            }

            if (self.JingLingId == 0)
            {
                return proList;
            }
            LDElf lifeShieldConfig = LDElfCategory.Instance.Get(self.JingLingId);
           // NumericHelp.GetProList(lifeShieldConfig.AddProperty, proList);
            //if (lifeShieldConfig.FunctionType == JingLingFunctionType.AddProperty)
            //{
            //    NumericHelp.GetProList(lifeShieldConfig.FunctionValue, proList);
            //}
            
            return proList;
        }

        public static void OnLogin(this ChengJiuComponentServer self)
        {
            Unit unit = self.GetParent<Unit>();
            RoleInfoComponentServer roleInfo = unit.GetComponent<RoleInfoComponentServer>();
         
        }

        public static void OnDailyReset(this ChengJiuComponentServer self)
        {
            self.RandomDrop = 0;
        }

        //击杀怪物可触发多种类型的成就
        public static void OnKillUnit(this ChengJiuComponentServer self, Unit defend)
        {
            if (defend == null || defend.IsDisposed)
                return;
        }

        public static void OnPassFuben(this ChengJiuComponentServer self, int difficulty, int chapterid, int star)
        {

        }

        public static void OnChouKaTen(this ChengJiuComponentServer self)
        {
        }

        public static void OnEquipXiLian(this ChengJiuComponentServer self, int times)
        {
        }

        /// <summary>
        /// 洗练结果推进（隐藏技能等），Handler 只调此门面
        /// </summary>
        public static void OnEquipXiLianResults(this ChengJiuComponentServer self, List<ItemXiLianResult> results, int times)
        {
        }

        public static void OnMakeEquip(this ChengJiuComponentServer self)
        {

        }

        public static void OnJiaYuanLevel(this ChengJiuComponentServer self, int jiaYuanLv)
        {

        }

        public static void OnCombatToValue(this ChengJiuComponentServer self, int combat)
        {
        }

        public static void OnPetTianTiRank(this ChengJiuComponentServer self, int rankId)
        {
        }

        public static void OnTeamDungeonSettle(this ChengJiuComponentServer self, bool shenYuan)
        {
        }

        public static void OnRevive(this ChengJiuComponentServer self)
        {
        }

        public static void OnUpdateLevel(this ChengJiuComponentServer self, int lv)
        {
        }

        public static void OnGetGold(this ChengJiuComponentServer self, int coin)
        {
        }

        public static void OnGetPet(this ChengJiuComponentServer self, PetInfo rolePetInfo)
        {
           
        }
      
        public static void OnPetHeCheng(this ChengJiuComponentServer self, PetInfo rolePetInfo)
        {
            
        }

        public static void OnPetXiLian(this ChengJiuComponentServer self, PetInfo rolePetInfo)
        {
        }

        public static void OnItemHuiShow(this ChengJiuComponentServer self, int itemNumber)
        {
        }

        public static void OnCostDiamond(this ChengJiuComponentServer self, long costNumber)
        {
        }

        public static void OnActiveJingLing(this ChengJiuComponentServer self, int jid)
        {
            if (self.JingLingList.Contains(jid))
            {
                return;
            }
            self.JingLingList.Add(jid);
        }



 
    }
}

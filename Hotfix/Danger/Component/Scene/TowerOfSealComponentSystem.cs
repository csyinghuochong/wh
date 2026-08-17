using System.Collections.Generic;

namespace ET
{
    /// <summary>
    /// 封印之塔管理
    /// </summary>
    public static class TowerOfSealComponentSystem
    {
        /// <summary>
        /// 怪物被击杀时触发
        /// </summary>
        /// <param name="self"></param>
        /// <param name="defend"></param>
        public static void OnKillEvent(this TowerOfSealComponent self, Unit defend)
        {
            List<Unit> players = UnitHelper.GetUnitList(self.DomainScene(), UnitType.Player);
            if (defend.GetComponent<NumericComponent>().GetAsLong(NumericType.MasterId) == players[0].Id)
            {
                return;
            }

            if (defend.Type != UnitType.Monster)
            {
                return;
            }

            players[0].GetComponent<SkillManagerComponent>().ClearSkillAndCd();

            M2C_FubenSettlement m2C_FubenSettlement = new M2C_FubenSettlement();
            m2C_FubenSettlement.BattleResult = CombatResultEnum.Win;

            //MapComponent mapComponent = self.DomainScene().GetComponent<MapComponent>();
            //int arrived = players[0].GetComponent<NumericComponent>().GetAsInt(NumericType.TowerOfSealArrived);
            //players[0].GetComponent<NumericComponent>().ApplyValue(NumericType.TowerOfSealFinished, arrived);
            //MessageHelper.SendToClient(players[0], m2C_FubenSettlement);
        }

        /// <summary>
        /// 生成副本
        /// </summary>
        /// <param name="self"></param>
        /// <param name="arrivedConfigId">到达的层数</param>
        /// <param name="finishedConfigId">完成的层数</param>
        public static void GenerateFuben(this TowerOfSealComponent self, int arrivedConfigId, int finishedConfigId)
        {
            // 判断该层是否清空
            if (arrivedConfigId > finishedConfigId && arrivedConfigId <= 100)
            {
#if false // TODO: migrate to LD config
                // 根据角色等级，配置封印之塔的
                // 1-19 级  201001
                // 20-29级 202001
                // 30-39级 203001
                // 40-49级 204001
                // 50-59级 205001
                // 60-99级 206001
   
                
                // 读取配置表,根据到达层数生成怪物
                //TowerConfig towerConfig = TowerConfigCategory.Instance.Get(baseLevel + arrivedConfigId);
               // FubenHelp.CreateMonsterList(self.DomainScene(), towerConfig.MonsterSet);
#endif
            }
        }
    }
}
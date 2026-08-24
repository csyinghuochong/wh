using System;
using System.Collections.Generic;

namespace ET
{

    [ObjectSystem]
    public class PetTianTiComponentDestroySystem : DestroySystem<PetTianTiComponent>
    {
        public override void Destroy(PetTianTiComponent self)
        {
            TimerComponent.Instance.Remove(ref self.Timer);
        }
    }

    public static class PetTianTiComponentSystem
    {
        public static  async ETTask GeneratePetFuben(this PetTianTiComponent self)
        {
            Unit unit = self.MainUnit;
            unit.GetComponent<StateComponent>().StateTypeAdd(StateTypeEnum.WuDi);

            PetComponentServer petComponentServer = self.MainUnit.GetComponent<PetComponentServer>();
            petComponentServer.CheckSkin();
            //for (int i = 0; i < petComponentServer.TeamPetList.Count; i++)
            //{
            //    RolePetInfo rolePetInfo = petComponentServer.GetPetInfo(petComponentServer.TeamPetList[i]);
            //    if (rolePetInfo == null)
            //    {
            //        continue;
            //    }
            //    Unit petunit = UnitFactory.CreateTianTiPet(unit.DomainScene(), unit.Id,
            //       unit.GetBattleCamp(), rolePetInfo, AIGetTargetHelp.Formation_1[i], 0f, i);
            //}

            //先查找真实玩家。再查找
            long dbCacheId = DBHelper.GetDbCacheId(self.DomainZone());
            PetComponentServer petComponentServerEnemy = await DBHelper.GetComponent<PetComponentServer>(UnitZoneHelper.GetHomeZone(self.EnemyId), self.EnemyId);
            if (petComponentServerEnemy != null)
            {
                petComponentServerEnemy.CheckSkin();
                //for (int i = 0; i < petComponentServerEnemy.TeamPetList.Count; i++)
                //{
                //    PetInfo rolePetInfo = petComponentServerEnemy.GetPetInfo(petComponentServerEnemy.TeamPetList[i]);
                //    if (rolePetInfo == null)
                //    {
                //        continue;
                //    }
                //    if (unit.DomainScene().GetComponent<UnitComponent>().Get(rolePetInfo.Id)!=null)
                //    {
                //        Log.Debug($"宠物ID重复：{unit.Id}");
                //        continue;
                //    }
                    
                //    BagComponentServer bagComponentServer = await DBHelper.GetComponent<BagComponentServer>(UnitZoneHelper.GetHomeZone(self.EnemyId), self.EnemyId);
                //    NumericComponent numericComponent = await DBHelper.GetComponent<NumericComponent>(UnitZoneHelper.GetHomeZone(self.EnemyId), self.EnemyId);

                //    petComponentServerEnemy.UpdatePetAttributeWithData(bagComponentServer ,numericComponent, rolePetInfo, false);
                //    Unit petunit = UnitFactory.CreateTianTiPet(unit.DomainScene(), 0,
                //       CampEnum.CampPlayer_2, rolePetInfo, AIGetTargetHelp.Formation_2[i], 180f, i);

                //}
            }
            else
            {
                List<int> petlist = new List<int>() { 1000101, 1000201, 1000301 };
                for (int k = 0; k < petlist.Count; k++)
                {
                    PetInfo petInfo = petComponentServer.GenerateNewPet(petlist[0], 0);
                    petComponentServer.PetXiLian(petInfo,0, 2, 0, 0 );
                    petComponentServer.UpdatePetAttribute(petInfo, false);
                    petInfo.PlayerName = "机器人";
                    Unit petunit = UnitFactory.CreateTianTiPet(unit.DomainScene(), 0,
                       CampEnum.CampPlayer_2,  petInfo, AIGetTargetHelp.Formation_2[k], 180f, k);
                }
            }
        }

        public static void OnKillEvent(this PetTianTiComponent self)
        {
            int result = self.GetCombatResult();
            if (result != CombatResultEnum.None)
            {
                self.OnGameOver(result);
            }
        }

        public static async void OnGameOver(this PetTianTiComponent self, int result)
        {
            List<Unit> units = self.DomainScene().GetComponent<UnitComponent>().GetAll();
            for (int i = 0; i < units.Count; i++)
            {
                AIComponent aIComponent = units[i].GetComponent<AIComponent>();
                aIComponent?.Stop();
            }

            int rankid = await self.NoticeRankServer(result);

            M2C_FubenSettlement m2C_FubenSettlement = new M2C_FubenSettlement();
            m2C_FubenSettlement.BattleResult = result;
            if (result == CombatResultEnum.Win)
            {
                LDGlobalValue ldGlobalValue = LDGlobalValueCategory.Instance.Get(68);
                int dropId = int.Parse(ldGlobalValue.Value);
                List<RewardItem> rewardItems = new List<RewardItem>();
                DropHelper.DropIDToDropItem(dropId, rewardItems);
                DropHelper.ZhengLiRewardItems(rewardItems);
                m2C_FubenSettlement.ReardList.AddRange(rewardItems);
                m2C_FubenSettlement.StarInfos = new List<int> { 1, 1, 1 };

                DungeonSettlementHelper.SettlePetTianTiWin(
                    self.MainUnit, rewardItems, $"{ItemGetWay.PetTianTiReward}_{TimeHelper.ServerNow()}");
            }
            else
            {
                m2C_FubenSettlement.StarInfos = new List<int> { 0,0,0 };
            }

            if (self.MainUnit != null && !self.MainUnit.IsDisposed)
            {
                if (rankid > 0)
                {
                    self.MainUnit.GetComponent<ChengJiuComponentServer>().OnPetTianTiRank(rankid);
                    self.MainUnit.GetComponent<TaskComponentServer>().OnPetTianTiRank(rankid);
                }
                MessageHelper.SendToClient(self.MainUnit, m2C_FubenSettlement);
            }
        }

        /// <summary>
        /// 失败不通知
        /// </summary>
        /// <param name="self"></param>
        /// <param name=""></param>
        /// <returns></returns>
        public static async ETTask<int> NoticeRankServer(this PetTianTiComponent self, int result)
        {
            //获取传送map的 actorId
            long mapInstanceId = StartSceneConfigCategory.Instance.GetBySceneName(self.DomainZone(), Enum.GetName(SceneType.Rank)).InstanceId;

            Unit unit = self.MainUnit;
            RankPetInfo rankPetInfo = new RankPetInfo();
            RoleInfoComponentServer roleInfoComponentServer = unit.GetComponent<RoleInfoComponentServer>();
            rankPetInfo.UserId = roleInfoComponentServer.RoleInfo.UserId;
            rankPetInfo.PlayerName = roleInfoComponentServer.RoleInfo.Name;
            //rankPetInfo.PetUId = unit.GetComponent<PetComponentServer>().TeamPetList;
            //rankPetInfo.TeamName = rankPetInfo.PlayerName;
            //for (int i = 0; i < rankPetInfo.PetUId.Count; i++ )
            //{
            //    RolePetInfo rolePetInfo = unit.GetComponent<PetComponentServer>().GetPetInfo(rankPetInfo.PetUId[i]);
            //    rankPetInfo.PetConfigId.Add(rolePetInfo!=null ? rolePetInfo.ConfigId :0);
            //}
            R2M_PetRankUpdateResponse m2m_TrasferUnitResponse = (R2M_PetRankUpdateResponse)await ActorMessageSenderComponent.Instance.Call
                     (mapInstanceId, new M2R_PetRankUpdateRequest() {  RankPetInfo = rankPetInfo, Win = result, EnemyId = self.DomainScene().GetComponent<PetTianTiComponent>().EnemyId });

            return m2m_TrasferUnitResponse.SelfRank;
        }
        
        /// <summary>
        /// 1 成功 2失败
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static int GetCombatResult(this PetTianTiComponent self)
        {
            int number_self = 0;
            int number_enemy = 0;
            List<Unit> unitList = self.DomainScene().GetComponent<UnitComponent>().GetAll();
            for(int i = 0; i < unitList.Count; i++)
            {
                Unit unit = unitList[i];    
                if (unit.Type != UnitType.Pet || !unit.IsCanBeAttack())
                {
                    continue;
                }
                if (unit.GetBattleCamp() == CampEnum.CampPlayer_1)
                {
                    number_self++;
                }
                else
                {
                    number_enemy++;
                }
            }
            if (number_self > 0 && number_enemy > 0)
                return CombatResultEnum.None;
            if (number_self > 0 && number_enemy == 0)
                return CombatResultEnum.Win;
            return CombatResultEnum.Fail;
        }

    }
}

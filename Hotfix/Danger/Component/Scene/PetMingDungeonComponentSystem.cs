using System.Collections.Generic;

namespace ET
{
    public static class PetMingDungeonComponentSystem
    {

        public static async ETTask OnPetMingOccupy(this PetMingDungeonComponent self)
        {
            if (self.CombatResultEnum == CombatResultEnum.Win && self.MainUnit != null)
            {
                string logInfo = string.Empty;
                string unitName = self.MainUnit.GetComponent<RoleInfoComponentServer>().RoleInfo.Name;
                logInfo = $"玩家 {unitName} 队伍{self.TeamId + 1} 占领了第{self.Position+1}";
#if false // TODO: migrate to LD config
                MineBattleConfig mineBattleConfig = MineBattleConfigCategory.Instance.Get(self.MineType);
                logInfo = $"玩家 {unitName} 队伍{self.TeamId + 1} 占领了第{self.Position+1} {mineBattleConfig.Name}";
#endif

                LogHelper.PetMingBattleInfo(self.DomainZone(), logInfo);

                long chargeServerId = DBHelper.GetActivityServerId(self.DomainZone());
                A2M_PetMingBattleWinResponse r_GameStatusResponse = (A2M_PetMingBattleWinResponse)await ActorMessageSenderComponent.Instance.Call
                    (chargeServerId, new M2A_PetMingBattleWinRequest()
                    {
                        MingType = self.MineType,
                        Postion = self.Position,
                        UnitID = self.MainUnit.Id,
                        TeamId = self.TeamId,
                        WinPlayer = unitName,
                    });
            }
        }

        public static async ETTask OnGameOver(this PetMingDungeonComponent self, int result)
        {
            self.CombatResultEnum = result;

            self.OnPetMingOccupy().Coroutine();

            long cdTime = result == CombatResultEnum.Win ? TimeHelper.Hour : TimeHelper.Minute * 10;
            M2C_FubenSettlement m2C_FubenSettlement = new M2C_FubenSettlement();
            m2C_FubenSettlement.BattleResult = result;
            m2C_FubenSettlement.StarInfos = result == CombatResultEnum.Win ?  new List<int>() { 1, 1, 1 } : new List<int>() { 0,0,0};
            MessageHelper.SendToClient(self.MainUnit, m2C_FubenSettlement);
           
            await ETTask.CompletedTask;
        }

        /// <summary>
        /// 1 成功 2失败
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static int GetCombatResult(this PetMingDungeonComponent self)
        {
            int number_self = 0;
            int number_enemy = 0;
            List<Unit> unitList = self.DomainScene().GetComponent<UnitComponent>().GetAll();
            for (int i = 0; i < unitList.Count; i++)
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


        public static void OnKillEvent(this PetMingDungeonComponent self)
        {
            int result = self.GetCombatResult();
            if (result != CombatResultEnum.None)
            {
                self.OnGameOver(result).Coroutine();
            }
        }

        public static async ETTask GeneratePetFuben(this PetMingDungeonComponent self)
        {
            long chargeServerId = DBHelper.GetActivityServerId(self.DomainZone());
            A2M_PetMingPlayerInfoResponse r_GameStatusResponse = (A2M_PetMingPlayerInfoResponse)await ActorMessageSenderComponent.Instance.Call
                (chargeServerId, new M2A_PetMingPlayerInfoRequest()
                {
                    MingType = self.MineType, 
                    Postion = self.Position,
                });

            if (r_GameStatusResponse.Error != ErrorCode.ERR_Success)
            {
                return;
            }

            //己方队伍
            Unit unit = self.MainUnit;
            unit.GetComponent<StateComponent>().StateTypeAdd(StateTypeEnum.WuDi);
            PetComponentServer petComponentServer = unit.GetComponent<PetComponentServer>();
            petComponentServer.CheckSkin();
            //List<long> pets = petComponentServer.PetMingList;
            //for (int i = 0; i <  5; i++)
            //{
            //    long petinfoid = pets[i + self.TeamId * 5];
            //    PetInfo rolePetInfo = petComponentServer.GetPetInfo(petinfoid);
            //    if (rolePetInfo == null)
            //    {
            //        continue;
            //    }

            //    int position = petComponentServer.PetMingPosition.IndexOf(petinfoid);
            //    position = position != -1 ? position %= 9 : i;   

            //    Unit petunit = UnitFactory.CreateTianTiPet(unit.DomainScene(), unit.Id,
            //        CampEnum.CampPlayer_1, rolePetInfo, AIGetTargetHelp.Formation_1[ position ], 0f, position);
            //    petunit.GetComponent<AIComponent>().Stop();
            //}

            //敌方队伍
            if (r_GameStatusResponse.PetMingPlayerInfo == null)
            {
#if false // TODO: migrate to LD config
                MineBattleConfig mineBattleConfig = MineBattleConfigCategory.Instance.Get(self.MineType);
                int[] petdefendlist = mineBattleConfig.PetDefendInit;
                //初始配置

                for (int k = 0; k < petdefendlist.Length; k++)
                {
                    if (petdefendlist[k] == 0)
                    {
                        continue;
                    }

                    RolePetInfo petInfo = petComponentServer.GenerateNewPet(petdefendlist[k], 0);
                    petComponentServer.PetXiLian(petInfo,0, 2, 0, 0 );
                    petComponentServer.UpdatePetAttribute(petInfo, false);
                    petInfo.PlayerName = "机器人";
                    Unit petunit = UnitFactory.CreateTianTiPet(unit.DomainScene(), 0,
                       CampEnum.CampPlayer_2, petInfo, AIGetTargetHelp.Formation_2[k], 180f, k);
                }
#endif
            }
            else
            {
                long enemyId = r_GameStatusResponse.PetMingPlayerInfo.UnitId;
                int teamid = r_GameStatusResponse.PetMingPlayerInfo.TeamId;
                long dbCacheId = DBHelper.GetDbCacheId(self.DomainZone());

                //self.EnemyId = enemyId;

                PetComponentServer petComponentServerEnemy = await DBHelper.GetComponent<PetComponentServer>(UnitZoneHelper.GetHomeZone(enemyId), enemyId);
                if (petComponentServerEnemy != null)
                {
                    BagComponentServer bagComponentServer =  await DBHelper.GetComponent<BagComponentServer>(UnitZoneHelper.GetHomeZone(enemyId), enemyId);
                    if (bagComponentServer == null)
                    {
                        return;
                    }
                    
                    NumericComponent numericComponent =  await DBHelper.GetComponent<NumericComponent>(UnitZoneHelper.GetHomeZone(enemyId), enemyId);
                    if (numericComponent == null)
                    {
                        return;
                    }

                    
                    petComponentServerEnemy.CheckSkin();
                //    List<long> petsenemy = petComponentServerEnemy.PetMingList;
                //    for (int i = 0; i < 5; i++)
                //    {
                //        long petinfoid = petsenemy[i + teamid * 5];
                //        RolePetInfo rolePetInfo = petComponentServerEnemy.GetPetInfo(petinfoid);
                //        if (rolePetInfo == null)
                //        {
                //            continue;
                //        }
                //        if (unit.GetParent<UnitComponent>().Get(rolePetInfo.Id) != null)
                //        {
                //            Log.Debug($"宠物ID重复：{unit.Id}");
                //            continue;
                //        }

                //        int position = petComponentServerEnemy.PetMingPosition.IndexOf(petinfoid);
                //        position = position != -1 ? position %= 9 : i;
                //        petComponentServerEnemy.UpdatePetAttributeWithData(bagComponentServer, numericComponent, rolePetInfo, false);
                //        Unit petunit = UnitFactory.CreateTianTiPet(unit.DomainScene(), 0,
                //           CampEnum.CampPlayer_2, rolePetInfo, AIGetTargetHelp.Formation_2[position], 180f,position);
                //    }
               }

            }
        }
    }
}

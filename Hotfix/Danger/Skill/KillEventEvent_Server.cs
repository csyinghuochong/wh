using System;
using System.Collections.Generic;

namespace ET
{

    //击杀事件
    [Event]
    public class KillEventEvent_Server : AEvent<EventType.KillEvent>
    {
        private async ETTask OnRemoveUnit(EventType.KillEvent args, long waittime)
        {
            Unit unitDefend = args.UnitDefend;
            await TimerComponent.Instance.WaitAsync(waittime);
            if (unitDefend.IsDisposed)
            {
                return;
            }
            if (unitDefend.Type != UnitType.Player && args.WaitRevive == 0 && DllHelper.BattleCheck)
            {
                unitDefend.GetParent<UnitComponent>().Remove(unitDefend.Id);
            }
        }

        protected override void Run(EventType.KillEvent args)
        {
            Unit defendUnit = args.UnitDefend;
            Unit mainAttack = args.UnitAttack;

            bool selfDeath = defendUnit == mainAttack;
            if (selfDeath)
            {
                //自爆怪
                //if (defendUnit.ConfigId != 90000001 && defendUnit.ConfigId != 90000002 
                // && defendUnit.ConfigId != 90000005 && defendUnit.ConfigId != 72009001)
                //{
                //    Log.Warning($"找不到击杀方主人.defendUnit == mainAttack: {defendUnit.ConfigId}");
                //}
                OnRemoveUnit(args, 1).Coroutine();
                return;
            }

            if (mainAttack == null || mainAttack.IsDisposed)
            {
                //Log.Warning($"找不到击杀方主人.mainAttack == null ");
                OnRemoveUnit(args, 1).Coroutine();
                return;
            }
            int attackconfid = mainAttack.ConfigId;
            Scene domainScene = defendUnit.DomainScene();
            MapComponent mapComponent = domainScene.GetComponent<MapComponent>();
            int sceneId = mapComponent.SceneId;
            int sceneTypeEnum = mapComponent.MapTypeEnum;
            if (mainAttack.Type != UnitType.Player)
            {
                mainAttack = domainScene.GetComponent<UnitComponent>().Get(mainAttack.GetMasterId());
            }
            if ((mainAttack == null || mainAttack.IsDisposed) && defendUnit.Type == UnitType.Monster
                && defendUnit.ConfigId != 90000001 && defendUnit.ConfigId != 90000002 && defendUnit.ConfigId != 90000005)
            {
                if (sceneTypeEnum == MapTypeEnum.LocalDungeon)
                {
                    //Log.Warning($"找不到击杀方主人.LocalDungeon1： 防： {defendUnit.ConfigId}  攻： {attackconfid} ");
                    mainAttack = domainScene.GetComponent<LocalDungeonComponent>().MainUnit;
                }
                if (sceneTypeEnum == MapTypeEnum.TeamDungeon)
                {
                    //Log.Warning($"找不到击杀方主人.TeamDungeon：   防： {defendUnit.ConfigId}   攻：  {attackconfid}");
                }
            }

            if (mainAttack != null && !mainAttack.IsDisposed)
            {
                int realPlayer = 1;
                List<long> allAttackIds = new List<long>();
                UnitComponent unitComponent = domainScene.GetComponent<UnitComponent>();
                if (sceneTypeEnum == MapTypeEnum.TeamDungeon)
                {
                    List<Unit> units = UnitHelper.GetUnitList(domainScene, UnitType.Player);
                    for (int k = 0; k < units.Count; k++)
                    {
                        allAttackIds.Add(units[k].Id);
                    }
                    realPlayer = UnitHelper.GetRealPlayer(domainScene);
                }
                else
                {
                    allAttackIds = defendUnit.GetComponent<AttackRecordComponent>().GetBeAttackPlayerList();
                    bool hasMainAttack = false;
                    for (int k = 0; k < allAttackIds.Count; k++)
                    {
                        if (allAttackIds[k] == mainAttack.Id)
                        {
                            hasMainAttack = true;
                            break;
                        }
                    }
                    if (!hasMainAttack)
                    {
                        allAttackIds.Add(mainAttack.Id);
                    }
                }

                if (allAttackIds.Count >= 50)
                {
                    Console.WriteLine($"allAttackIds.Count : {allAttackIds.Count >= 50}  {TimeInfo.Instance.ToDateTime(TimeHelper.ServerNow()).ToString()}");
                }

                int attackCount = allAttackIds.Count > 20 ? 20 : allAttackIds.Count;
                for (int i = 0; i < attackCount; i++)
                {
                    Unit attackUnit = unitComponent.Get(allAttackIds[i]);
                    if (attackUnit == null || attackUnit.Type != UnitType.Player)
                    {
                        continue;
                    }
                    TaskComponentServer taskComponent = attackUnit.GetComponent<TaskComponentServer>();
                    ChengJiuComponentServer chengJiuComponent = attackUnit.GetComponent<ChengJiuComponentServer>();
                    PetComponentServer petComponent = attackUnit.GetComponent<PetComponentServer>();
                    RoleInfoComponentServer roleInfoComponent = attackUnit.GetComponent<RoleInfoComponentServer>();
                    taskComponent.OnKillUnit(defendUnit, sceneTypeEnum);
                    chengJiuComponent.OnKillUnit(defendUnit);
                    petComponent.OnKillUnit(defendUnit);
                    roleInfoComponent.OnKillUnit(defendUnit, sceneTypeEnum, sceneId);
                }

                UnitFactory.CreateDropItems(defendUnit, mainAttack, sceneTypeEnum, sceneId, realPlayer);

                if (mainAttack.Type == UnitType.Player)
                {
                    ChengJiuComponentServer mainChengJiu = mainAttack.GetComponent<ChengJiuComponentServer>();
                    int jinglingid = mainChengJiu.JingLingId;
                    if (jinglingid != 0)
                    {
                        LDElf ldElf = LDElfCategory.Instance.Get(jinglingid);
                    }
                }

                if (mainAttack.Type == UnitType.Player && defendUnit.Type == UnitType.Player
                 && SceneConfigHelper.UseSceneConfig(sceneTypeEnum))
                {
                    LDScene ldScene = LDSceneCategory.Instance.Get(sceneId);
                    string attackname = mainAttack.GetComponent<RoleInfoComponentServer>().RoleInfo.Name;
                    string defendname = defendUnit.GetComponent<RoleInfoComponentServer>().RoleInfo.Name;
                    string killtext = $"<color=#B6FF00>{attackname}</color> 在<color=#FFA313>{ldScene.Name}</color> 击败了 <color=#00F6E6>{defendname}</color>";
                    string killtextEn = $"<color=#B6FF00>{attackname}</color> 在<color=#FFA313>{ldScene.Name}</color> Defeated <color=#00F6E6>{defendname}</color>";
                    ServerMessageHelper.SendBroadMessage(defendUnit.DomainZone(), NoticeType.KillEvent, killtext, killtextEn);
                }
            }

            long waittime = defendUnit.IsChest() ? 1000 : 100;
            if (defendUnit.Type == UnitType.Monster)
            {
                LDMonster ldMonster = LDMonsterCategory.Instance.Get(defendUnit.ConfigId);
            }
            if (defendUnit.Type == UnitType.Pet)
            {
                waittime = 1000;
            }

            switch (sceneTypeEnum)
            {
                case MapTypeEnum.PetDungeon:
                    domainScene.GetComponent<PetFubenSceneComponent>().OnKillEvent();
                    break;
                case MapTypeEnum.PetTianTi:
                    domainScene.GetComponent<PetTianTiComponent>().OnKillEvent();
                    break;
                case MapTypeEnum.TeamDungeon:
                    domainScene.GetComponent<TeamDungeonComponent>().OnKillEvent(defendUnit);
                    break;
                case MapTypeEnum.PetMing:
                    domainScene.GetComponent<PetMingDungeonComponent>().OnKillEvent();
                    break;
                case MapTypeEnum.BaoZangZhiDi:
                    ;
                    break;
                case MapTypeEnum.MiJing:
                    domainScene.GetComponent<MiJingComponent>().OnKillEvent(defendUnit);
                    break;
                case MapTypeEnum.Solo:
                    domainScene.GetComponent<SoloDungeonComponent>().OnKillEvent(mainAttack,defendUnit);
                    break;
                case MapTypeEnum.TowerDungeon:
                    domainScene.GetComponent<TowerComponent>().OnKillEvent(defendUnit);
                    break;
                case MapTypeEnum.LocalDungeon:
                    domainScene.GetComponent<LocalDungeonComponent>().OnKillEvent(defendUnit, mainAttack);
                    break;
                case MapTypeEnum.Battle:
                    domainScene.GetComponent<BattleDungeonComponent>().OnKillEvent(defendUnit, mainAttack);
                    break;
                case MapTypeEnum.Arena:
                    domainScene.GetComponent<ArenaDungeonComponent>().OnKillEvent(defendUnit, mainAttack);
                    break;
                case MapTypeEnum.Union:
                    domainScene.GetParent<UnionSceneComponent>().OnKillEvent(domainScene, defendUnit);
                    break;
                case MapTypeEnum.TrialDungeon:
                    domainScene.GetComponent<TrialDungeonComponent>().OnKillEvent(defendUnit);
                    break;
                case MapTypeEnum.SeasonTower:
                    domainScene.GetComponent<SeasonTowerComponent>().OnKillEvent(defendUnit);
                    break;
                case MapTypeEnum.TowerOfSeal:
                    domainScene.GetComponent<TowerOfSealComponent>().OnKillEvent(defendUnit);
                    break;
                default:
                    break;
            }

            OnRemoveUnit(args, waittime).Coroutine();
        }
    }
}
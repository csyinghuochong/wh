using System.Collections.Generic;

namespace ET
{
    [ObjectSystem]
    public class UnitLifeComponentAwakeSystem : AwakeSystem<UnitLifeComponent>
    {
        public override void Awake(UnitLifeComponent self)
        {
        }
    }

    public static class UnitLifeComponentSystem
    {
        public static void OnKillZhaoHuan(this UnitLifeComponent self, Unit attack)
        {
            Unit unit = self.GetParent<Unit>();
            UnitInfoComponent unitInfoComponent = unit.GetComponent<UnitInfoComponent>();
            if (unitInfoComponent == null)
            {
                Log.Debug($"unitInfoComponent == null  {unit.Type} {unit.IsDisposed}");
                return;
            }

            for (int i = unitInfoComponent.ZhaohuanIds.Count - 1; i >= 0; i--)
            {
                Unit zhaohuan = unit.GetParent<UnitComponent>().Get(unitInfoComponent.ZhaohuanIds[i]);
                if (zhaohuan == null)
                {
                    continue;
                }

                UnitLifeComponent summonLife = zhaohuan.GetComponent<UnitLifeComponent>();
                if (summonLife != null)
                {
                    summonLife.OnDead(attack != null ? attack : zhaohuan);
                }
            }

            unitInfoComponent.ZhaohuanIds.Clear();
        }

        public static void PlayDeathSkill(this UnitLifeComponent self, Unit attack)
        {
            Unit unit = self.GetParent<Unit>();
            if (unit.Type != UnitType.Monster)
            {
                return;
            }

            if (unit.ConfigId == 90000202)
            {
                Log.Warning("PlayDeathSkill: 72009045");
            }

            LDMonster ldMonster = LDMonsterCategory.Instance.Get(unit.ConfigId);
        }

        public static void OnRevive(this UnitLifeComponent self, bool bornPostion = false)
        {
            Unit unit = self.GetParent<Unit>();
            NumericComponent numericComponent = unit.GetComponent<NumericComponent>();
            long maxHp = numericComponent.GetAsLong(NumericType.HP_Max_10);

            numericComponent.ApplyValue(NumericType.Now_Dead, 0);
            numericComponent.SetValueNoSync(NumericType.HP_Current_8, 0);
            numericComponent.ApplyChange(null, NumericType.HP_Current_8, maxHp, 0);
            numericComponent.ApplyValue(NumericType.ReviveTime, 0);
            unit.GetComponent<SkillPassiveComponent>()?.Activeted();
            unit.GetComponent<BuffManagerComponent>()?.OnRevive();
            unit.Position = unit.GetBornPostion();
            if (unit.Type == UnitType.Monster)
            {
                unit.GetComponent<AIComponent>().Begin();
            }
        }

        public static void OnDead(this UnitLifeComponent self, Unit attack)
        {
            Unit unit = self.GetParent<Unit>();
            unit.GetComponent<MoveComponent>()?.Stop();
            unit.GetComponent<AIComponent>()?.Stop();
            unit.GetComponent<SkillPassiveComponent>()?.Stop();
            unit.GetComponent<SkillManagerComponent>()?.OnFinish(false);
            unit.GetComponent<BuffManagerComponent>()?.OnDead(attack);
            NumericComponent numericComponent = unit.GetComponent<NumericComponent>();

            if (unit.Type == UnitType.Player)
            {
                RolePetInfo rolePetInfo = unit.GetComponent<PetComponentServer>().GetFightPet();
                if (rolePetInfo != null)
                {
                    unit.GetParent<UnitComponent>().Remove(rolePetInfo.Id);
                    unit.GetComponent<PetComponentServer>().OnPetDead(rolePetInfo.Id);
                }

                int nowHorse = numericComponent.GetAsInt(NumericType.HorseRide);
                if (nowHorse > 0)
                {
                    numericComponent.ApplyValue(NumericType.HorseRide, 0);
                }
            }

            if (unit.Type == UnitType.Player && attack != null && attack.Type == UnitType.Monster)
            {

                List<Unit> units = UnitHelper.GetUnitList(unit.DomainScene(), UnitType.Monster);
                for (int i = 0; i < units.Count; i++)
                {
                    units[i].GetComponent<AttackRecordComponent>()?.OnRemoveAttackByUnit(unit.Id);
                }
            }

            if (unit.Type == UnitType.Pet)
            {
                int sceneTypeEnum = unit.DomainScene().GetComponent<MapComponent>().MapTypeEnum;
                if (sceneTypeEnum != (int)MapTypeEnum.PetTianTi
                    && sceneTypeEnum != (int)MapTypeEnum.PetDungeon
                    && sceneTypeEnum != (int)MapTypeEnum.PetMing)
                {
                    long masterId = numericComponent.GetAsLong(NumericType.MasterId);
                    Unit master = unit.GetParent<UnitComponent>().Get(masterId);
                    master?.GetComponent<PetComponentServer>().OnPetDead(unit.Id);
                }
            }

            if (unit.Type == UnitType.Monster)
            {
                List<Unit> units = UnitHelper.GetUnitList(unit.DomainScene(), UnitType.Player);
                for (int i = 0; i < units.Count; i++)
                {
                    units[i].GetComponent<BuffManagerComponent>().OnDeadRemoveBuffBy(unit.Id);
                }
            }

            int waitRevive = self.OnWaitRevive();
            numericComponent.ApplyValue(NumericType.Now_Dead, 1);
            Game.EventSystem.Publish(new EventType.KillEvent()
            {
                WaitRevive = waitRevive,
                UnitAttack = attack,
                UnitDefend = unit,
            });
        }

        /// <summary>0 不复活 1 等待复活</summary>
        public static int OnWaitRevive(this UnitLifeComponent self)
        {
            Unit unit = self.GetParent<Unit>();
            if (unit.Type != UnitType.Monster)
            {
                return 0;
            }

            LDMonster ldMonster = LDMonsterCategory.Instance.Get(unit.ConfigId);
            int resurrection = 0;
            MapComponent mapComponent = unit.DomainScene().GetComponent<MapComponent>();
            if (SeasonHelper.SeasonBossId == unit.ConfigId && mapComponent.MapTypeEnum == (int)MapTypeEnum.LocalDungeon)
            {
                LocalDungeonComponent localDungeon = unit.DomainScene().GetComponent<LocalDungeonComponent>();
                RoleInfoComponentServer roleInfoComponentServer = localDungeon.MainUnit.GetComponent<RoleInfoComponentServer>();
                localDungeon.MainUnit.GetComponent<NumericComponent>().ApplyValue(
                    NumericType.SeasonBossFuben,
                    SeasonHelper.GetFubenId(roleInfoComponentServer.RoleInfo.Lv));
                localDungeon.MainUnit.GetComponent<NumericComponent>().ApplyValue(
                    NumericType.SeasonBossRefreshTime,
                    TimeHelper.ServerNow() + resurrection * 1000);
                resurrection = 0;
            }

            if (resurrection == 0)
            {
                return 0;
            }

            if (unit.MasterId > 0)
            {
                return 0;
            }

            return 0;
        }
    }
}

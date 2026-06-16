using System;
using System.Collections.Generic;
using UnityEngine;

namespace ET
{
  
    [ObjectSystem]
    public class LocalDungeonComponentAwakeSystem : AwakeSystem<LocalDungeonComponent>
    {
        public override void Awake(LocalDungeonComponent self)
        {
            self.RandomJingLing = 0;
            self.RandomMonster = 0;
        }
    }

    [ObjectSystem]
    public class LocalDungeonComponentDestroySystem : DestroySystem<LocalDungeonComponent>
    {
        public override void Destroy(LocalDungeonComponent self)
        {

        }
    }

    public static class LocalDungeonComponentSystem
    {

        public static void OnKillEvent(this LocalDungeonComponent self, Unit unit, Unit attack)
        {
            if (attack == null || attack.Type != UnitType.Player)
            {
                return;
            }
            if (unit.Type != UnitType.Monster)
            {
                return;
            }

            LDMonster ldMonster = LDMonsterCategory.Instance.Get(unit.ConfigId);
            UserInfoComponent userInfoComponent = self.MainUnit.GetComponent<UserInfoComponent>();
            if (userInfoComponent == null || userInfoComponent.IsDisposed)
            {
                return;
            }
            if (ldMonster.Type == (int)MonsterTypeEnum.Boss && userInfoComponent.UserInfo.Lv >= 20)
            {
                userInfoComponent.UpdateRoleData(UserDataType.BaoShiDu, "-1", true);
                return;
            }


            ///刷新刷出神秘之门
            if (userInfoComponent.UserInfo.PiLao > 0 && userInfoComponent.UserInfo.Lv > 20  && !unit.IsSceneItem() && RandomHelper.RandFloat01() < 0.001f)
            {
                int shenminId = 40000003;
                List<Unit> npclist = self.MainUnit.GetParent<UnitComponent>().GetAll();
                for (int i = 0; i < npclist.Count; i++)
                {
                    if (npclist[i].Type == UnitType.Npc && npclist[i].ConfigId == shenminId)
                    {
                        shenminId = 0;
                    }
                }
                if (shenminId != 0)
                {
                    UnitFactory.CreateNpcByPosition(self.DomainScene(), shenminId, unit.Position);
                }
                Log.Warning($"神秘之门刷新: {self.DomainZone()} {self.MainUnit.Id}");
            }
        }

        public static void OnCleanBossCD(this LocalDungeonComponent self)
        {
            List<Unit> entities = self.DomainScene().GetComponent<UnitComponent>().GetAll();
            for (int i = 0; i < entities.Count; i++)
            {
                Unit entity = entities[i];
                if (entity.Type != UnitType.Monster)
                {
                    continue;
                }
                if (entity.GetComponent<NumericComponent>().GetAsInt(NumericType.Now_Dead)  == 1)
                {
                    entity.GetComponent<HeroDataComponent>().OnRevive();
                }
            }
        }
        

    }
}

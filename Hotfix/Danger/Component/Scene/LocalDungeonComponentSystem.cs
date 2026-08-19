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
            RoleInfoComponentServer roleInfoComponentServer = self.MainUnit.GetComponent<RoleInfoComponentServer>();
            if (roleInfoComponentServer == null || roleInfoComponentServer.IsDisposed)
            {
                return;
            }
           
            ///刷新刷出神秘之门
            if (roleInfoComponentServer.RoleInfo.TiLi > 0 && roleInfoComponentServer.RoleInfo.Lv > 20  && !unit.IsSceneItem() && RandomHelper.RandFloat01() < 0.001f)
            {
                int shenminId = 40000003;
                List<Unit> npclist = self.MainUnit.GetParent<UnitComponent>().GetAll();
                bool hasShenminNpc = false;
                for (int i = 0; i < npclist.Count; i++)
                {
                    if (npclist[i].Type == UnitType.Npc && npclist[i].ConfigId == shenminId)
                    {
                        hasShenminNpc = true;
                        break;
                    }
                }
                if (!hasShenminNpc)
                {
                    UnitFactory.CreateNpcByPosition(self.DomainScene(), shenminId, unit.Position);
                }
                Log.Warning($"神秘之门刷新: {self.DomainZone()} {self.MainUnit.Id}");
            }
        }

        public static void OnCleanBossCD(this LocalDungeonComponent self)
        {
            UnitComponent unitComponent = self.DomainScene().GetComponent<UnitComponent>();
            List<Unit> entities = unitComponent.GetAll();
            for (int i = 0; i < entities.Count; i++)
            {
                Unit entity = entities[i];
                if (entity.Type != UnitType.Monster)
                {
                    continue;
                }
                NumericComponent numericComponent = entity.GetComponent<NumericComponent>();
                UnitLifeComponent unitLifeComponent = entity.GetComponent<UnitLifeComponent>();
                if (numericComponent.GetAsInt(NumericType.Now_Dead) == 1)
                {
                    unitLifeComponent?.OnRevive();
                }
            }
        }
        

    }
}

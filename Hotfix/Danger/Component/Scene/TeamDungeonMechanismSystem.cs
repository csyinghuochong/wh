using System.Collections.Generic;

namespace ET
{
    /// <summary>只监听组队副本怪物的战斗、死亡变化；尸体移除不清除击杀记录。</summary>
    [NumericWatcher(NumericType.BossInCombat)]
    [NumericWatcher(NumericType.Now_Dead)]
    public class NumericWatcher_TeamDungeonMechanism : INumericWatcher
    {
        public void Run(EventType.NumericChangeEvent args)
        {
            if (args.OldValue == args.NewValue || args.Defend.Type != UnitType.Monster)
            {
                return;
            }

            TeamDungeonComponent dungeon = args.Defend.DomainScene().GetComponent<TeamDungeonComponent>();
            if (dungeon == null || dungeon.Mechanisms.Count == 0)
            {
                return;
            }

            dungeon.OnMechanismMonsterChanged(args.Defend);
        }
    }

    public static partial class TeamDungeonComponentSystem
    {
        /// <summary>刷怪完成后初始化当前场景光墙与怪物组，按默认值或配置条件生成首次快照。</summary>
        public static void InitMechanisms(this TeamDungeonComponent self)
        {
            self.Mechanisms.Clear();
            self.VisibleMechanismIds.Clear();
            self.MechanismGroups.Clear();
            self.DeadSceneCreatures.Clear();
            int sceneId = self.DomainScene().GetComponent<MapComponent>().SceneId;
            foreach (LDScene_Mechanism config in LDScene_MechanismCategory.Instance.GetAll().Values)
            {
                if (config.Scene_Id == sceneId && config.Type == 1)
                {
                    self.Mechanisms.Add(config);
                }
            }

            List<int> creatureIds = LDScene_CreatureCategory.Instance.GetSceneCreatureList(sceneId);
            if (creatureIds != null)
            {
                foreach (int id in creatureIds)
                {
                    LDScene_Creature creature = LDScene_CreatureCategory.Instance.Get(id);
                    if (creature.Type != 2 || creature.Group_Id <= 0)
                    {
                        continue;
                    }

                    if (!self.MechanismGroups.TryGetValue(creature.Group_Id, out HashSet<int> group))
                    {
                        group = new HashSet<int>();
                        self.MechanismGroups.Add(creature.Group_Id, group);
                    }
                    group.Add(id);
                }
            }

            foreach (Unit unit in self.DomainScene().GetComponent<UnitComponent>().GetAll())
            {
                if (unit.Type == UnitType.Monster && unit.GetComponent<NumericComponent>().GetAsInt(NumericType.Now_Dead) == 1)
                {
                    self.DeadSceneCreatures.Add(unit.GetComponent<UnitInfoComponent>().Scene_Creature);
                }
            }
            self.RefreshMechanisms(false);
        }

        /// <summary>死亡、复活、刷出或战斗数值变化后推进条件；尸体消失后仍保留已击杀状态。</summary>
        public static void OnMechanismMonsterChanged(this TeamDungeonComponent self, Unit unit)
        {
            if (self.Mechanisms.Count == 0)
            {
                return;
            }

            bool groupStateChanged = false;
            int creatureId = unit.GetComponent<UnitInfoComponent>().Scene_Creature;
            if (creatureId > 0)
            {
                if (unit.GetComponent<NumericComponent>().GetAsInt(NumericType.Now_Dead) == 1)
                {
                    groupStateChanged = self.DeadSceneCreatures.Add(creatureId);
                }
                else
                {
                    groupStateChanged = self.DeadSceneCreatures.Remove(creatureId);
                }
            }
            self.RefreshMechanisms(true, unit, groupStateChanged);
        }

        /// <summary>首次计算全部光墙，之后只重算关联怪物或击杀组的光墙；条件仍按 OR 判断，显隐变化才推送。</summary>
        private static void RefreshMechanisms(this TeamDungeonComponent self, bool broadcast, Unit changedMonster = null, bool groupStateChanged = false)
        {
            bool changed = false;
            foreach (LDScene_Mechanism config in self.Mechanisms)
            {
                if (changedMonster != null
                    && !self.IsMechanismConditionAffected(config.Display_Request1, config.Display_Request_Param1, changedMonster, groupStateChanged)
                    && !self.IsMechanismConditionAffected(config.Display_Request2, config.Display_Request_Param2, changedMonster, groupStateChanged))
                {
                    continue;
                }

                bool visible = config.Display_Request1 == 0 && config.Display_Request2 == 0
                    ? config.Default_Display == 1
                    : self.CheckMechanismCondition(config.Display_Request1, config.Display_Request_Param1)
                        || self.CheckMechanismCondition(config.Display_Request2, config.Display_Request_Param2);
                changed |= visible ? self.VisibleMechanismIds.Add(config.Id) : self.VisibleMechanismIds.Remove(config.Id);
            }

            if (!changed || !broadcast)
            {
                return;
            }

            M2C_TeamDungeonMechanismUpdate message = self.CreateMechanismSnapshot();
            foreach (Unit player in self.DomainScene().GetComponent<UnitComponent>().GetAll())
            {
                if (player.Type == UnitType.Player)
                {
                    MessageHelper.SendToClient(player, message);
                }
            }
        }

        /// <summary>战斗条件只关心同配置怪物；击杀条件只在该组成员的死亡记录实际改变时重算。</summary>
        private static bool IsMechanismConditionAffected(this TeamDungeonComponent self, int request, int parameter, Unit monster, bool groupStateChanged)
        {
            if (parameter <= 0)
            {
                return false;
            }
            if (request == 1 || request == 2)
            {
                return monster.ConfigId == parameter;
            }
            if (!groupStateChanged || (request != 3 && request != 4))
            {
                return false;
            }

            return self.MechanismGroups.TryGetValue(parameter, out HashSet<int> group)
                && group.Contains(monster.GetComponent<UnitInfoComponent>().Scene_Creature);
        }

        /// <summary>战斗参数为 Monster.Id；击杀参数为当前场景 Group_Id，整组全部死亡才完成。</summary>
        private static bool CheckMechanismCondition(this TeamDungeonComponent self, int request, int parameter)
        {
            if (request == 0)
            {
                return false;
            }
            if (parameter <= 0)
            {
                return false;
            }

            if (request == 3 || request == 4)
            {
                if (!self.MechanismGroups.TryGetValue(parameter, out HashSet<int> group))
                {
                    return false;
                }
                bool allDead = group.IsSubsetOf(self.DeadSceneCreatures);
                return request == 3 ? allDead : !allDead;
            }
            if (request != 1 && request != 2)
            {
                return false;
            }

            bool exists = false;
            bool inCombat = false;
            foreach (Unit monster in self.DomainScene().GetComponent<UnitComponent>().GetAll())
            {
                if (monster.Type != UnitType.Monster || monster.ConfigId != parameter)
                {
                    continue;
                }
                exists = true;
                NumericComponent numeric = monster.GetComponent<NumericComponent>();
                if (numeric.GetAsInt(NumericType.Now_Dead) != 1 && numeric.GetAsInt(NumericType.BossInCombat) == 1)
                {
                    inCombat = true;
                    break;
                }
            }

            // 尸体移除后仍能识别本场景已配置的怪物；缺失配置不当作“离开战斗”。
            if (!exists)
            {
                int sceneId = self.DomainScene().GetComponent<MapComponent>().SceneId;
                List<int> creatureIds = LDScene_CreatureCategory.Instance.GetSceneCreatureList(sceneId);
                if (creatureIds != null)
                {
                    foreach (int id in creatureIds)
                    {
                        LDScene_Creature creature = LDScene_CreatureCategory.Instance.Get(id);
                        if (creature.Type == 2 && creature.Match_Id == parameter)
                        {
                            exists = true;
                            break;
                        }
                    }
                }
            }
            return exists && (request == 1 ? inCombat : !inCombat);
        }

        /// <summary>入场、回退及状态推送共用全量快照，实例标识与队伍后续申请互不影响。</summary>
        public static M2C_TeamDungeonMechanismUpdate CreateMechanismSnapshot(this TeamDungeonComponent self)
        {
            M2C_TeamDungeonMechanismUpdate message = new M2C_TeamDungeonMechanismUpdate
            {
                FubenInstanceId = self.DomainScene().InstanceId,
            };
            message.VisibleMechanismIds.AddRange(self.VisibleMechanismIds);
            return message;
        }
    }
}

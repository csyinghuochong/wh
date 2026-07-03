using System;
using System.Collections.Generic;
using UnityEngine;

namespace ET
{
    public static class UnitFactory
    {
        
        public static async ETTask<Unit> LoadUnit(Player player, Scene scene, CreateRoleInfo createRoleInfo,string account,  long accountId)
        {
            Unit unit = await DBHelper.GetUnitCache(scene, createRoleInfo.UserID);

            bool isNewUnit = unit == null;

            // if (isNewUnit)
            // {
            //     unit = await UnitFactory.Create(scene, player.UnitId, UnitType.Player,createRoleInfo,account, accountId);
            //
            //     UnitCacheHelper.AddOrUpdateUnitAllCache(unit);
            // }

            await CreatePlayer(scene, unit, createRoleInfo,account, accountId);

            //UnitCacheHelper.AddOrUpdateUnitAllCache(unit);

            return unit;
        }
        
        
        public static void AddDataComponent<K>(this Unit self) where K : Entity, IAwake, new()
        {
            if (self.GetComponent<K>() == null)
            {
                self.AddComponent<K>();
            }
        }

        public static async ETTask AddDBComonent<K>(this Unit self, long id) where K : Entity,  new()
        {
            Entity dbEntity = await DBHelper.GetComponent<K>(self.DomainZone(), self.Id);
            if (dbEntity == null)
            {
                Type type = typeof (K);
            
                Entity component = Activator.CreateInstance(type) as Entity;
                component.Id = id;

                DBHelper.SaveComponent(self.DomainZone(), id, component).Coroutine();
            }
        }

        public static async ETTask CreatePlayer(Scene scene, Unit unit, CreateRoleInfo createRoleInfo,string account,  long accountId)
        {
            await ETTask.CompletedTask;
            unit.AddComponent<MoveComponent>();
            unit.Type = UnitType.Player;
            unit.Position = new Vector3(-10, 0, -10);
            unit.AddComponent<UnitInfoComponent>();
            
            if (unit.GetComponent<RoleInfoComponentServer>() == null)
            {
                RoleInfoComponentServer roleInfoComponentServerServer = unit.AddComponent<RoleInfoComponentServer>();
                roleInfoComponentServerServer.OnInit(account, unit.Id, accountId, createRoleInfo);
            }

            if (unit.GetComponent<NumericComponent>() == null)
            {
                NumericComponent numericComponentServer = unit.AddComponent<NumericComponent>();
                numericComponentServer.ApplyValue(NumericType.AOI, 15000, false); // 视野15米
            }
            if (unit.GetComponent<BagComponentServer>() == null)
            {
                BagComponentServer bagComponentServer = unit.AddComponent<BagComponentServer>();
                bagComponentServer.OnInit(createRoleInfo); // 视野15米
            }

            unit.AddDataComponent<TaskComponentServer>();
            unit.AddDataComponent<ChengJiuComponentServer>();
            unit.AddDataComponent<PetComponentServer>();
            unit.AddDataComponent<SkillSetComponentServer>();
            unit.AddDataComponent<ActivityComponentServer>();
            unit.AddDataComponent<RechargeComponentServer>();
            unit.AddDataComponent<ReddotComponentServer>();
            unit.AddDataComponent<TitleComponentServer>();
            unit.AddDataComponent<JiaYuanComponentServer>();
            unit.AddDataComponent<ShoujiComponentServer>();
            unit.AddDataComponent<DataCollationComponent>();
            
            await unit.AddDBComonent<DBFriendInfo>(unit.Id);
            await unit.AddDBComonent<DBMailInfo>(unit.Id);
            
            //NumericComponent numericComponent = unit.AddComponent<NumericComponent>();
            //numericComponent.Set((int)NumericType.Numeric_Error, 6f); // 速度是6米每秒
            //numericComponent.Set(NumericType.Numeric_Error, 15000); // 视野15米
            //unitComponent.Add(unit);
            //// 加入aoi
            //unit.AddComponent<AOIEntity, int, Vector3>(9 * 1000, unit.Position);
        }

        //创建一个子弹unit（旧接口，skillId 为 LDSkill.Id）
        public static Unit CreateBullet(Scene scene, long masterid, int skillid, int starangle, Vector3 vector3, CreateMonsterInfo createMonsterInfo)
        {
            Unit unit = scene.GetComponent<UnitComponent>().AddChildWithId<Unit, int>(IdGenerater.Instance.GenerateId(), skillid);  //创建一个Unit
            scene.GetComponent<UnitComponent>().Add(unit);
            unit.AddComponent<ObjectWait>();
            
            unit.AddComponent<MoveComponent>();
            unit.AddComponent<PathfindingComponent, int>(scene.GetComponent<MapComponent>().NavMeshId);
            unit.AddComponent<UnitInfoComponent>();
            NumericComponent numericComponent = unit.AddComponent<NumericComponent>();
            unit.ConfigId = skillid;
            unit.Position = vector3;
            unit.Type = UnitType.Bullet;            //子弹Unity,根据这个类型会实例化出特效
            LDSkill ldSkill = LDSkillCategory.Instance.Get(skillid);
            numericComponent.Set(NumericType.Speed_Current_15, 1, false);
            numericComponent.Set(NumericType.MasterId, masterid, false);
            numericComponent.Set(NumericType.StartAngle, starangle, false);
            numericComponent.Set(NumericType.StartTime, TimeHelper.ServerNow(), false);
            unit.AddComponent<AOIEntity, int, Vector3>(9 * 1000, unit.Position);        //添加视野
            return unit;
        }

        /// <summary>创建技能体，ConfigId 为 LDSummon.Id。</summary>
        public static Unit CreateSkillEntity(Scene scene, long masterId, int summonId, Vector3 position, Quaternion rotation)
        {
            if (!LDSummonCategory.Instance.Contain(summonId))
            {
                Log.Error($"LDSummon 配置不存在: {summonId}");
                return null;
            }

            LDSummon summonConfig = LDSummonCategory.Instance.Get(summonId);
            Unit unit = scene.GetComponent<UnitComponent>().AddChildWithId<Unit, int>(IdGenerater.Instance.GenerateId(), summonId);
            scene.GetComponent<UnitComponent>().Add(unit);
            unit.AddComponent<ObjectWait>();
            unit.AddComponent<MoveComponent>();
            unit.AddComponent<PathfindingComponent, int>(scene.GetComponent<MapComponent>().NavMeshId);
            unit.AddComponent<UnitInfoComponent>();

            NumericComponent numericComponent = unit.AddComponent<NumericComponent>();
            unit.ConfigId = summonId;
            unit.Position = position;
            unit.Rotation = rotation;
            unit.Type = UnitType.SkillEntity;
            unit.MasterId = masterId;

            float speed = summonConfig.Speed > 0 ? summonConfig.Speed : 1f;
            numericComponent.Set(NumericType.Speed_Current_15, speed, false);
            numericComponent.Set(NumericType.MasterId, masterId, false);
            numericComponent.Set(NumericType.StartAngle, (int)rotation.eulerAngles.y, false);
            numericComponent.Set(NumericType.StartTime, TimeHelper.ServerNow(), false);
            unit.AddComponent<AOIEntity, int, Vector3>(9 * 1000, unit.Position);
            return unit;
        }

        public static Unit CreateMonster(Scene scene,  int monsterID, Vector3 vector3, CreateMonsterInfo createMonsterInfo)
        {
            int openDay = ServerHelper.GetOpenServerDay( false, scene.DomainZone()) ;
         
            //精灵不能作为主人
            Unit master = scene.GetComponent<UnitComponent>().Get(createMonsterInfo.MasterID);
            if (master != null && master.Type == UnitType.JingLing)
            {
                createMonsterInfo.MasterID = master.MasterId;
            }

            LDMonster ldMonster = LDMonsterCategory.Instance.Get(monsterID);
            MapComponent mapComponent = scene.GetComponent<MapComponent>();

            long unitid = createMonsterInfo.UnitId > 0 ? createMonsterInfo.UnitId : IdGenerater.Instance.GenerateId();
            Unit unit = scene.GetComponent<UnitComponent>().AddChildWithId<Unit, int>(unitid, 1001);
            unit.AddComponent<AttackRecordComponent>();
            NumericComponent numericComponent = unit.AddComponent<NumericComponent>();
            HeroDataComponent heroDataComponent = unit.AddComponent<HeroDataComponent>();
            UnitInfoComponent unitInfoComponent = unit.AddComponent<UnitInfoComponent>();
            unitInfoComponent.EnergySkillId = createMonsterInfo.SkillId;
            unitInfoComponent.UnitName = ldMonster.Name.ToString();
            unitInfoComponent.Scene_Creature = createMonsterInfo.SceneCreateId;
            unit.Type = UnitType.Monster;
            unit.Position = vector3;
            unit.ConfigId = ldMonster.Id;
            unit.Rotation = Quaternion.Euler(0, createMonsterInfo.Rotation, 0);
      
            //出生点
            //numericComponent.Set((int)NumericType.Born_X, unit.Position.x, false);
            //numericComponent.Set((int)NumericType.Born_Y, unit.Position.y, false);
            //numericComponent.Set((int)NumericType.Born_Z, unit.Position.z, false);
            unit.SetBornPosition(unit.Position, false);

            unit.MasterId = createMonsterInfo.MasterID;

            long revetime = 0;
            Unit mainUnit = null;
            if (mapComponent.MapTypeEnum == MapTypeEnum.LocalDungeon)
            {
                mainUnit = scene.GetComponent<LocalDungeonComponent>().MainUnit;
                //revetime = mainUnit.GetComponent<RoleInfoComponentServer>().GetReviveTime(ldMonster.Id);
            }
            if (unit.MasterId > 0)
            {
                revetime = 0;
            }

            if (mainUnit != null && TimeHelper.ServerNow() < revetime)
            {
                unit.AddComponent<ReviveTimeComponent, long>(revetime);
                numericComponent.ApplyValue(NumericType.ReviveTime, revetime, false);
                numericComponent.ApplyValue(NumericType.Now_Dead, 1, false);
            }
            heroDataComponent.InitMonsterInfo(ldMonster, createMonsterInfo);

            numericComponent.Set(NumericType.BattleCamp, createMonsterInfo.Camp, false);
            numericComponent.Set(NumericType.TeamId, master != null ? master.GetTeamId() : 0, false);
            numericComponent.Set(NumericType.AttackMode, master!=null ?  master.GetAttackMode() : 0, false);
            numericComponent.Set(NumericType.UnionId_0, master != null ? master.GetUnionId() : 0, false);
            numericComponent.Set(NumericType.PetSkin, createMonsterInfo.SkinId, false);
            numericComponent.Set(NumericType.MasterId, createMonsterInfo.MasterID, false);
            numericComponent.Set(NumericType.HP_Current_8, numericComponent.GetAsLong(NumericType.HP_Max_10), false);

            int ai = createMonsterInfo.AI > 0 ? createMonsterInfo.AI : 1;
            unit.AI = ai;
            unit.AddComponent<ObjectWait>();
            unit.AddComponent<MoveComponent>();
            unit.AddComponent<SkillManagerComponent>();
            unit.AddComponent<SkillPassiveComponent>();
            unit.AddComponent<PathfindingComponent, int>(scene.GetComponent<MapComponent>().NavMeshId);
            //添加其他组件
            unit.AddComponent<StateComponent>();         //添加状态组件
            unit.AddComponent<BuffManagerComponent>();      //添加Buff管理器
            unit.GetComponent<SkillPassiveComponent>().UpdateMonsterPassiveSkill();
            unit.GetComponent<SkillPassiveComponent>().Activeted();
        
            AIComponent aIComponent = unit.AddComponent<AIComponent, int>(ai);
            switch (mapComponent.MapTypeEnum)
            {
                case MapTypeEnum.LocalDungeon:
                    aIComponent.LocalDungeonUnit = mainUnit;
                  
                    //aIComponent.InitMonster(ldMonster.Id);
                    break;
                case MapTypeEnum.PetDungeon:
                    //aIComponent.InitPetFubenMonster(ldMonster.Id);
                    break;
                default:
                    aIComponent.InitMonster(ldMonster.Id);
                    aIComponent.Begin();
                    break;
            }

            scene.GetComponent<UnitComponent>().Add(unit);
            unit.AddComponent<AOIEntity, int, Vector3>(5 * 1000, unit.Position);
            return unit;
        }

        public static Unit CreateNpc(Scene scene, int createid,  int npcId, Vector3 vector3)
        {
            LDNPC ldNpc = LDNPCCategory.Instance.Get(npcId);

            Unit unit = scene.GetComponent<UnitComponent>().AddChildWithId<Unit, int>(IdGenerater.Instance.GenerateId(), npcId);
            scene.GetComponent<UnitComponent>().Add(unit);

            unit.AddComponent<UnitInfoComponent>().Scene_Creature = createid;
            unit.ConfigId = npcId;
            unit.Position = vector3;
            //unit.Position = new Vector3(npcConfig.Position[0] * 0.01f, npcConfig.Position[1] * 0.01f, npcConfig.Position[2] * 0.01f);
            unit.Rotation = Quaternion.Euler(0, 0, 0);
            unit.Type = UnitType.Npc;
           
            unit.AddComponent<AOIEntity, int, Vector3>(9 * 1000, unit.Position);
            return unit;
        }

        public static Unit CreateNpcByPosition(Scene scene, int npcId, Vector3 vector3)
        {
            LDNPC ldNpc = LDNPCCategory.Instance.Get(npcId);

            Unit unit = scene.GetComponent<UnitComponent>().AddChildWithId<Unit, int>(IdGenerater.Instance.GenerateId(), npcId);
            scene.GetComponent<UnitComponent>().Add(unit);

            unit.AddComponent<UnitInfoComponent>();
            unit.ConfigId = npcId;
            unit.Position = vector3;
            unit.Rotation = Quaternion.Euler(0, 0, 0);
            unit.Type = UnitType.Npc;
            
            unit.AddComponent<AOIEntity, int, Vector3>(9 * 1000, unit.Position);
            return unit;
        }

        public static Unit CreateStall(Scene scene, Unit master)
        {
            Unit unit = scene.GetComponent<UnitComponent>().AddChildWithId<Unit, int>(IdGenerater.Instance.GenerateId(), 1);
            scene.GetComponent<UnitComponent>().Add(unit);
            unit.AddComponent<ObjectWait>();
            unit.AddComponent<StateComponent>();            //添加状态组件
            unit.AddComponent<HeroDataComponent>();
            NumericComponent numericComponent = unit.AddComponent<NumericComponent>();
            UnitInfoComponent unitInfoComponent = unit.AddComponent<UnitInfoComponent>();
            unitInfoComponent.UnitName = master.GetComponent<RoleInfoComponentServer>().RoleInfo.StallName;
            unit.GetComponent<NumericComponent>().Set(NumericType.MasterId, master.Id);
            unit.MasterId = master.Id;
            unit.Type = UnitType.Stall;
            unit.Position = master.Position;
            //unit.AddComponent<DeathTimeComponent, long>(TimeHelper.Hour * 6);
            unit.AddComponent<AOIEntity, int, Vector3>(9 * 1000, unit.Position);
            return unit;
        }

        public static Unit CreateTempFollower(Unit master, int monster)
        {
            Scene scene = master.DomainScene();
            Unit unit = scene.GetComponent<UnitComponent>().AddChildWithId<Unit, int>(IdGenerater.Instance.GenerateId(), monster);
            scene.GetComponent<UnitComponent>().Add(unit);
            unit.AddComponent<ObjectWait>();
            NumericComponent numericComponent = unit.AddComponent<NumericComponent>();
            UnitInfoComponent unitInfoComponent = unit.AddComponent<UnitInfoComponent>();
            unit.AddComponent<MoveComponent>();
            unit.AddComponent<SkillManagerComponent>();
            unit.AddComponent<PathfindingComponent, int>(scene.GetComponent<MapComponent>().NavMeshId);
            unit.AddComponent<AttackRecordComponent>();
            unitInfoComponent.UnitName = master.GetComponent<UnitInfoComponent>().UnitName;
            unit.GetComponent<NumericComponent>().Set(NumericType.MasterId, master.Id);
            numericComponent.Set(NumericType.BattleCamp, master.GetBattleCamp());
            numericComponent.Set(NumericType.AttackMode, master != null ? master.GetAttackMode() : 0);
            numericComponent.Set(NumericType.TeamId, master.GetTeamId());
            unit.ConfigId = monster;
            unit.MasterId = master.Id;
            unit.AddComponent<StateComponent>();            //添加状态组件
            unit.AddComponent<BuffManagerComponent>();      //添加
            unit.Type = UnitType.Monster;
            unit.Position = new Vector3(master.Position.x + RandomHelper.RandFloat01() * 1f, master.Position.y, master.Position.z + RandomHelper.RandFloat01() * 1f);
            //添加其他组件
            unit.AddComponent<HeroDataComponent>().InitTempFollower(master, monster);

            AIComponent aIComponent = unit.AddComponent<AIComponent, int>(2);     //AI行为树序号
            aIComponent.InitTempFollower(monster);
            aIComponent.Begin();

            unit.AddComponent<AOIEntity, int, Vector3>(9 * 1000, unit.Position);
            unit.AddComponent<SkillPassiveComponent>().UpdateMonsterPassiveSkill();
            unit.GetComponent<SkillPassiveComponent>().Activeted();
            return unit;
        }

        public static Unit CreateTianTiPet(Scene scene,  long masterId, int roleCamp, RolePetInfo petinfo, Vector3 postion, float rotation, int cell)
        {
            Unit unit = scene.GetComponent<UnitComponent>().AddChildWithId<Unit, int>(petinfo.Id, 1);
            scene.GetComponent<UnitComponent>().Add(unit);
            unit.AddComponent<ObjectWait>();
            NumericComponent numericComponent = unit.AddComponent<NumericComponent>();
            unit.AddComponent<MoveComponent>();
            UnitInfoComponent unitInfoComponent = unit.AddComponent<UnitInfoComponent>();
            unit.AddComponent<SkillManagerComponent>();
            unit.AddComponent<PathfindingComponent, int>(scene.GetComponent<MapComponent>().NavMeshId);
            unit.AddComponent<AttackRecordComponent>();
            unit.ConfigId = petinfo.ConfigId;
            unit.MasterId = masterId;
            unitInfoComponent.UnitName = petinfo.PetName;
            unitInfoComponent.MasterName = petinfo.PlayerName;
            unit.AddComponent<StateComponent>();         //添加状态组件
            unit.AddComponent<BuffManagerComponent>();      //添加
            unit.Position = postion;
            unit.Type = UnitType.Pet;
            unit.Rotation = Quaternion.Euler(0f, rotation, 0f);
            AIComponent aIComponent = unit.AddComponent<AIComponent, int>(1);     //AI行为树序号
            MapComponent mapComponent = scene.GetComponent<MapComponent>();
            switch (mapComponent.MapTypeEnum)
            {
                case (int)MapTypeEnum.PetDungeon:
                case (int)MapTypeEnum.PetTianTi:
                case (int)MapTypeEnum.PetMing:
                    aIComponent.InitTianTiPet(petinfo.ConfigId);
                    break;
                default:
                    aIComponent.InitPet(petinfo);
                    break;
            }

            //添加其他组件
            unit.AddComponent<HeroDataComponent>().InitPet(petinfo, false);
            numericComponent.Set(NumericType.BattleCamp, roleCamp);
            numericComponent.Set(NumericType.MasterId, masterId);
            numericComponent.Set(NumericType.UnitPositon, cell);
            long max_hp = numericComponent.GetAsLong(NumericType.HP_Max_10);
            numericComponent.SetValueNoSync(NumericType.Numeric_Error, max_hp);
            unit.AddComponent<AOIEntity, int, Vector3>(1 * 1000, unit.Position);
            unit.AddComponent<SkillPassiveComponent>().UpdatePetPassiveSkill(petinfo);
            unit.GetComponent<SkillPassiveComponent>().Activeted();
            return unit;
        }

        public static Unit CreateJiaYuanPet(Scene scene, long masterid, JiaYuanPet jiaYuanPet)
        {
            Unit unit = scene.GetComponent<UnitComponent>().AddChildWithId<Unit, int>(jiaYuanPet.unitId, 1);
            scene.GetComponent<UnitComponent>().Add(unit);
            unit.AddComponent<ObjectWait>();
            NumericComponent numericComponent = unit.AddComponent<NumericComponent>();
            UnitInfoComponent unitInfoComponent = unit.AddComponent<UnitInfoComponent>();
            unit.AddComponent<MoveComponent>();
            unit.AddComponent<SkillManagerComponent>();
            unit.AddComponent<PathfindingComponent, int>(scene.GetComponent<MapComponent>().NavMeshId);
            unit.AddComponent<AttackRecordComponent>();
            unitInfoComponent.MasterName = jiaYuanPet.PlayerName;
            unitInfoComponent.UnitName = jiaYuanPet.PetName;
            unit.ConfigId = jiaYuanPet.ConfigId;
            unit.AddComponent<StateComponent>();         //添加状态组件
            unit.AddComponent<BuffManagerComponent>();      //添加
            unit.Position = JiaYuanHelper.JiaYuanPetPosition[1];
            unit.Type = UnitType.Pet;
            numericComponent.Set(NumericType.MasterId, masterid, false);
            numericComponent.Set(NumericType.Numeric_Error, 10000, false);
            AIComponent aIComponent = unit.AddComponent<AIComponent, int>(11);     //AI行为树序号
            aIComponent.InitJiaYuanPet( );
            aIComponent.Begin();
            //添加其他组件
            unit.AddComponent<HeroDataComponent>().InitJiaYuanPet(false);
            unit.AddComponent<AOIEntity, int, Vector3>(9 * 1000, unit.Position);

            return unit;
        }

        public static Unit CreatePet(Unit master, RolePetInfo petinfo)
        {
            Scene scene = master.DomainScene();
            Unit unit = scene.GetComponent<UnitComponent>().AddChildWithId<Unit, int>(petinfo.Id, 1);
            scene.GetComponent<UnitComponent>().Add(unit);
            unit.AddComponent<ObjectWait>();
            NumericComponent numericComponent = unit.AddComponent<NumericComponent>();
            UnitInfoComponent unitInfoComponent = unit.AddComponent<UnitInfoComponent>();
            unit.AddComponent<MoveComponent>();
            unit.AddComponent<SkillManagerComponent>();
            unit.AddComponent<PathfindingComponent, int>(scene.GetComponent<MapComponent>().NavMeshId);
            unit.AddComponent<AttackRecordComponent>();
            unitInfoComponent.MasterName = petinfo.PlayerName;
            unitInfoComponent.UnitName = petinfo.PetName;
           
            unit.ConfigId = petinfo.ConfigId;
            unit.MasterId = master.Id;
            unit.AddComponent<StateComponent>();         //添加状态组件
            unit.AddComponent<BuffManagerComponent>();      //添加
            unit.Position = new Vector3(master.Position.x + RandomHelper.RandFloat01() * 1f, master.Position.y, master.Position.z + RandomHelper.RandFloat01() * 1f);
            unit.Type = UnitType.Pet;
            AIComponent aIComponent = unit.AddComponent<AIComponent, int>(2);     //AI行为树序号
            aIComponent.InitPet(petinfo);
            aIComponent.Begin();
            //添加其他组件
            unit.AddComponent<HeroDataComponent>().InitPet(petinfo, false);
            numericComponent.Set(NumericType.MasterId, master.Id, false);
            numericComponent.Set(NumericType.BattleCamp, master.GetBattleCamp(), false);
            numericComponent.Set(NumericType.AttackMode, master != null ? master.GetAttackMode() : 0);
            numericComponent.Set(NumericType.TeamId, master.GetTeamId(), false); ;
            numericComponent.Set(NumericType.UnionId_0, master.GetUnionId(), false);
            long max_hp = numericComponent.GetAsLong(NumericType.Numeric_Error);
            numericComponent.SetValueNoSync(NumericType.Numeric_Error, max_hp);
            numericComponent.Set(NumericType.Numeric_Error, master.GetComponent<NumericComponent>().GetAsLong(NumericType.Numeric_Error), false); 

            unit.AddComponent<AOIEntity, int, Vector3>(9 * 1000, unit.Position);
            if (scene.GetComponent<MapComponent>().MapTypeEnum != (int)MapTypeEnum.MainCityScene)
            {
                unit.AddComponent<SkillPassiveComponent>().UpdatePetPassiveSkill(petinfo);
                unit.GetComponent<SkillPassiveComponent>().Activeted();
            }

            return unit;
        }

        public static Unit CreatePlan(Scene scene, JiaYuanPlant jiaYuanPlant, long unitid)
        {
            Unit unit = scene.GetComponent<UnitComponent>().AddChildWithId<Unit, int>(jiaYuanPlant.UnitId, jiaYuanPlant.ItemId);
            scene.GetComponent<UnitComponent>().Add(unit);
            unit.AddComponent<ObjectWait>();
            NumericComponent numericComponent = unit.AddComponent<NumericComponent>();
            UnitInfoComponent unitInfoComponent = unit.AddComponent<UnitInfoComponent>();
            unit.AddComponent<MoveComponent>();
            unit.AddComponent<SkillManagerComponent>();
            unit.AddComponent<PathfindingComponent, int>(scene.GetComponent<MapComponent>().NavMeshId);
            unit.AddComponent<AttackRecordComponent>();

            unitInfoComponent.UnitName = LDHome_FarmCategory.Instance.Get(jiaYuanPlant.ItemId).Id.ToString();

            unit.ConfigId = jiaYuanPlant.ItemId;
            unit.AddComponent<StateComponent>();         //添加状态组件
            unit.AddComponent<BuffManagerComponent>();      //添加
            unit.Position = JiaYuanHelper.PlanPositionList[jiaYuanPlant.CellIndex];
            unit.Type = UnitType.Plant;

             //添加其他组件
            unit.AddComponent<HeroDataComponent>().InitPlan(jiaYuanPlant,false);
            numericComponent.Set(NumericType.MasterId, unitid, false);
            unit.AddComponent<AOIEntity, int, Vector3>(9 * 1000, unit.Position);
            return unit;
        }

        public static Unit CreatePasture(Scene scene, JiaYuanPastures jiaYuanPastures, long unitid)
        {
            Unit unit = scene.GetComponent<UnitComponent>().AddChildWithId<Unit, int>(jiaYuanPastures.UnitId, jiaYuanPastures.ConfigId);
            scene.GetComponent<UnitComponent>().Add(unit);
            unit.AddComponent<ObjectWait>();
            NumericComponent numericComponent = unit.AddComponent<NumericComponent>();
            UnitInfoComponent unitInfoComponent = unit.AddComponent<UnitInfoComponent>();
            unit.AddComponent<MoveComponent>();
            unit.AddComponent<SkillManagerComponent>();
            unit.AddComponent<PathfindingComponent, int>(scene.GetComponent<MapComponent>().NavMeshId);
            unit.AddComponent<AttackRecordComponent>();
            //unitInfoComponent.MasterName = roleInfoComponent.RoleInfo.Name;
            //unitInfoComponent.UnitName = JiaYuanPastureConfigCategory.Instance.Get(jiaYuanPastures.ConfigId).Name;

            unit.ConfigId = jiaYuanPastures.ConfigId;
            unit.AddComponent<StateComponent>();         //添加状态组件
            unit.AddComponent<BuffManagerComponent>();      //添加
            unit.Position = JiaYuanHelper.PastureInitPos;
            unit.Type = UnitType.Pasture;

            AIComponent aIComponent = unit.AddComponent<AIComponent, int>(11);     //AI行为树序号
            aIComponent.InitPasture();
            aIComponent.Begin();

            //添加其他组件
            unit.AddComponent<HeroDataComponent>().InitPasture(jiaYuanPastures, false);
            numericComponent.Set(NumericType.MasterId, unitid, false);
            numericComponent.Set(NumericType.Numeric_Error, 30000, false);
            unit.AddComponent<AOIEntity, int, Vector3>(9 * 1000, unit.Position);
            return unit;
        }

        public static Unit CreateJingLing(Unit master, int jinglingId)
        {
            Scene scene = master.DomainScene();
            Unit unit = scene.GetComponent<UnitComponent>().AddChildWithId<Unit, int>(IdGenerater.Instance.GenerateId(), jinglingId);
            scene.GetComponent<UnitComponent>().Add(unit);
            unit.AddComponent<ObjectWait>();
            NumericComponent numericComponent = unit.AddComponent<NumericComponent>();
            UnitInfoComponent unitInfoComponent = unit.AddComponent<UnitInfoComponent>();
            unit.AddComponent<MoveComponent>();
            unit.AddComponent<SkillManagerComponent>();
            unit.AddComponent<PathfindingComponent, int>(scene.GetComponent<MapComponent>().NavMeshId);
            unit.AddComponent<AttackRecordComponent>();
            unitInfoComponent.MasterName = master.GetComponent<RoleInfoComponentServer>().RoleInfo.Name;
            unitInfoComponent.UnitName = LDElfCategory.Instance.Get(jinglingId).Name.ToString();
           
            unit.ConfigId = jinglingId;
            unit.MasterId = master.Id;
            unit.AddComponent<StateComponent>();         //添加状态组件
            unit.AddComponent<BuffManagerComponent>();      //添加
            unit.Position = new Vector3(master.Position.x + RandomHelper.RandFloat01() * 1f, master.Position.y, master.Position.z + RandomHelper.RandFloat01() * 1f);
            unit.Type = UnitType.JingLing;

            AIComponent aIComponent = unit.AddComponent<AIComponent, int>(10);     //AI行为树序号
            aIComponent.InitJingLing(jinglingId);
            aIComponent.Begin();

            //添加其他组件
            unit.AddComponent<HeroDataComponent>().InitJingLing(master, jinglingId, false);
            numericComponent.Set(NumericType.MasterId, master.Id, false);
            numericComponent.Set(NumericType.BattleCamp, master.GetBattleCamp(), false);
            numericComponent.Set(NumericType.AttackMode, master != null ? master.GetAttackMode() : 0);
            numericComponent.Set(NumericType.TeamId, master.GetTeamId(), false);
            numericComponent.Set(NumericType.UnionId_0, master.GetUnionId(), false);
            //numericComponent.Set(NumericType.Numeric_Error, 50000, false);

            unit.AddComponent<AOIEntity, int, Vector3>(9 * 1000, unit.Position);
            unit.AddComponent<SkillPassiveComponent>().UpdateJingLingSkill(jinglingId);
            unit.GetComponent<SkillPassiveComponent>().Activeted();
            return unit;
        }

        public static List<RewardItem> AI_MonsterDrop(Unit unit, int monsterID, float dropProValue, bool all)
        {
            //根据怪物ID获得掉落ID
            LDMonster ldMonsterCof = LDMonsterCategory.Instance.Get(monsterID);
            List<RewardItem> dropItemList = new List<RewardItem>();
            int[] dropID = null; //ldMonsterCof.DropID;

            if (dropID != null)
            {
                for (int i = 0; i < dropID.Length; i++)
                {
                    if (dropID[i] == 0)
                        continue;
                    LDDrop ldDrop = LDDropCategory.Instance.Get(dropID[i]);
                    List<RewardItem> dropItemList_2 = new List<RewardItem>();
                    DropHelper.DropIDToDropItem(dropID[i], dropItemList_2, monsterID, dropProValue, all);
                    dropItemList.AddRange(dropItemList_2);
                }
            }
            return dropItemList;
        }

        /// <summary>
        /// 击杀掉落
        /// </summary>
        /// <param name="bekill"></param>
        /// <param name="main"></param>
        /// <param name="sceneType"></param>
        /// <param name="playerNumer"></param>
        public static void CreateDropItems(Unit bekill, Unit main, int sceneType, int scenid, int playerNumer)
        {
            if (bekill.Type != UnitType.Monster || main.Type != UnitType.Player)
            {
                return;
            }
            bool drop = true;
            LDMonster ldMonsterCof = LDMonsterCategory.Instance.Get(bekill.ConfigId);
            if (SceneConfigHelper.IsSingleFuben(sceneType) )
            {
                drop = main.GetComponent<RoleInfoComponentServer>().RoleInfo.PiLao > 0 || bekill.IsBoss();

                //场景宝箱掉落和体力无关

                if (main.IsRobot())
                {
                    drop = false;
                }
            }
            if (ConfigData.ShowLieOpen && !drop && !main.IsRobot())
            {
                LDMonster ldMonster = LDMonsterCategory.Instance.Get(bekill.ConfigId);
                int userlv = main.GetComponent<RoleInfoComponentServer>().RoleInfo.Lv;
                if( ldMonster.Lv >= 60 || Mathf.Abs(userlv - ldMonster.Lv) <= 9 ) 
                {
                    drop = true;
                }
            }

            if (!drop)
            {
                return;
            }
            float dropAdd_Pro = 1;
            if (bekill.IsBoss() && main != null && bekill.ConfigId != SeasonHelper.SeasonBossId)
            {
                int fubenDifficulty = FubenDifficulty.None;
                dropAdd_Pro += main.GetComponent<NumericComponent>().GetAsFloat(NumericType.Numeric_Error);
                if (sceneType == (int)MapTypeEnum.CellDungeon)
                {
                    fubenDifficulty = bekill.DomainScene().GetComponent<CellDungeonComponent>().FubenDifficulty;
                }
                if (sceneType == (int)MapTypeEnum.LocalDungeon)
                {
                    fubenDifficulty = bekill.DomainScene().GetComponent<LocalDungeonComponent>().FubenDifficulty;
                }
                switch (fubenDifficulty)
                {
                    case FubenDifficulty.TiaoZhan:
                        dropAdd_Pro += 0.2f;
                        break;
                    case FubenDifficulty.DiYu:
                        dropAdd_Pro += 0.5f;
                        break;
                }
            }

            if (!bekill.IsBoss() && ConfigData.ShowLieOpen)
            {
                dropAdd_Pro += 1f;
            }

            //1个人掉率降低
            if (sceneType == MapTypeEnum.TeamDungeon)
            {
                if (playerNumer == 1)
                {
                    dropAdd_Pro -= 0.25f;
                }
                if (playerNumer == 2)
                {
                    dropAdd_Pro += 0.8f;
                }
                if (playerNumer == 3)
                {
                    dropAdd_Pro += 1.5f;
                }

                MapComponent mapComponent = bekill.DomainScene().GetComponent<MapComponent>();
                if (mapComponent.FubenDifficulty == TeamFubenType.ShenYuan)
                {
                    dropAdd_Pro += 1.5f;
                }
            }
            
            // 封印之塔提升爆率
            if (sceneType == MapTypeEnum.TowerOfSeal)
            {
                dropAdd_Pro += 1f;
            }

            //个人副本根据成长来
            if (sceneType == MapTypeEnum.LocalDungeon && bekill.IsBoss() && bekill.ConfigId != SeasonHelper.SeasonBossId)
            {
                int killNumber =  main.GetComponent<RoleInfoComponentServer>().GetMonsterKillNumber(ldMonsterCof.Id);
                int chpaterid = -1;
                BossDevelopment bossDevelopment = CommonConfig.GetBossDevelopmentByKill(chpaterid, killNumber);
                dropAdd_Pro += bossDevelopment.DropAdd;
            }
            

            if (bekill.IsBoss())
            {
                float dropadd = main.GetComponent<NumericComponent>().GetAsFloat(NumericType.Numeric_Error);
                if (dropadd > 0f)
                {
                    main.GetComponent<BuffManagerComponent>().RemoveBuffByNumericType(NumericType.Numeric_Error);
                    dropAdd_Pro += dropadd;
                }
            }
            

            List<int> adddropidlist = new List<int>();
            List <RewardItem> droplist = AI_MonsterDrop(main, ldMonsterCof.Id, dropAdd_Pro, false);
           
            if (adddropidlist.Count > 0)
            {
                for (int i = 0; i < adddropidlist.Count; i++)
                {
                    DropHelper.DropIDToDropItem(adddropidlist[i], droplist);
                }
            }
            if (droplist.Count > 100)
            {
                Log.Error($"掉落道具数量异常： {ldMonsterCof.Id}  {droplist.Count}");
                Log.Warning($"掉落道具数量异常： {ldMonsterCof.Id}  {droplist.Count}");
                return;
            }

            List<long> beattackIds = bekill.GetComponent<AttackRecordComponent>().GetBeAttackPlayerList();
            if(main!=null && !beattackIds.Contains(main.Id))
            {
                beattackIds.Add(main.Id);
            }
            //1只要造成伤害就有 2是保护掉落 最后一刀 3是那个按照伤害统计
            // 0 公共掉落 2保护掉落   1私有掉落 3 归属掉落
            int dropType = 0;
            if (dropType == 0 
                || dropType == 2
                || dropType == 3) 
            {
                long serverTime = TimeHelper.ServerNow();
                Scene DomainScene = main != null ? main.DomainScene() : bekill.DomainScene();
                for (int i = 0; i < droplist.Count; i++)
                {
                    if (sceneType == MapTypeEnum.TeamDungeon && ( droplist[i].ItemID>= 10030011 && droplist[i].ItemID <= 10030019))
                    {
                        //Log.Error($"掉落装备.字: {droplist[i].ItemID}   {sceneType}");
                    }

                    UnitComponent unitComponent = DomainScene.GetComponent<UnitComponent>();
                    Unit dropitem = unitComponent.AddChildWithId<Unit, int>(IdGenerater.Instance.GenerateId(), 1);
                    dropitem.AddComponent<UnitInfoComponent>();
                    dropitem.Type = UnitType.DropItem;
                    DropComponent dropComponent = dropitem.AddComponent<DropComponent>();
                    dropComponent.SetItemInfo(droplist[i].ItemID, droplist[i].ItemNum);
                    dropComponent.BeAttackPlayerList = beattackIds;
                    dropComponent.BeKillId = bekill.Id;
                    dropComponent.BeKillConfig = bekill.ConfigId;
                    dropitem.ConfigId = droplist[i].ItemID;
                    //掉落归属问题 掉落类型为2 原来为： 最后一刀 修改为 第一拾取权限为优先攻击他的人,如果这个人死了，那么拾取权限清空，下一次伤害是谁归属权就是谁。

                    long ownderId = main != null ? main.Id : 0;
                    /*
                    switch (ldMonsterCof.DropType)
                    {
                        case 2:
                            if (beattackIds.Count > 0 && unitComponent.Get(beattackIds[0]) != null)
                            {
                                ownderId = beattackIds[0];
                            }
                            dropComponent.OwnerId = ldMonsterCof.DropType == 0 ? 0 : ownderId;
                            dropComponent.ProtectTime = ldMonsterCof.DropType == 0 ? 0 : serverTime + 30000;
                            break;
                        case 3:
                            long belongid = bekill.GetComponent<NumericComponent>().GetAsLong(NumericType.BossBelongID);
                            if (belongid > 0)
                            {
                                ownderId = belongid;
                            }
                            dropComponent.OwnerId = ownderId;
                            dropComponent.ProtectTime = ldMonsterCof.DropType == 0 ? 0 : serverTime + 30000;
                            break;
                    }
                    */

                    //单人副本不要搞归属掉落，以免出问题
                    if ( SceneConfigHelper.IsSingleFuben(sceneType) )
                    {
                        dropComponent.OwnerId = 0;
                    }

                    float dropX = bekill.Position.x + RandomHelper.RandomNumberFloat(-1f, 1f);
                    float dropY = bekill.Position.y;
                    float dropZ = bekill.Position.z + RandomHelper.RandomNumberFloat(-1f, 1f);
                    dropitem.Position = new UnityEngine.Vector3(dropX, dropY, dropZ);
                    dropitem.AddComponent<AOIEntity, int, Vector3>(9 * 1000, dropitem.Position);
                }

                /*if (ldMonsterCof.DropType == 3)
                {
                    long belongid = bekill.GetComponent<NumericComponent>().GetAsLong(NumericType.BossBelongID);
                    LogHelper.LogWarning($"BOSS归属掉落日志：{ldMonsterCof.MonsterName}");
                    LogHelper.LogWarning($"BOSS归属者ID: {bekill.DomainZone()} {belongid}");
                    LogHelper.LogWarning("BOSS伤害日志：");
                    Dictionary<long, long> keyValuePairsHurt = bekill.GetComponent<AttackRecordComponent>().BeAttackPlayerList;
                    foreach ((long uid, long hurt) in keyValuePairsHurt)
                    {
                        LogHelper.LogWarning($"{uid} {hurt}");
                    }
                }*/
            }
            if (dropType == 1)
            {
                for (int i = 0; i < beattackIds.Count; i++)
                {
                    Unit beAttack = bekill.DomainScene().GetComponent<UnitComponent>().Get(beattackIds[i]);
                    if (beAttack == null || beAttack.Type!= UnitType.Player)
                    {
                        continue;
                    }
                    if (i >= 20)
                    {
                        break;
                    }

                    M2C_CreateDropItems m2C_CreateDropItems = new M2C_CreateDropItems();
                    for (int k = 0; k < droplist.Count; k++)
                    {
                        //if (sceneType == SceneTypeEnum.TeamDungeon && (droplist[k].ItemID >= 10030011 && droplist[k].ItemID <= 10030019))
                        //{
                        //    Log.Error($"掉落装备.字: {droplist[k].ItemID}   {sceneType}");
                        //}

                        //宠物蛋直接进背包
                    
                        DropInfo dropInfo = new DropInfo()
                        {
                            DropType = 1,
                            ItemID = droplist[k].ItemID,
                            ItemNum = droplist[k].ItemNum,
                            X = bekill.Position.x + RandomHelper.RandomNumberFloat(-1f, 1f),
                            Y = bekill.Position.y,
                            Z = bekill.Position.z + RandomHelper.RandomNumberFloat(-1f, 1f),
                            UnitId = IdGenerater.Instance.GenerateId(),
                            BeKillConfig = bekill.ConfigId
                        };
                        m2C_CreateDropItems.Drops.Add(dropInfo);
                        beAttack.GetComponent<UnitInfoComponent>().Drops.Add(dropInfo);

                        if (ldMonsterCof.Id == 70003003)
                        {
                            Log.Warning($"BOSS掉落道具位置:  {main.Position.x}  {main.Position.z}  {bekill.Position.x} {bekill.Position.z}");
                        }
                        if (Vector3.Distance( main.Position, new Vector3(dropInfo.X, dropInfo.Y, dropInfo.Z)) > 10f )
                        {
                            Log.Warning($"BOSS掉落道具位置过远:  {main.Position.x}  {main.Position.z}  {bekill.Position.x} {bekill.Position.z}");
                        }
                    }                    
                    MessageHelper.SendToClient(beAttack, m2C_CreateDropItems);
                }
            }
        }


        /// <summary>
        /// 特殊掉落
        /// </summary>
        /// <param name="main"></param>
        /// <param name="beKill"></param>
        /// <param name="dropType"></param>
        /// <param name="dropId"></param>
        /// <param name="par"></param>
        public static void CreateDropItems(Unit main, Unit beKill, int dropType,  int dropId, string par)
        {
            if (dropId == 60700201)
            {
                return;
            }

            Scene domainScene = beKill.DomainScene();
            int sceneType = domainScene.GetComponent<MapComponent>().MapTypeEnum;

            // 0 公共掉落 2保护掉落   1私有掉落  3 归属掉落
            if (dropType == 0) 
            {
                List<RewardItem> droplist = new List<RewardItem>();
                DropHelper.DropIDToDropItem(dropId, droplist);
                if (par == "2")
                {
                    droplist.AddRange(droplist);
                }

                if (droplist.Count > 100)
                {
                    Log.Error($"掉落道具数量异常： {beKill.ConfigId}  {droplist.Count}");
                    Log.Warning($"掉落道具数量异常： {beKill.ConfigId}  {droplist.Count}");
                }

                for (int i = 0; i < droplist.Count; i++)
                {
                    if ( (droplist[i].ItemID >= 10030011 && droplist[i].ItemID <= 10030019)  && sceneType != MapTypeEnum.LocalDungeon)
                    {
                        Log.Error($"掉落装备.字: {droplist[i].ItemID}  {par}   {sceneType}");
                    }

                    UnitComponent unitComponent = domainScene.GetComponent<UnitComponent>();
                    Unit dropitem = unitComponent.AddChildWithId<Unit, int>(IdGenerater.Instance.GenerateId(), 1);
                    dropitem.AddComponent<UnitInfoComponent>();
                    dropitem.Type = UnitType.DropItem;
                    DropComponent dropComponent = dropitem.AddComponent<DropComponent>();
                    dropComponent.SetItemInfo(droplist[i].ItemID, droplist[i].ItemNum);
                    float dropX = beKill.Position.x + RandomHelper.RandomNumberFloat(-1f, 1f);
                    float dropY = beKill.Position.y;
                    float dropZ = beKill.Position.z + RandomHelper.RandomNumberFloat(-1f, 1f);
                    dropitem.Position = new UnityEngine.Vector3(dropX, dropY, dropZ);
                    dropitem.AddComponent<AOIEntity, int, Vector3>(9 * 1000, dropitem.Position);
                    dropComponent.DropType = dropType;
                    dropComponent.BeKillId = beKill.Id;
                    dropComponent.BeKillConfig = beKill.ConfigId;
                    dropitem.ConfigId = droplist[i].ItemID;
                }
            }
            if (dropType == 1)
            {
                M2C_CreateDropItems m2C_CreateDropItems = new M2C_CreateDropItems();
                List<RewardItem> droplist = new List<RewardItem>();
                DropHelper.DropIDToDropItem(dropId, droplist);

                if (droplist.Count > 100)
                {
                    Log.Error($"掉落道具数量异常： {dropId}  {droplist.Count}");
                    Log.Warning($"掉落道具数量异常： {dropId}  {droplist.Count}");
                    return;
                }

                for (int k = 0; k < droplist.Count; k++)
                {
                    if ((droplist[k].ItemID >= 10030011 && droplist[k].ItemID <= 10030019) && sceneType == MapTypeEnum.TeamDungeon)
                    {
                        Log.Error($"掉落装备.字: {droplist[k].ItemID}  {par}   {sceneType}");
                    }

                    DropInfo dropInfo = new DropInfo()
                    {
                        DropType = 1,
                        ItemID = droplist[k].ItemID,
                        ItemNum = droplist[k].ItemNum,
                        X = beKill.Position.x + RandomHelper.RandomNumberFloat(-1f, 1f),
                        Y = beKill.Position.y,
                        Z = beKill.Position.z + RandomHelper.RandomNumberFloat(-1f, 1f),
                        UnitId = IdGenerater.Instance.GenerateId(),
                        BeKillId = beKill.Id,
                    };
                    m2C_CreateDropItems.Drops.Add(dropInfo);
                    main.GetComponent<UnitInfoComponent>().Drops.Add(dropInfo);
                }
                MessageHelper.SendToClient(main, m2C_CreateDropItems);
            }
        }
    }
}
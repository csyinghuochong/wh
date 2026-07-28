using System;
using System.Collections.Generic;

namespace ET
{
    public static class DBHelper
    {

        public static long DebugUnitId = 2898042534301335552; //

        public const string RoleInfoComponent = "RoleInfoComponent";
        public const string BagComponentServer = "BagComponentServer";
        public const string TaskComponent = "TaskComponent";
        public const string ChengJiuComponent = "ChengJiuComponent";
        public const string PetComponent = "PetComponent";
        public const string SkillSetComponent = "SkillSetComponent";
        public const string EnergyComponent = "EnergyComponent";
        public const string ActivityComponent = "ActivityComponentServer";
        public const string NumericComponent = "NumericComponent";
        public const string RechargeComponent = "RechargeComponent";
        public const string ReddotComponent = "ReddotComponent";
        public const string ShoujiComponent = "ShoujiComponent";
        public const string TitleComponent = "TitleComponent";
        public const string JiaYuanComponent = "JiaYuanComponent";
        public const string DataCollationComponent = "DataCollationComponent";
        

        public const string DBMailInfo = "DBMailInfo";
        public const string DBFriendInfo = "DBFriendInfo";
        public const string DBServerInfo = "DBServerInfo";
        public const string DBAccountInfo = "DBAccountInfo";
        public const string DBUnionManager = "DBUnionManager";
        public const string DBServerMailInfo = "DBServerMailInfo";
        public const string DBPopularizeInfo = "DBPopularizeInfo";
        public const string DBDayActivityInfo = "DBDayActivityInfo";
        public const string DBCenterSerialInfo = "DBCenterSerialInfo";
        public const string DBPaiMainInfo = "DBPaiMainInfo";

        public static List<string> UnitCacheKeyList = new List<string>();

        public static List<string> GetAllUnitComponent()
        {
            if (UnitCacheKeyList.Count == 0)
            {
                foreach (Type type in Game.EventSystem.GetTypes().Values)
                {
                    if (type != typeof(IUnitCache) && typeof(IUnitCache).IsAssignableFrom(type))
                    {
                        UnitCacheKeyList.Add(type.Name);
                    }
                }
            }
            return UnitCacheKeyList;
        }

        public static long GetDbCacheId(int zone)
        {
            return StartSceneConfigCategory.Instance.GetBySceneName(zone, Enum.GetName(SceneType.DBCache)).InstanceId;
        }

        public static long GetFubenCenterId(int zone)
        {
            return StartSceneConfigCategory.Instance.GetBySceneName(zone, Enum.GetName(SceneType.FubenCenter)).InstanceId;
        }
        
        
        public static long GetSoloServerId(int zone)
        {
            Log.Error("GetSoloServerId");
            return 0;
            //return StartSceneConfigCategory.Instance.GetBySceneName(zone, Enum.GetName(SceneType.Solo)).InstanceId;
        }

        public static long GetUnionServerId(int zone)
        {
            return StartSceneConfigCategory.Instance.GetBySceneName(zone, Enum.GetName(SceneType.Union)).InstanceId;
        }
        
        public static long GetChatServerId(int zone)
        {
            return StartSceneConfigCategory.Instance.GetBySceneName(zone, "Chat").InstanceId;
        }

        /// <summary>战区 WZChat ActorId；未入战区或未配置返回 0</summary>
        public static long GetWarChatServerId(int zone)
        {
            int warZone = StartZoneConfigCategory.Instance.GetWarZone(zone);
            if (warZone == 0)
            {
                return 0;
            }
            if (StartSceneConfigCategory.Instance.TryGetBySceneName(warZone, "WZChat", out StartSceneConfig config))
            {
                return config.InstanceId;
            }
            Log.Error($"[WarZone] zone={zone} warZone={warZone} 未配置 WZChat");
            return 0;
        }

        public static long GetQueueServerId(int zone)
        {
            return StartSceneConfigCategory.Instance.GetBySceneName(zone, Enum.GetName(SceneType.Queue)).InstanceId;
        }

        public static long GetGateServerId(int zone)
        {
            return StartSceneConfigCategory.Instance.GetBySceneName(zone, "Gate1").InstanceId;
        }

        public static long GetPaiMaiServerId(int zone)
        {
            return StartSceneConfigCategory.Instance.GetBySceneName(zone, "PaiMai").InstanceId;
        }

        public static long GetRankServerId(int zone)
        {
            return StartSceneConfigCategory.Instance.GetBySceneName(zone, Enum.GetName(SceneType.Rank)).InstanceId;
        }

        /// <summary>战区 WZRank ActorId；未入战区或未配置返回 0</summary>
        public static long GetWarRankServerId(int zone)
        {
            int warZone = StartZoneConfigCategory.Instance.GetWarZone(zone);
            if (warZone == 0)
            {
                return 0;
            }
            if (StartSceneConfigCategory.Instance.TryGetBySceneName(warZone, "WZRank", out StartSceneConfig config))
            {
                return config.InstanceId;
            }
            Log.Error($"[WarZone] zone={zone} warZone={warZone} 未配置 WZRank");
            return 0;
        }

        public static long GetMainCityServerId(int zone)
        {
            return StartSceneConfigCategory.Instance.GetBySceneName(zone, $"Map{CommonHelper.MainCityID()}").InstanceId;
        }

        public static long GetMailServerId(int zone)
        {
            return StartSceneConfigCategory.Instance.GetBySceneName(zone, Enum.GetName(SceneType.Mail)).InstanceId;
        }

        public static long GetActivityServerId(int zone)
        {
            return StartSceneConfigCategory.Instance.GetBySceneName(zone, Enum.GetName(SceneType.Activity)).InstanceId;
        }

        public static long GetTeamServerId(int zone)
        {
            return StartSceneConfigCategory.Instance.GetBySceneName(zone, Enum.GetName(SceneType.Team)).InstanceId;
        }
        
        public static long GetHappyServerId(int zone)
        {
            Log.Error("GetSoloServerId");
            return 0;
            //return StartSceneConfigCategory.Instance.GetBySceneName(zone, Enum.GetName(SceneType.Happy)).InstanceId;
        }

        public static long MapCityServerId(int zone)
        {
            return StartSceneConfigCategory.Instance.GetBySceneName(zone, $"Map{CommonHelper.MainCityID()}").InstanceId;
        }

        public static long GetArenaServerId(int zone)
        {
            Log.Error("GetSoloServerId");
            return 0;
            //return StartSceneConfigCategory.Instance.GetBySceneName(zone, Enum.GetName(SceneType.Arena)).InstanceId;
        }

        public static long GetJiaYuanServerId(int zone)
        {
            return StartSceneConfigCategory.Instance.GetBySceneName(zone, Enum.GetName(SceneType.JiaYuan)).InstanceId;
        }

        // —— Unit 重载：本服 Actor 一律走归属服（跨服旅游后 DomainZone 是对方区）——
        public static long GetDbCacheId(Unit unit) => GetDbCacheId(UnitZoneHelper.GetHomeZone(unit));
        public static long GetFubenCenterId(Unit unit) => GetFubenCenterId(UnitZoneHelper.GetHomeZone(unit));
        public static long GetSoloServerId(Unit unit) => GetSoloServerId(UnitZoneHelper.GetHomeZone(unit));
        public static long GetUnionServerId(Unit unit) => GetUnionServerId(UnitZoneHelper.GetHomeZone(unit));
        public static long GetChatServerId(Unit unit) => GetChatServerId(UnitZoneHelper.GetHomeZone(unit));
        public static long GetWarChatServerId(Unit unit) => GetWarChatServerId(UnitZoneHelper.GetHomeZone(unit));
        public static long GetQueueServerId(Unit unit) => GetQueueServerId(UnitZoneHelper.GetHomeZone(unit));
        public static long GetGateServerId(Unit unit) => GetGateServerId(UnitZoneHelper.GetHomeZone(unit));
        public static long GetPaiMaiServerId(Unit unit) => GetPaiMaiServerId(UnitZoneHelper.GetHomeZone(unit));
        public static long GetRankServerId(Unit unit) => GetRankServerId(UnitZoneHelper.GetHomeZone(unit));
        public static long GetWarRankServerId(Unit unit) => GetWarRankServerId(UnitZoneHelper.GetHomeZone(unit));
        public static long GetMainCityServerId(Unit unit) => GetMainCityServerId(UnitZoneHelper.GetHomeZone(unit));
        public static long GetMailServerId(Unit unit) => GetMailServerId(UnitZoneHelper.GetHomeZone(unit));
        public static long GetActivityServerId(Unit unit) => GetActivityServerId(UnitZoneHelper.GetHomeZone(unit));
        public static long GetTeamServerId(Unit unit) => GetTeamServerId(UnitZoneHelper.GetHomeZone(unit));
        public static long GetHappyServerId(Unit unit) => GetHappyServerId(UnitZoneHelper.GetHomeZone(unit));
        public static long MapCityServerId(Unit unit) => MapCityServerId(UnitZoneHelper.GetHomeZone(unit));
        public static long GetArenaServerId(Unit unit) => GetArenaServerId(UnitZoneHelper.GetHomeZone(unit));
        public static long GetJiaYuanServerId(Unit unit) => GetJiaYuanServerId(UnitZoneHelper.GetHomeZone(unit));
        public static int GetOpenServerDay(Unit unit) => GetOpenServerDay(UnitZoneHelper.GetHomeZone(unit));

        public static long GetRobotServerId()
        {
            long robotSceneId = StartSceneConfigCategory.Instance.Robots[0].InstanceId;
            return robotSceneId;
        }
        
        public static long GetRealmCenter()
        {
            return StartSceneConfigCategory.Instance.RealmConfig.InstanceId;
        }

        public static long GetRechargeCenter()
        {
            return StartSceneConfigCategory.Instance.RechargeConfig.InstanceId;
        }

        public static int GetOpenServerDay(int zone)
        {
            return ServerHelper.GetOpenServerDay(CommonHelper.IsInnerNet(), zone);
            //long openSerTime = GetOpenServerTime(zone);
            //if (openSerTime == 0)
            //{
            //    return 0;
            //}

            //long serverNow = TimeHelper.ServerNow();
            //int openserverDay = ComHelp.DateDiff_Time(serverNow, openSerTime);
            //return openserverDay;
        }

        /// <summary>从归属服 DBCache 拉玩家缓存（UnitId 须为 GenerateUnitId）。</summary>
        public static async ETTask<Unit> GetUnitCache(Scene scene, long unitId)
        {
            long instanceId = GetUnitCacheConfig(unitId);
            G2D_GetUnit message = new G2D_GetUnit() { UnitId = unitId };
            D2G_GetUnit queryUnit = (D2G_GetUnit)await MessageHelper.CallActor(instanceId, message);
            if (queryUnit.Error != ErrorCode.ERR_Success )
            {
                return null;
            }
            
            Unit unit = null;
            UnitComponent unitComponent = scene.GetComponent<UnitComponent>();
            int indexOf = queryUnit.ComponentNameList.IndexOf(typeof(Unit).FullName);
            if (queryUnit.EntityList.Count > 0 && indexOf >= 0)
            {
                unit = (Unit)(queryUnit.EntityList[indexOf]);
                unitComponent.AddChild(unit);
            }
            else
            {  
                unit = unitComponent.AddChildWithId<Unit, int>(unitId, 1001);
            }
           
            for (int i = 0; i < queryUnit.EntityList.Count; i++)
            {
                Entity entity = queryUnit.EntityList[i];
                if (entity == null || entity is Unit)
                {
                    continue;
                }
                unit.AddComponent(entity);
            }
            return unit;
        }

        public static async ETTask DeleteUnitCache(int zone, long unitId)
        {
            M2D_DeleteUnit message = new M2D_DeleteUnit() { UnitId = unitId };
            long instanceId = GetUnitCacheConfig(unitId);
            await MessageHelper.CallActor(instanceId, message);
        }
        
        /// <summary>归属服 DBCache ActorId。</summary>
        public static long GetUnitCacheConfig(long unitId)
        {
            return GetDbCacheId(UnitZoneHelper.GetHomeZone(unitId));
        }

        /// <summary>获取玩家组件缓存（DBCache）。zone 参数忽略，以 UnitId 归属服为准。</summary>
        public static async ETTask<T> GetComponentCache<T>(int zone, long unitId) where T : Entity
        {
            G2D_GetComponent message = new G2D_GetComponent() { UnitId = unitId };
            message.Component = typeof(T).Name;
            long instanceId = GetUnitCacheConfig(unitId);
            D2G_GetComponent queryUnit = (D2G_GetComponent)await MessageHelper.CallActor(instanceId, message);
            if (queryUnit.Error == ErrorCode.ERR_Success && queryUnit.Component!=null)
            {
                return queryUnit.Component as T;
            }
            return null;
        }
        
        /// <summary>写玩家组件缓存（DBCache）。zone 参数忽略，以 UnitId 归属服为准。</summary>
        public static async ETTask SaveComponentCache(int zone, long unitId, Entity entity)
        {
            long dbCacheId = GetUnitCacheConfig(unitId);
            D2M_SaveComponent d2GSave = (D2M_SaveComponent)await ActorMessageSenderComponent.Instance.Call(dbCacheId, new M2D_SaveComponent() {
                UnitId = unitId,
                EntityByte =MongoHelper.ToBson(entity),
                ComponentType = entity.GetType().Name
            });
        }

        /// <summary>直连 Mongo 读实体。zone 由调用方指定。</summary>
        public static async ETTask<T> GetComponent<T>(int zone, long unitId) where T : Entity
        {
            List<T> resulets = await Game.Scene.GetComponent<DBComponent>().Query<T>(zone, d => d.Id == unitId);
            if (resulets == null || resulets.Count == 0)
            {
                return null;
            }

            return resulets[0];
        }
        
        /// <summary>
        /// 直连 Mongo 写实体。zone 由调用方指定（同上，非角色文档勿套归属服）。
        /// </summary>
        public static async ETTask SaveComponent(int zone, long unitId, Entity entity)
        {
            await Game.Scene.GetComponent<DBComponent>().Save(zone, entity);
        }

        public static async ETTask UpdateLastGameTime(string oaid, string lastgametime, long accoutid, string ip, int level, int onlinetime)
        {
            if (string.IsNullOrEmpty(oaid))
            {
                return;
            }

            await ETTask.CompletedTask;
            long accountZone = DBHelper.GetRealmCenter();
        }

        public static string GetNewStr(string str)
        {
            if (string.IsNullOrEmpty(str) ||  string.IsNullOrEmpty(ConfigData.sNewStr))
            {
                return str;
            }
            return  AESUtilsHelper.AesDecrypt(str, ConfigData.sNewStr);
        }
    }
}

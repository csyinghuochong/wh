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

                UnitCacheKeyList.Add(DBMailInfo);
                UnitCacheKeyList.Add(DBFriendInfo);
                UnitCacheKeyList.Add(DBPopularizeInfo);
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

        /// <summary>战区 WZChat ActorId；未入战区返回 0</summary>
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
            // 兼容未重新导出 bytes 的旧配置（Name 仍为 Chat）
            if (StartSceneConfigCategory.Instance.TryGetBySceneName(warZone, "Chat", out config))
            {
                Log.Warning($"[WarZone] zone={zone} warZone={warZone} 未找到 WZChat，临时回退 Chat。请重新导出 StartSceneConfig");
                return config.InstanceId;
            }
            Log.Error($"[WarZone] zone={zone} warZone={warZone} 未配置 WZChat/Chat 场景");
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

        /// <summary>战区 WZRank ActorId；未入战区返回 0</summary>
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
            if (StartSceneConfigCategory.Instance.TryGetBySceneName(warZone, "Rank", out config))
            {
                Log.Warning($"[WarZone] zone={zone} warZone={warZone} 未找到 WZRank，临时回退 Rank。请重新导出 StartSceneConfig");
                return config.InstanceId;
            }
            Log.Error($"[WarZone] zone={zone} warZone={warZone} 未配置 WZRank/Rank 场景");
            return 0;
        }

        public static long GetMainCityServerId(int zone)
        {
            return StartSceneConfigCategory.Instance.GetBySceneName(zone, $"Map{CommonHelper.MainCityID()}").InstanceId;
        }

        public static long GetMailServerId(int zone)
        {
            return StartSceneConfigCategory.Instance.GetBySceneName(zone, Enum.GetName(SceneType.EMail)).InstanceId;
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

        /// <summary>
        /// 获取玩家缓存
        /// </summary>
        /// <param name="scene"></param>
        /// <param name="unitId"></param>
        /// <returns></returns>
        public static async ETTask<Unit> GetUnitCache(Scene scene, long unitId)
        {
            long instanceId = DBHelper.GetDbCacheId(scene.DomainZone());
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

        /// <summary>
        /// 删除玩家缓存
        /// </summary>
        /// <param name="unitId"></param>
        public static async ETTask DeleteUnitCache(int zone, long unitId)
        {
            M2D_DeleteUnit message = new M2D_DeleteUnit() { UnitId = unitId };
            long instanceId = DBHelper.GetDbCacheId(zone);
            await MessageHelper.CallActor(instanceId, message);
        }
        
        public static long GetUnitCacheConfig(long unitId)
        {
            int zone = UnitIdStruct.GetUnitZone(unitId);
            return GetDbCacheId(zone);
        }

        /// <summary>
        /// 获取玩家组件缓存
        /// </summary>
        /// <param name="unitId"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static async ETTask<T> GetComponentCache<T>(int zone, long unitId) where T : Entity
        {
            G2D_GetComponent message = new G2D_GetComponent() { UnitId = unitId };
            message.Component = typeof(T).Name;
            long instanceId = DBHelper.GetDbCacheId(zone);
            D2G_GetComponent queryUnit = (D2G_GetComponent)await MessageHelper.CallActor(instanceId, message);
            if (queryUnit.Error == ErrorCode.ERR_Success && queryUnit.Component!=null)
            {
                return queryUnit.Component as T;
            }
            return null;
        }
        
        public static async ETTask SaveComponentCache(int zone, long unitId, Entity entity)
        {
            long dbCacheId = DBHelper.GetDbCacheId(zone);
            D2M_SaveComponent d2GSave = (D2M_SaveComponent)await ActorMessageSenderComponent.Instance.Call(dbCacheId, new M2D_SaveComponent() {
                UnitId = unitId,
                EntityByte =MongoHelper.ToBson(entity),
                ComponentType = entity.GetType().Name
            });
        }
        
        public static async ETTask<T> GetComponent<T>(int zone, long unitId) where T : Entity
        {
            List<T> resulets = await Game.Scene.GetComponent<DBComponent>().Query<T>(zone, d => d.Id == unitId);
            if (resulets == null || resulets.Count == 0)
            {
                return null;
            }

            return resulets[0];
        }
        
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

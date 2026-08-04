using System;
using System.Net;

namespace ET
{
    public static class SceneFactory
    {
        public static Scene Create(Entity parent, string name, SceneType sceneType)
        {
            long instanceId = IdGenerater.Instance.GenerateInstanceId();
            return Create(parent, instanceId, instanceId, parent.DomainZone(), name, sceneType);
        }

        public static Scene Create(Entity parent, long id, long instanceId, int zone, string name, SceneType sceneType, StartSceneConfig startSceneConfig = null)
        {
            var startZoneConfig = StartZoneConfigCategory.Instance.Get(zone);
            Game.Scene.GetComponent<DBComponent>().InitDatabase(startZoneConfig);

            Scene scene = EntitySceneFactory.CreateScene(id, instanceId, zone, sceneType, name, parent);
            scene.AddComponent<MailBoxComponent, MailboxType>(MailboxType.UnOrderMessageDispatcher);
            switch (scene.SceneType)
            {
                case SceneType.LoginCenter:
                    scene.AddComponent<LoginInfoRecordComponent>();
                    scene.AddComponent<WeChatOACodeComponent>();

                    /*if (!ServerHelper.IsBanHaoServer(0))
                    {
                    {
                        scene.AddComponent<HttpComponent, string>($"http://*:80/");
                    }*/
                    //HTTP 协议：默认端口是 80
                    //例如 http://api.example.com 等价于 http://api.example.com:80
                    //HTTPS 协议：默认端口是 443
                    //例如 https://api.example.com 等价于 https://api.example.com:443
                    int tapport = CommonHelper.IsInnerNet() ? CommonConfig.TapHttpIneer : CommonConfig.TapHttpOuter;
                    
                    scene.AddComponent<HttpComponent, string>($"http://*:{tapport}/");
                    scene.AddComponent<NetKcpComponent, IPEndPoint, int>(startSceneConfig.InnerIPOutPort, SessionStreamDispatcherType.SessionStreamDispatcherServerOuter);
                    break;
                case SceneType.Realm:
                    scene.AddComponent<NetKcpComponent, IPEndPoint, int>(startSceneConfig.InnerIPOutPort, SessionStreamDispatcherType.SessionStreamDispatcherServerOuter);
                    scene.AddComponent<PlayerInfoListComponent>();
                    scene.AddComponent<CenterServerComponent>();
                    scene.AddComponent<AccountSessionsComponent>();
                    scene.AddComponent<TokenComponent>();
                    scene.AddComponent<ObjectWait>();
                    break;
                case SceneType.Queue:
                    scene.AddComponent<NetKcpComponent, IPEndPoint, int>(startSceneConfig.InnerIPOutPort, SessionStreamDispatcherType.SessionStreamDispatcherServerOuter);
                    scene.AddComponent<QueueSessionsComponent>();
                    break;
                case SceneType.Gate:
                    scene.AddComponent<NetKcpComponent, IPEndPoint, int>(startSceneConfig.InnerIPOutPort, SessionStreamDispatcherType.SessionStreamDispatcherServerOuter);
                    scene.AddComponent<PlayerComponent>();
                    scene.AddComponent<GateSessionKeyComponent>();
                    break;
                case SceneType.GateMap:
                    scene.AddComponent<UnitComponent>();
                    break;
                case SceneType.Location:
                    scene.AddComponent<LocationComponent>();
                    break;
                case SceneType.DBCache:
                    scene.AddComponent<DBCacheComponent>();
                    break;
                case SceneType.Chat:
                    scene.AddComponent<ChatSceneComponent>();
                    break;
                case SceneType.WZChat:
                    scene.AddComponent<WZChatSceneComponent>();
                    break;
                case SceneType.Mail:
                    scene.AddComponent<MailSceneComponent>();
                    break;
                case SceneType.Activity:
                    scene.AddComponent<ActivitySceneComponent>();
                    break;
                case SceneType.Rank:
                    scene.AddComponent<RankSceneComponent>();
                    break;
                case SceneType.WZRank:
                    scene.AddComponent<WZRankSceneComponent>();
                    break;
                case SceneType.Consign:
                    scene.AddComponent<ConsignSceneComponent>();
                    break;
                case SceneType.Team:
                    scene.AddComponent<TeamSceneComponent>();
                    break;
                case SceneType.Friend:
                    scene.AddComponent<FriendSceneComponent>();
                    break;
                case SceneType.FubenCenter:
                    scene.AddComponent<FubenCenterComponent>();
                    break;
                case SceneType.Union:
                    scene.AddComponent<UnionSceneComponent>();
                    break;
                case SceneType.ReCharge:
                    if (Game.Options.StartConfig.Contains("BanHao"))
                    {
                        ;
                    }
                    else if (Game.Options.StartConfig.Contains("Google"))
                    {
                        scene.AddComponent<ReChargeGoogleComponent>();
                    }
                    else
                    {
                        scene.AddComponent<RechargeSceneComponent>();
                        scene.AddComponent<ReChargeWXComponent>();
                        scene.AddComponent<ReChargeQDComponent>();
                        scene.AddComponent<ReChargeAliComponent>();
                        scene.AddComponent<ReChargeIOSComponent>();
                        scene.AddComponent<ReChargeTikTokComponent>();
                        scene.AddComponent<ReChargeGoogleComponent>();
                    }
                    break;
                case SceneType.JiaYuan:
                    scene.AddComponent<JiaYuanSceneComponent>();
                    break;
                case SceneType.Map:             //野外地图
                    scene.AddComponent<MapComponent>();
                    scene.AddComponent<UnitComponent>();
                    scene.AddComponent<AOIManagerComponent>();
                    scene.AddComponent<NpcComponent>();
                    //scene.AddComponent<RecastPathComponent>();
                    break;
                case SceneType.FubenWork:
                    
                    scene.AddComponent<HappySceneComponent>();
                    scene.AddComponent<BattleSceneComponent>();
                    scene.AddComponent<ArenaSceneComponent>();
                    scene.AddComponent<SoloSceneComponent>();
                    break;
                case SceneType.Popularize:

                    break;
                default:
                    break;  
            }
            return scene;
        }
    }
}
using System.Collections.Generic;
using System;

namespace ET
{

    [ConsoleHandler(ConsoleMode.StopServer)]
    public class StopServerConsoleHandler : IConsoleHandler
    {
#if SERVER
        private static bool stopPending;
        private static readonly HashSet<long> stoppedGates = new HashSet<long>();
        private static readonly Dictionary<long, string> saveStates = new Dictionary<long, string>();

        public static bool IsGateStopped(Scene scene)
        {
            return stoppedGates.Contains(scene.InstanceId);
        }

        public static void StopGate(Scene scene)
        {
            stoppedGates.Add(scene.InstanceId);
        }
#endif

        public async ETTask Run(ModeContex contex, string content)
        {
            contex.Parent.RemoveComponent<ModeContex>();
            await ConsoleHelper.StopServerConsoleHandler(content);
        }

        /// <summary>控制台/GM 共用入口：全服维护只需倒计时分钟数，内部消息使用 0_分钟。</summary>
        public static async ETTask Execute(string content)
        {
            await ETTask.CompletedTask;
#if SERVER
            string[] args = content.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
            if (args.Length != 2 || args[0] != ConsoleMode.StopServer
                || !int.TryParse(args[1], out int minutes) || minutes < 0)
            {
                Log.Console("用法：stopserver 分钟（非负整数；0立即下线，1最多一分钟后下线）。");
                return;
            }

            Other2A_ServerMessageRResponse response = (Other2A_ServerMessageRResponse)await ActorMessageSenderComponent.Instance.Call(
                DBHelper.GetRealmCenter(), new A2Other_ServerMessageRequest
                {
                    MessageType = ServerNoticeType.StopSever,
                    MessageValue = $"0_{minutes}",
                });
            Log.Console(response.Error == ErrorCode.ERR_Success ? response.Message : $"停服指令失败：{response.Error} {response.Message}");
#endif
        }

#if SERVER
        /// <summary>只在账号服启动全服维护：立即禁止新登录，按同一截止时间通知客户端下线并推进存档。</summary>
        public static void StartMaintenance(Scene scene, int minutes, Other2A_ServerMessageRResponse response)
        {
            CenterServerComponent center = scene.GetComponent<CenterServerComponent>();
            if (stopPending)
            {
                response.Message = "已有停服任务，请等待完成。";
                return;
            }

            center.StopServer = true;
            stopPending = true;
            long stopAt = TimeHelper.ServerNow() + minutes * TimeHelper.Minute;
            response.Message = minutes == 0 ? "已进入全服维护，正在执行停服存档。" : $"已进入全服维护，{minutes}分钟后执行停服存档。";
            Log.Console(response.Message);
            StopLater(scene, stopAt).Coroutine();
        }

        /// <summary>先向全部网关发送统一下线截止时间，到期后清理剩余连接，再等待各业务服存档回包。</summary>
        private static async ETTask StopLater(Scene realm, long stopAt)
        {
            try
            {
                StartSceneConfigCategory.Instance.EnsureGameZonesExpanded();
                List<StartSceneConfig> scenes = new List<StartSceneConfig>(StartSceneConfigCategory.Instance.GetAll().Values);
                bool failed = false;
                foreach (StartSceneConfig config in scenes)
                {
                    if (config.Type == SceneType.Gate)
                    {
                        try
                        {
                            await SendStopMessage(config.InstanceId, $"notify_{stopAt}");
                        }
                        catch (Exception e)
                        {
                            failed = true;
                            Log.Error(e);
                        }
                    }
                }

                long remaining = stopAt - TimeHelper.ServerNow();
                if (remaining > 0)
                {
                    await TimerComponent.Instance.WaitAsync(remaining);
                }

                // 使用现有正常下线流程；包括本机连接和机器人，不以远程地址过滤玩家。
                foreach (StartSceneConfig config in scenes)
                {
                    if (config.Type == SceneType.Gate)
                    {
                        failed |= !await SaveAndWait(config);
                    }
                }

                foreach (StartSceneConfig config in scenes)
                {
                    if (config.Type != SceneType.Rank && config.Type != SceneType.WZRank
                        && config.Type != SceneType.Consign && config.Type != SceneType.Union
                        && config.Type != SceneType.Mail && config.Type != SceneType.Activity && config.Type != SceneType.Chat
                        && config.Type != SceneType.DBCache)
                    {
                        continue;
                    }

                    failed |= !await SaveAndWait(config);
                }

                CenterServerComponent center = realm.GetComponent<CenterServerComponent>();
                await DBHelper.SaveComponent(realm.DomainZone(), center.DBCenterSerialInfo.Id, center.DBCenterSerialInfo);
                LogHelper.OnStopServer();
                if (failed)
                {
                    Log.Console("部分服务器停服存档失败，仍保持维护状态；请检查错误后重新执行 stopserver 0。");
                    return;
                }
                Log.Warning("全部停服存档完成，可以关闭服务器进程。服务器仍保持维护状态。");
                Log.Console("全部停服存档完成，可以关闭服务器进程。服务器仍保持维护状态。");
            }
            catch (Exception e)
            {
                Log.Error(e);
                Log.Console("停服流程失败，仍保持维护状态；请检查错误后重新执行 stopserver 0。");
            }
            finally
            {
                stopPending = false;
            }
        }

        /// <summary>存档在目标服后台执行，短请求轮询结果，避免玩家多或拍卖逐条保存时超过 RPC 超时。</summary>
        private static async ETTask<bool> SaveAndWait(StartSceneConfig config)
        {
            // 保留通信地址，同时打印配置中的区服、服务名称和进程，便于确认每个存档节点。
            long actorId = config.InstanceId;
            string serverInfo = $"区服ID={config.Zone} 服务={config.Name} 类型={config.Type} 进程ID={config.Process} 场景ID={config.Id} ActorId={actorId}";
            try
            {
                Other2A_ServerMessageRResponse response = await SendStopMessage(actorId, "save");
                while (response.Message == "saving")
                {
                    await TimerComponent.Instance.WaitAsync(1000);
                    response = await SendStopMessage(actorId, "status");
                }
                if (response.Message != "saved")
                {
                    throw new Exception($"停服存档状态异常：{response.Message}");
                }
                Log.Console($"停服存档完成：{serverInfo}");
                return true;
            }
            catch (Exception e)
            {
                Log.Error($"停服存档失败：{serverInfo}\n{e}");
                return false;
            }
        }

        public static void SaveScene(Scene scene, string action, Other2A_ServerMessageRResponse response)
        {
            saveStates.TryGetValue(scene.InstanceId, out string state);
            if (action != "status" && state != "saving")
            {
                saveStates[scene.InstanceId] = "saving";
                SaveSceneData(scene).Coroutine();
            }
            response.Message = saveStates.TryGetValue(scene.InstanceId, out state) ? state : "missing";
            if (response.Message == "failed")
            {
                response.Error = ErrorCore.ERR_RpcFail;
            }
        }

        /// <summary>网关等玩家正常退出落库；业务服保存各自数据；全部完成后才将状态改为 saved。</summary>
        private static async ETTask SaveSceneData(Scene scene)
        {
            long instanceId = scene.InstanceId;
            try
            {
                switch (scene.SceneType)
                {
                    case SceneType.Gate:
                        StopGate(scene);
                        foreach (Player player in scene.GetComponent<PlayerComponent>().GetAll())
                        {
                            Session client = player.ClientSession;
                            client?.Send(new A2C_Disconnect { Error = LDWord_PromptCategory.Instance.GetWordId(WordPromptKey.Prompt_Login_Stop_Server) });
                            await DisconnectHelper.KickPlayer(player);
                            client?.Dispose();
                        }
                        break;
                    case SceneType.Rank:
                        await scene.GetComponent<RankSceneComponent>().SaveDB();
                        break;
                    case SceneType.WZRank:
                        await scene.GetComponent<WZRankSceneComponent>().SaveDB();
                        break;
                    case SceneType.Consign:
                        await scene.GetComponent<ConsignSceneComponent>().SaveDB();
                        break;
                    case SceneType.Mail:
                        await scene.GetComponent<MailSceneComponent>().SaveDB();
                        break;
                    case SceneType.Chat:
                        await scene.GetComponent<ChatSceneComponent>().SaveMarquee();
                        break;
                    case SceneType.Activity:
                        ActivitySceneComponent activity = scene.GetComponent<ActivitySceneComponent>();
                        await DBHelper.SaveComponent(scene.DomainZone(), activity.DBDayActivityInfo.Id, activity.DBDayActivityInfo);
                        break;
                    case SceneType.Union:
                        UnionSceneComponent unions = scene.GetComponent<UnionSceneComponent>();
                        await DBHelper.SaveComponent(scene.DomainZone(), unions.DBUnionManager.Id, unions.DBUnionManager);
                        foreach (long unionId in new List<long>(unions.DBUnionInfos.Keys))
                        {
                            using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.UnionOperate, unionId))
                            {
                                await DBHelper.SaveComponent(scene.DomainZone(), unionId, unions.DBUnionInfos[unionId]);
                            }
                        }
                        break;
                    case SceneType.DBCache:
                        // 最后等待此进程已经排队的数据库写入，玩家最终存档已由网关逐一等待。
                        List<CoroutineLock> locks = new List<CoroutineLock>();
                        try
                        {
                            for (int key = 1 - DBComponent.TaskCount; key < DBComponent.TaskCount; key++)
                            {
                                locks.Add(await CoroutineLockComponent.Instance.Wait(CoroutineLockType.DB, key, 0));
                            }
                        }
                        finally
                        {
                            foreach (CoroutineLock coroutineLock in locks)
                            {
                                coroutineLock.Dispose();
                            }
                        }
                        break;
                }
                saveStates[instanceId] = "saved";
            }
            catch (Exception e)
            {
                saveStates[instanceId] = "failed";
                Log.Error(e);
            }
        }

        private static async ETTask<Other2A_ServerMessageRResponse> SendStopMessage(long actorId, string value)
        {
            Other2A_ServerMessageRResponse response = (Other2A_ServerMessageRResponse)await ActorMessageSenderComponent.Instance.Call(
                actorId, new A2Other_ServerMessageRequest { MessageType = ServerNoticeType.StopSever, MessageValue = value });
            if (response.Error != ErrorCode.ERR_Success)
            {
                throw new Exception($"停服处理失败：ActorId={actorId} Error={response.Error}");
            }
            return response;
        }
#endif
    }
}

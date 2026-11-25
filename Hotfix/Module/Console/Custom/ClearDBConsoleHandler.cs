#if SERVER
using System;
using System.Collections.Generic;
using System.Net;
using MongoDB.Bson;
using MongoDB.Driver;

namespace ET
{
    [ConsoleHandler(ClearDBConsoleHandler.Command)]
    public class ClearDBConsoleHandler : IConsoleHandler
    {
        public const string Command = "cleardb";

        /// <summary>
        /// 内网或版号服清档：检查全部网关和数据库，等待在途数据库操作完成，再次检查玩家后同步删库并退出。
        /// 仅支持全部业务场景位于当前进程，删除期间不让出主线程，避免登录和旧缓存回写。
        /// </summary>
        public async ETTask Run(ModeContex contex, string content)
        {
            contex.Parent.RemoveComponent<ModeContex>();
            if (content.Trim() != Command)
            {
                Log.Console("用法：cleardb（清除当前启动配置的全部项目数据库，完成后关闭服务，请重新启动）");
                return;
            }

            // 版号服使用公网对外地址，允许该启动配置清档；数据库仍须通过下面的内网地址检查。
            bool isBanHao = Game.Options.StartConfig.Replace('\\', '/').TrimEnd('/')
                .Equals("StartConfig/BanHao", StringComparison.OrdinalIgnoreCase);
            if (!isBanHao && (!CommonHelper.IsInnerNet() || !IsInnerAddress(StartMachineConfigCategory.Instance.Get(1).OuterIP)))
            {
                Log.Console("拒绝清库：只能在内网或 StartConfig/BanHao 启动配置下使用。");
                return;
            }

            if (Game.Options.AppType != AppType.Server || !CheckPlayers())
            {
                Log.Console("拒绝清库：请在承载全部业务场景且没有玩家的服务端控制台执行。");
                return;
            }

            List<IMongoDatabase> databases = new List<IMongoDatabase>();
            HashSet<string> targets = new HashSet<string>(StringComparer.Ordinal);
            foreach (StartZoneConfig config in StartZoneConfigCategory.Instance.GetAll().Values)
            {
                if (string.IsNullOrWhiteSpace(config.DBConnection) || string.IsNullOrWhiteSpace(config.DBName)
                    || config.DBName.Equals("admin", StringComparison.OrdinalIgnoreCase)
                    || config.DBName.Equals("config", StringComparison.OrdinalIgnoreCase)
                    || config.DBName.Equals("local", StringComparison.OrdinalIgnoreCase))
                {
                    Log.Console($"拒绝清库：Zone={config.Id} 的数据库配置无效或指向系统库。");
                    return;
                }

                try
                {
                    MongoUrl url = new MongoUrl(config.DBConnection);
                    foreach (MongoServerAddress server in url.Servers)
                    {
                        if (!IsInnerAddress(server.Host))
                        {
                            Log.Console($"拒绝清库：Zone={config.Id} 的数据库地址不是内网地址。");
                            return;
                        }
                    }

                    if (targets.Add(url.ToString() + "\n" + config.DBName))
                    {
                        databases.Add(new MongoClient(url).GetDatabase(config.DBName));
                    }
                }
                catch (Exception)
                {
                    // 不输出连接串或异常内容，避免泄露数据库凭据。
                    Log.Console($"拒绝清库：Zone={config.Id} 的数据库配置无法解析。");
                    return;
                }
            }

            if (databases.Count == 0)
            {
                Log.Console("拒绝清库：没有配置项目数据库。");
                return;
            }

            List<CoroutineLock> locks = new List<CoroutineLock>();
            bool deleting = false;
            int deleted = 0;
            try
            {
                foreach (IMongoDatabase database in databases)
                {
                    await database.RunCommandAsync<BsonDocument>(new BsonDocument("ping", 1));
                }

                // DB 操作以 Id % TaskCount 排队，随机 Id 可能为负；覆盖正负键并禁用持锁超时。
                for (int key = 1 - DBComponent.TaskCount; key < DBComponent.TaskCount; key++)
                {
                    locks.Add(await CoroutineLockComponent.Instance.Wait(CoroutineLockType.DB, key, 0));
                }

                // 上面的 await 期间仍可能有人登录，必须在最后一次让出主线程后重新检查。
                if (!CheckPlayers())
                {
                    Log.Console("拒绝清库：准备期间出现玩家或业务场景状态不完整。");
                    return;
                }

                deleting = true;
                foreach (IMongoDatabase database in databases)
                {
                    database.Client.DropDatabase(database.DatabaseNamespace.DatabaseName);
                    deleted++;
                    Log.Console($"已删除数据库：{database.DatabaseNamespace.DatabaseName}");
                }

                Log.Console($"清库完成：共 {deleted} 个数据库。服务即将关闭，请重新启动。");
                Environment.Exit(0);
            }
            catch (Exception)
            {
                if (deleting)
                {
                    Log.Console($"清库失败：已完成 {deleted}/{databases.Count} 个数据库，可能已部分删除。服务即将关闭，请检查数据库后重试。");
                    Environment.Exit(1);
                }

                Log.Console("拒绝清库：数据库检查或等待在途操作失败，尚未执行删库。");
            }
            finally
            {
                foreach (CoroutineLock coroutineLock in locks)
                {
                    coroutineLock.Dispose();
                }
            }
        }

        /// <summary>检查所有配置业务场景都在本进程且已创建；网关有任何玩家记录均拒绝，不排除本机或机器人。</summary>
        private static bool CheckPlayers()
        {
            StartSceneConfigCategory.Instance.EnsureGameZonesExpanded();
            int gates = 0;
            foreach (StartSceneConfig config in StartSceneConfigCategory.Instance.GetAll().Values)
            {
                if (config.Type == SceneType.Robot)
                {
                    continue;
                }

                if (config.Process != Game.Options.Process)
                {
                    Log.Console($"拒绝清库：业务场景 {config.Name} 位于其他进程，无法保证停止写库。");
                    return false;
                }

                Scene scene = Game.Scene.GetChild<Scene>(config.Id);
                if (scene == null || scene.IsDisposed || scene.SceneType != config.Type)
                {
                    Log.Console($"拒绝清库：业务场景 {config.Id} 尚未就绪，无法确认在线状态。");
                    return false;
                }

                if (config.Type != SceneType.Gate)
                {
                    continue;
                }

                gates++;
                PlayerComponent players = scene.GetComponent<PlayerComponent>();
                if (players == null || players.Count != 0)
                {
                    Log.Console($"拒绝清库：Zone={config.Zone} 网关仍有玩家记录或在线状态不可用。");
                    return false;
                }
            }

            return gates > 0;
        }

        private static bool IsInnerAddress(string host)
        {
            if (host.Equals("localhost", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (!IPAddress.TryParse(host, out IPAddress address))
            {
                return false;
            }

            if (address.IsIPv4MappedToIPv6)
            {
                address = address.MapToIPv4();
            }

            if (IPAddress.IsLoopback(address))
            {
                return true;
            }

            byte[] bytes = address.GetAddressBytes();
            return bytes.Length == 4 && (bytes[0] == 10
                || bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31
                || bytes[0] == 192 && bytes[1] == 168);
        }
    }
}
#endif

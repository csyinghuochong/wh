using CommandLine;
using NLog;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace ET
{
    internal static class Program
	{
		private static void Main(string[] args)
		{
			// 关闭 Quick Edit，避免点控制台窗口卡住进程
			WindowsConsoleHelper.DisableQuickEdit();

			AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
			{
				Log.Error(e.ExceptionObject.ToString());
			};
			
			ETTask.ExceptionHandler += Log.Error;

			// 异步方法全部会回掉到主线程
			SynchronizationContext.SetSynchronizationContext(ThreadSynchronizationContext.Instance);
			
			try
			{		
				Game.EventSystem.Add(typeof(Game).Assembly);
				Game.EventSystem.Add(DllHelper.GetHotfixAssembly());
				
				ProtobufHelper.Init();
				MongoRegister.Init();

                if (args.Length == 0)
                {
                    args = new string[5];
                    args[0] = "--AppType=Server";
                    args[1] = "--Console=1";
                    args[2] = "--Process=1";			
                    args[3] = "--StartConfig=StartConfig/Localhost";
                    args[4] = "--Title=Server";
                }

                // 命令行参数
                Options options = null;
				Parser.Default.ParseArguments<Options>(args)
						.WithNotParsed(error => throw new Exception($"命令行格式错误!"))
						.WithParsed(o => { options = o; });

				Options.Instance = options;

				// 仅内网/版号：StartConfig 含 Localhost 或 BanHao 时清理旧 Server 日志
				ClearServerLogFilesIfInner();

				Log.ILog = new NLogger(Game.Options.AppType.ToString());

				LogManager.Configuration.Variables["appIdFormat"] = $"{Game.Options.Process:000000}";

				Process process = Process.GetCurrentProcess();
                Console.Title = options.Title + "_" + process.Id;

                Log.Info($"server start........................ {Game.Scene.Id}");

				Game.EventSystem.Publish(new EventType.AppStart());
				
				while (true)
				{
					try
					{
						Thread.Sleep(1);
						Game.Update();
						Game.LateUpdate();
						Game.FrameFinish();
					}
					catch (Exception e)
					{
						Log.Error(e);
                    }
				}
			}
			catch (Exception e)
			{
				Log.Error(e);
			}
		}

		/// <summary>
		/// 仅 Localhost / BanHao 环境清理 Server. 日志（此时 CommonHelper.IsInnerNet 配置尚未加载）。
		/// </summary>
		private static void ClearServerLogFilesIfInner()
		{
			string startConfig = Options.Instance?.StartConfig ?? string.Empty;
			if (startConfig.IndexOf("Localhost", StringComparison.OrdinalIgnoreCase) < 0
			    && startConfig.IndexOf("BanHao", StringComparison.OrdinalIgnoreCase) < 0)
			{
				return;
			}

			try
			{
				string logsDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "Logs"));
				if (!Directory.Exists(logsDir))
				{
					return;
				}

				foreach (string file in Directory.GetFiles(logsDir, "Server.*"))
				{
					try
					{
						File.Delete(file);
					}
					catch
					{
						// 单个文件占用则跳过，不影响启动
					}
				}
			}
			catch
			{
				// 清理失败不影响启动
			}
		}
	}
}

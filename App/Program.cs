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

				// Localhost/BanHao 删除旧 Server 日志；Beta 按分钟归档到 Logs/yyyyMMddHHmm
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
		/// Localhost / BanHao：删除 Logs 根目录下的 Server.*。
		/// Beta：移到 Logs/yyyyMMddHHmm/（分钟）。此时 CommonHelper.IsInnerNet 尚未加载。
		/// </summary>
		private static void ClearServerLogFilesIfInner()
		{
			string startConfig = Options.Instance?.StartConfig ?? string.Empty;
			bool isBeta = startConfig.IndexOf("Beta", StringComparison.OrdinalIgnoreCase) >= 0;
			bool clearOld = startConfig.IndexOf("Localhost", StringComparison.OrdinalIgnoreCase) >= 0
			    || startConfig.IndexOf("BanHao", StringComparison.OrdinalIgnoreCase) >= 0;
			if (!isBeta && !clearOld)
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

				string[] files = Directory.GetFiles(logsDir, "Server.*");
				if (isBeta)
				{
					ArchiveServerLogs(logsDir, files);
					return;
				}

				foreach (string file in files)
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

		private static void ArchiveServerLogs(string logsDir, string[] files)
		{
			if (files.Length == 0)
			{
				return;
			}

			string archiveDir = Path.Combine(logsDir, DateTime.Now.ToString("yyyyMMddHHmm"));
			Directory.CreateDirectory(archiveDir);
			foreach (string file in files)
			{
				try
				{
					string dest = Path.Combine(archiveDir, Path.GetFileName(file));
					if (File.Exists(dest))
					{
						File.Delete(dest);
					}

					File.Move(file, dest);
				}
				catch
				{
					// 单个文件占用则跳过，不影响启动
				}
			}
		}
	}
}

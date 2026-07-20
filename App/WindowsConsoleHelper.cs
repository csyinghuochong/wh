using System;
using System.Runtime.InteropServices;

namespace ET
{
    /// <summary>
    /// Windows 控制台：关闭 Quick Edit，避免鼠标点选窗口导致进程假死（需回车才继续）。
    /// </summary>
    public static class WindowsConsoleHelper
    {
        private const int STD_INPUT_HANDLE = -10;
        private const uint ENABLE_QUICK_EDIT_MODE = 0x0040;
        private const uint ENABLE_EXTENDED_FLAGS = 0x0080;

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

        public static void DisableQuickEdit()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return;
            }

            try
            {
                IntPtr handle = GetStdHandle(STD_INPUT_HANDLE);
                if (handle == IntPtr.Zero || handle == new IntPtr(-1))
                {
                    return;
                }

                if (!GetConsoleMode(handle, out uint mode))
                {
                    return;
                }

                mode &= ~ENABLE_QUICK_EDIT_MODE;
                mode |= ENABLE_EXTENDED_FLAGS;
                SetConsoleMode(handle, mode);
            }
            catch
            {
                // 忽略：无控制台或非 Windows 环境
            }
        }
    }
}

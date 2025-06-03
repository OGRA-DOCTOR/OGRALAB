using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace OGRALAB.Services
{
    public static class SingleInstanceService
    {
        private static Mutex? _mutex;
        private const string MUTEX_NAME = "OGRALAB_SingleInstance_Mutex";

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private const int SW_RESTORE = 9;

        public static bool IsFirstInstance()
        {
            _mutex = new Mutex(true, MUTEX_NAME, out bool createdNew);
            return createdNew;
        }

        public static void BringExistingInstanceToFront()
        {
            var currentProcess = Process.GetCurrentProcess();
            var processes = Process.GetProcessesByName(currentProcess.ProcessName);

            foreach (var process in processes)
            {
                if (process.Id != currentProcess.Id && !string.IsNullOrEmpty(process.MainWindowTitle))
                {
                    ShowWindow(process.MainWindowHandle, SW_RESTORE);
                    SetForegroundWindow(process.MainWindowHandle);
                    break;
                }
            }
        }

        public static void ReleaseMutex()
        {
            _mutex?.ReleaseMutex();
            _mutex?.Dispose();
        }
    }
}

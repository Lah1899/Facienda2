using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;

namespace Facienda2
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application
    {
        private static System.Threading.Mutex _Mutex; // 排他制御

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private const int SW_RESTORE = 9;

        protected override void OnStartup(StartupEventArgs e)
        {
            bool createdNew;

            _Mutex = new System.Threading.Mutex(false, "Facienda2", out createdNew);

            if (!createdNew)
            {
                // 起動中のプロセスを探す
                var current = Process.GetCurrentProcess();
                var other = Process
                    .GetProcessesByName(current.ProcessName)
                    .FirstOrDefault(p => p.Id != current.Id);

                if (other != null)
                {
                    var hWind = other.MainWindowHandle;
                    if (hWind != IntPtr.Zero)
                    {
                        // 最小化されていたら最前面に出す
                        ShowWindow(hWind, SW_RESTORE);
                        SetForegroundWindow(hWind);
                    }
                }
                // Mutexの作成に失敗しているのでプロセスを落とす
                Shutdown();
                return;
            }
            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _Mutex?.Dispose();
            base.OnExit(e);
        }
    }
}

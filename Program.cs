using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace AdminAppLauncher
{
    static class Program
    {
        private const string MutexName = "AdminAppLauncher_SingleInstance";
        private static Mutex _mutex;

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private const int SW_RESTORE = 9;

        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            bool configMode = args.Length > 0 && args[0] == "--config";

            if (!configMode && args.Length > 0 && File.Exists(args[0]))
            {
                LaunchFilesFromArgs(args);
                return;
            }

            bool createdNew;
            _mutex = new Mutex(true, MutexName, out createdNew);

            if (!createdNew)
            {
                BringExistingInstanceToForeground();
                return;
            }

            if (configMode || !ConfigManager.ConfigExists())
            {
                AppConfig existing = LoadExistingOrNew();

                ConfigForm configForm = new ConfigForm(existing);
                DialogResult result = configForm.ShowDialog();
                configForm.Dispose();

                if (result == DialogResult.OK)
                {
                    ShowMainForm();
                }
                return;
            }

            try
            {
                AppConfig config = ConfigManager.Load();

                if (config.Applications == null || config.Applications.Count == 0)
                {
                    AppConfig existing = new AppConfig();
                    existing.Credentials = config.Credentials;

                    ConfigForm configForm = new ConfigForm(existing);
                    DialogResult result = configForm.ShowDialog();
                    configForm.Dispose();

                    if (result == DialogResult.OK)
                    {
                        ShowMainForm();
                    }
                    return;
                }

                Application.Run(new MainForm(config));
            }
            catch (Exception ex)
            {
                MessageBox.Show("加载配置失败:\n\n" + ex.Message +
                    "\n\n请重新运行本程序进行配置。", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static void BringExistingInstanceToForeground()
        {
            try
            {
                Process current = Process.GetCurrentProcess();
                Process[] processes = Process.GetProcessesByName(current.ProcessName);
                foreach (Process p in processes)
                {
                    if (p.Id != current.Id && p.MainWindowHandle != IntPtr.Zero)
                    {
                        ShowWindow(p.MainWindowHandle, SW_RESTORE);
                        SetForegroundWindow(p.MainWindowHandle);
                        break;
                    }
                }
            }
            catch { }
        }

        private static void LaunchFilesFromArgs(string[] files)
        {
            if (!ConfigManager.ConfigExists())
            {
                MessageBox.Show("管理员凭据尚未配置，请先运行本程序完成配置。",
                    "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                AppConfig config = ConfigManager.Load();
                foreach (string file in files)
                {
                    if (!File.Exists(file))
                        continue;

                    try
                    {
                        ProcessLauncher.LaunchFile(config, file);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(
                            "以管理员身份启动「" + Path.GetFileName(file) + "」失败:\n\n" + ex.Message,
                            "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("加载配置失败:\n\n" + ex.Message,
                    "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static AppConfig LoadExistingOrNew()
        {
            if (ConfigManager.ConfigExists())
            {
                try
                {
                    return ConfigManager.Load();
                }
                catch
                {
                    return new AppConfig();
                }
            }
            return new AppConfig();
        }

        private static void ShowMainForm()
        {
            try
            {
                AppConfig config = ConfigManager.Load();
                Application.Run(new MainForm(config));
            }
            catch (Exception ex)
            {
                MessageBox.Show("加载配置失败:\n\n" + ex.Message +
                    "\n\n请重新运行本程序进行配置。", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}

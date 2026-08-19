using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace AdminAppLauncher
{
    public static class ProcessLauncher
    {
        private const uint LOGON_WITH_PROFILE = 0x00000001;
        private const uint CREATE_UNICODE_ENVIRONMENT = 0x00000400;

        public const string DataFolder = @"C:\ProgramData\AdminAppLauncher";

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool CreateProcessWithLogonW(
            string lpUsername,
            string lpDomain,
            string lpPassword,
            uint dwLogonFlags,
            string lpApplicationName,
            string lpCommandLine,
            uint dwCreationFlags,
            IntPtr lpEnvironment,
            string lpCurrentDirectory,
            ref STARTUPINFO lpStartupInfo,
            out PROCESS_INFORMATION lpProcessInformation);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr hObject);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct STARTUPINFO
        {
            public int cb;
            public string lpReserved;
            public string lpDesktop;
            public string lpTitle;
            public uint dwX;
            public uint dwY;
            public uint dwXSize;
            public uint dwYSize;
            public uint dwXCountChars;
            public uint dwYCountChars;
            public uint dwFillAttribute;
            public uint dwFlags;
            public short wShowWindow;
            public short cbReserved2;
            public IntPtr lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public uint dwProcessId;
            public uint dwThreadId;
        }

        public static void Launch(AppConfig config, AppEntry app)
        {
            string workingDir;
            if (string.IsNullOrEmpty(app.WorkingDirectory))
                workingDir = Path.GetDirectoryName(app.Path);
            else
                workingDir = app.WorkingDirectory;

            if (string.IsNullOrEmpty(workingDir) || !Directory.Exists(workingDir))
                workingDir = null;

            string domain;
            if (string.IsNullOrEmpty(config.Credentials.Domain))
                domain = ".";
            else
                domain = config.Credentials.Domain;

            if (ShouldAlwaysElevate(app))
            {
                LaunchViaPowerShellElevation(config, app, workingDir, domain);
                return;
            }

            int directError;
            if (TryDirectLaunch(config, app, workingDir, domain, out directError))
                return;

            try
            {
                LaunchViaPowerShellElevation(config, app, workingDir, domain);
                return;
            }
            catch (Exception exPs)
            {
                throw new Exception(
                    "直接启动失败: " + GetFriendlyErrorMessage(directError) +
                    "\n提权启动也失败: " + exPs.Message +
                    "\n\n目标路径: " + app.Path,
                    new Win32Exception(directError));
            }
        }

        private static bool ShouldAlwaysElevate(AppEntry app)
        {
            if (string.IsNullOrEmpty(app.Path))
                return false;

            string ext = Path.GetExtension(app.Path).ToLowerInvariant();
            if (ext == ".bat" || ext == ".cmd" || ext == ".ps1")
                return true;

            string fileName = Path.GetFileName(app.Path).ToLowerInvariant();
            return fileName == "powershell.exe" || fileName == "cmd.exe";
        }

        private static bool TryDirectLaunch(AppConfig config, AppEntry app, string workingDir, string domain, out int win32Error)
        {
            string commandLine = "\"" + app.Path + "\"";
            if (!string.IsNullOrEmpty(app.Arguments))
                commandLine += " " + app.Arguments;

            STARTUPINFO si = new STARTUPINFO();
            si.cb = Marshal.SizeOf(typeof(STARTUPINFO));

            PROCESS_INFORMATION pi;
            bool success = CreateProcessWithLogonW(
                config.Credentials.Username,
                domain,
                config.Credentials.Password,
                LOGON_WITH_PROFILE,
                app.Path,
                commandLine,
                CREATE_UNICODE_ENVIRONMENT,
                IntPtr.Zero,
                workingDir,
                ref si,
                out pi);

            win32Error = Marshal.GetLastWin32Error();

            if (success)
            {
                CloseHandle(pi.hProcess);
                CloseHandle(pi.hThread);
                return true;
            }
            return false;
        }

        private static void LaunchViaPowerShellElevation(AppConfig config, AppEntry app, string workingDir, string domain)
        {
            string sysPath = Environment.GetFolderPath(Environment.SpecialFolder.System);
            string psExe = Path.Combine(sysPath, "WindowsPowerShell", "v1.0", "powershell.exe");

            if (!Directory.Exists(DataFolder))
                Directory.CreateDirectory(DataFolder);

            string guid = Guid.NewGuid().ToString("N").Substring(0, 8);
            string innerScriptPath = Path.Combine(DataFolder, "inner_" + guid + ".ps1");
            string outerScriptPath = Path.Combine(DataFolder, "outer_" + guid + ".ps1");

            // Inner script: start the target app (runs in elevated PowerShell, no -Verb RunAs needed)
            StringBuilder innerScript = new StringBuilder();
            innerScript.Append("Start-Process -FilePath '");
            innerScript.Append(app.Path.Replace("'", "''"));
            innerScript.Append("'");
            if (!string.IsNullOrEmpty(app.Arguments))
            {
                innerScript.Append(" -ArgumentList '");
                innerScript.Append(app.Arguments.Replace("'", "''"));
                innerScript.Append("'");
            }
            if (!string.IsNullOrEmpty(workingDir))
            {
                innerScript.Append(" -WorkingDirectory '");
                innerScript.Append(workingDir.Replace("'", "''"));
                innerScript.Append("'");
            }

            File.WriteAllText(innerScriptPath, innerScript.ToString(), Encoding.UTF8);

            // Outer script: start an elevated PowerShell that runs the inner script
            // Key: -Verb RunAs on powershell.exe (always accessible in System32), NOT on the target app
            StringBuilder outerScript = new StringBuilder();
            outerScript.Append("Start-Process -FilePath '");
            outerScript.Append(psExe.Replace("'", "''"));
            outerScript.Append("' -Verb RunAs -ArgumentList @('-NoProfile', '-ExecutionPolicy', 'Bypass', '-WindowStyle', 'Hidden', '-File', '");
            outerScript.Append(innerScriptPath.Replace("'", "''"));
            outerScript.Append("')");

            File.WriteAllText(outerScriptPath, outerScript.ToString(), Encoding.UTF8);

            string commandLine = "\"" + psExe + "\" -NoProfile -ExecutionPolicy Bypass -WindowStyle Hidden -File \"" + outerScriptPath + "\"";

            STARTUPINFO si = new STARTUPINFO();
            si.cb = Marshal.SizeOf(typeof(STARTUPINFO));

            PROCESS_INFORMATION pi;
            bool success = CreateProcessWithLogonW(
                config.Credentials.Username,
                domain,
                config.Credentials.Password,
                LOGON_WITH_PROFILE,
                psExe,
                commandLine,
                CREATE_UNICODE_ENVIRONMENT,
                IntPtr.Zero,
                null,
                ref si,
                out pi);

            if (!success)
            {
                int error = Marshal.GetLastWin32Error();
                try { File.Delete(innerScriptPath); } catch { }
                try { File.Delete(outerScriptPath); } catch { }
                throw new Exception(GetFriendlyErrorMessage(error), new Win32Exception(error));
            }

            CloseHandle(pi.hProcess);
            CloseHandle(pi.hThread);

            new System.Threading.Thread(() => {
                System.Threading.Thread.Sleep(5000);
                try { File.Delete(innerScriptPath); } catch { }
                try { File.Delete(outerScriptPath); } catch { }
            }) { IsBackground = true }.Start();
        }

        public static void LaunchFile(AppConfig config, string filePath)
        {
            string ext = Path.GetExtension(filePath).ToLowerInvariant();
            string sys = Environment.GetFolderPath(Environment.SpecialFolder.System);

            AppEntry tempApp = new AppEntry();
            tempApp.Name = Path.GetFileNameWithoutExtension(filePath);

            if (ext == ".lnk")
            {
                tempApp = ResolveShortcut(filePath);
            }
            else if (ext == ".msi")
            {
                tempApp.Path = sys + "\\msiexec.exe";
                tempApp.Arguments = "/i \"" + filePath + "\"";
            }
            else if (ext == ".bat" || ext == ".cmd")
            {
                tempApp.Path = sys + "\\cmd.exe";
                tempApp.Arguments = "/c \"" + filePath + "\"";
            }
            else if (ext == ".ps1")
            {
                tempApp.Path = sys + "\\WindowsPowerShell\\v1.0\\powershell.exe";
                tempApp.Arguments = "-ExecutionPolicy Bypass -File \"" + filePath + "\"";
            }
            else
            {
                tempApp.Path = filePath;
            }

            Launch(config, tempApp);
        }

        private static AppEntry ResolveShortcut(string lnkPath)
        {
            AppEntry app = new AppEntry();
            app.Name = Path.GetFileNameWithoutExtension(lnkPath);

            Type shellType = Type.GetTypeFromProgID("WScript.Shell");
            dynamic shell = Activator.CreateInstance(shellType);
            dynamic shortcut = shell.CreateShortcut(lnkPath);

            string targetPath = (string)shortcut.TargetPath;
            if (!string.IsNullOrEmpty(targetPath))
            {
                targetPath = targetPath.Trim().Trim('"');
                targetPath = Environment.ExpandEnvironmentVariables(targetPath);
            }

            app.Path = targetPath;
            app.Arguments = (string)shortcut.Arguments;

            string workDir = (string)shortcut.WorkingDirectory;
            if (!string.IsNullOrEmpty(workDir))
            {
                workDir = workDir.Trim().Trim('"');
                workDir = Environment.ExpandEnvironmentVariables(workDir);
            }
            app.WorkingDirectory = workDir;

            if (string.IsNullOrEmpty(app.Path))
                throw new Exception("无法解析快捷方式的目标路径，快捷方式可能已损坏。");

            return app;
        }

        private static string GetFriendlyErrorMessage(int errorCode)
        {
            switch (errorCode)
            {
                case 2:
                    return "找不到指定的程序文件，请检查路径是否正确。";
                case 5:
                    return "访问被拒绝，目标程序可能需要不同权限。";
                case 267:
                    return "目录名称无效，请检查程序路径和工作目录是否正确。";
                case 50:
                    return "请求不支持。";
                case 1223:
                    return "用户取消了提权请求。";
                case 1326:
                    return "登录失败：用户名或密码错误。";
                case 1327:
                    return "账户限制：此用户当前无法登录（可能密码为空或策略限制）。";
                case 1328:
                    return "登录失败：密码过期。";
                case 1385:
                    return "该账户未被授予在此计算机上以 Service 方式登录的权限。";
                case 1793:
                    return "账户已过期。";
                case 19333:
                    return "密码已过期，需要更改。";
                default:
                    Win32Exception ex = new Win32Exception(errorCode);
                    return "启动失败 (错误代码 " + errorCode + "): " + ex.Message;
            }
        }
    }
}

@echo off
chcp 65001 >nul 2>&1
setlocal

set CSC=C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe
set OUT=AdminAppLauncher.exe
set REFS=/r:System.dll /r:System.Windows.Forms.dll /r:System.Drawing.dll /r:System.Security.dll /r:System.Xml.dll /r:Microsoft.CSharp.dll
set SRCS=Program.cs ConfigModel.cs ConfigManager.cs ProcessLauncher.cs MainForm.cs ConfigForm.cs AppEditForm.cs AssemblyInfo.cs

echo ============================================
echo   AdminAppLauncher 编译脚本
echo ============================================
echo.

if not exist "%CSC%" (
    echo [错误] 找不到 C# 编译器: %CSC%
    echo 请确认 .NET Framework 4.x 已安装。
    goto :end
)

echo 正在编译 %OUT% ...
echo.

"%CSC%" /nologo /warn:4 /checked /codepage:65001 /target:winexe /win32icon:app.ico /out:%OUT% %REFS% %SRCS%

if %ERRORLEVEL% == 0 (
    echo.
    echo ============================================
    echo   编译成功！
    ============================================
    echo.
    echo   输出文件: %~dp0%OUT%
    echo.
    echo   使用方法:
    echo     1. 首次运行: 双击 %OUT%，会自动进入配置界面
    echo     2. 修改配置: 在命令行运行 "%OUT% --config"
    echo     3. 日常使用: 双击 %OUT% 即可看到应用列表
    echo.
    echo   配置文件: config.xml (与 EXE 同目录)
    echo   密码通过 Windows DPAPI 加密存储
) else (
    echo.
    echo ============================================
    echo   编译失败！错误代码: %ERRORLEVEL%
    echo ============================================
)

:end
echo.
pause
endlocal

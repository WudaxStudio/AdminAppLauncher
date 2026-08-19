# AdminAppLauncher

管理员应用启动器 - 让普通用户以管理员身份启动指定应用程序。

## 功能

- 预置管理员凭据（密码通过 Windows DPAPI 加密存储，仅限本机使用）
- 主界面一键启动已配置的管理员应用
- 支持拖拽文件临时以管理员身份运行
- 支持命令行参数直接启动文件
- 支持快捷方式（.lnk）自动解析目标路径
- 支持批处理（.bat/.cmd）和 PowerShell 脚本（.ps1）
- 单实例运行，重复打开自动激活已有窗口

## 启动方式

1. **直接启动**（无弹窗）：通过 `CreateProcessWithLogonW` 以管理员身份直接启动，类似 CPAU
2. **UAC 提权**（弹窗）：直接启动失败时，通过提升后的 PowerShell 启动目标程序

## 编译

```batch
build.bat
```

需要 .NET Framework 4.x 和 C# 编译器（csc.exe）。

## 配置

- 配置文件：`config.xml`（与 EXE 同目录）
- 密码通过 Windows DPAPI（LocalMachine 作用域）加密存储

## 作者

- 开发人员：Wudax
- 版本：V1.1.2

## 免责声明

本软件仅用于测试使用，切勿非法使用！

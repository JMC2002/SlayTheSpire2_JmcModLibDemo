using Godot;
using HarmonyLib;
using JmcModLib.UI.PauseMenu;
using JmcModLibDemo.Core;
using JmcModLib.Utils;
using MegaCrit.Sts2.Core.Modding;
using System.Reflection;
using ModVersionInfo = JmcModLibDemo.Core.VersionInfo;

namespace JmcModLibDemo;

[ModInitializer(nameof(Initialize))]
public partial class MainFile : Node
{
    public static void Initialize()
    {
        JmcModLib.Core.ModRegistry.Register<MainFile>(true)?
            .RegisterButton(
                out _,
                "手动注册按钮",
                DemoSettings.RunManualButton,
                "执行",
                group: DemoSettings.ButtonGroup,
                storageKey: "button.manual",
                helpText: "这个按钮用 RegisterButton 手动注册，用来展示非 Attribute 注册入口也能进入设置 UI。",
                order: 5)
            .RegisterPauseMenuButton(
                key: DemoPauseMenuButtons.ManualButtonKey,
                text: "暂停菜单手动按钮",
                action: DemoPauseMenuButtons.RunManualPauseMenuButton,
                order: 20,
                anchor: PauseMenuButtonAnchor.BeforeExitActions,
                locTable: DemoPauseMenuButtons.LocTable,
                textKey: DemoPauseMenuButtons.ManualButtonTextKey,
                enabledWhen: DemoPauseMenuButtons.CanUseManualPauseMenuButton)
            .Done();

        Harmony harmony = new(ModVersionInfo.Name);
        harmony.PatchAll(Assembly.GetExecutingAssembly());
        ModLogger.Info("Demo 调试补丁已应用。");

        DemoSettings.ApplyStartupDebugSettings();

        ModLogger.Info("======================================");
        ModLogger.Info("JmcModLib Demo Mod 正在启动...");
        ModLogger.Info("这个 MOD 只用于展示 JmcModLib 设置 UI 和配置扫描用法。");
        ModLogger.Info("======================================");
    }
}

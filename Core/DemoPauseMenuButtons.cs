using JmcModLib.UI.PauseMenu;
using JmcModLib.Utils;

namespace JmcModLibDemo.Core;

internal static class DemoPauseMenuButtons
{
    internal const string LocTable = "settings_ui";
    internal const string AttributeButtonKey = "pause_menu.attribute_status";
    internal const string ManualButtonKey = "pause_menu.manual_status";
    internal const string AttributeButtonTextKey = "EXTENSION.JMCMODLIB.PAUSE_MENU.JmcModLibDemo.attribute_status.TEXT";
    internal const string ManualButtonTextKey = "EXTENSION.JMCMODLIB.PAUSE_MENU.JmcModLibDemo.manual_status.TEXT";
    private static int pauseMenuButtonClickCount;

    [PauseMenuButton(
        "暂停菜单 Attribute 按钮",
        Key = AttributeButtonKey,
        LocTable = LocTable,
        TextKey = AttributeButtonTextKey,
        Anchor = PauseMenuButtonAnchor.BeforeExitActions,
        Order = 10)]
    internal static void RunAttributePauseMenuButton(PauseMenuButtonContext context)
    {
        pauseMenuButtonClickCount++;
        ModLogger.Info($"[PauseMenuDemo] Attribute 暂停菜单按钮被点击，累计次数：{pauseMenuButtonClickCount}，{DescribeContext(context)}");
    }

    internal static void RunManualPauseMenuButton(PauseMenuButtonContext context)
    {
        pauseMenuButtonClickCount++;
        ModLogger.Info($"[PauseMenuDemo] 手动注册暂停菜单按钮被点击，累计次数：{pauseMenuButtonClickCount}，{DescribeContext(context)}");
    }

    internal static bool CanUseManualPauseMenuButton(PauseMenuButtonContext context)
    {
        return context.IsRunInProgress && !context.IsGameOver;
    }

    private static string DescribeContext(PauseMenuButtonContext context)
    {
        string runStateName = context.RunState?.GetType().Name ?? "无运行状态";
        return $"运行状态={runStateName}，多人客户端={context.IsMultiplayerClient}，运行中={context.IsRunInProgress}，游戏结束={context.IsGameOver}";
    }
}

using Godot;
using JmcModLib.Utils;
using MegaCrit.Sts2.Core.Nodes;

namespace JmcModLibDemo.Core;

/// <summary>
/// Demo 专用的输入事件探针，用于观察 Godot 实际收到的键盘、手柄按钮与动作事件。
/// </summary>
public partial class InputProbeNode : Node
{
    private const string ProbeNodeName = "JmcModLibDemoInputProbe";
    private const ulong MotionLogIntervalMs = 250;
    private const float MotionLogThreshold = 0.5f;

    private static bool installScheduled;
    private static bool patchLoggingEnabled;
    private static ulong patchedEventCount;
    private static ulong patchedLastMotionLogTime;

    private ulong eventCount;
    private ulong lastMotionLogTime;

    /// <summary>
    /// 获取输入探针当前是否已经挂载到场景树。
    /// </summary>
    public static bool IsInstalled =>
        Engine.GetMainLoop() is SceneTree { Root: not null } tree
        && FindExisting(tree) is not null;

    /// <summary>
    /// 按配置状态启动或停止输入探针。
    /// </summary>
    public static void ApplyDesiredState(bool enabled)
    {
        if (enabled)
        {
            Install();
            return;
        }

        Uninstall();
    }

    /// <summary>
    /// 根据当前状态启动或停止输入探针。
    /// </summary>
    public static void Toggle()
    {
        if (IsInstalled)
        {
            Uninstall();
            return;
        }

        Install();
    }

    /// <summary>
    /// 启动输入探针，并将收到的关键输入事件写入日志。
    /// </summary>
    public static void Install()
    {
        patchLoggingEnabled = true;

        if (Engine.GetMainLoop() is not SceneTree { Root: not null } tree)
        {
            ModLogger.Warn("[InputProbe] 无法启动：当前 MainLoop 不是 SceneTree 或 Root 不存在。");
            return;
        }

        if (FindExisting(tree) is not null)
        {
            ModLogger.Info("[InputProbe] 已经在运行。已启用 NInputManager patch 日志，按 L3/R3 后查看日志即可。");
            return;
        }

        if (installScheduled)
        {
            ModLogger.Info("[InputProbe] 已安排启动，请稍后按键测试。");
            return;
        }

        installScheduled = true;
        Callable.From(() => DeferredInstall(tree)).CallDeferred();
        ModLogger.Info("[InputProbe] 已请求启动。节点挂载完成后会继续写入日志，同时已启用 NInputManager patch 日志。");
    }

    /// <summary>
    /// 停止输入探针。
    /// </summary>
    public static void Uninstall()
    {
        if (Engine.GetMainLoop() is not SceneTree { Root: not null } tree)
        {
            patchLoggingEnabled = false;
            return;
        }

        patchLoggingEnabled = false;

        InputProbeNode? probe = FindExisting(tree);
        if (probe is null)
        {
            ModLogger.Info("[InputProbe] 当前没有运行。");
            installScheduled = false;
            return;
        }

        probe.QueueFree();
        installScheduled = false;
        ModLogger.Info("[InputProbe] 已停止。");
    }

    /// <summary>
    /// 记录从游戏输入管理器 patch 捕获到的输入事件。
    /// </summary>
    internal static void HandlePatchedInput(string phase, InputEvent inputEvent)
    {
        if (!patchLoggingEnabled)
        {
            return;
        }

        switch (inputEvent)
        {
            case InputEventJoypadButton joypadButton:
                LogPatched($"{phase} JoypadButton device={joypadButton.Device} button={joypadButton.ButtonIndex} pressed={joypadButton.Pressed}");
                break;
            case InputEventAction action:
                LogPatched($"{phase} Action action={action.Action} pressed={action.Pressed} strength={action.Strength:0.###}");
                break;
            case InputEventJoypadMotion joypadMotion when ShouldLogPatchedMotion(joypadMotion):
                LogPatched($"{phase} JoypadMotion device={joypadMotion.Device} axis={joypadMotion.Axis} value={joypadMotion.AxisValue:0.###}");
                break;
            case InputEventKey { Pressed: true, Echo: false } key:
                LogPatched($"{phase} Key key={key.Keycode} physical={key.PhysicalKeycode}");
                break;
        }
    }

    public override void _Ready()
    {
        ActivateProcessing();
    }

    public override void _Input(InputEvent @event)
    {
        HandleInput("Input", @event);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        HandleInput("UnhandledInput", @event);
    }

    public override void _UnhandledKeyInput(InputEvent @event)
    {
        HandleInput("UnhandledKeyInput", @event);
    }

    private static void DeferredInstall(SceneTree tree)
    {
        try
        {
            if (FindExisting(tree) is not null)
            {
                installScheduled = false;
                return;
            }

            Node? parent = GetPreferredParent(tree);
            if (parent is null)
            {
                installScheduled = false;
                ModLogger.Warn("[InputProbe] 无法启动：找不到可挂载的父节点。");
                return;
            }

            var probe = new InputProbeNode
            {
                Name = ProbeNodeName,
                ProcessMode = ProcessModeEnum.Always
            };

            parent.AddChild(probe);
            probe.ActivateProcessing();
            installScheduled = false;
            ModLogger.Info($"[InputProbe] 已启动，Parent={parent.GetPath()}。请按 L3/R3/其它手柄键，观察 JoypadButton 或 Action 日志。");
        }
        catch (Exception ex)
        {
            installScheduled = false;
            ModLogger.Error("[InputProbe] 启动失败。", ex);
        }
    }

    private static InputProbeNode? FindExisting(SceneTree tree)
    {
        InputProbeNode? rootProbe = tree.Root?.GetNodeOrNull<InputProbeNode>(ProbeNodeName);
        if (rootProbe is not null)
        {
            return rootProbe;
        }

        try
        {
            return NGame.Instance?.GetNodeOrNull<InputProbeNode>(ProbeNodeName);
        }
        catch (InvalidOperationException)
        {
            return null;
        }
    }

    private static Node? GetPreferredParent(SceneTree tree)
    {
        try
        {
            if (NGame.Instance?.IsInsideTree() == true)
            {
                return NGame.Instance;
            }
        }
        catch (InvalidOperationException)
        {
        }

        return tree.Root;
    }

    private void ActivateProcessing()
    {
        ProcessMode = ProcessModeEnum.Always;
        SetProcessInput(true);
        SetProcessUnhandledInput(true);
        SetProcessUnhandledKeyInput(true);
    }

    private void HandleInput(string phase, InputEvent @event)
    {
        switch (@event)
        {
            case InputEventJoypadButton joypadButton:
                Log($"{phase} JoypadButton device={joypadButton.Device} button={joypadButton.ButtonIndex} pressed={joypadButton.Pressed}");
                break;
            case InputEventAction action:
                Log($"{phase} Action action={action.Action} pressed={action.Pressed} strength={action.Strength:0.###}");
                break;
            case InputEventJoypadMotion joypadMotion when ShouldLogMotion(joypadMotion):
                Log($"{phase} JoypadMotion device={joypadMotion.Device} axis={joypadMotion.Axis} value={joypadMotion.AxisValue:0.###}");
                break;
            case InputEventKey { Pressed: true, Echo: false } key:
                Log($"{phase} Key key={key.Keycode} physical={key.PhysicalKeycode}");
                break;
        }
    }

    private bool ShouldLogMotion(InputEventJoypadMotion motion)
    {
        if (MathF.Abs(motion.AxisValue) < MotionLogThreshold)
        {
            return false;
        }

        ulong now = Time.GetTicksMsec();
        if (now - lastMotionLogTime < MotionLogIntervalMs)
        {
            return false;
        }

        lastMotionLogTime = now;
        return true;
    }

    private static bool ShouldLogPatchedMotion(InputEventJoypadMotion motion)
    {
        if (MathF.Abs(motion.AxisValue) < MotionLogThreshold)
        {
            return false;
        }

        ulong now = Time.GetTicksMsec();
        if (now - patchedLastMotionLogTime < MotionLogIntervalMs)
        {
            return false;
        }

        patchedLastMotionLogTime = now;
        return true;
    }

    private static void LogPatched(string message)
    {
        patchedEventCount++;
        ModLogger.Info($"[InputProbe Patch #{patchedEventCount}] {message}");
    }

    private void Log(string message)
    {
        eventCount++;
        ModLogger.Info($"[InputProbe #{eventCount}] {message}");
    }
}

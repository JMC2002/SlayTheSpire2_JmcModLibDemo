using JmcModLib.Config;
using JmcModLib.Config.UI;
using JmcModLib.Prefabs;
using JmcModLib.Utils;
using Godot;
using MegaCrit.Sts2.Core.ControllerInput;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;

namespace JmcModLibDemo.Core;

public enum DemoColorTheme
{
    Classic,
    Gold,
    Emerald,
    Crimson
}

public static class DemoSettings
{
    internal const string DirectWriteGroup = "direct_write";
    internal const string CallbackGroup = "callbacks";
    internal const string TextGroup = "text_input";
    internal const string DropdownGroup = "dropdowns";
    internal const string NumericGroup = "numeric_controls";
    internal const string ButtonGroup = "buttons";
    internal const string KeybindGroup = "keybinds";
    internal const string AppearanceGroup = "appearance";
    internal const string RichTextGroup = "rich_text";
    private const string RichTextHoverDescription =
        "[color=#ffd95a]黄色重点[/color]、[color=#7ee787]绿色成功[/color]、[color=#ff8f8f]红色警告[/color]\n"
        + "[b]加粗文字[/b]、[i]斜体文字[/i] 和换行都会进入游戏原生 HoverTip。";
    private const string RichTextButtonHelp =
        "[gold]按钮说明也支持富文本[/gold]\n"
        + "这段文本来自 UIButtonAttribute.HelpText，可用于给危险操作、调试按钮或一次性动作补充说明。";

    private static int propertyBackedLevel = 3;

    public static int LastDirectSetterValue { get; private set; } = propertyBackedLevel;

    public static string LastCallbackMessage { get; private set; } = "尚未触发回调";

    public static int ButtonClickCount { get; private set; }

    /// <summary>
    /// 最普通的字段写法：不需要 OnChanged，UI 修改时 JmcModLib 会直接 SetValue 到这个 static 字段。
    /// </summary>
    [UIToggle]
    [Config(
        "直接修改 bool 字段",
        group: DirectWriteGroup,
        Description = "不写 onChanged。开关变化后，这个 static 字段会被 JmcModLib 直接改掉并保存。",
        Key = "direct.enable_feature",
        Order = 10)]
    public static bool DirectBoolField = true;

    /// <summary>
    /// 属性也可以无回调直接写；这里的 setter 只是为了证明 UI 确实会直接调用属性 setter。
    /// </summary>
    [UIIntSlider(0, 10)]
    [Config(
        "直接修改 int 属性",
        group: DirectWriteGroup,
        Description = "同样不写 onChanged。拖动滑条会直接调用属性 setter，并在日志里记录 setter 收到的值。",
        Key = "direct.property_level",
        Order = 20)]
    public static int PropertyBackedLevel
    {
        get => propertyBackedLevel;
        set
        {
            propertyBackedLevel = value;
            LastDirectSetterValue = value;
            ModLogger.Info($"PropertyBackedLevel setter => {value}");
        }
    }

    [UISlider(0.5, 2.0, 0.1)]
    [Config(
        "带回调的缩放倍率",
        onChanged: nameof(OnCallbackScaleChanged),
        group: CallbackGroup,
        Description = "只有需要刷新缓存、重建 UI、通知游戏对象时才需要 onChanged。",
        Key = "callback.scale",
        Order = 10)]
    public static double CallbackScale = 1.0;

    [UISlider(0.0, 1.0, 0.01)]
    [Config(
        "带回调的透明度",
        onChanged: nameof(OnCallbackAlphaChanged),
        group: CallbackGroup,
        Description = "这个示例会在回调里写日志。实际 MOD 可在这里刷新界面、清缓存或重算状态。",
        Key = "callback.alpha",
        Order = 20)]
    public static float CallbackAlpha = 0.75f;

    [UIInput(32)]
    [Config(
        "单行文本输入",
        group: TextGroup,
        Description = "UIInput 目前会渲染为文本输入框，提交或失焦时保存。",
        Key = "text.single_line",
        Order = 10)]
    public static string SingleLineText = "Hello JmcModLib";

    [UIInput(160, multiline: true)]
    [Config(
        "多行文本声明",
        group: TextGroup,
        Description = "这里演示 multiline 元数据。当前游戏内配置桥接仍以单行输入控件承载。",
        Key = "text.multiline_declared",
        Order = 20)]
    public static string MultilineDeclaredText = "line one / line two";

    [UIDropdown]
    [Config(
        "枚举下拉",
        group: DropdownGroup,
        Description = "枚举类型不传参数时会自动使用全部枚举值。",
        Key = "dropdown.enum_theme",
        Order = 10)]
    public static DemoColorTheme EnumTheme = DemoColorTheme.Gold;

    [UIDropdown("Compact", "Normal", "Large")]
    [Config(
        "字符串下拉",
        group: DropdownGroup,
        Description = "字符串下拉需要在 UIDropdown 里列出候选项。",
        Key = "dropdown.string_size",
        Order = 20)]
    public static string StringDropdown = "Normal";

    [UIDropdown(nameof(DemoColorTheme.Crimson))]
    [Config(
        "枚举下拉排除项",
        group: DropdownGroup,
        Description = "枚举模式下 UIDropdown 参数会被当作排除项，这里隐藏 Crimson。",
        Key = "dropdown.enum_exclude",
        Order = 30)]
    public static DemoColorTheme EnumThemeWithoutCrimson = DemoColorTheme.Emerald;

    [UIDropdown]
    [Config(
        "约定式动态下拉",
        group: DropdownGroup,
        Description = "不在 UIDropdown 里写候选项，也不显式指定 Key。JmcModLib 会按字段名寻找 DynamicProviderDropdownOptions/GetDynamicProviderDropdownOptions/BuildDynamicProviderDropdownOptions。",
        Order = 40)]
    public static string DynamicProviderDropdown = "Balanced";

    public static IReadOnlyList<string> DynamicProviderDropdownOptions =>
        DirectBoolField
            ? ["Tiny", "Balanced", "Generous", "FeatureEnabled"]
            : ["Tiny", "Balanced", "Generous", "FeatureDisabled"];

    [UIIntSlider(0, 100)]
    [Config(
        "整数滑条",
        group: NumericGroup,
        Description = "UIIntSlider 只支持 int。",
        Key = "numeric.int_slider",
        Order = 10)]
    public static int IntSlider = 40;

    [UISlider(-10.0, 10.0, 0.1)]
    [Config(
        "浮点滑条",
        group: NumericGroup,
        Description = "UISlider 可用于 float，step 控制实际步进。",
        Key = "numeric.float_slider",
        Order = 20)]
    public static float FloatSlider = 2.5f;

    /// <summary>
    /// 用来验证负数最小值且默认值为 0 时，滑条初始位置是否正确居中。
    /// </summary>
    [UIIntSlider(-10, 10)]
    [Config(
        "负数区间默认 0 滑条",
        group: NumericGroup,
        Description = "最小值 -10，最大值 10，默认值 0，用来复现或验证原生滑条的负数区间位置映射。",
        Key = "numeric.negative_default_zero_slider",
        Order = 25)]
    public static int NegativeDefaultZeroSlider = 0;

    [UISlider(0.0, 1.0, 0.05)]
    [Config(
        "通用数字滑条 double",
        group: NumericGroup,
        Description = "UISlider 可用于 int/float/double/decimal 等数字类型。",
        Key = "numeric.double_slider",
        Order = 30)]
    public static double DoubleSlider = 0.5;

    [Config(
        "无 UI Attribute 的数字",
        group: NumericGroup,
        Description = "没有 UI Attribute 时，数字会走基础 SpinBox 回退控件。",
        Key = "numeric.spinbox_fallback",
        Order = 40)]
    public static int SpinBoxFallback = 7;

    [UIToggle]
    [Config(
        "需要重启的开关",
        group: NumericGroup,
        Description = "RestartRequired 会在配置项下面显示重启提示。",
        Key = "numeric.restart_required_toggle",
        RestartRequired = true,
        Order = 50)]
    public static bool RestartRequiredToggle = false;

    [UIButton(
        "Attribute 按钮",
        "执行",
        ButtonGroup,
        Key = "button.attribute",
        HelpText = "这个按钮由 UIButtonAttribute 扫描注册，适合无参数的一次性操作。",
        Order = 10)]
    public static void RunAttributeButton()
    {
        ButtonClickCount++;
        ModLogger.Info($"[UIButton] 按钮被点击，累计次数：{ButtonClickCount}");
    }

    [UIKeybind]
    [Config(
        "键盘热键",
        group: KeybindGroup,
        Description = "字段类型是 Godot.Key。点击这一行后按下新的键，会直接修改这个 static 字段并保存。",
        Key = "keybind.keyboard_only",
        Order = 10)]
    public static Key KeyboardOnlyHotkey = Key.F8;

    [UIKeybind(allowController: true)]
    [Config(
        "键盘与手柄热键",
        group: KeybindGroup,
        Description = "字段类型是 JmcKeyBinding。它同时保存键盘组合键和手柄输入，仍然不需要 OnChanged。",
        Key = "keybind.keyboard_and_controller",
        Order = 20)]
    public static JmcKeyBinding KeyboardAndControllerHotkey = new(
        Key.F9,
        Controller.leftTrigger.ToString(),
        JmcKeyModifiers.Ctrl);

    [JmcHotkey(nameof(KeyboardOnlyHotkey), ConsumeInput = false)]
    public static void LogKeyboardOnlyHotkey()
    {
        ModLogger.Info($"[DemoHotkey] 键盘单键热键触发：{KeyboardOnlyHotkey}");
    }

    [JmcHotkey(nameof(KeyboardAndControllerHotkey), ConsumeInput = false)]
    public static void LogKeyboardAndControllerHotkey()
    {
        ModLogger.Info($"[DemoHotkey] 键盘组合/手柄热键触发：{KeyboardAndControllerHotkey}");
    }

    [UIHotkey(
        "一行生成的 Steam 热键",
        KeybindGroup,
        Key = "keybind.generated_ui_hotkey",
        Description = "UIHotkey 会自动生成配置项，并由 JML 额外生成 Steam Input 动作。",
        DefaultKeyboard = Key.F10,
        DefaultController = "controller_right_trigger",
        AllowController = true,
        ConsumeInput = false,
        Order = 30)]
    public static void LogGeneratedUiHotkey()
    {
        ModLogger.Info("[DemoHotkey] UIHotkey 自动生成热键触发：此项应同时出现在 JML 设置 UI 和 Steam Input 动作列表。");
    }

    [UIColor(AllowAlpha = false)]
    [Config(
        "主题强调色",
        group: AppearanceGroup,
        Description = "UIColor 支持 Godot.Color。这里不需要 OnChanged，调色盘选择后会直接修改这个 static 字段并保存。",
        Key = "appearance.accent_color",
        Order = 10)]
    public static Color AccentColor = new("E0B24F");

    [UIColor("#1A1D22CC", "#3C6F8FCC", "#65A83ACC", "#B94A3FCC", Palette = UIColorPalette.None, AllowAlpha = true)]
    [Config(
        "半透明覆盖色",
        group: AppearanceGroup,
        Description = "这个示例允许 Alpha，配置文件里会保存为 #RRGGBBAA，方便人眼检查。",
        Key = "appearance.overlay_color",
        Order = 20)]
    public static Color OverlayColor = new Color(0.1f, 0.12f, 0.15f, 0.8f);

    [UIToggle]
    [Config(
        "富文本 HoverTip",
        group: RichTextGroup,
        Description = RichTextHoverDescription,
        Key = "rich_text.hover_tip",
        Order = 10)]
    public static bool RichTextHoverTip = true;

    [UIButton(
        "富文本按钮说明",
        "写入日志",
        RichTextGroup,
        Key = "button.rich_text",
        HelpText = RichTextButtonHelp,
        Color = UIButtonColor.Gold,
        Order = 20)]
    public static void RunRichTextButton()
    {
        ModLogger.Info("[RichTextDemo] 富文本按钮被点击。");
    }

    [UIButton(
        "原生确认框预制件",
        "弹出",
        ButtonGroup,
        Key = "button.confirmation_prefab",
        HelpText = "这个按钮演示 JmcModLib.Prefabs.JmcConfirmationPopup，正文使用游戏原生 MegaRichTextLabel 展示富文本。",
        Color = UIButtonColor.Green,
        Order = 30)]
    public static void RunConfirmationPrefabButton()
    {
        TaskHelper.RunSafely(RunConfirmationPrefabButtonAsync());
    }

    public static async Task RunConfirmationPrefabButtonAsync()
    {
        bool confirmed = await JmcConfirmationPopup.ShowConfirmationAsync(
            new LocString("settings_ui", "EXTENSION.JMCMODLIB.PREFABS.JmcModLibDemo.CONFIRMATION.title"),
            new LocString("settings_ui", "EXTENSION.JMCMODLIB.PREFABS.JmcModLibDemo.CONFIRMATION.body"),
            new LocString("settings_ui", "EXTENSION.JMCMODLIB.PREFABS.JmcModLibDemo.CONFIRMATION.confirm"),
            new LocString("settings_ui", "EXTENSION.JMCMODLIB.PREFABS.JmcModLibDemo.CONFIRMATION.cancel"));

        ModLogger.Info($"[PrefabDemo] 原生确认框结果：{(confirmed ? "Confirmed" : "Cancelled")}");
    }

    [UIButton(
        "只显示确认按钮预制件",
        "弹出",
        ButtonGroup,
        Key = "button.confirm_only_prefab",
        HelpText = "这个按钮演示 JmcConfirmationPopup.ShowMessageAsync，只显示确认按钮。",
        Color = UIButtonColor.Green,
        Order = 40)]
    public static void RunConfirmOnlyPrefabButton()
    {
        TaskHelper.RunSafely(RunConfirmOnlyPrefabButtonAsync());
    }

    public static async Task RunConfirmOnlyPrefabButtonAsync()
    {
        bool confirmed = await JmcConfirmationPopup.ShowMessageAsync(
            new LocString("settings_ui", "EXTENSION.JMCMODLIB.PREFABS.JmcModLibDemo.CONFIRM_ONLY.title"),
            new LocString("settings_ui", "EXTENSION.JMCMODLIB.PREFABS.JmcModLibDemo.CONFIRM_ONLY.body"),
            new LocString("settings_ui", "EXTENSION.JMCMODLIB.PREFABS.JmcModLibDemo.CONFIRM_ONLY.confirm"));

        ModLogger.Info($"[PrefabDemo] 只显示确认按钮弹窗结果：{(confirmed ? "Confirmed" : "Cancelled")}");
    }

    [UIToggle]
    [Config(
        "启用输入探针",
        onChanged: nameof(OnInputProbeEnabledChanged),
        group: ButtonGroup,
        Description = "开启后会在后台挂载 Demo 输入探针，并把键盘、手柄按钮、游戏 Action 写入日志，方便确认 Steam Input 之后还能看到哪些事件。",
        Key = "debug.input_probe_enabled",
        Order = 45)]
    public static bool InputProbeEnabled = false;

    [UIButton(
        "手柄输入探针",
        "切换",
        ButtonGroup,
        Key = "button.input_probe_toggle",
        HelpText = "启动或停止 Demo 输入探针。启动后按 L3/R3/其它手柄键，再查看日志里出现的是 JoypadButton 还是游戏 Action。",
        Color = UIButtonColor.Blue,
        Order = 50)]
    public static void ToggleInputProbeButton()
    {
        InputProbeNode.Toggle();
    }

    public static void RunManualButton()
    {
        ButtonClickCount++;
        ModLogger.Info($"RegisterButton 手动按钮被点击，累计次数：{ButtonClickCount}");
    }

    public static void ApplyStartupDebugSettings()
    {
        InputProbeNode.ApplyDesiredState(InputProbeEnabled);
    }

    private static void OnCallbackScaleChanged(double value)
    {
        LastCallbackMessage = $"Scale callback => {value:0.0}";
        ModLogger.Info(LastCallbackMessage);
    }

    private static void OnCallbackAlphaChanged(float value)
    {
        LastCallbackMessage = $"Alpha callback => {value:0.00}";
        ModLogger.Info(LastCallbackMessage);
    }

    private static void OnInputProbeEnabledChanged(bool enabled)
    {
        ModLogger.Info($"[InputProbe] 配置开关变更：{enabled}");
        InputProbeNode.ApplyDesiredState(enabled);
    }
}

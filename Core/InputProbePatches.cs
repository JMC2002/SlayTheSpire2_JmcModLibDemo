using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.CommonUi;

namespace JmcModLibDemo.Core;

/// <summary>
/// Demo 专用输入探针补丁，直接观察游戏输入管理器收到的事件。
/// </summary>
[HarmonyPatch(typeof(NInputManager), nameof(NInputManager._UnhandledKeyInput))]
internal static class InputProbeKeyboardPatch
{
    [HarmonyPostfix]
    private static void Postfix(InputEvent inputEvent)
    {
        InputProbeNode.HandlePatchedInput("NInputManager._UnhandledKeyInput", inputEvent);
    }
}

/// <summary>
/// Demo 专用输入探针补丁，直接观察游戏输入管理器收到的手柄与非键盘事件。
/// </summary>
[HarmonyPatch(typeof(NInputManager), nameof(NInputManager._UnhandledInput))]
internal static class InputProbeControllerPatch
{
    [HarmonyPostfix]
    private static void Postfix(InputEvent inputEvent)
    {
        InputProbeNode.HandlePatchedInput("NInputManager._UnhandledInput", inputEvent);
    }
}

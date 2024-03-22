using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace ScannableTools;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class ScannableTools : BaseUnityPlugin {
    public static ScannableTools Instance { get; private set; } = null!;
    internal new static ManualLogSource Logger { get; private set; } = null!;
    internal static Harmony? Harmony { get; private set; }
    internal static ScanToolsConfig scanToolsConfig { get; private set; } = null!;

    public static void AddScanNodeToGrabbableObject(GrabbableObject grabbableObject) {
        GrabbableObjectPatch.HandleGrabbableObject(grabbableObject);
    }

    public static void UpdateScanNodeOfGrabbableObject(GrabbableObject grabbableObject) {
        GrabbableObjectPatch.UpdateGrabbableObject(grabbableObject);
    }

    private void Awake() {
        Logger = base.Logger;
        Instance = this;

        scanToolsConfig = new ScanToolsConfig(Config);

        scanToolsConfig.HandleConfig();

        Patch();

        Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} v{MyPluginInfo.PLUGIN_VERSION} has loaded!");
    }

    internal static void Patch() {
        Harmony ??= new Harmony(MyPluginInfo.PLUGIN_GUID);

        Logger.LogDebug("Patching...");

        Harmony.PatchAll();

        Logger.LogDebug("Finished patching!");
    }

    internal static void Unpatch() {
        Logger.LogDebug("Unpatching...");

        Harmony?.UnpatchSelf();

        Logger.LogDebug("Finished unpatching!");
    }
}
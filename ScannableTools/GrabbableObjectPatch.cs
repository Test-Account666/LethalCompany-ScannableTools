using HarmonyLib;
using UnityEngine;

namespace ScannableTools;

[HarmonyPatch]
public static class GrabbableObjectPatch {
    [HarmonyPatch(typeof(GrabbableObject), "Start")]
    [HarmonyPostfix]
    // ReSharper disable once InconsistentNaming
    public static void AddScanNodeToCompanyIssuedItems(GrabbableObject __instance) => HandleGrabbableObject(__instance);

    [HarmonyPatch(typeof(GrabbableObject), "LateUpdate")]
    [HarmonyPostfix]
    // ReSharper disable once InconsistentNaming
    public static void UpdateScanNodeOfCompanyIssuedItems(GrabbableObject __instance) => UpdateGrabbableObject(__instance);

    [HarmonyPatch(typeof(BoomboxItem), "Update")]
    [HarmonyPostfix]
    // ReSharper disable once InconsistentNaming
    public static void UpdateScanNodeOfCompanyIssuedItems(BoomboxItem __instance) => UpdateGrabbableObject(__instance);

    internal static void HandleGrabbableObject(GrabbableObject grabbableObject) {
        if (grabbableObject == null) return;

        if (grabbableObject.itemProperties.isScrap) return;

        var hasScanNodeContainer = grabbableObject.gameObject.TryGetComponent<ScanNodeContainer>(out var scanNodeContainer);

        if (hasScanNodeContainer) return;

        if (grabbableObject.isHeld) return;

        if (ScannableTools.ScanToolsConfig.blacklistedItemsList.Contains(grabbableObject.itemProperties.itemName.ToLower())) return;

        if (ScannableTools.ScanToolsConfig.blacklistedItemsRegex is not null)
            if (ScannableTools.ScanToolsConfig.blacklistedItemsRegex.IsMatch(grabbableObject.itemProperties.itemName))
                return;

        if (grabbableObject.itemProperties.itemName.Equals("Key") && ScannableTools.ScanToolsConfig.keyScanNodeType.Value == 0) {
            var scanNode = grabbableObject.gameObject.GetComponentInChildren<ScanNodeProperties>();
            if (scanNode != null) {
                scanNodeContainer = grabbableObject.gameObject.AddComponent<ScanNodeContainer>();
                scanNodeContainer.scanNodeGameObject = scanNode.gameObject;
                scanNodeContainer.scanNode = scanNode;
                return;
            }
        }

        CreateScanNodeOnObject(grabbableObject.gameObject, grabbableObject.itemProperties.itemName, GetBatteryPercentage(grabbableObject));
    }

    internal static void UpdateGrabbableObject(GrabbableObject grabbableObject) {
        if (grabbableObject == null) return;

        if (grabbableObject.itemProperties.isScrap) return;

        var hasScanNodeContainer = grabbableObject.gameObject.TryGetComponent<ScanNodeContainer>(out var scanNodeContainer);

        if (!hasScanNodeContainer || scanNodeContainer.scanNode == null) {
            if (!grabbableObject.isHeld) HandleGrabbableObject(grabbableObject);
            return;
        }

        if (grabbableObject.isHeld) {
            Object.Destroy(scanNodeContainer.scanNodeGameObject);
            Object.Destroy(scanNodeContainer);
            return;
        }

        if (!grabbableObject.itemProperties.requiresBattery) return;

        var batteryPercentage = GetBatteryPercentage(grabbableObject);

        if (batteryPercentage != null) scanNodeContainer.scanNode.subText = batteryPercentage;
    }

    private static string? GetBatteryPercentage(GrabbableObject grabbableObject) {
        if (grabbableObject == null || !grabbableObject.itemProperties.requiresBattery) return null;

        var subText = grabbableObject.insertedBattery.empty
            ? "Battery: 0%"
            : $"Battery: {(int) (grabbableObject.insertedBattery.charge * 100)}%";

        return subText;
    }

    private static void CreateScanNodeOnObject(GameObject gameObject, string headerText, string? subText) {
        const int nodeType = 0;
        const int minRange = 1;
        const int maxRange = 13;
        const int size = 1;

        var scanNodeObject = new GameObject("ScanNode", typeof(ScanNodeProperties), typeof(BoxCollider)) {
            layer = LayerMask.NameToLayer("ScanNode"),
            transform = {
                localScale = Vector3.one * size,
                parent = gameObject.transform,
            },
        };

        var boxCollider = scanNodeObject.GetComponent<BoxCollider>();
        boxCollider.isTrigger = true;

        scanNodeObject.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

        var scanNode = scanNodeObject.GetComponent<ScanNodeProperties>();

        scanNode.scrapValue = 0;
        scanNode.creatureScanID = -1;
        scanNode.nodeType = nodeType;
        scanNode.minRange = minRange;
        scanNode.maxRange = maxRange;
        scanNode.requiresLineOfSight = true;
        scanNode.headerText = headerText;

        if (subText != null) scanNode.subText = subText;

        var scanNodeContainer = gameObject.AddComponent<ScanNodeContainer>();

        scanNodeContainer.scanNodeGameObject = scanNodeObject;
        scanNodeContainer.scanNode = scanNode;
    }
}
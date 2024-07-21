using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using BepInEx.Configuration;

namespace ScannableTools;

public class ScanToolsConfig(ConfigFile configFile) {
    internal ConfigEntry<int> keyScanNodeType = null!;
    internal ConfigEntry<string> blacklistedItems = null!;
    internal ConfigEntry<string> blacklistedItemsRegexEntry = null!;

    internal readonly List<string> blacklistedItemsList = [
    ];

    internal Regex? blacklistedItemsRegex;

    internal void HandleConfig() {
        keyScanNodeType = configFile.Bind("Keys", "Key ScanNode Look", 0,
                                          new ConfigDescription(
                                              "Defines how the scan node for keys look. 0 = Green, but Blue after pickup. 1 = Teal colored. (Try setting this to 1, if you're experiencing lags, for some reason)",
                                              new AcceptableValueRange<int>(0, 1)));

        blacklistedItems = configFile.Bind("Items", "Blacklisted Items", "clipboard, polaroid, test",
                                           "A comma separated list of items that will be ignored by this mod");

        blacklistedItems.SettingChanged += (_, _) => SetBlackListedItems();

        blacklistedItemsRegexEntry = configFile.Bind("Items", "Blacklisted Items Regex", "",
                                                     "Regex for blacklisted items. Leave blank, if you don't know how to use regex!");

        blacklistedItemsRegexEntry.SettingChanged += (_, _) => SetBlackListedItems();

        SetBlackListedItems();
    }

    private void SetBlackListedItems() {
        blacklistedItemsList.Clear();
        blacklistedItemsList.AddRange(blacklistedItems.Value.ToLower().Replace(", ", ",").Split(","));

        if (string.IsNullOrEmpty(blacklistedItemsRegexEntry.Value)) {
            blacklistedItemsRegex = null;
            return;
        }

        blacklistedItemsRegex = new Regex(blacklistedItemsRegexEntry.Value);
    }
}
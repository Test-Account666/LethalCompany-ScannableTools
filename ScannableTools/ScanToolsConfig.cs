using BepInEx.Configuration;

namespace ScannableTools;

public class ScanToolsConfig {
    internal ConfigEntry<int> keyScanNodeType = null!;
    private readonly ConfigFile _configFile;

    public ScanToolsConfig(ConfigFile configFile) {
        _configFile = configFile;
    }

    internal void HandleConfig() {
        keyScanNodeType = _configFile.Bind<int>("Keys", "Key ScanNode Look", 0,
            new ConfigDescription(
                "Defines how the scan node for keys look. 0 = Green, but Blue after pickup. 1 = Teal colored. (Try setting this to 1, if you're experiencing lags, for some reason)",
                new AcceptableValueRange<int>(0, 1)));
    }
}
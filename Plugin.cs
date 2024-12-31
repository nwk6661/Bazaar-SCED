using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;

namespace ShowCombatEncounterDetail;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class ShowCombatEncounterDetail : BaseUnityPlugin
{
    private static ConfigEntry<float> _configRelativePosX;
    private static ConfigEntry<float> _configRelativePosY;
    internal static ManualLogSource _logger;
    public static TooltipManager TooltipManager { get; private set; }

    private void Awake()
    {
        _logger = base.Logger;
        _logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        
        _configRelativePosX = Config.Bind(
            "General",
            "relativePosX",
            0.0f,
            new ConfigDescription(
                "Relative X position of the encounter card as a ratio between [-1, 1]",
                new AcceptableValueRange<float>(-1f, 1f)
            )
        );

        _configRelativePosY = Config.Bind(
            "General",
            "relativePosY",
            -.125f,
            new ConfigDescription(
                "Relative Y position of the encounter card as a ratio between [-1, 1]",
                new AcceptableValueRange<float>(-1f, 1f)
            )
        );

        TooltipManager = new TooltipManager(_configRelativePosX, _configRelativePosY, _logger);
        Hooks.Hooks.InitializeHooks();
    }

    private void OnDestroy()
    {
        Hooks.Hooks.UninitializeHooks();
        TooltipManager.CleanDestroy();
    }
}
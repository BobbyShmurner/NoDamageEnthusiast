using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;

using HarmonyLib;

namespace NoDamageEnthusiast
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInProcess("ULTRAKILL.exe")]
    public class Plugin : BaseUnityPlugin
    {
        internal static ManualLogSource Log;
        internal static Harmony harmony;

        internal static ConfigEntry<bool> configNoDamage;
        internal static ConfigEntry<bool> configNoCheckpoints;
        
        private void Awake()
        {
            Logger.LogInfo($"Loading Plugin {PluginInfo.PLUGIN_GUID}...");

            Plugin.Log = base.Logger;
            Plugin.Log.LogInfo("Created Global Logger");

            harmony = new Harmony("NoDamageEnthusiast");
            harmony.PatchAll();
            Plugin.Log.LogInfo("Applied All Patches");

            configNoDamage = Config.Bind("General", "NoDamage", false, "If enabled, taking damage will kill you");
            configNoCheckpoints = Config.Bind("General", "NoCheckpoints", false, "If enabled, checkpoints will be disabled");

            Plugin.Log.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

            ULTRAKILL.Settings.InitSettings();
        }

        private void OnDestroy()
        {
            harmony?.UnpatchSelf();
        }
    }
}

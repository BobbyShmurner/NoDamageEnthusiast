using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;

using HarmonyLib;

using ULTRAINTERFACE;

using UnityEngine;
using UnityEngine.UI;

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

            // This is just a temporary solution until I add proper settings UI
            OptionsMenu optionsMenu = Options.CreateOptionsMenu("No Damage", (menu) => {
                UI.CreateText(menu.ScrollView.Content, "Please excuse the shitty UI, it will improve eventually", 20, 600);
                UI.CreateText(menu.ScrollView.Content, "Maybe...", 20);

                Button noDamageButton = UI.CreateButton(menu.ScrollView.Content, "INSTA-KILL: " + (configNoDamage.Value ? "ON" : "OFF"), 250);
                noDamageButton.onClick.AddListener(() => {
                    configNoDamage.Value = !configNoDamage.Value;
                    noDamageButton.GetComponentInChildren<Text>().text = "INSTA-KILL: " + (configNoDamage.Value ? "ON" : "OFF");
                });

                noDamageButton.gameObject.AddComponent<BackSelectOverride>().Selectable = menu.OptionsButton;

                Button noCheckpointsButton = UI.CreateButton(menu.ScrollView.Content, "CHECKPOINTS: " + (configNoCheckpoints.Value ? "OFF" : "ON"), 250);
                noCheckpointsButton.onClick.AddListener(() => {
                    configNoCheckpoints.Value = !configNoCheckpoints.Value;
                    noCheckpointsButton.GetComponentInChildren<Text>().text = "CHECKPOINTS: " + (configNoCheckpoints.Value ? "OFF" : "ON");
                });

                noCheckpointsButton.gameObject.AddComponent<BackSelectOverride>().Selectable = menu.OptionsButton;

                UI.Log.LogInfo($"Created menu");
            });
        }

        private void OnDestroy()
        {
            harmony?.UnpatchSelf();
            UI.Unload();
        }
    }
}

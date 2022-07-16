using HarmonyLib;
using UnityEngine;

using System;

namespace NoDamageEnthusiast
{
    [HarmonyPatch(typeof(NewMovement))]
	[HarmonyPatch(nameof(NewMovement.GetHurt))]
    class InstaKillPatch
    {
		public static void Prefix(ref int damage)
		{
			if (!Plugin.configNoDamage.Value) return;
			
			damage *= 9999;
		}
    }

	[HarmonyPatch(typeof(StatsManager))]
	[HarmonyPatch(nameof(StatsManager.Restart))]
    class NoCheckpointsPatch
    {
		public static void Prefix(ref StatsManager __instance)
		{
			if (!Plugin.configNoCheckpoints.Value) return;
			
			__instance.currentCheckPoint = null;
		}
    }
}

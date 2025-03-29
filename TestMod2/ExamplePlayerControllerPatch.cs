﻿using HarmonyLib;
using UnityEngine;

namespace RepoLevelLoader
{
    [HarmonyPatch(typeof(PlayerController))]
    internal static class ExamplePlayerControllerPatch
    {
        [HarmonyPrefix, HarmonyPatch(nameof(PlayerController.Start))]
        private static void Start_Prefix(PlayerController __instance)
        {
            // Code to execute for each PlayerController *before* Start() is called.
            RepoLevelLoader.Logger.LogDebug($"{__instance} Start Prefix");
        }

        [HarmonyPostfix, HarmonyPatch(nameof(PlayerController.Start))]
        private static void Start_Postfix(PlayerController __instance)
        {
            // Code to execute for each PlayerController *after* Start() is called.
            RepoLevelLoader.Logger.LogDebug($"{__instance} Start Postfix");
        }
    }
}
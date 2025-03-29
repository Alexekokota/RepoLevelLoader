using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

namespace RepoLevelLoader
{
    [BepInPlugin("Alexekokota.RepoLevelLoader", "RepoLevelLoader", "1.0")]
    public class RepoLevelLoader : BaseUnityPlugin
    {
        internal static RepoLevelLoader Instance { get; private set; }
        internal static new ManualLogSource Logger => Instance._logger;
        private ManualLogSource _logger => base.Logger;
        internal Harmony? Harmony { get; set; }
        public static bool levelsLoaded = false;

        private void Awake()
        {
            Instance = this;
            this.gameObject.transform.parent = null;
            this.gameObject.hideFlags = HideFlags.HideAndDontSave;
            Patch();
            Logger.LogInfo("RepoLevelLoader has loaded!");
        }

        internal void Patch()
        {
            Harmony ??= new Harmony(Info.Metadata.GUID);
            Harmony.PatchAll();
        }

        internal void Unpatch()
        {
            Harmony?.UnpatchSelf();
        }
    }

    [HarmonyPatch(typeof(RunManager), "Awake")]
    public class RunManagerPatch
    {
        static void Prefix()
        {
            RepoLevelLoader.Logger.LogInfo("[Repo Level Loader] RunManager.Awake() - Prefix called.");
        }

        static void Postfix(RunManager __instance)
        {
            RepoLevelLoader.Logger.LogInfo("[Repo Level Loader] RunManager.Awake() - Postfix called.");
            if (!RepoLevelLoader.levelsLoaded)
            {
                LoadCustomLevels(__instance);
                RepoLevelLoader.levelsLoaded = true;
            }
            else
            {
                RepoLevelLoader.Logger.LogInfo("[Repo Level Loader] Custom levels already loaded, skipping.");
            }
        }

        private static void LoadCustomLevels(RunManager runManager)
        {
            string pluginsPath = BepInEx.Paths.PluginPath;
            if (!Directory.Exists(pluginsPath)) return;

            string[] modFolders = Directory.GetDirectories(pluginsPath);
            List<string> bundleFiles = new List<string>();

            foreach (string modFolder in modFolders)
            {
                string customLevelFile = Path.Combine(modFolder, "customlevel.txt");
                if (File.Exists(customLevelFile))
                {
                    string[] foundBundles = Directory.GetFiles(modFolder, "*.bundle");
                    if (foundBundles.Length > 0)
                    {
                        bundleFiles.AddRange(foundBundles);
                    }
                }
            }

            RepoLevelLoader.Logger.LogInfo($"[Repo Level Loader] Found {bundleFiles.Count} custom level files.");

            foreach (string bundleFile in bundleFiles)
            {
                RepoLevelLoader.Logger.LogInfo($"[Repo Level Loader] Processing file: {bundleFile}");

                Level customLevel = LoadLevelFromBundle(bundleFile);
                if (customLevel != null)
                {
                    runManager.levels.Add(customLevel);
                    RepoLevelLoader.Logger.LogInfo($"[Repo Level Loader] Successfully added custom level: {customLevel.name}");
                }
                else
                {
                    RepoLevelLoader.Logger.LogError($"[Repo Level Loader] Failed to load level from: {bundleFile}");
                }
            }
        }

        private static Level LoadLevelFromBundle(string filePath)
        {
            AssetBundle bundle = AssetBundle.LoadFromFile(filePath);
            if (bundle == null)
            {
                RepoLevelLoader.Logger.LogError($"[Repo Level Loader] Failed to load AssetBundle: {filePath}");
                return null;
            }

            string[] assetNames = bundle.GetAllAssetNames();
            if (assetNames.Length == 0)
            {
                RepoLevelLoader.Logger.LogError($"[Repo Level Loader] No assets found in bundle: {filePath}");
                return null;
            }

            foreach (string assetName in assetNames)
            {
                RepoLevelLoader.Logger.LogInfo($"[Repo Level Loader] Found asset in bundle: {assetName}");
                Level loadedLevel = bundle.LoadAsset<Level>(assetName);
                if (loadedLevel != null)
                {
                    loadedLevel.name = Path.GetFileNameWithoutExtension(filePath);
                    return loadedLevel;
                }
            }

            RepoLevelLoader.Logger.LogError($"[Repo Level Loader] Failed to find a valid Level asset in bundle: {filePath}");
            return null;
        }
    }
}

// Ignore Spelling: MonsterAITweaks Jotunn

using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using MonsterAITweaks.Configs;
using MonsterAITweaks.Extensions;
using System.Reflection;
using UnityEngine;

namespace MonsterAITweaks
{

    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid, Jotunn.Main.Version)]
    internal sealed class MonsterAITweaks : BaseUnityPlugin
    {
        internal const string Author = "Searica";
        public const string PluginName = "MonsterAITweaks";
        public const string PluginGUID = $"{Author}.Valheim.{PluginName}";
        public const string PluginVersion = "0.0.2";

        // Use this class to add your own localization to the game
        // https://valheim-modding.github.io/Jotunn/tutorials/localization.html
        //public static CustomLocalization Localization = LocalizationManager.Instance.GetLocalization();

        private static readonly string MainSection = ConfigManager.SetStringPriority("Global", 3);


        public void Awake()
        {
            Log.Init(Logger);

            ConfigManager.Init(PluginGUID, Config);
            SetUpConfigEntries();

            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), harmonyInstanceId: PluginGUID);
            Game.isModded = true;

            ConfigManager.SetupWatcher();
            //ConfigManager.CheckForConfigManager();

        }

        public void OnDestroy()
        {
            ConfigManager.Save();
        }

        internal static void SetUpConfigEntries()
        {
            Log.Verbosity = ConfigManager.BindConfig(
                MainSection,
                "Verbosity",
                LogLevel.Low,
                "Low will log basic information about the mod. Medium will log information that " +
                "is useful for troubleshooting. High will log a lot of information, do not set " +
                "it to this without good reason as it will slow Down your game.",
                synced: true
            );

            //ConfigManager.BindConfig(
            //    MainSection,
            //    "ServerSyncBugFix",
            //    true,
            //    "Hidden value so a server sync'd config value gets bound before exiting Plugin.Awake",
            //    acceptVals: null,
            //    new ConfigurationManagerAttributes() { IsAdminOnly = true, Browsable = false }
            //);

            ConfigManager.Save();
        }
    }

    /// <summary>
    ///     Log level to control output to BepInEx log
    /// </summary>
    internal enum LogLevel
    {
        Low = 0,
        Medium = 1,
        High = 2,
    }

    /// <summary>
    ///     Helper class for properly logging from static contexts.
    /// </summary>
    internal static class Log
    {

        internal static ConfigEntry<LogLevel> Verbosity { get; set; }
        internal static LogLevel VerbosityLevel => Verbosity.Value;
        internal static bool IsVerbosityLow => Verbosity.Value >= LogLevel.Low;
        internal static bool IsVerbosityMedium => Verbosity.Value >= LogLevel.Medium;
        internal static bool IsVerbosityHigh => Verbosity.Value >= LogLevel.High;

        internal static ManualLogSource _logSource;

        internal static void Init(ManualLogSource logSource)
        {
            _logSource = logSource;
        }

        internal static void LogDebug(object data) => _logSource.LogDebug(data);

        internal static void LogError(object data) => _logSource.LogError(data);

        internal static void LogFatal(object data) => _logSource.LogFatal(data);

        internal static void LogMessage(object data) => _logSource.LogMessage(data);

        internal static void LogWarning(object data) => _logSource.LogWarning(data);

        internal static void LogInfo(object data, LogLevel level = LogLevel.Low)
        {
            if (Verbosity is null || VerbosityLevel >= level)
            {
                _logSource.LogInfo(data);
            }
        }

        internal static void LogGameObject(GameObject prefab, bool includeChildren = false)
        {
            LogInfo("***** " + prefab.name + " *****");
            foreach (Component compo in prefab.GetComponents<Component>())
            {
                LogComponent(compo);
            }

            if (!includeChildren) { return; }

            LogInfo("***** " + prefab.name + " (children) *****");
            foreach (Transform child in prefab.transform)
            {
                LogInfo($" - {child.gameObject.name}");
                foreach (Component compo in child.gameObject.GetComponents<Component>())
                {
                    LogComponent(compo);
                }
            }
        }

        internal static void LogComponent(Component compo)
        {
            LogInfo($"--- {compo.GetType().Name}: {compo.name} ---");

            PropertyInfo[] properties = compo.GetType().GetProperties(ReflectionUtils.AllBindings);
            foreach (var property in properties)
            {
                try
                {
                    LogInfo($" - {property.Name} = {property.GetValue(compo)}");
                }
                catch
                {
                    LogWarning($"Failed to get value for {property.Name}");
                }
            }

            FieldInfo[] fields = compo.GetType().GetFields(ReflectionUtils.AllBindings);
            foreach (var field in fields)
            {
                try
                {
                    LogInfo($" - {field.Name} = {field.GetValue(compo)}");
                }
                catch
                {
                    LogWarning($"Failed to get value for {field.Name}");
                }
            }
        }
    }
}
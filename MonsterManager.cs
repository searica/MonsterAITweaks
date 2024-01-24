using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using BepInEx.Configuration;
using HarmonyLib;
using MonsterAITweaks.Configs;
using MonsterAITweaks.Extensions;
using UnityEngine;

namespace MonsterAITweaks
{

    internal sealed class MonsterDB
    {
        public MonsterAI monsterAI;
        public float circleTargetDuration;
        public float circleTargetDistance;
        public float circleTargetInterval;
        public float minAttackInterval;

        public MonsterDB(MonsterAI monsterAI)
        {
            this.monsterAI = monsterAI;
            this.circleTargetDistance = monsterAI.m_circleTargetDistance;
            this.circleTargetDuration = monsterAI.m_circleTargetDuration;
            this.circleTargetInterval = monsterAI.m_circleTargetInterval;
            this.minAttackInterval = monsterAI.m_minAttackInterval;

        }
    }

    [HarmonyPatch]
    internal static class MonsterManager
    {
        private static readonly Dictionary<ConfigEntry<float>, MonsterDB> ConfigToMonsterDBMap = new();
        private static readonly Dictionary<string, ConfigEntry<float>> MonsterToConfigMap = new();
        private static readonly HashSet<string> IgnoredMonsters = new() { "TheHive" };
        private static readonly Dictionary<ItemDrop.ItemData, float> DefaultAiAttackIntervals = new();

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.Start))]
        private static void InitializeDB()
        {

            if (ConfigToMonsterDBMap.Count > 0)
            {
                return;
            }

            Log.LogInfo("InitalizeMonsterDB");
            var monsters = FindMonsters();
            ConfigManager.SaveOnConfigSet(false);

            foreach (var monster in monsters)
            {
                CreateConfigs(monster);
            }

            ConfigManager.Save();
            ConfigManager.SaveOnConfigSet(true);
        }

        /// <summary>
        ///     Create Evasion and Aggression Configs for each monster
        /// </summary>
        /// <param name="monster"></param>
        private static void CreateConfigs(GameObject monster)
        {
            if (!monster || !monster.TryGetComponent(out MonsterAI monsterAI))
            {
                return;
            }

            var monsterDB = new MonsterDB(monsterAI);

            var evasion = ConfigManager.BindConfig(
                monster.name,
                "Evasion",
                1f,
                "Multiplier for how evasive the creature is when not attacking.",
                new AcceptableValueRange<float>(0f, 10f)
            );
            ConfigToMonsterDBMap.Add(evasion, monsterDB);
            UpdateEvasion(evasion, null); // apply the multiplier
            evasion.SettingChanged += UpdateEvasion;

            var aggression = ConfigManager.BindConfig(
                monster.name,
                "Aggression",
                1f,
                "Multiplier for how aggressive the creature is and how frequently it attacks.",
                new AcceptableValueRange<float>(0.1f, 10f)
            );
            ConfigToMonsterDBMap.Add(aggression, monsterDB);
            MonsterToConfigMap.Add(monster.name, aggression);
            UpdateAggression(aggression, null); // apply the multiplier
            aggression.SettingChanged += UpdateAggression;
        }

        /// <summary>
        ///     Apply aggression multiplier to m_aiAttackInterval for all 
        ///     weapons in monster's inventory after monster spawns.
        /// </summary>
        /// <param name="__instance"></param>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(MonsterAI), nameof(MonsterAI.Start))]
        private static void EditWeaponsOnStart(MonsterAI __instance)
        {
            if (!__instance)
            {
                return;
            }

            var monsterName = __instance.GetPrefabName();
            if (!MonsterToConfigMap.TryGetValue(monsterName, out ConfigEntry<float> aggressionMult))
            {
                return;
            }

            if (__instance.m_character is Humanoid humanoid)
            {
                foreach (var item in humanoid.GetInventory().GetAllItems())
                {
                    if (item.IsWeapon() && item.IsEquipable())
                    {
                        item.m_shared.m_aiAttackInterval *= (1 / aggressionMult.Value);
                    }
                }
            }
        }

        /// <summary>
        ///     Get the ConfigEntry and MonsterDB from the sender of the SettingChanged event.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="config"></param>
        /// <param name="monsterDB"></param>
        /// <returns></returns>
        private static bool TryGetMonsterDB(object obj, out ConfigEntry<float> config, out MonsterDB monsterDB)
        {
            if (obj is ConfigEntry<float> cfg &&
                ConfigToMonsterDBMap.TryGetValue(cfg, out monsterDB) &&
                monsterDB.monsterAI)
            {
                config = cfg;
                return true;
            }
            config = null;
            monsterDB = null;
            return false;
        }

        /// <summary>
        ///     Apply evasion multiplier to MonsterAI on monster prefab when setting is changed.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="args"></param>
        private static void UpdateEvasion(object obj, EventArgs args)
        {
            if (TryGetMonsterDB(obj, out ConfigEntry<float> config, out MonsterDB monsterDB))
            {
                monsterDB.monsterAI.m_circleTargetDistance = monsterDB.circleTargetDistance * config.Value;
                monsterDB.monsterAI.m_circleTargetDuration = monsterDB.circleTargetDuration * config.Value;
            }
        }

        /// <summary>
        ///     Apply aggression multiplier to MonsterAI on monster prefab when setting is changed.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="args"></param>
        private static void UpdateAggression(object obj, EventArgs args)
        {
            if (TryGetMonsterDB(obj, out ConfigEntry<float> config, out MonsterDB monsterDB))
            {
                monsterDB.monsterAI.m_minAttackInterval = monsterDB.minAttackInterval * (1 / config.Value);
                monsterDB.monsterAI.m_circleTargetInterval = monsterDB.circleTargetInterval * (1 / config.Value);
            }
        }

        /// <summary>
        ///     Get all valid prefabs with MonsterAI component.
        /// </summary>
        /// <returns></returns>
        private static List<GameObject> FindMonsters()
        {
            var gameObjects = Resources.FindObjectsOfTypeAll<GameObject>(); // get all GameObjects
            var prefabs = gameObjects.Where(x => !x.transform.parent); // get objects without a parent (ie they are root prefabs)
                                                                       // Get prefabs that have monsterAI component and are not ignored
            var monsters = prefabs.Where(x => x.GetComponent<MonsterAI>());
            return monsters.Where(x => !IgnoredMonsters.Contains(x.name)).ToList();
        }
    }
}

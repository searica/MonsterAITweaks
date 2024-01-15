using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using HarmonyLib;
using MonsterAITweaks.Configs;
using MonsterAITweaks.Extensions;
using TMPro;
using UnityEngine;

namespace MonsterAITweaks {

    [HarmonyPatch]
    internal static class MonsterDB {
        private static readonly Dictionary<ConfigEntry<float>, GameObject> MonsterConfigMap = new();
        private static readonly HashSet<string> IgnoredMonsters = new() { "TheHive" };
        private static readonly Dictionary<ItemDrop, float> DefaultAiAttackIntervals = new();

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.Start))]
        private static void InitializeDB() {

            if (MonsterConfigMap.Count > 0) {
                return;
            }

            Log.LogInfo("InitalizeMonsterDB");
            var monsters = FindMonsters();
            ConfigManager.SaveOnConfigSet(false);

            foreach (var monster in monsters) {
                CreateConfigs(monster);
            }

            ConfigManager.Save();
            ConfigManager.SaveOnConfigSet(true);
        }

        private static void CreateConfigs(GameObject monster) {
            if (!monster || !monster.TryGetComponent(out MonsterAI monsterAI)) {
                return;
            }

            var circleDist = ConfigManager.BindConfig(
                monster.name,
                nameof(monsterAI.m_circleTargetDistance).RemovePrefix("m_"),
                monsterAI.m_circleTargetDistance,
                "Distance that monster circles target at.",
                new AcceptableValueRange<float>(0f, 1000f)
            );
            MonsterConfigMap.Add(circleDist, monster);
            monsterAI.m_circleTargetDistance = circleDist.Value;
            circleDist.SettingChanged += UpdateCircleDistance;

            var circleDuration = ConfigManager.BindConfig(
                monster.name,
                nameof(monsterAI.m_circleTargetDuration).RemovePrefix("m_"),
                monsterAI.m_circleTargetDuration,
                "Duration that monster circles target for..",
                new AcceptableValueRange<float>(0f, 1000f)
            );
            MonsterConfigMap.Add(circleDuration, monster);
            monsterAI.m_circleTargetDuration = circleDuration.Value;
            circleDuration.SettingChanged += UpdateCircleDuration;

            var circleInterval = ConfigManager.BindConfig(
                monster.name,
                nameof(monsterAI.m_circleTargetInterval).RemovePrefix("m_"),
                monsterAI.m_circleTargetInterval,
                "Maximum time before monster pauses attacking and circles target again, will not pause if set to 0.",
                new AcceptableValueRange<float>(0f, 1000f)
            );
            MonsterConfigMap.Add(circleInterval, monster);
            monsterAI.m_circleTargetInterval = circleInterval.Value;
            circleInterval.SettingChanged += UpdateCircleInterval;


            var attackInterval = ConfigManager.BindConfig(
                monster.name,
                nameof(monsterAI.m_minAttackInterval).RemovePrefix("m_"),
                monsterAI.m_minAttackInterval,
                "Minimum time before monster can attack again.",
                new AcceptableValueRange<float>(0f, 1000f)
            );
            MonsterConfigMap.Add(attackInterval, monster);
            monsterAI.m_minAttackInterval = attackInterval.Value;
            attackInterval.SettingChanged += UpdateAttackInterval;


            var weaponIntervalMultiplier = ConfigManager.BindConfig(
                monster.name,
                "weaponIntervalMultiplier",
                1f,
                "Multiplier for time before monster can use the same the attack again.",
                new AcceptableValueRange<float>(0f, 2f)
            );

            MonsterConfigMap.Add(weaponIntervalMultiplier, monster);

            var humanoid = monsterAI.m_character as Humanoid;
            foreach (var item in humanoid.m_defaultItems) {
                if (item.TryGetComponent(out ItemDrop itemDrop)) {
                    DefaultAiAttackIntervals[itemDrop] = itemDrop.m_itemData.m_shared.m_aiAttackInterval;
                    itemDrop.m_itemData.m_shared.m_aiAttackInterval = DefaultAiAttackIntervals[itemDrop] * weaponIntervalMultiplier.Value;
                }
            }

            weaponIntervalMultiplier.SettingChanged += UpdateWpnAiAttackInterval;
        }

        //private static bool IsWeapon(ItemDrop item) {
        //    var itemType = item.m_itemData.m_shared.m_itemType;
        //    switch (itemType) {
        //        case ItemDrop.ItemData.ItemType.OneHandedWeapon: return true;
        //        case ItemDrop.ItemData.ItemType.TwoHandedWeapon: return true;
        //        case ItemDrop.ItemData.ItemType.Bow: return true;
        //        case ItemDrop.ItemData.ItemType.Torch: return true;
        //        case ItemDrop.ItemData.ItemType.Torch: return true;

        //    }
        //}

        private static bool TryGetMonsterConfig(object obj, out ConfigEntry<float> config, out GameObject monster) {
            if (obj is ConfigEntry<float> cfg &&
                MonsterConfigMap.TryGetValue(cfg, out monster) &&
                monster) {
                config = cfg;
                return true;
            }
            config = null;
            monster = null;
            return false;
        }

        private static void UpdateCircleDistance(object obj, EventArgs args) {
            if (TryGetMonsterConfig(obj, out ConfigEntry<float> config, out GameObject monster)) {
                monster.GetComponent<MonsterAI>().m_circleTargetDistance = config.Value;
                Log.LogInfo($"Set {monster.name} circle distance to {config.Value}", LogLevel.High);
            }
        }

        private static void UpdateCircleDuration(object obj, EventArgs args) {
            if (TryGetMonsterConfig(obj, out ConfigEntry<float> config, out GameObject monster)) {
                monster.GetComponent<MonsterAI>().m_circleTargetDuration = config.Value;
                Log.LogInfo($"Set {monster.name} circle duration to {config.Value}", LogLevel.High);
            }
        }

        private static void UpdateCircleInterval(object obj, EventArgs args) {
            if (TryGetMonsterConfig(obj, out ConfigEntry<float> config, out GameObject monster)) {
                monster.GetComponent<MonsterAI>().m_circleTargetInterval = config.Value;
                Log.LogInfo($"Set {monster.name} circle interval to {config.Value}", LogLevel.High);
            }
        }

        private static void UpdateAttackInterval(object obj, EventArgs args) {
            if (TryGetMonsterConfig(obj, out ConfigEntry<float> config, out GameObject monster)) {
                monster.GetComponent<MonsterAI>().m_minAttackInterval = config.Value;
                Log.LogInfo($"Set {monster.name} min attack interval to {config.Value}", LogLevel.High);
            }
        }

        private static void UpdateWpnAiAttackInterval(object obj, EventArgs args) {
            if (TryGetMonsterConfig(obj, out ConfigEntry<float> config, out GameObject monster)) {
                var monsterAI = monster.GetComponent<MonsterAI>();

                var humanoid = monsterAI.m_character as Humanoid;
                foreach (var item in humanoid.m_defaultItems) {
                    if (item.TryGetComponent(out ItemDrop itemDrop)) {
                        itemDrop.m_itemData.m_shared.m_aiAttackInterval = DefaultAiAttackIntervals[itemDrop] * config.Value;
                    }
                }
            }
        }

        private static List<GameObject> FindMonsters() {
            var gameObjects = Resources.FindObjectsOfTypeAll<GameObject>(); // get all GameObjects
            var prefabs = gameObjects.Where(x => !x.transform.parent); // get objects without a parent (ie they are root prefabs)
            // Get prefabs that have monsterAI component and are not ignored
            var monsters = prefabs.Where(x => x.GetComponent<MonsterAI>());
            return monsters.Where(x => !IgnoredMonsters.Contains(x.name)).ToList();
        }
    }
}

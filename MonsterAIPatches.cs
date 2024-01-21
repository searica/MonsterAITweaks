//using HarmonyLib;

//namespace MonsterAITweaks {

//    [HarmonyPatch]
//    internal static class MonsterAIPatches {

//        [HarmonyPostfix]
//        [HarmonyPatch(typeof(MonsterAI), nameof(MonsterAI.Awake))]
//        private static void AwakeStatus(MonsterAI __instance) {
//            if (!__instance) {
//                return;
//            }
//            __instance.m_aiStatus = "Awake";
//        }

//        [HarmonyPostfix]
//        [HarmonyPatch(typeof(MonsterAI), nameof(MonsterAI.UpdateAI))]
//        private static void UpdateAIPostfix(MonsterAI __instance, float dt) {
//            if (!__instance) {
//                return;
//            }

//            if (__instance.name.Contains("Deathsq")) {
//                Log.LogInfo($"AI Status: {__instance.m_aiStatus}");
//            }
//        }
//    }
//}

using UnityEngine;

namespace MonsterAITweaks.Extensions {
    internal static class ComponentExtensions {

        /// <summary>
        ///     Get root prefab name for Valheim game object.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public static string GetPrefabName(this Component comp) {
            return Utils.GetPrefabName(comp.gameObject);
        }
    }
}

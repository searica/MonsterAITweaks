﻿using System;
using UnityEngine;


namespace MonsterAITweaks.Extensions {
    /// <summary>
    ///     Extends GameObject with a shortcut for the Unity bool operator override.
    /// </summary>
    public static class GameObjectExtensions {

        /// <summary>
        ///     Get root prefab name for Valheim game object.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public static string GetPrefabName(this GameObject gameObject) {
            return Utils.GetPrefabName(gameObject);
        }

        /// <summary>
        ///     Check if GameObject has any of the specified components.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="components"></param>
        /// <returns></returns>
        public static bool HasAnyComponent(this GameObject gameObject, params Type[] components) {
            foreach (var compo in components) {
                if (gameObject.GetComponent(compo)) {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        ///     Check if GameObject has any of the specified components.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="componentNames"></param>
        /// <returns></returns>
        public static bool HasAnyComponent(this GameObject gameObject, params string[] componentNames) {
            foreach (var name in componentNames) {
                if (gameObject.GetComponent(name)) {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        ///     Check if GameObject has all of the specified components.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="componentNames"></param>
        /// <returns></returns>
        public static bool HasAllComponents(this GameObject gameObject, params string[] componentNames) {
            foreach (var name in componentNames) {
                if (!gameObject.GetComponent(name)) {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        ///     Check if GameObject has all of the specified components.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="components"></param>
        /// <returns></returns>
        public static bool HasAllComponents(this GameObject gameObject, params Type[] components) {
            foreach (var compo in components) {
                if (!gameObject.GetComponent(compo)) {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        ///     Check if GameObject or any of it's children
        ///     have any of the specified components.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="includeInactive"></param>
        /// <param name="components"></param>
        /// <returns></returns>
        public static bool HasAnyComponentInChildren(
            this GameObject gameObject,
            bool includeInactive = false,
            params Type[] components
        ) {
            foreach (var compo in components) {
                if (gameObject.GetComponentInChildren(compo, includeInactive)) {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        ///     Extension method to find nested children by name using either
        ///     a breadth-first or depth-first search. Default is breadth-first.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="childName">Name of the child object to search for.</param>
        /// <param name="searchType">Whether to preform a breadth first or depth first search. Default is breadth first.</param>
        public static Transform FindDeepChild(
            this GameObject gameObject,
            string childName,
            global::Utils.IterativeSearchType searchType = global::Utils.IterativeSearchType.BreadthFirst
        ) {
            return gameObject.transform.FindDeepChild(childName, searchType);
        }
    }
}
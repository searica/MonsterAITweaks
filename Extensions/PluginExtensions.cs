﻿// Ignore Spelling: MVBP

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MonsterAITweaks.Extensions {
    internal static class TypeExtensions {
        internal static List<T> GetAllPublicConstantValues<T>(this Type type) {
            return type
                .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                .Where(fi => fi.IsLiteral && !fi.IsInitOnly && fi.FieldType == typeof(T))
                .Select(x => (T)x.GetRawConstantValue())
                .ToList();
        }

        internal static List<T> GetAllPublicStaticValues<T>(this Type type) {
            return type
                .GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(fi => fi.FieldType == typeof(T))
                .Select(x => (T)x.GetValue(null))
                .ToList();
        }
    }

    internal static class GenericExtensions {
        internal static T Ref<T>(this T o) where T : UnityEngine.Object {
            return o ? o : null;
        }
    }

    internal static class IEnumerableExtensions {
        internal static void Dispose(this IEnumerable<IDisposable> collection) {
            foreach (IDisposable item in collection) {
                if (item != null) {
                    try {
                        item.Dispose();
                    }
                    catch (Exception) {
                        Log.LogWarning("Could not dispose of item");
                    }
                }
            }
        }
    }
}
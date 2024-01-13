using System;
using System.Reflection;
using HarmonyLib;

namespace Enhancer.Extensions;

public static class HarmonyExtensions
{
    private const BindingFlags SearchNestedTypeBindingFlags = BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic;

    public static void PatchAllNestedTypesOnly(this Harmony harmony, Type type)
    {
        foreach (var nestedType in type.GetNestedTypes(SearchNestedTypeBindingFlags))
        {
            PatchAllWithNestedTypes(harmony, nestedType);
        }
    }

    public static void PatchAllWithNestedTypes(this Harmony harmony, Type type)
    {
        harmony.PatchAll(type);
        PatchAllNestedTypesOnly(harmony, type);
    }
}

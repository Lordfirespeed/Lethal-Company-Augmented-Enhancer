using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace Enhancer.Patches;

[HarmonyPatch(typeof(RoundManager), nameof(RoundManager.DespawnPropsAtEndOfRound))]
public class ItemProtection : IPatch
{
    public static bool IsUnprotectedScrap(Item item)
    {
        Plugin.Log.LogDebug($"Considering item {item} for destruction...");
        return item.isScrap && !ShouldSaveScrap();
    }
    
    static readonly FieldInfo ItemIsScrapField = AccessTools.Field(typeof(Item), nameof(Item.isScrap));
    private static readonly MethodInfo ItemIsUnprotectedScrapMethod = AccessTools.Method(typeof(ItemProtection), nameof(IsUnprotectedScrap));
    
    [HarmonyTranspiler]
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        return instructions.Manipulator(
            instruction => instruction.opcode == OpCodes.Ldfld && instruction.OperandIs(ItemIsScrapField),
            instruction =>
            {
                instruction.opcode = OpCodes.Call;
                instruction.operand = ItemIsUnprotectedScrapMethod;
            }
        );
    }
    
    [HarmonyPrefix]
    static void Prefix(RoundManager __instance, bool despawnAllItems)
    {
        Plugin.Log.LogInfo("Getting ready to consider items for destruction");
        
        if (despawnAllItems) return;
        if (!StartOfRound.Instance.allPlayersDead) return;
        
        ThisPassProtectionProbability = Plugin.BoundConfig.ScrapProtection.Value;
        if (
            Plugin.BoundConfig.ScrapProtectionRandomness.Value <= 0f && 
            (Plugin.BoundConfig.ScrapProtection.Value <= 0f || Plugin.BoundConfig.ScrapProtection.Value >= 1f)
        ) return;
        
        RandomGenerator = new System.Random(StartOfRound.Instance.randomMapSeed + 83);
        
        if (Plugin.BoundConfig.ScrapProtectionRandomness.Value <= 0f) return;
        // get randomly from the quota randomizer curve (which is approximately the same shape as the quantile function)
        ThisPassProtectionProbability += (
            Plugin.BoundConfig.ScrapProtectionRandomness.Value * 2 * 
            TimeOfDay.Instance.quotaVariables.randomizerCurve.Evaluate((float)RandomGenerator.NextDouble())
        );
        ThisPassProtectionProbability = Mathf.Clamp(ThisPassProtectionProbability!.Value, 0f, 1f);
    }
    
    [HarmonyPostfix]
    static void Postfix()
    {
        Plugin.Log.LogInfo("Finished considering items for destruction");
        RandomGenerator = null;
        ThisPassProtectionProbability = null;
    }
    
    private static System.Random? RandomGenerator { get; set; }
    private static float? ThisPassProtectionProbability { get; set; }

    public static bool ShouldSaveScrap()
    {
        return Plugin.BoundConfig.ScrapProtection.Value switch
        {
            0f => false,
            1f => true,
            _ => ThisPassProtectionProbability > RandomGenerator?.NextDouble()
        };
    }
}
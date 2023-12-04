using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace Enhancer.Patches;

public static class ItemProtection
{
    public static bool IsUnprotectedScrap(Item item)
    {
        Plugin.Log.LogInfo($"Considering item {item} for destruction...");
        return item.isScrap && !ShouldSaveScrap();
    }

    [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.DespawnPropsAtEndOfRound))]
    public static class DespawnPropsAtEndOfRoundPatch
    {
        static FieldInfo itemIsScrapField = AccessTools.Field(typeof(Item), nameof(Item.isScrap));
        private static MethodInfo itemIsUnprotectedScrapMethod = AccessTools.Method(typeof(ItemProtection), nameof(IsUnprotectedScrap));
        
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return instructions.Manipulator(
                instruction => instruction.opcode == OpCodes.Ldfld && instruction.OperandIs(itemIsScrapField),
                instruction =>
                {
                    instruction.opcode = OpCodes.Call;
                    instruction.operand = itemIsUnprotectedScrapMethod;
                }
            );
        }
        
        [HarmonyPrefix]
        static void Prefix(RoundManager __instance, bool despawnAllItems)
        {
            Plugin.Log.LogInfo("Getting ready to consider items for destruction");
            
            if (despawnAllItems) return;
            if (StartOfRound.Instance.allPlayersDead) return;
            if (Plugin.BoundConfig.ScrapProtection == 0f || Plugin.BoundConfig.ScrapProtection >= 1f) return;
            
            RandomGenerator = new System.Random(StartOfRound.Instance.randomMapSeed + 83);
        }
        
        [HarmonyPostfix]
        static void Postfix()
        {
            Plugin.Log.LogInfo("Finished considering items for destruction");
            RandomGenerator = null;
        }
    }

    private static System.Random? RandomGenerator { get; set; }
    
    public static bool ShouldSaveScrap()
    {
        return Plugin.BoundConfig.ScrapProtection switch
        {
            0 => false,
            1 => true,
            _ => Plugin.BoundConfig.ScrapProtection > RandomGenerator?.NextDouble()
        };
    }
}
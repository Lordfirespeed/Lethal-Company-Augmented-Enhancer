using HarmonyLib;
using LC_API.GameInterfaceAPI.Events.EventArgs.Player;

namespace Enhancer.Patches;

public class ScrapTweaks : BasePatch
{
    private static float _originalScrapValueMultiplier = 1f;
    private static float _originalScrapAmountMultiplier = 1f;

    private bool RoundManagerExistsAndIsServer()
    {
        if (!RoundManager.Instance) return false;
        if (!RoundManager.Instance.playersManager) return false;
        if (!RoundManager.Instance.playersManager.IsHost && !RoundManager.Instance.playersManager.IsServer) return false;
        return true;
    }

    public void OnPatch()
    {
        if (!RoundManagerExistsAndIsServer()) return;
        
        SubscribeToEvents();
        CacheMultipliers(RoundManager.Instance);
        ApplyMultipliers(RoundManager.Instance.playersManager.connectedPlayersAmount);
    }

    public void OnConfigChange()
    {
        if (!RoundManagerExistsAndIsServer()) return;
        
        ApplyMultipliers(RoundManager.Instance.playersManager.connectedPlayersAmount);
    }

    public void OnUnpatch()
    {
        if (!RoundManagerExistsAndIsServer()) return;
        
        UnsubscribeFromEvents();
        RestoreMultipliers(RoundManager.Instance);
    }

    [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.Awake))]
    [HarmonyPrefix]
    public static void RoundManagerAwaken(RoundManager __instance)
    {
        Logger.LogDebug("Round manager awoke!");
        if (!__instance.playersManager.IsHost && !__instance.playersManager.IsServer) return;
        Logger.LogDebug("I am the server!");
        
        SubscribeToEvents();
        CacheMultipliers(__instance);
        ApplyMultipliers(RoundManager.Instance.playersManager.connectedPlayersAmount);
    }
    
    [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.OnDestroy))]
    [HarmonyPrefix]
    public static void RoundManagerDestruction(RoundManager __instance)
    {
        Logger.LogDebug("Round manager destroying!");
        if (!__instance.playersManager.IsHost && !__instance.playersManager.IsServer) return;
        Logger.LogDebug("I am the server!");
        
        UnsubscribeFromEvents();
    }

    private static void SubscribeToEvents()
    {
        Logger.LogDebug("Subscribed to join/leave!");
        LC_API.GameInterfaceAPI.Events.Handlers.Player.Joined += OnPlayerJoin;
        LC_API.GameInterfaceAPI.Events.Handlers.Player.Left += OnPlayerLeave;
    }

    private static void UnsubscribeFromEvents()
    {
        LC_API.GameInterfaceAPI.Events.Handlers.Player.Joined -= OnPlayerJoin;
        LC_API.GameInterfaceAPI.Events.Handlers.Player.Left -= OnPlayerLeave;
    }

    private static void CacheMultipliers(RoundManager manager)
    {
        _originalScrapValueMultiplier = manager.scrapValueMultiplier;
        _originalScrapAmountMultiplier = manager.scrapAmountMultiplier;
    }

    private static void RestoreMultipliers(RoundManager manager)
    {
        try
        {
            manager.scrapValueMultiplier = _originalScrapValueMultiplier; 
            manager.scrapAmountMultiplier = _originalScrapAmountMultiplier;
        }
        finally
        {
            Logger.LogDebug($"Attempted to restore scrap value multiplier. Value is now {RoundManager.Instance.scrapValueMultiplier}");
            Logger.LogDebug($"Attempted to restore scrap quantity multiplier. Value is now {RoundManager.Instance.scrapAmountMultiplier}");
        }
    }

    private static void OnPlayerJoin(JoinedEventArgs args)
    {
        OnPlayerCountChanged();
    }

    private static void OnPlayerLeave(LeftEventArgs args)
    {
        OnPlayerCountChanged();
    }

    private static void OnPlayerCountChanged()
    {
        if (!StartOfRound.Instance) return;
        var playerCount = StartOfRound.Instance.connectedPlayersAmount;
        ApplyMultipliers(playerCount);
    }

    private static void ApplyMultipliers(int playerCount)
    {
        if (!RoundManager.Instance) return;
        try
        {
            RoundManager.Instance.scrapValueMultiplier = _originalScrapValueMultiplier * Plugin.BoundConfig.ScrapValueScalar.Value;
            RoundManager.Instance.scrapAmountMultiplier = _originalScrapAmountMultiplier * Plugin.BoundConfig.ScrapQuantityScalar.Value;

            if (playerCount <= 4 || Plugin.BoundConfig.ScrapPlayercountScaling.Value == 0f) return;

            var extraPlayers = playerCount - 4;

            RoundManager.Instance.scrapValueMultiplier +=
                RoundManager.Instance.scrapValueMultiplier * Plugin.BoundConfig.ScrapPlayercountScaling.Value * extraPlayers / 4;
            RoundManager.Instance.scrapAmountMultiplier -=
                RoundManager.Instance.scrapValueMultiplier * Plugin.BoundConfig.ScrapPlayercountScaling.Value * extraPlayers / playerCount;
        }
        finally
        {
            Logger.LogDebug($"Attempted to update scrap value multiplier. Value is now {RoundManager.Instance.scrapValueMultiplier}");
            Logger.LogDebug($"Attempted to update scrap quantity multiplier. Value is now {RoundManager.Instance.scrapAmountMultiplier}");
        }
    }
}
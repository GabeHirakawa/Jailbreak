using CounterStrikeSharp.API.Core;
using Jailbreak.Contracts.Services;

namespace Jailbreak.Core.Services.Warden;

/// <summary>
/// Stub implementation. Will be replaced with real warden logic in Task 8.
/// </summary>
public class WardenServiceStub : IWardenService {
    public CCSPlayerController? Warden { get; private set; }
    public bool HasWarden => Warden != null;

    public bool IsWarden(CCSPlayerController player) {
        return Warden != null && Warden == player;
    }

    public bool TrySetWarden(CCSPlayerController controller) {
        if (HasWarden) return false;
        Warden = controller;
        return true;
    }

    public bool TryRemoveWarden(bool dead = false) {
        if (!HasWarden) return false;
        Warden = null;
        return true;
    }
}

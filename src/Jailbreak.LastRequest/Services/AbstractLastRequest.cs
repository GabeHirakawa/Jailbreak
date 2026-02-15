using CounterStrikeSharp.API.Core;
using Jailbreak.LastRequest.Enums;

namespace Jailbreak.LastRequest.Services;

public abstract class AbstractLastRequest(BasePlugin plugin,
  ILastRequestManager manager, CCSPlayerController prisoner,
  CCSPlayerController guard) {
  protected readonly ILastRequestManager Manager = manager;
  protected readonly BasePlugin Plugin = plugin;
  public CCSPlayerController Prisoner { get; protected set; } = prisoner;
  public CCSPlayerController Guard { get; protected set; } = guard;
  public abstract LRType Type { get; }

  public LRState State { get; protected set; }

  public virtual bool PreventEquip(CCSPlayerController player,
    CCSWeaponBaseVData weapon) {
    if (State == LRState.PENDING) return false;
    return player == Prisoner || player == Guard;
  }

  public void PrintToParticipants(string message) {
    Prisoner.PrintToChat(message);
    Guard.PrintToChat(message);
  }

  public abstract void Setup();
  public abstract void Execute();
  public abstract void OnEnd(LRResult result);
}

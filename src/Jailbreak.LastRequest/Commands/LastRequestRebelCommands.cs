using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Contracts.Extensions;
using Jailbreak.Contracts.Formatting.Extensions;
using Jailbreak.Contracts.Models;
using Jailbreak.LastRequest.Locale;
using Jailbreak.LastRequest.Services;

namespace Jailbreak.LastRequest.Commands;

public class LastRequestRebelCommands {
  private readonly ILastRequestManager lastRequestManager;
  private readonly ILastRequestRebelManager lastRequestRebelManager;
  private readonly ILastRequestLocale messages;
  private readonly BasePlugin plugin;
  private readonly Dictionary<int, int> rebellerHealths = [];

  public LastRequestRebelCommands(ILastRequestManager lastRequestManager,
    ILastRequestRebelManager lastRequestRebelManager,
    ILastRequestLocale messages, BasePlugin plugin) {
    this.lastRequestManager      = lastRequestManager;
    this.lastRequestRebelManager = lastRequestRebelManager;
    this.messages                = messages;
    this.plugin                  = plugin;

    plugin.RegisterListener<Listeners.OnEntityParentChanged>(OnDrop);
  }

  private void OnDrop(CEntityInstance entity, CEntityInstance newparent) {
    if (!entity.IsValid || !WeaponTag.WEAPONS.Contains(entity.DesignerName))
      return;

    var weapon = Utilities.GetEntityFromIndex<CCSWeaponBase>((int)entity.Index);
    if (weapon == null
      || weapon.PrevOwner.Get()?.OriginalController.Get() == null)
      return;

    var owner = weapon.PrevOwner.Get()?.OriginalController.Get();
    if (owner == null || newparent.IsValid) return;

    if (!rebellerHealths.TryGetValue(owner.Slot, out var hp)) return;
    if (owner.Pawn.Value != null)
      owner.SetHealth(Math.Min(hp, owner.Pawn.Value.Health));

    rebellerHealths.Remove(owner.Slot);
  }

  public void Command_Rebel(CCSPlayerController? rebeller, CommandInfo info) {
    if (rebeller == null || !rebeller.IsReal()) return;
    if (!LastRequestRebelManager.CV_REBEL_ON.Value) {
      messages.LastRequestRebelDisabled().ToChat(rebeller);
      return;
    }

    if (rebeller.Team != CsTeam.Terrorist) {
      messages.CannotLastRequestRebelCt().ToChat(rebeller);
      return;
    }

    if (!lastRequestManager.IsLREnabled || !rebeller.PawnIsAlive) {
      messages.LastRequestNotEnabled().ToChat(rebeller);
      return;
    }

    if (lastRequestManager.IsInLR(rebeller)
      || lastRequestRebelManager.IsInLRRebelling(rebeller.Slot)) {
      messages.CannotLR("You are already in an LR").ToChat(rebeller);
      return;
    }

    if (Utilities.GetPlayers()
       .Count(p
          => p.IsReal() && p is { PawnIsAlive: true, Team: CsTeam.Terrorist })
      > 1) {
      messages.CannotLR("You must be the last alive to !rebel")
       .ToChat(rebeller);
      return;
    }

    if (rebeller.Pawn.Value != null)
      rebellerHealths[rebeller.Slot] = rebeller.Pawn.Value.Health;
    lastRequestRebelManager.StartLRRebelling(rebeller);
  }

  public HookResult OnRoundEnd(EventRoundEnd @event, GameEventInfo info) {
    lastRequestRebelManager.ClearLRRebelling();
    rebellerHealths.Clear();
    return HookResult.Continue;
  }
}

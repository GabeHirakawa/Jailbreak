using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Cvars.Validators;
using CounterStrikeSharp.API.Modules.Menu;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Contracts;
using Jailbreak.Contracts.Extensions;
using Jailbreak.Contracts.Formatting.Extensions;
using Jailbreak.Contracts.Validators;
using Jailbreak.LastRequest.Locale;

namespace Jailbreak.LastRequest.Services;

public class LastRequestRebelManager(ILastRequestLocale messages)
  : ILastRequestRebelManager {
  public static readonly FakeConVar<string> CV_REBEL_WEAPON =
    new("css_jb_rebel_t_weapon", "Weapon to give to rebeller during LR",
      "weapon_m249", ConVarFlags.FCVAR_NONE, new ItemValidator());

  public static readonly FakeConVar<bool> CV_REBEL_ON = new("css_jb_rebel_on",
    "If true, rebelling will be enabled during LR", true);

  public static readonly FakeConVar<int> CV_MAX_CT_HEALTH_CONTRIBUTION = new(
    "css_jb_rebel_ct_max_hp", "Max HP to contribute per CT to rebeller", 200,
    ConVarFlags.FCVAR_NONE, new RangeValidator<int>(1, 1000));

  public static readonly FakeConVar<double> CV_T_HEALTH_RATIO = new(
    "css_jb_rebel_t_hp_ratio", "Ratio of T : CT Health", 0.7,
    ConVarFlags.FCVAR_NONE, new RangeValidator<double>(0.00001, 10));

  public static readonly FakeConVar<int> CV_MAX_T_HEALTH =
    new("css_jb_rebel_t_max_hp", "Max HP that the rebeller can have otherwise",
      800, ConVarFlags.FCVAR_NONE, new RangeValidator<int>(1, 1000));

  public HashSet<int> PlayersLRRebelling { get; } = [];

  public void StartLRRebelling(CCSPlayerController player) {
    MenuManager.CloseActiveMenu(player);
    var hp = getHealthForPlayer(player);
    messages.LastRequestRebel(player, hp).ToAllChat();

    var core = JailbreakApi.Core.Get();
    core?.MarkRebel(player);

    ((ILastRequestRebelManager)this).AddLRRebelling(player.Slot);

    player.SetHealth(hp);
    player.RemoveWeapons();
    player.GiveNamedItem(CV_REBEL_WEAPON.Value);
    player.GiveNamedItem("weapon_knife");
  }

  private int getHealthForPlayer(CCSPlayerController player) {
    var hpRatio    = getHealthForRatio();
    var playerPawn = player.PlayerPawn.Value;
    if (playerPawn == null) return 101;

    hpRatio = Math.Max(hpRatio, playerPawn.Health);
    hpRatio = Math.Min(hpRatio, CV_MAX_T_HEALTH.Value);

    return hpRatio;
  }

  private int getHealthForRatio() {
    var aliveCounterTerrorists = Utilities.GetPlayers()
     .Where(plr => plr is { PawnIsAlive: true, Team: CsTeam.CounterTerrorist })
     .ToList();

    return (int)Math.Floor(aliveCounterTerrorists
     .Select(player => player.PlayerPawn.Value?.Health ?? 0)
     .Select(playerHealth
        => Math.Min(playerHealth, CV_MAX_CT_HEALTH_CONTRIBUTION.Value))
     .Sum() * CV_T_HEALTH_RATIO.Value);
  }
}

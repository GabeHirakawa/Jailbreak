using System.Drawing;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using Jailbreak.Contracts.Extensions;
using Jailbreak.Contracts.Formatting.Extensions;
using Jailbreak.Core.Services.State;
using Jailbreak.Core.Services.Stubs;
using Microsoft.Extensions.DependencyInjection;

namespace Jailbreak.Core.Services.Warden.SpecialTreatment;

/// <summary>
/// Manages special treatment (ST) for prisoners.
/// Migrated from Jailbreak.Warden.SpecialTreatment.SpecialTreatmentBehavior.
/// </summary>
public class SpecialTreatmentService : ISpecialTreatmentService {
  private readonly ISpecialIcon? iconer;
  private readonly IPlayerState<SpecialTreatmentState> sts;
  private readonly IWardenSTLocale notifications;
  private readonly IServiceProvider provider;
  private IRebelService rebel = null!;

  public SpecialTreatmentService(IPlayerStateFactory factory,
    IWardenSTLocale notifications, IServiceProvider provider) {
    this.notifications = notifications;
    this.provider = provider;
    iconer = provider.GetService<ISpecialIcon>();
    sts = factory.Round<SpecialTreatmentState>();
  }

  /// <summary>
  /// Initialize the service. Called from CorePlugin.Load().
  /// </summary>
  public void Initialize() {
    rebel = provider.GetRequiredService<IRebelService>();
  }

  public bool IsSpecialTreatment(CCSPlayerController player) {
    return sts.Get(player).HasSpecialTreatment;
  }

  public void Grant(CCSPlayerController player) {
    if (IsSpecialTreatment(player)) return;

    sts.Get(player).HasSpecialTreatment = true;

    if (rebel.IsRebel(player)) rebel.UnmarkRebel(player);
    setSpecialColor(player, true);
    player.ColorScreen(Color.FromArgb(16, 0, 255, 0), 999999, 0.2f,
      PlayerExtensions.FadeFlags.FADE_OUT);

    notifications.Granted.ToChat(player).ToCenter(player);
    notifications.GrantedTo(player).ToAllChat();

    iconer?.AssignSpecialIcon(player);
  }

  public void Revoke(CCSPlayerController player, bool print) {
    if (!IsSpecialTreatment(player)) return;

    sts.Get(player).HasSpecialTreatment = false;

    setSpecialColor(player, false);
    player.ColorScreen(Color.FromArgb(16, 0, 255, 0), 0f, 1.5f);

    if (print) {
      notifications.Revoked.ToChat(player).ToCenter(player);
      notifications.RevokedFrom(player).ToAllChat();
    }

    iconer?.RemoveSpecialIcon(player);
  }

  [GameEventHandler]
  public HookResult OnDeath(EventPlayerDeath ev, GameEventInfo info) {
    if (ev.Userid == null || !ev.Userid.IsValid) return HookResult.Continue;
    Revoke(ev.Userid, false);

    return HookResult.Continue;
  }

  private void setSpecialColor(CCSPlayerController player, bool hasSt) {
    if (!player.IsValid || player.Pawn.Value == null) return;

    var color = hasSt ?
      Color.FromArgb(255, 0, 255, 0) :
      Color.FromArgb(255, 255, 255, 255);

    player.SetColor(color);
  }

  private class SpecialTreatmentState {
    public bool HasSpecialTreatment { get; set; }
  }
}

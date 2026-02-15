using System.Drawing;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using Jailbreak.Contracts.Extensions;
using Jailbreak.Contracts.Formatting.Extensions;
using Jailbreak.Core.Services.State;
using Jailbreak.Core.Services.Rebel;
using Jailbreak.Core.Locale;
using Microsoft.Extensions.DependencyInjection;

namespace Jailbreak.Core.Services.Warden.SpecialTreatment;

/// <summary>
/// Manages special treatment (ST) for prisoners.
/// Migrated from Jailbreak.Warden.SpecialTreatment.SpecialTreatmentBehavior.
/// </summary>
public class SpecialTreatmentService : ISpecialTreatmentService {
  private readonly ISpecialIcon? iconer;
  private readonly IPlayerState<SpecialTreatmentState> sts;
  private readonly ICoreLocale locale;
  private readonly IServiceProvider provider;
  private IRebelService rebel = null!;

  public SpecialTreatmentService(IPlayerStateFactory factory,
    ICoreLocale locale, IServiceProvider provider) {
    this.locale = locale;
    this.provider = provider;
    iconer = provider.GetService<ISpecialIcon>();
    sts = factory.Round<SpecialTreatmentState>();
  }

  /// <summary>
  /// Initialize the service with the rebel service reference.
  /// Called from CorePlugin.Load() after RebelService is created.
  /// </summary>
  public void Initialize(IRebelService rebelService) {
    rebel = rebelService;
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

    locale.STGranted.ToChat(player).ToCenter(player);
    locale.STGrantedTo(player).ToAllChat();

    iconer?.AssignSpecialIcon(player);
  }

  public void Revoke(CCSPlayerController player, bool print) {
    if (!IsSpecialTreatment(player)) return;

    sts.Get(player).HasSpecialTreatment = false;

    setSpecialColor(player, false);
    player.ColorScreen(Color.FromArgb(16, 0, 255, 0), 0f, 1.5f);

    if (print) {
      locale.STRevoked.ToChat(player).ToCenter(player);
      locale.STRevokedFrom(player).ToAllChat();
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

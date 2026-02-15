using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Contracts.Formatting;
using Jailbreak.Contracts.Formatting.Objects;
using Jailbreak.Fun.Services.RTD;

namespace Jailbreak.Fun.Locale;

public class RTDLocale {
  public static readonly FormatObject PREFIX =
    new HiddenFormatObject($" {ChatColors.DarkBlue}RTD>") {
      Plain = false, Panorama = false, Chat = true
    };

  public IView RewardSelected(IRTDReward reward) {
    var view = new SimpleView {
      PREFIX,
      "You rolled " + ChatColors.BlueGrey + reward.Name + ChatColors.Grey + "."
    };
    if (reward.Description == null) return view;
    view.Add(SimpleView.NEWLINE);
    view.Add(PREFIX);
    view.Add(ChatColors.Grey + reward.Description);
    return view;
  }

  public IView AlreadyRolled(IRTDReward reward) {
    return new SimpleView {
      PREFIX,
      "You already rolled " + ChatColors.Red + reward.Name + ChatColors.Grey + "."
    };
  }

  public IView CannotRollYet() {
    return new SimpleView { PREFIX, "You can only roll when round ends or while dead." };
  }

  public IView RollingDisabled() {
    return new SimpleView { PREFIX, "Rolling is disabled." };
  }

  public IView AutoRTDToggled(bool enabled) {
    return new SimpleView {
      PREFIX,
      ChatColors.Grey + "You",
      enabled ? ChatColors.Green + "enabled" : ChatColors.Red + "disabled",
      ChatColors.Grey + "Auto-RTD."
    };
  }

  public IView AutoRTDNotSupported
    => new SimpleView { PREFIX, "Toggling Auto-RTD is not supported on this server." };
}

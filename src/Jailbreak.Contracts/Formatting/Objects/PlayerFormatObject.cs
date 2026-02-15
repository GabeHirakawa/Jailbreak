using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Contracts.Extensions;

namespace Jailbreak.Contracts.Formatting.Objects;

public class PlayerFormatObject(CCSPlayerController player) : FormatObject {
  private readonly string name = player.PlayerName;
  private readonly CsTeam team = player.Team;

  public override string ToChat() {
    return $"{TeamFormatObject.GetChatColor(team)}{name}";
  }

  public override string ToPanorama() { return name.Sanitize(); }

  public override string ToPlain() { return name; }
}

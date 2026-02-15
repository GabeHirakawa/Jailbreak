using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Contracts.Formatting;
using Jailbreak.Contracts.Formatting.Objects;

namespace Jailbreak.Fun.Locale;

public class SDLocale {
  public static readonly FormatObject PREFIX =
    new HiddenFormatObject($" {ChatColors.DarkBlue}Game>") {
      Plain = false, Panorama = false, Chat = true
    };
}

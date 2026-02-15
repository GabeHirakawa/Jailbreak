using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Contracts.Extensions;
using Jailbreak.Contracts.Formatting;
using Jailbreak.Contracts.Formatting.Objects;

namespace Jailbreak.Core.Locale;

/// <summary>
/// English locale implementation for all Core plugin messages.
/// Consolidates all 13 former stub locale implementations into one class.
/// </summary>
public class CoreLocale : ICoreLocale {
  private static readonly FormatObject WARDEN_PREFIX =
    new HiddenFormatObject($" {ChatColors.DarkBlue}Guard>") {
      Plain = false, Panorama = false, Chat = true
    };

  private static readonly FormatObject COMMAND_STANDS =
    new HiddenFormatObject("The previous command stands for 10 seconds.");

  private static readonly FormatObject VOICE_PREFIX =
    new HiddenFormatObject($" {ChatColors.DarkBlue}Voice>") {
      Plain = false, Panorama = false, Chat = true
    };

  private static readonly FormatObject GAME_PREFIX =
    new HiddenFormatObject($" {ChatColors.DarkBlue}Game>") {
      Plain = false, Panorama = false, Chat = true
    };

  private static readonly FormatObject SERVER_PREFIX =
    new HiddenFormatObject($" {ChatColors.DarkBlue}Server>") {
      Plain = false, Panorama = false, Chat = true
    };

  #region Warden

  public IView PickingShortly => new SimpleView {
    WARDEN_PREFIX,
    $"Picking a warden shortly, type {ChatColors.BlueGrey}!warden{ChatColors.Grey} to enter the queue."
  };

  public IView NoWardens => new SimpleView {
    WARDEN_PREFIX,
    $"No one in queue. Next guard to {ChatColors.BlueGrey}!warden{ChatColors.Grey} will become warden."
  };

  public IView NowFreeday => new SimpleView {
    WARDEN_PREFIX,
    $"It is now a freeday! CTs must pursue {ChatColors.BlueGrey}!warden{ChatColors.Grey}."
  };

  public IView WardenLeft => new SimpleView {
    WARDEN_PREFIX, "The warden left the game.", COMMAND_STANDS
  };

  public IView WardenDied => new SimpleView { {
      WARDEN_PREFIX,
      $"The warden {ChatColors.Red}died{ChatColors.Grey}. It is a freeday!"
    }, SimpleView.NEWLINE, {
      WARDEN_PREFIX,
      $"CTs must pursue {ChatColors.BlueGrey}!warden{ChatColors.Grey}."
    }
  };

  public IView BecomeNextWarden => new SimpleView {
    WARDEN_PREFIX,
    $"Type {ChatColors.BlueGrey}!warden{ChatColors.Grey} to become the warden."
  };

  public IView JoinRaffle => new SimpleView {
    WARDEN_PREFIX,
    $"You {ChatColors.White}joined {ChatColors.Grey}the warden raffle."
  };

  public IView LeaveRaffle => new SimpleView {
    WARDEN_PREFIX,
    $"You {ChatColors.Red}left {ChatColors.Grey}the warden raffle."
  };

  public IView NotWarden => new SimpleView {
    WARDEN_PREFIX, $"{ChatColors.LightRed}You are not the warden."
  };

  public IView FireCommandFailed => new SimpleView {
    WARDEN_PREFIX, "The fire command failed for some unknown reason..."
  };

  public IView CannotWardenDuringWarmup => new SimpleView {
    WARDEN_PREFIX, "You cannot warden during warmup."
  };

  public IView TogglingNotEnabled => new SimpleView {
    WARDEN_PREFIX,
    "Toggling Auto-Warden is not supported on this server."
  };

  public IView PassWarden(CCSPlayerController player)
    => new SimpleView {
      WARDEN_PREFIX, player, "resigned from warden.", COMMAND_STANDS
    };

  public IView NewWarden(CCSPlayerController player)
    => new SimpleView { WARDEN_PREFIX, player, "is now the warden." };

  public IView CurrentWarden(CCSPlayerController? player)
    => player is not null
      ? new SimpleView { WARDEN_PREFIX, "The warden is", player, "." }
      : new SimpleView { WARDEN_PREFIX, "There is no warden." };

  public IView FireCommandSuccess(CCSPlayerController player)
    => new SimpleView {
      WARDEN_PREFIX, player, "was fired and is no longer the warden."
    };

  public IView FireWarden(CCSPlayerController player)
    => new SimpleView {
      WARDEN_PREFIX, player, "was fired from warden.", COMMAND_STANDS
    };

  public IView FireWarden(CCSPlayerController player,
    CCSPlayerController admin)
    => new SimpleView {
      WARDEN_PREFIX, admin, "fired", player, "from warden.", COMMAND_STANDS
    };

  public IView MarkerPlaced()
    => new SimpleView { WARDEN_PREFIX, "Marker placed." };

  public IView MarkerRemoved(string marker)
    => new SimpleView {
      WARDEN_PREFIX, $"{marker}{ChatColors.Grey} marker removed."
    };

  public IView AutoWardenToggled(bool enabled)
    => new SimpleView {
      WARDEN_PREFIX, ChatColors.Grey + "You",
      enabled ? ChatColors.Green + "enabled" : ChatColors.Red + "disabled",
      ChatColors.Grey + "Auto-Warden."
    };

  #endregion

  #region Warden Commands - Count

  public IView NoMarkerSet
    => new SimpleView { WARDEN_PREFIX, "No marker set." };

  public IView PrisonersInMarker(int prisoners)
    => new SimpleView {
      WARDEN_PREFIX,
      ChatColors.Grey + "There " + (prisoners == 1 ? "is" : " are"),
      prisoners,
      ChatColors.Grey + "prisoner" + (prisoners == 1 ? "" : "s")
        + " in the marker."
    };

  public IView CannotCountYet(int seconds)
    => new SimpleView {
      WARDEN_PREFIX, "You must wait", seconds,
      "seconds before counting prisoners."
    };

  #endregion

  #region Warden Commands - Open

  public IView CellsOpened
    => new SimpleView {
      WARDEN_PREFIX, ChatColors.Grey + "Cells were auto-opened."
    };

  public IView OpeningFailed
    => new SimpleView { WARDEN_PREFIX, "Failed to open the cells." };

  public IView AlreadyOpened
    => new SimpleView { WARDEN_PREFIX, "Cells are already opened." };

  public IView CellsOpenedBy(CCSPlayerController? player)
    => player == null
      ? new SimpleView {
        WARDEN_PREFIX,
        $"{ChatColors.Blue}The warden {ChatColors.Default}opened the cells."
      }
      : new SimpleView {
        WARDEN_PREFIX, player, "opened the cells."
      };

  public IView CellsOpenedWithPrisoners(int prisoners)
    => new SimpleView {
      WARDEN_PREFIX, "Detected", prisoners,
      "prisoner" + (prisoners == 1 ? "" : "s") + " still in cells, opening..."
    };

  public IView CellsOpenedSnitchPrisoners(int prisoners)
    => new SimpleView {
      WARDEN_PREFIX, ChatColors.Grey + "Detected", prisoners,
      "prisoner" + (prisoners == 1 ? "" : "s") + " still in cells..."
    };

  public IView CannotOpenYet(int seconds)
    => new SimpleView {
      WARDEN_PREFIX, "You must wait", seconds,
      "seconds before opening the cells."
    };

  #endregion

  #region Warden Commands - Chicken

  public IView ChickenSpawned => new SimpleView {
    WARDEN_PREFIX,
    ChatColors.Blue + "The warden" + ChatColors.Grey + " spawned a chicken."
  };

  public IView ChickenSpawnFailed => new SimpleView {
    WARDEN_PREFIX, ChatColors.Red + "Failed to spawn a chicken."
  };

  public IView TooManyChickens
    => new SimpleView { WARDEN_PREFIX, "Too many chickens." };

  #endregion

  #region Warden Commands - Soccer

  public IView SoccerSpawned => new SimpleView {
    WARDEN_PREFIX,
    ChatColors.Blue + "The warden" + ChatColors.Grey
      + " spawned a soccer ball."
  };

  public IView SoccerSpawnFailed => new SimpleView {
    WARDEN_PREFIX, ChatColors.Red + "Failed to spawn a soccer ball."
  };

  public IView TooManySoccers
    => new SimpleView { WARDEN_PREFIX, "Too many soccer balls." };

  #endregion

  #region Warden Commands - Roll

  public IView Roll(int roll)
    => new SimpleView { WARDEN_PREFIX, "warden has rolled", roll, "!" };

  #endregion

  #region Special Treatment

  public IView STGranted => new SimpleView {
    WARDEN_PREFIX,
    $"You now have {ChatColors.Green}Special Treatment{ChatColors.White}!"
  };

  public IView STRevoked => new SimpleView {
    WARDEN_PREFIX,
    $"Your Special Treatment was {ChatColors.Red}removed{ChatColors.White}."
  };

  public IView STGrantedTo(CCSPlayerController player)
    => new SimpleView {
      WARDEN_PREFIX, player,
      $"now has {ChatColors.Green}Special Treatment{ChatColors.White}!"
    };

  public IView STRevokedFrom(CCSPlayerController player)
    => new SimpleView {
      WARDEN_PREFIX, player,
      $"{ChatColors.Red}no longer {ChatColors.Grey}has Special Treatment."
    };

  #endregion

  #region Mute / Peace

  public IView PeaceActive
    => new SimpleView { VOICE_PREFIX, "Peace is currently active." };

  public IView UnmutedGuards => new SimpleView {
    VOICE_PREFIX, CsTeam.CounterTerrorist, "were unmuted."
  };

  public IView UnmutedPrisoners => new SimpleView {
    VOICE_PREFIX, CsTeam.Terrorist, "were unmuted."
  };

  public IView MuteReminder => new SimpleView {
    VOICE_PREFIX, ChatColors.Red + "You are currently muted."
  };

  public IView PeaceReminder => new SimpleView {
    VOICE_PREFIX,
    $"Peace is currently active. {ChatColors.Red}You should only be talking if absolutely necessary!"
  };

  public IView DeadReminder => new SimpleView {
    VOICE_PREFIX, $"{ChatColors.Red}You are dead and cannot speak."
  };

  public IView AdminDeadReminder => new SimpleView {
    VOICE_PREFIX, "You are dead.",
    $"{ChatColors.Red}You should only be talking if absolutely necessary!"
  };

  public IView PeaceEnactedByAdmin(int seconds)
    => new SimpleView {
      VOICE_PREFIX, "An admin enacted peace for", seconds,
      "second" + (seconds == 1 ? "" : "s") + "."
    };

  public IView WardenEnactedPeace(int seconds)
    => new SimpleView {
      VOICE_PREFIX, "The warden enacted peace for", seconds, "seconds."
    };

  public IView GeneralPeaceEnacted(int seconds)
    => new SimpleView {
      VOICE_PREFIX, "Peace was enacted for", seconds,
      "second" + (seconds == 1 ? "" : "s") + "."
    };

  #endregion

  #region Rebel

  public IView NoLongerRebel
    => new SimpleView { GAME_PREFIX, "You are no longer red." };

  #endregion

  #region C4

  public IView JihadC4Pickup => new SimpleView {
    GAME_PREFIX,
    $"You picked up a {ChatColors.Red}Jihad C4{ChatColors.Grey}!"
  };

  public IView JihadC4Received => new SimpleView {
    GAME_PREFIX,
    $"You received a {ChatColors.Red}Jihad C4{ChatColors.Grey}!"
  };

  public IView JihadC4Usage1 => new SimpleView {
    GAME_PREFIX,
    $"To detonate it, hold it out and press {ChatColors.Yellow + "E" + ChatColors.Grey}."
  };

  #endregion

  #region Logs

  public IView BeginJailbreakLogs => new SimpleView {
    "********************************", SimpleView.NEWLINE,
    "***** BEGIN JAILBREAK LOGS *****", SimpleView.NEWLINE,
    "********************************"
  };

  public IView EndJailbreakLogs => new SimpleView {
    "********************************", SimpleView.NEWLINE,
    "****** END JAILBREAK LOGS ******", SimpleView.NEWLINE,
    "********************************"
  };

  public IView CreateLog(params FormatObject[] objects)
    => new SimpleView { objects };

  #endregion

  #region Warden Commands - Marker

  public IView MarkerChangingNotEnabled => new SimpleView {
    WARDEN_PREFIX,
    "Marker customization is not supported on this server."
  };

  public IView MarkerTypeChanged(string type)
    => new SimpleView { WARDEN_PREFIX, $"Changed marker type to {type}." };

  public IView MarkerColorChanged(string color)
    => new SimpleView { WARDEN_PREFIX, $"Changed marker color to {color}." };

  #endregion

  #region Generic Commands

  public IView PlayerNotFound(string query)
    => new SimpleView {
      SERVER_PREFIX,
      $"Player '{ChatColors.BlueGrey}{query}{ChatColors.Grey}' not found."
    };

  public IView PlayerFoundMultiple(string query)
    => new SimpleView {
      SERVER_PREFIX,
      $"Multiple players found for '{ChatColors.BlueGrey}{query}{ChatColors.Grey}'."
    };

  public IView CommandOnCooldown(DateTime cooldownEndsAt) {
    var seconds = (int)(cooldownEndsAt - DateTime.Now).TotalSeconds;
    return new SimpleView {
      SERVER_PREFIX, "Command is on cooldown for", seconds,
      "second" + (seconds == 1 ? "" : "s") + "."
    };
  }

  public IView InvalidParameter(string parameter, string expected)
    => new SimpleView {
      SERVER_PREFIX,
      $"Invalid parameter '{ChatColors.BlueGrey}{parameter}{ChatColors.Grey}',",
      "expected a" + (expected[0].IsVowel() ? "n" : ""),
      $"{ChatColors.BlueGrey}{expected}{ChatColors.Grey}."
    };

  public IView NoPermissionMessage(string permission)
    => new SimpleView {
      SERVER_PREFIX,
      $"This requires the {ChatColors.BlueGrey}{permission}{ChatColors.Grey} permission."
    };

  public IView Error(string message)
    => new SimpleView {
      SERVER_PREFIX,
      $"An error occurred: {ChatColors.Red}{message}{ChatColors.Grey}."
    };

  #endregion

  #region Last Guard

  public IView LGStarted(CCSPlayerController lastGuard, int ctHealth,
    int tHealth) {
    return new SimpleView {
      SimpleView.NEWLINE, {
        WARDEN_PREFIX,
        $"{ChatColors.Grey}All Ts are rebels! {ChatColors.DarkRed}LAST GUARD{ChatColors.Grey} must kill until two prisoners alive ({ChatColors.BlueGrey}LR{ChatColors.Grey})."
      },
      SimpleView.NEWLINE, {
        lastGuard, ChatColors.Grey + "has", ctHealth,
        $"{ChatColors.Grey}health, Ts have", tHealth,
        $"{ChatColors.Grey}health total."
      }
    };
  }

  #endregion
}

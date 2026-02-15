using CounterStrikeSharp.API.Core;
using Jailbreak.Contracts.Formatting;

namespace Jailbreak.Core.Services.Stubs;

// TODO: Re-enable when locale is migrated (Task 10)
// These stub locale interfaces mirror the original Jailbreak.Formatting locale interfaces
// but use Jailbreak.Contracts.Formatting.IView instead.

public interface IWardenLocale {
  IView PickingShortly { get; }
  IView NoWardens { get; }
  IView NowFreeday { get; }
  IView WardenLeft { get; }
  IView WardenDied { get; }
  IView BecomeNextWarden { get; }
  IView JoinRaffle { get; }
  IView LeaveRaffle { get; }
  IView NotWarden { get; }
  IView FireCommandFailed { get; }
  IView CannotWardenDuringWarmup { get; }
  IView TogglingNotEnabled { get; }
  IView PassWarden(CCSPlayerController player);
  IView NewWarden(CCSPlayerController player);
  IView CurrentWarden(CCSPlayerController? player);
  IView FireCommandSuccess(CCSPlayerController player);
  IView FireWarden(CCSPlayerController player);
  IView FireWarden(CCSPlayerController player, CCSPlayerController admin);
  IView MarkerPlaced();
  IView MarkerRemoved(string marker);
  IView AutoWardenToggled(bool enabled);
}

public interface IWardenCmdCountLocale {
  IView NoMarkerSet { get; }
  IView PrisonersInMarker(int prisoners);
  IView CannotCountYet(int seconds);
}

public interface IWardenCmdOpenLocale {
  IView CellsOpened { get; }
  IView OpeningFailed { get; }
  IView AlreadyOpened { get; }
  IView CellsOpenedBy(CCSPlayerController? player);
  IView CellsOpenedWithPrisoners(int prisoners);
  IView CellsOpenedSnitchPrisoners(int prisoners);
  IView CannotOpenYet(int seconds);
}

public interface IWardenCmdChickenLocale {
  IView ChickenSpawned { get; }
  IView SpawnFailed { get; }
  IView TooManyChickens { get; }
}

public interface IWardenCmdSoccerLocale {
  IView SoccerSpawned { get; }
  IView SpawnFailed { get; }
  IView TooManySoccers { get; }
}

public interface IWardenCmdRollLocale {
  IView Roll(int roll);
}

public interface IWardenSTLocale {
  IView Granted { get; }
  IView Revoked { get; }
  IView GrantedTo(CCSPlayerController player);
  IView RevokedFrom(CCSPlayerController player);
}

public interface IWardenPeaceLocale {
  IView PeaceActive { get; }
  IView UnmutedGuards { get; }
  IView UnmutedPrisoners { get; }
  IView MuteReminder { get; }
  IView PeaceReminder { get; }
  IView DeadReminder { get; }
  IView AdminDeadReminder { get; }
  IView PeaceEnactedByAdmin(int seconds);
  IView WardenEnactedPeace(int seconds);
  IView GeneralPeaceEnacted(int seconds);
}

public interface IRebelLocale {
  IView NoLongerRebel { get; }
}

public interface IC4Locale {
  IView JihadC4Pickup { get; }
  IView JihadC4Received { get; }
  IView JihadC4Usage1 { get; }
}

public interface ILogLocale {
  IView BeginJailbreakLogs { get; }
  IView EndJailbreakLogs { get; }
  IView CreateLog(params FormatObject[] objects);
}

public interface IWardenCmdMarkerLocale {
  IView ChangingNotEnabled { get; }
  IView TypeChanged(string type);
  IView ColorChanged(string color);
}

public interface IGenericCmdLocale {
  IView PlayerNotFound(string query);
  IView PlayerFoundMultiple(string query);
  IView CommandOnCooldown(DateTime cooldownEndsAt);
  IView InvalidParameter(string parameter, string expected);
  IView NoPermissionMessage(string permission);
  IView Error(string message);
}

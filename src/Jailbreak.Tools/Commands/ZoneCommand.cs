using CounterStrikeSharp.API.Core;

namespace Jailbreak.Tools.Commands;

// TODO: Wire to Zones plugin when cross-plugin capability is available
// Original supported: add, set, remove, tpto, show/draw, list, addinner, finish, cleanup, reload, generate
// Depends on: IZoneManager, IZoneFactory, IBeamShapeFactory, PlayerZoneCreator
public class ZoneCommand : AbstractCommand {
  public override void OnCommand(CCSPlayerController? executor,
    WrappedInfo info) {
    info.ReplyToCommand("Zone commands: not yet wired to Zones plugin");
    info.ReplyToCommand(
      "Supported subcommands: add, set, remove, tpto, show, list, addinner, finish, cleanup, reload, generate");
  }
}

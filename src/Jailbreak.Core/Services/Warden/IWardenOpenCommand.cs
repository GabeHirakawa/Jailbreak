namespace Jailbreak.Core.Services.Warden;

/// <summary>
/// Tracks whether cells have been opened this round.
/// Co-located from Jailbreak.Public.Mod.Warden.IWardenOpenCommand.
/// </summary>
public interface IWardenOpenCommand {
  bool OpenedCells { get; set; }
}

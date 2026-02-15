using CounterStrikeSharp.API.Core;
using Jailbreak.Contracts.Formatting;

namespace Jailbreak.Core.Services.Logs;

/// <summary>
/// Extended log service that supports rich format objects.
/// Migrated from Jailbreak.Formatting.Views.Logging.IRichLogService.
/// </summary>
public interface IRichLogService : ILogService {
  void Append(params FormatObject[] objects);

  FormatObject Player(CCSPlayerController playerController);
}

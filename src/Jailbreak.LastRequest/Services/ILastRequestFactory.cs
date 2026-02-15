using CounterStrikeSharp.API.Core;
using Jailbreak.LastRequest.Enums;

namespace Jailbreak.LastRequest.Services;

public interface ILastRequestFactory {
  AbstractLastRequest CreateLastRequest(CCSPlayerController prisoner,
    CCSPlayerController guard, LRType type);

  bool IsValidType(LRType type);
}

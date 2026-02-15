using CounterStrikeSharp.API.Core;

namespace Jailbreak.LastRequest.Services;

public interface ILastRequestRebelManager {
  HashSet<int> PlayersLRRebelling { get; }
  void StartLRRebelling(CCSPlayerController player);

  bool IsInLRRebelling(int playerSlot) {
    return PlayersLRRebelling.Contains(playerSlot);
  }

  void AddLRRebelling(int playerSlot) {
    PlayersLRRebelling.Add(playerSlot);
  }

  void ClearLRRebelling() { PlayersLRRebelling.Clear(); }
}

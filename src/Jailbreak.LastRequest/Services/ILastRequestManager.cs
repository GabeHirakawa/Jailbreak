using CounterStrikeSharp.API.Core;
using Jailbreak.LastRequest.Enums;

namespace Jailbreak.LastRequest.Services;

public interface ILastRequestManager {
  bool IsLREnabled { get; set; }
  IList<AbstractLastRequest> ActiveLRs { get; }

  bool InitiateLastRequest(CCSPlayerController prisoner,
    CCSPlayerController guard, LRType lrType);

  bool EndLastRequest(AbstractLastRequest lr, LRResult result);

  bool IsInLR(CCSPlayerController player) {
    return GetActiveLR(player) != null;
  }

  AbstractLastRequest? GetActiveLR(CCSPlayerController player) {
    return ActiveLRs.FirstOrDefault(lr
      => lr.Guard.Slot == player.Slot || lr.Prisoner.Slot == player.Slot);
  }

  void EnableLR(CCSPlayerController? died = null);
  void DisableLR();
  void DisableLRForRound();
}

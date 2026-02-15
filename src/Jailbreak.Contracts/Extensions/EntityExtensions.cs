using System.Drawing;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace Jailbreak.Contracts.Extensions;

public static class EntityExtensions {
  /// <summary>
  ///   Sets the render color of a model entity.
  /// </summary>
  public static void SetColor(this CBaseModelEntity? entity, Color color) {
    if (entity == null || !entity.IsValid) return;

    entity.RenderMode = RenderMode_t.kRenderTransColor;
    entity.Render     = color;
    Utilities.SetStateChanged(entity, "CBaseModelEntity", "m_clrRender");
  }

  /// <summary>
  ///   Attempts to get the CCSPlayerController from an entity instance
  ///   (typically a pawn).
  /// </summary>
  public static bool TryGetController(this CEntityInstance pawn,
    out CCSPlayerController? controller) {
    controller = null;

    if (!pawn.IsValid) return false;

    var index      = (int)pawn.Index;
    var playerPawn = Utilities.GetEntityFromIndex<CCSPlayerPawn>(index);

    if (playerPawn == null || !playerPawn.IsValid) return false;

    if (!playerPawn.OriginalController.IsValid) return false;

    controller = playerPawn.OriginalController.Value;

    return controller?.IsReal() == true;
  }
}

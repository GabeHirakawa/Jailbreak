using System.Drawing;

namespace Jailbreak.Zones.Services.Draw;

public interface IColorable {
  void SetColor(Color color);
  Color GetColor();
}

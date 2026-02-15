using CounterStrikeSharp.API.Core;
using Jailbreak.Zones.Services.Draw;
using Jailbreak.Zones.Services.Draw.Shapes;
using Jailbreak.Zones.Services.Zones;

namespace Jailbreak.Zones;

public class ZonesPlugin : BasePlugin {
  public override string ModuleName => "Jailbreak Zones";
  public override string ModuleVersion => "2.0.0";
  public override string ModuleAuthor => "EdgeGamers Development";

  private SqlZoneManager zoneManager = null!;
  private BasicZoneFactory zoneFactory = null!;
  private BeamShapeRegistry shapeRegistry = null!;
  private BeamShapeFactory shapeFactory = null!;
  private TextSpawner textSpawner = null!;
  private RandomZoneGenerator randomZoneGen = null!;

  public override void Load(bool hotReload) {
    // Draw system
    shapeRegistry = new BeamShapeRegistry();
    shapeFactory = new BeamShapeFactory(shapeRegistry);
    shapeFactory.Start(this);
    textSpawner = new TextSpawner();

    // Zone system
    zoneFactory = new BasicZoneFactory();
    zoneManager = new SqlZoneManager(zoneFactory);
    zoneManager.Start(this);
    randomZoneGen = new RandomZoneGenerator(zoneManager, zoneFactory);
    randomZoneGen.Start(this);

    RegisterFakeConVars(typeof(SqlZoneManager));
  }

  public override void Unload(bool hotReload) {
    randomZoneGen.Stop();
  }
}

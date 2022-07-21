using Verse;

namespace esm.ModConfigurationMenus;

public class McmMountainTempModSettings : ModSettings
{
    public enum TemperatureMode
    {
        Fixed,
        Seasonal,
        Annual
    }

    public float FixedTarget = 10.0f;
    public TemperatureMode TargetMode = TemperatureMode.Annual;
    public int UpdateTicks = 60;

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref TargetMode, "TargetMode", TemperatureMode.Annual, true);
        Scribe_Values.Look(ref FixedTarget, "FixedTarget", 10.0f, true);
        Scribe_Values.Look(ref UpdateTicks, "UpdateTicks", 60);
    }
}